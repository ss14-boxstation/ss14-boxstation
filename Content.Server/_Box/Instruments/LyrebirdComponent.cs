namespace Content.Server._Box.Instruments;

[RegisterComponent]
[Access(typeof(LyrebirdSystem))]
public sealed partial class LyrebirdComponent : Component
{
    public bool DefaultPercussion = false;
}
