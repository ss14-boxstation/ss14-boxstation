using Content.Shared.Weapons.Ranged.Upgrades.Components;

namespace Content.Shared._Box.Weapons.Ranged.Upgrades;

/// <summary>
/// Raised when an upgraded gun is upgraded with a fire rate buff
/// </summary>
[ByRefEvent]
public record struct UpgradeableGunRefreshFireRateEvent(float Coefficient);
