using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles behavior of AI controlled enemy units
 */

public class Enemy : Unit
{
    protected Tile target;
    protected bool takingTurn;

    [Header("Debug/Testing")]
    [SerializeField] bool viewValidMoveTiles = false;

    protected override void Start()
    {
        base.Start();

        takingTurn = false;
    }

    public virtual void StartTurn()
    {
        takingTurn = true;

        /// AI METHOD 2
        float waitTime = 0f;
        if (viewValidMoveTiles)
            waitTime = 2f;

        StartCoroutine(ExecuteTurn(waitTime));

        /// AI METHOD 1
        /*
        Character[] characters = FindObjectsOfType<Character>();
        TileGroup[] characterPaths = GeneratePathsToCharacters(characters);
        List<Character> charsInRange = GetCharactersInRange(characters, characterPaths);

        if (charsInRange.Count == 1) //There was exactly one character within sight & range
        {
            StartAttack(charsInRange[0].occupiedTile);
        }
        else //There was zero OR more than one character unit within sight & range
        {
            int closestChar = GetClosestCharacterIndex(characters, characterPaths);
            target = characters[closestChar].occupiedTile;
            TileGroup shortestPath = characterPaths[closestChar];
            Tile moveTile = FindTileToMoveTo(shortestPath);
            TileGroup movePath = Pathfinder.FindPath(occupiedTile, moveTile);

            StartMove(movePath);
        }
        */

    }

    protected IEnumerator ExecuteTurn(float waitTime)
    {
        Tile bestMoveTile = FindBestMoveTile();

        yield return new WaitForSeconds(waitTime);

        if (viewValidMoveTiles)
            occupiedTile.Highlighter.ClearAllTileHighlights();

        Character[] characters = FindObjectsOfType<Character>();
        TileGroup[] characterPaths = GeneratePathsToCharacters(characters);

        //Enemy cannot attack a character this turn
        //Instead, move towards the nearest character
        if (bestMoveTile == null)
        {
            int closestChar = GetClosestCharacterIndex(characters, characterPaths);
            target = characters[closestChar].occupiedTile;
            TileGroup movePath = Pathfinder.FindPath(occupiedTile, characterPaths[closestChar].tiles[CurrMoveRange]);

            StartMove(movePath);
        }
        //Enemy is currently standing on best tile, immediately attack
        else if (bestMoveTile == occupiedTile)
        {
            List<Character> charsInRange = GetCharactersInRange(characters, characterPaths);

            if (charsInRange.Count > 0)
            {
                target = charsInRange[0].occupiedTile;
                StartAttack(target);
            }
            else
                Debug.LogError("Failed to properly find an attackable character unit");
        }
        //Enemy needs to move towards the best tile, then attack if possible
        else
        {
            TileGroup movePath = Pathfinder.FindPath(occupiedTile, bestMoveTile);

            if (movePath == null)
            {
                Debug.LogError(gameObject + " Failed to find a path to the best tile: " + bestMoveTile);
                EndTurn();
                StopAllCoroutines();
            }

            //If the best tile is further than this unit's move range, shorten the path
            if (movePath.tiles.Length - 1 > CurrMoveRange)
                movePath = Pathfinder.FindPath(occupiedTile, movePath.tiles[CurrMoveRange]);

            StartMove(movePath);
        }
    }

    protected void EndTurn()
    {
        takingTurn = false;
        EventManager.OnEnemyTurnEnded();
    }

    /// <summary>
    /// End the movement along a path for this unit
    /// </summary>
    /// <param name="tile"></param>
    protected override void FinalizePosition(Tile tile)
    {
        base.FinalizePosition(tile);

        if(takingTurn)
        {
            Character[] characters = FindObjectsOfType<Character>();
            TileGroup[] characterPaths = GeneratePathsToCharacters(characters);
            List<Character> charsInRange = GetCharactersInRange(characters, characterPaths);

            target = null;
            if (charsInRange.Count > 0)
                target = charsInRange[0].occupiedTile;

            if (target == null)
            {
                EndTurn();
            }
            else
            {
                if (CanAttackTarget(tile, target))
                    StartAttack(target);
                else
                    EndTurn();
            }
        }
    }

    public override void EndAttack()
    {
        base.EndAttack();

        EndTurn();
    }

