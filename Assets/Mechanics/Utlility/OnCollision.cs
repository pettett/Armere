using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollision : MonoBehaviour
{
    public delegate void OnCollisionDel(GameObject obj, Collider collision);
    public OnCollisionDel onCollisionDel;

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionEnter(Collision other)
    {

        if (!other.collider.isTrigger && onCollisionDel != null)
            onCollisionDel(gameObject, other.collider);
    }
    void OnTriggerEnter(Collider other)
    {

        if (!other.isTrigger && onCollisionDel != null)
            onCollisionDel(gameObject, other);
    }
}
