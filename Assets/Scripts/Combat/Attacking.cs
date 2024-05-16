using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using static Cinemachine.AxisState;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Handles how units attack
 */

public enum AttackType
{
    shoot,
    laser,
    artillery,
    blast,
    melee
}

public class Attacking : SingletonPattern<Attacking>
{
    [Header("Attack Modifiers")]
    [SerializeField] private float arcHeight = 10;
    [SerializeField] private float minAttackTime = 0.75f;
    [SerializeField] private float maxAttackTime = 2.5f;

    [Header("Layer Info")]
    [SerializeField] private LayerMask wallLayer;

    private float maxDist = 40;
    private Vector3 offset;

    private void Start()
    {
        offset = new Vector3 (0, 1, 0);
    }

    private void OnEnable()
    {
        EventManager.UnitStartedAttacking += StartAttacking;
    }

    private void OnDisable()
    {
        EventManager.UnitStartedAttacking -= StartAttacking;
    }

    private void StartAttacking(Unit unit, Tile target)
    {
        switch (unit.weapon.attackType)
        {
            case AttackType.shoot:
                {
                    StartCoroutine(ShootAttack(unit, target));
                    break;
                }
            case AttackType.laser:
                {
                    StartCoroutine(LaserAttack(unit, target));
                    break;
                }
            case AttackType.artillery:
                {
                    StartCoroutine(ArtilleryAttack(unit, target));
                    break;
                }
            case AttackType.blast:
                {

                    break;
                }
            case AttackType.melee:
                {

                    break;
                }
            default:
                break;
        }
    }

    /// <summary>
    /// Determines if the trajectory between A & B is valid (unit can attack PointB)
    /// </summary>
    /// <returns></returns>
    public bool ValidAttackTrajectory()
    {
        Character selectedCharacter = Interact.Instance.SelectedCharacter;
        Tile currTile = Interact.Instance.CurrentTile;

        if (selectedCharacter == null || currTile == null)
            return false;

        if (!currTile.Walkable)
            return false;

        //If the character's weapon is not a laser, check for range to target
        if (selectedCharacter.weapon.attackType != AttackType.laser)
        {
            if (currTile.RangeFromOrigin > selectedCharacter.weapon.range)
                return false;
        }

        Vector3 pointA = selectedCharacter.transform.position + offset;
        Vector3 pointB = currTile.transform.position + offset;

        //If the character's weapon is not artillery, check for line of sight to target tile
        if (selectedCharacter.weapon.attackType != AttackType.artillery)
        {
            if (Physics.Linecast(pointA, pointB, wallLayer))
                return false;
        }

        return true;
    }

    //--------------------------------------------
    // Shoot Attack
    //--------------------------------------------

    /// <summary>
    /// Lerps the projectile of a shoot attack along a straight line each frame
    /// </summary>
    /// <param name="pointA">The starting point to draw a trajectory from</param>
    /// <param name="pointB">The ending point to draw a trajectory to</param>
    /// <returns></returns>
    private IEnumerator ShootAttack(Unit unit, Tile target)
    {
        Vector3 pointA = unit.transform.position + offset;
        Vector3 pointB = target.transform.position + offset;

        GameObject projectile = Instantiate(unit.weapon.projectile, pointA, Quaternion.identity);
        projectile.GetComponent<LookAtTarget>().targetPos = pointB;

        float dist = Mathf.Clamp(Vector3.Distance(pointA, pointB), 0, maxDist);
        float attackTime = Mathf.Lerp(minAttackTime, maxAttackTime, dist / maxDist);
        float timeElapsed = 0f;
        float u;

        //Interpolate projectile from attacking unit to target
        while (timeElapsed < attackTime)
        {
            u = timeElapsed / attackTime;
            projectile.transform.position = Vector3.Lerp(pointA, pointB, u);

            yield return null;
            timeElapsed += Time.deltaTime;
        }

        Destroy(projectile);

        //Deal damage to any units occupying the target tile
        if (target.Occupied)
        {
            target.OccupyingUnit.Damage(unit.weapon.baseDamage);
        }

        unit.EndAttack();
    }

    //--------------------------------------------
    // Laser Attack
    //--------------------------------------------

