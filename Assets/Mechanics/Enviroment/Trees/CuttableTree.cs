using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
public class CuttableTree : MonoBehaviour
{

    public MeshFilter meshFilter;

    public float cutHeight = 1;
    public float cutSize = 0.2f;

    public Vector3 cutCenter;

    [Range(0, 1)]
    public float testCutIntensity = 0.5f;
    [System.Serializable]
    public struct CutVector
    {
        [Range(0, 2 * Mathf.PI)]
        public float angle;
        public float intensity;
    }
    public CutVector cutVector;

    public void CutTree(Vector2 hitDirection)
    {
        if (!meshFilter.sharedMesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");
        Mesh clone = Instantiate(meshFilter.sharedMesh);

        print(clone.vertices[0]);


        print("Hit Tree");
    }
    [MyBox.ButtonMethod]
    public void FindCenterPoint()
    {
        List<(int start, int end)> cutRing = new List<(int, int)>();
        float halfCutSize = cutSize / 2;

        Vector3[] verts = meshFilter.sharedMesh.vertices;
        int[] tris = meshFilter.sharedMesh.triangles;

        void TestEdge(int startVert, int endVert)
        {
            //Start below, end above
            if (verts[startVert].y < cutHeight - halfCutSize &&
                verts[endVert].y > cutHeight + halfCutSize)
            {
                cutRing.Add((startVert, endVert));
            }
            //End below, start above
            else if (verts[endVert].y < cutHeight - halfCutSize &&
                    verts[startVert].y > cutHeight + halfCutSize)
            {
                cutRing.Add((endVert, startVert));
            }
        }
        for (int i = 0; i < tris.Length; i += 3)
        {
            TestEdge(tris[i], tris[i + 1]);
            TestEdge(tris[i], tris[i + 2]);
            TestEdge(tris[i + 2], tris[i + 1]);
        }

        cutCenter = Vector3.zero;
        for (int i = 0; i < cutRing.Count; i++)
        {
            //Lerp on the y axis
            Vector3 bottomCut = Vector3.Lerp(
                verts[cutRing[i].start],
                verts[cutRing[i].end],
                Mathf.InverseLerp(verts[cutRing[i].start].y, verts[cutRing[i].end].y, cutHeight - halfCutSize));

            Vector3 topCut = Vector3.Lerp(
                 verts[cutRing[i].start],
                 verts[cutRing[i].end],
                 Mathf.InverseLerp(verts[cutRing[i].start].y, verts[cutRing[i].end].y, cutHeight + halfCutSize));

            cutCenter += bottomCut + topCut;
        }

        //Find average center position
        cutCenter /= cutRing.Count * 2;
    }


    struct CutTriangle
    {
        public Vector3[] points;
        public int[] triangles;

        static Vector3 CutLine(Vector3 start, Vector3 end, float height) => Vector3.Lerp(start, end, Mathf.InverseLerp(start.y, end.y, height));

        public Mesh mesh;

        public CutTriangle(Vector3 p1, Vector3 p2, Vector3 p3, float bottomCut, float topCut, float leftIntensity, float rightIntensity, Vector3 cutCenter, bool pointAtTop)
        {
            mesh = new Mesh();
            //No Indent
            if (leftIntensity == 0 && leftIntensity == 0)
            {
                points = new Vector3[] { p1, p2, p3 };
                if (pointAtTop)
                    triangles = new int[] { 1, 0, 2 };
                else
                    triangles = new int[] { 0, 1, 2 };
            }//Indent
            else
            {
                points = new Vector3[15];
                triangles = new int[21] {
                11,0,12,
                9,10,8,
                10,14,8,
                5,7,6,
                7,13,6,
                1,3,4,
                1,4,2
            };

                points[0] = p1;
                points[1] = p2;
                points[2] = p3;

                //3 and 4 are bottom cuts
                if (pointAtTop)
                {
                    points[3] = CutLine(points[0], points[1], bottomCut);
                    points[4] = CutLine(points[0], points[2], bottomCut);

                    points[9] = CutLine(points[0], points[1], topCut);
                    points[10] = CutLine(points[0], points[2], topCut);
                }
                else
                {
                    (points[1], points[2]) = (points[2], points[1]);

                    points[3] = CutLine(points[0], points[1], topCut);
                    points[4] = CutLine(points[0], points[2], topCut);

                    points[9] = CutLine(points[0], points[1], bottomCut);
                    points[10] = CutLine(points[0], points[2], bottomCut);
                }
                //Duplicate points
                points[5] = points[3];
                points[6] = points[4];
                points[11] = points[9];
                points[12] = points[10];

                //Place the center points
                //Left inner center
                points[7] = Vector3.Lerp(Vector3.Lerp(points[3], points[9], 0.5f), cutCenter, leftIntensity);
                points[8] = points[7];
                //right inner center
                points[13] = Vector3.Lerp(Vector3.Lerp(points[4], points[10], 0.5f), cutCenter, rightIntensity);
                points[14] = points[13];

            }


            mesh.vertices = points;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.UploadMeshData(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (!meshFilter.sharedMesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");


        Mesh clone = meshFilter.sharedMesh;
        Vector3[] verts = clone.vertices;
        int[] tris = clone.triangles;


        Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.left * 2, transform.rotation, transform.lossyScale);

        // Gizmos.DrawMesh(clone);

        //DestroyImmediate(clone);

        //Find the edges that will be cut by the cutting
        Profiler.BeginSample("Find Cut Lines");

        List<CutTriangle> cutTriangles = new List<CutTriangle>();

        Vector3 cutDirection = new Vector3(Mathf.Sin(cutVector.angle), 0, Mathf.Cos(cutVector.angle));


        Gizmos.DrawLine(cutCenter, cutCenter + cutDirection * cutVector.intensity);

        float halfCutSize = cutSize / 2;

        (bool hit, bool pointUp) TestEdge(int startVert, int endVert)
        {
            //Start below, end above
            if (verts[startVert].y < cutHeight - halfCutSize &&
                verts[endVert].y > cutHeight + halfCutSize)
            {
                return (true, false);
            }
            //End below, start above
            else if (verts[endVert].y < cutHeight - halfCutSize &&
                    verts[startVert].y > cutHeight + halfCutSize)
            {
                return (true, true);
            }
            return (false, false);
        }

        for (int i = 0; i < tris.Length; i += 3)
        {
            var t1 = TestEdge(tris[i], tris[i + 1]);
            var t2 = TestEdge(tris[i], tris[i + 2]);
            var t3 = TestEdge(tris[i + 2], tris[i + 1]);

            //Find the left and right normals
            int firstVert = -1;
            int secondVert = -1;
            int thirdVert = -1;
            bool pointAtTop;

            if (t1.hit && t2.hit)
            {
                firstVert = tris[i];
                secondVert = tris[i + 1];
                thirdVert = tris[i + 2];
                pointAtTop = t1.pointUp;
            }
            if (t3.hit && t2.hit)
            {
                firstVert = tris[i + 2];
                secondVert = tris[i + 1];
                thirdVert = tris[i];
                pointAtTop = t2.pointUp;
            }
            if (t1.hit && t3.hit)
            {
                firstVert = tris[i + 1];
                secondVert = tris[i];
                thirdVert = tris[i + 2];
                pointAtTop = t1.pointUp;
            }

            if (t1.hit || t2.hit || t3.hit) //Add a triangle to be cut
                cutTriangles.Add(new CutTriangle(
                                verts[firstVert],
                                verts[secondVert],
                                verts[thirdVert],
                                cutHeight - halfCutSize,
                                cutHeight + halfCutSize,
                                testCutIntensity,
                                 testCutIntensity,
                                  cutCenter,
                                   t1.pointUp));
        }
        //Cut the edges


        Profiler.EndSample();





        //Draw the triangles
        for (int i = 0; i < cutTriangles.Count; i++)
        {
            Gizmos.DrawMesh(cutTriangles[i].mesh);
            Gizmos.DrawWireMesh(cutTriangles[i].mesh);
        }



    }
}
