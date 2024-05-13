using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class TrajectoryIllustrator : MonoBehaviour
{
    private ObjectPool objectPool;   
    private List<TrajectoryDot> dots = new List<TrajectoryDot>();
    private List<TrajectoryDot> activeDots = new List<TrajectoryDot>();

    [Header("Trajectory Modifiers")]
    [Range(1, 15)] public float arcHeight;
    public float spacing = 1.25f;
    public float speedMod = 1f;

    public Vector3 pointA { get; set; }
    public Vector3 pointB { get; set; }
    public Vector3 PointC 
    {
        get
        {
            Vector3 pointC = Vector3.Lerp(pointA, pointB, 0.5f);
            pointC += new Vector3(0, arcHeight * 2, 0);

            return pointC;
        }
    }

    private int numDots;
    private float interpSpeed;
    private float dist;
    private float maxDist = 40;
    private Vector3 offset;
    private float interpSpacing;
    private float interpolateAmt = 0;
    private float startingArcHeight;
    private bool drawingTrajectory;

    private void Start()
    {
        objectPool = GetComponent<ObjectPool>();

        //Add the pooled objects into the dots list
        foreach (var obj in objectPool.pooledObjects)
            dots.Add(obj.GetComponent<TrajectoryDot>());

        pointA = Vector3.zero;
        pointB = Vector3.zero;
        offset = new Vector3(0, 1, 0);
        startingArcHeight = arcHeight;
    }

    public void TempButtonPress(bool useArc)
    {
        if (drawingTrajectory)
        {
            HideTrajectory();
            return;
        }

        pointA = Interact.Instance.SelectedCharacter.transform.position + offset;
        pointB = FindObjectOfType<Enemy>().transform.position + offset;
        ShowTrajectory(pointA, pointB, useArc);
    }

    /// <summary>
    /// Start drawing a trajectory line with an arc between two points
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    public void ShowTrajectory(Vector3 pointA, Vector3 pointB, bool drawArc = false)
    {
        drawingTrajectory = true;

        StartCoroutine(DrawTrajectory());

        if (drawArc) //draw arced trajectory
            arcHeight = startingArcHeight;
        else //draw straight line trajectory
            arcHeight = 0;
    }   

    /// <summary>
    /// Stop drawing a trajectory line
    /// </summary>
    public void HideTrajectory()
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
    public IEnumerator DrawTrajectory()
    {
        Vector3 lastPointA = Vector3.zero;
        Vector3 lastPointB = Vector3.zero;

        SetTrajectoryPoints();
        CalculateTrajectoryInfo();

        //Move dots along trajectory
        while (drawingTrajectory)
        {
            //Ensure the user does not draw a trajectory with the same start and end points
            if(pointA == pointB)
                pointB = lastPointB;

            //If pointA or pointB have changed, re-initialize dots
            if (pointA != lastPointA || pointB != lastPointB)
            {
                CalculateTrajectoryInfo();
                InitializeDotsAlongTrajectory();
            }

            //Interpolate each dot along trajectory path
            for (int i = 0; i < numDots; i++)
            {
                interpolateAmt = dots[i].currLerpAmt;
                interpolateAmt = (interpolateAmt + (Time.deltaTime * interpSpeed * speedMod) / dist) % 1f;

                Vector3 pointAC = Vector3.Lerp(pointA, PointC, interpolateAmt);
                Vector3 pointCB = Vector3.Lerp(PointC, pointB, interpolateAmt);
                dots[i].transform.position = Vector3.Lerp(pointAC, pointCB, interpolateAmt);
                dots[i].currLerpAmt = interpolateAmt;
            }

            lastPointA = pointA;
            lastPointB = pointB;

            yield return null;

            SetTrajectoryPoints();
        }
    }

    private void SetTrajectoryPoints()
    {
        if(Interact.Instance.SelectedCharacter)
            pointA = Interact.Instance.SelectedCharacter.transform.position + offset;

        if (Interact.Instance.CurrentTile)
            pointB = Interact.Instance.CurrentTile.transform.position + offset;
        else
            pointB = FindObjectOfType<Enemy>().transform.position + offset;
    }

    private void CalculateTrajectoryInfo()
    {
        dist = Mathf.Clamp(Vector3.Distance(pointA, pointB), 0, maxDist);
        numDots = Mathf.Clamp((int)Mathf.Round(dist / spacing), 1, dots.Count);

        //Add extra dots to the trajectory if it is an Arc
        if (arcHeight > 1)
            numDots = Mathf.Clamp(numDots + 3, 1, dots.Count);

        interpSpeed = Mathf.Lerp(2, 3f, dist / maxDist);
        interpSpacing = (float)1 / numDots;
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

            Vector3 pointAC = Vector3.Lerp(pointA, PointC, interpolateAmt);
            Vector3 pointCB = Vector3.Lerp(PointC, pointB, interpolateAmt);
            dots[i].transform.position = Vector3.Lerp(pointAC, pointCB, interpolateAmt);

            interpolateAmt += interpSpacing;

            dots[i].currLerpAmt = interpolateAmt;
        }
    }

    /// <summary>
    /// Lerps the dots of a trajectory line along a straight line each frame
    /// </summary>
    /// <param name="pointA">The starting point to draw a trajectory from</param>
    /// <param name="pointB">The ending point to draw a trajectory to</param>
    /// <returns></returns>
    public IEnumerator DrawTrajectory(Vector3 pointA, Vector3 pointB)
    {
        float interpolateAmt = 0;
        numDots = Mathf.Clamp((int)Mathf.Round(dist / spacing), 1, dots.Count);

        float interpSpacing = (float) 1 / numDots;

        //Initialize dot visibility and positions
        for (int i = 0; i < numDots; i++)
        {
            dots[i].gameObject.SetActive(true);
            dots[i].transform.position = Vector3.Lerp(pointA, pointB, interpolateAmt);
            interpolateAmt += interpSpacing;

            dots[i].currLerpAmt = interpolateAmt;
        }

        //Move dots along trajectory
        while (drawingTrajectory)
        {
            for (int i = 0; i < numDots; i++)
            {
                interpolateAmt = dots[i].currLerpAmt;
                interpolateAmt = (interpolateAmt + (Time.deltaTime * interpSpeed * speedMod) /dist) % 1f;

                dots[i].transform.position = Vector3.Lerp(pointA, pointB, interpolateAmt);
                dots[i].currLerpAmt = interpolateAmt;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Lerps the dots of a trajectory line along an arced line each frame
    /// </summary>
    /// <param name="pointA">The starting point to draw a trajectory from</param>
    /// <param name="pointB">The ending point to draw a trajectory to</param>
    /// <returns></returns>
    public IEnumerator DrawTrajectory(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        float interpolateAmt = 0;
        numDots = Mathf.Clamp((int)Mathf.Round(dist / spacing) + 3, 1, dots.Count);

        float interpSpacing = (float)1 / numDots;

        //Initialize dot visibility and positions
        for (int i = 0; i < numDots; i++)
        {
            dots[i].gameObject.SetActive(true);
            Vector3 pointAC = Vector3.Lerp(pointA, pointC, interpolateAmt);
            Vector3 pointCB = Vector3.Lerp(pointC, pointB, interpolateAmt);
            dots[i].transform.position = Vector3.Lerp(pointAC, pointCB, interpolateAmt);
            interpolateAmt += interpSpacing;

            dots[i].currLerpAmt = interpolateAmt;
        }

        //Move dots along trajectory
        while (drawingTrajectory)
        {
            pointC = Vector3.Lerp(pointA, pointB, 0.5f) + new Vector3(0, arcHeight * 2, 0);

            for (int i = 0; i < numDots; i++)
            {
                interpolateAmt = dots[i].currLerpAmt;
                interpolateAmt = (interpolateAmt + (Time.deltaTime * interpSpeed * speedMod) / dist) % 1f;

                Vector3 pointAC = Vector3.Lerp(pointA, pointC, interpolateAmt);
                Vector3 pointCB = Vector3.Lerp(pointC, pointB, interpolateAmt);
                dots[i].transform.position = Vector3.Lerp(pointAC, pointCB, interpolateAmt);
                dots[i].currLerpAmt = interpolateAmt;
            }

            yield return null;
        }
    }
}