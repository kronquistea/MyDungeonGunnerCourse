using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class PoolManager : SingletonMonobehavior<PoolManager>
{
    #region Tooltip
    [Tooltip("Populate this array with prefabs that you want to add to the pool, and specify the number of gameobjects to be created for each.")]
    #endregion
    [SerializeField] private Pool[] poolArray = null;
    private Transform objectPoolTransform;
    private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();

    [System.Serializable]
    public struct Pool
    {
        public int poolSize;
        public GameObject prefab;
        public string componentType;
    }

    private void Start()
    {
        // This singletone gameobject will be the object pool parent
        objectPoolTransform = this.gameObject.transform;

        // Create object pools on start
        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
        }
    }

    /// <summary>
    /// Create the object pool with the specified prefabs and the specified pool size for each
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="poolSize"></param>
    /// <param name="componentType"></param>
    private void CreatePool(GameObject prefab, int poolSize, string componentType)
    {
        int poolKey = prefab.GetInstanceID();
        
        string prefabName = prefab.name;
        
        GameObject parentGameObject = new GameObject(prefabName + "Anchor"); // Create parent gameobject to parent the child objects to
        parentGameObject.transform.SetParent(objectPoolTransform);

        // If the pool dictionary does not yet contain a queue for the component, create one
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            // If poolSize is 30, this loop will create 30 new components of type componentType in the queue
            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;

                newObject.SetActive(false);

                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    /// <summary>
    /// Reuse a gameobject component in the pool.
    /// </summary>
    /// <param name="prefab">Prefab gameobject containing the component</param>
    /// <param name="position">World position for the gameobject where it should appear when enabled</param>
    /// <param name="rotation">Should be set if the gameobject needs to be rotated</param>
    /// <returns></returns>
    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            // Get object from pool queue
            Component componentToReuse = GetComponentFromPool(poolKey);

            ResetObject(position, rotation, componentToReuse, prefab);

            return componentToReuse;
        }
        else
        {
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }

    /// <summary>
    /// Get a gameobject component from the pool using poolKey
    /// </summary>
    /// <param name="poolKey"></param>
    /// <returns></returns>
    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);

        // If the component is active, deactivate it
        if (componentToReuse.gameObject.activeSelf)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        return componentToReuse;
    }

    /// <summary>
    /// Reset the gameobject
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="componentToReuse"></param>
    /// <param name="prefab"></param>
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.gameObject.transform.localScale = prefab.transform.localScale;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(poolArray), poolArray);
    }
#endif
    #endregion
}
