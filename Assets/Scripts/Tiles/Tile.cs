using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public enum TileType
{
    None,
    Standard,
    Wall
}

public class Tile : MonoBehaviour
{
    public Tile parent;
    public Tile connectedTile;
    public List<Tile> neighbors = new List<Tile>();
    public Unit occupyingUnit;

    public TileType tileType = TileType.Standard;
    public float costFromOrigin = 0;
    public float costToDestination = 0;
    public int terrainCost = 0;
    public int rangeFromOrigin = 0;
    public float TotalCost { get { return costFromOrigin + costToDestination + terrainCost; } }
    public bool Occupied { get; set; } = false;
    public bool walkable = true;
    public bool moveAreaHighlighted;

    [SerializeField] private TMP_Text tileText;
    [SerializeField] private GameObject hexTileStandard;
    [SerializeField] private GameObject hexTileWall;
    [SerializeField] private GameObject highlightMesh;
    private Color pathHighlightColor;
    private Color moveAreaColor;
    private Color unitHighlightColor;
    private Renderer mesh;

    private void Awake()
    {
        tileText.text = "";
        highlightMesh.gameObject.SetActive(true);
        mesh = highlightMesh.GetComponent<Renderer>();
        mesh.enabled = false;
    }

    private void Start()
    {
        pathHighlightColor = GameManager.Instance.tilePathHighlightColor;
        moveAreaColor = GameManager.Instance.tileMoveAreaColor;
        unitHighlightColor = GameManager.Instance.tileUnitHighlightColor;
        mesh.material.color = pathHighlightColor;
        SetTileParameters();
    }

    private void Update()
    {
        //update the highlight color of tiles each frame for testing
        //highlightColor = GameManager.Instance.tileHighlightColor;
        //mesh.material.color = highlightColor;
    }

    /// <summary>
    /// Enables highlight mesh and sets color to path highlight color
    /// </summary>
    /// <param name="col"></param>
    public void HighlightPath()
    {
        if (tileType == TileType.Standard)
        {
            mesh.enabled = true;
            mesh.material.color = pathHighlightColor;
        }
    }

    /// <summary>
    /// Enables highlight mesh and sets color to move area highlight color
    /// </summary>
    /// <param name="col"></param>
    public void HighlightMoveArea()
    {
        if (tileType == TileType.Standard)
        {
            mesh.enabled = true;
            mesh.material.color = moveAreaColor;
            moveAreaHighlighted = true;
        }
    }

    /// <summary>
    /// Enables highlight mesh and sets color to unit highlight color
    /// </summary>
    /// <param name="col"></param>
    public void HighlightUnit()
    {
        if (tileType == TileType.Standard)
        {
            mesh.enabled = true;
            mesh.material.color = unitHighlightColor;
        }
    }

    public void ClearPathHighlight()
    {
        if(moveAreaHighlighted)
        {
            mesh.material.color = moveAreaColor;
            DisplayText(rangeFromOrigin.ToString());
        }
        else
        {
            mesh.enabled = false;
            ClearText();
        }
    }

    public void ClearMoveAreaHighlight()
    {
        if (tileType == TileType.Standard)
        {
            mesh.enabled = false;
            moveAreaHighlighted = false;
        }
    }

    public void ClearUnitHighlight()
    {
        if (tileType == TileType.Standard)
        {
            mesh.enabled = false;
        }
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
    
    public void DebugCostText()
    {
        tileText.text = TotalCost.ToString("F1");
    }

    public void DisplayDistancesText(int tileDist)
    {
        tileText.text = tileDist.ToString();
    }

    public void DisplayText(string text)
    {
        tileText.text = text;
    }

    public void ClearText()
    {
        tileText.text = "";
    }
}
