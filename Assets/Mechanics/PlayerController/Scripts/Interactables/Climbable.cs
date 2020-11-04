using System.Collections;
using System.Collections.Generic;
using Armere.PlayerController;
using UnityEngine;
using System.Linq;
public class Climbable : MonoBehaviour, IInteractable
{
    public enum ClimbableSurface { Line, Mesh }

    public ClimbableSurface surfaceType;



    [MyBox.ConditionalField("surfaceType", false, ClimbableSurface.Line)] public float rungDistance = 0.25f;
    [MyBox.ConditionalField("surfaceType", false, ClimbableSurface.Line)] public float rungOffset = 0.1f;
    [MyBox.ConditionalField("surfaceType", false, ClimbableSurface.Line)] public float ladderHeight = 12;

    [MyBox.ConditionalField("surfaceType", false, ClimbableSurface.Mesh)] public MeshFilter mesh;
    [MyBox.ConditionalField("surfaceType", false, ClimbableSurface.Mesh)] public Vector3 localTestPos;

    public bool canInteract { get => enabled; set => enabled = value; }
    [Range(0, 360)]
    public float requiredLookAngle = 180;
    public float requiredLookDot => Mathf.Cos(requiredLookAngle);

    public string interactionDescription => "Climb";

    Vector3[] vertices;
    Vector3[] normals;
    int[] triangles;

