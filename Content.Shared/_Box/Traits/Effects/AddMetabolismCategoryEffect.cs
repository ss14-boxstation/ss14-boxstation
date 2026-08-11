using Content.Shared._Box.Metabolism;
using Content.Shared._DV.Traits.Effects;
using Content.Shared.Body;
using Content.Shared.Metabolism;
using Robust.Shared.Prototypes;

namespace Content.Shared._Box.Traits.Effects;

/// <summary>
/// Effect that adds metabolism categories to the player entity's organs.
/// </summary>
public sealed partial class AddMetabolismCategoryEffect : BaseTraitEffect
{
    /// <summary>
    /// The metabolsim categories to add to the entity.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<MetabolismCategoryPrototype>> Categories;

    public override void Apply(TraitEffectContext ctx)
    {
        ctx.EntMan.TryGetComponent<BodyComponent>(ctx.Player, out var body);
        if (body == null || body.Organs == null)
            return;

        foreach (var organ in body.Organs.ContainedEntities)
        {
            if (!ctx.EntMan.TryGetComponent<MetabolizerComponent>(organ, out var comp))
                continue;
            foreach (var category in Categories)
            {
                if (comp.MetabolismCategories.Contains(category))
                    continue;
                comp.MetabolismCategories.Add(category);
            }
        }
    }
}
