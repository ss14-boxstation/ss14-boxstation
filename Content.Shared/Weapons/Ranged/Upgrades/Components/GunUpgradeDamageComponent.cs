using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the damage of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeSystem))]
public sealed partial class GunUpgradeDamageComponent : Component
{
// Box Change Start - Move to coefficient
    // /// <summary>
    // /// Additional damage added onto the projectile's base damage.
    // /// </summary>
    // [DataField]
    // public DamageSpecifier Damage = new();

    /// <summary>
    /// Additional damage added onto the projectile.
    /// Each modkit adds to the total coefficient.
    /// </summary>
    [DataField]
    public float DamageCoefficient = 0.5f; // Could this be replaced with a coefficient per damage type?
// Box Change End
}
