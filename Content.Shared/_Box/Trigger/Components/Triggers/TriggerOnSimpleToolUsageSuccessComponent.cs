using Content.Shared.Tools.Components;
using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;

namespace Content.Shared._Box.Trigger.Components.Triggers;

/// <summary>
/// This is a clone of <see cref="TriggerOnSimpleToolUsageComponent"/> that actually checks if the doAfter was completed succesfully.
/// This is presumably how it's supposed to function, but it is not.
/// Implemented as its own namespaced component instead of changing the original code to avoid potential upstream conflicts.
/// Triggers an entity with <see cref="SimpleToolUsageComponent"/> when the correct tool
/// is used on it and the DoAfter has finished.
/// The user is the player using the tool.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnSimpleToolUsageSuccessComponent : BaseTriggerOnXComponent;
