using Content.Shared._Box.Light;
using Content.Shared._Starfall.Particles;
using Robust.Shared.Random;

namespace Content.Client._Starfall.Particles;

public sealed partial class ParticleOnEventSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    private readonly Dictionary<EntityUid, ActiveEmitter> _lightEmitters = new();
    public void BoxInit()
    {
        SubscribeNetworkEvent<LightBreakEvent>(OnBreak);
        SubscribeLocalEvent<ParticleOnLightBreakComponent, PredictedLightBreakEvent>(OnPredictedBreak);
    }

    private void OnBreak(LightBreakEvent args)
    {
        EntityUid uid = GetEntity(args.Entity);
        if (!TryComp<ParticleOnLightBreakComponent>(uid, out var comp))
            return;

        Entity<ParticleOnLightBreakComponent> ent = (uid, comp);

        SpawnParticles(ent);
    }
    private void OnPredictedBreak(Entity<ParticleOnLightBreakComponent> ent, ref PredictedLightBreakEvent args)
    {
        SpawnParticles(ent);
    }

    private void SpawnParticles(Entity<ParticleOnLightBreakComponent> ent)
    {
        if (!_lightEmitters.TryGetValue(ent.Owner, out var activeEmitter) || activeEmitter.Exhausted == true)
        {
            _particles.RemoveParticle(activeEmitter);
            var particle = ent.Comp.Effects[_random.Next(ent.Comp.Effects.Count)];
            var emitter = _particles.CreateParticle(particle, ent.Owner, ent.Comp.ColorOverride);
            _lightEmitters.Remove(ent.Owner);
            if (emitter != null)
                _lightEmitters.Add(ent.Owner, emitter);
        }
    }
}
