using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

public class BuoyantBox : BuoyantBody
{
    public Vector3[,,] voxels;
    public Vector3[,,] forces;
    public Vector3 voxelSize;
    public Vector3Int voxelCount = new Vector3Int(2, 2, 2);
    new BoxCollider collider;

    float voxelSpan;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
        CalculateVoxels();
        rb.mass = collider.size.x * collider.size.y * collider.size.z * density;
        voxelSpan = Mathf.Max(voxelSize.x, voxelSize.y, voxelSize.z);
    }
    void CalculateVoxels()
    {
        voxels = new Vector3[voxelCount.x, voxelCount.y, voxelCount.z];
        forces = new Vector3[voxelCount.x, voxelCount.y, voxelCount.z];
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

    public float VoxelSubmersion(in Vector3 voxel, in Bounds bounds)
    {
        return Mathf.Clamp01((bounds.max.y - voxel.y) / voxelSpan + 0.5f);
    }

    private void FixedUpdate()
    {
        if (volume != null)
        {
            Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            float voxelContrib = 1f / voxels.Length;
            float voxelVolume = voxelSize.x * voxelSize.y * voxelSize.z;
            //Apply drag
            float voxelMass = voxelVolume * density;

            Vector3 buoyantForce = -Physics.gravity * voxelVolume * volume.density;

            for (int x = 0; x < voxelCount.x; x++)
            {
                for (int y = 0; y < voxelCount.y; y++)
                {
                    for (int z = 0; z < voxelCount.z; z++)
                    {
                        Vector3 point = localToWorld.MultiplyPoint(voxels[x, y, z]);
                        float submersion = VoxelSubmersion(point, volume.bounds);

                        if (submersion > 0)
                        {
                            Vector3 velocity = rb.GetPointVelocity(point);

                            Vector3 localDamping = -(velocity * waterDrag * voxelMass);



                            Vector3 force = localDamping + Mathf.Sqrt(submersion) * buoyantForce;

                            rb.AddForceAtPosition(force, point);
                            forces[x, y, z] = force;

                        }
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



            Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Matrix4x4 worldToLocal = localToWorld.inverse;
            Gizmos.matrix = localToWorld;

            float voxelVolume = voxelSize.x * voxelSize.y * voxelSize.z;
            float voxelMass = voxelVolume * density;

            for (int x = 0; x < voxelCount.x; x++)
            {
                for (int y = 0; y < voxelCount.y; y++)
                {
                    for (int z = 0; z < voxelCount.z; z++)
                    {
                        Vector3 point = localToWorld.MultiplyPoint(voxels[x, y, z]);
                        float sub = VoxelSubmersion(point, volume.bounds);
                        Gizmos.color = Color.Lerp(Color.white, Color.red, sub);


                        Gizmos.DrawWireCube(voxels[x, y, z], voxelSize);

                        Gizmos.DrawLine(voxels[x, y, z], voxels[x, y, z] + worldToLocal.MultiplyVector(forces[x, y, z] / rb.mass));

                    }
                }
            }
        }
    }
}
