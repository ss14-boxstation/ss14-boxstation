using Content.Client._Starfall.Particles;
using Content.Shared._Box.Particles;
using Robust.Client.GameObjects;

namespace Content.Client._Box.Particles;

/// <summary>
/// Adds fire and smoke particles to static, non-burning entities, for props like bonfires.
/// </summary>
public sealed class FireParticleSystem : EntitySystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    private sealed class FireState
    {
        public ActiveEmitter? FireEmitter;
        public ActiveEmitter? SmokeEmitter;
    }

    private readonly Dictionary<EntityUid, FireState> _active = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FireParticlesComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<FireParticlesComponent, ComponentShutdown>(OnCompShutdown);
    }

    private void OnCompInit(Entity<FireParticlesComponent> ent, ref ComponentInit args)
    {
        if (!_active.TryGetValue(ent, out var state))
        {
            state = new FireState();
            _active[ent] = state;
        }

        // Ignited: spawn emitter
        var coords = _transform.GetMapCoordinates(ent);
        state.SmokeEmitter = _particles.SpawnEffect(ent.Comp.SmokeEffect, coords, ent.Owner, ent.Comp.SmokeColorOverride);
        state.FireEmitter = _particles.SpawnEffect(ent.Comp.FireEffect, coords, ent.Owner, ent.Comp.FireColorOverride);

        if (state.FireEmitter == null || state.SmokeEmitter == null)
            return;

        if (ent.Comp.SmokeSpawnOffset != default)
            state.SmokeEmitter.SpawnOffset = ent.Comp.SmokeSpawnOffset;

        if (ent.Comp.FireSpawnOffset != default)
            state.FireEmitter.SpawnOffset = ent.Comp.FireSpawnOffset;


        state.SmokeEmitter.Intensity = ent.Comp.SmokeIntensity;
        state.FireEmitter.Intensity = ent.Comp.FireIntensity;
    }

    private void OnCompShutdown(Entity<FireParticlesComponent> ent, ref ComponentShutdown args)
    {
        if (_active.Remove(ent, out var state))
            StopState(state);
    }

    private void StopState(FireState state)
    {
        if (state.FireEmitter != null)
        {
            _particles.RemoveParticle(state.FireEmitter);
        }

        if (state.SmokeEmitter != null)
        {
            _particles.RemoveParticle(state.SmokeEmitter);
        }
    }
}

