using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Box.Silicons;

/// <summary>
/// Entities with this component take caustic damage over time as they are infected.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class InfectedIPCComponent : Component
{
    /// <summary>
    ///   The damage amount applied to infected IPC over time.
    /// </summary>
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Caustic", 0.3 } // parity with the poison damage from Romerol
        }
    };
    // For use in the InfectedIPCSystem as a keeper of time
    [DataField]
    public TimeSpan NextTick;

    /// <summary>
    /// The amount of time left before infected IPCs begin taking damage
    /// </summary>
    [DataField("gracePeriod"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan GracePeriod = TimeSpan.FromSeconds(20f);

    /// <summary>
    /// Popup infection warning so the IPC knows something is wrong.
    /// </summary>
    [DataField("infectionWarning")]
    public List<string> InfectionWarnings = new()
    {
        "ipc-zombie-infection-warning",
        "ipc-zombie-infection-underway",
        "ipc-zombie-infection-continues"
    };

    /// <summary>
    /// The chance each second that a warning will be shown to the afflicted IPC.
    /// </summary>
    [DataField("infectionWarningChance")]
    public float InfectionWarningChance = 0.0166f; // same as normal zombie warnings
}
