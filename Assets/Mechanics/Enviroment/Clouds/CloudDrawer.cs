using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteAlways]
public class CloudDrawer : MonoBehaviour
{
    public Mesh planeMesh;
    public Material cloudMaterial;
    public float height = 5;
    [Range(1, 20)]
    public int layers = 3;
    public float layersThickness = 3;
    public Quaternion rotation = Quaternion.Euler(180, 0, 0);
    public float scale = 100;
    public Transform sun;
    public void SetCloudDensity(float density)
    {
        cloudMaterial.SetFloat("_AlphaClipping", 1 - density);
    }


    public void Update()
    {
        MaterialPropertyBlock b = new MaterialPropertyBlock();
        Matrix4x4 mat;
        cloudMaterial.SetVector("_SunDir", sun.forward);

        for (int i = 0; i < layers; i++)
        {
            float distFromCenter = ((i / (float)(layers - 1)) - 0.5f) * 0.5f;

            mat = Matrix4x4.TRS(Vector3.up * (height + distFromCenter * layersThickness), rotation, Vector3.one * scale);
            b.SetFloat("_DistFromCenter", distFromCenter);
            Graphics.DrawMesh(
                planeMesh,
                mat,
                 cloudMaterial,
                 0, null, 0, b, ShadowCastingMode.TwoSided);
        }

    }
}
