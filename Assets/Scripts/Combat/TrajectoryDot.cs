using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: A single dot to be instantiated and moved along a trajectory line
 * Stores its position along the trajectory and allows setting of its color
 */

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
