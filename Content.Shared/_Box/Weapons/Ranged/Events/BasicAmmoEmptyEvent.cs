namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Raised on guns with <see cref"RechargeBasicEntityAmmoComponent"/> that run out of ammo
/// </summary>
[ByRefEvent]
public record struct BasicAmmoEmptyEvent(EntityUid User);
