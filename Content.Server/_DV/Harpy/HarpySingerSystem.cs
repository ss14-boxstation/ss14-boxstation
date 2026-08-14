using Content.Server.Instruments;
using Content.Server.Instruments;
using Content.Server.Speech.Components;
using Content.Shared.Instruments;
using Content.Shared.ActionBlocker;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems; //Box Change - Namespace change
using Content.Shared.Damage.ForceSay;
using Content.Shared._DV.Harpy;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.UserInterface;
using Content.Shared.Zombies;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
// Start Box Change: Additions for sspeech/emote checks
using Content.Shared.Speech.Muting;
using Content.Shared.Abilities.Mime;
// End Box Change

namespace Content.Server._DV.Harpy;

public sealed class HarpySingerSystem : SharedHarpySingerSystem
{
    [Dependency] private readonly InstrumentSystem _instrument = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InstrumentComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<EntityZombifiedEvent>(OnZombified);
        //SubscribeLocalEvent<InstrumentComponent, KnockedDownEvent>(OnKnockedDown); // Box Change: Disabled, see below
        SubscribeLocalEvent<InstrumentComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<InstrumentComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<InstrumentComponent, StatusEffectAddedEvent>(OnStatusEffect);
        SubscribeLocalEvent<InstrumentComponent, DamageChangedEvent>(OnDamageChanged);

        // This is intended to intercept the UI event and stop the MIDI UI from opening if the
        // singer is unable to sing. Thus it needs to run before the ActivatableUISystem.
        SubscribeLocalEvent<HarpySingerComponent, OpenUiActionEvent>(OnInstrumentOpen, before: new[] { typeof(ActivatableUISystem) });
    }

    private void OnEquip(GotEquippedEvent args)
    {
        // Check if an item that makes the singer mumble is equipped to their face
        // (not their pockets!). As of writing, this should just be the muzzle.
        if (TryComp<AddAccentClothingComponent>(args.Equipment, out var accent) &&
            accent.ReplacementPrototype == "mumble" &&
            args.Slot == "mask")
        {
            CloseMidiUi(args.Equipee);
        }
    }

    private void OnMobStateChangedEvent(EntityUid uid, InstrumentComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Critical or MobState.Dead)
            CloseMidiUi(args.Target);
    }

    private void OnZombified(ref EntityZombifiedEvent args)
    {
        CloseMidiUi(args.Target);
    }

    // Box Change: Almost every instance of forced knockdown also comes with stun. All this really does is cause harpies to close their MIDI interface when voluntarily going prone.
    // The only exception I can even find (something that knocks you down without stunning you) is slipping into the bingle pit.
    //private void OnKnockedDown(EntityUid uid, InstrumentComponent component, ref KnockedDownEvent args)
    //{
    //    CloseMidiUi(uid);
    //}
    // End Box Change

    private void OnStunned(EntityUid uid, InstrumentComponent component, ref StunnedEvent args)
    {
        CloseMidiUi(uid);
    }

    private void OnSleep(EntityUid uid, InstrumentComponent component, ref SleepStateChangedEvent args)
    {
        if (args.FellAsleep)
            CloseMidiUi(uid);
    }

    private void OnStatusEffect(EntityUid uid, InstrumentComponent component, StatusEffectAddedEvent args)
    {
        if (args.Key == "Muted")
            CloseMidiUi(uid);
    }

    /// <summary>
    /// Almost a copy of Content.Server.Damage.ForceSay.DamageForceSaySystem.OnDamageChanged.
    /// Done so because DamageForceSaySystem doesn't output an event, and my understanding is
    /// that we don't want to change upstream code more than necessary to avoid merge conflicts
    /// and maintenance overhead. It still reuses the values from DamageForceSayComponent, so
    /// any tweaks to that will keep ForceSay consistent with singing interruptions.
    /// </summary>
    private void OnDamageChanged(EntityUid uid, InstrumentComponent instrumentComponent, DamageChangedEvent args)
    {
        if (!TryComp<DamageForceSayComponent>(uid, out var component) ||
            args.DamageDelta == null ||
            !args.DamageIncreased ||
            args.DamageDelta.GetTotal() < component.DamageThreshold ||
            component.ValidDamageGroups == null)
            return;

        var totalApplicableDamage = FixedPoint2.Zero;
        foreach (var (group, value) in args.DamageDelta.GetDamagePerGroup(_prototype))
        {
            if (!component.ValidDamageGroups.Contains(group))
                continue;

            totalApplicableDamage += value;
        }

        if (totalApplicableDamage >= component.DamageThreshold)
            CloseMidiUi(uid);
    }

    /// <summary>
    /// Closes the MIDI UI if it is open.
    /// </summary>
    private void CloseMidiUi(EntityUid uid)
    {
        if (HasComp<ActiveInstrumentComponent>(uid) &&
            TryComp<ActorComponent>(uid, out var actor))
        {
            _instrument.ToggleInstrumentUi(uid, uid);
        }
    }

    // Box TODO: As it stands, this cancels the UI openinng in a kinda scuffed way that leaves the singing visuals stuck on your character permanently.
    //           We should probably find a way to remove the action completely instead.
    /// <summary>
    /// Prevent the player from opening the MIDI UI under some circumstances.
    /// </summary>
    private void OnInstrumentOpen(EntityUid uid, HarpySingerComponent component, OpenUiActionEvent args)
    {
        // CanSpeak covers all reasons you can't talk, including being incapacitated
        // (crit/dead), asleep, or for any reason mute inclding glimmer or a mime's vow.
        // Start Box Change: Check for Muted/MimePowers components instead of speech blocker. Cover other instances with CanEmote instead.
        var canNotSpeak = HasComp<MutedComponent>(uid) || HasComp<MimePowersComponent>(uid);
        var canNotEmote = !_blocker.CanEmote(uid);
        // End Box Change
        var zombified = TryComp<ZombieComponent>(uid, out var _);
        var muzzled = _inventorySystem.TryGetSlotEntity(uid, "mask", out var maskUid) &&
            TryComp<AddAccentClothingComponent>(maskUid, out var accent) &&
            accent.ReplacementPrototype == "mumble";

        // Set this event as handled when the singer should be incapable of singing in order
        // to stop the ActivatableUISystem event from opening the MIDI UI.
        args.Handled = canNotSpeak || muzzled || zombified || canNotEmote; // Box Change: Add canNotEmote

        // Tell the user that they can not sing.
        if (args.Handled)
            _popupSystem.PopupEntity(Loc.GetString("no-sing-while-no-speak"), uid, uid, PopupType.Medium);
    }
}
