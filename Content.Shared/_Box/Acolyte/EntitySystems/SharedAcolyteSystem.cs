using Content.Shared._Box.Acolyte.Components;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._Box.Acolyte.EntitySystems;

public abstract class SharedAcolyteSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AcolyteComponent, MapInitEvent>(OnAcolyteMapInit);
    }

    private void OnAcolyteMapInit(Entity<AcolyteComponent> entity, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(entity, ref entity.Comp.SoulHarvestActionEntity, entity.Comp.SoulHarvestAction);
        Dirty(entity);
    }
}

