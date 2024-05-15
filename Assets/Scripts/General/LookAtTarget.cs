using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Forces this object to look at a specified target object each frame
 */

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] protected Transform target;

    protected virtual void LateUpdate()
    {
        LookAt();
    }

    protected virtual void LookAt()
    {
        if (target == null)
            Debug.LogError("target to look at is not defined!");
        else
            transform.LookAt(target.position);
    }
}
