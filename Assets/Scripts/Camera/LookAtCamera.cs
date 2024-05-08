using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : LookAtTarget
{
    void Start()
    {
        target = Camera.main.transform;
    }

    protected override void LookAt()
    {
        transform.LookAt(target.position);
        transform.Rotate(0, 180, 0);
    }
}