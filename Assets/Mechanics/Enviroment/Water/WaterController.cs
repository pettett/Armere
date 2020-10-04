
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.AddressableAssets;

public class WaterController : MonoBehaviour
{
    [System.Serializable]
    public class WaterPathNode
    {
        public Transform transform;
        public float waterWidth = 5;
    }
    public WaterPathNode[] path = new WaterPathNode[0];
    public static VisualEffect splashEffect = null;
    public AssetReferenceGameObject splashEffectPrefab = new AssetReferenceGameObject("Enviroment/Water/WaterSplash");

    public Collider waterVolume;

    public float density = 1000f;

    public Bounds bounds => waterVolume.bounds;
    private async void Start()
    {
        if (splashEffect == null)
        {
            splashEffect = (await Addressables.InstantiateAsync(splashEffectPrefab).Task).GetComponent<VisualEffect>();
        }
    }

    static void DrawCross(Vector3 center, float size)
    {
        const float time = 2f;
        Debug.DrawLine(center + Vector3.up * size, center - Vector3.up * size, Color.green, time);
        Debug.DrawLine(center + Vector3.right * size, center - Vector3.right * size, Color.green, time);
        Debug.DrawLine(center + Vector3.forward * size, center - Vector3.forward * size, Color.green, time);
    }

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<IWaterObject>()?.OnWaterEnter(this);

        CreateSplash(GetCollisionPosition(other.transform.position));

    }


    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<IWaterObject>()?.OnWaterExit(this);

        CreateSplash(GetCollisionPosition(other.transform.position));
    }
    public Vector3 GetSurfacePosition(Vector3 inWaterPosition)
    {
        return waterVolume.ClosestPoint(inWaterPosition + Vector3.up * 10);
    }
    public Vector3 GetCollisionPosition(Vector3 otherCenter)
    {
        Vector3 collisionEstimate = waterVolume.ClosestPoint(otherCenter + Vector3.up);
        // collisionEstimate = other.ClosestPoint(collisionEstimate);
        //  collisionEstimate = waterVolume.ClosestPoint(collisionEstimate);
        DrawCross(collisionEstimate, 0.1f);
        return collisionEstimate;
    }
    public void CreateSplash(Vector3 position, float size = 1)
    {
        position.y += 0.03f;
        if (splashEffect != null)
        {
            splashEffect.transform.position = position;

            splashEffect.SetFloat("Splash Size", size);
            splashEffect.SendEvent("Splash");
        }
    }



    static Vector2 Flatten(Vector3 p)
    {
        return new Vector2(p.x, p.z);
    }
    static Vector2 Normal(Vector2 p1, Vector2 p2)
    {
        return Vector2.Perpendicular(p1 - p2).normalized;
    }
    static Vector2 PointNormal(Vector2 p1, Vector2 p2, Vector2 p3)
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



}