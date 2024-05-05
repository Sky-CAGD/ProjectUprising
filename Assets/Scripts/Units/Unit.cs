using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool Moving { get; private set; } = false;

    public CharacterMoveData movedata;
    public Tile occupiedTile;

    [SerializeField] private LayerMask GroundLayerMask;

    private void Awake()
    {
        FindTileAtStart();
    }

    /// <summary>
    /// If no starting tile has been manually assigned, we find one beneath us
    /// </summary>
    void FindTileAtStart()
    {
        if (occupiedTile != null)
        {
            FinalizePosition(occupiedTile);
            return;
        }

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 50f, GroundLayerMask))
        {
            FinalizePosition(hit.transform.GetComponent<Tile>());
            return;
        }

        Debug.LogError(gameObject.name + " was unable to find a start position");
    }

    /// <summary>
    /// Start moving this unit along a designated path
    /// </summary>
    /// <param name="_path"></param>
    public void StartMove(Path _path)
    {
        Moving = true;
        occupiedTile.Occupied = false;
        StartCoroutine(MoveAlongPath(_path));
    }

    /// <summary>
    /// Moves this unit along a given path over time
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    IEnumerator MoveAlongPath(Path path)
    {
        const float MIN_DISTANCE = 0.05f;
        const float TERRAIN_PENALTY = 0.5f;

        int currentStep = 0;
        int pathLength = path.tiles.Length - 1;
        Tile currentTile = path.tiles[0];
        float animationTime = 0f;

        while (currentStep <= pathLength)
        {
            yield return null;

            //Move towards the next step in the path until we are closer than MIN_DIST
            Vector3 nextTilePosition = path.tiles[currentStep].transform.position;

            float movementTime = animationTime / (movedata.MoveSpeed + path.tiles[currentStep].terrainCost * TERRAIN_PENALTY);
            MoveAndRotate(currentTile.transform.position, nextTilePosition, movementTime);
            animationTime += Time.deltaTime;

            if (Vector3.Distance(transform.position, nextTilePosition) > MIN_DISTANCE)
                continue;

            //Min dist has been reached, look to next step in path
            currentTile = path.tiles[currentStep];
            currentStep++;
            animationTime = 0f;
        }

        FinalizePosition(path.tiles[pathLength]);
        Pathfinder.Instance.Illustrator.ClearPathHighlights(path);
    }

    /// <summary>
    /// End the movement along a path for this unit
    /// </summary>
    /// <param name="tile"></param>
    void FinalizePosition(Tile tile)
    {
        transform.position = tile.transform.position;
        occupiedTile = tile;
        Moving = false;
        tile.Occupied = true;
        tile.occupyingUnit = this;
    }

    void MoveAndRotate(Vector3 origin, Vector3 destination, float duration)
    {
        transform.position = Vector3.Lerp(origin, destination, duration);
        transform.rotation = Quaternion.LookRotation(origin.DirectionTo(destination).Flat(), Vector3.up);
    }
}
