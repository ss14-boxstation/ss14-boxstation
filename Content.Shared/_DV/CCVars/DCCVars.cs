using Robust.Shared.Configuration;

namespace Content.Shared._DV.CCVars;

/// <summary>
/// DeltaV specific cvars.
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming - Shush you
public sealed partial class DCCVars
{

    /// <summary>
    /// Maximum number of traits that can be selected globally.
    /// </summary>
    public static readonly CVarDef<int> MaxTraitCount =
        CVarDef.Create("traits.max_count", 1000, CVar.SERVER | CVar.REPLICATED); // Box Change: Make this an arbitrarily high value; change it later if we decide we want to go the "positive traits with point balance" route

    /// <summary>
    /// Maximum trait points available to spend.
    /// Traits with positive cost consume points, negative cost traits grant points.
    /// </summary>
    public static readonly CVarDef<int> MaxTraitPoints =
        CVarDef.Create("traits.max_points", 1000, CVar.SERVER | CVar.REPLICATED); // Box Change: Make this an arbitrarily high value; change it later if we decide we want to go the "positive traits with point balance" route

    /// <summary>
    /// Whether to skip showing the disabled traits popup when spawning.
    /// </summary>
    public static readonly CVarDef<bool> SkipDisabledTraitsPopup =
        CVarDef.Create("traits.skip_disabled_traits_popup", false, CVar.CLIENT | CVar.ARCHIVE);
}
