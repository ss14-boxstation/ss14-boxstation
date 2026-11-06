using Content.Shared._Starfall.Particles;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Box.Particles;

/// <summary>
/// A component used to add compound fire and smoke particles to an entity, without needing to actually light it on fire.
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
    [DataField]
    public HashSet<string> DamageTypes = [];
    [DataField]
    public float Chance = 100;
    [DataField]
    public bool UseBloodColor = false;
}
