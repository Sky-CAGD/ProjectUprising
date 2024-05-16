using UnityEngine;
using UnityEngine.Events;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: All game events are declared and referenced here
 */

public static class EventManager
{
    public static event UnityAction<Character> CharacterSelected;
    public static event UnityAction CharacterDeselected;
    public static event UnityAction<Character> CharacterMoved;
    public static event UnityAction<Character> CharacterStartedPlanningAttack;
    public static event UnityAction CharacterEndedPlanningAttack;
    public static event UnityAction<Unit, Tile> UnitStartedAttacking;

    public static void OnCharacterSelected(Character character) => CharacterSelected?.Invoke(character);
    public static void OnCharacterDeselected() => CharacterDeselected?.Invoke();
    public static void OnCharacterMoved(Character character) => CharacterMoved?.Invoke(character);
    public static void OnCharacterStartedPlanningAttack(Character character) => CharacterStartedPlanningAttack?.Invoke(character);
    public static void OnCharacterEndedPlanningAttack() => CharacterEndedPlanningAttack?.Invoke();
    public static void OnUnitStartedAttacking(Unit unit, Tile target) => UnitStartedAttacking?.Invoke(unit, target);
}
