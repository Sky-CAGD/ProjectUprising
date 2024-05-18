using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/18/2024
 * Description: Handles performing each enemy's turn sequentially
 */

public class EnemyManager : SingletonPattern<EnemyManager>
{
    private Queue<Enemy> enemies = new Queue<Enemy>();

    private void OnEnable()
    {
        EventManager.EnemyPhaseStarted += InitializeEnemyPhase;
        EventManager.EnemyTurnEnded += StartNextEnemyTurn;
    }

    private void OnDisable()
    {
        EventManager.EnemyPhaseStarted -= InitializeEnemyPhase;
        EventManager.EnemyTurnEnded -= StartNextEnemyTurn;
    }

    /// <summary>
    /// Finds all enemies in the scene, puts them in a Queue, and starts iterating through the Queue
    /// </summary>
    private void InitializeEnemyPhase()
    {
        Enemy[] enemiesArray = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemiesArray)
            enemies.Enqueue(enemy);

        StartNextEnemyTurn();
    }

    /// <summary>
    /// Checks for more enemies in the queue and starts the next one's turn if there are any
    /// </summary>
    private void StartNextEnemyTurn()
    {
        if (enemies.Count <= 0) //No more enemies to act, enemy phase is over
            EndEnemyPhase();
        else //More enemies, have the next one take its turn
        {
            enemies.Peek().StartTurn();
            enemies.Dequeue();
        }
    }

    /// <summary>
    /// All enemies have acted, fire an event to switch back to the player's turn
    /// </summary>
    private void EndEnemyPhase()
    {
        EventManager.OnEnemyPhaseEnded();
    }
}
