using Content.Server._Box.Implants.Components;
using Content.Server._Box.Instruments;
using Content.Shared.Implants;

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

    private void OnImplantImplanted(Entity<LyrebirdImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        EnsureComp<LyrebirdComponent>(args.Implanted, out var lyrebird);
        _lyrebird.AddImplant(args.Implanted);
    }

    private void OnImplantRemoved(Entity<LyrebirdImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        _lyrebird.RemoveImplant(args.Implanted);
        RemComp<LyrebirdComponent>(args.Implanted);
    }
}
