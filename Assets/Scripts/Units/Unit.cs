using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour, IDamagable
{
    [field: SerializeField] public UnitData unitData { get; private set; }
    [SerializeField] private Slider shieldBar;
    [SerializeField] private Slider healthBar;
    public LayerMask GroundLayerMask;

    public int MaxShield { get; private set; }
    public int Shield { get; private set; }
    public int MaxHealth { get; private set; }
    public int Health { get; private set; }
    public bool Moving { get; private set; }

    public int MaxMove { get; private set; }
    public float MoveSpeed { get; private set; }

    [HideInInspector] public Tile occupiedTile;

    private void Awake()
    {
        //set unit to occupy a tile on the hex grid
        FindTileAtStart();

        //set starting data from unit data
        MaxShield = unitData.MaxShield;
        Shield = unitData.Shield;
        MaxHealth = unitData.MaxHealth;
        Health = unitData.Health;
        MaxMove = unitData.MaxMove;
        MoveSpeed = unitData.MoveSpeed;

        //update the health & shield bar UI
        UpdateHealthUI();

        Moving = false;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Damage(3);
        }
    }

    //--------------------------------------------
    // Unit Movement
    //--------------------------------------------

    #region Movement
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

            float movementTime = animationTime / (MoveSpeed + path.tiles[currentStep].terrainCost * TERRAIN_PENALTY);
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
        Vector3 lookDir = origin.DirectionTo(destination).Flat();

        if(lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
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
    public void Damage(int damage)
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
    public void Heal(int healAmt)
    {
        Health += Mathf.Min(healAmt, MaxHealth);
    }

    /// <summary>
    /// Called from Damage when Health is reduced to 0
    /// </summary>
    private void OnDeath()
    {
        Destroy(gameObject);
        Debug.Log("Player unit killed!");
    }

    private void UpdateHealthUI()
    {
        shieldBar.maxValue = MaxShield;
        shieldBar.value = Shield;
        healthBar.maxValue = MaxHealth;
        healthBar.value = Health;
    }
    #endregion

}
