using System.IO;
using UnityEngine;

public class PathIllustrator : MonoBehaviour
{
    public void HighlightPath(Path path)
    {
        foreach (Tile tile in path.tiles)
        {
            tile.Highlight();
        }
    }

    public void ClearPathHighlights(Path path)
    {
        if (path == null)
            return;

        foreach (Tile tile in path.tiles)
        {
            tile.ClearHighlight();
            tile.ClearText();
        }
    }

    //Debug only
    public void DebugPathCosts(Path path)
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
    public void DisplayPathDistances(Path path)
    {
        int tileNum = 0;
        foreach (Tile tile in path.tiles)
        {
            if (tileNum != 0)
                tile.DisplayDistancesText(tileNum);

            tileNum++;
        }
    }
}
