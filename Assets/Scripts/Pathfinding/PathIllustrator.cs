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

    //Debug only
    public void ClearPathCosts(Path path)
    {
        if (path == null)
            return;

        foreach (Tile tile in path.tiles)
        {
            tile.ClearText();
        }
    }
}
