namespace Content.Server._Box.Instruments;

/// <summary>
/// Allows the entity to toggle DAW-style MIDI playback at will via a right-click verb.
/// </summary>
[RegisterComponent]
[Access(typeof(LyrebirdSystem))]
public sealed partial class LyrebirdComponent : Component
{
    /// <summary>
    /// Whether the entity is able to perform percussion without DAW mode enabled.
    /// </summary>
    [DataField]
    public bool DefaultPercussion = false;
}
