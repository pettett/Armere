using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;


public class GrassController : MonoBehaviour
{
    public readonly int texSize = 32;
    private const string k_RenderGrassTag = "Render Grass";
    private ProfilingSampler m_Grass_Profile;
    public static List<GrassPusher> pushers = new List<GrassPusher>();
    public static GrassController singleton;
    public Vector3Int threadGroups = new Vector3Int(8, 1, 1);
    [System.NonSerialized] public Vector3Int threadGroupSize;
    public int totalPopulation;

    public int currentGrassCellCapacity; //Theretical max grass loaded at once

    [Header("Grass Creation")]
    public float range;
    public ComputeShader createGrassInBoundsCompute;
    public Texture2D gradientTexture;
    public Texture2D grassDensity;
    public int groupsOf8PerCell = 3;
    Queue<CreateGrassInstruction> createGrassInstructions = new Queue<CreateGrassInstruction>();

    [Header("Grass Rendering")]
    public Material material;
    public ComputeShader compute;
    public ComputeShader destroyGrassInBounds;
    public ComputeShader destroyGrassInSector;

    private ComputeBuffer meshPropertiesConsumeBuffer;
    private ComputeBuffer meshPropertiesAppendBuffer;

    private ComputeBuffer matrixesBuffer;
    private ComputeBuffer drawIndirectArgsBuffer;
    private Mesh mesh;
    private Bounds bounds;

    public Terrain terrain;
    public Gradient colorGradient = new Gradient();

    public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
    public Vector2 quadHeightRange = new Vector2(0.5f, 1f);
    Camera mainCamera;
    int mainKernel;
    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public float boundsYRot = 45f;
    public Vector3 testPoint;
    public Bounds killingBounds;
    public Transform trackingSector;
    public Vector2 sectorRange = new Vector2(0.5f, 1.5f);

    QuadTree chunkTree;
    bool[,] cells;

    public readonly struct CreateGrassInstruction
    {
        public readonly int chunkID;
        public readonly Rect rect;
        public readonly int cellsMultiplier;
        public CreateGrassInstruction(int chunkID, Rect rect, int cells)
        {
            this.chunkID = chunkID;
            this.rect = rect;
            this.cellsMultiplier = cells;
        }
    }


    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties
    {
        public Vector3 position;
        public float yRot;
        public Vector2 size;
        public Vector3 color;
        public int chunkID;
        public static int Size()
        {
            return
                //  rotation, position,size
                sizeof(float) * (3 + 1 + 2 + 3) + sizeof(int);
        }
    }

    private struct MatricesStruct
    {
        public Matrix4x4 matrix;
        public Vector3 color;

        public static int Size()
        {
            return sizeof(float) * (4 * 4 + 3);
        }
    }

    public struct RotatedBounds
    {
        public Bounds bounds;
        public float rotation;
    }

