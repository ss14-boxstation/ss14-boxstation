using Content.Shared.Lock;
using Content.Shared.Trigger;
using Content.Shared._Box.Trigger.Components.Triggers;

namespace Content.Shared._Box.Trigger.Systems;

public sealed class TriggerOnLockToggleSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriggerOnLockToggleComponent, LockToggledEvent>(OnLockToggle);
    }

    private void OnLockToggle(Entity<TriggerOnLockToggleComponent> ent, ref LockToggledEvent args)
    {
        Trigger.Trigger(ent.Owner, null, ent.Comp.KeyOut);
    }
}
