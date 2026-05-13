using Content.Shared.Examine;

namespace Content.Shared._Box.SimpleExamineText;

public sealed class SimpleExamineTextSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SimpleExamineTextComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, SimpleExamineTextComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(component.ExamineText));
    }
}
