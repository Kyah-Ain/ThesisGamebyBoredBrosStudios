//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class ObjectPool
//{
//    private PoolableObjects poolablePrefabs;
//    private List<PoolableObjects> AvailableObjects;

//    private ObjectPool(PoolableObjects poolableObjects, int size) 
//    {
//        this.poolablePrefabs = poolableObjects;
//        AvailableObjects = new List<PoolableObjects>(size);
//    }

//    public static ObjectPool CreateInstance(PoolableObjects prefab, int size) 
//    {
//        ObjectPool pool = new ObjectPool(prefab, size);

//        GameObject poolObjects = new GameObject($"{prefab.name} Pool");
//        pool.CreateObjects(poolObjects.transform, size);

//        return pool;
//    }

//    private void CreateObjects(Transform parent, int size)
//    {
//        for (int i = 0; i < size; i++)
//        {
//            PoolableObjects newPoolableObject = GameObject.Instantiate(poolablePrefabs, Vector3.zero, Quaternion.identity, parent.transform);
//            newPoolableObject.Parent = this;
//            AvailableObjects.Add(newObject);
//        }
//    }
//}