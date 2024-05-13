using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathIllustrator))]
public class Pathfinder : SingletonPattern<Pathfinder>
{
    [SerializeField] private LayerMask tileMask;

    private PathIllustrator illustrator;

    public PathIllustrator Illustrator {  get { return illustrator; } }

    protected override void Awake()
    {
        base.Awake();
        illustrator = GetComponent<PathIllustrator>();
    }

    /// <summary>
    /// Main pathfinding function, marks tiles as being in frontier, while keeping a copy of the frontier
    /// in "currentFrontier" for later clearing
    /// </summary>
    /// <param name="character"></param>
    public TileGroup FindPath(Tile origin, Tile destination)
    {
        List<Tile> openSet = new List<Tile>(); //contains all new neighboring tiles that we discover
        List<Tile> closedSet = new List<Tile>(); //contains tiles that will become the actual path
        
        //Add starting tile to openSet list and set its cost to 0
        openSet.Add(origin);
        origin.costFromOrigin = 0;

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
            foreach (Tile neighbor in GetNeighborTiles(currentTile))
            {
                //Skip checking a neighbor tile that is already within the actual path
                //Skip checking a tile that is not walkable
                if(closedSet.Contains(neighbor) || !neighbor.walkable)
                    continue;

                //Skip checking a tile that is occupied by an enemy
                if (neighbor.Occupied && neighbor.occupyingUnit.GetComponent<Enemy>())
                    continue;

                float costToNeighbor = currentTile.costFromOrigin + neighbor.terrainCost + tileDistance;
                if (costToNeighbor < neighbor.costFromOrigin || !openSet.Contains(neighbor))
                {
                    neighbor.costFromOrigin = costToNeighbor;
                    neighbor.costToDestination = Vector3.Distance(destination.transform.position, neighbor.transform.position);
                    neighbor.parent = currentTile;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a list of all tiles within a set range from an origin tile
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<Tile> FindTilesInRange(Tile origin, int range)
    {
        //If range is 0 (or less), return nothing
        if (range <= 0)
            return null;

        List<Tile> neighborTiles = GetNeighborTiles(origin);
        List<Tile> range1Tiles = new List<Tile>();

        //Get all walkable neighbor tiles of the origin tile
        foreach (Tile neighbor in neighborTiles)
        {
            //Skip adding unwalkable tiles
            if (!neighbor.walkable)
                continue;

            //Skip adding tiles with enemy units
            if (neighbor.Occupied && neighbor.occupyingUnit.GetComponent<Enemy>())
                continue;

            range1Tiles.Add(neighbor);
            neighbor.rangeFromOrigin = 1;
        }

        //If range is 1, return the range 1 tiles
        if (range == 1)
            return range1Tiles;

        List<Tile> tilesInRange = new List<Tile>();
        List<Tile> nextRangeNeighborTiles = new List<Tile>();

        //Add all valid range 1 tiles to the tilesInRange list
        tilesInRange.AddRange(range1Tiles);

        //Set the tiles to check next to the range1Tiles
        nextRangeNeighborTiles = range1Tiles;

        for (int currTileRange = 1; currTileRange < range; currTileRange++)
        {
            List<Tile> tilesAtNewRange = new List<Tile>();

            //Iterate through each tile at the current range
            foreach (Tile tile in nextRangeNeighborTiles)
            {
                //Get neighbors of the tile at the current range
                List<Tile> newNeighborTiles = GetNeighborTiles(tile);

                //Iterate through the new neighbors found
                foreach (Tile nextNeighborTile in newNeighborTiles)
                {
                    //If tile is already within list or is not walkable, skip it
                    if (tilesInRange.Contains(nextNeighborTile) || !nextNeighborTile.walkable)
                        continue;

                    //If tile is occupied by an enemy, skip it
                    if (nextNeighborTile.Occupied && nextNeighborTile.occupyingUnit.GetComponent<Enemy>())
                        continue;

                    //Add walkable tiles at next range to the tilesAtNewRange list
                    tilesInRange.Add(nextNeighborTile);
                    tilesAtNewRange.Add(nextNeighborTile);

                    //Set range from origin for the added tiles
                    nextNeighborTile.rangeFromOrigin = currTileRange + 1;
                }
            }

            //Clear out the list of tiles at the current range, replace with list of tiles at the next range
            nextRangeNeighborTiles.Clear();
            nextRangeNeighborTiles = tilesAtNewRange;
        }

        return tilesInRange;
    }

    /// <summary>
    /// Returns a list of all neighboring hexagonal tiles and ladders
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public List<Tile> GetNeighborTiles(Tile origin)
    {
        //Check if this tile has stored neighbors
        if (origin.neighbors.Count > 0)
            return origin.neighbors;

        List<Tile> neighbors = new List<Tile>();

        //Perform an OverlapSphere cast to find nearby tiles
        Collider[] hitColliders = Physics.OverlapSphere(origin.transform.position, 2f, tileMask);

        //Add all tiles found in the OverlapSphere cast
        foreach (Collider tileCollider in hitColliders)
            neighbors.Add(tileCollider.GetComponent<Tile>());

        //Additionally add connected tiles such as ladders
        if (origin.connectedTile != null)
            neighbors.Add(origin.connectedTile);

        //Store the neighbors of this tile to save time in in future searches
        origin.neighbors = neighbors;

        return neighbors;
    }

    /// <summary>
    /// Creates a path between two tiles
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    private TileGroup MakePath(Tile destination, Tile origin)
    {
        List<Tile> tiles = new List<Tile>();
        Tile current = destination;

        while (current != origin)
        {
            tiles.Add(current);
            if (current.parent != null)
                current = current.parent;
            else
                break;
        }

        tiles.Add(origin);
        tiles.Reverse();

        TileGroup path = new TileGroup();
        path.tiles = tiles.ToArray();

        return path;
    }
}
