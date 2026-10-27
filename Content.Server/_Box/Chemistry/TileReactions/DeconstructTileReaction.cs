using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Server._Box.Chemistry.TileReactions;

/// <summary>
/// A version of PryTileReaction that instead deconstructs the tile down to lattice.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class DeconstructTileReaction : ITileReaction
{
    public FixedPoint2 TileReact(TileRef tile,
        ReagentPrototype reagent,
        FixedPoint2 reactVolume,
        IEntityManager entityManager,
        List<ReagentData>? data)
    {
        var sys = entityManager.System<TileSystem>();
        sys.DeconstructTile(tile);
        return reactVolume;
    }
}
