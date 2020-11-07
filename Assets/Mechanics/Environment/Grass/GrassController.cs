using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;


public class GrassController : MonoBehaviour
{


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

    public struct RotatedBounds
    {
        public Bounds bounds;
        public float rotation;
    }



    private const string k_RenderGrassTag = "Render Grass";

    private const string k_FireDensityMap = "FireDensityMap";
    private const string k_FireMap = "FireMap";

    private ProfilingSampler m_Grass_Profile;

    public static List<GrassPusher> pushers = new List<GrassPusher>();

    public static GrassController singleton;

    //COMPUTE SHADERS
    public Vector3Int threadGroups = new Vector3Int(8, 1, 1);
    [System.NonSerialized] public Vector3Int threadGroupSize;
    public int totalPopulation;

    //FIRE
    public Vector2Int fireMapResolution = new Vector2Int(16, 16);
    private RenderTexture _fireMap;
    private RenderTexture _fireDensityMap;
    public int debugDrawSize = 256;
    //GRASS
    public float range;

    public Material material;
    [Header("Grass Movement")]
    public ComputeShader compute;
    [Header("Destroy Grass")]
    public ComputeShader destroyGrassInBounds;
    public ComputeShader destroyGrassInSector;
    [Header("Fire Compute")]
    public ComputeShader firePropagationCompute;
    public ComputeShader burnGrassCompute;
    public ComputeShader updateFireMapDensityCompute;


    [Header("Fire Spread Settings")]
    public float fireSpreadSpeed = 10f;

    [Range(0, 1), Tooltip("Fire value on one tile required to spread to the next")]
    public float fireSpreadThreshold = 0.7f;



    [Header("Grass Mesh Settings")]
    public Terrain terrain;
    public Gradient colorGradient = new Gradient();
    [ColorUsage(false)] public Color burntGrassColor = new Color(0.1f, 0.1f, 0.1f);

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

    private ComputeBuffer meshPropertiesConsumeBuffer;
    private ComputeBuffer meshPropertiesAppendBuffer;

    private ComputeBuffer matrixesBuffer;
    private ComputeBuffer drawIndirectArgsBuffer;
    private Mesh mesh;
    private Bounds bounds;

    Vector4[] pushersData = new Vector4[0];
    Queue<RotatedBounds> destroyGrassQueue = new Queue<RotatedBounds>();

    public bool fireMapDirty = true;


    private void Setup()
    {
        Mesh mesh = CreateQuad();
        this.mesh = mesh;


        CreateFireMap();

        InitializeBuffers();

        SetupComputeShaders();
    }


    private void CreateFireMap()
    {
        _fireMap = new RenderTexture(fireMapResolution.x, fireMapResolution.y, 0, RenderTextureFormat.RHalf);
        _fireMap.wrapMode = TextureWrapMode.Clamp;
        _fireMap.enableRandomWrite = true;
        _fireMap.Create();


        _fireDensityMap = new RenderTexture(fireMapResolution.x, fireMapResolution.y, 0, RenderTextureFormat.R8);
        _fireDensityMap.filterMode = FilterMode.Point;
        _fireDensityMap.wrapMode = TextureWrapMode.Clamp;
        _fireDensityMap.enableRandomWrite = true;
        _fireDensityMap.Create();

        //Give the fire map to all compute shaders that will need it
        firePropagationCompute.SetTexture(0, k_FireMap, _fireMap);
        firePropagationCompute.SetTexture(0, k_FireDensityMap, _fireDensityMap);

        burnGrassCompute.SetTexture(0, k_FireMap, _fireMap);

        updateFireMapDensityCompute.SetTexture(0, k_FireDensityMap, _fireDensityMap);
        updateFireMapDensityCompute.SetTexture(1, k_FireDensityMap, _fireDensityMap);


        burnGrassCompute.SetVector("burntColor", burntGrassColor);
    }

    //Update the density of the grass after grass has been destroyed
    private void UpdateFireMap(CommandBuffer cmd)
    {
        cmd.SetComputeBufferParam(updateFireMapDensityCompute, 0, "_ArgsData", drawIndirectArgsBuffer);

        //Reset the count field
        cmd.DispatchCompute(updateFireMapDensityCompute, 1, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);

        SetDispatchSize(updateFireMapDensityCompute);

        cmd.DispatchCompute(updateFireMapDensityCompute, 0, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);

    }


    //Update the fire map texture's x value to spread fire
    //Also apply the burning to the grass's colour
    private void UpdateFirePropagation(CommandBuffer cmd)
    {
        cmd.SetComputeFloatParam(firePropagationCompute, "dt", Time.deltaTime * fireSpreadSpeed);

        cmd.DispatchCompute(firePropagationCompute, 0, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);

        SetDispatchSize(burnGrassCompute);
        cmd.SetComputeFloatParam(burnGrassCompute, "dt", Time.deltaTime);

        cmd.DispatchCompute(burnGrassCompute, 0, threadGroups.x, threadGroups.y, threadGroups.z);
    }

