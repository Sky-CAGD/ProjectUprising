using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Forces an object to look at a target object (like the camera) each frame
/// </summary>
public class LookAtTarget : MonoBehaviour
{
    public Transform target;

    protected virtual void FixedUpdate()
    {
        LookAt();
    }

    protected virtual void LookAt()
    {
        transform.LookAt(target.position);
    }
}
