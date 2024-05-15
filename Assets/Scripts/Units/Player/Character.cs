/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles behavior of player controlled character units
 */

public class Character : Unit
{
    protected override void Start()
    {
        base.Start();
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
}