using System.Collections;
using UnityEngine;

public class Character : Unit
{
    public bool planningAttack;

    public void StartPlanningAttack()
    {
        planningAttack = true;
    }

    public void StopPlanningAttack() 
    {
        planningAttack = false;
    }

    public override void RefreshUnit()
    {
        CurrMoveRange = MaxMoveRange;
        CurrActionPoints = MaxActionPoints;
    }

    /// <summary>
    /// Gets all tiles within this unit's attack range and highlights them
    /// </summary>
    public override void ShowAttackRange()
    {
        base.ShowAttackRange();

        planningAttack = true;
    }

    /// <summary>
    /// Hides the highlights and text for all tiles within the last generated attack range
    /// </summary>
    public override void HideAttackRange()
    {
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.ClearTileHighlight();

        planningAttack = false;
    }
}