using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles how each unit on the hex grid is selected, moves, attacks, & takes damage
 */

public enum UnitState
{
    idle,
    moving,
    attacking,
    planningAttack,
    planningMovement
}

public abstract class Unit : MonoBehaviour, IDamagable
{
    //Inspector exposed fields
    [field: SerializeField] public UnitData unitData { get; protected set; }
    [field: SerializeField] public Weapon weapon { get; protected set; }
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask wallLayer;

    [Header("Health UI")]
    [SerializeField] protected TMP_Text weaponTypeText;
    [SerializeField] protected Slider shieldBar;
    [SerializeField] protected Slider healthBar;

    [Header("Debug/Testing")]
    //If true, any amount of shield will prevent all health damage
    [SerializeField] protected bool shieldBlocksAll; 

    //Health/Shield Properties
    public int MaxShield { get; protected set; }
    public int Shield { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int Health { get; protected set; }
    
    //Movement Properties
    public int MaxMoveRange { get; protected set; }
    public int CurrMoveRange { get; protected set; }
    public float MoveSpeed { get; protected set; }
    public UnitState CurrState { get; protected set; }

    //Attacking Properties
    public int MaxActionPoints { get; protected set; }
    public int CurrActionPoints { get; protected set; }

    public Tile occupiedTile;

    protected List<Tile> tilesInRange = new List<Tile>();

    protected virtual void Awake()
    {
        //Set unit to occupy a tile on the hex grid
        FindTileAtStart();

        //Set starting data from unit data
        MaxShield = unitData.maxShield;
        Shield = Mathf.Clamp(unitData.startingShield, 0, MaxShield);
        MaxHealth = unitData.maxHealth;
        Health = Mathf.Clamp(unitData.startingHealth, 0, MaxHealth);
        MaxMoveRange = unitData.maxMove;
        MoveSpeed = unitData.moveSpeed;
        MaxActionPoints = unitData.maxActionPoints;
        CurrActionPoints = unitData.startingActionPoints;

        //Set CurrMoveRange to MaxMoveRange
        RefreshUnit();

        //Update the health & shield bar UI
        UpdateHealthUI();
        weaponTypeText.text = weapon.attackType.ToString();

        CurrState = UnitState.idle;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            print("Debug: T Key pressed, units gain 1 action point");
            AddActionPoints(1);
        }
    }

    //--------------------------------------------
    // Unit Selection
    //--------------------------------------------

    #region Selection

    /// <summary>
    /// Gets all tiles within this unit's movement range and highlights them
    /// </summary>
    public virtual void ShowMovementRange()
    {
        if (CurrMoveRange <= 0)
            return;

        //Get all tiles in move range
        if (tilesInRange.Count == 0)
            tilesInRange = Rangefinder.FindTilesInRange(occupiedTile, CurrMoveRange);

        //Hightlight all tiles in move range
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.HighlightTile(HighlightType.moveArea);

        //Displays the distances to all tiles in move range
        //Pathfinder.Instance.Illustrator.DisplayMoveAreaDistances(tilesInRange);
    }

    /// <summary>
    /// Gets all tiles within this unit's attack range and highlights them
    /// </summary>
    public virtual void ShowAttackRange()
    {
        if (CurrActionPoints <= 0)
            return;

        //Get all tiles in attack range
        if (tilesInRange.Count == 0)
        {
            //Get tiles within weapon range
            if (weapon.attackType == AttackType.laser)
                tilesInRange = FindObjectsOfType<Tile>().ToList();
            else
                tilesInRange = Rangefinder.FindTilesInRange(occupiedTile, weapon.range, true);

            //Get tiles within line of sight
            if (weapon.attackType != AttackType.artillery)
            {
                Vector3 offset = new Vector3(0, 1, 0);
                Vector3 origin = transform.position + offset;
                List<Tile> sightOfSightTiles = new List<Tile>();
                foreach (Tile tile in tilesInRange)
                {
                    Vector3 tilePos = tile.transform.position + offset;
                    if (!Physics.Linecast(origin, tilePos, wallLayer))
                        sightOfSightTiles.Add(tile);
                }

                tilesInRange = sightOfSightTiles;
            }
        }

        //Highlight all tiles in attack range
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.HighlightTile(HighlightType.attackArea);
    }

    /// <summary>
    /// Clears out the last generated movement or attack range when exiting planning
    /// </summary>
    public virtual void ClearTilesInRange()
    {
        foreach (Tile tile in tilesInRange)
            tile.RangeFromOrigin = int.MaxValue;

        if(tilesInRange.Count > 0)
            tilesInRange.Clear();
    }

