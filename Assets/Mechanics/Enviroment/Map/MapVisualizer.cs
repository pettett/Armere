using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[ExecuteAlways]
public class MapVisualizer : MonoBehaviour
{
    public bool gizmos;
    public Map map;
    public Mesh[] meshes;
    private void OnEnable()
    {

    }
    void GenerateMeshes()
    {
        meshes = new Mesh[map.regions.Length];
        for (int i = 0; i < map.regions.Length; i++)
        {
            meshes[i] = new Mesh();
            Vector3[] v = new Vector3[map.regions[i].shape.Length];
            for (int j = 0; j < v.Length; j++)
            {
                v[j] = new Vector3(map.regions[i].shape[j].x, 0, map.regions[i].shape[j].y);

            }
            meshes[i].vertices = v;
            meshes[i].triangles = map.regions[i].triangles;
            meshes[i].RecalculateNormals();
            meshes[i].UploadMeshData(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (!gizmos) return;
        GenerateMeshes();
        for (int i = 0; i < meshes.Length; i++)
        {
            Gizmos.DrawMesh(meshes[i]);
        }
    }
}
