using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles interactions/events relating to the HUD UI elements
 */

public class HUD : MonoBehaviour
{
    [Header("Unit Move Range")]
    [SerializeField] private GameObject unitMoveRangeHex;
    [SerializeField] private TMP_Text currMoveRangeText;
    [SerializeField] private TMP_Text maxMoveRangeText;

    [Header("Unit Attacking")]
    [SerializeField] private GameObject characterAttackPanel;
    [SerializeField] private TMP_Text characterWeaponTypeText;

    private Character selectedCharacter;

    private void OnEnable()
    {
        EventManager.CharacterSelected += CharacterSelected;
        EventManager.CharacterDeselected += CharacterDeselected;
        EventManager.CharacterMoved += UpdateCharacterInfo;
    }

    private void OnDisable()
    {
        EventManager.CharacterSelected -= CharacterSelected;
        EventManager.CharacterDeselected -= CharacterDeselected;
        EventManager.CharacterMoved -= UpdateCharacterInfo;
    }

    private void Awake()
    {
        unitMoveRangeHex.SetActive(false);
        characterAttackPanel.SetActive(false);
    }

    /// <summary>
    /// Shows UI relevant to a newly selected character unit
    /// </summary>
    /// <param name="character">The character that was selected</param>
    private void CharacterSelected(Character character)
    {
        selectedCharacter = character;
        unitMoveRangeHex.SetActive(true);
        UpdateCharacterInfo(character);

        //Attack UI
        characterAttackPanel.SetActive(true);
    }

    /// <summary>
    /// Hides UI relevant to selected character units
    /// </summary>
    private void CharacterDeselected()
    {
        selectedCharacter = null;

        //Movement UI
        unitMoveRangeHex.SetActive(false);

        //Attack UI
        characterAttackPanel.SetActive(false);
    }

    /// <summary>
    /// Updates UI relevant to selected character units
    /// </summary>
    /// <param name="character"></param>
    private void UpdateCharacterInfo(Unit character)
    {
        currMoveRangeText.text = character.CurrMoveRange.ToString();
        maxMoveRangeText.text = character.MaxMoveRange.ToString();
        characterWeaponTypeText.text = character.weapon.attackType.ToString();
    }

    /// <summary>
    /// Called when a UI element event is triggered to start planning a character unit's attack
    /// </summary>
    public void CharacterAttackInitiated()
    {
        if (selectedCharacter == null)
            return;

        selectedCharacter.StartPlanningAttack();
        characterAttackPanel.SetActive(false);
    }
}
