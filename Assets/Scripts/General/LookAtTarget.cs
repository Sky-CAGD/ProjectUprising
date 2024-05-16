using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Forces this object to look at a specified target object each frame
 */

public class LookAtTarget : MonoBehaviour
{
    //prioritze looking at target set in inspector
    public Transform target; 

    //set targetPos from other scripts to look at a position in space
    [HideInInspector] public Vector3 targetPos;

    protected virtual void LateUpdate()
    {
        LookAt();
    }

    protected virtual void LookAt()
    {
        if (target == null && targetPos == Vector3.zero)
            Debug.LogError("target to look at is not defined!");
        else if(target != null)
            transform.LookAt(target.position);
        else if(targetPos != Vector3.zero)
            transform.LookAt(targetPos);
    }
}
