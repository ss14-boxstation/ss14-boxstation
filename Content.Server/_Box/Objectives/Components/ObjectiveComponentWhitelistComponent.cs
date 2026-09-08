using Content.Server._Box.Objectives.Systems;
using Content.Shared.Whitelist;

namespace Content.Server._Box.Objectives.Components;

/// <summary>
/// Specifies a list of components that the attached entity must have in order to roll an objective.
/// </summary>
[RegisterComponent, Access(typeof(ObjectiveComponentWhitelistSystem))]
public sealed partial class ObjectiveComponentWhitelistComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public EntityWhitelist Whitelist = new();
}
