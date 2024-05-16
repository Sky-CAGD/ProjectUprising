using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Holds data about weapon that can be equipped by units as a scriptable object
 */

[CreateAssetMenu(menuName = "ScriptableObjects/Weapon", fileName = "New Weapon", order = 2)]
public class Weapon : ScriptableObject
{
    public AttackType attackType = AttackType.shoot;
    public GameObject projectile;
    [Range(1, 99)] public int range = 6;
    [Range(1, 99)] public int baseDamage = 4;
}
