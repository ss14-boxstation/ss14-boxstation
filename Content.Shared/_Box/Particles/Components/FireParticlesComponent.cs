using Content.Shared._Starfall.Particles;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Box.Particles;

/// <summary>
/// A component used to add compound fire and smoke particles to an entity, without needing to actually light it on fire.
/// </summary>
[RegisterComponent]
public sealed partial class FireParticlesComponent : Component
{
    /// <summary>
    /// The particle to use for fire.
    /// </summary>
    [DataField]
    public ProtoId<ParticleEffectPrototype> FireEffect = "SfFireContinuous";

    /// <summary>
    /// The particle to use for smoke.
    /// </summary>
    [DataField]
    public ProtoId<ParticleEffectPrototype> SmokeEffect = "SfFireSmoke";
    /// <summary>
    /// Optional color tint applied to the fire.
    /// </summary>
    [DataField]
    public Color? FireColorOverride;
    /// <summary>
    /// Optional color tint applied to the smoke.
    /// </summary>
    [DataField]
    public Color? SmokeColorOverride;
    /// <summary>
    /// The intensity of the fire. Scales emission rate and particle size.
    /// </summary>
    [DataField]
    public float FireIntensity = 1f;
    /// <summary>
    /// The intensity of the smoke. Scales emission rate and particle size.
    /// </summary>
    [DataField]
    public float SmokeIntensity = 1f;
    /// <summary>
    /// Local-space offset applied to the spawn origin of the fire.
    /// Useful for shifting the effect away from the entity's anchor point.
    /// </summary>
    [DataField]
    public Vector2 FireSpawnOffset;
    /// <summary>
    /// Local-space offset applied to the spawn origin of the smoke.
    /// Useful for shifting the effect away from the entity's anchor point.
    /// </summary>
    [DataField]
    public Vector2 SmokeSpawnOffset;
}
