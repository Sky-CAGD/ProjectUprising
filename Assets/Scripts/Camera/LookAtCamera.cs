using UnityEngine;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Forces this object to look at the main camera (creating a billboard)
 */

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