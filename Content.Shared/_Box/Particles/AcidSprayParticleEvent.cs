using Content.Shared._Starfall.Particles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Box.Particles;

[Serializable, NetSerializable]
public sealed class AcidSprayParticleEvent : EntityEventArgs
{
    public NetEntity Entity;
    public ProtoId<ParticleEffectPrototype> Effect;
    public Color Color;
}
