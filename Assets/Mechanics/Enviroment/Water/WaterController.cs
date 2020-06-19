
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    [System.Serializable]
    public class WaterPathNode
    {
        public Transform transform;
        public float waterWidth = 5;
    }
    public WaterPathNode[] path = new WaterPathNode[0];
    Vector2 Flatten(Vector3 p)
    {
        return new Vector2(p.x, p.z);
    }
    Vector2 Normal(Vector2 p1, Vector2 p2)
    {
        return Vector2.Perpendicular(p1 - p2).normalized;
    }
    Vector2 PointNormal(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (Normal(p1, p2) + Normal(p2, p3)).normalized;
    }
    Vector3 Extrude(int i, float e)
    {
        Vector3 start = path[i].transform.position;
        Vector2 n;
        if (i == 0)
        {
            n = Normal(
                Flatten(path[i].transform.position),
                Flatten(path[i + 1].transform.position));
        }
        else if (i == path.Length - 1)
        {
            n = Normal(
                Flatten(path[i - 1].transform.position),
                Flatten(path[i].transform.position));
        }
        else
        {
            n = PointNormal(
                Flatten(path[i - 1].transform.position),
                Flatten(path[i].transform.position),
                Flatten(path[i + 1].transform.position));
        }
        return start + new Vector3(n.x, 0, n.y) * e;
    }

    public Vector3[] GetExtrudedPath(float multiplier)
    {
        Vector3[] p = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
            p[i] = Extrude(i, path[i].waterWidth * multiplier);
        return p;
    }
    public Vector3[] GetPath()
    {
        Vector3[] p = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
            p[i] = path[i].transform.position;
        return p;
    }

    private void OnDrawGizmosSelected()
    {
        var l = GetExtrudedPath(1);
        var r = GetExtrudedPath(-1);
        Gizmos.color = Color.blue;
        for (int i = 0; i < path.Length - 1; i++)
        {
            Gizmos.DrawLine(l[i], l[i + 1]);
            Gizmos.DrawLine(r[i], r[i + 1]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IWaterObject>()?.OnWaterEnter(this);
    }
    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<IWaterObject>()?.OnWaterExit(this);
    }

}