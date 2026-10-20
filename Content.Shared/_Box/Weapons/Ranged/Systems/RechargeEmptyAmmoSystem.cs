// TODO: Figure out a way to use the visual delay wheel
// TODO: is it possible to make this work the moment the gun is empty, instead of when an empty gun tries to be shot?
// TODO: There's probably a way to get the "finish recharging" logic and sound to go here instead, and to make them not delayed from when they happen.

using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class RechargeEmptyAmmoSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RechargeEmptyAmmoComponent, OnEmptyGunShotEvent>(OnEmptyGunshot);
        SubscribeLocalEvent<RechargeEmptyAmmoComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<RechargeEmptyAmmoComponent, BasicAmmoFullEvent>(OnRechargeFinish);
    }

    private void OnEmptyGunshot(Entity<RechargeEmptyAmmoComponent> ent, ref OnEmptyGunShotEvent args)
    {
        // Check if the gun even has ammo and can reload
        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var basic) || !HasComp<RechargeBasicEntityAmmoComponent>(ent))
            return;

        if (basic.Count != 0 || basic.Count == null || basic.Capacity == null)
            return;

        // Check if the gun is already reloadoing
        if (ent.Comp.IsRecharging)
            return;

        // Start Reloading
        ent.Comp.IsRecharging = true;

        // Visually show a delay, this implementation requires that the YAML is tuned perfectly however.
        if (TryComp<UseDelayComponent>(ent, out var delay))
            _delay.TryResetDelay((ent, delay));

        Dirty(ent, ent.Comp);
        if (!_netManager.IsServer)
            _audio.PlayPredicted(ent.Comp.RechargeStartSound, ent, ent.Owner);
    }

    private void OnRechargeFinish(Entity<RechargeEmptyAmmoComponent> ent, ref BasicAmmoFullEvent args)
    {
        ent.Comp.IsRecharging = false;
        if (!_netManager.IsServer)
            _audio.PlayPredicted(ent.Comp.RechargeEndSound, ent, ent.Owner);
        // Cancel the delay if its still going
        if (TryComp<UseDelayComponent>(ent, out var delay))
            _delay.CancelDelay((ent, delay));
        Dirty(ent, ent.Comp);
    }

    private void OnShotAttempted(Entity<RechargeEmptyAmmoComponent> ent, ref ShotAttemptedEvent args)
    {
        // If the gun is recharging, don't let it shoot
        if (ent.Comp.IsRecharging)
            args.Cancel();
    }
}
