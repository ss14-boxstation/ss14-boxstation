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
/// Effect that completely replaces the metabolizer types of the player entity's organs.
/// </summary>
/// <remarks>
/// Note that the target organ's MetabolizerTypes list must not be null.
/// I'm unsure why a prototype would add the metabolizer component but not define a MetabolizerType, but...
/// </remarks>
public sealed partial class ReplaceMetabolizerTypeEffect : BaseTraitEffect
{
    /// <summary>
    /// The entity's new list of metabolizm types.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<MetabolizerTypePrototype>> Types;

    /// <summary>
    /// Whitelist to filter organs by.
    /// </summary>
    /// <remarks>
    /// Use to, e.g., apply the metabolizer type(s) only to the stomach by filtering for StomachComponent.
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

            comp.MetabolizerTypes.Clear();
            foreach (var type in Types)
            {
                comp.MetabolizerTypes.Add(type);
            }
        }
    }
}
