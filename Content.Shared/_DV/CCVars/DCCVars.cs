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
        CVarDef.Create("traits.max_count", 25, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Maximum trait points available to spend.
    /// Traits with positive cost consume points, negative cost traits grant points.
    /// </summary>
    public static readonly CVarDef<int> MaxTraitPoints =
        CVarDef.Create("traits.max_points", 15, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Whether to skip showing the disabled traits popup when spawning.
    /// </summary>
    public static readonly CVarDef<bool> SkipDisabledTraitsPopup =
        CVarDef.Create("traits.skip_disabled_traits_popup", false, CVar.CLIENT | CVar.ARCHIVE);
		
    /// <summary>
    /// Disables all vision filters for species like Vulpkanin or Harpies. There are good reasons someone might want to disable these.
    /// </summary>
    public static readonly CVarDef<bool> NoVisionFilters =
        CVarDef.Create("accessibility.no_vision_filters", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
