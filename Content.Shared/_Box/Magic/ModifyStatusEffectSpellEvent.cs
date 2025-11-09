using Content.Shared.Actions;

namespace Content.Shared._Box.Magic;

public sealed partial class ModifyStatusEffectSpell : EntityTargetActionEvent
{
    [DataField]
    public string? Key;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [DataField]
    public string Component = String.Empty;
}
