using Content.Shared._Starfall.Particles;
using Content.Shared._Starfall.Particles.Visuals;
using Robust.Shared.Prototypes;

namespace Content.Client._Starfall.Particles.Visuals;

/// <summary>
/// Receives <see cref="GibMistParticleEvent"/> from the server and spawns
/// a blood-mist particle burst tinted to the entity's actual blood color.
/// </summary>
public sealed class GibMistParticleSystem : EntitySystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;

    private static readonly ProtoId<ParticleEffectPrototype> MistEffect = "MoffGibMist"; // Box Change: Moff effects
    private static readonly ProtoId<ParticleEffectPrototype> SpeckEffect = "MoffGibSpecks"; // Box Change: Moff effects

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GibMistParticleEvent>(OnGibMist);
    }

    private void OnGibMist(GibMistParticleEvent ev)
    {
        var emitter = _particles.SpawnEffect(MistEffect, ev.Coords);
        // Start Box Change: Moff effects
        var emitter2 = _particles.SpawnEffect(SpeckEffect, ev.Coords);
        if (emitter == null || emitter2 == null)
        // End Box Change
            return;

        emitter.ColorOverride = ev.BloodColor;
        emitter2.ColorOverride = ev.BloodColor; // Box Change: Moff Effects
    }
}

