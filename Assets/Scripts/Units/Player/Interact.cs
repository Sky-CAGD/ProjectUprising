using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

//[RequireComponent(typeof(AudioSource))]
public class Interact : SingletonPattern<Interact>
{
    [SerializeField] private LayerMask interactMask;

    //Debug purposes only
    [SerializeField] private bool debug;

    private Camera mainCam;
    private Pathfinder pathfinder;
    private Transform lastThingHit;

    public Tile CurrentTile { get; private set; }
    public Character SelectedCharacter { get; private set; }
    public bool NewInteraction { get; private set; }

    private void Start()
    {
        mainCam = Camera.main;
        pathfinder = Pathfinder.Instance;
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
        }
        else //The mouse is over a non-interactable object
        {
            NothingMousedOver();
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

        SelectedUnitTilesToHighlight();

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

        lastThingHit = objectHit;
    }


    /// <summary>
    /// This function defines what happens when the user mouses over non-interactable things in the scene
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

        //Check if selected unit is planning an attack - show information related to this
        if (SelectedCharacter.CurrState == UnitState.planningAttack)
        {
            SelectedCharacter.ShowAttackRange();
        }
        //Check if selected unit is idle - highlight unit and move area
        else if (SelectedCharacter.CurrState == UnitState.idle)
        {
            SelectedCharacter.ShowMovementRange();
        }
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

    private void InspectTile()
    {
        //If a tile with a character unit is hovered over, inspect the unit
        if (CurrentTile.Occupied && CurrentTile.occupyingUnit.GetComponent<Character>())
            InspectCharacterUnit();
        //If a tile with an enemy unit is hovered over, inspect the unit
        else if (CurrentTile.Occupied && CurrentTile.occupyingUnit.GetComponent<Enemy>())
            InspectEnemyUnit();
        //Check if the hovered tile is not occupied and a unit is selected
        else if (SelectedCharacter != null)
        {
            //Check if character unit is planning to attack or move
            if (SelectedCharacter.CurrState == UnitState.planningAttack)
                DisplayAttackToTile(); //Show attack
            else if (SelectedCharacter.CurrState == UnitState.idle)
                NavigateToTile(); //Generate a path
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
    /// Handles what happens when the mouse interacts with a character unit
    /// Shows movement range of character units, allows for selecting/deselecting them
    /// </summary>
    private void InspectCharacterUnit()
    {
        //Exit if current tile is not occupied or occupied unit is not idle
        if (!CurrentTile.Occupied || CurrentTile.occupyingUnit.CurrState != UnitState.idle)
            return;

        //Check if no character unit is selected and this is a new interaction
        if (SelectedCharacter == null && NewInteraction)
        {
            //Highlight this character unit's tile and show its move area
            CurrentTile.occupyingUnit.ShowMovementRange();
        }
        //Check if a selected character unit is planning to attack
        else if (SelectedCharacter && SelectedCharacter.CurrState == UnitState.planningAttack)
            DisplayAttackToTile(); //Show attack

        //Mouse clicked on unit
        if (Input.GetMouseButtonDown(0))
        {
            //No character unit selected - select it
            if(SelectedCharacter == null)
            {
                SelectUnit();
            }
            //Character unit is selected - deselect it (and potentially select a new one)
            else
            {
                DeselectUnit();

                //Check if the character unit interacted with is a different unit - select it
                if (CurrentTile.occupyingUnit != SelectedCharacter)
                {
                    SelectUnit();
                    SelectedCharacter.ShowMovementRange();
                }
            }
        }
    }

    private void InspectEnemyUnit()
    {
        //Exit if current tile is not occupied or occupied unit is not idle
        if (!CurrentTile.Occupied || CurrentTile.occupyingUnit.CurrState != UnitState.idle)
            return;

        //Check if a selected character unit is planning to attack
        if (SelectedCharacter && SelectedCharacter.CurrState == UnitState.planningAttack)
            DisplayAttackToTile(); //Show attack
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

        CurrentTile.Highlighter.ClearAllTileHighlights();
        CurrentTile = null;

        //Hide the unit movement range on the HUD if no unit is selected
        if (!SelectedCharacter)
            HUD.Instance.HideUnitMoveRange();
    }

    /// <summary>
    /// Sets a unit to be selected and sets the camera to follow it
    /// </summary> 
    public void SelectUnit()
    {
        SelectedCharacter = CurrentTile.occupyingUnit.GetComponent<Character>();
        SelectedCharacter.UnitSelected();
        CameraController.Instance.followTarget = SelectedCharacter.transform;
        HUD.Instance.ShowUnitAttackPanel();
    }

    /// <summary>
    /// Deselect current selected unit and stops the camera from following it
    /// </summary>
    public void DeselectUnit()
    {
        if (SelectedCharacter == null)
            return;

        SelectedCharacter.UnitDeselected();
        SelectedCharacter = null;
        CameraController.Instance.followTarget = null;
        HUD.Instance.HideUnitAttackPanel();
    }

    private void DisplayAttackToTile()
    {
        CurrentTile.Highlighter.HighlightTile(HighlightType.attackTarget);
    }

    /// <summary>
    /// If a unit is selected and a tile hovered over - draw a path
    /// </summary>
    private void NavigateToTile()
    {
        //Exit path navigation if no unit is selected or if a selected unit is not idle
        if (SelectedCharacter != null && SelectedCharacter.CurrState != UnitState.idle)
            return;

        //Get and draw a path to the current tile
        if (RetrievePath(out TileGroup newPath, SelectedCharacter.CurrMoveRange))
        {
            //User clicks to move unit along drawn path
            if (Input.GetMouseButtonDown(0))
            {
                //Check if unit has enough movement to complete this path
                if(SelectedCharacter.CurrMoveRange >= newPath.tiles.Length - 1)
                {
                    //Start move along path
                    SelectedCharacter.StartMove(newPath);
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
        path = pathfinder.FindPath(SelectedCharacter.occupiedTile, CurrentTile);
        
        if (path == null)
            return false;

        pathfinder.Illustrator.DrawPath(path, unitMoveRange);

        return true;
    }
}