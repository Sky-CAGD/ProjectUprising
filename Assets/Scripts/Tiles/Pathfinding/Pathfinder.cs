using System.Collections.Generic;
using UnityEngine;

/*
 * Author: ForlornU
 * https://github.com/ForlornU/A-Star-pathfinding
 * Edited By: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Finds paths from one tile to another using A* pathfinding
 */

public static class Pathfinder
{
    /// <summary>
    /// Main pathfinding function, marks tiles as being in frontier, while keeping a copy of the frontier
    /// in "currentFrontier" for later clearing
    /// </summary>
    /// <param name="character"></param>
    public static TileGroup FindPath(Tile origin, Tile destination)
    {
        List<Tile> openSet = new List<Tile>(); //contains all new neighboring tiles that we discover
        List<Tile> closedSet = new List<Tile>(); //contains tiles that will become the actual path
        
        //Add starting tile to openSet list and set its cost to 0
        openSet.Add(origin);
        origin.CostFromOrigin = 0;

        //Calculate the width/distance of a tile using its mesh size
        float tileDistance = origin.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z * 2;

        //Continue checking neighboring tiles in openSet list until there are none remaining
        while (openSet.Count > 0)
        {
            //Sort all neighboring tiles by cost (find lowest cost)
            openSet.Sort((x, y) => x.TotalCost.CompareTo(y.TotalCost));
            Tile currentTile = openSet[0];

            //Remove the lowest cost neighbor from openSet and add it to closedSet (the actual path)
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            //Destination reached
            if (currentTile == destination)
            {  
                return MakePath(destination, origin);
            }

            //Evaluate each adjacent tile and their cost
            foreach (Tile neighbor in Rangefinder.GetNeighborTiles(currentTile))
            {
                //Skip checking a neighbor tile that is already within the actual path
                //Skip checking a tile that is not walkable
                if(closedSet.Contains(neighbor) || !neighbor.Walkable)
                    continue;

                float costToNeighbor = currentTile.CostFromOrigin + neighbor.TerrainCost + tileDistance;
                if (costToNeighbor < neighbor.CostFromOrigin || !openSet.Contains(neighbor))
                {
                    neighbor.CostFromOrigin = costToNeighbor;
                    neighbor.CostToDestination = Vector3.Distance(destination.transform.position, neighbor.transform.position);
                    neighbor.Parent = currentTile;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Creates a path between two tiles
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    private static TileGroup MakePath(Tile destination, Tile origin)
    {
        List<Tile> tiles = new List<Tile>();
        Tile current = destination;

        while (current != origin)
        {
            tiles.Add(current);
            if (current.Parent != null)
                current = current.Parent;
            else
                break;
        }

        tiles.Add(origin);
        tiles.Reverse();

        TileGroup path = new TileGroup();
        path.tiles = tiles.ToArray();

        return path;
    }

    /// <summary>
    /// Checks if a given path is valid given the selected unit's move range
    /// </summary>
    /// <param name="path">The path that a selected unit plans to move along</param>
    /// <returns>The movement range of the selected unit</returns>
    public static bool ValidPath(TileGroup path, int moveRange)
    {
        if(path.tiles == null)
            return false;

        //Check if path is longer than move range
        if (path.tiles.Length - 1 > moveRange)
            return false;

        //Check if path destination is occupied
        if (Interact.Instance.CurrentTile.Occupied)
            return false;

        return true;
    }
}
