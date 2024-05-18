using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Finds neighbor tiles and tiles within range of an origin
 * IMPORTANT: Require that the tile layer is Layer 3
 */

public static class Rangefinder
{
    /// <summary>
    /// Returns a list of all tiles within a set range from an origin tile
    /// </summary>
    /// <param name="origin">The tile to get a range from</param>
    /// <param name="range">The number of tiles away from the origin to find</param>
    /// <param name="includeCharacters">If true, includes tiles occupied by character units</param>
    /// <param name="includeEnemies">If true, includes tiles occupied by enemy units</param>
    /// <returns></returns>
    public static List<Tile> FindTilesInRange(Tile origin, int range, bool includeUnwalkable = false, bool includeCharacters = true, bool includeEnemies = true)
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
            if (!includeUnwalkable && !neighbor.Walkable)
                continue;

            //Skip adding tiles with enemy units
            if (!includeEnemies && neighbor.Occupied && neighbor.OccupyingUnit.GetComponent<Enemy>())
                continue;

            //Skip adding tiles with character units
            if (!includeCharacters && neighbor.Occupied && neighbor.OccupyingUnit.GetComponent<Character>())
                continue;

            range1Tiles.Add(neighbor);
            neighbor.RangeFromOrigin = 1;
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
                    if (tilesInRange.Contains(nextNeighborTile))
                        continue;

                    //Skip adding unwalkable tiles (unless otherwise specified)
                    if (!includeUnwalkable && !nextNeighborTile.Walkable)
                        continue;

                    //Skip adding tiles with enemy units
                    if (!includeEnemies && nextNeighborTile.Occupied && nextNeighborTile.OccupyingUnit.GetComponent<Enemy>())
                        continue;

                    //Skip adding tiles with character units
                    if (!includeCharacters && nextNeighborTile.Occupied && nextNeighborTile.OccupyingUnit.GetComponent<Character>())
                        continue;

                    //Add walkable tiles at next range to the tilesAtNewRange list
                    tilesInRange.Add(nextNeighborTile);
                    tilesAtNewRange.Add(nextNeighborTile);

                    //Set range from origin for the added tiles
                    nextNeighborTile.RangeFromOrigin = currTileRange + 1;
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
    /// <param name="origin">The starting tile to get neighbors of</param>
    /// <returns></returns>
    public static List<Tile> GetNeighborTiles(Tile origin)
    {
        //Check if this tile has stored neighbors
        if (origin.Neighbors != null && origin.Neighbors.Count > 0)
            return origin.Neighbors;

        List<Tile> neighbors = new List<Tile>();

        //Perform an OverlapSphere cast to find nearby tiles
        Collider[] hitColliders = Physics.OverlapSphere(origin.transform.position, 2f, 1<<3);

        //Add all tiles found in the OverlapSphere cast
        foreach (Collider tileCollider in hitColliders)
            neighbors.Add(tileCollider.GetComponent<Tile>());

        //Additionally add connected tiles such as ladders
        if (origin.ConnectedTile != null)
            neighbors.Add(origin.ConnectedTile);

        //Store the neighbors of this tile to save time in in future searches
        origin.Neighbors = neighbors;

        return neighbors;
    }
}
