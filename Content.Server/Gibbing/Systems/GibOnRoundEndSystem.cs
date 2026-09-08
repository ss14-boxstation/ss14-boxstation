using Content.Shared.GameTicking;
using Content.Shared.Gibbing.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Systems;
using Content.Shared.Gibbing;
using Robust.Shared.Random; // Box Change: For paraclone RNG

namespace Content.Server.Gibbing.Systems;
public sealed class GibOnRoundEndSystem : EntitySystem
{
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly IRobustRandom _random = default!; // Box Change: For paraclone RNG

    public override void Initialize()
    {
        base.Initialize();

        // this is raised after RoundEndTextAppendEvent, so they can successfully greentext before we gib them
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundEndMessageEvent args)
    {
        var gibQuery = EntityQueryEnumerator<GibOnRoundEndComponent>();

        // gib everyone with the component
        while (gibQuery.MoveNext(out var uid, out var gibComp))
        {
            var gib = false;
            // if they fulfill all objectives given in the component they are not gibbed
            if (_mind.TryGetMind(uid, out var mindId, out var mindComp))
            {
                foreach (var objectiveId in gibComp.PreventGibbingObjectives)
                {
                    if (!_mind.TryFindObjective((mindId, mindComp), objectiveId, out var objective)
                        || !_objectives.IsCompleted(objective.Value, (mindId, mindComp)))
                    {
                        gib = true;
                        break;
                    }
                }
            }
            else
                gib = true;

            // Start Box Change: Roll for safety, even if their objectives haven't been completed
            if (_random.Prob(gibComp.SafetyChance))
            {
                gib = false;
            }
            // End Box Change

            if (!gib)
                continue;

            if (gibComp.SpawnProto != null)
                SpawnAtPosition(gibComp.SpawnProto, Transform(uid).Coordinates);

            _gibbing.Gib(uid);
        }
    }
}
