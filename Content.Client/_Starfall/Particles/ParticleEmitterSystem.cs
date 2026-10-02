using Content.Shared._Starfall.Particles;
using Robust.Shared.Map;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// Spawns a particle effect on this client when an entity with
/// <see cref="ParticleEmitterComponent"/> is initialized (including on PVS re-entry).
/// </summary>
public sealed class ParticleEmitterSystem : EntitySystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    // Track emitter references so we can stop them when the entity leaves PVS or is removed.
    private readonly Dictionary<EntityUid, ActiveEmitter> _activeEmitters = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ParticleEmitterComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<ParticleEmitterComponent, ComponentShutdown>(OnCompShutdown);
    }

    private void OnCompInit(Entity<ParticleEmitterComponent> ent, ref ComponentInit args)
    {
        // Stop any lingering emitter from a previous PVS cycle for this entity.
        if (_activeEmitters.TryGetValue(ent.Owner, out var old))
        {
            _particles.RemoveParticle(old);
            _activeEmitters.Remove(ent.Owner);
        }

        var coords = _transform.GetMapCoordinates(ent.Owner);

        // Prototype previews and other UI dummy entities are initialized in
        // nullspace. There is no map entity to convert those coordinates
        // through, so wait for a real PVS/map initialization instead of trying
        // to create a world-space emitter for the preview.
        if (coords.MapId == MapId.Nullspace)
            return;

        var emitter = _particles.SpawnEffect(ent.Comp.Effect, coords, ent.Owner, ent.Comp.ColorOverride);
        if (emitter == null)
            return;

        if (ent.Comp.Intensity != 1f) // ᓚᘏᗢ <(loss of precision is fineeeeeee
            emitter.Intensity = ent.Comp.Intensity;

        if (ent.Comp.SpawnOffset != default)
            emitter.SpawnOffset = ent.Comp.SpawnOffset;

        _activeEmitters[ent.Owner] = emitter;
    }

    private void OnCompShutdown(Entity<ParticleEmitterComponent> ent, ref ComponentShutdown args)
    {
        if (_activeEmitters.Remove(ent.Owner, out var emitter))
            _particles.RemoveParticle(emitter);
    }
}

