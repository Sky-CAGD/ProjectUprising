using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles behavior of player controlled character units
 */

public class Character : Unit
{
    [Header("Action Points")]
    [SerializeField] private Color actionPointColor = Color.red;
    [SerializeField] private Color bonusActionPointColor = Color.yellow;
    [SerializeField] private Color usedActionPointColor = Color.black;
    [SerializeField] private GameObject actionPointPanel;
    [SerializeField] private GameObject actionPointPrefab;
    private List<Image> actionPoints = new List<Image>();
    protected override void Start()
    {
        base.Start();
        RefreshUnit();
    }

    /// <summary>
    /// Called when this character is Selected
    /// </summary>
    public void CharacterSelected()
    {
        StartPlanningMovement();
    }

    /// <summary>
    /// Called when this character is Deselected
    /// </summary>
    public void CharacterDeselected()
    {
        if (CurrState == UnitState.planningMovement || CurrState == UnitState.planningAttack)
            StopPlanning();
    }

    /// <summary>
    /// Starts the planning state of movement and highlights related tiles
    /// </summary>
    public void StartPlanningMovement()
    {
        ClearTilesInRange();
        CurrState = UnitState.planningMovement;
        ShowMovementRange();
    }

    /// <summary>
    /// Starts the planning state of attacking and highlights related tiles
    /// </summary>
    public void StartPlanningAttack()
    {
        ClearTilesInRange();
        occupiedTile.Highlighter.ClearAllTileHighlights();
        EventManager.OnCharacterStartedPlanningAttack(this);
        CurrState = UnitState.planningAttack;
        ShowAttackRange();
    }

    /// <summary>
    /// Ends the planning state of moving or attacking and un-highlights related tiles
    /// </summary>
    public void StopPlanning()
    {
        if(CurrState == UnitState.planningAttack)
            EventManager.OnCharacterEndedPlanningAttack();

        CurrState = UnitState.idle;
        ClearTilesInRange();
    }

    /// <summary>
    /// Starts character movement along a path, fires OnCharacterMoved event
    /// </summary>
    /// <param name="_path">The path that this unit will move along</param>
    public override void StartMove(TileGroup _path)
    {
        base.StartMove(_path);

        EventManager.OnCharacterMoved(this);
    }

    public override void StartAttack(Tile target)
    {
        StopPlanning();

        base.StartAttack(target);
        SetUpActionPointsPanel();
        DisplayRemainingActionPoints();
    }

    public override void EndAttack()
    {
        base.EndAttack();

        if (CurrActionPoints > 0)
            StartPlanningAttack();
        else if (CurrMoveRange > 0)
            StartPlanningMovement();
        else
            CurrState = UnitState.idle;
    }

    /// <summary>
    /// Moves this unit along a given path over time
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    protected override IEnumerator MoveAlongPath(TileGroup path)
    {
        StartCoroutine(base.MoveAlongPath(path));

        occupiedTile.Highlighter.ClearAllTileHighlights();
        ShowMovementRange();
        occupiedTile.Highlighter.HighlightTile(HighlightType.unitSelection);

        yield return null;
    }

    /// <summary>
    /// End the movement along a path for this unit
    /// </summary>
    /// <param name="tile"></param>
    protected override void FinalizePosition(Tile tile)
    {
        base.FinalizePosition(tile);

        if (CurrMoveRange > 0)
            CurrState = UnitState.planningMovement;
    }

    public override void RefreshUnit()
    {
        base.RefreshUnit();
        SetUpActionPointsPanel();
        DisplayRemainingActionPoints();
    }

    //--------------------------------------------
    // Action Points
    //--------------------------------------------
    protected void SetUpActionPointsPanel()
    {
        if (actionPoints.Count == CurrActionPoints)
            return;

        //Action points were reduced - remove and clear them to regenerate the correct #
        if (actionPoints.Count > CurrActionPoints)
        {
            foreach (Image AP in actionPoints)
                Destroy(AP.gameObject);

            actionPoints.Clear();
        }

        //Keep adding action points until the correct number have been added
        int numAP = Mathf.Max(CurrActionPoints, MaxActionPoints);
        while (actionPoints.Count < numAP)
        {
            GameObject newAP = Instantiate(actionPointPrefab, actionPointPanel.transform);
            actionPoints.Add(newAP.GetComponent<Image>());
        }
    }

    /// <summary>
    /// For each action point the player has, sets the UI to diplay as:
    /// Yellow: Bonus AP - These are temp AP that are greater than the Unit's maximum
    /// Red: Unused AP - These are AP within the maximum that have not been used yet this turn
    /// Black: Used AP - These are AP that have already been used during this turn
    /// </summary>
    protected void DisplayRemainingActionPoints()
    {
        for (int i = 0; i < actionPoints.Count; i++)
        {
            if (i >= MaxActionPoints)
                actionPoints[i].color = bonusActionPointColor;
            else if (CurrActionPoints > i)
                actionPoints[i].color = actionPointColor;
            else
                actionPoints[i].color = usedActionPointColor;
        }
    }
    protected override void AddActionPoints(int actionPoints)
    {
        base.AddActionPoints(actionPoints);
        SetUpActionPointsPanel();
        DisplayRemainingActionPoints();
    }
}