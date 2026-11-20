using Content.Client._Starfall.Particles;
using Content.Shared._Box.Particles;

namespace Content.Client._Box.Particles;

public sealed class ParticleOnHitSystem : EntitySystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ParticleOnHitEvent>(OnDamageChanged);
    }

    public void OnDamageChanged(ParticleOnHitEvent args)
    {
        _particles.SpawnEffect(args.Effect, args.Coordinates, GetEntity(args.Entity), args.ColorOverride);
    }
}
