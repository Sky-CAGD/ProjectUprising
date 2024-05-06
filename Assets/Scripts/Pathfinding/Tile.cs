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

    [SerializeField] private TMP_Text tileText;
    [SerializeField] private GameObject hexTileStandard;
    [SerializeField] private GameObject hexTileWall;
    [SerializeField] private GameObject highlightMesh;
    private Color highlightColor;
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
        highlightColor = GameManager.Instance.tileHighlightColor;
        mesh.material.color = highlightColor;
        SetTileParameters();
    }

    private void Update()
    {
        //highlightColor = GameManager.Instance.tileHighlightColor;
        //mesh.material.color = highlightColor;
    }

    /// <summary>
    /// Changes color of the tile by activating child-objects of different colors
    /// </summary>
    /// <param name="col"></param>
    public void Highlight()
    {
        if (tileType == TileType.Standard)
            mesh.enabled = true;
    }

    public void ClearHighlight()
    {
        mesh.enabled = false;
    }

    private void SetTileParameters()
    {
        switch (tileType)
        {
            case TileType.None:
                {
                    hexTileStandard.SetActive(false);
                    hexTileWall.SetActive(false);
                    traversable = false;
                }
                break;
            case TileType.Standard:
                {
                    hexTileStandard.SetActive(true);
                    hexTileWall.SetActive(false);
                    traversable = true;
                }
                break;
            case TileType.Wall:
                {
                    hexTileStandard.SetActive(false);
                    hexTileWall.SetActive(true);
                    traversable = false;
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

    private void SetMaterial(Material mat)
    {
        GetComponent<MeshRenderer>().material = mat;
    }
    
    public void DebugCostText()
    {
        tileText.text = TotalCost.ToString("F1");
    }

    public void DisplayDistancesText(int tileDist)
    {
        tileText.text = tileDist.ToString();
    }

    public void ClearText()
    {
        tileText.text = "";
    }
}
