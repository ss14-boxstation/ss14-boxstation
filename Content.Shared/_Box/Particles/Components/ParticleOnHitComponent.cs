using Content.Shared._Starfall.Particles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Box.Particles;

/// <summary>
/// Used to spawn particles when an entity is hit, with optional sound, at a random chance.
/// </summary>
[RegisterComponent]
public sealed partial class ParticleOnHitComponent : Component
{
    /// <summary>
    /// Particle effect prototype to use.
    /// </summary>
    [DataField]
    [ViewVariables]
    public ProtoId<ParticleEffectPrototype> Effect = "SfWeldingSparksBurst";
    /// <summary>
    /// Optional color tint.
    /// </summary>
    [DataField]
    public Color? ColorOverride;
    /// <summary>
    ///  Scales emission rate and particle size.
    /// </summary>
    [DataField]
    public float Intensity = 1f;
    /// <summary>
    /// Local-space offset applied to the spawn origin..
    /// Useful for shifting the effect away from the entity's anchor point.
    /// </summary>
    [DataField]
    public Vector2 SpawnOffset;
    /// <summary>
    /// Minimum single-hit damage required to spawn particles.
    /// </summary>
    [DataField]
    public float MinDamage = 8;
    /// <summary>
    /// A list of damage types that are able to spawn particles.
    /// </summary>
    [DataField]
    public HashSet<string> DamageTypes = [];
    /// <summary>
    /// The chance of particles spawning.
    /// </summary>
    [DataField]
    public float Chance = 100;
    /// <summary>
    /// If true, overrides the particle color with the entity's blood color.
    /// </summary>
    [DataField]
    public bool UseBloodColor = false;
    /// <summary>
    /// The sound to play when particles are spawned.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;
}
