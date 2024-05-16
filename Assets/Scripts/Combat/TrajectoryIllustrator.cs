using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles drawing a trajectory line from given points A to B, with potential to arc towards a pointC
 */

[RequireComponent(typeof(ObjectPool))]
public class TrajectoryIllustrator : MonoBehaviour
{
    private ObjectPool objectPool;   
    private List<TrajectoryDot> dots = new List<TrajectoryDot>();
    private List<TrajectoryDot> activeDots = new List<TrajectoryDot>();

    [Header("Trajectory Modifiers")]
    [SerializeField][Range(1, 15)] private float arcHeight;
    [SerializeField] private float spacing = 1.25f;
    [SerializeField] private float speedMod = 1f;

    public Vector3 PointA { get; private set; } //Start point (Unit)
    public Vector3 PointB { get; private set; } //End point (Target)
    public Vector3 PointC //Middle point between A & B with added height for drawing arcs
    {
        get
        {
            Vector3 pointC = Vector3.Lerp(PointA, PointB, 0.5f);
            pointC += new Vector3(0, arcHeight * 2, 0);

            return pointC;
        }
    }

    public bool CanFire { get; private set; }

    private int numDots;
    private float interpSpeed;
    private float dist;
    private float maxDist = 40;
    private Vector3 offset;
    private float interpSpacing;
    private float interpolateAmt = 0;
    private float startingArcHeight;
    private bool drawingTrajectory;

    private void OnEnable()
    {
        EventManager.CharacterStartedPlanningAttack += StartDrawingTrajectory;
        EventManager.CharacterEndedPlanningAttack += HideTrajectory;
    }

    private void OnDisable()
    {
        EventManager.CharacterStartedPlanningAttack -= StartDrawingTrajectory;
        EventManager.CharacterEndedPlanningAttack -= HideTrajectory;
    }

    private void Start()
    {
        objectPool = GetComponent<ObjectPool>();

        //Add the pooled objects into the dots list
        foreach (var obj in objectPool.pooledObjects)
            dots.Add(obj.GetComponent<TrajectoryDot>());

        PointA = Vector3.zero;
        PointB = Vector3.zero;
        offset = new Vector3(0, 1, 0);
        startingArcHeight = arcHeight;
    }

    /// <summary>
    /// Start drawing a trajectory line with an arc between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    private void StartDrawingTrajectory(Character character)
    {
        if (drawingTrajectory)
            return;

        drawingTrajectory = true;

        PointA = character.occupiedTile.transform.position + offset;

        //draw arced trajectory
        if (character.weapon.attackType == AttackType.artillery)
            arcHeight = startingArcHeight;
        //draw straight line trajectory
        else
            arcHeight = 0;

        StartCoroutine(DrawTrajectory());
    }

    /// <summary>
    /// Stop drawing a trajectory line
    /// </summary>
    private void HideTrajectory()
    {
        drawingTrajectory = false;

        foreach (var dot in dots)
            dot.gameObject.SetActive(false);
    }

    /// <summary>
    /// Lerps the dots of a trajectory line along a straight line each frame
    /// </summary>
    /// <param name="pointA">The starting point to draw a trajectory from</param>
    /// <param name="pointB">The ending point to draw a trajectory to</param>
    /// <returns></returns>
    private IEnumerator DrawTrajectory()
    {
        Vector3 lastPointA = Vector3.zero;
        Vector3 lastPointB = Vector3.zero;

        SetPointB();

        //Do not start drawing a trajectory until the user mouses over a tile
        while(PointB == Vector3.zero)
        {
            yield return null;
            SetPointB();
        }

        //Move dots along trajectory
        while (drawingTrajectory)
        {
            //Ensure the user does not draw a trajectory with the same start and end points
            if(PointA == PointB)
                PointB = lastPointB;

            //If pointA or pointB have changed, re-initialize dots
            if (PointA != lastPointA || PointB != lastPointB)
            {
                CalculateTrajectoryInfo();
                InitializeDotsAlongTrajectory();
            }

            //Interpolate each dot along trajectory path
            for (int i = 0; i < numDots; i++)
            {
                interpolateAmt = dots[i].currLerpAmt;
                interpolateAmt = (interpolateAmt + (Time.deltaTime * interpSpeed * speedMod) / dist) % 1f;

                Vector3 pointAC = Vector3.Lerp(PointA, PointC, interpolateAmt);
                Vector3 pointCB = Vector3.Lerp(PointC, PointB, interpolateAmt);
                dots[i].transform.position = Vector3.Lerp(pointAC, pointCB, interpolateAmt);
                dots[i].currLerpAmt = interpolateAmt;
            }

            lastPointA = PointA;
            lastPointB = PointB;

            yield return null;

            SetPointB();
        }
    }

    private void SetPointB()
    {
        if (Interact.Instance.CurrentTile)
            PointB = Interact.Instance.CurrentTile.transform.position + offset;
    }

    private void CalculateTrajectoryInfo()
    {
        dist = Mathf.Clamp(Vector3.Distance(PointA, PointB), 0, maxDist);
        numDots = Mathf.Clamp((int)Mathf.Round(dist / spacing), 1, dots.Count);

        //Add extra dots and adjust the interpSpeed if the trajectory is an Arc
        if (arcHeight > 1)
        {
            numDots = Mathf.Clamp(numDots + 3, 1, dots.Count);
            interpSpeed = Mathf.Lerp(1, 3f, dist / maxDist);
        }
        else
            interpSpeed = Mathf.Lerp(2, 3f, dist / maxDist);

        interpSpacing = (float)1 / numDots;

        CanFire = Attacking.Instance.ValidAttackTrajectory();
    }

    private void InitializeDotsAlongTrajectory()
    {
        //Disable all active dots
        foreach (var dot in activeDots)
            dot.gameObject.SetActive(false);

        activeDots.Clear();

        interpolateAmt = 0;

        //Initialize dot visibility and positions
        for (int i = 0; i < numDots; i++)
        {
            dots[i].gameObject.SetActive(true);
            activeDots.Add(dots[i]);

            Vector3 pointAC = Vector3.Lerp(PointA, PointC, interpolateAmt);
            Vector3 pointCB = Vector3.Lerp(PointC, PointB, interpolateAmt);
            dots[i].transform.position = Vector3.Lerp(pointAC, pointCB, interpolateAmt);

            interpolateAmt += interpSpacing;
            dots[i].currLerpAmt = interpolateAmt;

            if (CanFire)
                dots[i].SetValidTrajectoryColor();
            else
                dots[i].SetInvalidTrajectoryColor();
        }
    }
}
