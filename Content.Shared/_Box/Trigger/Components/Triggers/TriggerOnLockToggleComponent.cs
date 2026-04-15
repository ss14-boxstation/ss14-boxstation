using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;

namespace Content.Shared._Box.Trigger.Components.Triggers;

/// <summary>
/// Triggers an entity with <see cref="LockCompoent"/> when the lock
/// is toggled.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnLockToggleComponent : BaseTriggerOnXComponent;
