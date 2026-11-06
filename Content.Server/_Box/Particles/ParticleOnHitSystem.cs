using Content.Shared._Box.Particles;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Box.Particles;

public sealed class ParticleOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ParticleOnHitComponent, DamageChangedEvent>(OnDamageChanged);
    }

    public void OnDamageChanged(Entity<ParticleOnHitComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta != null && _random.Next(1, 100) < ent.Comp.Chance)
        {
            if (ent.Comp.DamageTypes.Count > 0)
            {
                var damageDict = args.DamageDelta.DamageDict;
                foreach (var (type, _) in damageDict)
                {
                    if (ent.Comp.DamageTypes.Contains(type) && damageDict[type] > ent.Comp.MinDamage)
                    {
                        DoParticles(ent, ref args);
                        break;
                    }
                }
            }
            else if (args.DamageDelta.GetTotal().Float() > ent.Comp.MinDamage)
                DoParticles(ent, ref args);
        }
    }

    private void DoParticles(Entity<ParticleOnHitComponent> ent, ref DamageChangedEvent args)
    {
        var coords = _transform.GetMapCoordinates(ent.Owner);
        var colorOverride = ent.Comp.ColorOverride;
        if (ent.Comp.UseBloodColor)
        {
            if (TryComp<BloodstreamComponent>(ent, out var bloodComp))
            {
                var contents = bloodComp.BloodReferenceSolution.Contents;
                if (contents.Count > 0 && _proto.TryIndex(contents[0].Reagent.Prototype, out ReagentPrototype? reagentProto))
                    colorOverride = reagentProto.SubstanceColor;
            }
        }
        var ev = new ParticleOnHitEvent(ent.Comp.Effect, coords, GetNetEntity(ent), colorOverride, ent.Comp.SpawnOffset);
        RaiseNetworkEvent(ev);
    }
}
