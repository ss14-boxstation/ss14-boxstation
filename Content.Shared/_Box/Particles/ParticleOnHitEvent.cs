using Content.Shared._Starfall.Particles;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Shared._Box.Particles;

[Serializable, NetSerializable]
public sealed class ParticleOnHitEvent : EntityEventArgs
{
    public MapCoordinates Coordinates;
    public ProtoId<ParticleEffectPrototype> Effect;
    public NetEntity Entity;
    public Color? ColorOverride;
    public Vector2? SpawnOffset;
    public ParticleOnHitEvent(ProtoId<ParticleEffectPrototype> effect, MapCoordinates coords, NetEntity ent, Color? colorOverride, Vector2? spawnOffset)
    {
        Effect = effect;
        Coordinates = coords;
        Entity = ent;
        ColorOverride = colorOverride;
        SpawnOffset = spawnOffset;
    }
}
