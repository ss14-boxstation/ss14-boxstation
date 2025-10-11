using Robust.Shared.Serialization;
using Content.Shared.Actions; // Box Change - #IMP - Duo XenoArch

namespace Content.Shared.Xenoarchaeology.XenoArtifacts;

[Serializable, NetSerializable]
public enum SharedArtifactsVisuals : byte
{
    SpriteIndex,
    IsActivated,
    IsUnlocking
}
