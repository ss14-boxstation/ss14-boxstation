using Robust.Shared.Map;

namespace Content.Shared.Maps; // Upstream namespace intentional

public sealed partial class TileSystem : EntitySystem
{
    /// <summary>
    /// Deconstructs tiles, but leaves lattice and planet floors intact.
    /// </summary>
    public bool DeconstructTileSafe(TileRef tileRef, bool pryPlating)
    {
        var tile = tileRef.Tile;

        if (tile.IsEmpty)
            return false;

        var tileDef = (ContentTileDefinition)_tileDefinitionManager[tile.TypeId];

        if (tileDef.MapAtmosphere)
            return false;

        return DeconstructTile(tileRef, false);
    }
}
