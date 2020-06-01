using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(WaterController))]
public class WaterAudioController : MonoBehaviour
{
    public AudioSource source;
    public AudioListener listener;
    WaterController c;
    private void Start()
    {
        listener = FindObjectOfType<AudioListener>();
        c = GetComponent<WaterController>();
    }
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    public Vector3 ClosestPointOnPath(Vector3[] path, Vector3 point)
    {
        Vector3 closestPoint = default;
        float closestSqrDistance = Mathf.Infinity;
        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 pos = Vector3.Lerp(path[i], path[i + 1], InverseLerp(path[i], path[i + 1], point));
            if ((pos - point).sqrMagnitude < closestSqrDistance)
            {
                closestSqrDistance = (pos - point).sqrMagnitude;
                closestPoint = pos;
            }
        }

        return closestPoint;
    }

    private void Update()
    {
        //Find the closest point on the path to the listener
        Vector3 rightPath = ClosestPointOnPath(c.GetExtrudedPath(-1), listener.transform.position);
        Vector3 leftPath = ClosestPointOnPath(c.GetExtrudedPath(1), listener.transform.position);
        Vector3 closest = Vector3.Lerp(rightPath, leftPath, InverseLerp(rightPath, leftPath, listener.transform.position));
        source.transform.position = closest;
    }
}
