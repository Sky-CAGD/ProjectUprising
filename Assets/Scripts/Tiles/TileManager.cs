using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    None,
    Standard,
    Wall
}

public class TileManager : SingletonPattern<TileManager>
{
    public Material noneMaterial;
    public Material standardMaterial;
    public Material wallMaterial;
    public Material highlightMaterial;

    public void SetTileAsNone(Tile tile)
    {

    }

    public void SetTileAsStandard(Tile tile)
    {

    }

    public void SetTileAsWall(Tile tile)
    {

    }
}
