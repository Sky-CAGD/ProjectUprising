using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Unit Data", fileName = "New Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Health")]
    public int maxShield;
    public int startingShield;
    public int maxHealth;
    public int startingHealth;

    [Header("Movement")]
    public int maxMove = 8;
    public float moveSpeed = 0.5f;

    [Header("Attacking")]
    public int maxActionPoints;
    public int startingActionPoints;
}
