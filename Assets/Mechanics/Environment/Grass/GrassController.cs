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
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct MeshProperties
    {
        public Vector3 position; //3 * 4
        public float yRot; //1 * 4
        public Vector2 size; //2*4
        public Vector3 color;//3 * 4
        public int chunkID;//1 * 4
        public static int Size()
        {
            //  rotation, position,size
            return sizeof(float) * (3 + 1 + 2 + 3) + sizeof(int) * 1;
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
    public readonly struct CreateGrassInstruction
    {
        public readonly Rect rect;
        public readonly int chunks;
        public readonly int chunkID;

        public CreateGrassInstruction(Rect rect, int chunks, int chunkID)
        {
            this.rect = rect;
            this.chunks = chunks;
            this.chunkID = chunkID;
        }
    }


    private const string k_RenderGrassTag = "Render Grass";

    private const string k_FireDensityMap = "FireDensityMap";
    private const string k_FireMap = "FireMap";

    private const float reciprocal255 = 1f / 255f;

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

    //GRASS
    [Header("Chunking")]
    public float range;
    public int chunkSize;

    public bool[] chunksEnabled;
    public bool[,] chunksEnabled2D;
    QuadTree chunkQuadTree;
    public float viewDistance = 5f;
    public float viewDistanceFading = 5f;
    public Material material;

    //Number of multiples of 64 on each chunk
    public int grassGroupsPerChunk = 2;

    public bool createGrassOnGPU = false;

    public ComputeShader AddGrassInBoundsCompute;
    [Header("Grass Movement")]
    public ComputeShader compute;
    [Header("Destroy Grass")]
    public ComputeShader destroyGrassInBounds;
    public ComputeShader destroyGrassInChunkCompute;
    [Header("Fire Compute")]
    public ComputeShader firePropagationCompute;
    public ComputeShader burnGrassCompute;
    public ComputeShader updateFireMapDensityCompute;
    public ComputeShader fireMapToParticleTextureCompute;

    public UnityEngine.VFX.VisualEffect grassBurningEffect;

    [Header("Fire Spread Settings")]
    public float firePropagationSpeed = 10f;
    //Time taken to burn one blade of grass
    public float fireBurnTime = 0.5f;

    public int maxFireParticles = 64;

    [Range(0, 1), Tooltip("Fire value on one tile required to spread to the next")]
    public float fireSpreadThreshold = 0.7f;
    public bool debugTextureView = false;
    [MyBox.ConditionalField("debugTextureView")] public int debugDrawSize = 256;

    [Header("Grass Mesh Settings")]
    public Terrain terrain;
    public Gradient colorGradient = new Gradient();
    public Texture2D gradientTexture;
    [ColorUsage(false)] public Color burntGrassColor = new Color(0.1f, 0.1f, 0.1f);

    public Vector2 minQuadSize = new Vector2(0.5f, 0.27f);
    public Vector2 maxQuadSize = new Vector2(1f, 0.8f);
    Camera mainCamera;
    int mainKernel;
    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public float SampleRandomRange(Vector2 range) => Random.Range(range.x, range.y);

    public float boundsYRot = 45f;
    public Vector3 testPoint;
    public Bounds killingBounds;
    public Transform player;

    public Vector2 sectorRange = new Vector2(0.5f, 1.5f);

    private ComputeBuffer meshPropertiesConsumeBuffer;
    private ComputeBuffer meshPropertiesAppendBuffer;
    private ComputeBuffer matrixesBuffer;
    private ComputeBuffer drawIndirectArgsBuffer;
    private ComputeBuffer fireParticleDataBuffer;
    private RenderTexture fireParticleDataTexture;

    private Mesh mesh;
    private Bounds bounds;

    Vector4[] pushersData = new Vector4[0];
    Vector4[] fireSpreadersData = new Vector4[0];
    Queue<RotatedBounds> destroyGrassQueue = new Queue<RotatedBounds>();
    Queue<int> destroyGrassInChunkQueue = new Queue<int>();

    Queue<CreateGrassInstruction> createGrassQueue = new Queue<CreateGrassInstruction>();

    [System.NonSerialized] bool fireMapDirty = true;
    List<FlammableBody> flammablesInsideBounds = new List<FlammableBody>();
    bool onFire = false;


    QuadTreeEnd[] quadTreeEndsNearPlayer = null;
    // List<QuadTreeEnd> addedChunks = new List<QuadTreeEnd>(10);
    // List<QuadTreeEnd> removedChunks = new List<QuadTreeEnd>(10);


    private void OnValidate()
    {
        maxFireParticles = Mathf.RoundToInt((float)maxFireParticles / 64f) * 64;

        if (chunksEnabled == null || chunksEnabled.Length != chunkSize * chunkSize)
        {
            chunksEnabled = new bool[chunkSize * chunkSize];
        }
        if (chunksEnabled2D == null || chunksEnabled2D.GetLength(0) != chunkSize)
        {
            chunksEnabled2D = new bool[chunkSize, chunkSize];
        }

        System.Buffer.BlockCopy(chunksEnabled, 0, chunksEnabled2D, 0, chunksEnabled.Length);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<FlammableBody>(out var f) && !flammablesInsideBounds.Contains(f))
        {
            flammablesInsideBounds.Add(f);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<FlammableBody>(out var f) && flammablesInsideBounds.Contains(f))
        {
            flammablesInsideBounds.Remove(f);
        }
    }

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

        //Red is current fire value, green is "fuel" so fires don't burn forever
        _fireMap = new RenderTexture(fireMapResolution.x, fireMapResolution.y, 0, RenderTextureFormat.RGHalf);
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
        firePropagationCompute.SetTexture(1, k_FireMap, _fireMap);
        firePropagationCompute.SetTexture(0, k_FireDensityMap, _fireDensityMap);

        burnGrassCompute.SetTexture(0, k_FireMap, _fireMap);

        updateFireMapDensityCompute.SetTexture(0, k_FireDensityMap, _fireDensityMap);
        updateFireMapDensityCompute.SetTexture(1, k_FireDensityMap, _fireDensityMap);


        burnGrassCompute.SetVector("burntColor", burntGrassColor);

        //Quickley reset the color of the fire map
        firePropagationCompute.Dispatch(1, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);


        //Also create the data for the fire particles that will be passed to the vfx graph
        fireParticleDataBuffer = new ComputeBuffer(maxFireParticles, sizeof(float) * 4, ComputeBufferType.Append);
        fireParticleDataTexture = new RenderTexture(maxFireParticles, 1, 0, RenderTextureFormat.ARGBFloat);

        fireParticleDataTexture.enableRandomWrite = true;
        fireParticleDataTexture.filterMode = FilterMode.Point;
        fireParticleDataTexture.Create();


        //This shader will turn the fire map into a list of points for the vfx graph
        fireMapToParticleTextureCompute.SetTexture(0, k_FireMap, _fireMap);
        fireMapToParticleTextureCompute.SetBuffer(0, "ParticleData", fireParticleDataBuffer);

        fireMapToParticleTextureCompute.SetBuffer(1, "ParticleDataBuffer", fireParticleDataBuffer);
        fireMapToParticleTextureCompute.SetTexture(1, "ParticleTexture", fireParticleDataTexture);

        grassBurningEffect.SetTexture("ParticleTexture", fireParticleDataTexture);

        //reset the particle data to zeros
        fireMapToParticleTextureCompute.SetBuffer(2, "ParticleData", fireParticleDataBuffer);
        fireMapToParticleTextureCompute.Dispatch(2, maxFireParticles / 64, 1, 1);

    }

    //Update the density of the grass after grass has been destroyed
    private void UpdateFireMap(CommandBuffer cmd)
    {
        cmd.SetComputeBufferParam(updateFireMapDensityCompute, 0, "_ArgsData", drawIndirectArgsBuffer);

        //Reset the count field
        cmd.DispatchCompute(updateFireMapDensityCompute, 1, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);

        cmd.DispatchCompute(updateFireMapDensityCompute, 0, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);

    }


    //Update the fire map texture's x value to spread fire
    //Also apply the burning to the grass's colour
    private void UpdateFirePropagation(CommandBuffer cmd)
    {
        //Debug.Log("Fire propergating");
        cmd.SetComputeFloatParam(firePropagationCompute, "dt", Time.deltaTime);
        cmd.SetComputeFloatParam(firePropagationCompute, "propagationSpeed", firePropagationSpeed);
        cmd.SetComputeFloatParam(firePropagationCompute, "burnRate", reciprocal255 / fireBurnTime);

        cmd.SetComputeVectorArrayParam(firePropagationCompute, "fireSpreaders", fireSpreadersData);
        cmd.SetComputeIntParam(firePropagationCompute, "fireSpreaderCount", fireSpreadersData.Length);

        cmd.DispatchCompute(firePropagationCompute, 0, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);

        cmd.SetComputeFloatParam(burnGrassCompute, "dt", Time.deltaTime);

        cmd.DispatchCompute(burnGrassCompute, 0, threadGroups.x, threadGroups.y, threadGroups.z);


        //Then update the fire particles
        cmd.SetComputeBufferCounterValue(fireParticleDataBuffer, 0);
        cmd.DispatchCompute(fireMapToParticleTextureCompute, 0, fireMapResolution.x / 8, fireMapResolution.y / 8, 1);
        cmd.DispatchCompute(fireMapToParticleTextureCompute, 1, maxFireParticles / 64, 1, 1);
        //Then clear the buffer
        cmd.DispatchCompute(fireMapToParticleTextureCompute, 2, maxFireParticles / 64, 1, 1);


    }

    private void DisposeBuffers()
    {
        ReleaseBuffer(ref drawIndirectArgsBuffer);
        ReleaseBuffer(ref matrixesBuffer);
        ReleaseBuffer(ref meshPropertiesAppendBuffer);
        ReleaseBuffer(ref meshPropertiesConsumeBuffer);
        ReleaseBuffer(ref fireParticleDataBuffer);
    }

    void ReleaseBuffer(ref ComputeBuffer b)
    {
        b?.Release();
        b = null;
    }
    private void SetDispatchSize(ComputeShader shader)
    {
        shader.SetInts("dispatchSize", threadGroups.x, threadGroups.y);
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

        AddGrassInBoundsCompute.SetTexture(0, "_Gradient", gradientTexture);

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
        SetDispatchSize(destroyGrassInChunkCompute);
        SetDispatchSize(AddGrassInBoundsCompute);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);
        material.SetBuffer("_Properties", matrixesBuffer);
    }


    void OnUpdateMeshPropertiesBuffer()
    {
        compute.SetBuffer(mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        burnGrassCompute.SetBuffer(mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        updateFireMapDensityCompute.SetBuffer(0, "_Properties", meshPropertiesConsumeBuffer);
        AddGrassInBoundsCompute.SetBuffer(0, "_Grass", meshPropertiesConsumeBuffer);
    }
    void OnUpdateMeshPropertiesBuffer(CommandBuffer cmd)
    {
        cmd.SetComputeBufferParam(compute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(burnGrassCompute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(updateFireMapDensityCompute, 0, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(AddGrassInBoundsCompute, 0, "_Grass", meshPropertiesConsumeBuffer);
    }




    private void FillBuffers()
    {
        if (meshPropertiesConsumeBuffer == null ||
            meshPropertiesConsumeBuffer.count != totalPopulation) InitializeBuffers();

        int temp = 0;
        chunkQuadTree = new QuadTree(chunksEnabled2D, Vector2.zero, new Vector2(range * 2, range * 2), ref temp);

        if (!createGrassOnGPU)
        {

            int enabledChunks = chunksEnabled.Where(x => x).Count();
            int bladesPerChunk = totalPopulation / enabledChunks;

            Debug.Log(bladesPerChunk);


            // Initialize buffer with the given population.
            NativeArray<MeshProperties> properties = new NativeArray<MeshProperties>(totalPopulation, Allocator.Temp, NativeArrayOptions.ClearMemory);

            int filledBlades = 0;


            void FillBufferWithTree(QuadTree tree, int chunkCount)
            {
                foreach (var l in tree)
                {
                    if (l is QuadTree t)
                    {
                        FillBufferWithTree(t, chunkCount / 4);
                    }
                    else if (l is QuadTreeEnd e && e.enabled)
                    {
                        //Place blades in this end position
                        //Divide by 4 as this end is only a quater of the values
                        for (int i = filledBlades; i < filledBlades + bladesPerChunk * chunkCount / 4; i++)
                        {

                            MeshProperties props = new MeshProperties();

                            Vector3 position = new Vector3(
                                e.rect.center.x + Random.Range(-e.rect.size.x / 2, e.rect.size.x / 2),
                                0,
                                e.rect.center.y + Random.Range(-e.rect.size.y / 2, e.rect.size.y / 2));

                            if (terrain != null)
                                position.y = terrain.SampleHeight(position + transform.position);


                            Vector3 scale = Vector3.one;
                            props.position = position;
                            props.yRot = Random.Range(-Mathf.PI, Mathf.PI);

                            props.size = new Vector2(
                                SampleRandomRange(new Vector2(minQuadSize.x, maxQuadSize.x)),
                                SampleRandomRange(new Vector2(minQuadSize.y, maxQuadSize.y)));

                            Vector3 ColToVec3(Color col) => new Vector3(col.r, col.g, col.b);

                            props.color = ColToVec3(colorGradient.Evaluate(Random.value));
                            props.chunkID = e.id;
                            properties[i] = props;

                        }
                        filledBlades += bladesPerChunk * chunkCount / 4;
                    }
                }
            }


            FillBufferWithTree(chunkQuadTree, chunkSize * chunkSize);


            //Fill the remaining with empty

            for (int i = filledBlades; i < totalPopulation; i++)
            {
                properties[i] = new MeshProperties();
            }

            meshPropertiesConsumeBuffer.SetData<MeshProperties>(properties);
            meshPropertiesConsumeBuffer.SetCounterValue((uint)filledBlades);
            properties.Dispose();
        }
        else
        {
            meshPropertiesConsumeBuffer.SetCounterValue(0u);
        }
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


    Vector2Int playerChunk;

    QuadTreeEnd[] addedChunks;
    QuadTreeEnd[] removedChunks;
    private void Update()
    {
        if (pushersData == null || pushersData.Length != pushers.Count)
            pushersData = new Vector4[pushers.Count];

        for (int i = 0; i < pushersData.Length; i++)
        {
            pushersData[i] = pushers[i].Data;
            pushersData[i] -= new Vector4(transform.position.x, transform.position.y, transform.position.z);
        }

        int fires = flammablesInsideBounds.Where(x => x != null && x.onFire).Count();

        if (fireSpreadersData.Length != fires)
        {
            fireSpreadersData = new Vector4[fires];
        }

        if (fires > 0)
        {
            onFire = true;
            grassBurningEffect.enabled = onFire;
            int i = 0;
            foreach (var item in flammablesInsideBounds.Where(x => x != null && x.onFire))
            {
                fireSpreadersData[i] = new Vector4(
                    item.transform.position.x - transform.position.x,
                    item.transform.position.y - transform.position.y,
                    item.transform.position.z - transform.position.z,
                    0);
                i++;
            }
        }

        //Get the player's chunk

        Vector2Int newPlayerChunk = new Vector2Int(
            Mathf.FloorToInt((player.position.x - transform.position.x + range) * chunkSize / (range * 2)),
            Mathf.FloorToInt((player.position.z - transform.position.z + range) * chunkSize / (range * 2)));

        if (newPlayerChunk != playerChunk && createGrassOnGPU)
        {
            playerChunk = newPlayerChunk;


            Rect playerRect = new Rect(
                player.position.x - transform.position.x - viewDistance,
                player.position.z - transform.position.z - viewDistance,
                viewDistance * 2, viewDistance * 2);

            QuadTreeEnd[] leaves = chunkQuadTree.GetLeavesInRect(playerRect).Where(x => x.enabled).ToArray();

            if (quadTreeEndsNearPlayer == null) quadTreeEndsNearPlayer = new QuadTreeEnd[0];

            addedChunks = leaves.Except(quadTreeEndsNearPlayer).ToArray();
            removedChunks = quadTreeEndsNearPlayer.Except(leaves).ToArray();

            //Add all the new chunks
            foreach (var end in addedChunks)
                createGrassQueue.Enqueue(new CreateGrassInstruction(end.rect, end.chunksWidth, end.id));

            //Remove all the old chunks
            foreach (var end in removedChunks)
                DestroyBladesInBounds(end.rect, 0, transform.position);

            quadTreeEndsNearPlayer = leaves;
        }

        //Setup the call to draw the grass when the time comes

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, drawIndirectArgsBuffer, castShadows: shadowCastingMode, receiveShadows: true);

        // uint[] temp = new uint[5];
        // drawIndirectArgsBuffer.GetData(temp);
        // Debug.Log($"{temp[1]}, max: {totalPopulation}");
    }

    private void OnGUI()
    {
        if (debugTextureView)
        {
            GUI.DrawTexture(new Rect(0, 0, debugDrawSize, debugDrawSize), _fireMap, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(debugDrawSize, 0, debugDrawSize, debugDrawSize), _fireDensityMap, ScaleMode.ScaleToFit, false);
            GUI.DrawTexture(new Rect(debugDrawSize * 2, 0, debugDrawSize, debugDrawSize), fireParticleDataTexture, ScaleMode.StretchToFill, false);
        }
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
            bool destroyedGrass = destroyGrassQueue.Count != 0 || destroyGrassInChunkQueue.Count != 0;


            while (destroyGrassQueue.Count != 0)
            {
                DestroyBladesInBounds(destroyGrassQueue.Dequeue(), cmd);
            }
            while (destroyGrassInChunkQueue.Count != 0)
            {
                DestroyBladesInChunk(destroyGrassInChunkQueue.Dequeue(), cmd);
            }

            if (destroyedGrass)
            {
                //Update the main grass with the new append buffer
                OnUpdateMeshPropertiesBuffer(cmd);
                fireMapDirty = true;
            }

            bool changedGrassCount = destroyedGrass || createGrassQueue.Count != 0;

            while (createGrassQueue.Count != 0)
            {

                CreateGrassInstruction r = createGrassQueue.Dequeue();
                //Scale dispatch over grass groups per chunk

                //These passes could be done once
                cmd.SetComputeVectorParam(AddGrassInBoundsCompute, "boundsMinMax", new Vector4(
                    r.rect.min.x, r.rect.min.y, r.rect.max.x, r.rect.max.y));

                cmd.SetComputeVectorParam(AddGrassInBoundsCompute, "grassSizeMinMax", new Vector4(minQuadSize.x, minQuadSize.y, maxQuadSize.x, maxQuadSize.y));

                cmd.SetComputeIntParam(AddGrassInBoundsCompute, "chunkID", r.chunkID);

                cmd.DispatchCompute(AddGrassInBoundsCompute, 0, r.chunks * grassGroupsPerChunk, r.chunks, 1);
            }

            if (changedGrassCount)
            {
                //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
                // cmd.CopyCounterValue(meshPropertiesConsumeBuffer, drawIndirectArgsBuffer, sizeof(uint));
                // Debug.Log("Copied counters");
            }


            if (fireMapDirty)
            {
                UpdateFireMap(cmd);
                fireMapDirty = false;
            }



            if (onFire)
                UpdateFirePropagation(cmd);

            UpdateGrassMovement(cmd);

            //Swap the buffers and copy them back

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

        cmd.SetComputeVectorParam(compute, "viewRangeMinMax", new Vector2(viewDistance - viewDistanceFading, viewDistance));
        cmd.SetComputeVectorParam(compute, "cameraPosition", mainCamera.transform.position - transform.position);



        //Copies Properties -> Output with processing
        cmd.DispatchCompute(compute, mainKernel, threadGroups.x, threadGroups.y, threadGroups.z);
    }



    [MyBox.ButtonMethod]
    public void DestroyGrassInKillingBounds()
    {
        DestroyBladesInBounds(killingBounds, boundsYRot * Mathf.Deg2Rad);
    }

    public void DestroyBladesInBounds(Rect rect, float angleRad, Vector3 offset)
    {
        //Send the data needed and destroy grass
        destroyGrassQueue.Enqueue(new RotatedBounds()
        {
            bounds = new Bounds(new Vector3(rect.center.x + offset.x, offset.y, rect.center.y + offset.z), new Vector3(rect.size.x, 5, rect.size.y)),
            rotation = angleRad
        });
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

    public void DestroyBladesInChunk(int chunk, CommandBuffer cmd)
    {
        //Send the data needed and destroy grass

        cmd.SetComputeIntParam(destroyGrassInChunkCompute, "chunkID", chunk);

        DestroyBlades(destroyGrassInChunkCompute, cmd);
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
        //cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

        // destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
        // destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

        cmd.SetComputeBufferParam(shader, 0, "_Grass", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(shader, 0, "_CulledGrass", meshPropertiesAppendBuffer);
        cmd.SetComputeBufferParam(shader, 0, "_ArgsData", drawIndirectArgsBuffer);

        cmd.DispatchCompute(shader, 0, threadGroups.x, threadGroups.y, threadGroups.y);

        //Swap the buffers around
        (meshPropertiesConsumeBuffer, meshPropertiesAppendBuffer) = (meshPropertiesAppendBuffer, meshPropertiesConsumeBuffer);


    }


    private void OnDrawGizmosSelected()
    {
        //Draw quad tree structure


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
        int tempID = 0;
        chunkQuadTree = new QuadTree(chunksEnabled2D, Vector2.zero, new Vector2(range * 2, range * 2), ref tempID);

        Vector2Int testPointChunk = new Vector2Int(
            Mathf.FloorToInt((testPoint.x - transform.position.x + range) * chunkSize / (range * 2)),
            Mathf.FloorToInt((testPoint.z - transform.position.z + range) * chunkSize / (range * 2)));


        Vector3 chunkScale = new Vector3(1, 0, 1) * range * 2 / chunkSize;
        Vector3 chunkCenter = new Vector3(
            testPointChunk.x * range * 2 / chunkSize - range + chunkScale.x / 2, 0,
            testPointChunk.y * range * 2 / chunkSize - range + chunkScale.z / 2);


        if (createGrassOnGPU)
        {
            foreach (var l in quadTreeEndsNearPlayer)
            {
                l.DrawGizmos();
            }
            Gizmos.color = new Color(1, 0, 1, 0.5f);
            foreach (var l in addedChunks)
            {
                l.DrawGizmos(false);
            }
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            foreach (var l in removedChunks)
            {
                l.DrawGizmos(false);
            }
        }
        else
        {
            Gizmos.DrawCube(chunkCenter, chunkScale);

            chunkQuadTree.DrawGizmos();
        }
    }
}