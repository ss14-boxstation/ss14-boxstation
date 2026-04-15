using Content.Shared.Tools.Components;
using Content.Shared.Trigger;
using Content.Shared._Box.Trigger.Components.Triggers;

namespace Content.Shared._Box.Trigger.Systems;

/// This is a clone of TriggerOnToolUseSystem that actually checks if the doAfter was completed succesfully.
/// This is presumably how it's supposed to function, but it is not.
/// Implemented as its own namespaced component instead of changing the original code to avoid potential upstream conflicts.
public sealed class TriggerOnToolUseSuccessSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnSimpleToolUsageSuccessComponent, SimpleToolDoAfterEvent>(OnToolUse);
    }

    private void OnToolUse(Entity<TriggerOnSimpleToolUsageSuccessComponent> ent, ref SimpleToolDoAfterEvent args)
    {
        if (args.DoAfter.Completed)
        {
            Trigger.Trigger(ent.Owner, args.User, ent.Comp.KeyOut);
        }
    }
}
