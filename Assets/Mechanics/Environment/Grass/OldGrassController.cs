using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;


public class OldGrassController : MonoBehaviour
{
    public Vector3Int threadGroups = new Vector3Int(8, 1, 1);
    [System.NonSerialized] public Vector3Int destroyGrassThreadGroupSize;
    [System.NonSerialized] public Vector3Int mainThreadGroupSize;
    public int totalPopulation;




    public float range;

    public Material material;
    public ComputeShader compute;
    public ComputeShader destroyGrassInBounds;
    public ComputeShader destroyGrassInSector;

    private ComputeBuffer meshPropertiesBuffer;

    private ComputeBuffer matrixesBuffer;
    private ComputeBuffer argsBuffer;
    private Mesh mesh;
    private Bounds bounds;

    public Terrain terrain;
    public Gradient colorGradient = new Gradient();

    public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
    public Vector2 quadHeightRange = new Vector2(0.5f, 1f);
    Camera mainCamera;
    int mainKernel;
    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public float SampleRandomRange(Vector2 range) => Random.Range(range.x, range.y);

    public float boundsYRot = 45f;
    public Vector3 testPoint;
    public Bounds killingBounds;
    public Transform trackingSector;
    public Vector2 sectorRange = new Vector2(0.5f, 1.5f);




    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector2 size;
        public Vector4 color;
        public static int Size()
        {
            return
                //  rotation, position,size
                sizeof(float) * (3 + 3 + 2 + 4);
        }
    }

    private struct MatricesStruct
    {
        public Matrix4x4 matrix;
        public Vector4 color;

        public static int Size()
        {
            return sizeof(float) * (4 * 4 + 4);
        }

    }

    private void Setup()
    {
        Mesh mesh = CreateQuad();
        this.mesh = mesh;



        InitializeBuffers();
    }

    private void DisposeBuffers()
    {
        if (meshPropertiesBuffer != null)
        {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (matrixesBuffer != null)
        {
            matrixesBuffer.Release();
        }
        matrixesBuffer = null;

        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
        argsBuffer = null;
    }

    void UpdateBounds()
    {
        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range * 2 + 1));
    }

    void UpdateThreadGroupSizes()
    {
        compute.GetKernelThreadGroupSizes(mainKernel, out uint x, out uint y, out uint z);
        mainThreadGroupSize = new Vector3Int((int)x, (int)y, (int)z);
        totalPopulation = threadGroups.x * threadGroups.y * threadGroups.z * (int)x * (int)y * (int)z;

        destroyGrassInBounds.GetKernelThreadGroupSizes(0, out x, out y, out z);
        destroyGrassThreadGroupSize = new Vector3Int((int)x, (int)y, (int)z);

    }

    private void InitializeBuffers()
    {
        UpdateBounds();
        UpdateThreadGroupSizes();
        if (mesh == null) return;


        mainKernel = compute.FindKernel("CSMain");

        //int frustumKernel = frustumCuller.FindKernel("CSMain");

        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)totalPopulation;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        meshPropertiesBuffer = new ComputeBuffer(totalPopulation, MeshProperties.Size());
        matrixesBuffer = new ComputeBuffer(totalPopulation, MatricesStruct.Size(), ComputeBufferType.Default, ComputeBufferMode.Immutable);

        //matrixesBuffer.SetCounterValue(0);

        FillBuffers();
        //frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

        compute.SetBuffer(mainKernel, "_Properties", meshPropertiesBuffer);
        compute.SetBuffer(mainKernel, "_Output", matrixesBuffer);

        SetDispatchSize(compute);
        SetDispatchSize(destroyGrassInBounds);
        SetDispatchSize(destroyGrassInSector);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);
        material.SetBuffer("_Properties", matrixesBuffer);
    }

    private void SetDispatchSize(ComputeShader shader)
    {
        shader.SetInts("dispatchSize", threadGroups.x, threadGroups.y);
    }

    private void FillBuffers()
    {
        if (meshPropertiesBuffer == null ||
            meshPropertiesBuffer.count != totalPopulation) InitializeBuffers();

        // Initialize buffer with the given population.
        NativeArray<MeshProperties> properties = new NativeArray<MeshProperties>(totalPopulation, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < totalPopulation; i++)
        {
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

            if (terrain != null)
                position.y = terrain.SampleHeight(position + transform.position);

            Quaternion rotation = Quaternion.Euler(Mathf.PI, Random.Range(-Mathf.PI, Mathf.PI), 0);
            Vector3 scale = Vector3.one;
            props.position = position;
            props.rotation = rotation.eulerAngles;

            props.size = new Vector2(SampleRandomRange(quadWidthRange), SampleRandomRange(quadHeightRange));

            props.color = colorGradient.Evaluate(Random.value);

            properties[i] = props;
        }

        meshPropertiesBuffer.SetData<MeshProperties>(properties);

        properties.Dispose();
    }


    private static Mesh CreateQuad(float width = 1f, float height = 1f)
    {
        // Create a quad mesh.
        var mesh = new Mesh();

        float w = width * .5f;
        float h = height;
        var vertices = new Vector3[] {
            new Vector3(-w, 0, 0),
            new Vector3(w, 0, 0),
            new Vector3(-w, h, 0),
            new Vector3(w, h, 0),
            //back plane
            new Vector3(-w, 0, 0),
            new Vector3(w, 0, 0),
            new Vector3(-w, h, 0),
            new Vector3(w, h, 0),
            //left plane
            new Vector3(0, 0, -w),
            new Vector3(0, 0, w),
            new Vector3(0, h, -w),
            new Vector3(0, h, w),
            //right plane
            new Vector3(0, 0, -w),
            new Vector3(0, 0, w),
            new Vector3(0, h, -w),
            new Vector3(0, h, w),
        };

        var tris = new int[] {
            // lower left tri.
            0, 2, 1,
            // lower right tri
            2, 3, 1,
//backplane
            // lower left tri.
            4, 5, 6,
            // lower right tri
            6, 5, 7,
//left plane
            // lower left tri.
            8, 9, 10,
            // lower right tri
            10, 9, 11,
//right plane

            // lower left tri.
            8, 10, 9,
            // lower right tri
            10, 11, 9,
        };

        var normals = new Vector3[] {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            //back plane
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,

            //left plane
            Vector3.right,
            Vector3.right,
            Vector3.right,
            Vector3.right,

            //right plane
            -Vector3.right,
            -Vector3.right,
            -Vector3.right,
            -Vector3.right,
        };

        var uv = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),

            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.normals = normals;
        mesh.uv = uv;

        return mesh;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        Setup();
    }

    private void Update()
    {
        //DestroyBladesInBounds();


        CommandBuffer cmd = CommandBufferPool.Get("Grass Compute");

        meshPropertiesBuffer.SetCounterValue((uint)totalPopulation);


        cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
        //Reset the buffer position so old things are written over

        // matrixesBuffer.SetCounterValue(0);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Properties", meshPropertiesBuffer);
        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);

        Vector4[] data = new Vector4[TypeGroup<GrassPusher>.allObjects.Count];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = TypeGroup<GrassPusher>.allObjects[i].Data;
            data[i] -= new Vector4(transform.position.x, transform.position.y, transform.position.z);
        }


        cmd.SetComputeVectorArrayParam(compute, "_PusherPositions", data);
        cmd.SetComputeIntParam(compute, "pushers", data.Length);

        cmd.SetComputeFloatParam(compute, "deltatime", Time.deltaTime);
        cmd.SetComputeFloatParam(compute, "time", Time.time);



        //Copies Properties -> Output with processing
        cmd.DispatchCompute(compute, mainKernel, threadGroups.x, threadGroups.y, threadGroups.z);

        //Swap the buffers and copy them back
        // cmd.SetComputeBufferParam(compute, copyBuffersKernel, "_Properties", meshPropertiesBuffer);
        // cmd.SetComputeBufferParam(compute, copyBuffersKernel, "_Output", matrixesBuffer);
        // cmd.DispatchCompute(compute, copyBuffersKernel, population.x * 64, population.y, population.z);

        // ComputeBuffer.CopyCount(matrixesBuffer, argsBuffer, sizeof(uint));

        Graphics.ExecuteCommandBufferAsync(cmd, ComputeQueueType.Default);

        CommandBufferPool.Release(cmd);

        // uint[] temp = new uint[5];
        // argsBuffer.GetData(temp);
        // Debug.Log(temp[1]);

        // material.SetBuffer("_Properties", meshPropertiesBuffer);

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer, castShadows: shadowCastingMode, receiveShadows: true);
    }

    [MyBox.ButtonMethod]
    public void DestroyBladesInBounds()
    {
        //Send the data needed and destroy grass
        destroyGrassInBounds.SetVector("boundsTransform",
            new Vector4(killingBounds.center.x - transform.position.x,
            killingBounds.center.y - transform.position.y,
            killingBounds.center.z - transform.position.z,
            boundsYRot * Mathf.Deg2Rad));

        destroyGrassInBounds.SetVector("boundsExtents", killingBounds.extents);

        DestroyBlades(destroyGrassInBounds);
    }

    [MyBox.ButtonMethod]
    public void DestroyBladesInSector()
    {
        //Send the data needed and destroy grass
        destroyGrassInSector.SetVector("sectorCenter",
            new Vector4(trackingSector.position.x - transform.position.x, trackingSector.position.z - transform.position.z, 0, 0));

        destroyGrassInSector.SetVector("sectorDirection", new Vector4(trackingSector.forward.x, trackingSector.forward.z, 0, 0).normalized);
        destroyGrassInSector.SetFloat("sectorDot", -0.25f);
        destroyGrassInSector.SetVector("sectorRadiusRange", sectorRange);

        DestroyBlades(destroyGrassInSector);
    }


    public void DestroyBlades(ComputeShader shader)
    {
        //dispatch a compute shader that will take in buffer of all mesh data
        //And return an append buffer of mesh data remaining
        //Then use this buffer as the main buffer

        ComputeBuffer appendMeshData = new ComputeBuffer(totalPopulation, MeshProperties.Size(), ComputeBufferType.Append);
        appendMeshData.SetCounterValue(0);

        // destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
        // destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

        shader.SetBuffer(0, "_Grass", meshPropertiesBuffer);
        shader.SetBuffer(0, "_CulledGrass", appendMeshData);
        shader.SetBuffer(0, "_ArgsData", argsBuffer);

        //Destroy grass shader only works with thread group 1,1,1?
        shader.Dispatch(0, threadGroups.x * mainThreadGroupSize.x, threadGroups.y, threadGroups.z);

        meshPropertiesBuffer.Dispose();

        meshPropertiesBuffer = appendMeshData;

        compute.SetBuffer(mainKernel, "_Properties", meshPropertiesBuffer);

        //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
        ComputeBuffer.CopyCount(meshPropertiesBuffer, argsBuffer, sizeof(uint));
    }


    private void OnDisable()
    {
        DisposeBuffers();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Vector3 localPoint = testPoint - killingBounds.center;
        //Rotate this point around the y axis
        float s = Mathf.Sin(boundsYRot * Mathf.Deg2Rad);
        float c = Mathf.Cos(boundsYRot * Mathf.Deg2Rad);
        localPoint = new Vector3(
            localPoint.x * c - localPoint.z * s,
            localPoint.y,
            localPoint.x * s + localPoint.z * c
        );

        Bounds temp = new Bounds(Vector3.zero, killingBounds.size);

        //Test if this point is within the bounds of the un rotated bounds
        if (temp.Contains(localPoint))
        {
            Gizmos.color = Color.red;

        }
        else
        {
            Gizmos.color = Color.white;

        }


        Matrix4x4 mat = Matrix4x4.TRS(killingBounds.center, Quaternion.Euler(0, boundsYRot, 0), killingBounds.size);

        // Vector3 trans = mat.inverse.MultiplyPoint(testPoint);

        // if (trans.x > -0.5 && trans.x < 0.5 && trans.y > -0.5 && trans.y < 0.5 && trans.z > -0.5 && trans.z < 0.5)
        // {
        //     Gizmos.color = Color.red;
        // }
        // else
        // {
        //     Gizmos.color = Color.white;
        // }

        Gizmos.DrawWireSphere(testPoint, 0.1f);

        Gizmos.color = Color.white;

        Gizmos.matrix = mat;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}