using Robust.Shared.GameStates;

namespace Content.Shared._Box.Traits;

/// <summary>
/// Set a prefix to all instances of the players original species
/// i.e. "Vulpkanin" > "Synthetic Vulpkanin"
/// </summary>
/// <remark>
/// based heavily on the Cosmatic Drift synth trait
/// Box - Added support for custom strings
/// </remark>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeciesPrefixComponent : Component
{

    /// <summary>
    /// The Loc String to pull when the entity is Examined
    /// The string must always take a "species" arguement
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ExaminePrefix { get; set; } = "strange-prefix-examine";

    /// <summary>
    /// The Loc String to pull in all other circumstances
    /// The string must always take a "species" arguement
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Prefix { get; set; } = "strange-species-prefix";

}
