using Content.Shared.Power.Components; // Box Change - BatteryComponent was moved to Shared
using Content.Shared._EE.Silicon.Systems;
using Content.Shared.Bed.Sleep;
using Content.Server._EE.Silicon.Charge;
using Content.Server._EE.Power.Components;
using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.StatusEffectNew; // starcup
using Content.Shared.Stunnable; // Box Change - IPC No Battery Refactor

namespace Content.Server._EE.Silicon.Death;

public sealed class SiliconDeathSystem : EntitySystem
{
    [Dependency] private readonly SleepingSystem _sleep = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!; // starcup
    [Dependency] private readonly SharedStunSystem _stun = default!; // Box Change - IPC No Battery Refactor

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconDownOnDeadComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);

        SubscribeLocalEvent<SiliconDownOnDeadComponent, StandUpAttemptEvent>(OnStandUpAttempt); // Box Change - IPC No Battery Refactor
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, SiliconChargeStateUpdateEvent args)
    {
        if (!_silicon.TryGetSiliconBattery(uid, out var batteryComp))
        {
            SiliconDead(uid, siliconDeadComp, batteryComp, uid);
            return;
        }

        if (args.ChargePercent == 0 && siliconDeadComp.Dead)
            return;

        if (args.ChargePercent == 0 && !siliconDeadComp.Dead)
            SiliconDead(uid, siliconDeadComp, batteryComp, uid);
        else if (args.ChargePercent != 0 && siliconDeadComp.Dead)
                SiliconUnDead(uid, siliconDeadComp, batteryComp, uid);
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        var deadEvent = new SiliconChargeDyingEvent(uid, batteryComp, batteryUid);
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        // Box Change Start - IPC No Battery Refactor
        // EntityManager.EnsureComponent<SleepingComponent>(uid);
        // _statusEffect.TrySetStatusEffectDuration(uid, SleepingSystem.StatusEffectForcedSleeping); // starcup: edited for status effects refactor
        // Box Change End

        if (TryComp(uid, out HumanoidAppearanceComponent? humanoidAppearanceComponent))
        {
            var layers = HumanoidVisualLayersExtension.Sublayers(HumanoidVisualLayers.HeadSide);
            _humanoidAppearanceSystem.SetLayersVisibility((uid, humanoidAppearanceComponent), layers, visible: false);
        }

        siliconDeadComp.Dead = true;

        EnsureComp<KnockedDownComponent>(uid); // Box Change - IPC No Battery Refactor

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, batteryComp, batteryUid));
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        // Box Change Start - IPC No Battery Refactor
        // _statusEffect.TryRemoveStatusEffect(uid, SleepingSystem.StatusEffectForcedSleeping); // starcup: edited for status effects refactor
        // _sleep.TryWaking(uid, true, null);
        // Box Change End

        siliconDeadComp.Dead = false;

        _stun.TryStanding(uid); // Box Change - IPC No Battery Refactor

        RaiseLocalEvent(uid, new SiliconChargeAliveEvent(uid, batteryComp, batteryUid));
    }

    // Box Change Start - Alt Low Battery System
    private void OnStandUpAttempt(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, ref StandUpAttemptEvent args)
    {
        if (siliconDeadComp.Dead)
            args.Cancelled = true;
    }
    // Box Change End
}

/// <summary>
///     A cancellable event raised when a Silicon is about to go down due to charge.
/// </summary>
/// <remarks>
///     This probably shouldn't be modified unless you intend to fill the Silicon's battery,
///     as otherwise it'll just be triggered again next frame.
/// </remarks>
public sealed class SiliconChargeDyingEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDyingEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has gone down due to charge.
/// </summary>
public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has reawoken due to an increase in charge.
/// </summary>
public sealed class SiliconChargeAliveEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeAliveEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}