    protected virtual Tile FindBestMoveTile()
    {
        Character[] characters = FindObjectsOfType<Character>();
        Tile bestTile = null;
        List<Tile> potentialAttackTiles = new List<Tile>();
        List<Tile> tilesInRange = new List<Tile>();

        //Find all tiles that this unit could attack from to hit a character unit
        if (weapon.attackType == AttackType.laser)
            tilesInRange = FindObjectsOfType<Tile>().ToList();
        else
        {
            //Iterate through each character unit
            foreach (Character character in characters)
            {
                //Find tiles within attack range of this character
                List<Tile> newTilesInRange = Rangefinder.FindTilesInRange(character.occupiedTile, weapon.range);

                //Add tiles that are not already within the tilesInRange list
                foreach (Tile tile in newTilesInRange)
                    if (!tilesInRange.Contains(tile))
                        tilesInRange.Add(tile);
            }
        }

        //Of the tiles in attack range, store the ones with line of sight to a character unit
        foreach (Character character in characters)
        {
            foreach (Tile tile in tilesInRange)
            {
                //Check if the tile potential move tile is walkable & unoccupied, or if it is this units current tile
                if(!tile.Occupied && tile.Walkable || tile == occupiedTile)
                {
                    //Check line of sight except for artillery
                    if (weapon.attackType == AttackType.artillery || CanSeeTarget(tile, character.occupiedTile))
                    {
                        //Add tiles that are not already within the potentialAttackTiles list
                        if (!potentialAttackTiles.Contains(tile))
                        {
                            potentialAttackTiles.Add(tile);

                            if (viewValidMoveTiles)
                                tile.Highlighter.HighlightTile(HighlightType.validPath);
                        }
                    }
                }
            }
        }

        float closestTile = float.MaxValue;
        //Find the nearest tile this unit can attack from
        foreach (Tile tile in potentialAttackTiles)
        {
            float distToTile = Vector3.Distance(occupiedTile.transform.position, tile.transform.position);

            if(distToTile < closestTile)
            {
                closestTile = distToTile;
                bestTile = tile;
            }
        }

        if (viewValidMoveTiles)
            bestTile.Highlighter.HighlightTile(HighlightType.attackTarget);

        return bestTile;
    }

    /// <summary>
    /// Finds each character that this enemy can attack currently
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    protected virtual List<Character> GetCharactersInRange(Character[] characters, TileGroup[] charPaths)
    {
        List<Character> charsInRange = new List<Character>();

        for (int i = 0; i < characters.Length; i++)
        {
            //Check for walls blocking line of sight
            if (CanSeeTarget(occupiedTile, characters[i].occupiedTile))
            {
                //Check if character is within weapon's attack range
                if (charPaths[i] != null && charPaths[i].tiles.Length - 1 <= weapon.range)
                    charsInRange.Add(characters[i]);
            }
        }

        return charsInRange;
    }

    /// <summary>
    /// Returns the array index of the character that is nearest to this enemy
    /// </summary>
    /// <param name="characters"></param>
    /// <param name="charPaths"></param>
    /// <returns></returns>
    protected virtual int GetClosestCharacterIndex(Character[] characters, TileGroup[] charPaths)
    {
        int closestCharIndex = 0;
        int closestCharRange = int.MaxValue;

        //Check for the closest character by tile range
        for (int i = 0; i < characters.Length; i++)
        {
            int charRange = charPaths[i].tiles.Length - 1;

            if(charRange < closestCharRange)
            {
                closestCharRange = charRange;
                closestCharIndex = i;
            }
        }

        return closestCharIndex;
    }

    /// <summary>
    /// Looks through tiles in a given path for the best one to move to
    /// </summary>
    /// <returns></returns>
    protected virtual Tile FindTileToMoveTo(TileGroup path)
    {
        Tile bestTile = null;
        Tile lastTile = path.tiles[path.tiles.Length - 1];
        int rangeFromCurrTile = path.tiles.Length - 1;

        int numTilesToCheck = Mathf.Min(CurrMoveRange, path.tiles.Length - 2);
        for (int i = 0; i < numTilesToCheck; i++)
        {
            //Check if there is line of sight and weapon range to target's tile
            if (CanSeeTarget(path.tiles[i], lastTile) && rangeFromCurrTile <= weapon.range)
            {
                bestTile = path.tiles[i];
                break;
            }
            rangeFromCurrTile--;
        }

        //Check if no tiles were found this unit can attack from
        if(bestTile == null)
            bestTile = path.tiles[CurrMoveRange]; //Move to furthest possible tile in move range

        return bestTile;
    }

    /// <summary>
    /// Uses A* pathfinding to get a path to each character unit
    /// </summary>
    /// <param name="characters"></param>
    /// <returns></returns>
    protected virtual TileGroup[] GeneratePathsToCharacters(Character[] characters)
    {
        TileGroup[] characterPaths = new TileGroup[characters.Length];

        for (int i = 0; i < characters.Length; i++)
        {
            TileGroup path = Pathfinder.FindPath(occupiedTile, characters[i].occupiedTile);
            characterPaths[i] = path;
        }

        return characterPaths;
    }

    /// <summary>
    /// Checks if a target tile can be seen from a particular tile, returns true if no walls were hit
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    protected virtual bool CanSeeTarget(Tile tile, Tile target)
    {
        bool canSee = false;
        Vector3 offset = new Vector3(0, 1, 0);

        //Check for walls blocking line of sight
        if (!Physics.Linecast(tile.transform.position + offset, target.transform.position + offset, wallLayer))
            canSee = true;

        if (weapon.attackType == AttackType.artillery)
            canSee = true;

        return canSee;
    }

    /// <summary>
    /// Checks if a target tile can be attacked from a particular tile
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    protected virtual bool CanAttackTarget(Tile tile, Tile target)
    {
        bool canHit = false;

        int rangeFromCurrTile = Pathfinder.FindPath(tile, target).tiles.Length - 1;

        //Check if there is line of sight and weapon range to target's tile
        if (CanSeeTarget(tile, target) && rangeFromCurrTile <= weapon.range)
            canHit = true;

        return canHit;
    }
}
