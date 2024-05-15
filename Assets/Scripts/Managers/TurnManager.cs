using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles switching between player/enemy turns
 */

public class TurnManager : MonoBehaviour
{
    private Character[] characterUnits;
    private Enemy[] enemyUnits;

    private void Start()
    {
        characterUnits = FindObjectsOfType<Character>();
        enemyUnits = FindObjectsOfType<Enemy>();
    }

    /// <summary>
    /// Find all player units, reset their move range and deselect them
    /// </summary>
    public void NextTurn()
    {
        foreach (Character unit in characterUnits)
            unit.RefreshUnit();

        foreach (Enemy unit in enemyUnits)
            unit.RefreshUnit();

        Interact.Instance.DeselectUnit();
    }
}
