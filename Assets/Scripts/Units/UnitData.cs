using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Unit Data", fileName = "New Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Health")]
    public int MaxShield;
    public int Shield;
    public int MaxHealth;
    public int Health;

    [Header("Movement")]
    public int MaxMove = 8;
    public float MoveSpeed = 0.5f;
}
