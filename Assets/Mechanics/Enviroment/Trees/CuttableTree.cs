using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Armere.AddressableTypes;

[ExecuteAlways]
public class CuttableTree : MonoBehaviour
{

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
    [System.Serializable]
    public struct Triangle
    {
        public int a;
        public int b;
        public int c;
        public int index;
        public int spawnCase;
        public bool pointingUpwards;

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

    [Header("References")]
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public AudioSource audioSource;
    public Mesh originalMesh;
    [Header("Cutting")]
    public float cutHeight = 1;
    public float cutSize = 0.2f;
    public Vector3 cutCenter;
    public bool mergeFaces = false;
    public float subdivisionScalar = 5;
    [Range(1, 5)]
    public int minSubdivisions = 2;
    [Range(1, 5)]
    public int maxSubdivisions = 3;
    [Range(0, 1)]
    public float intensityCutoff = 0.05f;
    [Range(0, 1)]
    public float minSeveredIntensity = 0.1f;
    public float bevelProfile = 1;
    public float bevelDistribution = 1;
    public List<CutVector> activeCutVectors = new List<CutVector>();
    public bool gizmos = false;
    float totalDamage = 0;
    public float damageToCut = 1;

    [Header("Log Felling")]
    public float logDensity = 700f;
    public float logEstimateHeight = 3f;
    public float logEstimateRadius = 0.15f;
    public float logKnockingForce = 70f;
    public Material logMaterial;
    public Material crosssectionMaterial;

    [Header("Texturing")]
    [Range(0, 1)]
    public float crossSectionScale = 0.9f;

    [Header("Impact")]
    public AudioClip[] impactClips;

    public Triangle[] cylinderTriangles;
    BitArray vertsAboveCut = null;
    BitArray trianglesAboveCut = null;
    BitArray trianglesBelowCut = null;



    public float LogMass => Mathf.PI * logEstimateRadius * logEstimateRadius * logEstimateHeight * logDensity;







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

    private void Start()
    {
        activeCutVectors = new List<CutVector>();
        UpdateMeshFilter(TriangleCutMode.Full);
    }



