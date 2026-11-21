using Content.Client._Starfall.Particles;
using Content.Shared._Box.Particles;
using Robust.Client.GameObjects;

namespace Content.Client._Box.Particles;

/// <summary>
/// Spawns particles when Xeno acid spray triggers.
/// </summary>
public sealed class AcidSprayParticleSystem : EntitySystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<AcidSprayParticleEvent>(OnAcidSpray);
    }

    private void OnAcidSpray(AcidSprayParticleEvent args)
    {
        var ent = GetEntity(args.Entity);
        _particles.CreateParticle(args.Effect, ent, args.Color);
    }
}

