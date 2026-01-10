
using UnityEngine;

public class SingletonMonoBehaviourAutoCreate<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var instance = new GameObject();
                _instance = instance.AddComponent<T>();
                _instance.GetComponent<SingletonMonoBehaviourAutoCreate<T>>().OnCreate();
                return _instance;
            }

            return _instance;
        }
    }
    private static T _instance;

    protected virtual void OnCreate() { }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}