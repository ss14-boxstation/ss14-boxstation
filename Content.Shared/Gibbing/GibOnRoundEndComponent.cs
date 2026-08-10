using Robust.Shared.Prototypes;

namespace Content.Shared.Gibbing.Components;

/// <summary>
/// Gibs an entity on round end.
/// </summary>
[RegisterComponent]
public sealed partial class GibOnRoundEndComponent : Component
{
    /// <summary>
    /// If the entity has all these objectives fulfilled they won't be gibbed.
    /// </summary>
    [DataField]
    public HashSet<EntProtoId> PreventGibbingObjectives = new();

    /// <summary>
    /// Entity to spawn when gibbed. Can be used for effects.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnProto;

    // Start Box Change
    /// <summary>
    /// Chance that the entity will not be gibbed, even if their objectives haven't been completed.
    /// </summary>
    [DataField]
    public float SafetyChance = 0f;
    // End Box Change}
}