    private void DisposeBuffers()
    {
        if (meshPropertiesConsumeBuffer != null)
        {
            meshPropertiesConsumeBuffer.Release();
        }
        meshPropertiesConsumeBuffer = null;

        if (meshPropertiesAppendBuffer != null)
        {
            meshPropertiesAppendBuffer.Release();
        }
        meshPropertiesAppendBuffer = null;

        if (matrixesBuffer != null)
        {
            matrixesBuffer.Release();
        }
        matrixesBuffer = null;

        if (drawIndirectArgsBuffer != null)
        {
            drawIndirectArgsBuffer.Release();
        }
        drawIndirectArgsBuffer = null;
    }

    void UpdateBounds()
    {
        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range * 2 + 1));
    }

    void UpdateThreadGroupSizes()
    {
        compute.GetKernelThreadGroupSizes(mainKernel, out uint x, out uint y, out uint z);
        threadGroupSize = new Vector3Int((int)x, (int)y, (int)z);
        totalPopulation = threadGroups.x * threadGroups.y * threadGroups.z * (int)x * (int)y * (int)z;
    }

    private void SetupComputeShaders()
    {
        firePropagationCompute.SetFloat("spreadThreshold", fireSpreadThreshold);
    }

    private void InitializeBuffers()
    {


        mainKernel = compute.FindKernel("CSMain");

        UpdateBounds();
        UpdateThreadGroupSizes();
        if (mesh == null) return;


        //int frustumKernel = frustumCuller.FindKernel("CSMain");

        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = mesh.GetIndexCount(0);
        args[1] = (uint)totalPopulation;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        drawIndirectArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        drawIndirectArgsBuffer.SetData(args);

        meshPropertiesConsumeBuffer = new ComputeBuffer(totalPopulation, MeshProperties.Size(), ComputeBufferType.Append);
        meshPropertiesAppendBuffer = new ComputeBuffer(totalPopulation, MeshProperties.Size(), ComputeBufferType.Append);

        matrixesBuffer = new ComputeBuffer(totalPopulation, MatricesStruct.Size(), ComputeBufferType.Default, ComputeBufferMode.Immutable);

        //matrixesBuffer.SetCounterValue(0);

        FillBuffers();
        //frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

        OnUpdateMeshPropertiesBuffer();

        compute.SetBuffer(mainKernel, "_Output", matrixesBuffer);

        SetDispatchSize(compute);
        SetDispatchSize(destroyGrassInBounds);
        SetDispatchSize(destroyGrassInSector);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);
        material.SetBuffer("_Properties", matrixesBuffer);
    }


    void OnUpdateMeshPropertiesBuffer()
    {
        compute.SetBuffer(mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        burnGrassCompute.SetBuffer(mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        updateFireMapDensityCompute.SetBuffer(0, "_Properties", meshPropertiesConsumeBuffer);
    }
    void OnUpdateMeshPropertiesBuffer(CommandBuffer cmd)
    {
        cmd.SetComputeBufferParam(compute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(burnGrassCompute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(updateFireMapDensityCompute, 0, "_Properties", meshPropertiesConsumeBuffer);
    }


    private void SetDispatchSize(ComputeShader shader)
    {
        shader.SetInts("dispatchSize", threadGroups.x, threadGroups.y);
    }


    private void FillBuffers()
    {
        if (meshPropertiesConsumeBuffer == null ||
            meshPropertiesConsumeBuffer.count != totalPopulation) InitializeBuffers();

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

        meshPropertiesConsumeBuffer.SetData<MeshProperties>(properties);

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

    //Functions called over lifetime (in logical order)
    private void OnEnable()
    {
        Setup();
        singleton = this;

        RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);
    }

    private void Update()
    {
        if (pushersData == null || pushersData.Length != pushers.Count)
            pushersData = new Vector4[pushers.Count];

        for (int i = 0; i < pushersData.Length; i++)
        {
            pushersData[i] = pushers[i].Data;
            pushersData[i] -= new Vector4(transform.position.x, transform.position.y, transform.position.z);
        }


        //Setup the call to draw the grass when the time comes

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, drawIndirectArgsBuffer, castShadows: shadowCastingMode, receiveShadows: true);

        // uint[] temp = new uint[5];
        // drawIndirectArgsBuffer.GetData(temp);
        // Debug.Log($"{temp[1]}, max: {totalPopulation}");
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, debugDrawSize, debugDrawSize), _fireMap, ScaleMode.ScaleToFit, false);
        GUI.DrawTexture(new Rect(debugDrawSize, 0, debugDrawSize, debugDrawSize), _fireDensityMap, ScaleMode.ScaleToFit, false);
    }

    private void OnDisable()
    {
        DisposeBuffers();
        singleton = null;

        RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
    }



    void OnBeginCameraRendering(ScriptableRenderContext context, Camera[] camera)
    {
        //This is called once per frame no matter the number of cameras

        //DestroyBladesInBounds();



        if (Time.deltaTime == 0) return; //No need to update grass - nothing has happened

        CommandBuffer cmd = CommandBufferPool.Get(k_RenderGrassTag);

        using (new ProfilingScope(cmd, m_Grass_Profile))
        {

            //meshPropertiesBuffer.SetCounterValue((uint)totalPopulation);

            cmd.Clear();
            //cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);

            //First set up to destroy all the grass in bounds selected
            bool destroyedGrass = destroyGrassQueue.Count != 0;
            while (destroyGrassQueue.Count != 0)
            {
                DestroyBladesInBounds(destroyGrassQueue.Dequeue(), cmd);
            }
            if (destroyedGrass)
            {
                fireMapDirty = true;
            }

            if (fireMapDirty)
            {
                UpdateFireMap(cmd);
                fireMapDirty = false;
            }


            UpdateFirePropagation(cmd);

            UpdateGrassMovement(cmd);

            //Swap the buffers and copy them back
            // cmd.SetComputeBufferParam(compute, copyBuffersKernel, "_Properties", meshPropertiesBuffer);
            // cmd.SetComputeBufferParam(compute, copyBuffersKernel, "_Output", matrixesBuffer);
            // cmd.DispatchCompute(compute, copyBuffersKernel, population.x * 64, population.y, population.z);

            // ComputeBuffer.CopyCount(matrixesBuffer, argsBuffer, sizeof(uint));

            context.ExecuteCommandBuffer(cmd);
        }

        CommandBufferPool.Release(cmd);

        // material.SetBuffer("_Properties", meshPropertiesBuffer);
    }


    private void UpdateGrassMovement(CommandBuffer cmd)
    {
        //Then move the remaining grass

        cmd.SetComputeVectorArrayParam(compute, "_PusherPositions", pushersData);
        cmd.SetComputeIntParam(compute, "pushers", pushersData.Length);


        cmd.SetComputeFloatParam(compute, "deltatime", Time.deltaTime);
        cmd.SetComputeFloatParam(compute, "time", Time.time);

        //Copies Properties -> Output with processing
        cmd.DispatchCompute(compute, mainKernel, threadGroups.x, threadGroups.y, threadGroups.z);
    }



    [MyBox.ButtonMethod]
    public void DestroyGrassInKillingBounds()
    {
        DestroyBladesInBounds(killingBounds, boundsYRot * Mathf.Deg2Rad);
    }
    public void DestroyBladesInBounds(Bounds bounds, float angleRad)
    {
        //Send the data needed and destroy grass
        destroyGrassQueue.Enqueue(new RotatedBounds() { bounds = bounds, rotation = angleRad });
    }


    public void DestroyBladesInBounds(RotatedBounds bounds, CommandBuffer cmd)
    {
        //Send the data needed and destroy grass
        cmd.SetComputeVectorParam(destroyGrassInBounds, "boundsTransform",
            new Vector4(bounds.bounds.center.x - transform.position.x,
            bounds.bounds.center.y - transform.position.y,
            bounds.bounds.center.z - transform.position.z,
            bounds.rotation));

        cmd.SetComputeVectorParam(destroyGrassInBounds, "boundsExtents", bounds.bounds.extents);

        DestroyBlades(destroyGrassInBounds, cmd);
    }


    // [MyBox.ButtonMethod]
    // public void DestroyBladesInSector()
    // {
    //     //Send the data needed and destroy grass
    //     destroyGrassInSector.SetVector("sectorCenter",
    //         new Vector4(trackingSector.position.x - transform.position.x, trackingSector.position.z - transform.position.z, 0, 0));

    //     destroyGrassInSector.SetVector("sectorDirection", new Vector4(trackingSector.forward.x, trackingSector.forward.z, 0, 0).normalized);
    //     destroyGrassInSector.SetFloat("sectorDot", -0.25f);
    //     destroyGrassInSector.SetVector("sectorRadiusRange", sectorRange);

    //     DestroyBlades(destroyGrassInSector);
    // }


    public void DestroyBlades(ComputeShader shader, CommandBuffer cmd)
    {
        //dispatch a compute shader that will take in buffer of all mesh data
        //And return an append buffer of mesh data remaining
        //Then use this buffer as the main buffer


        cmd.SetComputeBufferCounterValue(meshPropertiesAppendBuffer, 0);
        //  cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

        // destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
        // destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

        cmd.SetComputeBufferParam(shader, 0, "_Grass", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(shader, 0, "_CulledGrass", meshPropertiesAppendBuffer);
        cmd.SetComputeBufferParam(shader, 0, "_ArgsData", drawIndirectArgsBuffer);


        cmd.DispatchCompute(shader, 0, threadGroups.x, threadGroups.y, threadGroups.z);

        //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
        cmd.CopyCounterValue(meshPropertiesAppendBuffer, drawIndirectArgsBuffer, sizeof(uint));

        //Swap the buffers around
        (meshPropertiesConsumeBuffer, meshPropertiesAppendBuffer) = (meshPropertiesAppendBuffer, meshPropertiesConsumeBuffer);


        //Update the main grass with the new append buffer
        OnUpdateMeshPropertiesBuffer(cmd);

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