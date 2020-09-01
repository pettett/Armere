using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
[ExecuteAlways]
public class CuttableTree : MonoBehaviour
{
    public MeshFilter meshFilter;
    public Mesh originalMesh;



    public float cutHeight = 1;
    public float cutSize = 0.2f;

    public Vector3 cutCenter;
    public bool mergeFaces = false;

    public float subdivisionScalar = 5;
    [Range(2, 5)]
    public int minSubdivisions = 2;
    [Range(2, 5)]
    public int maxSubdivisions = 3;
    public TriangleCutMode testCutMode;


    [System.Serializable]
    public struct CutVector
    {
        [Range(0, 2 * Mathf.PI)]
        public float angle;
        public float intensity;

        public CutVector(float angle, float intensity)
        {
            this.angle = angle;
            this.intensity = intensity;
        }
    }
    public List<CutVector> activeCutVectors = new List<CutVector>();

    public bool gizmos = false;
    public void CutTree(Vector3 hitPoint)
    {
        if (!originalMesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");

        print("Hit Tree");
        activeCutVectors.Add(new CutVector(Vector3.SignedAngle(Vector3.forward, (hitPoint - transform.position).normalized, Vector3.up) * Mathf.Deg2Rad, 0.2f));
        UpdateMeshFilter();
    }

    Triangle[] cylinderTriangles;



    [MyBox.ButtonMethod]
    public void FindCenterPoint()
    {
        List<(int start, int end)> cutRing = new List<(int, int)>();
        float halfCutSize = cutSize / 2;

        Vector3[] verts = originalMesh.vertices;
        int[] tris = originalMesh.triangles;

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

    public readonly struct Triangle
    {

        public readonly int a;
        public readonly int b;
        public readonly int c;
        public readonly int index;
        public readonly int spawnCase;
        public readonly bool pointingUpwards;

        public Triangle(int a, int b, int c, int index, bool pointingUpwards, int spawnCase)
        {

            this.a = a;
            this.b = b;
            this.c = c;
            this.index = index;
            this.spawnCase = spawnCase;
            this.pointingUpwards = pointingUpwards;
        }
    }

    public enum TriangleCutMode { Full, Top, Base }


    static float InverseHeightLerp(Vector3 start, Vector3 end, float height) => Mathf.InverseLerp(start.y, end.y, height);

    public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c) => Vector3.Cross(b - a, c - a);

    void CreateLineCut(int offset, int a, int other, float bottom, float top, float intensity,
                        int subDivisions, Vector3 cutCenter, Vector3[] meshVerts, Vector3[] meshNormals,
                        Vector2[] meshUV, TriangleCutMode cutMode, bool pointingUp)
    {
        float bottomCut = InverseHeightLerp(meshVerts[a], meshVerts[other], bottom);
        float topCut = InverseHeightLerp(meshVerts[a], meshVerts[other], top);


        //Top vertices
        int verticesCount = cutMode == TriangleCutMode.Full ? subDivisions : subDivisions * 2;
        float lerpOffset = cutMode == TriangleCutMode.Top ? 0.5f : 0;

        for (int i = 0; i <= subDivisions; i++)
        {
            float progress = i / (float)(verticesCount) + lerpOffset;
            float t = Mathf.Lerp(bottomCut, topCut, progress);
            float depth = (1 - Mathf.Abs(progress - 0.5f) * 2) * intensity;
            //Every set of 2 points is identical as they do not share normals
            meshVerts[offset + i * 2] = meshVerts[offset + i * 2 + 1] = Vector3.Lerp(Vector3.Lerp(meshVerts[a], meshVerts[other], t), cutCenter, depth);
        }

        if (cutMode == TriangleCutMode.Base || cutMode == TriangleCutMode.Full)
        {
            meshNormals[offset] = Vector3.Slerp(meshNormals[a], meshNormals[other], bottom);
        }
        if (cutMode == TriangleCutMode.Top || cutMode == TriangleCutMode.Full)
        {
            meshNormals[offset + subDivisions * 2 + 1] = Vector3.Slerp(meshNormals[a], meshNormals[other], top);
        }


        Vector3 cutSurfaceCenter = Vector3.Lerp(meshVerts[a], meshVerts[other], (topCut + bottomCut) * 0.5f);

        Vector3 leftDirection = TriangleNormal(meshVerts[offset + 1], cutCenter, cutSurfaceCenter);

        for (int i = 0; i < subDivisions; i++)
        {
            meshNormals[offset + i * 2 + 1] = meshNormals[offset + i * 2 + 2] = Vector3.Cross(meshVerts[offset + i * 2 + 1] - meshVerts[offset + i * 2 + 2], leftDirection);
        }



        // meshNormals[offset + 3] = meshNormals[offset + cutLines * 2] = -Vector3.Cross(meshVerts[offset + cutLines * 2] - meshVerts[offset + 3], leftDirection);

        // meshNormals[offset] = Vector3.Slerp(meshNormals[a], meshNormals[other], bottomCut);
        // meshNormals[offset + cutLines * 2 + 1] = Vector3.Slerp(meshNormals[a], meshNormals[other], topCut);

        // meshUV[offset] = Vector2.Lerp(meshUV[a], meshUV[other], bottomCut);
        // meshUV[offset + cutLines * 2 + 1] = Vector2.Lerp(meshUV[a], meshUV[other], topCut);


    }



    public int FindFullSubdivisions(float intensity)
    {
        //Base of 2
        return Mathf.Clamp(Mathf.FloorToInt(subdivisionScalar * intensity) + minSubdivisions, minSubdivisions, maxSubdivisions);
    }
    public int FindHalfSubdivisions(float intensity)
    {
        return Mathf.CeilToInt(FindFullSubdivisions(intensity) * 0.5f);
    }

    public int FindSubdivisions(TriangleCutMode cutMode, float intensity) => cutMode == TriangleCutMode.Full ?
                                                    FindFullSubdivisions(intensity) :
                                                    FindHalfSubdivisions(intensity);


    public int LinePointCount(TriangleCutMode cutMode, float intensity) => FindSubdivisions(cutMode, intensity) * 2 + 2;



    public void CutTriangle(Triangle t, bool connectLeft, Vector3[] meshVerts, Vector3[] meshNormals, Vector2[] meshUV,
                            List<int> triangles, int triOffset, float cutHeight, float cutSize, float leftIntensity, float rightIntensity,
                            Vector3 cutCenter, TriangleCutMode cutMode)
    {
        //Cut the mesh
        int leftPointCount = LinePointCount(cutMode, leftIntensity);
        int leftSubdivisions = FindSubdivisions(cutMode, leftIntensity);
        int rightPointCount = LinePointCount(cutMode, rightIntensity);
        int rightSubdivisions = FindSubdivisions(cutMode, rightIntensity);



        int left = 0;
        int right = leftPointCount; //Right side is one line over

        int verts = connectLeft ? rightPointCount : rightPointCount + leftPointCount;
        // points = new Vector3[verts];
        // normals = new Vector3[verts];
        // uv = new Vector2[verts];


        //Calculate which parts will connect to the a b and c key vertices
        int connectA1 = t.pointingUpwards ? left + leftPointCount - 1 : right;
        int connectA2 = t.pointingUpwards ? right + rightPointCount - 1 : left;
        int connectBC1 = t.pointingUpwards ? left : right + rightPointCount - 1;
        int connectBC2 = t.pointingUpwards ? right : left + leftPointCount - 1;


        // for (int i = 0; i < verts; i++)
        // {
        //     meshUV[triOffset + i] = new Vector2(1f, 1f); //TEMP
        // }

        //Cut lines for both left and right

        // if (t.pointingUpwards) (leftIntensity, rightIntensity) = (rightIntensity, leftIntensity);

        if (!connectLeft)
        { //Create the left connection
            CreateLineCut(triOffset + left, t.a, t.pointingUpwards ? t.c : t.b, cutHeight - cutSize * leftIntensity, cutHeight + cutSize * leftIntensity,
                            leftIntensity, leftSubdivisions, cutCenter, meshVerts, meshNormals, meshUV, cutMode, t.pointingUpwards);
            CreateLineCut(triOffset + right, t.a, t.pointingUpwards ? t.b : t.c, cutHeight - cutSize * rightIntensity, cutHeight + cutSize * rightIntensity,
                            rightIntensity, rightSubdivisions, cutCenter, meshVerts, meshNormals, meshUV, cutMode, t.pointingUpwards);
        }
        else
        {
            //Create right line with no relative offset
            CreateLineCut(triOffset, t.a, t.pointingUpwards ? t.b : t.c, cutHeight - cutSize * rightIntensity,
                            cutHeight + cutSize * rightIntensity, rightIntensity, rightSubdivisions, cutCenter,
                            meshVerts, meshNormals, meshUV, cutMode, t.pointingUpwards);
        }


        if (connectLeft) //Make right start from 0
            triOffset -= right;

        //Setup triangles - add all the triangles for top and bottom parts
        //Add top triangle
        if (t.pointingUpwards && cutMode != TriangleCutMode.Base || !t.pointingUpwards && cutMode != TriangleCutMode.Top)
            triangles.AddRange(new int[3] { triOffset + connectA1, t.a, triOffset + connectA2 });

        if (!t.pointingUpwards && cutMode != TriangleCutMode.Base || t.pointingUpwards && cutMode != TriangleCutMode.Top)
            triangles.AddRange(new int[6] {
                    t.b,                triOffset+connectBC1,     triOffset+connectBC2,
                    t.b,                 t.c,   triOffset+ connectBC1
                });

        bool reverseTriangles = leftSubdivisions < rightSubdivisions;

        int addRight = reverseTriangles ? 2 : 1;
        int addLeft = reverseTriangles ? 1 : 2;

        //Add all the triangles for the subdivisions
        for (int i = 0; i < Mathf.Min(leftSubdivisions, rightSubdivisions); i++)
        {

            //Triangle 1
            triangles.Add(triOffset + left + 1 + i * 2);
            triangles.Add(triOffset + left + 2 + i * 2);
            triangles.Add(triOffset + right + addRight + i * 2);
            //Triangle 2
            triangles.Add(triOffset + left + addLeft + i * 2);
            triangles.Add(triOffset + right + 2 + i * 2);
            triangles.Add(triOffset + right + 1 + i * 2);
        }
        if (leftSubdivisions < rightSubdivisions)
        {
            //Add final additional triangle
            triangles.Add(triOffset + left + leftPointCount - 2);
            triangles.Add(triOffset + right + rightPointCount - 2);
            triangles.Add(triOffset + right + rightPointCount - 3);
        }

        if (leftSubdivisions > rightSubdivisions)
        {
            //Add final additional triangle
            triangles.Add(triOffset + left + leftPointCount - 3);
            triangles.Add(triOffset + left + leftPointCount - 2);
            triangles.Add(triOffset + right + rightPointCount - 2);
        }
    }



    [MyBox.ButtonMethod]
    public void UpdateMeshFilter()
    {
        meshFilter.sharedMesh = CreateCutMesh();
    }


    public void FindCylinderTriangles()
    {
        //Find the edges that will be cut by the cutting
        Profiler.BeginSample("Find Triangles");
        List<Vector3> verts = new List<Vector3>();
        originalMesh.GetVertices(verts);
        List<int> tris = new List<int>();
        originalMesh.GetTriangles(tris, 0);
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



        List<Triangle> cylinderTriangles = new List<Triangle>();

        for (int i = 0; i < tris.Count; i += 3)
        {
            var t1 = TestEdge(tris[i], tris[i + 1]);
            var t2 = TestEdge(tris[i], tris[i + 2]);
            var t3 = TestEdge(tris[i + 2], tris[i + 1]);

            //Find the left and right normals
            Triangle? triangle = null;

            if (t1.hit && t2.hit)
            {
                triangle = new Triangle(tris[i], tris[i + 1], tris[i + 2], i, t1.pointUp, 0);
            }
            if (t3.hit && t2.hit)
            {
                triangle = new Triangle(tris[i + 2], tris[i], tris[i + 1], i, t3.pointUp, 1);
            }
            if (t1.hit && t3.hit)
            {
                triangle = new Triangle(tris[i + 1], tris[i + 2], tris[i], i, !t1.pointUp, 2);
            }

            //First vert is the point



            if (triangle is Triangle t)
            {
                //Add a triangle to be cut
                cylinderTriangles.Add(t);
            }
        }
        Profiler.EndSample();
        Profiler.BeginSample("Sort Triangles");
        SortTrianglesCounterClockwise(ref cylinderTriangles, verts);
        Profiler.EndSample();

        this.cylinderTriangles = cylinderTriangles.ToArray();
    }



    void SortTrianglesCounterClockwise(ref List<Triangle> triangles, List<Vector3> verts)
    {
        if (triangles.Count == 0) return;

        //Sort the triangles in counterclockwise order
        List<Triangle> sortedCylinderTriangles = new List<Triangle>(triangles.Count);

        sortedCylinderTriangles.Add(triangles[0]);
        triangles.RemoveAt(0);


        int loops = 0;
        int maxLoops = triangles.Count + 1;


        while (triangles.Count != 0 && loops < maxLoops)
        {
            loops++;
            //Go through the list of unsorteds and attempt to place it at
            //the start or end of the sorted list
            for (int i = 0; i < triangles.Count; i++)
            {
                int lastIndex = sortedCylinderTriangles.Count - 1;


                bool triUp = triangles[i].pointingUpwards;
                bool lastSortedUp = sortedCylinderTriangles[lastIndex].pointingUpwards;


                Triangle lastTri = sortedCylinderTriangles[lastIndex];

                //To be placed at the end of the list, the last tri has to be to the left of this tri
                //Connected by an entire edge section

                if (triUp && lastSortedUp && verts[triangles[i].a] == verts[lastTri.a] && verts[triangles[i].c] == verts[lastTri.b] || //UP-UP
                    triUp && !lastSortedUp && verts[triangles[i].c] == verts[lastTri.a] && verts[triangles[i].a] == verts[lastTri.c] || //UP-DOWN
                    !triUp && lastSortedUp && verts[triangles[i].b] == verts[lastTri.a] && verts[triangles[i].a] == verts[lastTri.b] || //DOWN-UP
                    !triUp && !lastSortedUp && verts[triangles[i].a] == verts[lastTri.a] && verts[triangles[i].b] == verts[lastTri.c]  //DOWN-DOWN 
                    )
                {
                    sortedCylinderTriangles.Add(triangles[i]);
                    triangles.RemoveAt(i);
                }
                //Do not bother searching for triangles attached to the front, adding to the start of the list takes too long
            }
        }

        if (loops == maxLoops)
        {
            Debug.LogErrorFormat(gameObject, "Sort reached limit, missing {0} triangles", triangles.Count);
            //sortedCylinderTriangles.AddRange(triangles);
            // sortedCylinderTriangleIndexes.AddRange(indices);
        }

        triangles = sortedCylinderTriangles;
    }



    public Mesh CreateCutMesh(System.Action<Vector3, string> label = null, System.Action<Vector3, Vector3> line = null)
    {
        if (!originalMesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");


        Profiler.BeginSample("Load Mesh");
        //Get data from deep inside the unity c++ core. spooky
        List<Vector3> verts = new List<Vector3>();
        originalMesh.GetVertices(verts);
        List<Vector3> normals = new List<Vector3>();
        originalMesh.GetNormals(normals);
        List<int> tris = new List<int>();
        originalMesh.GetTriangles(tris, 0);
        List<Vector2> uv = new List<Vector2>();
        originalMesh.GetUVs(0, uv);



        Profiler.EndSample();


        //DEBUG - draw triangle verts
        // for (int i = 0; i < cylinderTriangles.Count; i++)
        // {
        //     Vector3 avg = (verts[cylinderTriangles[i].a] + verts[cylinderTriangles[i].b] + verts[cylinderTriangles[i].c]) / 3f;

        //     label?.Invoke(Vector3.Lerp(avg, verts[cylinderTriangles[i].a], 0.9f) + transform.position, "a");
        //     label?.Invoke(Vector3.Lerp(avg, verts[cylinderTriangles[i].b], 0.9f) + transform.position, "b");
        //     label?.Invoke(Vector3.Lerp(avg, verts[cylinderTriangles[i].c], 0.9f) + transform.position, "c");

        //     label?.Invoke(avg + transform.position + Vector3.down * 0.1f, string.Format("up:{0} {1}", cylinderTriangles[i].pointingUpwards, cylinderTriangles[i].spawnCase));
        // }




        Profiler.BeginSample("Create weights");

        Vector3 TriangleNormal(Triangle t, bool left)
        {
            if (!t.pointingUpwards) left = !left;
            int other = left ? t.c : t.b;
            return Vector3.Slerp(normals[t.a], normals[other], Mathf.InverseLerp(verts[t.a].y, verts[other].y, cutHeight));
        }
        if (cylinderTriangles == null)
            FindCylinderTriangles();



        //Calculate all the intensities for the triangles
        float[] cutIntensities = new float[cylinderTriangles.Length];
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            Vector3 rightNormal = TriangleNormal(cylinderTriangles[i], false);
            for (int j = 0; j < activeCutVectors.Count; j++)
            {
                Vector3 cutDirection = new Vector3(Mathf.Sin(activeCutVectors[j].angle), 0, Mathf.Cos(activeCutVectors[j].angle));
                cutIntensities[i] += Mathf.Clamp01(Vector3.Dot(rightNormal, cutDirection)) * activeCutVectors[j].intensity;
            }
        }
        int additionalVertices = 0;
        int cutTrianglesCount = 0;
        bool chainToLeft = false;
        //And calculate the number of additional vertices that will be required
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            int leftTriangle = i - 1;
            if (leftTriangle == -1) leftTriangle = cylinderTriangles.Length - 1;
            if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0))
            {
                additionalVertices += chainToLeft ? LinePointCount(testCutMode, cutIntensities[i]) :
                                                    LinePointCount(testCutMode, cutIntensities[leftTriangle]) + LinePointCount(testCutMode, cutIntensities[i]);
                cutTrianglesCount++;
                chainToLeft = mergeFaces;
            }
            else
            {
                chainToLeft = false;
            }
        }

        //DEBUG - draw sorted indexes
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            Vector3 avg = (verts[cylinderTriangles[i].a] + verts[cylinderTriangles[i].b] + verts[cylinderTriangles[i].c]) / 3f + transform.position;
            label?.Invoke(avg, string.Format("t:{0} up:{1}", i, cylinderTriangles[i].pointingUpwards));
        }




        int totalVertices = verts.Count + additionalVertices;
        Profiler.EndSample();
        Profiler.BeginSample("Create Vertex array");
        //Add all these triangles to the new mesh

        Vector3[] newVertices = new Vector3[totalVertices];
        Vector3[] newNormals = new Vector3[totalVertices];
        Vector2[] newUVs = new Vector2[totalVertices];

        verts.CopyTo(newVertices);
        normals.CopyTo(newNormals);
        uv.CopyTo(newUVs);

        Profiler.EndSample();

        Profiler.BeginSample("Cut Triangles");
        //Cut the edges

        chainToLeft = false;
        float halfCutSize = cutSize / 2;
        int triangleOffset = totalVertices - additionalVertices;
        int[] triangleIndices = cylinderTriangles.Select(x => x.index).ToArray();

        //Use this data to finally create the cuts
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            Triangle t = cylinderTriangles[i];
            //Blend between triangles on the left (-1) and the right (+1)
            int leftTriangle = i - 1;
            if (leftTriangle == -1) leftTriangle = cylinderTriangles.Length - 1;

            if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0))
            {
                CutTriangle(t, chainToLeft, newVertices, newNormals, newUVs,
                            tris, triangleOffset, cutHeight, halfCutSize,
                            cutIntensities[leftTriangle], cutIntensities[i], cutCenter, testCutMode);


                //Remove this triangle from the mesh
                tris.RemoveRange(triangleIndices[i], 3);
                for (int otherTriIndex = 0; otherTriIndex < triangleIndices.Length; otherTriIndex++)
                {
                    if (triangleIndices[otherTriIndex] > triangleIndices[i]) triangleIndices[otherTriIndex] -= 3;
                }

                //Track number of total verts (again)
                triangleOffset += chainToLeft ? LinePointCount(testCutMode, cutIntensities[i]) : LinePointCount(testCutMode, cutIntensities[leftTriangle]) + LinePointCount(testCutMode, cutIntensities[i]);
                //This triangle will be the first in a chain sharing vertices

                chainToLeft = mergeFaces;
            }
            else
            {
                chainToLeft = false;
            }
            //rightIntensity = leftIntensity;
        }

        Profiler.EndSample();

        Mesh cutMesh = new Mesh();
        //Before locking in the vert counts for the cutting, remove all vertices not encompassed by the cut mode
        if (testCutMode != TriangleCutMode.Full)
        {
            Profiler.BeginSample("Remove tall vertices");
            List<Vector3> cutVerts = newVertices.ToList();
            List<Vector3> cutNormals = newNormals.ToList();
            List<Vector2> cutUVs = newUVs.ToList();


            for (int i = cutVerts.Count - 1; i >= 0; i--)
            {
                if (testCutMode == TriangleCutMode.Base && cutVerts[i].y > cutHeight + 0.05f || testCutMode == TriangleCutMode.Top && cutVerts[i].y + 0.05f < cutHeight)
                {
                    cutVerts.RemoveAt(i);
                    cutNormals.RemoveAt(i);
                    cutUVs.RemoveAt(i);

                    for (int t = 0; t < tris.Count; t += 3)
                    {
                        //Remove all triangles with this vert
                        if (tris[t] == i || tris[t + 1] == i || tris[t + 2] == i)
                        {
                            tris.RemoveRange(t, 3);
                            t -= 3;
                        }
                        else
                        {
                            if (tris[t] > i) tris[t]--;
                            if (tris[t + 1] > i) tris[t + 1]--;
                            if (tris[t + 2] > i) tris[t + 2]--;
                        }
                    }
                    i++;
                }
            }

            Profiler.EndSample();
            Profiler.BeginSample("Create Mesh");
            cutMesh.SetVertices(cutVerts);
            cutMesh.SetNormals(cutNormals);
            cutMesh.SetUVs(0, cutUVs);
        }
        else
        {
            Profiler.BeginSample("Create Mesh");
            cutMesh.SetVertices(newVertices);
            cutMesh.SetNormals(newNormals);
            cutMesh.SetUVs(0, newUVs);
        }

        cutMesh.SetTriangles(tris, 0);

        cutMesh.UploadMeshData(true);
        Profiler.EndSample();







        return cutMesh;
    }

    private void OnDrawGizmos()
    {
        if (!gizmos) return;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        //Find the edges that will be cut by the cutting

        Mesh mesh = CreateCutMesh(UnityEditor.Handles.Label);
        Gizmos.DrawWireMesh(mesh);


    }
}
