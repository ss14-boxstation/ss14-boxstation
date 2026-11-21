namespace Content.Server._Box.Objectives.Components;

/// <summary>
/// Given to Learner Roles at round start. Used to prevent objective assignments.
/// </summary>
[RegisterComponent]
public sealed partial class LearnerStaffComponent : Component
{

}

// TODO this should probably be on a mind role, not the mob
// Only when WizDen's CommandStaffComponent is also set to do that.
