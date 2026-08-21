using Content.Shared._Box.Metabolism;
using Content.Shared._DV.Traits.Effects;
using Content.Shared.Body;
using Content.Shared.Metabolism;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed.Commands.Math;
using System.Linq;

namespace Content.Shared._Box.Traits.Effects;

/// <summary>
/// Effect that adds metabolizer types to the player entity's organs.
/// </summary>
public sealed partial class AddMetabolizerTypeEffect : BaseTraitEffect
{
    /// <summary>
    /// The metabolizer types to add to the entity.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<MetabolizerTypePrototype>> Types;

    /// <summary>
    /// Whitelist to filter organs by.
    /// </summary>
    /// <remarks>
    /// Use to, e.g., apply the metabolizer type only to the stomach by filtering for StomachComponent.
    /// </remarks>
    [DataField]
    public EntityWhitelist OrganWhitelist;

    public override void Apply(TraitEffectContext ctx)
    {
        ctx.EntMan.TryGetComponent<BodyComponent>(ctx.Player, out var body);
        if (body == null || body.Organs == null)
            return;

        foreach (var organ in body.Organs.ContainedEntities)
        {
            if (!ctx.EntMan.TryGetComponent<MetabolizerComponent>(organ, out var comp) || comp.MetabolizerTypes == null || ctx.Whitelist.IsWhitelistFail(OrganWhitelist, organ))
                continue;
            foreach (var type in Types)
            {
                if (comp.MetabolizerTypes.Contains(type))
                    continue;
                comp.MetabolizerTypes.Add(type);
            }
        }
    }
}
