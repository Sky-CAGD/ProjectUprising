using System.Collections;
using UnityEngine;

public class Character : Unit
{
    public void StartPlanningAttack()
    {
        CurrState = UnitState.planningAttack;
        ShowAttackRange();
    }

    public void StopPlanningAttack() 
    {
        CurrState = UnitState.idle;
        HideAttackRange();
    }

    public override void RefreshUnit()
    {
        CurrMoveRange = MaxMoveRange;
        CurrActionPoints = MaxActionPoints;
    }
}