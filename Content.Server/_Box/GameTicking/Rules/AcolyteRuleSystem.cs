using Content.Server._Box.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Administration.Logs;
using Content.Server.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.Popups;
using Content.Server.Mind;
using Content.Shared._Box.Acolyte.Components;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Server.Player;
using Robust.Shared.Utility;

namespace Content.Server._Box.GameTicking.Rules;

public sealed class AcolyteRuleSystem : GameRuleSystem<AcolyteRuleComponent>
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AcolyteComponent, AcolyteSoulHarvestActionEvent>(OnAcolyteSoulHarvest);
    }

    private void OnAcolyteSoulHarvest(Entity<AcolyteComponent> entity, 
        ref AcolyteSoulHarvestActionEvent args)
    {
        
    }
}