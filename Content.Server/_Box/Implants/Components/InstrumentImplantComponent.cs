namespace Content.Server._Box.Implants.Components;

/// <summary>
/// Expands the user's natural instrument selection.
/// </summary>
[RegisterComponent]
public sealed partial class InstrumentImplantComponent : Component
{
    /// <summary>
    /// The instruments to add.
    /// See <see cref="SwappableInstrumentComponent"/>
    /// string = display name of the instrument
    /// byte 1 = instrument midi program
    /// byte 2 = instrument midi bank
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, (byte, byte)> InstrumentList = new();
    /// <summary>
    /// List of instruments to remove upon deimplantation.
    /// Instruments are only added to this list if they were successfully added to the target, so that the target does not lose natural instruments when the implant is removed.
    /// </summary>
    public HashSet<string> RemovalList = [];
}
