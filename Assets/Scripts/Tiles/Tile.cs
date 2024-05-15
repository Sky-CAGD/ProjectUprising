using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using System.Collections.Generic;
using System;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles information storage & setting tile types for a tile
 */

public enum TileType
{
    None,
    Standard,
    Wall
}

[RequireComponent(typeof(TileHighlighter))]
public class Tile : MonoBehaviour
{
    public Tile Parent { get; set; }
    public Tile ConnectedTile { get; set; }
    public List<Tile> Neighbors { get; set; }
    public Unit OccupyingUnit { get; set; }

    public TileHighlighter Highlighter { get; private set; }
    public TileType ThisTileType { get; private set; }
    public bool Walkable { get; private set; }
    public float CostFromOrigin { get; set; }
    public float CostToDestination { get; set; }
    public int TerrainCost { get; set; }
    public int RangeFromOrigin { get; set; }
    public float TotalCost { get { return CostFromOrigin + CostToDestination + TerrainCost; } }

    [field: SerializeField] public TMP_Text TileText { get; private set; }
    [field: SerializeField] public GameObject HexTileStandard { get; private set; }
    [field: SerializeField] public GameObject HexTileWall { get; private set; }
    [field: SerializeField] public GameObject HighlightMesh { get; private set; }

    public bool Occupied
    {
        get
        {
            if (OccupyingUnit == null)
                return false;
            else
                return true;
        }
    }

    private void Start()
    {
        Highlighter = GetComponent<TileHighlighter>();
        Neighbors = new List<Tile>();
        RandomizeTileType();
        SetTileParameters();
        RangeFromOrigin = int.MaxValue;
    }

    private void RandomizeTileType()
    {
        int randValue = Random.Range(0, 100);

        if (randValue < 75)
            ThisTileType = TileType.Standard;
        else if (randValue < 90)
            ThisTileType = TileType.Wall;
        else
            ThisTileType = TileType.None;

        if(Occupied)
            ThisTileType = TileType.Standard;
    }

    private void Update()
    {
        //update the highlight color of tiles each frame for testing
        //highlightColor = GameManager.Instance.tileHighlightColor;
        //mesh.material.color = highlightColor;
    }

    private void SetTileParameters()
    {
        switch (ThisTileType)
        {
            case TileType.None:
                {
                    HexTileStandard.SetActive(false);
                    HexTileWall.SetActive(false);
                    Walkable = false;
                }
                break;
            case TileType.Standard:
                {
                    HexTileStandard.SetActive(true);
                    HexTileWall.SetActive(false);
                    Walkable = true;
                }
                break;
            case TileType.Wall:
                {
                    HexTileStandard.SetActive(false);
                    HexTileWall.SetActive(true);
                    Walkable = false;
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
        int currTileIndex = (int)ThisTileType;

        currTileIndex += changeVal;
        if (currTileIndex > numTileTypes - 1)
            currTileIndex = 0;
        else if(currTileIndex < 0)
            currTileIndex = numTileTypes - 1;

        ThisTileType = (TileType)currTileIndex;

        SetTileParameters();
    }
}
