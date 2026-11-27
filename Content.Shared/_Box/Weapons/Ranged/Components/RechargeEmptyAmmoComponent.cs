using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Prevents <see cref="RechargeBasicEntityAmmoComponent"/> from recharging until the gun is empty.
/// Then, prevents the gun from shooting until its full again.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RechargeEmptyAmmoComponent : Component
{

    /// <summary>
    /// The audio that plays when the gun starts recharging
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public SoundSpecifier? RechargeStartSound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg")
    {
        Params = AudioParams.Default.WithVolume(-5f)
    };

    /// <summary>
    /// The audio that plays when the gun finishes recharging
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public SoundSpecifier? RechargeEndSound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg")
    {
        Params = AudioParams.Default.WithVolume(-5f)
    };

    /// <summary>
    /// Should you be prevented from firing while the gun is reloading?
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public bool RechargePreventShot = true;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsRecharging = false;
}
