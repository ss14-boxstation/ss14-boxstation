using Content.Server.Instruments;
using Content.Shared.Instruments;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Server._Box.Instruments;

public sealed class LyrebirdSystem : EntitySystem
{
    [Dependency] private readonly SharedInstrumentSystem _sharedInstrument = default!;
    [Dependency] private readonly InstrumentSystem _instrument = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LyrebirdComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LyrebirdComponent, GetVerbsEvent<AlternativeVerb>>(AddVerb);
    }

    private void OnMapInit(Entity<LyrebirdComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<InstrumentComponent>(ent.Owner, out var instrument))
            return;
        ent.Comp.DefaultPercussion = instrument.AllowPercussion; // Store the user's pre-implanting percussive ability, so they don't lose it when the implant is toggled or removed.
    }

    private void AddVerb(Entity<LyrebirdComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<InstrumentComponent>(ent.Owner, out var instrument) || !TryComp<LyrebirdComponent>(ent, out var lyrebird))
            return;

        if (ent.Owner != args.User)
            return;

        var user = args.User;
        AlternativeVerb selection = new()
        {
            Text = "Toggle DAW mode",
            Priority = -1,
            Act = () =>
            {
                bool state = false;
                if (!instrument.AllowProgramChange)
                {
                    state = true;
                    _sharedInstrument.SetProgramChange(ent, instrument, true, true);
                }
                else
                {
                    state = false;
                    _sharedInstrument.SetProgramChange(ent, instrument, false, lyrebird.DefaultPercussion);
                }
                CloseUi(ent, instrument);
                _popup.PopupEntity(Loc.GetString("lyrebird-toggle", ("state", state ? "Enabled" : "Disabled")), user, user);
            }
        };
        args.Verbs.Add(selection);
    }
    public void AddImplant(EntityUid ent)
    {
        if (!TryComp<InstrumentComponent>(ent, out var instrument) || !TryComp<LyrebirdComponent>(ent, out var lyrebird))
            return;
        _sharedInstrument.SetProgramChange(ent, instrument, true, true);
        CloseUi(ent, instrument);
    }

    public void RemoveImplant(EntityUid ent)
    {
        if (!TryComp<InstrumentComponent>(ent, out var instrument) || !TryComp<LyrebirdComponent>(ent, out var lyrebird))
            return;
        _sharedInstrument.SetProgramChange(ent, instrument, false, lyrebird.DefaultPercussion);
        CloseUi(ent, instrument);
    }

    /// <summary>
    /// Forcibly closes the user's MIDI interface.
    /// Toggling AllowProgramChange mid-playback has odd and inconsistent behavior, so it's best to just completely stop all playback and reset the player whenever it's changed.
    /// </summary>
    private void CloseUi(EntityUid ent, InstrumentComponent instrument)
    {
        if (HasComp<ActiveInstrumentComponent>(ent))
            _instrument.ToggleInstrumentUi(ent, ent, instrument);
    }
}
