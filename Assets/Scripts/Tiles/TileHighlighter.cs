using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles highlighting and setting text on a tile
 */

[RequireComponent(typeof(Tile))]
public class TileHighlighter : MonoBehaviour
{
    public HighlightType CurrHighlight { get; private set;}

    private Color moveAreaColor;
    private Color validPathColor;
    private Color invalidPathColor;
    private Color unitSelectionColor;
    private Color attackAreaColor;
    private Color attackTargetColor;

    private static List<Tile> highlightedTiles = new List<Tile>();

    private Tile tile;
    private Renderer mesh;

    private void Start()
    {
        //Get tile to highlight and its highlight mesh
        tile = GetComponent<Tile>();
        mesh = tile.highlightMesh.GetComponent<Renderer>();

        //Set up tile text and highlight mesh to default values
        tile.tileText.text = "";
        tile.highlightMesh.gameObject.SetActive(true);
        mesh.enabled = false;
        CurrHighlight = HighlightType.none;

        //Set local references to all highlight color options
        moveAreaColor = HighlightColors.Instance.MoveAreaColor;
        validPathColor = HighlightColors.Instance.ValidPathColor;
        invalidPathColor = HighlightColors.Instance.InvalidPathColor;
        unitSelectionColor = HighlightColors.Instance.UnitSelectionColor;
        attackAreaColor = HighlightColors.Instance.AttackAreaColor;
        attackTargetColor = HighlightColors.Instance.AttackTargetColor;
    }

    //--------------------------------------------
    // Setting/Clearing Tile Highlight
    //--------------------------------------------

    /// <summary>
    /// Sets a highlight color based on the passed in parameter and enables the highlight mesh
    /// </summary>
    /// <param name="hlType">The type of highlight to be applied to this tile</param>
    public void HighlightTile(HighlightType hlType)
    {
        //If this tile is not a highlightable type, exit w/o highlighting it
        if (tile.tileType != TileType.Standard)
            return;

        //Set the color of the highlight based on the provided type
        switch (hlType)
        {
            case HighlightType.none:
                return;
            case HighlightType.validPath:
                {
                    mesh.material.color = validPathColor;
                    break;
                }
            case HighlightType.invalidPath:
                {
                    mesh.material.color = invalidPathColor;
                    break;
                }
            case HighlightType.moveArea:
                {
                    mesh.material.color = moveAreaColor;
                    break;
                }
            case HighlightType.unitSelection:
                {
                    mesh.material.color = unitSelectionColor;
                    break;
                }
            case HighlightType.attackArea:
                {
                    mesh.material.color = attackAreaColor;
                    break;
                }
            case HighlightType.attackTarget:
                {
                    mesh.material.color = attackTargetColor;
                    break;
                }
            default:
                {
                    Debug.LogWarning("Highlight Type not recognized, no highlight was applied");
                    return;
                }
        }

        //Enable the highlight mesh and set the curr highlight type
        mesh.enabled = true;
        CurrHighlight = hlType;

        //If this tile was not already highlighted, add it to the highlighted tiles list
        if(!highlightedTiles.Contains(tile))
            highlightedTiles.Add(tile);
    }

    public void ClearTileHighlight()
    {
        //If this tile is not a highlightable type, exit w/o clearing highlights (should not be highlighted)
        if (tile.tileType != TileType.Standard || CurrHighlight == HighlightType.none)
            return;

        //Clear any highlights and text
        mesh.enabled = false;
        CurrHighlight = HighlightType.none;
        ClearText();

        //Remove this tile from the highlighted tiles list
        if (highlightedTiles.Contains(tile))
            highlightedTiles.Remove(tile);
    }

    /// <summary>
    /// Removes highlights and text from all tiles
    /// </summary>
    public void ClearAllTileHighlights()
    {
        foreach (Tile tile in highlightedTiles)
        {
            tile.Highlighter.mesh.enabled = false;
            tile.Highlighter.CurrHighlight = HighlightType.none;
            tile.Highlighter.ClearText();
        }

        highlightedTiles.Clear();
    }

    //--------------------------------------------
    // Setting/Clearing Tile Text
    //--------------------------------------------

    public void DebugCostText()
    {
        tile.tileText.text = tile.TotalCost.ToString("F1");
    }

    public void DisplayDistancesText(int tileDist)
    {
        tile.tileText.text = tileDist.ToString();
    }

    public void DisplayText(string text)
    {
        tile.tileText.text = text;
    }

    public void ClearText()
    {
        tile.tileText.text = "";
    }
}
