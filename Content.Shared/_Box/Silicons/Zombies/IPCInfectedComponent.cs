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
}