using Robust.Shared.Serialization;

namespace Content.Shared._Box.Light;

[Serializable, NetSerializable]
public sealed class LightBreakEvent : EntityEventArgs
{
    public NetEntity Entity;
    public LightBreakEvent(NetEntity ent)
    {
        Entity = ent;
    }
}

[ByRefEvent]
public record struct PredictedLightBreakEvent;
