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

    private void OnEnable()
    {
        EventManager.EnemyPhaseEnded += StartPlayerTurn;
    }

    private void OnDisable()
    {
        EventManager.EnemyPhaseEnded -= StartPlayerTurn;
    }

    private void Start()
    {
        characterUnits = FindObjectsOfType<Character>();
        enemyUnits = FindObjectsOfType<Enemy>();
    }

    /// <summary>
    /// Called when player clicks next/end turn button, sends event to start enemies moving/attacking
    /// </summary>
    public void StartEnemyTurn()
    {
        Interact.Instance.DeselectUnit();
        Interact.Instance.CanInteract = false;
        EventManager.OnEnemyPhaseStarted();
    }

    /// <summary>
    /// Called when all enemies have finished moving & attacking
    /// </summary>
    private void StartPlayerTurn()
    {
        RefreshAllUnits();
        Interact.Instance.CanInteract = true;
    }

    /// <summary>
    /// Find all player units, reset their move range and deselect them
    /// </summary>
    private void RefreshAllUnits()
    {
        foreach (Character unit in characterUnits)
            unit.RefreshUnit();

        foreach (Enemy unit in enemyUnits)
            unit.RefreshUnit();
    }
}
