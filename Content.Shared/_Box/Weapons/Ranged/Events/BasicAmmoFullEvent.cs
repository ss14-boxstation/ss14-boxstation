namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Raised on guns with <see cref""/> that have finished reloading
/// </summary>
[ByRefEvent]
public record struct BasicAmmoFullEvent(EntityUid User);