    private IEnumerator LaserAttack(Unit unit, Tile target)
    {
        Vector3 pointA = unit.transform.position + offset;
        Vector3 pointB = target.transform.position + offset;

        GameObject projectile = Instantiate(unit.weapon.projectile, pointA, Quaternion.identity);
        LineRenderer laserLine = projectile.GetComponent<LineRenderer>();
        laserLine.SetPosition(0, pointA);

        //Set width of laser based on damage output
        float maxWidthDmg = 30f;
        float widthU = Mathf.Clamp(unit.weapon.baseDamage, 0 , maxWidthDmg) / maxWidthDmg;
        float laserWidth = Mathf.Lerp(0.1f, 1f, widthU);
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;

        float laserLerpTime = 0.05f;
        float laserHoldTime = 2f;
        float timeElapsed = 0f;
        float u;

        //Interpolate projectile from attacking unit to target
        while (timeElapsed < laserLerpTime)
        {
            u = timeElapsed / laserLerpTime;
            Vector3 pointC = Vector3.Lerp(pointA, pointB, u);
            laserLine.SetPosition(1, pointC);

            yield return null;
            timeElapsed += Time.deltaTime;
        }
        laserLine.SetPosition(1, pointB);

        yield return new WaitForSeconds(laserHoldTime);

        Destroy(projectile);

        //Deal damage to any units occupying the target tile
        if (target.Occupied)
        {
            target.OccupyingUnit.Damage(unit.weapon.baseDamage);
        }

        unit.EndAttack();
    }

    //--------------------------------------------
    // Artillery Attack
    //--------------------------------------------

    /// <summary>
    /// Lerps the projectile of an artillery attack along an arced line each frame
    /// </summary>
    /// <param name="pointA">The starting point to draw a trajectory from</param>
    /// <param name="pointB">The ending point to draw a trajectory to</param>
    /// <returns></returns>
    private IEnumerator ArtilleryAttack(Unit unit, Tile target)
    {
        Vector3 pointA = unit.transform.position + offset;
        Vector3 pointB = target.transform.position + offset;
        Vector3 pointC = Vector3.Lerp(pointA, pointB, 0.5f);
        pointC += new Vector3(0, arcHeight * 2, 0);

        float inFrontAmt = 0.1f;

        //Have the artillery look at this point to apply proper rotation
        Vector3 pointAC = Vector3.Lerp(pointA, pointC, inFrontAmt);
        Vector3 pointCB = Vector3.Lerp(pointC, pointB, inFrontAmt);
        Vector3 pointD = Vector3.Lerp(pointAC, pointCB, inFrontAmt);

        GameObject projectile = Instantiate(unit.weapon.projectile, pointA, Quaternion.identity);
        LookAtTarget projLookAt = projectile.GetComponent<LookAtTarget>();
        projLookAt.targetPos = pointD;

        float dist = Mathf.Clamp(Vector3.Distance(pointA, pointB), 0, maxDist);
        float attackTime = Mathf.Lerp(minAttackTime, maxAttackTime, dist / maxDist);
        float timeElapsed = 0f;
        float u;
        float Du;

        //Interpolate projectile from attacking unit to target
        while (timeElapsed < attackTime)
        {
            //Projectile movement
            u = timeElapsed / attackTime;
            pointAC = Vector3.Lerp(pointA, pointC, u);
            pointCB = Vector3.Lerp(pointC, pointB, u);
            projectile.transform.position = Vector3.Lerp(pointAC, pointCB, u);

            //Projectile rotation
            Du = Mathf.Clamp(u + inFrontAmt, 0, 1);
            pointAC = Vector3.Lerp(pointA, pointC, Du);
            pointCB = Vector3.Lerp(pointC, pointB, Du);
            pointD = Vector3.Lerp(pointAC, pointCB, Du);
            projLookAt.targetPos = pointD;

            yield return null;
            timeElapsed += Time.deltaTime;
        }

        Destroy(projectile);

        //Deal damage to any units occupying the target tile
        if (target.Occupied)
        {
            target.OccupyingUnit.Damage(unit.weapon.baseDamage);
        }

        unit.EndAttack();
    }

}
