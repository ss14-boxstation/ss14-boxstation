/// TODO: Figure out a way to make the ammo slowly recharge over the delay
/// TODO: Figure out a way to decouple the gun's usedelay and recharge delay, while keeping the visual delay wheel
/// TODO: Different noises for when the recharge starts and when the recharge ends
/// TODO: is it possible to make this work the moment the gun is empty, instead of when an empty gun tries to be shot?
using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared.Weapons.Ranged.Systems;

/// <summary>
/// Recharges ammo to full over time whenever the gun is fired while empty
/// </summary>
public sealed class RechargeEmptyAmmoSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RechargeEmptyAmmoComponent, OnEmptyGunShotEvent>(OnEmptyGunshot);
        SubscribeLocalEvent<RechargeEmptyAmmoComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnEmptyGunshot(Entity<RechargeEmptyAmmoComponent> ent, ref OnEmptyGunShotEvent args)
    {
        // Check if the gun even has ammo
        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var basic))
            return;

        if (basic.Count != 0 || basic.Count == null || basic.Capacity == null)
            return;

        // Force the UseDelay
        if (TryComp<UseDelayComponent>(ent, out var delay) && ent.Comp.ForceUseDelay)
            _delay.TryResetDelay((ent, delay));

        // We don't predict this because occasionally on client it may not play.
        // PlayPredicted will still be predicted on the client.
        if (_netManager.IsServer)
            _audio.PlayPvs(ent.Comp.RechargeSound, ent);

        // Fully Reload the gun
        _gun.UpdateBasicEntityAmmoCount((ent, basic), basic.Count.Value + basic.Capacity.Value);
        Dirty(ent, basic);
    }

    // This feels scuffed as fuck, but I can't think of a better way right now
    private void OnShotAttempted (Entity<RechargeEmptyAmmoComponent> ent, ref ShotAttemptedEvent args)
    {
        // If the gun has UseDelay, don't let it shoot (Why don't all guns do this?)
        if (_delay.IsDelayed(ent.Owner) && ent.Comp.ForceUseDelay)
            args.Cancel();
    }
}
