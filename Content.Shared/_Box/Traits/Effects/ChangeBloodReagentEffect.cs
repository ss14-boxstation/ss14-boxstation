using Content.Shared._DV.Traits.Effects;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Forensics;
using Content.Shared.Forensics.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Box.Traits.Effects;

/// <summary>
/// Effect that changes the player entity's bloodstream reagent.
/// </summary>
public sealed partial class ChangeBloodReagentEffect : BaseTraitEffect
{
    /// <summary>
    /// The target reagent.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent;

    public override void Apply(TraitEffectContext ctx)
    {
        if (!ctx.EntMan.TryGetComponent<BloodstreamComponent>(ctx.Player, out var blood))
            return;

        var newBloodstream = new Solution(Reagent, blood.BloodReferenceSolution.Volume);

        ctx.Bloodstream.ChangeBloodReagents(ctx.Player, newBloodstream);
        // We need to regenerate the DNA, or else the new blood reagent won't have it.
        ctx.Bloodstream.RegenerateDNA(ctx.Player);
    }
}
