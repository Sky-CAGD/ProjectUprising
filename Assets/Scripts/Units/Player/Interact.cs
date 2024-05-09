using System.Collections.Generic;
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
                Clear();
 
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

        ClearPath(lastPath);
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
        //Exit if unit is moving
        if (currentTile.occupyingUnit.Moving)
            return;

        //Check if this unit was not the last one viewed
        if (lastUnitViewed == null || lastThingHit != lastUnitViewed.gameObject)
        {
            //Set this as the last unit viewed - used to clear move area highlights later
            lastUnitViewed = currentTile.occupyingUnit;

            //Highlight this unit's tile
            currentTile.HighlightUnit();

            //Highlight all tiles with this unit's range
            currentTile.occupyingUnit.ShowMovementRange();
        }

        //Mouse clicked on unit
        if (Input.GetMouseButtonDown(0))
        {
            //No unit selected - select it
            if(selectedUnit == null)
            {
                SelectUnit();
            }
            //Unit is selected - deselect it
            else
            {
                DeselectUnit();
            }
        }
    }

    private void Clear()
    {
        if (lastThingHit == null || currentTile == null  || currentTile.Occupied == false)
            return;

        ClearPath(lastPath);

        //if no unit is selected - clear its highlight(s)
        if (!selectedUnit)
        {
            currentTile.ClearUnitHighlight();

            //if the unit is not the same as the one viewed last frame - hide its movement area
            if(lastUnitViewed != null)
                lastUnitViewed.HideMovementRange();
        }

        currentTile = null;
        lastUnitViewed = null;
    }

    public void SelectUnit()
    {
        selectedUnit = currentTile.occupyingUnit;
        selectedUnit.UnitSelected();
        CameraController.Instance.followTarget = selectedUnit.transform;
        //GetComponent<AudioSource>().PlayOneShot(pop);
    }

    public void DeselectUnit()
    {
        if (selectedUnit == null)
            return;

        selectedUnit.UnitDeselected();
        selectedUnit = null;
        CameraController.Instance.followTarget = null;
        //GetComponent<AudioSource>().PlayOneShot(pop);
    }

    /// <summary>
    /// If a unit is selected and a tile hovered over - draw a path
    /// </summary>
    private void NavigateToTile()
    {
        if (selectedUnit == null || selectedUnit.Moving == true)
            return;

        if (RetrievePath(out TileGroup newPath))
        {
            if (Input.GetMouseButtonDown(0))
            {
                //GetComponent<AudioSource>().PlayOneShot(click);
                selectedUnit.StartMove(newPath);
            }
        }
    }

    bool RetrievePath(out TileGroup path)
    {
        path = pathfinder.FindPath(selectedUnit.occupiedTile, currentTile);
        
        if (path == null || path == lastPath)
            return false;

        ClearPath(lastPath);
        DrawPath(path);

        lastPath = path;
        return true;
    }

    private void DrawPath(TileGroup path)
    {
        illustrator.HighlightPath(path);

        if (debug) //Debug only
            illustrator.DebugPathCosts(path);
        else
            illustrator.DisplayPathDistances(path);
    }

    private void ClearPath(TileGroup path)
    {
        if (path != null)
        {
            illustrator.ClearPathHighlights(path);
        }
    }
}