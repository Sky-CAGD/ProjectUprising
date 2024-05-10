using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class Interact : MonoBehaviour
{
    //[SerializeField] private AudioClip click, pop;
    [SerializeField] private LayerMask interactMask;

    //Debug purposes only
    [SerializeField] private bool debug;
    private TileGroup lastPath;

    private Camera mainCam;
    private Tile currentTile;
    private Unit selectedUnit;
    private Unit lastUnitViewed;
    private List<Tile> unitMoveArea;
    private Pathfinder pathfinder;
    private PathIllustrator illustrator;
    private GameObject lastThingHit;

    private void Start()
    {
        mainCam = Camera.main;
        pathfinder = Pathfinder.Instance;
        illustrator = pathfinder.Illustrator;
    }

    private void Update()
    {
        MouseUpdate();
    }

    private void MouseUpdate()
    {
        //Check if mouse hit object on an interactable layer (tiles, units, etc)
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue, interactMask))
        {
            //Check if a new object was hit (mouse moved over new thing)
            if (hit.transform.gameObject != lastThingHit)
            {
                //Clear all highlights & text
                Clear();

                //if a unit is selected - highlight unit and move area
                if (selectedUnit)
                    HighlightUnitArea(selectedUnit);

                //If a tile was moused over, set as the current tile and inspect the tile
                if (hit.transform.GetComponent<Tile>())
                {
                    currentTile = hit.transform.GetComponent<Tile>();
                    InspectTile();
                }
                //If a unit was moused over, set the current tile as its occupied tile and inspect the unit
                else if (hit.transform.GetComponent<Unit>())
                {
                    currentTile = hit.transform.GetComponent<Unit>().occupiedTile;
                    InspectUnit();
                }

                lastThingHit = hit.transform.gameObject;
            }
            else //The same object was hit as the last frame - inspect it but do not clear and draw highlights
            {
                if (lastThingHit.GetComponent<Tile>())
                    InspectTile();
                else if (lastThingHit.GetComponent<Unit>())
                    InspectUnit();
            }
        }
        else //The mouse is over a non-interactable object
        {
            //Check for left click input in open area to deselect units/paths
            if (Input.GetMouseButtonDown(0))
            {
                ClearAllSelections();
            }
        }

        //Check for right click input to deselect units/paths
        if (Input.GetMouseButtonDown(1))
        {
            ClearAllSelections();
        }
    }

    /// <summary>
    /// Deselect/Clear all units and paths unless a selected unit is moving
    /// </summary>
    private void ClearAllSelections()
    {
        if (selectedUnit && selectedUnit.Moving)
            return;

        FindObjectOfType<TileHighlighter>().ClearAllTileHighlights();
        DeselectUnit();
    }

    private void InspectTile()
    {
        //If a tile with a unit is hovered over, highlight the unit
        if (currentTile.Occupied)
            InspectUnit();
        //If a unit is selected and the hovered tile is not occupied, generate a path
        else if (selectedUnit != null)
            NavigateToTile();

        //Alter tile type by left/right clicking on open tile while no unit is selected
        if (!currentTile.Occupied && selectedUnit == null)
        {
            if(Input.GetMouseButtonDown(0))
            {
                currentTile.ChangeTile(1);
            }
            else if(Input.GetMouseButtonDown(1))
            {
                currentTile.ChangeTile(-1);
            }
        }
    }

    private void InspectUnit()
    {
        //Exit if current tile is not occupied or occupied unit is moving
        if (!currentTile.Occupied || currentTile.occupyingUnit.Moving)
            return;

        //Check if no unit is selected and this unit was not the last one viewed
        if (selectedUnit == null && (lastUnitViewed == null || lastThingHit != lastUnitViewed.gameObject))
        {
            //Set this as the last unit viewed - used to clear move area highlights later
            lastUnitViewed = currentTile.occupyingUnit;

            //Highlight this unit's tile and show its move area
            HighlightUnitArea(lastUnitViewed);
        }

        //Mouse clicked on unit
        if (Input.GetMouseButtonDown(0))
        {
            //No unit selected - select it
            if(selectedUnit == null)
            {
                SelectUnit();
            }
            //Unit is selected - deselect it (and potentially select a new one)
            else
            {
                DeselectUnit();

                //if the unit interacted with is a different unit - select it
                if (currentTile.occupyingUnit != selectedUnit)
                {
                    SelectUnit();
                    HighlightUnitArea(selectedUnit);
                }
            }
        }
    }

    /// <summary>
    /// Highlight this unit's tile and show its move area
    /// </summary>
    private void HighlightUnitArea(Unit unit)
    {
        //Highlight this unit's tile
        unit.occupiedTile.Highlighter.HighlightTile(HighlightType.unitSelection);

        //Highlight all tiles with this unit's range
        unit.ShowMovementRange();
    }
    
    /// <summary>
    /// Clears highlighted tiles - Called when mousing over a new thing
    /// </summary>
    private void Clear()
    {
        //Do not clear highlighted tiles if:
        //There was no last thing or tile hit
        if (lastThingHit == null || currentTile == null)
            return;
        //Or if a selected unit is moving
        if (selectedUnit && selectedUnit.Moving)
            return;

        currentTile.Highlighter.ClearAllTileHighlights();
        currentTile = null;
    }

    /// <summary>
    /// Sets a unit to be selected and sets the camera to follow it
    /// </summary> 
    public void SelectUnit()
    {
        selectedUnit = currentTile.occupyingUnit;
        selectedUnit.UnitSelected(); //Does nothing (yet)
        CameraController.Instance.followTarget = selectedUnit.transform;
        //GetComponent<AudioSource>().PlayOneShot(pop);
    }

    /// <summary>
    /// Deselect current selected unit and stops the camera from following it
    /// </summary>
    public void DeselectUnit()
    {
        if (selectedUnit == null)
            return;

        selectedUnit.UnitDeselected(); //Does nothing (yet)
        selectedUnit = null;
        CameraController.Instance.followTarget = null;
        //GetComponent<AudioSource>().PlayOneShot(pop);
    }

    /// <summary>
    /// If a unit is selected and a tile hovered over - draw a path
    /// </summary>
    private void NavigateToTile()
    {
        //Exit path navigation if no unit is selected or if a selected unit is currently moving
        if (selectedUnit == null || selectedUnit.Moving == true)
            return;

        //Get and draw a path to the current tile
        if (RetrievePath(out TileGroup newPath))
        {
            if (Input.GetMouseButtonDown(0))
            {
                //GetComponent<AudioSource>().PlayOneShot(click);
                selectedUnit.StartMove(newPath);
            }
        }
    }

    /// <summary>
    /// Gets a path from a selected unit to the current tile moused over and draws it
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Returns true if a new valid path was found and drawn</returns>
    bool RetrievePath(out TileGroup path)
    {
        path = pathfinder.FindPath(selectedUnit.occupiedTile, currentTile);
        
        if (path == null)
            return false;

        pathfinder.Illustrator.DrawPath(path);

        lastPath = path;
        return true;
    }
}