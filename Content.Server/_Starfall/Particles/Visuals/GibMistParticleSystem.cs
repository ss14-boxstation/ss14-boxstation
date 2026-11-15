using Content.Shared._Starfall.Particles.Visuals;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Gibbing;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Starfall.Particles.Visuals;

/// <summary>
/// Reads the blood color of a gibbed entity and forwards it to nearby clients via
/// <see cref="GibMistParticleEvent"/> so they can tint the particle effect correctly.
/// This is the only server-side particle code that cannot be eliminated and it bothers me deeply.
/// <see cref="GibbingSystem.Gib"/> is server-side only and raises <see cref="BeingGibbedEvent"/>
/// at the exact moment of gibbing, so this is the only way to get the blood color.
/// </summary>
/// TODO: KILL WHEN GIBBING IS PREDICTED/SHARED I BEG
public sealed partial class GibMistParticleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        // SubscribeLocalEvent<BloodstreamComponent, BeingGibbedEvent>(OnBeingGibbed); // Box Change: Disable gib mist effect
    }

    private void OnBeingGibbed(Entity<BloodstreamComponent> ent, ref BeingGibbedEvent args)
    {
        var color = Color.Red;
        var contents = ent.Comp.BloodReferenceSolution.Contents;
        if (contents.Count > 0 && _proto.TryIndex(contents[0].Reagent.Prototype, out ReagentPrototype? reagentProto))
            color = reagentProto.SubstanceColor;

        RaiseNetworkEvent(new GibMistParticleEvent(_transform.GetMapCoordinates(ent), color), Filter.Pvs(ent.Owner));
    }
}
