using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    shoot,
    laser,
    artillery,
    blast,
    melee
}

[CreateAssetMenu(menuName = "ScriptableObjects/Weapon", fileName = "New Weapon", order = 2)]
public class Weapon : ScriptableObject
{
    public AttackType attackType = AttackType.shoot;
    [Range(1, 99)] public int range = 6;
    [Range(1, 99)] public int baseDamage = 4;
}
