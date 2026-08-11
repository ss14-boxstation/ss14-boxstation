using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks if the player meets overall playtime requirements.
/// </summary>
public sealed partial class PlaytimeCondition : BaseTraitCondition
{
    /// <summary>
    /// The required time, in minutes.
    /// </summary>
    [DataField(required: true)]
    public int Time;

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (ctx.PlayTimes == null)
            return false;

        var hasPlaytime = ctx.PlayTimes.TryGetValue("Overall", out var playtime);
        return Time < playtime.TotalMinutes;
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc)
    {
        return loc.GetString("trait-condition-overall-playtime", ("time", Time / 60));
    }
}
