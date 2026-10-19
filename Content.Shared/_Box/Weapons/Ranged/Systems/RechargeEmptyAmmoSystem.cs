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
    }

    private void OnEmptyGunshot(Entity<RechargeEmptyAmmoComponent> ent, ref OnEmptyGunShotEvent args)
    {
        var recharge = ent.Comp;

        // Check if the gun even has ammo
        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var basic))
            return;

        if (basic.Count != 0 || basic.Count == null || basic.Capacity == null)
            return;

        // Force the UseDelay
        if (TryComp<UseDelayComponent>(ent, out var delay) && recharge.ForceUseDelay)
            _delay.TryResetDelay((ent, delay));

        // We don't predict this because occasionally on client it may not play.
        // PlayPredicted will still be predicted on the client.
        if (_netManager.IsServer)
            _audio.PlayPvs(recharge.RechargeSound, ent);

        // Fully Reload the gun
        _gun.UpdateBasicEntityAmmoCount((ent, basic), basic.Count.Value + basic.Capacity.Value);
        Dirty(ent, basic);

    }
}
