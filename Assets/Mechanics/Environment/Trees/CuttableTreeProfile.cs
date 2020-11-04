using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[CreateAssetMenu(fileName = "New Cuttable Tree Mesh", menuName = "Game/Cuttable Tree Mesh", order = 0)]
public class CuttableTreeProfile : ScriptableObject
{
    [System.Serializable]
    public class CuttableCylinderMesh
    {
        public Mesh mesh;
        public Triangle[] cutCylinder;
        public Vector3 centerPoint;
    }

    [Header("Cutting")]
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
    public float damageToCut = 1;


    public CuttableCylinderMesh treeMesh;
    public CuttableCylinderMesh stumpMesh;
    public CuttableCylinderMesh logMesh;
    public Mesh canopyMesh;

    public float cutHeight = 1;
    public float cutSize = 1.2f;
    public AudioClipSet cutClips;

    [Header("Log Felling")]
    public float logDensity = 700f;
    public float logEstimateHeight = 3f;
    public float logEstimateRadius = 0.15f;
    public float logKnockingForce = 70f;
    [Header("Texturing")]
    [Range(0, 1)]
    public float crossSectionScale = 0.9f;
    public Material logMaterial;
    public Material crosssectionMaterial;

    public float LogMass => Mathf.PI * logEstimateRadius * logEstimateRadius * logEstimateHeight * logDensity;

    [MyBox.ButtonMethod]
    public void BakeProfile()
    {
        FindCylinderTriangles(treeMesh);
        FindCylinderTriangles(stumpMesh);
        FindCylinderTriangles(logMesh);
    }


    static void SortTrianglesCounterClockwise(ref List<Triangle> triangles, List<Vector3> verts)
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
            Debug.LogErrorFormat("Sort reached limit, missing {0} triangles", triangles.Count);
            //sortedCylinderTriangles.AddRange(triangles);
            // sortedCylinderTriangleIndexes.AddRange(indices);
        }

        triangles = sortedCylinderTriangles;
    }

    public void FindCylinderTriangles(CuttableCylinderMesh mesh)
    {
        if (!mesh.mesh.isReadable) throw new System.Exception("Tree mesh is not marked as readable");

        //Find the edges that will be cut by the cutting
        Profiler.BeginSample("Find Triangles");
        List<Vector3> verts = new List<Vector3>();
        mesh.mesh.GetVertices(verts);
        List<int> tris = new List<int>();
        mesh.mesh.GetTriangles(tris, 0);
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
                triangle = new Triangle(tris[i], tris[i + 1], tris[i + 2], i, t1.pointUp);
            }
            if (t3.hit && t2.hit)
            {
                triangle = new Triangle(tris[i + 2], tris[i], tris[i + 1], i, t3.pointUp);
            }
            if (t1.hit && t3.hit)
            {
                triangle = new Triangle(tris[i + 1], tris[i + 2], tris[i], i, !t1.pointUp);
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
        //Find the center point of the cylinder
        Vector3 total = Vector3.zero;
        for (int i = 0; i < cylinderTriangles.Count; i++)
        {
            total += verts[cylinderTriangles[i].a] + verts[cylinderTriangles[i].b] + verts[cylinderTriangles[i].c];
        }
        total /= cylinderTriangles.Count * 3;
        mesh.centerPoint = total;
        mesh.centerPoint.y = cutHeight;

        mesh.cutCylinder = cylinderTriangles.ToArray();
    }


}