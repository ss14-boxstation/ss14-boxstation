using Content.Server._Box.Implants.Components;
using Content.Server.Instruments;
using Content.Server._Box.Instruments;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Instruments;
using Content.Shared.Radio.Components;
using System.Linq;

namespace Content.Server._Box.Implants;

public sealed class LyrebirdImplantSystem : EntitySystem
{
    [Dependency] private readonly LyrebirdSystem _lyrebird = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LyrebirdImplantComponent, ImplantImplantedEvent>(OnImplantImplanted);
        SubscribeLocalEvent<LyrebirdImplantComponent, ImplantRemovedEvent>(OnImplantRemoved);
    }

    /// <summary>
    /// If implanted with a radio implant, installs the necessary intrinsic radio components
    /// </summary>
    private void OnImplantImplanted(Entity<LyrebirdImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        EnsureComp<LyrebirdComponent>(args.Implanted, out var lyrebird);
        _lyrebird.AddImplant(args.Implanted);
    }

    /// <summary>
    /// Removes intrinsic radio components once the Radio Implant is removed
    /// </summary>
    private void OnImplantRemoved(Entity<LyrebirdImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        _lyrebird.RemoveImplant(args.Implanted);
        RemComp<LyrebirdComponent>(args.Implanted);
    }
}
