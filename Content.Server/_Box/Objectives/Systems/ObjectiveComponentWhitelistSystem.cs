using Content.Server._Box.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Whitelist;

namespace Content.Server._Box.Objectives.Systems;

/// <summary>
/// Checks if the player's character passes a component/tag/etc whitelist, and cancels the objective if they don't.
/// </summary>
/// <remarks>
/// Used to check for consent for DAGD.
/// </remarks>
public sealed partial class ObjectiveComponentWhitelistSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObjectiveComponentWhitelistComponent, RequirementCheckEvent>(OnCheck);
    }

    private void OnCheck(EntityUid uid, ObjectiveComponentWhitelistComponent comp, ref RequirementCheckEvent args)
    {
        if (args.Cancelled)
            return;

        if ((args.Mind.OwnedEntity is { } entity && _whitelistSystem.IsWhitelistFail(comp.Whitelist, entity)))
        {
            args.Cancelled = true;
            return;
        }
    }
}
