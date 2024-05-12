using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class Interact : SingletonPattern<Interact>
{
    //[SerializeField] private AudioClip click, pop;
    [SerializeField] private LayerMask interactMask;

    //Debug purposes only
    [SerializeField] private bool debug;

    private Camera mainCam;
    private Tile currentTile;
    private Unit lastUnitViewed;
    private Pathfinder pathfinder;
    private GameObject lastThingHit;

    [field: SerializeField] public Character selectedCharacter { get; private set; }

    private void Start()
    {
        mainCam = Camera.main;
        pathfinder = Pathfinder.Instance;
    }

    private void Update()
    {
        BaseInteractions();
   
        //Check for right click input to deselect units & highlights
        if (Input.GetMouseButtonDown(1))
            ClearAllSelections();
    }

    private void BaseInteractions()
    {
        //Check if mouse hit object on an interactable layer (tiles, units, etc)
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue, interactMask))
        {
            //Check if a new object was hit (mouse moved over new thing)
            if (hit.transform.gameObject != lastThingHit)
            {
                //Clear all highlights & text
                Clear();

                //Check if a unit is selected
                if (selectedCharacter != null)
                {
                    //Check if selected unit is planning an attack
                    if (selectedCharacter != null && selectedCharacter.planningAttack)
                    {
                        selectedCharacter.ShowAttackRange();
                    }
                    //Check if selected unit is not moving or attacking - highlight unit and move area
                    else if(!selectedCharacter.Moving)
                    {
                        selectedCharacter.ShowMovementRange();
                    }
                }

                //If a tile was moused over, set as the current tile and inspect the tile
                if (hit.transform.GetComponent<Tile>())
                {
                    currentTile = hit.transform.GetComponent<Tile>();
                    InspectTile();
                }
                //If a unit was moused over, set the current tile as its occupied tile and inspect the unit
                else if (hit.transform.GetComponent<Character>())
                {
                    currentTile = hit.transform.GetComponent<Character>().occupiedTile;
                    InspectUnit();
                }

                lastThingHit = hit.transform.gameObject;
            }
            else //The same object was hit as the last frame - inspect it but do not clear and draw highlights
            {
                if (lastThingHit.GetComponent<Tile>())
                    InspectTile();
                else if (lastThingHit.GetComponent<Character>())
                    InspectUnit();
            }
        }
        else //The mouse is over a non-interactable object
        {
            //Skip changing interactions if a selected character unit is attacking
            if (selectedCharacter != null && selectedCharacter.planningAttack)
                return;

            Clear();

            //Check for left click input in open area to deselect units/paths
            if (Input.GetMouseButtonDown(0))
            {
                //ClearAllSelections();
            }

            //Show the movement range of any selected character units
            if(selectedCharacter != null)
                selectedCharacter.ShowMovementRange();

            lastThingHit = null;
        }
    }

    /// <summary>
    /// Deselect/Clear all units and paths unless a selected unit is moving
    /// </summary>
    private void ClearAllSelections()
    {
        if (selectedCharacter && selectedCharacter.Moving)
            return;

        FindObjectOfType<TileHighlighter>().ClearAllTileHighlights();
        DeselectUnit();
    }

    private void InspectTile()
    {
        //If a tile with a unit is hovered over, highlight the unit
        if (currentTile.Occupied)
            InspectUnit();
        //Check if the hovered tile is not occupied and a unit is selected
        else if (selectedCharacter != null)
        {
            //Check if character unit is planning to attack or move
            if (selectedCharacter.planningAttack)
                DisplayAttackToTile(); //Show attack
            else
                NavigateToTile(); //Generate a path
        }


        //Alter tile type by left/right clicking on open tile while no unit is selected
        if (!currentTile.Occupied && selectedCharacter == null)
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

        //Exit if tile is occupied by an enemy
        if (currentTile.occupyingUnit.GetComponent<Enemy>())
            return;

        //Check if no character unit is selected and this unit was not the last one viewed
        if (selectedCharacter == null && (lastUnitViewed == null || lastThingHit != lastUnitViewed.gameObject))
        {
            //Set this character as the last unit viewed - used to clear move area highlights later
            lastUnitViewed = currentTile.occupyingUnit;

            //Highlight this character unit's tile and show its move area
            lastUnitViewed.ShowMovementRange();
        }

        //Mouse clicked on unit
        if (Input.GetMouseButtonDown(0))
        {
            //No character unit selected - select it
            if(selectedCharacter == null)
            {
                SelectUnit();
            }
            //Character unit is selected - deselect it (and potentially select a new one)
            else
            {
                DeselectUnit();

                //Check if the character unit interacted with is a different unit - select it
                if (currentTile.occupyingUnit != selectedCharacter)
                {
                    SelectUnit();
                    selectedCharacter.ShowMovementRange();
                }
            }
        }
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
        if (selectedCharacter && selectedCharacter.Moving)
            return;

        currentTile.Highlighter.ClearAllTileHighlights();
        currentTile = null;

        //Hide the unit movement range on the HUD if no unit is selected
        if (!selectedCharacter)
            HUD.Instance.HideUnitMoveRange();
    }

    /// <summary>
    /// Sets a unit to be selected and sets the camera to follow it
    /// </summary> 
    public void SelectUnit()
    {
        selectedCharacter = currentTile.occupyingUnit.GetComponent<Character>();
        selectedCharacter.UnitSelected();
        CameraController.Instance.followTarget = selectedCharacter.transform;
        HUD.Instance.ShowUnitAttackPanel();
    }

    /// <summary>
    /// Deselect current selected unit and stops the camera from following it
    /// </summary>
    public void DeselectUnit()
    {
        if (selectedCharacter == null)
            return;

        selectedCharacter.UnitDeselected();
        selectedCharacter = null;
        CameraController.Instance.followTarget = null;
        HUD.Instance.HideUnitAttackPanel();
    }

    private void DisplayAttackToTile()
    {
        currentTile.Highlighter.HighlightTile(HighlightType.attackTarget);
    }

    /// <summary>
    /// If a unit is selected and a tile hovered over - draw a path
    /// </summary>
    private void NavigateToTile()
    {
        //Exit path navigation if no unit is selected or if a selected unit is currently moving
        if (selectedCharacter == null || selectedCharacter.Moving == true)
            return;

        //Get and draw a path to the current tile
        if (RetrievePath(out TileGroup newPath, selectedCharacter.CurrMoveRange))
        {
            //User clicks to move unit along drawn path
            if (Input.GetMouseButtonDown(0))
            {
                //Check if unit has enough movement to complete this path
                if(selectedCharacter.CurrMoveRange >= newPath.tiles.Length - 1)
                {
                    //Start move along path
                    selectedCharacter.StartMove(newPath);
                }
                else
                {
                    //Unit does not have enough move range to complete the path
                }
            }
        }
    }

    /// <summary>
    /// Gets a path from a selected unit to the current tile moused over and draws it
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Returns true if a new valid path was found and drawn</returns>
    bool RetrievePath(out TileGroup path, int unitMoveRange)
    {
        path = pathfinder.FindPath(selectedCharacter.occupiedTile, currentTile);
        
        if (path == null)
            return false;

        pathfinder.Illustrator.DrawPath(path, unitMoveRange);

        return true;
    }
}