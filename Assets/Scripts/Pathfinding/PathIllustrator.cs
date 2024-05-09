using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathIllustrator : MonoBehaviour
{
    public void HighlightPath(TileGroup path)
    {
        foreach (Tile tile in path.tiles)
        {
            tile.HighlightPath();
        }
    }

    public void ClearPathHighlights(TileGroup path)
    {
        if (path == null)
            return;

        foreach (Tile tile in path.tiles)
        {
            tile.ClearPathHighlight();
        }
    }

    //Debug only
    public void DebugPathCosts(TileGroup path)
    {
        foreach (Tile tile in path.tiles)
        {
            tile.DebugCostText();
        }
    }

    /// <summary>
    /// Displays the distance of each tile along a path relative to the origin
    /// </summary>
    /// <param name="path"></param>
    public void DisplayPathDistances(TileGroup path)
    {
        int tileNum = 0;
        foreach (Tile tile in path.tiles)
        {
            if (tileNum != 0)
                tile.DisplayDistancesText(tileNum);

            tileNum++;
        }
    }

    /// <summary>
    /// Displays the distance of each tile within a move area from a unit
    /// </summary>
    /// <param name="path"></param>
    public void DisplayMoveAreaDistances(List<Tile> tilesAtRange)
    {
        foreach (Tile tile in tilesAtRange)
        {
            tile.DisplayDistancesText(tile.rangeFromOrigin);
        }
    }
}
