using Content.Shared.Damage;
using Robust.Shared.Timing;
using Content.Shared._Box.Silicons;
namespace Content.Server._Box.Silicons
{    
    public sealed partial class InfectedIPCSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;

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
                
                _damageable.TryChangeDamage(uid, comp.Damage, true, false, damage);
            }
        }
    }
}