using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BuoyantSphere : MonoBehaviour
{
    SphereCollider collider;
    Rigidbody rb;
    BuoyancyVolume volume;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out BuoyancyVolume v))
        {
            volume = v;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out BuoyancyVolume v))
        {
            if (volume == v)
            {
                volume = null;
            }
        }
    }

    public void Start()
    {
        collider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
    }
    public void FixedUpdate()
    {
        if (volume != null)
        {
            rb.drag = volume.drag;
            float hDiff = volume.bounds.max.y - transform.position.y + collider.radius;
            float v = (Mathf.PI * hDiff * hDiff / 3f) * (3f * collider.radius - hDiff);
            Vector3 volumeWeight = Physics.gravity * volume.density * v;
            rb.AddForce(-volumeWeight);

        }
        else
        {
            rb.drag = 0;
        }
    }
}
