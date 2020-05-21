using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class BuoyancyVolume : MonoBehaviour
{
    public float density = 998.23f;
    public float drag = 2;
    BoxCollider boxCollider;
    public Bounds bounds => boxCollider.bounds;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }
}
