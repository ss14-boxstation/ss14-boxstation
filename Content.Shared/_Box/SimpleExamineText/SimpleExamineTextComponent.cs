using Robust.Shared.GameStates;

namespace Content.Shared._Box.SimpleExamineText;

/// <summary>
/// Adds a line of text to the entity's examine window.
/// </summary>
[RegisterComponent]
public sealed partial class SimpleExamineTextComponent : Component
{
    /// <summary>
    ///     Text to add to the examinee window.
    /// </summary>
    [DataField]
    public LocId ExamineText;
}
