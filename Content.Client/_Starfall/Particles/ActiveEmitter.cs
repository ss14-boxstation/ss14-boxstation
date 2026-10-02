using System.Numerics;
using Content.Shared._Starfall.Particles;
using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// A running particle emitter and its live particle pool.
/// Created in <see cref="ParticleSystem"/>.
/// </summary>
public sealed class ActiveEmitter
{
    public ParticleEffectPrototype Proto = null!;

    /// <summary>
    /// How many sub-emitter links deep this emitter is. Root emitters are 0.
    /// Sub-emitters will not spawn if depth would exceed <see cref="ParticleSystem.MaxSubEmitterDepth"/>.
    /// </summary>
    public int SubEmitterDepth;

    /// <summary>Current map-space origin of the emitter, refreshed from <see cref="Coordinates"/> each frame.</summary>
    public MapCoordinates MapCoords;

    /// <summary>Grid-relative origin, so the effect follows moving and rotating grids.</summary>
    public EntityCoordinates Coordinates;

    /// <summary>
    /// Spawn offset copied from the prototype. Directional emitters interpret this in the
    /// attached entity's local space; other emitters interpret it in map space.
    /// </summary>
    public Vector2 SpawnOffset;

    /// <summary>Entity this emitter follows (if any).</summary>
    public EntityUid? AttachedEntity;

    /// <summary>Time elapsed since this emitter was created.</summary>
    public TimeSpan Age;

    /// <summary>Emission accumulator for sub-tick emission rates.</summary>
    public float EmitAccum;

    /// <summary>Time accumulated between quality-scaled simulation steps.</summary>
    public float SimulationAccumulator;

    /// <summary>True once the emitter stops producing new particles. Existing particles live out their lifetimes.</summary>
    public bool Exhausted;

    /// <summary>
    /// Unique client-side handle for addressing this emitter by ID.
    /// Prefer holding the <see cref="ActiveEmitter"/> reference directly when possible, please.
    /// </summary>
    public uint Handle;

    /// <summary>Color tint multiplied on top of every particle's computed color.</summary>
    public Color? ColorOverride;

    /// <summary>Intensity multiplier for emission rate and particle size. 1.0 = normal.</summary>
    public float Intensity = 1f;

    /// <summary>
    /// Live overrides shadowing individual prototype fields.
    /// Non-null values take priority, null falls back to the prototype.
    /// </summary>
    public ParticleRuntimeOverrides? Overrides;

    /// <summary>Pre-sampled prototype curves shared by emitters of the same effect.</summary>
    internal ParticleCurveCache Curves = null!;

    /// <summary>Explicit prototype max or the automatically calculated fallback.</summary>
    internal int ResolvedPrototypeMaxCount;

    // =^..^= Velocity tracking =^..^=

    public Vector2 PreviousPosition;
    public Vector2 EmitterVelocity;
    public bool VelocityInitialized;

    // =^..^= Aim-at targeting =^..^=

    /// <summary>
    /// When set, each tick the emit angle is recomputed to point toward this entity.
    /// Falls back to <see cref="TargetPosition"/> if the entity is deleted.
    /// </summary>
    public EntityUid? TargetEntity;

    /// <summary>
    /// When set, the emit angle points toward this world position.
    /// Used as a fallback when <see cref="TargetEntity"/> is unset or gone.
    /// </summary>
    public Vector2? TargetPosition;

    /// <summary>Resolved emit angle in radians, recomputed each tick from the target if one is set.</summary>
    public float EffectiveEmitAngle;

    // =^..^= Timed bursts =^..^=

    /// <summary>Tracks which <see cref="ParticleEffectPrototype.Bursts"/> entries have already fired.</summary>
    public readonly List<bool> FiredBursts = new();

    // =^..^= Animation =^..^=

    /// <summary>Resolved RSI frames. Populated on creation.
    /// Single-frame sprites have one entry and empty Delays.</summary>
    public Texture[] Frames = [];

    /// <summary>frame delays when an RSI defines animation.</summary>
    public float[] Delays = [];

    public int AnimFrame;
    public float AnimTimer;

    // =^..^= Particles =^..^=

    // LiveParticles stays dense: dead entries are removed immediately and pushed into FreePool.
    // The next emission pops from FreePool and resets the object rather than allocating a new one.
    // This avoids GC pressure from short lived allocations without making simulation/rendering
    // walk a graveyard of dead slots every frame. MaxCount should still be kept reasonable.

    /// <summary>Particles currently being simulated and rendered.</summary>
    public readonly List<ParticleData> LiveParticles = new();

    /// <summary>Dead particles available for reuse.</summary>
    public readonly Queue<ParticleData> FreePool = new();

    public int LiveCount => LiveParticles.Count;
}
