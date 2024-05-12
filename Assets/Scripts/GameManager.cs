using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonPattern<GameManager>
{
    /// <summary>
    /// Find all player units, reset their move range and deselect them
    /// </summary>
    public void NextTurn()
    {
        Character[] playerUnits = FindObjectsOfType<Character>();

        foreach (Character unit in playerUnits)
        {
            unit.RefreshUnit();
        }

        Interact.Instance.DeselectUnit();
    }
}