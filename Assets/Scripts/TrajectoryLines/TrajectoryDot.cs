using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryDot : MonoBehaviour
{
    [SerializeField] private Color validTrajectoryColor;
    [SerializeField] private Color invalidTrajectoryColor;
    private SpriteRenderer spRenderer;

    public float currLerpAmt { get; set; }

    private void Awake()
    {
        spRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetValidTrajectoryColor()
    {
        spRenderer.color = validTrajectoryColor;
    }

    public void SetInvalidTrajectoryColor()
    {
        spRenderer.color = invalidTrajectoryColor;
    }
}
