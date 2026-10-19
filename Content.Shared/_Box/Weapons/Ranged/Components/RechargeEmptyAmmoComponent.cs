using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Recharges a gun upon the ammo being empty
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RechargeEmptyAmmoComponent : Component
{

    /// <summary>
    /// The audio that plays when the gun is recharging
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public SoundSpecifier? RechargeSound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg")
    {
        Params = AudioParams.Default.WithVolume(-5f)
    };

    /// <summary>
    /// Should reloading force a use delay?
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public bool ForceUseDelay = true;

}