    public void SyncMesh(MeshFilter mesh)
    {
        var vertices = mesh.sharedMesh.vertices.ToList();
        var normals = mesh.sharedMesh.normals.ToList();
        triangles = mesh.sharedMesh.triangles;
        //filter out repeats that exist for some reason
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = 0; j < vertices.Count; j++)
            {
                if (i != j && vertices[i] == vertices[j])
                {
                    vertices.RemoveAt(j);
                    normals.RemoveAt(j);
                    for (int index = 0; index < triangles.Length; index++)
                        if (triangles[index] == j) triangles[index] = i;//Replace 
                        else if (triangles[index] > j) triangles[index]--;//above this, shuffle down
                }
            }
        }
        this.vertices = vertices.ToArray();
        this.normals = normals.ToArray();
    }

    public void Interact(IInteractor c)
    {
        //Change to state ladder

        SyncMesh(mesh);

        //Maybe change this to happen inside the player controller?
        //Yes past me, that sounds like a good idea for later-er. But instead Im going to expand this to include climbable walls
        //Okay i did it but badly

        //(c as PlayerController.Player_CharacterController).ChangeToState<LadderClimb>(this);
    }

    public Vector3 LadderPosAtHeight(float height, float radius)
    {
        return transform.position - transform.forward * radius + Vector3.up * height;
    }

    public Vector3 LadderPosByRung(float rung, float right)
    {
        return transform.position + Vector3.up * (rung * rungDistance + rungOffset) + transform.right * right;
    }

    //Ladder is not highlighted
    public void OnEndHighlight()
    {

    }

    public void OnStartHighlight()
    {


    }

    public static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
    public struct ClosestPointData
    {
        public Vector3 point;
        public Vector3 normal;
        public int closestVert;
    }

    Vector3 GetBarycentricCoords(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var dX = p.x - p2.x;
        var dY = p.y - p2.y;
        var dX21 = p2.x - p1.x;
        var dY12 = p1.y - p2.y;
        var D = dY12 * (p0.x - p2.x) + dX21 * (p0.y - p2.y);
        var s = dY12 * dX + dX21 * dY;
        var t = (p2.y - p0.y) * dX + (p0.x - p2.x) * dY;
        return new Vector3(s, t, D);

    }
    bool InsideTri(Vector3 barycentricCoords)
    {
        var D = barycentricCoords.z;
        var s = barycentricCoords.x;
        var t = barycentricCoords.y;
        if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
        else return s >= 0 && t >= 0 && s + t <= D;
    }


    ///<summary>Operates in local space </summary>
    public ClosestPointData GetClosestPointOnMesh(Vector3 point)
    {
        if (vertices.Length < 3) throw new System.Exception("Climbable has no mesh loaded");

        int closestVert = 0;
        float sqrDist;
        Vector3 localPoint = mesh.transform.InverseTransformPoint(point);
        float bestSqrDistance = (vertices[closestVert] - localPoint).sqrMagnitude;

        //Dont need to check first vertex
        for (int i = 1; i < vertices.Length; i++)
        {
            sqrDist = (vertices[i] - localPoint).sqrMagnitude;
            if (sqrDist < bestSqrDistance)
            {
                bestSqrDistance = sqrDist;
                closestVert = i;
            }
        }
        //
        //Gizmos.DrawWireCube(mesh.sharedMesh.bounds.center, mesh.sharedMesh.bounds.size);
        //Gizmos.DrawWireSphere(vertices[closestVert], 0.05f);

        //Find triangles containing closest vertex
        List<int> tris = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Debug.LogFormat("{0} {1} {2} Search {3}", mesh.sharedMesh.triangles[i], mesh.sharedMesh.triangles[i + 1], mesh.sharedMesh.triangles[i + 2], closestVert);
            if (triangles[i] == closestVert || triangles[i + 1] == closestVert || triangles[i + 2] == closestVert)
                tris.Add(i);
        }

        Vector3 closestPointOnMesh = Vector3.zero;
        Vector3 closestNormalOnMesh = Vector3.zero;
        bestSqrDistance = Mathf.Infinity;

        void TestPoint(Vector3 testPoint, Vector3 normal)
        {
            sqrDist = (localPoint - testPoint).sqrMagnitude;
            if (sqrDist < bestSqrDistance)
            {
                bestSqrDistance = sqrDist;
                closestPointOnMesh = testPoint;
                closestNormalOnMesh = normal;
            }
        }


        for (int i = 0; i < tris.Count; i++)
        {
            int aIndex = triangles[tris[i]];
            int bIndex = triangles[tris[i] + 2];
            int cIndex = triangles[tris[i] + 1];

            Vector3 a = vertices[aIndex];
            Vector3 b = vertices[bIndex];
            Vector3 c = vertices[cIndex];

            Plane p = new Plane(a, b, c);

            Vector3 closestPoint = p.ClosestPointOnPlane(localPoint);
            //Test if this point is within the triangle

            Vector2 A;
            Vector2 B;
            Vector2 C;
            Vector2 P;
            //Project triangle onto 2D plane


            if (Mathf.Abs(p.normal.x) > Mathf.Abs(p.normal.y))
            {
                if (Mathf.Abs(p.normal.x) > Mathf.Abs(p.normal.z))
                {

                    //YZ Plane
                    A = new Vector2(a.y, a.z);
                    B = new Vector2(b.y, b.z);
                    C = new Vector2(c.y, c.z);
                    P = new Vector2(closestPoint.y, closestPoint.z);

                }
                else
                {
                    //XY Plane
                    A = new Vector2(a.x, a.y);
                    B = new Vector2(b.x, b.y);
                    C = new Vector2(c.x, c.y);
                    P = new Vector2(closestPoint.x, closestPoint.y);
                }
            }
            else
            {
                if (Mathf.Abs(p.normal.y) > Mathf.Abs(p.normal.z))
                {

                    //XZ Plane
                    A = new Vector2(a.x, a.z);
                    B = new Vector2(b.x, b.z);
                    C = new Vector2(c.x, c.z);
                    P = new Vector2(closestPoint.x, closestPoint.z);
                }
                else
                {

                    //XY Plane
                    A = new Vector2(a.x, a.y);
                    B = new Vector2(b.x, b.y);
                    C = new Vector2(c.x, c.y);
                    P = new Vector2(closestPoint.x, closestPoint.y);
                }
            }

            Vector3 bary = GetBarycentricCoords(P, A, B, C);

            Vector3 na = normals[aIndex];
            Vector3 nb = normals[bIndex];
            Vector3 nc = normals[cIndex];

            if (InsideTri(bary))
            {
                //Point is within triangle
                //Use barycentric coords to lerp to normal
                float s = bary.x / bary.z;
                float t = bary.y / bary.z;
                if (bary.z > 0)
                    TestPoint(closestPoint, Vector3.Slerp(Vector3.Slerp(na, nb, s), nc, t));
                else
                    TestPoint(closestPoint, Vector3.Slerp(Vector3.Slerp(nc, nb, t), na, s));
            }
            else
            {
                //Also test all the lines on the triangle
                //Should not bother testing the line not attached to the closest point
                //Nope that was wrong


                float t1 = InverseLerp(a, b, localPoint);
                float t2 = InverseLerp(b, c, localPoint);
                float t3 = InverseLerp(c, a, localPoint);


                TestPoint(Vector3.Lerp(a, b, t1), Vector3.Slerp(na, nb, t1));
                TestPoint(Vector3.Lerp(b, c, t2), Vector3.Slerp(nb, nc, t2));
                TestPoint(Vector3.Lerp(c, a, t3), Vector3.Slerp(nc, na, t3));
            }
        }

        return new ClosestPointData
        {
            point = mesh.transform.TransformPoint(closestPointOnMesh),
            normal = mesh.transform.TransformDirection(closestNormalOnMesh),
            closestVert = closestVert,
        };
    }

    private void OnDrawGizmosSelected()
    {
        if (surfaceType == ClimbableSurface.Line)
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * ladderHeight);
            //Draw all the rungs
            int rungCount = Mathf.FloorToInt(ladderHeight / rungDistance);
            for (int i = 0; i < rungCount; i++)
            {
                Vector3 h = Vector3.up * (rungDistance * i + rungOffset);
                Gizmos.DrawLine(transform.position + transform.right * 0.2f + h, transform.position - transform.right * 0.2f + h);
            }
        }
        else if (surfaceType == ClimbableSurface.Mesh)
        {

            var closestPoint = GetClosestPointOnMesh(localTestPos);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(closestPoint.point, 0.05f);
            Gizmos.DrawLine(closestPoint.point, closestPoint.point + closestPoint.normal * 0.25f);


            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(vertices[closestPoint.closestVert], 0.05f);


            for (int i = 0; i < triangles.Length; i += 3)
            {
                Gizmos.DrawLine(vertices[triangles[i]], vertices[triangles[i + 1]]);
                Gizmos.DrawLine(vertices[triangles[i + 2]], vertices[triangles[i + 1]]);
                Gizmos.DrawLine(vertices[triangles[i]], vertices[triangles[i + 2]]);
            }
        }
    }
}
