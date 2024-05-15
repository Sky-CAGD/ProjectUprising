using UnityEngine;

/*
 * Author: ??? [This is pulled from a .unitypackage that's been passed along many times.
 *              Kudos to the original creator, this is extremely useful!]
 * Last Updated: N/A
 * Description: Other scripts may inherit from this to easily become a Singleton
 */

public abstract class SingletonPattern<T> : MonoBehaviour where T : SingletonPattern<T>
{
    public static T Instance;

    //To create Awake in derived classes, use an override Awake and call base.Awake()
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this as T;
        //DontDestroyOnLoad(this);
    }
}