    private void Setup()
    {
        Mesh mesh = CreateQuad();
        this.mesh = mesh;

        InitializeBuffers();
    }
    public void UpdateChunkTree()
    {
        Color[] pix = grassDensity.GetPixels();
        if (cells == null) cells = new bool[texSize, texSize];

        for (int x = 0; x < texSize; x++)
        {
            for (int y = 0; y < texSize; y++)
            {
                cells[x, y] = pix[x + y * texSize].r > 0.1f;
            }
        }

        int tempID = 0;
        chunkTree = new QuadTree(cells, Vector2.zero, Vector2.one * range * 2, ref tempID);
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
        drawIndirectArgsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        drawIndirectArgsBuffer.SetData(args);

        meshPropertiesConsumeBuffer = new ComputeBuffer(totalPopulation, MeshProperties.Size(), ComputeBufferType.Append);
        meshPropertiesAppendBuffer = new ComputeBuffer(totalPopulation, MeshProperties.Size(), ComputeBufferType.Append);

        matrixesBuffer = new ComputeBuffer(totalPopulation, MatricesStruct.Size(), ComputeBufferType.Default, ComputeBufferMode.Immutable);

        //matrixesBuffer.SetCounterValue(0);

        PlaceBlades();
        //frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

        compute.SetBuffer(mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        compute.SetBuffer(mainKernel, "_Output", matrixesBuffer);

        createGrassInBoundsCompute.SetBuffer(0, "_Grass", meshPropertiesConsumeBuffer);
        createGrassInBoundsCompute.SetTexture(0, "_Gradient", gradientTexture);

        SetDispatchSize(compute);
        SetDispatchSize(destroyGrassInBounds);
        SetDispatchSize(destroyGrassInSector);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);
        material.SetBuffer("_Properties", matrixesBuffer);
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

            bool destroyedGrass = destroyGrassQueue.Count != 0;
            bool changedGrassCount = destroyedGrass || createGrassInstructions.Count != 0;
            //First set up to destroy all the grass in bounds selected
            while (destroyGrassQueue.Count != 0)
            {
                DestroyBladesInBounds(destroyGrassQueue.Dequeue(), cmd);
            }

            int maxAddIterations = 4;

            while (createGrassInstructions.Count != 0 && maxAddIterations > 0)
            {
                var command = createGrassInstructions.Dequeue();

                //These passes could be done once
                cmd.SetComputeVectorParam(createGrassInBoundsCompute, "boundsMinMax", new Vector4(
                    command.rect.min.x, command.rect.min.y, command.rect.max.x, command.rect.max.y));

                cmd.SetComputeVectorParam(createGrassInBoundsCompute, "grassSizeMinMax", new Vector4(quadWidthRange.x, quadHeightRange.x, quadWidthRange.y, quadHeightRange.y));

                cmd.SetComputeIntParam(createGrassInBoundsCompute, "chunkID", command.chunkID);


                int dispatch = command.cellsMultiplier;

                cmd.DispatchCompute(createGrassInBoundsCompute, 0, command.cellsMultiplier * groupsOf8PerCell, 1, 1);

                currentGrassCellCapacity += command.cellsMultiplier * groupsOf8PerCell * 8;

                maxAddIterations--;
            }

            if (changedGrassCount)
            {
                //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
                cmd.CopyCounterValue(meshPropertiesConsumeBuffer, drawIndirectArgsBuffer, sizeof(uint));
            }


            //Then move the remaining grass

            cmd.SetComputeVectorArrayParam(compute, "_PusherPositions", pushersData);
            cmd.SetComputeIntParam(compute, "pushers", pushersData.Length);


            cmd.SetComputeFloatParam(compute, "deltatime", Time.deltaTime);
            cmd.SetComputeFloatParam(compute, "time", Time.time);

            //Copies Properties -> Output with processing
            cmd.DispatchCompute(compute, mainKernel, threadGroups.x, threadGroups.y, threadGroups.z);

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



    private void SetDispatchSize(ComputeShader shader)
    {
        shader.SetInts("dispatchSize", threadGroups.x, threadGroups.y);
    }

    private void PlaceBlades()
    {
        if (meshPropertiesConsumeBuffer == null ||
            meshPropertiesConsumeBuffer.count != totalPopulation) InitializeBuffers();
        UpdateChunkTree();

        // // // Initialize buffer with the given population.
        // NativeArray<MeshProperties> properties = new NativeArray<MeshProperties>(totalPopulation, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        // for (int i = 0; i < totalPopulation; i++)
        // {
        //     MeshProperties props = new MeshProperties();
        //     Vector3 position = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

        //     if (terrain != null)
        //         position.y = terrain.SampleHeight(position + transform.position);


        //     Vector3 scale = Vector3.one;
        //     props.position = position;
        //     props.yRot = Random.Range(-Mathf.PI, Mathf.PI);

        //     props.size = new Vector2(SampleRandomRange(quadWidthRange), SampleRandomRange(quadHeightRange));
        //     Vector3 col(Color c) => new Vector3(c.r, c.g, c.b);
        //     props.color = col(colorGradient.Evaluate(Random.value));

        //     properties[i] = props;
        // }

        void CreateGrassInTree(QuadTree tree)
        {
            foreach (QuadTreeLeaf leaf in tree)
            {
                if (leaf is QuadTree t)
                {
                    CreateGrassInTree(t);
                }
                else if (leaf is QuadTreeEnd end && end.enabled)
                {
                    createGrassInstructions.Enqueue(new CreateGrassInstruction(end.id, end.rect, end.cellsWidth * end.cellsWidth));
                }
            }
        }
        CreateGrassInTree(chunkTree);

        // meshPropertiesConsumeBuffer.SetData<MeshProperties>(properties);

        // properties.Dispose();
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
        m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);
    }

    private void OnEnable()
    {
        Setup();
        singleton = this;

        RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;

    }

    private void OnDisable()
    {
        DisposeBuffers();
        singleton = null;

        RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
    }

    Vector4[] pushersData = new Vector4[0];
    Queue<RotatedBounds> destroyGrassQueue = new Queue<RotatedBounds>();



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



        //Swap the buffers around
        (meshPropertiesConsumeBuffer, meshPropertiesAppendBuffer) = (meshPropertiesAppendBuffer, meshPropertiesConsumeBuffer);


        //Update the main grass with the new append buffer
        cmd.SetComputeBufferParam(compute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(createGrassInBoundsCompute, 0, "_Grass", meshPropertiesConsumeBuffer);
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

        Gizmos.matrix = Matrix4x4.Translate(transform.position + Vector3.up * 0.1f);


        UpdateChunkTree();
        if (chunkTree != null)
            chunkTree.DrawGizmos();
    }
}