    #endregion

    //--------------------------------------------
    // Unit Combat
    //--------------------------------------------


    #region Combat

    /// <summary>
    /// Reduces the unit's action points and begins an attack against a single target
    /// </summary>
    /// <param name="target"></param>
    public virtual void StartAttack(Tile target)
    {
        if (CurrActionPoints <= 0)
            return;

        CurrActionPoints--;
        EventManager.OnUnitStartedAttacking(this, target);
        CurrState = UnitState.attacking;
    }


    public virtual void EndAttack()
    {
        occupiedTile.Highlighter.ClearAllTileHighlights();
    }

    #endregion

    //--------------------------------------------
    // Unit Movement
    //--------------------------------------------

    #region Movement
    /// <summary>
    /// If no starting tile has been manually assigned, unit looks for one beneath it to occupy
    /// </summary>
    protected virtual void FindTileAtStart()
    {
        if (occupiedTile != null)
        {
            FinalizePosition(occupiedTile);
            return;
        }

        if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), -transform.up, out RaycastHit hit, 50f, groundLayer))
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
    public virtual void StartMove(TileGroup _path)
    {
        Tile destination = _path.tiles[_path.tiles.Length - 1];

        if (destination.Occupied || !destination.Walkable)
            return;

        CurrState = UnitState.moving;
        occupiedTile.OccupyingUnit = null;

        CurrMoveRange -= _path.tiles.Length - 1;

        StartCoroutine(MoveAlongPath(_path));
    }

    /// <summary>
    /// Moves this unit along a given path over time
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    protected virtual IEnumerator MoveAlongPath(TileGroup path)
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

            float movementTime = animationTime / ((1/MoveSpeed) + path.tiles[currentStep].TerrainCost * TERRAIN_PENALTY);
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
    }

    /// <summary>
    /// End the movement along a path for this unit
    /// </summary>
    /// <param name="tile"></param>
    protected virtual void FinalizePosition(Tile tile)
    {
        tilesInRange.Clear();
        transform.position = tile.transform.position;
        occupiedTile = tile;
        tile.OccupyingUnit = this;
        CurrState = UnitState.idle;
    }

    protected virtual void MoveAndRotate(Vector3 origin, Vector3 destination, float duration)
    {
        transform.position = Vector3.Lerp(origin, destination, duration);
        Vector3 lookDir = origin.DirectionTo(destination).Flat();

        if(lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    /// <summary>
    /// When the round ends, refresh this unit's movement & other stats
    /// </summary>
    public virtual void RefreshUnit()
    {
        CurrMoveRange = MaxMoveRange;
        CurrActionPoints = MaxActionPoints;
    }

    #endregion

    //--------------------------------------------
    // Unit Health
    //--------------------------------------------

    #region Health
    /// <summary>
    /// Decreases Health by provided damage value, prevents Health from becoming zero
    /// </summary>
    /// <param name="damage"></param>
    public virtual void Damage(int damage)
    {
        if(shieldBlocksAll) //Debug/testing - Health cannot be reduced if unit has any shield
        {
            if(Shield > 0)
                Shield = Mathf.Max(Shield - damage, 0);
            else
                Health = Mathf.Max(Health - damage, 0);
        }
        else //Damage dealt past the current shield value affects the unit's Health
        {
            int dmgToHealth = (Shield - damage) <= 0 ? Mathf.Abs(Shield - damage) : 0;

            Shield = Mathf.Max(Shield - damage, 0);
            Health = Mathf.Max(Health - dmgToHealth, 0);
        }

        UpdateHealthUI();

        if(Health <= 0)
            OnDeath();
    }

    /// <summary>
    /// Increases Health by provided healAmt value, prevents Health from being larger than MaxHealth
    /// </summary>
    /// <param name="healAmt"></param>
    public virtual void Heal(int healAmt)
    {
        Health += Mathf.Min(healAmt, MaxHealth);
    }

    /// <summary>
    /// Called from Damage when Health is reduced to 0
    /// </summary>
    protected virtual void OnDeath()
    {
        Destroy(gameObject);
        Debug.Log("Unit killed!");
    }

    protected virtual void UpdateHealthUI()
    {
        shieldBar.maxValue = MaxShield;
        shieldBar.value = Shield;
        healthBar.maxValue = MaxHealth;
        healthBar.value = Health;
    }
    #endregion

    //--------------------------------------------
    // Action Points
    //--------------------------------------------
    protected virtual void AddActionPoints(int actionPoints)
    {
        CurrActionPoints += actionPoints;
    }
}
