using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this script as a requirement alongside anything that needs a pool of objects to pull from
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [field: SerializeField] public GameObject objectToPool { get; private set; }
    [field: SerializeField] public int amountToPool { get; private set; }

    public List<GameObject> pooledObjects { get; private set; }

    void Awake()
    {
        //Check if an object to pool was provided
        if(objectToPool == null)
        {
            Debug.LogError("Object Pooler has no object to spawn and pool");
            return;
        }

        pooledObjects = new List<GameObject>();

        //Spawn the object pool and Disable each spawned object
        GameObject spawnedObject;
        for (int i = 0; i < amountToPool; i++)
        {
            spawnedObject = Instantiate(objectToPool, transform);
            spawnedObject.SetActive(false);
            pooledObjects.Add(spawnedObject);
        }
    }
}
