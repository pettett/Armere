using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

public class BuoyantBox : BuoyantBody
{
    public Vector3[,,] voxels;
    public Vector3 voxelSize;
    public Vector3Int voxelCount = new Vector3Int(2, 2, 2);
    new BoxCollider collider;

    float voxelSubmersionSpan;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
        CalculateVoxels();
        rb.mass = collider.size.x * collider.size.y * collider.size.z * density;
        voxelSubmersionSpan = Mathf.Min(voxelSize.x, voxelSize.y, voxelSize.z);
    }
    void CalculateVoxels()
    {
        voxels = new Vector3[voxelCount.x, voxelCount.y, voxelCount.z];
        voxelSize = new Vector3(collider.size.x / voxelCount.x, collider.size.y / voxelCount.y, collider.size.z / voxelCount.z);
        Vector3 halfVoxelSize = voxelSize / 2;

        for (int x = 0; x < voxelCount.x; x++)
        {
            for (int y = 0; y < voxelCount.y; y++)
            {
                for (int z = 0; z < voxelCount.z; z++)
                {
                    voxels[x, y, z] = new Vector3(voxelSize.x * x, voxelSize.y * y, voxelSize.z * z) + halfVoxelSize - collider.size * 0.5f;
                }
            }
        }
    }

    public float VoxelSubmersion(Vector3 voxel, Bounds bounds)
    {
        return Mathf.Clamp01((voxel.y - (bounds.center.y + bounds.extents.y) + 0.5f) * voxelSubmersionSpan);
    }

    private void FixedUpdate()
    {
        Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        float voxelContrib = 1f / voxels.Length;

        if (volume != null)
            for (int x = 0; x < voxelCount.x; x++)
            {
                for (int y = 0; y < voxelCount.y; y++)
                {
                    for (int z = 0; z < voxelCount.z; z++)
                    {
                        Vector3 point = localToWorld.MultiplyPoint(voxels[x, y, z]);
                        if (volume.bounds.Contains(point))
                        {
                            Vector3 velocity = rb.GetRelativePointVelocity(voxels[x, y, z]);

                            float voxelVolume = voxelSize.x * voxelSize.y * voxelSize.z;
                            //Apply drag
                            float voxelMass = voxelVolume * density;
                            rb.AddForceAtPosition(
                                -Physics.gravity * voxelVolume * volume.density,
                                point);
                            rb.AddForceAtPosition(
                                -velocity * voxelContrib * waterDrag,
                                point, ForceMode.VelocityChange);

                        }
                    }
                }
            }
    }
    private void OnDrawGizmos()
    {

        if (voxels != null && volume != null && voxels.GetLength(0) == voxelCount.x && voxels.GetLength(1) == voxelCount.y && voxels.GetLength(2) == voxelCount.z)
        {

            Gizmos.DrawWireCube(volume.bounds.center, volume.bounds.size);


            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);


            for (int x = 0; x < voxelCount.x; x++)
            {
                for (int y = 0; y < voxelCount.y; y++)
                {
                    for (int z = 0; z < voxelCount.z; z++)
                    {
                        Vector3 p = localToWorld.MultiplyPoint(voxels[x, y, z]);
                        Gizmos.color = Color.Lerp(Color.red, Color.white, VoxelSubmersion(p, volume.bounds));


                        Gizmos.DrawWireCube(voxels[x, y, z], voxelSize);

                    }
                }
            }
        }
    }
}
