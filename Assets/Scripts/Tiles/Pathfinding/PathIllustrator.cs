using System.Collections.Generic;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles drawing highlights & text on paths
 */

public static class PathIllustrator
{
    /// <summary>
    /// Highlights all tiles along a path and displays the distance to each tile
    /// </summary>
    /// <param name="path"></param>
    public static void DrawPath(TileGroup path, int unitMoveRange)
    {
        HighlightPath(path, unitMoveRange);
        DisplayPathDistances(path);
    }

    /// <summary>
    /// Highlights all tiles along a given path except the starting tile
    /// </summary>
    /// <param name="path"></param>
    private static void HighlightPath(TileGroup path, int unitMoveRange)
    {
        if (path == null)
            return;

        HighlightType hlType = HighlightType.validPath;

        //Show the path as invalid if it is longer than the moveRange or if the destination is occupied
        if (path.tiles.Length - 1 > unitMoveRange || Interact.Instance.CurrentTile.Occupied)
            hlType = HighlightType.invalidPath;

        for (int i = 0; i < path.tiles.Length; i++)
        {
            //Skip drawing the path highlight on the first tile (where the unit itself is)
            if (i == 0)
                continue;

            path.tiles[i].Highlighter.HighlightTile(hlType);
        }

        path.tiles[0].Highlighter.HighlightTile(HighlightType.unitSelection);
    }

    /// <summary>
    /// [Obsolete??] Clears Highlights of all tiles along a given path except the starting tile
    /// </summary>
    /// <param name="path"></param>
    public static void ClearPathHighlights(TileGroup path)
    {
        if (path == null)
            return;

        for (int i = 0; i < path.tiles.Length; i++)
        {
            //Skip clearing the path highlight on the first tile (where the unit itself is)
            if (i == 0)
                continue;

            path.tiles[i].Highlighter.ClearTileHighlight();
        }
    }

    /// <summary>
    /// Displays the distance of each tile along a path relative to the origin
    /// </summary>
    /// <param name="path"></param>
    public static void DisplayPathDistances(TileGroup path)
    {
        for (int tileDist = 0; tileDist < path.tiles.Length; tileDist++)
        {
            //Skip drawing the distance of the starting tile
            if (tileDist == 0)
                continue;

            path.tiles[tileDist].Highlighter.DisplayDistancesText(tileDist);
        }
    }

    /// <summary>
    /// Displays the distance of each tile within a move area from a unit
    /// </summary>
    /// <param name="path"></param>
    public static void DisplayMoveAreaDistances(List<Tile> tilesAtRange)
    {
        foreach (Tile tile in tilesAtRange)
        {
            if(tile.ThisTileType == TileType.Standard)
                tile.Highlighter.DisplayDistancesText(tile.RangeFromOrigin);
        }
    }

    //Debug only
    public static void DebugPathCosts(TileGroup path)
    {
        foreach (Tile tile in path.tiles)
        {
            if (tile.ThisTileType == TileType.Standard)
                tile.Highlighter.DebugCostText();
        }
    }
}
