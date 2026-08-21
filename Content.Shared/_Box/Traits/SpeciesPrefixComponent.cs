using Robust.Shared.GameStates;

namespace Content.Shared._Box.Traits;

/// <summary>
/// Set a prefix to all instances of the players original species
/// i.e. "Vulpkanin" > "Synthetic Vulpkanin
/// </summary>
/// <remark>
/// based heavily on the Cosmatic Drift synth trait
/// </remark>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeciesPrefixComponent : Component {
    // todo: make this take a loc string
}
