using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles mouse interactions with units and hex grid
 */

public class Interact : SingletonPattern<Interact>
{
    [SerializeField] private LayerMask interactMask;

    //Debug purposes only
    [SerializeField] private bool debug;

    private Camera mainCam;
    private Transform lastThingHit;

    public Tile CurrentTile { get; private set; }
    public Character SelectedCharacter { get; private set; }
    public bool NewInteraction { get; private set; }

    private TileGroup lastPath = new TileGroup();

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        CheckMouseOverInteraction();
   
        //Check for right click input to deselect units & highlights
        if (Input.GetMouseButtonDown(1))
            ClearAllSelections();
    }

    private void CheckMouseOverInteraction()
    {
        //Check if mouse hit UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            NothingMousedOver();
            return;
        }

        //Check if mouse hit object on an interactable layer (tiles, units, etc)
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, float.MaxValue, interactMask))
        {
            ObjectMousedOver(hit.transform);
            CheckToStartAttacking();
        }
        else //The mouse is over a non-interactable object
        {
            NothingMousedOver();
        }
    }

    /// <summary>
    /// Checks for when the user clicks to attack a target tile
    /// </summary>
    private void CheckToStartAttacking()
    {
        //Exit if any of the following conditions are true:
        if (!Attacking.Instance.ValidAttackTrajectory())
            return;

        if (SelectedCharacter.CurrState != UnitState.planningAttack) 
            return;

        //User has clicked to initiate an attack
        if (Input.GetMouseButtonDown(0))
        {
            SelectedCharacter.StartAttack(CurrentTile);
        }
    }

    private void ObjectMousedOver(Transform objectHit)
    {
        //Check if the same object was hit as the last frame - inspect it but do not clear and draw highlights
        if (objectHit == lastThingHit)
        {
            NewInteraction = false;

            if (lastThingHit.GetComponent<Tile>())
                InspectTile();
            else if (lastThingHit.GetComponent<Character>())
                InspectCharacterUnit();
            else if (lastThingHit.GetComponent<Enemy>())
                InspectEnemyUnit();

            return;
        }
        //If a new object was hit (mouse moved over new thing)
        //Clear all highlights & text
        Clear();

        NewInteraction = true;

        //If a tile was moused over, set as the current tile and inspect the tile
        if (objectHit.GetComponent<Tile>())
        {
            CurrentTile = objectHit.GetComponent<Tile>();
            InspectTile();
        }
        //If a character unit was moused over, set the current tile as its occupied tile and inspect the unit
        else if (objectHit.GetComponent<Character>())
        {
            CurrentTile = objectHit.GetComponent<Character>().occupiedTile;
            InspectCharacterUnit();
        }
        else if (objectHit.GetComponent<Enemy>())
        {
            CurrentTile = objectHit.GetComponent<Enemy>().occupiedTile;
            InspectEnemyUnit();
        }

        SelectedUnitTilesToHighlight();

        lastThingHit = objectHit;
    }


    /// <summary>
    /// Handles what happens when the user mouses over non-interactable things in the scene
    /// </summary>
    private void NothingMousedOver()
    {
        //Check if this is the first frame which the player has moused off interactable objects
        if (lastThingHit != null)
        {
            Clear();
            SelectedUnitTilesToHighlight();
        }

        //Check for left click input in open area to deselect units/paths
        //[DISABLED b/c clicking attack UI buttons deselects the unit - need fix for]
        if (Input.GetMouseButtonDown(0))
        {
            //ClearAllSelections();
        }

        lastThingHit = null;
    }

    private void SelectedUnitTilesToHighlight()
    {
        if (SelectedCharacter == null)
            return;

        //Check if selected unit is planning to move - highlight unit and move area
        if (SelectedCharacter.CurrState == UnitState.planningMovement)
        {
            SelectedCharacter.ShowMovementRange();

            if(CurrentTile != null)
                NavigateToTile();
        }
        //Check if selected unit is planning an attack - show information related to this
        else if(SelectedCharacter.CurrState == UnitState.planningAttack)
        {
            SelectedCharacter.ShowAttackRange();

            if (CurrentTile != null)
                DisplayAttackToTile();
        }

        //Highlight this unit's tile
        SelectedCharacter.occupiedTile.Highlighter.HighlightTile(HighlightType.unitSelection);
    }

    /// <summary>
    /// Deselect/Clear all units and paths unless a selected unit is moving/attacking (not idle)
    /// </summary>
    private void ClearAllSelections()
    {
        if (SelectedCharacter)
        {
            if (SelectedCharacter.CurrState == UnitState.moving || SelectedCharacter.CurrState == UnitState.attacking)
                return;
        }

        FindObjectOfType<TileHighlighter>().ClearAllTileHighlights();
        DeselectUnit();
    }

    /// <summary>
    /// Handles what happens when the user mouses over tiles in the scene
    /// </summary>
    private void InspectTile()
    {
        //If a tile with a unit is hovered over, inspect the unit
        if (CurrentTile.Occupied && CurrentTile.OccupyingUnit.GetComponent<Character>())
            InspectCharacterUnit();
        else if (CurrentTile.Occupied && CurrentTile.OccupyingUnit.GetComponent<Enemy>())
            InspectEnemyUnit();

        //Check if a selected unit is planning to move (a path is being drawn)
        if(SelectedCharacter != null && SelectedCharacter.CurrState == UnitState.planningMovement)
        {
            //User clicks to move unit along drawn path
            if (Input.GetMouseButtonDown(0) && lastPath != null)
            {
                //Check if unit has enough movement to complete this path
                if (Pathfinder.ValidPath(lastPath, SelectedCharacter.CurrMoveRange))
                {
                    //Start move along path
                    SelectedCharacter.StartMove(lastPath);
                }
                else
                {
                    //Unit does not have enough move range to complete the path or destination is occupied
                }
            }
        }

        //Check if moused over unoccupied tile and no unit is selected - allow altering tile
        if (!CurrentTile.Occupied && SelectedCharacter == null)
            CheckToAlterTileType();
    }

    /// <summary>
    /// Allow user to alter tile type by left/right clicking on open tiles
    /// </summary>
    private void CheckToAlterTileType()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CurrentTile.ChangeTile(1);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            CurrentTile.ChangeTile(-1);
        }
    }

    /// <summary>
    /// Handles what happens when the user mouses over a character unit
    /// Shows movement range of character units, allows for selecting/deselecting them
    /// </summary>
    private void InspectCharacterUnit()
    {
        //Exit if current tile is not occupied or occupied unit is not idle
        if (!CurrentTile.Occupied || CurrentTile.OccupyingUnit.CurrState != UnitState.idle)
            return;

        //Check if no character unit is selected and this is a new interaction
        if (!SelectedCharacter && NewInteraction)
        {
            Character newCharacter = CurrentTile.OccupyingUnit.GetComponent<Character>();
            //Highlight this character unit's tile and show its move area
            newCharacter.ShowMovementRange();
            CurrentTile.Highlighter.HighlightTile(HighlightType.unitSelection);
            HUD.Instance.ShowCharacterInfo(newCharacter);
        }

        //Mouse clicked on unit
        if (Input.GetMouseButtonDown(0))
        {
            //No character unit selected - select it
            if(!SelectedCharacter)
            {
                SelectUnit();
            }
            //Character unit is selected
            //Check if selected character is planning an attack (don't deselect or switch selections)
            else if (SelectedCharacter.CurrState != UnitState.planningAttack)
            {
                //Selected unit not planning an attack - deselect it
                DeselectUnit();

                //Check if the character unit interacted with is a different unit - select it
                if (CurrentTile.OccupyingUnit != SelectedCharacter)
                    SelectUnit();
            }
        }
    }

    /// <summary>
    /// Handles what happens when the user mouses over an enemy unit
    /// </summary>
    private void InspectEnemyUnit()
    {
        //Exit if current tile is not occupied or occupied unit is not idle
        if (!CurrentTile.Occupied || CurrentTile.OccupyingUnit.CurrState != UnitState.idle)
            return;
    }

    /// <summary>
    /// Clears highlighted tiles - Called when mousing over a new thing
    /// </summary>
    private void Clear()
    {
        //Do not clear highlighted tiles if:
        //There was no last thing or tile hit
        if (lastThingHit == null || CurrentTile == null)
            return;
        //Or if a selected unit is moving or attacking
        if (SelectedCharacter)
        {
            if (SelectedCharacter.CurrState == UnitState.moving || SelectedCharacter.CurrState == UnitState.attacking)
                return;
        }
        else //If no character is selected, hide Character UI elements
            HUD.Instance.HideCharacterInfo();

        CurrentTile.Highlighter.ClearAllTileHighlights();
        CurrentTile = null;
    }

    /// <summary>
    /// Sets a unit to be selected and sets the camera to follow it
    /// </summary> 
    public void SelectUnit()
    {
        //Set the newly selected character
        SelectedCharacter = CurrentTile.OccupyingUnit.GetComponent<Character>();
        SelectedCharacter.CharacterSelected();

        EventManager.OnCharacterSelected(SelectedCharacter);
    }

    /// <summary>
    /// Deselect current selected unit and stops the camera from following it
    /// </summary>
    public void DeselectUnit()
    {
        if (SelectedCharacter == null)
            return;

        SelectedCharacter.CharacterDeselected();
        EventManager.OnCharacterDeselected();

        SelectedCharacter = null;
    }

    private void DisplayAttackToTile()
    {
        if (CurrentTile == null) return;

        CurrentTile.Highlighter.HighlightTile(HighlightType.attackTarget);
    }

    /// <summary>
    /// If a unit is selected and a tile hovered over - draw a path
    /// </summary>
    private void NavigateToTile()
    {
        //Exit path navigation if a selected character unit is not planning to move
        if (SelectedCharacter != null && SelectedCharacter.CurrState != UnitState.planningMovement)
            return;

        //Get and draw a path to the current tile
        if (RetrievePath(out TileGroup newPath, SelectedCharacter.CurrMoveRange))
        {
            lastPath = newPath;
        }
        else
            lastPath = null;
    }

    /// <summary>
    /// Gets a path from a selected unit to the current tile moused over and draws it
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Returns true if a new valid path was found and drawn</returns>
    private bool RetrievePath(out TileGroup path, int unitMoveRange)
    {
        path = Pathfinder.FindPath(SelectedCharacter.occupiedTile, CurrentTile);
        
        if (path == null)
            return false;

        PathIllustrator.DrawPath(path, unitMoveRange);

        return true;
    }
}