using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassPusher : MonoBehaviour
{
    public float radius;
    public Vector3 offset;
    public Vector4 Data => new Vector4(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z, radius);


    private void OnEnable()
    {
        GrassController.pushers.Add(this);
    }
    private void OnDisable()
    {
        GrassController.pushers.Remove(this);
    }
}
