using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassPusher : MonoBehaviour, IScanable
{
    public float radius;
    public Vector3 offset;
    public Vector4 Data => new Vector4(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z, radius);

    Vector3 IScanable.offset => offset;

    private void OnEnable()
    {
        TypeGroup<GrassPusher>.allObjects.Add(this);
    }
    private void OnDisable()
    {
        TypeGroup<GrassPusher>.allObjects.Remove(this);
    }
}
