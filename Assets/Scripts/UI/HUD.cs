using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : SingletonPattern<HUD>
{
    [Header("Unit Move Range")]
    [SerializeField] private GameObject unitRangeHex;
    [SerializeField] private TMP_Text currRangeText;
    [SerializeField] private TMP_Text maxRangeText;

    private void Start()
    {
        unitRangeHex.SetActive(false);
    }

    public void ShowUnitMoveRange(int currRange, int maxRange)
    {
        unitRangeHex.SetActive(true);
        currRangeText.text = currRange.ToString();
        maxRangeText.text = maxRange.ToString();
    }

    public void HideUnitMoveRange()
    {
        unitRangeHex.SetActive(false);
    }
}
