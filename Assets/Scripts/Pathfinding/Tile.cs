using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class Tile : MonoBehaviour
{
    public TileType tileType = TileType.Standard;
    public Tile parent;
    public Tile connectedTile;
    public Unit occupyingUnit;

    public float costFromOrigin = 0;
    public float costToDestination = 0;
    public int terrainCost = 0;
    public float TotalCost { get { return costFromOrigin + costToDestination + terrainCost; } }
    public bool Occupied { get; set; } = false;
    public bool traversable = true;

    [SerializeField] private TMP_Text costText;
    [SerializeField] private GameObject hexTileWall;

    private Material baseMaterial;
    private Material highlightMaterial;

    private void Start()
    {
        costText.text = "";
        highlightMaterial = TileManager.Instance.highlightMaterial;
        SetTileParameters();
    }

    /// <summary>
    /// Changes color of the tile by activating child-objects of different colors
    /// </summary>
    /// <param name="col"></param>
    public void Highlight()
    {
        SetMaterial(highlightMaterial);
    }

    public void ClearHighlight()
    {
        SetMaterial(baseMaterial);
    }

    private void SetTileParameters()
    {
        switch (tileType)
        {
            case TileType.None:
                {
                    GetComponent<Renderer>().enabled = false;
                    hexTileWall.SetActive(false);
                    traversable = false;
                    //baseMaterial = TileManager.Instance.noneMaterial;
                    //TileManager.Instance.SetTileAsNone(this);
                }
                break;
            case TileType.Standard:
                {
                    GetComponent<Renderer>().enabled = true;
                    hexTileWall.SetActive(false);
                    baseMaterial = TileManager.Instance.standardMaterial;
                    traversable = true;
                    //TileManager.Instance.SetTileAsStandard(this);
                }
                break;
            case TileType.Wall:
                {
                    GetComponent<Renderer>().enabled = false;
                    hexTileWall.SetActive(true);
                    baseMaterial = TileManager.Instance.wallMaterial;
                    traversable = false;
                    //TileManager.Instance.SetTileAsWall(this);
                }
                break;
            default:
                break;
        }

        SetMaterial(baseMaterial);
    }

    /// <summary>
    /// This is called when right clicking a tile to change its type
    /// </summary>
    /// <param name="value"></param>
    public void ChangeTile()
    {
        int numTileTypes = Enum.GetValues(typeof(TileType)).Length;
        int currTileIndex = (int)tileType;

        currTileIndex++;
        if (currTileIndex > numTileTypes - 1)
            currTileIndex = 0;

        tileType = (TileType)currTileIndex;

        SetTileParameters();
    }

    private void SetMaterial(Material mat)
    {
        GetComponent<MeshRenderer>().material = mat;
    }
    
    public void DebugCostText()
    {
        costText.text = TotalCost.ToString("F1");
    }

    public void ClearText()
    {
        costText.text = "";
    }
}
