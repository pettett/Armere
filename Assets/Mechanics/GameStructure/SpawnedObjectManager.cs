// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SpawnedObjectManager : MonoBehaviour
// {
//     public static Transform instance;
//     private void Awake()
//     {
//         instance = transform;
//     }
// }

// ///<summary>
// ///This script keeps track of every object spawned into the game,
// ///so they can be saved and restored and so when moved far away from
// ///the object it can be deleted or placed back into a pool
// ///</summary>
// public static class Spawner
// {
//     public static List<GameObject> spawnedGameobjects = new List<GameObject>();

//     public static GameObject Spawn(ref Queue<GameObject> objectPool, string name, params System.Type[] components)
//     {
//         GameObject go;
//         if (objectPool.Count == 0)
//         {
//             //need to instantiate new item
//             go = new GameObject(name, components);
//             go.transform.SetParent(SpawnedObjectManager.instance);
//         }
//         else
//         {
//             //Retrive an object from the pool
//             go = DePool(ref objectPool);
//             go.name = name;
//         }
//         return go;
//     }

//     public static GameObject Spawn(ref Queue<GameObject> objectPool, GameObject prefab, Vector3 position, Quaternion rotation)
//     {
//         GameObject go;
//         if (objectPool.Count == 0)
//         {
//             //need to instantiate new item
//             go = MonoBehaviour.Instantiate(prefab, position, rotation, SpawnedObjectManager.instance);
//         }
//         else
//         {
//             go = DePool(ref objectPool);
//             go.name = prefab.name;

//         }
//         return go;
//     }

//     static GameObject DePool(ref Queue<GameObject> objectPool)
//     {
//         //Retrive an object from the pool
//         var go = objectPool.Dequeue();
//         go.SetActive(true);
//         return go;
//     }

//     public static void DeSpawn(GameObject gameObject, ref Queue<GameObject> objectPool)
//     {
//         gameObject.SetActive(false);
//         objectPool.Enqueue(gameObject);
//     }

// }
