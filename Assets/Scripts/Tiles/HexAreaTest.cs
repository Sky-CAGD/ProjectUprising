using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HexAreaTest : MonoBehaviour
{

    [SerializeField] private LayerMask tileLayer;

    private Camera mainCam;
    private Tile currentTile;
    private List<Tile> selectedTiles = new List<Tile>();

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Clear();
        MouseUpdate();
    }

    private void Clear()
    {
        if (selectedTiles.Count == 0)
            return;
        else if (selectedTiles.Count == 1)
        {
            currentTile.ClearPathHighlight();
            selectedTiles.Remove(currentTile);
        }
        else if (selectedTiles.Count > 1)
            ClearAllSelections();

        currentTile = null;
    }

    private void MouseUpdate()
    {
        //Check if mouse hit object on an interactable layer (tiles, units, etc)
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue, tileLayer))
        {
            //If a tile was moused over, set as the current tile
            if (hit.transform.GetComponent<Tile>())
            {
                currentTile = hit.transform.GetComponent<Tile>();
                InspectTile();
            }        
        }
    }

    private void InspectTile()
    {
        if (currentTile != null)
        {
            currentTile.HighlightPath();
            selectedTiles.Add(currentTile);

            List<Tile> newTileList = Pathfinder.Instance.NeighborTiles(currentTile);

            foreach (Tile tile in newTileList)
            {
                selectedTiles.Add(tile);
                tile.HighlightPath();
            }
        }
    }

    /// <summary>
    /// Deselect/Clear all units and paths unless a selected unit is moving
    /// </summary>
    private void ClearAllSelections()
    {
        foreach (Tile tile in selectedTiles)
            tile.ClearPathHighlight();

        selectedTiles.Clear();
    }
}
