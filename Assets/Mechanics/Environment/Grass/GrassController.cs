// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// [ExecuteAlways]
// public class GrassController : MonoBehaviour
// {
//     public static GrassController singleton;
//     public Vector3Int population;
//     public int totalPopulation => population.x * population.y * population.z * 64;
//     public float range;

//     public Material material;
//     public Transform pusher;

//     public Bounds bounds;

//     public Terrain terrain;
//     public Gradient colorGradient = new Gradient();

//     public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
//     public Vector2 quadHeightRange = new Vector2(0.5f, 1f);


//     private void Awake()
//     {
//         singleton = this;

//         bounds = new Bounds(transform.position, Vector3.one * (range * 2 + 1));
//     }


//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.DrawWireCube(bounds.center, bounds.size);
//     }
// }