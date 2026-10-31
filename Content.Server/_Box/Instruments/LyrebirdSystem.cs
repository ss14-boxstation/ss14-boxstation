using Content.Server.Instruments;
using Content.Shared.Instruments;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using System.Diagnostics.Metrics;

namespace Content.Server._Box.Instruments;

public sealed class LyrebirdSystem : EntitySystem
{
    [Dependency] private readonly SharedInstrumentSystem _sharedInstrument = default!;
    [Dependency] private readonly InstrumentSystem _instrument = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LyrebirdComponent, GetVerbsEvent<AlternativeVerb>>(AddVerb);
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
            //Category = VerbCategory.InstrumentStyle,
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
        lyrebird.DefaultPercussion = instrument.AllowPercussion;
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

    private void CloseUi(EntityUid ent, InstrumentComponent instrument)
    {
        if (HasComp<ActiveInstrumentComponent>(ent))
            _instrument.ToggleInstrumentUi(ent, ent, instrument);
    }
}
