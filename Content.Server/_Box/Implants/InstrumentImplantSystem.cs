using Content.Server._Box.Implants.Components;
using Content.Server.Instruments;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Instruments;
using Content.Shared.Radio.Components;
using System.Linq;

namespace Content.Server._Box.Implants;

public sealed class InstrumentImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedInstrumentSystem _sharedInstrument = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InstrumentImplantComponent, ImplantImplantedEvent>(OnImplantImplanted);
        SubscribeLocalEvent<InstrumentImplantComponent, ImplantRemovedEvent>(OnImplantRemoved);
    }

    /// <summary>
    /// If implanted with a radio implant, installs the necessary intrinsic radio components
    /// </summary>
    private void OnImplantImplanted(Entity<InstrumentImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!TryComp<SwappableInstrumentComponent>(args.Implanted, out var target))
        {
            return;
        }

        foreach (var instrument in ent.Comp.InstrumentList)
        {
            if (target.InstrumentList.TryAdd(instrument.Key, instrument.Value))
            {
                ent.Comp.RemovalList.Add(instrument.Key);
            }
        }
    }

    /// <summary>
    /// Removes intrinsic radio components once the Radio Implant is removed
    /// </summary>
    private void OnImplantRemoved(Entity<InstrumentImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        if (!TryComp<SwappableInstrumentComponent>(args.Implanted, out var target))
            return;

        foreach (var instrument in ent.Comp.RemovalList)
        {
            target.InstrumentList.Remove(instrument);
            if (!TryComp<InstrumentComponent>(args.Implanted, out var instComp))
                return; // How did we get here?
            _sharedInstrument.SetInstrumentProgram(args.Implanted, instComp, target.InstrumentList.ElementAt(0).Value.Item1, target.InstrumentList.ElementAt(0).Value.Item2);
        }
    }
}
