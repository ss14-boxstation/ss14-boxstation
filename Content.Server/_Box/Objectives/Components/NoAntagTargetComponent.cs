namespace Content.Server._Box.Objectives.Components;

/// <summary>
/// Used to prevent objective assignments. Given to Learner Roles at round start.
/// </summary>
[RegisterComponent]
public sealed partial class NoAntagTargetComponent : Component
{

}

// TODO this should probably be on a mind role, not the mob
// Only when WizDen's CommandStaffComponent is also set to do that.
