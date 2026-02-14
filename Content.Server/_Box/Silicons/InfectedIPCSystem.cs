using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared._Box.Silicons;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;
namespace Content.Server._Box.Silicons
{
    public sealed partial class InfectedIPCSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            var curTime = _timing.CurTime;

            // Hurt Infected IPCs
            var query = EntityQueryEnumerator<InfectedIPCComponent, DamageableComponent>();
            while (query.MoveNext(out var uid, out var comp, out var damage))
            {
                // Process only one per second
                if (comp.NextTick > curTime)
                    continue;
                comp.NextTick = curTime + TimeSpan.FromSeconds(1f);

                comp.GracePeriod -= TimeSpan.FromSeconds(1f);
                if (comp.GracePeriod > TimeSpan.Zero)
                    continue;

                _damageable.ChangeDamage((uid, damage), comp.Damage, true, false);

                // show signs of infection
                if (_random.Prob(comp.InfectionWarningChance))
                    _popup.PopupEntity(Loc.GetString(_random.Pick(comp.InfectionWarnings)), uid, uid);
            }
        }
    }
}
