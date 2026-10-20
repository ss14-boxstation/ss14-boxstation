// TODO: Figure out a way to use the visual delay wheel
// TODO: is it possible to make this work the moment the gun is empty, instead of when an empty gun tries to be shot?
// TODO: There's probably a way to get the "finish recharging" logic and sound to go here instead, and to make them not delayed from when they happen.

using System;
using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

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
        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var ammo) || !TryComp<RechargeBasicEntityAmmoComponent>(ent, out var recharge))
            return;

        if (ammo.Count != 0 || ammo.Count == null || ammo.Capacity == null)
            return;

        // Check if the gun is already reloadoing
        if (ent.Comp.IsRecharging)
            return;

        // Start Reloading
        ent.Comp.IsRecharging = true;

        // Calculate how long the delay should be
        double delaySeconds = (double)(ammo.Capacity * recharge.RechargeCooldown);
        var delay = TimeSpan.FromSeconds(delaySeconds);

        // Force a delay based on how long it would take to recharge the gun
        // This only visually shows the player how long recharging should take, but does not mechanically force it.
        _delay.SetLength(ent.Owner, delay);
        _delay.TryResetDelay(ent);

        Dirty(ent, ent.Comp);
        if (_netManager.IsServer)
            _audio.PlayPredicted(ent.Comp.RechargeStartSound, ent, ent.Owner);
    }

    private void OnRechargeFinish(Entity<RechargeEmptyAmmoComponent> ent, ref BasicAmmoFullEvent args)
    {
        ent.Comp.IsRecharging = false;
        if (!_netManager.IsServer)
            _audio.PlayPredicted(ent.Comp.RechargeEndSound, ent, ent.Owner);
        Dirty(ent, ent.Comp);
    }

    private void OnShotAttempted(Entity<RechargeEmptyAmmoComponent> ent, ref ShotAttemptedEvent args)
    {
        // If the gun is recharging, don't let it shoot
        if (ent.Comp.IsRecharging)
            args.Cancel();
    }
}
