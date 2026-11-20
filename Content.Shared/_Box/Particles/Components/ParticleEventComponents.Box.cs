using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Starfall.Particles;

[RegisterComponent]
public sealed partial class ParticleOnLightBreakComponent : Component // Not inheriting from ParticleOnEventBase! Uses multiple effects instead of one for timing variance
{
    /// <summary>
    /// The list of particle effects to spawn. One will be chosen from at random.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<ParticleEffectPrototype>> Effects = [];

    /// <summary>
    /// Optional color tint applied to the spawned particles.
    /// </summary>
    [DataField]
    public Color? ColorOverride;
}
