using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Unit : MonoBehaviour, IDamagable
{
    [field: SerializeField] public UnitData unitData { get; protected set; }
    [SerializeField] protected Slider shieldBar;
    [SerializeField] protected Slider healthBar;
    public LayerMask GroundLayerMask;

    //Health/Shield Properties
    public int MaxShield { get; protected set; }
    public int Shield { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int Health { get; protected set; }
    
    //Movement Properties
    public int MaxMoveRange { get; protected set; }
    public int CurrMoveRange { get; protected set; }
    public float MoveSpeed { get; protected set; }
    public bool Moving { get; protected set; }

    //Attacking Properties
    public int MaxActionPoints { get; protected set; }
    public int CurrActionPoints { get; protected set; }

    [HideInInspector] public Tile occupiedTile;

    protected List<Tile> tilesInRange = new List<Tile>();

    protected virtual void Awake()
    {
        //Set unit to occupy a tile on the hex grid
        FindTileAtStart();

        //Set starting data from unit data
        MaxShield = unitData.MaxShield;
        Shield = Mathf.Clamp(unitData.StartingShield, 0, MaxShield);
        MaxHealth = unitData.MaxHealth;
        Health = Mathf.Clamp(unitData.StartingHealth, 0, MaxHealth);
        MaxMoveRange = unitData.MaxMove;
        MoveSpeed = unitData.MoveSpeed;
        MaxActionPoints = unitData.MaxActionPoints;
        CurrActionPoints = unitData.StartingActionPoints;

        //Set CurrMoveRange to MaxMoveRange
        RefreshUnit();

        //Update the health & shield bar UI
        UpdateHealthUI();

        Moving = false;
    }

    protected virtual void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            print("Debug: T Key pressed, units take 3 damage");
            Damage(3);
        }
    }

    //--------------------------------------------
    // Unit Selection
    //--------------------------------------------

    #region Selection

    public virtual void UnitSelected()
    {
        HUD.Instance.ShowUnitMoveRange(CurrMoveRange, MaxMoveRange);
    }

    public virtual void UnitDeselected()
    {
        HUD.Instance.HideUnitMoveRange();
    }

    /// <summary>
    /// Gets all tiles within this unit's movement range and highlights them
    /// </summary>
    public virtual void ShowMovementRange()
    {
        if (CurrMoveRange <= 0)
            return;

        //Get all tiles in move range
        tilesInRange = Pathfinder.Instance.FindTilesInRange(occupiedTile, CurrMoveRange);

        //Hightlight all tiles in move range
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.HighlightTile(HighlightType.moveArea);

        //Highlight this unit's tile
        occupiedTile.Highlighter.HighlightTile(HighlightType.unitSelection);

        //Show the unit's movement range on the HUD
        HUD.Instance.ShowUnitMoveRange(CurrMoveRange, MaxMoveRange);

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

        //Get all tiles in move range
        tilesInRange = Pathfinder.Instance.FindTilesInRange(occupiedTile, MaxMoveRange);

        //Hightlight all tiles in move range
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.HighlightTile(HighlightType.attackArea);

        //Highlight this unit's tile
        occupiedTile.Highlighter.HighlightTile(HighlightType.unitSelection);
    }

    /// <summary>
    /// Hides the highlights and text for all tiles within the last generated movement range
    /// </summary>
    public virtual void HideMovementRange()
    {
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.ClearTileHighlight();
    }


    /// <summary>
    /// Hides the highlights and text for all tiles within the last generated attack range
    /// </summary>
    public virtual void HideAttackRange()
    {
        foreach (Tile tile in tilesInRange)
            tile.Highlighter.ClearTileHighlight();
    }

    //--------------------------------------------
    // Unit Combat
    //--------------------------------------------

    public void Attack()
    {
        CurrActionPoints--;
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

        if (Physics.Raycast(transform.position + new Vector3(0, 1, 0), -transform.up, out RaycastHit hit, 50f, GroundLayerMask))
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
        Moving = true;
        occupiedTile.occupyingUnit = null;

        CurrMoveRange -= _path.tiles.Length - 1;
        HUD.Instance.ShowUnitMoveRange(CurrMoveRange, MaxMoveRange);

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

            float movementTime = animationTime / ((1/MoveSpeed) + path.tiles[currentStep].terrainCost * TERRAIN_PENALTY);
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

        occupiedTile.Highlighter.ClearAllTileHighlights();
        ShowMovementRange();
        occupiedTile.Highlighter.HighlightTile(HighlightType.unitSelection);
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
        Moving = false;
        tile.occupyingUnit = this;
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
        int dmgToHealth = (Shield - damage) <= 0 ? Mathf.Abs(Shield - damage) : 0;

        Shield = Mathf.Max(Shield - damage, 0);
        Health = Mathf.Max(Health - dmgToHealth, 0);
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

}
