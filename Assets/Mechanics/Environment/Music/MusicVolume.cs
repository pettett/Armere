using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicVolume : MonoBehaviour
{
    public float m_blendDistance = 0;
    public MusicTrack m_track;

    Collider[] colliders;



    // Start is called before the first frame update
    void Start()
    {
        GetColliders();
        if (m_track != null) //Do not register volumes with no music
            MusicController.instance.Register(this);
    }
    void GetColliders()
    {
        colliders = GetComponents<Collider>();
    }
    void OnDisable()
    {
        MusicController.instance.Unregister(this);
    }
    public Vector3 ClosestPoint(Vector3 to)
    {
        Vector3 closest = Vector3.zero;
        //TODO - make work
        foreach (var c in colliders)
        {
            //Closest point inside a collider is itself - zero distance
            closest = c.ClosestPoint(to);
        }
        return closest;
    }

    private void OnDrawGizmos()
    {
        if (colliders == null) GetColliders();
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        foreach (var col in colliders)
        {
            //Stolen from URP Post-process volumes
            //It uses a switch to cast types? pretty epic
            switch (col)
            {
                case BoxCollider c:
                    Gizmos.DrawWireCube(c.center, c.size + Vector3.one * m_blendDistance * 2);
                    break;
                case SphereCollider c:
                    // For sphere the only scale that is used is the transform.x
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.DrawSphere(c.center, c.radius);
                    break;
                case MeshCollider c:
                    // Only convex mesh m_Colliders are allowed
                    if (!c.convex)
                        c.convex = true;

                    // Mesh pivot should be centered or this won't work
                    Gizmos.DrawMesh(c.sharedMesh);
                    break;
                default:
                    // Nothing for capsule (DrawCapsule isn't exposed in Gizmo), terrain, wheel and
                    // other m_Colliders...
                    break;
            }
        }
    }

}
