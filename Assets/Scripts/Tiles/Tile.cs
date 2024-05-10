using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEditor;
using Unity.VisualScripting;

public enum TileType
{
    None,
    Standard,
    Wall
}

[RequireComponent(typeof(TileHighlighter))]
public class Tile : MonoBehaviour
{
    public Tile parent;
    public Tile connectedTile;
    public List<Tile> neighbors = new List<Tile>();
    public Unit occupyingUnit;

    public TileHighlighter Highlighter { get; private set; }

    public TileType tileType = TileType.Standard;
    public float costFromOrigin = 0;
    public float costToDestination = 0;
    public int terrainCost = 0;
    public int rangeFromOrigin = 0;
    public float TotalCost { get { return costFromOrigin + costToDestination + terrainCost; } }
    public bool walkable = true;

    [field: SerializeField] public TMP_Text tileText { get; private set; }
    [field: SerializeField] public GameObject hexTileStandard { get; private set; }
    [field: SerializeField] public GameObject hexTileWall { get; private set; }
    [field: SerializeField] public GameObject highlightMesh { get; private set; }

    public bool Occupied
    {
        get
        {
            if (occupyingUnit == null)
                return false;
            else
                return true;
        }
    }

    private void Start()
    {
        Highlighter = GetComponent<TileHighlighter>();
        SetTileParameters();
    }

    private void Update()
    {
        //update the highlight color of tiles each frame for testing
        //highlightColor = GameManager.Instance.tileHighlightColor;
        //mesh.material.color = highlightColor;
    }

    private void SetTileParameters()
    {
        switch (tileType)
        {
            case TileType.None:
                {
                    hexTileStandard.SetActive(false);
                    hexTileWall.SetActive(false);
                    walkable = false;
                }
                break;
            case TileType.Standard:
                {
                    hexTileStandard.SetActive(true);
                    hexTileWall.SetActive(false);
                    walkable = true;
                }
                break;
            case TileType.Wall:
                {
                    hexTileStandard.SetActive(false);
                    hexTileWall.SetActive(true);
                    walkable = false;
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// This is called when right clicking a tile to change its type
    /// </summary>
    /// <param name="value"></param>
    public void ChangeTile(int changeVal)
    {
        int numTileTypes = Enum.GetValues(typeof(TileType)).Length;
        int currTileIndex = (int)tileType;

        currTileIndex += changeVal;
        if (currTileIndex > numTileTypes - 1)
            currTileIndex = 0;
        else if(currTileIndex < 0)
            currTileIndex = numTileTypes - 1;

        tileType = (TileType)currTileIndex;

        SetTileParameters();
    }
}
