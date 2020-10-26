using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public struct Triangle
{
    public int a;
    public int b;
    public int c;
    public int index;
    public bool pointingUpwards;

    public Triangle(int a, int b, int c, int index, bool pointingUpwards)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.index = index;
        this.pointingUpwards = pointingUpwards;
    }
}

[ExecuteAlways]
public class CuttableTree : MonoBehaviour, IAttackable
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


    public enum TriangleCutMode { Full, Top, Base }

    [Header("References")]
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public AudioSource audioSource;
    public CuttableTreeProfile profile;

    public List<CutVector> activeCutVectors = new List<CutVector>();

    float totalDamage = 0;



    private void Start()
    {
        activeCutVectors = new List<CutVector>();
        UpdateMeshFilter(TriangleCutMode.Full);
    }

    public void Attack(ItemName weapon, GameObject controller, Vector3 hitPosition)
    {
        CutTree(hitPosition, controller.transform.position);
    }


    public void CutTree(Vector3 hitPoint, Vector3 hitterPosition)
    {
        Profiler.BeginSample("Cut Tree");
        if (totalDamage >= profile.damageToCut)
        {
            return;
        }


        float intensity = 0.2f;
        Vector3 direction = (hitPoint - transform.position);
        direction.y = 0;
        direction.Normalize();

        activeCutVectors.Add(new CutVector(Vector3.SignedAngle(transform.forward, direction, Vector3.up) * Mathf.Deg2Rad, intensity));
        totalDamage += intensity;

        if (profile.cutClips.Length != 0)
            audioSource.PlayOneShot(profile.cutClips[Random.Range(0, profile.cutClips.Length)]);


        if (totalDamage < profile.damageToCut) UpdateMeshFilter(TriangleCutMode.Full);
        else SplitTree(hitterPosition);

        Profiler.EndSample();
        //Debug.Break();
    }

    public void SplitTree(Vector3 hitterPosition)
    {
        if (meshCollider == null) throw new System.Exception("Mesh collider required");

        Mesh stump = CreateCutMesh(TriangleCutMode.Base);
        meshFilter.sharedMesh = stump;
        meshCollider.sharedMesh = stump;
        Mesh trunkMesh = CreateCutMesh(TriangleCutMode.Top);
        GameObject log = new GameObject("Tree trunk", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Rigidbody), typeof(CutLog));
        log.transform.SetPositionAndRotation(transform.position + Vector3.up * profile.cutSize * 0.5f, transform.rotation);

        MeshCollider logCollider = log.GetComponent<MeshCollider>();
        Rigidbody logRB = log.GetComponent<Rigidbody>();
        MeshFilter logFilter = log.GetComponent<MeshFilter>();
        MeshRenderer logRenderer = log.GetComponent<MeshRenderer>();
        CutLog cutLog = log.GetComponent<CutLog>();

        cutLog.trunk = gameObject;
        GameObject canopy = new GameObject("Canopy", typeof(MeshFilter), typeof(MeshRenderer));
        canopy.GetComponent<MeshFilter>().sharedMesh = profile.canopyMesh;
        canopy.GetComponent<MeshRenderer>().material = profile.logMaterial;
        canopy.transform.SetParent(log.transform, false);
        cutLog.canopy = canopy;
        cutLog.lengthRegion = new Vector2(profile.cutHeight, profile.cutHeight + profile.logEstimateHeight);

        logCollider.convex = true;
        logCollider.sharedMesh = trunkMesh;
        logFilter.sharedMesh = trunkMesh;
        logRenderer.materials = new Material[] { profile.logMaterial, profile.crosssectionMaterial };

        //Calculate the direction the log should fall in
        Vector3 playerDirection = transform.position - hitterPosition;
        playerDirection.y = 0;
        playerDirection.Normalize();

        logRB.mass = profile.LogMass;

        logRB.AddForceAtPosition(playerDirection * profile.logKnockingForce,
                                logRB.centerOfMass + Vector3.up * profile.logEstimateHeight * 0.5f - playerDirection * profile.logEstimateRadius);
    }


    static float InverseHeightLerp(Vector3 start, Vector3 end, float height) => Mathf.InverseLerp(start.y, end.y, height);

    public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c) => Vector3.Cross(b - a, c - a);




    void CreateLineCut(int vertexOffset, int a, int other, float bottom, float top, float intensity,
                      int subDivisions, Vector3 cutCenter,
                      Vector3[] meshVerts, Vector3[] meshNormals, Vector2[] meshUV,
                      TriangleCutMode cutMode, bool pointingUp)
    {

        float bottomCut = InverseHeightLerp(meshVerts[a], meshVerts[other], bottom);
        float topCut = InverseHeightLerp(meshVerts[a], meshVerts[other], top);


        float rangeBottom = cutMode == TriangleCutMode.Top ? (bottom + top) * 0.5f : bottom;
        float rangeTop = cutMode == TriangleCutMode.Base ? (bottom + top) * 0.5f : top;


        float lerpRangeBottom = cutMode == TriangleCutMode.Top ? 0.5f : 0;
        float lerpRangeSize = cutMode == TriangleCutMode.Full ? 1 : 0.5f;


        bool flattenLastVert = intensity < 0;


        //Top vertices
        float invTotalDivisions = 1f / subDivisions;

        for (int i = 0; i <= subDivisions; i++)
        {
            //Progress in range 0 to 1
            float progress = i * invTotalDivisions * lerpRangeSize + lerpRangeBottom;
            //X should be in range of -1 to 1
            float x = progress * 2 - 1;
            //Power x to bevel distribution to space out points
            x = Mathf.Pow(Mathf.Abs(x), 1f / profile.bevelDistribution) * Mathf.Sign(x);
            //Apply this power to the progress
            progress = (x + 1) * 0.5f;

            float depth = Mathf.Pow(1 - Mathf.Pow(Mathf.Abs(x), profile.bevelProfile), 1f / profile.bevelProfile) * Mathf.Abs(intensity);

            Vector3 p1 = Vector3.Lerp(
                Vector3.Lerp(meshVerts[a], meshVerts[other], bottomCut),
                Vector3.Lerp(meshVerts[a], meshVerts[other], topCut), progress);
            Vector3 pos = Vector3.Lerp(p1, cutCenter, depth);

            if (flattenLastVert && i == subDivisions) pos.y = rangeBottom;

            //Every set of 2 points is identical as they do not share normals
            //Vector3 pos = Vector3.Lerp(Vector3.Lerp(meshVerts[a], meshVerts[other], t), cutCenter, depth);

            pos.y = Mathf.Clamp(pos.y, rangeBottom, rangeTop);
            meshVerts[vertexOffset + i * 2] = meshVerts[vertexOffset + i * 2 + 1] = pos;

            Vector2 dir = new Vector2(pos.x - cutCenter.x, pos.z - cutCenter.z).normalized;
            meshUV[vertexOffset + i * 2] = meshUV[vertexOffset + i * 2 + 1] = Vector2.one * 0.5f + dir * (1 - depth) * profile.crossSectionScale * 0.5f;
        }
        //Only change these uvs if they are not supposed to be a part of a flat trunk 
        if (cutMode != TriangleCutMode.Top)
            meshUV[vertexOffset] = Vector2.Lerp(meshUV[a], meshUV[other], bottomCut);
        if (cutMode != TriangleCutMode.Base)
            meshUV[vertexOffset + subDivisions * 2 + 1] = Vector2.Lerp(meshUV[a], meshUV[other], topCut);

        if (cutMode == TriangleCutMode.Base || cutMode == TriangleCutMode.Full)
        {
            meshNormals[vertexOffset] = Vector3.Slerp(meshNormals[a], meshNormals[other], bottom);
        }
        else
        {
            meshNormals[vertexOffset] = Vector3.down;
        }

        if (cutMode == TriangleCutMode.Top || cutMode == TriangleCutMode.Full)
        {
            meshNormals[vertexOffset + subDivisions * 2 + 1] = Vector3.Slerp(meshNormals[a], meshNormals[other], top);
        }
        else
        {
            meshNormals[vertexOffset + subDivisions * 2 + 1] = Vector3.up;
        }


        Vector3 cutSurfaceCenter = Vector3.Lerp(meshVerts[a], meshVerts[other], (topCut + bottomCut) * 0.5f);

        Vector3 leftDirection = TriangleNormal(meshVerts[pointingUp ? other : a], cutCenter, meshVerts[pointingUp ? a : other]);

        for (int i = 0; i < subDivisions; i++)
        {
            meshNormals[vertexOffset + i * 2 + 1] = meshNormals[vertexOffset + i * 2 + 2] =
                Vector3.Cross(meshVerts[vertexOffset + i * 2 + 1] - meshVerts[vertexOffset + i * 2 + 2], leftDirection);
        }





        // meshNormals[offset + 3] = meshNormals[offset + cutLines * 2] = -Vector3.Cross(meshVerts[offset + cutLines * 2] - meshVerts[offset + 3], leftDirection);

        // meshNormals[offset] = Vector3.Slerp(meshNormals[a], meshNormals[other], bottomCut);
        // meshNormals[offset + cutLines * 2 + 1] = Vector3.Slerp(meshNormals[a], meshNormals[other], topCut);



    }



    public int FindFullSubdivisions(float intensity)
    {
        //Base of 2 - intensity of 0 should have no subdivisions
        return intensity == 0 ? 1 : Mathf.Clamp(Mathf.FloorToInt(profile.subdivisionScalar * intensity) + profile.minSubdivisions, profile.minSubdivisions, profile.maxSubdivisions);
    }

    public int FindSubdivisions(TriangleCutMode cutMode, float intensity) => cutMode == TriangleCutMode.Full ?
                                                    FindFullSubdivisions(intensity) * 2 :
                                                    FindFullSubdivisions(intensity);




    public int LinePointCount(TriangleCutMode cutMode, float intensity) => FindSubdivisions(cutMode, intensity) * 2 + 2;



    public void CutTriangle(in Triangle t, bool connectLeft, Vector3[] meshVerts, Vector3[] meshNormals, Vector2[] meshUV,
                            List<int> cutTriangles, int[] meshTriangles, int meshTriangleOffset, int vertexOffset, float cutHeight, float cutSize, float leftIntensity, float rightIntensity,
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
            CreateLineCut(vertexOffset + left, t.a, t.pointingUpwards ? t.c : t.b, cutHeight - cutSize * leftIntensity, cutHeight + cutSize * leftIntensity,
                            leftIntensity, leftSubdivisions, cutCenter,
                            meshVerts, meshNormals, meshUV,
                            cutMode, t.pointingUpwards);
            CreateLineCut(vertexOffset + right, t.a, t.pointingUpwards ? t.b : t.c, cutHeight - cutSize * rightIntensity, cutHeight + cutSize * rightIntensity,
                            rightIntensity, rightSubdivisions, cutCenter, meshVerts, meshNormals, meshUV, cutMode, t.pointingUpwards);
        }
        else
        {
            //Create right line with no relative offset
            CreateLineCut(vertexOffset, t.a, t.pointingUpwards ? t.b : t.c, cutHeight - cutSize * rightIntensity,
                            cutHeight + cutSize * rightIntensity, rightIntensity, rightSubdivisions, cutCenter,
                            meshVerts, meshNormals, meshUV, cutMode, t.pointingUpwards);
        }

        if (connectLeft) //Make right start from 0
            vertexOffset -= right;

        //Setup triangles - add all the triangles for top and bottom parts
        //Add top triangle

        if (t.pointingUpwards && cutMode != TriangleCutMode.Base || !t.pointingUpwards && cutMode != TriangleCutMode.Top)
        {
            //Calculate which parts will connect to the a b and c key vertices
            int connectA1 = t.pointingUpwards ? left + leftPointCount - 1 : right;
            int connectA2 = t.pointingUpwards ? right + rightPointCount - 1 : left;

            meshTriangles[meshTriangleOffset] = (vertexOffset + connectA1);
            meshTriangles[meshTriangleOffset + 1] = (t.a);
            meshTriangles[meshTriangleOffset + 2] = (vertexOffset + connectA2);
            meshTriangleOffset += 3; //This is a copied parameter so can be changed - update for other triangles
        }

        if (!t.pointingUpwards && cutMode != TriangleCutMode.Base || t.pointingUpwards && cutMode != TriangleCutMode.Top)
        {
            //Calculate which parts will connect to the a b and c key vertices
            int connectBC1 = t.pointingUpwards ? left : right + rightPointCount - 1;
            int connectBC2 = t.pointingUpwards ? right : left + leftPointCount - 1;

            meshTriangles[meshTriangleOffset] = (t.b);
            meshTriangles[meshTriangleOffset + 1] = (vertexOffset + connectBC1);
            meshTriangles[meshTriangleOffset + 2] = (vertexOffset + connectBC2);

            meshTriangles[meshTriangleOffset + 3] = (t.b);
            meshTriangles[meshTriangleOffset + 4] = (t.c);
            meshTriangles[meshTriangleOffset + 5] = (vertexOffset + connectBC1);
        }

        // print($"{meshTriangles.Count} from {o} : o = {meshTriangleOffset}");

        if (cutMode == TriangleCutMode.Base)
        {
            //Add triangle to cap piece
            cutTriangles.Add(vertexOffset + right + rightPointCount - 1);
            cutTriangles.Add(vertexOffset + left + leftPointCount - 1);
            cutTriangles.Add(meshVerts.Length);
        }
        else if (cutMode == TriangleCutMode.Top)
        {
            //Add triangle to cap piece
            cutTriangles.Add(vertexOffset + left);
            cutTriangles.Add(vertexOffset + right);
            cutTriangles.Add(meshVerts.Length);
        }


        bool reverseTriangles = leftSubdivisions < rightSubdivisions;

        int addRight = reverseTriangles ? 2 : 1;
        int addLeft = reverseTriangles ? 1 : 2;

        //Add all the triangles for the subdivisions
        for (int i = 0; i < Mathf.Min(leftSubdivisions, rightSubdivisions); i++)
        {

            //Triangle 1
            cutTriangles.Add(vertexOffset + left + 1 + i * 2);
            cutTriangles.Add(vertexOffset + left + 2 + i * 2);
            cutTriangles.Add(vertexOffset + right + addRight + i * 2);
            //Triangle 2
            cutTriangles.Add(vertexOffset + left + addLeft + i * 2);
            cutTriangles.Add(vertexOffset + right + 2 + i * 2);
            cutTriangles.Add(vertexOffset + right + 1 + i * 2);
        }
        if (rightSubdivisions > leftSubdivisions)
        {
            int requiredTriangles = rightSubdivisions - leftSubdivisions;
            //Add final additional triangles
            for (int i = 0; i < requiredTriangles; i++)
            {
                cutTriangles.Add(vertexOffset + left + leftPointCount - 2);
                cutTriangles.Add(vertexOffset + right + rightPointCount - 2 - i * 2);
                cutTriangles.Add(vertexOffset + right + rightPointCount - 3 - i * 2);
            }
        }

        else if (leftSubdivisions > rightSubdivisions)
        {
            //Add final additional triangles

            int requiredTriangles = leftSubdivisions - rightSubdivisions;
            for (int i = 0; i < requiredTriangles; i++)
            {

                cutTriangles.Add(vertexOffset + left + leftPointCount - 3 - i * 2);
                cutTriangles.Add(vertexOffset + left + leftPointCount - 2 - i * 2);
                cutTriangles.Add(vertexOffset + right + rightPointCount - 2);
            }
        }

    }



    public void UpdateMeshFilter(TriangleCutMode cutMode)
    {
        Profiler.BeginSample("Update Mesh Filter");
        if (meshFilter == null) throw new System.Exception("No mesh filter set to update");
        if (activeCutVectors.Count > 0) //Cut into the mesh
        {
            meshFilter.sharedMesh = CreateCutMesh(cutMode);
            meshRenderer.materials = new Material[] { profile.logMaterial, profile.crosssectionMaterial };
        }
        else //Do not cut into the mesh
        {
            meshFilter.sharedMesh = profile.treeMesh.mesh;
            meshRenderer.materials = new Material[] { profile.logMaterial };
        }
        meshCollider.sharedMesh = profile.treeMesh.mesh;
        Profiler.EndSample();
    }




    public Mesh CreateCutMesh(TriangleCutMode cutMode, System.Action<Vector3, string> label = null, System.Action<Vector3, Vector3> line = null)
    {
        Profiler.BeginSample("Create Cut Mesh");

        CuttableTreeProfile.CuttableCylinderMesh m = null;
        switch (cutMode)
        {
            case TriangleCutMode.Full:
                m = profile.treeMesh;
                break;
            case TriangleCutMode.Top:
                m = profile.logMesh;
                break;
            case TriangleCutMode.Base:
                m = profile.stumpMesh;
                break;
        }



        Profiler.BeginSample("Load Mesh");
        //Get data from deep inside the unity c++ core. spooky
        List<Vector3> verts = new List<Vector3>();
        m.mesh.GetVertices(verts);

        List<Vector3> normals = new List<Vector3>();
        m.mesh.GetNormals(normals);

        List<Vector2> uv = new List<Vector2>();
        m.mesh.GetUVs(0, uv);

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

        //Calculate all the intensities for the triangles
        System.Span<float> cutIntensities = m.cutCylinder.Length < 128 ? stackalloc float[m.cutCylinder.Length] : new float[m.cutCylinder.Length];
        Vector3 rightNormal;

        Profiler.BeginSample("Find cut vectors");
        //stackalloc is faster but small as works in the stack (cache)
        System.Span<Vector3> cutVectors = activeCutVectors.Count < 32 ? stackalloc Vector3[activeCutVectors.Count] : new Vector3[activeCutVectors.Count];

        for (int i = 0; i < activeCutVectors.Count; i++)
        {
            cutVectors[i] = new Vector3(
                Mathf.Sin(activeCutVectors[i].angle), 0,
                Mathf.Cos(activeCutVectors[i].angle)) * activeCutVectors[i].intensity;
        }

        Profiler.EndSample();
        Profiler.BeginSample("Process cut vectors");

        for (int i = 0; i < m.cutCylinder.Length; i++)
        {
            int other = m.cutCylinder[i].pointingUpwards ? m.cutCylinder[i].b : m.cutCylinder[i].c;
            rightNormal = Vector3.SlerpUnclamped(normals[m.cutCylinder[i].a], normals[other], Mathf.InverseLerp(verts[m.cutCylinder[i].a].y, verts[other].y, profile.cutHeight));

            for (int j = 0; j < cutVectors.Length; j++)
            {
                if (cutMode == TriangleCutMode.Full)
                    cutIntensities[i] += Mathf.Clamp01(Vector3.Dot(rightNormal, cutVectors[j]));
                else
                    cutIntensities[i] += Vector3.Dot(rightNormal, cutVectors[j]);
            }
            //Add a bevel effect to the stump

            if (cutMode != TriangleCutMode.Full)
            {
                //add the splinter effect
                if (cutIntensities[i] <= 0)
                {
                    cutIntensities[i] *= 0.5f - (i % 2);
                }

                //cutIntensities[i] = Mathf.Clamp(cutIntensities[i], minSeveredIntensity, float.MaxValue);
            }
            else if (cutIntensities[i] < profile.intensityCutoff) cutIntensities[i] = 0;

        }
        Profiler.EndSample();
        Profiler.BeginSample("Calculate required vertices");
        int additionalVertices = 0;
        int triangleCount = 0;

        bool chainToLeft = false;

        int[] meshTriangleOffsets = new int[m.cutCylinder.Length];

        int[] triangleIndices = new int[m.cutCylinder.Length];
        for (int i = 0; i < m.cutCylinder.Length; i++)
            triangleIndices[i] = m.cutCylinder[i].index;

        //Get the triangles
        List<int> meshTriangles = new List<int>();
        m.mesh.GetTriangles(meshTriangles, 0);

        //And calculate the number of additional vertices that will be required
        for (int i = 0; i < m.cutCylinder.Length; i++)
        {
            int leftTriangle = i - 1;
            if (leftTriangle == -1) leftTriangle = m.cutCylinder.Length - 1;
            if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0) || cutMode != TriangleCutMode.Full)
            {
                additionalVertices += chainToLeft ? LinePointCount(cutMode, cutIntensities[i]) :
                                                    LinePointCount(cutMode, cutIntensities[leftTriangle]) + LinePointCount(cutMode, cutIntensities[i]);
                //Calculate the number of triangles this cut will use
                bool up = m.cutCylinder[i].pointingUpwards;
                meshTriangleOffsets[i] = triangleCount;

                if (up && cutMode != TriangleCutMode.Base || !up && cutMode != TriangleCutMode.Top)
                {
                    triangleCount += 3;
                }
                if (!up && cutMode != TriangleCutMode.Base || up && cutMode != TriangleCutMode.Top)
                {
                    triangleCount += 2 * 3;
                }


                //Remove this triangle from the mesh
                meshTriangles.RemoveRange(triangleIndices[i], 3);
                for (int j = 0; j < triangleIndices.Length; j++)
                {
                    if (triangleIndices[j] > triangleIndices[i]) triangleIndices[j] -= 3;
                }

                chainToLeft = profile.mergeFaces;
            }
            else
            {
                chainToLeft = false;
            }
        }


        for (int i = 0; i < m.cutCylinder.Length; i++)
            meshTriangleOffsets[i] += meshTriangles.Count;

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

        //Cut the edges
        Profiler.BeginSample("Remove unwanted triangles");
        chainToLeft = false;
        float halfCutSize = profile.cutSize / 2;




        List<int> cutTriangles = new List<int>();
        if (m.mesh.subMeshCount > 1)//Some parts of the mesh will be pre - cut
            m.mesh.GetTriangles(cutTriangles, 1);



        triangleCount += meshTriangles.Count;
        //Update the capacities of the lists for less allocations.
        //TODO - make array for multicore cutting
        int[] newMeshTriangles = new int[triangleCount];
        meshTriangles.CopyTo(newMeshTriangles);
        //meshTriangles.Capacity = triangleCount;
        //meshTriangles.AddRange(Enumerable.Repeat(0, triangleCount - meshTriangles.Count));



        Profiler.EndSample();

        //Time to actually cut after all this pre processing

        Profiler.BeginSample($"Cut {m.cutCylinder.Length} Triangles");
        int vertexOffset = totalVertices - additionalVertices;

        //Use this data to finally create the cuts
        for (int i = 0; i < m.cutCylinder.Length; i++)
        {
            //Blend between triangles on the left (-1) and the right (+1)
            int leftTriangle = i - 1;
            if (leftTriangle == -1) leftTriangle = m.cutCylinder.Length - 1;

            if (!(cutIntensities[leftTriangle] == 0 && cutIntensities[i] == 0) || cutMode != TriangleCutMode.Full)
            {
                CutTriangle(in m.cutCylinder[i], chainToLeft, newVertices, newNormals, newUVs,
                            cutTriangles, newMeshTriangles, meshTriangleOffsets[i], vertexOffset, profile.cutHeight, halfCutSize,
                            cutIntensities[leftTriangle], cutIntensities[i], m.centerPoint, cutMode);
                //Track number of total verts (again)
                vertexOffset += chainToLeft ? LinePointCount(cutMode, cutIntensities[i]) : LinePointCount(cutMode, cutIntensities[leftTriangle]) + LinePointCount(cutMode, cutIntensities[i]);
                //This triangle will be the first in a chain sharing vertices
                chainToLeft = profile.mergeFaces;
            }
            else
            {
                chainToLeft = false;
            }
            //rightIntensity = leftIntensity;
        }

        Profiler.EndSample();

        //Copy added data to the array of all the data

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

            cutVerts.Add(m.centerPoint);
            cutNormals.Add(cutMode == TriangleCutMode.Base ? Vector3.up : Vector3.down); //Point normal in correct direction
            //This should be in the center
            cutUVs.Add(Vector2.one * 0.5f);


            Profiler.BeginSample("Remove tall vertices");
            void Remove(int index)
            {
                cutVerts.RemoveAt(index);
                cutNormals.RemoveAt(index);
                cutUVs.RemoveAt(index);
                for (int t = 0; t < cutTriangles.Count; t += 3)
                {
                    if (cutTriangles[t] > index) cutTriangles[t]--;
                    if (cutTriangles[t + 1] > index) cutTriangles[t + 1]--;
                    if (cutTriangles[t + 2] > index) cutTriangles[t + 2]--;
                }
                for (int t = 0; t < newMeshTriangles.Length; t += 3)
                {
                    if (newMeshTriangles[t] > index) newMeshTriangles[t]--;
                    if (newMeshTriangles[t + 1] > index) newMeshTriangles[t + 1]--;
                    if (newMeshTriangles[t + 2] > index) newMeshTriangles[t + 2]--;
                }
            }
            //Use a hashset to hold every *unique* vertex being removed
            SortedSet<int> toBeRemoved = new SortedSet<int>();

            //If at top, remove cylinder ring at base
            for (int i = 0; i < m.cutCylinder.Length; i++)
            {

                if (m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Base || !m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Top)
                {
                    toBeRemoved.Add(m.cutCylinder[i].a);
                }
                if (!m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Base || m.cutCylinder[i].pointingUpwards && cutMode == TriangleCutMode.Top)
                {
                    toBeRemoved.Add(m.cutCylinder[i].b);
                    toBeRemoved.Add(m.cutCylinder[i].c);
                }
            }
            //Then removed all of them backwards so no offset errors from list size chaning
            foreach (int index in toBeRemoved.Reverse())
            {
                Remove(index);
            }

            //If at base, remove cylinder ring at top

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

        //  print($"{meshTriangles.Count} : {triangleCount}");

        cutMesh.subMeshCount = 2;

        cutMesh.SetTriangles(newMeshTriangles, 0);
        cutMesh.SetTriangles(cutTriangles, 1);

        cutMesh.UploadMeshData(true);
        Profiler.EndSample();





        Profiler.EndSample();

        return cutMesh;
    }




    public static int GetCardinality(BitArray bitArray)
    {

        int[] ints = new int[(bitArray.Count >> 5) + 1];

        bitArray.CopyTo(ints, 0);

        int count = 0;

        // fix for not truncated bits in last integer that may have been set to true with SetAll()
        ints[ints.Length - 1] &= ~(-1 << (bitArray.Count % 32));

        for (int i = 0; i < ints.Length; i++)
        {

            int c = ints[i];

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



    // void OnDrawGizmos()
    // {
    //     if (gizmos)
    //     {
    //         Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
    //         var s = new System.Diagnostics.Stopwatch();
    //         s.Start();
    //         Mesh mesh = CreateCutMesh(CuttableTree.TriangleCutMode.Full, UnityEditor.Handles.Label);
    //         s.Stop();
    //         Debug.Log($"Time to cut: {s.ElapsedMilliseconds}ms");
    //         Gizmos.DrawWireMesh(mesh);
    //     }
    // }
}