    public void CutTree(Vector3 hitPoint, Vector3 hitterPosition)
    {
        if (!originalMesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");
        if (totalDamage >= damageToCut)
        {
            return;
        }


        float intensity = 0.2f;
        Vector3 direction = (hitPoint - transform.position);
        direction.y = 0;
        direction.Normalize();

        activeCutVectors.Add(new CutVector(Vector3.SignedAngle(Vector3.forward, direction, Vector3.up) * Mathf.Deg2Rad, intensity));
        totalDamage += intensity;

        if (impactClips.Length != 0)
            audioSource.PlayOneShot(impactClips[Random.Range(0, impactClips.Length)]);


        if (totalDamage < damageToCut) UpdateMeshFilter(TriangleCutMode.Full);
        else SplitTree(hitterPosition);
    }

    public void SplitTree(Vector3 hitterPosition)
    {
        if (meshCollider == null) throw new System.Exception("Mesh collider required");

        Mesh stump = CreateCutMesh(TriangleCutMode.Base);
        meshFilter.sharedMesh = stump;
        meshCollider.sharedMesh = stump;
        Mesh trunkMesh = CreateCutMesh(TriangleCutMode.Top);
        GameObject log = new GameObject("Tree trunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Rigidbody));
        log.transform.SetPositionAndRotation(transform.position, transform.rotation);

        MeshCollider logCollider = log.GetComponent<MeshCollider>();
        Rigidbody logRB = log.GetComponent<Rigidbody>();
        MeshFilter logFilter = log.GetComponent<MeshFilter>();
        MeshRenderer logRenderer = log.GetComponent<MeshRenderer>();

        logCollider.convex = true;
        logCollider.sharedMesh = trunkMesh;
        logFilter.sharedMesh = trunkMesh;
        logRenderer.materials = new Material[] { logMaterial, crosssectionMaterial };

        //Calculate the direction the log should fall in
        Vector3 playerDirection = transform.position - hitterPosition;
        playerDirection.y = 0;
        playerDirection.Normalize();

        logRB.AddForceAtPosition(playerDirection * logKnockingForce,
                                transform.position + Vector3.up * (logEstimateHeight + cutHeight) - playerDirection * logEstimateRadius);
    }



    //Fill the bitarray with trues and false depending on if the things are above the cut lines
    public void TestForVerticesAboveCut()
    {
        Vector3[] verts = originalMesh.vertices;
        int[] tris = originalMesh.triangles;

        vertsAboveCut = new BitArray(verts.Length);
        //Triangles can be above, below or both if they span so two arrays are required
        trianglesAboveCut = new BitArray(tris.Length / 3);
        trianglesBelowCut = new BitArray(tris.Length / 3);


        for (int i = 0; i < verts.Length; i++)
        {
            if (verts[i].y > cutHeight)
            {
                vertsAboveCut[i] = true;

                for (int t = 0; t < tris.Length / 3; t += 1)
                {
                    //Remove all triangles with this vert
                    if (tris[t * 3] == i || tris[t * 3 + 1] == i || tris[t * 3 + 2] == i)
                    {
                        trianglesAboveCut[t] = true;
                    }
                }
            }
            else if (verts[i].y < cutHeight)
            {
                for (int t = 0; t < tris.Length / 3; t += 1)
                {
                    //Remove all triangles with this vert
                    if (tris[t * 3] == i || tris[t * 3 + 1] == i || tris[t * 3 + 2] == i)
                    {
                        trianglesBelowCut[t] = true;
                    }
                }
            }
        }

    }


    static float InverseHeightLerp(Vector3 start, Vector3 end, float height) => Mathf.InverseLerp(start.y, end.y, height);

    public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c) => Vector3.Cross(b - a, c - a);




    void CreateLineCut(int offset, int a, int other, float bottom, float top, float intensity,
                      int subDivisions, Vector3 cutCenter, Vector3[] meshVerts, Vector3[] meshNormals,
                      Vector2[] meshUV, TriangleCutMode cutMode, bool pointingUp)
    {
        float bottomCut = InverseHeightLerp(meshVerts[a], meshVerts[other], bottom);
        float topCut = InverseHeightLerp(meshVerts[a], meshVerts[other], top);


        float rangeBottom = cutMode == TriangleCutMode.Top ? (bottom + top) * 0.5f : bottom;
        float rangeTop = cutMode == TriangleCutMode.Base ? (bottom + top) * 0.5f : top;


        float lerpRangeBottom = cutMode == TriangleCutMode.Top ? 0.5f : 0;
        float lerpRangeSize = cutMode == TriangleCutMode.Full ? 1 : 0.5f;


        //Top vertices
        float invTotalDivisions = 1f / subDivisions;

        for (int i = 0; i <= subDivisions; i++)
        {
            //Progress in range 0 to 1
            float progress = i * invTotalDivisions * lerpRangeSize + lerpRangeBottom;
            //X should be in range of -1 to 1
            float x = progress * 2 - 1;
            //Power x to bevel distribution to space out points
            x = Mathf.Pow(Mathf.Abs(x), 1f / bevelDistribution) * Mathf.Sign(x);
            //Apply this power to the progress
            progress = (x + 1) * 0.5f;

            float depth = Mathf.Pow(1 - Mathf.Pow(Mathf.Abs(x), bevelProfile), 1f / bevelProfile) * intensity;

            Vector3 p1 = Vector3.Lerp(
                Vector3.Lerp(meshVerts[a], meshVerts[other], bottomCut),
                Vector3.Lerp(meshVerts[a], meshVerts[other], topCut), progress);
            Vector3 pos = Vector3.Lerp(p1, cutCenter, depth);

            //Every set of 2 points is identical as they do not share normals
            //Vector3 pos = Vector3.Lerp(Vector3.Lerp(meshVerts[a], meshVerts[other], t), cutCenter, depth);

            pos.y = Mathf.Clamp(pos.y, rangeBottom, rangeTop);
            meshVerts[offset + i * 2] = meshVerts[offset + i * 2 + 1] = pos;

            Vector2 dir = new Vector2(pos.x - cutCenter.x, pos.z - cutCenter.z).normalized;
            meshUV[offset + i * 2] = meshUV[offset + i * 2 + 1] = Vector2.one * 0.5f + dir * (1 - depth) * crossSectionScale * 0.5f;
        }
        //Only change these uvs if they are not supposed to be a part of a flat trunk 
        if (cutMode != TriangleCutMode.Top)
            meshUV[offset] = Vector2.Lerp(meshUV[a], meshUV[other], bottomCut);
        if (cutMode != TriangleCutMode.Base)
            meshUV[offset + subDivisions * 2 + 1] = Vector2.Lerp(meshUV[a], meshUV[other], topCut);

        if (cutMode == TriangleCutMode.Base || cutMode == TriangleCutMode.Full)
        {
            meshNormals[offset] = Vector3.Slerp(meshNormals[a], meshNormals[other], bottom);
        }
        else
        {
            meshNormals[offset] = Vector3.down;
        }

        if (cutMode == TriangleCutMode.Top || cutMode == TriangleCutMode.Full)
        {
            meshNormals[offset + subDivisions * 2 + 1] = Vector3.Slerp(meshNormals[a], meshNormals[other], top);
        }
        else
        {
            meshNormals[offset + subDivisions * 2 + 1] = Vector3.up;
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



    }



    public int FindFullSubdivisions(float intensity)
    {
        //Base of 2 - intensity of 0 should have no subdivisions
        return intensity == 0 ? 1 : Mathf.Clamp(Mathf.FloorToInt(subdivisionScalar * intensity) + minSubdivisions, minSubdivisions, maxSubdivisions);
    }

    public int FindSubdivisions(TriangleCutMode cutMode, float intensity) => cutMode == TriangleCutMode.Full ?
                                                    FindFullSubdivisions(intensity) * 2 :
                                                    FindFullSubdivisions(intensity);




    public int LinePointCount(TriangleCutMode cutMode, float intensity) => FindSubdivisions(cutMode, intensity) * 2 + 2;



    public void CutTriangle(in Triangle t, bool connectLeft, Vector3[] meshVerts, Vector3[] meshNormals, Vector2[] meshUV,
                            List<int> cutTriangles, List<int> meshTriangles, int triOffset, float cutHeight, float cutSize, float leftIntensity, float rightIntensity,
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
        {
            //Calculate which parts will connect to the a b and c key vertices
            int connectA1 = t.pointingUpwards ? left + leftPointCount - 1 : right;
            int connectA2 = t.pointingUpwards ? right + rightPointCount - 1 : left;

            meshTriangles.Add(triOffset + connectA1);
            meshTriangles.Add(t.a);
            meshTriangles.Add(triOffset + connectA2);
        }

        if (!t.pointingUpwards && cutMode != TriangleCutMode.Base || t.pointingUpwards && cutMode != TriangleCutMode.Top)
        {
            //Calculate which parts will connect to the a b and c key vertices
            int connectBC1 = t.pointingUpwards ? left : right + rightPointCount - 1;
            int connectBC2 = t.pointingUpwards ? right : left + leftPointCount - 1;

            meshTriangles.Add(t.b);
            meshTriangles.Add(triOffset + connectBC1);
            meshTriangles.Add(triOffset + connectBC2);

            meshTriangles.Add(t.b);
            meshTriangles.Add(t.c);
            meshTriangles.Add(triOffset + connectBC1);
        }

        if (cutMode == TriangleCutMode.Base)
        {
            //Add triangle to cap piece
            cutTriangles.Add(triOffset + right + rightPointCount - 1);
            cutTriangles.Add(triOffset + left + leftPointCount - 1);
            cutTriangles.Add(meshVerts.Length);
        }
        if (cutMode == TriangleCutMode.Top)
        {
            //Add triangle to cap piece
            cutTriangles.Add(triOffset + left);
            cutTriangles.Add(triOffset + right);
            cutTriangles.Add(meshVerts.Length);
        }


        bool reverseTriangles = leftSubdivisions < rightSubdivisions;

        int addRight = reverseTriangles ? 2 : 1;
        int addLeft = reverseTriangles ? 1 : 2;

        //Add all the triangles for the subdivisions
        for (int i = 0; i < Mathf.Min(leftSubdivisions, rightSubdivisions); i++)
        {

            //Triangle 1
            cutTriangles.Add(triOffset + left + 1 + i * 2);
            cutTriangles.Add(triOffset + left + 2 + i * 2);
            cutTriangles.Add(triOffset + right + addRight + i * 2);
            //Triangle 2
            cutTriangles.Add(triOffset + left + addLeft + i * 2);
            cutTriangles.Add(triOffset + right + 2 + i * 2);
            cutTriangles.Add(triOffset + right + 1 + i * 2);
        }
        if (rightSubdivisions > leftSubdivisions)
        {
            int requiredTriangles = rightSubdivisions - leftSubdivisions;
            //Add final additional triangles
            for (int i = 0; i < requiredTriangles; i++)
            {
                cutTriangles.Add(triOffset + left + leftPointCount - 2);
                cutTriangles.Add(triOffset + right + rightPointCount - 2 - i * 2);
                cutTriangles.Add(triOffset + right + rightPointCount - 3 - i * 2);
            }
        }

        else if (leftSubdivisions > rightSubdivisions)
        {
            //Add final additional triangles

            int requiredTriangles = leftSubdivisions - rightSubdivisions;
            for (int i = 0; i < requiredTriangles; i++)
            {

                cutTriangles.Add(triOffset + left + leftPointCount - 3 - i * 2);
                cutTriangles.Add(triOffset + left + leftPointCount - 2 - i * 2);
                cutTriangles.Add(triOffset + right + rightPointCount - 2);
            }
        }
    }



    public void UpdateMeshFilter(TriangleCutMode cutMode)
    {
        if (meshFilter == null) throw new System.Exception("No mesh filter set to update");
        if (activeCutVectors.Count > 0) //Cut into the mesh
        {
            meshFilter.sharedMesh = CreateCutMesh(cutMode);
            meshRenderer.materials = new Material[] { logMaterial, crosssectionMaterial };
        }
        else //Do not cut into the mesh
        {
            meshFilter.sharedMesh = originalMesh;
            meshRenderer.materials = new Material[] { logMaterial };
        }
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



    public Mesh CreateCutMesh(TriangleCutMode cutMode, System.Action<Vector3, string> label = null, System.Action<Vector3, Vector3> line = null)
    {
        if (!originalMesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");


        Profiler.BeginSample("Load Mesh");
        //Get data from deep inside the unity c++ core. spooky
        List<Vector3> verts = new List<Vector3>();
        originalMesh.GetVertices(verts);
        List<Vector3> normals = new List<Vector3>();
        originalMesh.GetNormals(normals);

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

        if (cylinderTriangles == null)
            FindCylinderTriangles();



        //Calculate all the intensities for the triangles
        float[] cutIntensities = new float[cylinderTriangles.Length];
        Vector3 rightNormal;

        Profiler.BeginSample("Find cut vectors");
        Vector3[] cutVectors = new Vector3[activeCutVectors.Count];
        for (int i = 0; i < activeCutVectors.Count; i++)
        {
            cutVectors[i] = new Vector3(Mathf.Sin(activeCutVectors[i].angle), 0, Mathf.Cos(activeCutVectors[i].angle)) * activeCutVectors[i].intensity;
        }
        Profiler.EndSample();
        Profiler.BeginSample("Process cut vectors");
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            int other = cylinderTriangles[i].pointingUpwards ? cylinderTriangles[i].b : cylinderTriangles[i].c;
            rightNormal = Vector3.SlerpUnclamped(normals[cylinderTriangles[i].a], normals[other], Mathf.InverseLerp(verts[cylinderTriangles[i].a].y, verts[other].y, cutHeight));

            for (int j = 0; j < cutVectors.Length; j++)
            {
                cutIntensities[i] += Mathf.Clamp01(Vector3.Dot(rightNormal, cutVectors[j]));
            }
            //Add a bevel effect to the stump
            if (cutMode != TriangleCutMode.Full)
                cutIntensities[i] = Mathf.Clamp(cutIntensities[i], minSeveredIntensity, float.MaxValue);
            else
                if (cutIntensities[i] < intensityCutoff) cutIntensities[i] = 0;
        }
        Profiler.EndSample();
        Profiler.BeginSample("Calculate required vertices");
        int additionalVertices = 0;
        int cutTrianglesCount = 0;
        bool chainToLeft = false;
        //And calculate the number of additional vertices that will be required
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            int leftTriangle = i - 1;
            if (leftTriangle == -1) leftTriangle = cylinderTriangles.Length - 1;
            if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0) || cutMode != TriangleCutMode.Full)
            {
                additionalVertices += chainToLeft ? LinePointCount(cutMode, cutIntensities[i]) :
                                                    LinePointCount(cutMode, cutIntensities[leftTriangle]) + LinePointCount(cutMode, cutIntensities[i]);
                cutTrianglesCount++;
                chainToLeft = mergeFaces;
            }
            else
            {
                chainToLeft = false;
            }
        }
        Profiler.EndSample();
        //DEBUG - draw sorted indexes
        // for (int i = 0; i < cylinderTriangles.Length; i++)
        // {
        //     Vector3 avg = (verts[cylinderTriangles[i].a] + verts[cylinderTriangles[i].b] + verts[cylinderTriangles[i].c]) / 3f + transform.position;
        //     label?.Invoke(avg, string.Format("t:{0} up:{1}", i, cylinderTriangles[i].pointingUpwards));
        // }




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

        int[] triangleIndices = null;
        if (cutMode == TriangleCutMode.Full) // Otherwise every triangle will be removed
            triangleIndices = cylinderTriangles.Select(x => x.index).ToArray();

        //Get the triangles
        List<int> meshTriangles = new List<int>();
        originalMesh.GetTriangles(meshTriangles, 0);
        List<int> cutTriangles = new List<int>();

        //Remove the triangles that will not be needed

        if (vertsAboveCut == null)
            TestForVerticesAboveCut();


        if (cutMode != TriangleCutMode.Full)
        {
            //remove the triangles above
            int removedTriOffset = 0;
            for (int i = 0; i < trianglesAboveCut.Length; i++)
            {
                if (cutMode == TriangleCutMode.Base && trianglesAboveCut[i] || cutMode == TriangleCutMode.Top && trianglesBelowCut[i])
                {
                    int triIndex = (i - removedTriOffset) * 3;
                    meshTriangles.RemoveRange(triIndex, 3);
                    removedTriOffset++;
                }
            }
        }


        //Use this data to finally create the cuts
        for (int i = 0; i < cylinderTriangles.Length; i++)
        {
            //Blend between triangles on the left (-1) and the right (+1)
            int leftTriangle = i - 1;
            if (leftTriangle == -1) leftTriangle = cylinderTriangles.Length - 1;

            if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0) || cutMode != TriangleCutMode.Full)
            {
                CutTriangle(in cylinderTriangles[i], chainToLeft, newVertices, newNormals, newUVs,
                            cutTriangles, meshTriangles, triangleOffset, cutHeight, halfCutSize,
                            cutIntensities[leftTriangle], cutIntensities[i], cutCenter, cutMode);


                //Remove this triangle from the mesh - if it hasn't been removed before
                if (cutMode == TriangleCutMode.Full)
                {
                    meshTriangles.RemoveRange(triangleIndices[i], 3);
                    for (int j = 0; j < triangleIndices.Length; j++)
                    {
                        if (triangleIndices[j] > triangleIndices[i]) triangleIndices[j] -= 3;
                    }
                }

                //Track number of total verts (again)
                triangleOffset += chainToLeft ? LinePointCount(cutMode, cutIntensities[i]) : LinePointCount(cutMode, cutIntensities[leftTriangle]) + LinePointCount(cutMode, cutIntensities[i]);
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
        if (cutMode != TriangleCutMode.Full)
        {
            Profiler.BeginSample("Copy to new lists");
            List<Vector3> cutVerts = newVertices.ToList();
            List<Vector3> cutNormals = newNormals.ToList();
            List<Vector2> cutUVs = newUVs.ToList();
            Profiler.EndSample();
            //Add the vert for the cap / base part

            cutVerts.Add(cutCenter);
            cutNormals.Add(Vector3.up);
            //This should be in the center
            cutUVs.Add(Vector2.one * 0.5f);


            Profiler.BeginSample("Remove tall vertices");
            for (int i = vertsAboveCut.Length - 1; i >= 0; i--)
            {
                if (cutMode == TriangleCutMode.Base && vertsAboveCut[i] ||
                    cutMode == TriangleCutMode.Top && !vertsAboveCut[i])
                {
                    cutVerts.RemoveAt(i);
                    cutNormals.RemoveAt(i);
                    cutUVs.RemoveAt(i);

                    for (int t = 0; t < meshTriangles.Count; t += 3)
                    {
                        if (meshTriangles[t] > i) meshTriangles[t]--;
                        if (meshTriangles[t + 1] > i) meshTriangles[t + 1]--;
                        if (meshTriangles[t + 2] > i) meshTriangles[t + 2]--;
                    }
                    for (int t = 0; t < cutTriangles.Count; t += 3)
                    {
                        if (cutTriangles[t] > i) cutTriangles[t]--;
                        if (cutTriangles[t + 1] > i) cutTriangles[t + 1]--;
                        if (cutTriangles[t + 2] > i) cutTriangles[t + 2]--;
                    }
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Create Mesh");
            cutMesh.SetVertices(cutVerts);
            cutMesh.SetNormals(cutNormals);
            cutMesh.SetUVs(0, cutUVs);
            cutMesh.SetUVs(1, cutUVs);
        }
        else
        {
            Profiler.BeginSample("Create Mesh");
            cutMesh.SetVertices(newVertices);
            cutMesh.SetNormals(newNormals);
            cutMesh.SetUVs(0, newUVs);
            cutMesh.SetUVs(1, newUVs);
        }

        cutMesh.subMeshCount = 2;

        cutMesh.SetTriangles(meshTriangles, 0);
        cutMesh.SetTriangles(cutTriangles, 1);

        cutMesh.UploadMeshData(true);
        Profiler.EndSample();







        return cutMesh;
    }




    public static System.Int32 GetCardinality(BitArray bitArray)
    {

        System.Int32[] ints = new System.Int32[(bitArray.Count >> 5) + 1];

        bitArray.CopyTo(ints, 0);

        System.Int32 count = 0;

        // fix for not truncated bits in last integer that may have been set to true with SetAll()
        ints[ints.Length - 1] &= ~(-1 << (bitArray.Count % 32));

        for (System.Int32 i = 0; i < ints.Length; i++)
        {

            System.Int32 c = ints[i];

            // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
            unchecked
            {
                c = c - ((c >> 1) & 0x55555555);
                c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
            }
            count += c;
        }
        return count;
    }

    private void OnDrawGizmos()
    {


        if (!gizmos) return;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        //Find the edges that will be cut by the cutting

        //Mesh mesh = CreateCutMesh(testCutMode, UnityEditor.Handles.Label);
        //Gizmos.DrawWireMesh(mesh);
    }

}
