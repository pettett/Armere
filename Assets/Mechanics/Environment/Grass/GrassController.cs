using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using System;

public class GrassController : MonoBehaviour
{
    [System.Serializable]
    public class GrassLayer
    {
        public enum LayerType { Main, Detail }
        public bool enabled = true;
        public LayerType layerType;
        public Vector3Int threadGroups = new Vector3Int(8, 1, 1);
        [System.NonSerialized] public Vector3Int threadGroupSize;
        [System.NonSerialized] public bool inited;
        public int totalPopulation;
        public int groupsOf8PerCellGroup = 3;
        public int currentGrassCellCapacity; //Theretical max grass loaded at once
        public int splatMapLayer = 0;
        public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
        public Vector2 quadHeightRange = new Vector2(0.5f, 1f);

        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public ComputeBuffer meshPropertiesConsumeBuffer;
        public ComputeBuffer meshPropertiesAppendBuffer;

        public ComputeBuffer matrixesBuffer;
        public ComputeBuffer drawIndirectArgsBuffer;
        public Mesh mesh;
        public Texture2D texture;
        [Range(0f, 1f)]
        public float viewDistanceScalar = 1;

        [Header("Quad tree generation")]
        public ushort smallestCellGroupPower = 1;
        public short greatestCellGroupPower = 5;

        public QuadTree chunkTree;
        Queue<GrassInstruction> localInstructions = new Queue<GrassInstruction>();
        [System.NonSerialized] public QuadTreeEnd[] endsInRange = new QuadTreeEnd[0];

        public void UpdateChunkTree(GrassController c)
        {
            Texture2D grassDensity = c.terrain.terrainData.alphamapTextures[0];
            int texSize = grassDensity.width;
            bool[,] cells = new bool[texSize, texSize];

            Color[] pix = grassDensity.GetPixels();

            for (int x = 0; x < texSize; x++)
            {
                for (int y = 0; y < texSize; y++)
                {
                    cells[x, y] = pix[x + y * texSize][splatMapLayer] > 0.1f;
                }
            }

            int tempID = 0;
            chunkTree = QuadTree.CreateQuadTree(cells, Vector2.one * 0.5f, Vector2.one, 1 << smallestCellGroupPower, 1 << greatestCellGroupPower, ref tempID);
        }

        public int seed { get; private set; }
        MaterialPropertyBlock block;
        public void InitLayer(GrassController c, int index)
        {
            seed = Mathf.CeilToInt(c.range * 2 * index);

            drawIndirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

            meshPropertiesConsumeBuffer = new ComputeBuffer(totalPopulation, MeshProperties.size, ComputeBufferType.Append, ComputeBufferMode.Immutable);
            meshPropertiesAppendBuffer = new ComputeBuffer(totalPopulation, MeshProperties.size, ComputeBufferType.Append, ComputeBufferMode.Immutable);

            matrixesBuffer = new ComputeBuffer(totalPopulation, MatricesStruct.size, ComputeBufferType.Default, ComputeBufferMode.Immutable);

            //material.SetBuffer("_Properties", matrixesBuffer);
            block = new MaterialPropertyBlock();
            block.SetBuffer("_Properties", matrixesBuffer);
            block.SetTexture("_BaseMap", texture);
        }
        private void SetDispatchSize(ComputeShader shader, CommandBuffer cmd)
        {
            cmd.SetComputeIntParams(shader, "dispatchSize", threadGroups.x, threadGroups.y);
        }
        public void InitComputeShaders(GrassController c, CommandBuffer cmd)
        {
            // Argument buffer used by DrawMeshInstancedIndirect.
            uint[] args = new uint[5];
            // Arguments for drawing mesh.
            // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
            args[0] = mesh.GetIndexCount(0);
            args[1] = 0;
            args[2] = mesh.GetIndexStart(0);
            args[3] = mesh.GetBaseVertex(0);
            args[4] = 0; //Start instance location

            cmd.SetComputeBufferData(drawIndirectArgsBuffer, args);

            cmd.SetComputeBufferCounterValue(meshPropertiesAppendBuffer, 0);
            cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

            SetDispatchSize(c.compute, cmd);
            SetDispatchSize(c.destroyGrassInBounds, cmd);
            SetDispatchSize(c.destroyGrassInChunk, cmd);


            cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, "_Gradient", c.gradientTexture);

            Texture2D grassDensity = c.terrain.terrainData.alphamapTextures[0];

            cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, "_Density", grassDensity);

            if (c.terrain != null)
            {
                RenderTexture grassHeight = c.terrain.terrainData.heightmapTexture;

                cmd.SetComputeTextureParam(c.createGrassInBoundsCompute, 0, "_Height", c.terrain.terrainData.heightmapTexture);
                cmd.SetComputeFloatParam(c.createGrassInBoundsCompute, "grassHeightScale", c.terrain.terrainData.heightmapScale.y / 128f);
            }

            inited = true;
        }

        public void SetBuffers(GrassController c, CommandBuffer cmd)
        {

            cmd.SetComputeBufferParam(c.compute, 0, "_Properties", meshPropertiesConsumeBuffer);
            cmd.SetComputeBufferParam(c.compute, 0, "_Output", matrixesBuffer);

            cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, "_Grass", meshPropertiesConsumeBuffer);

            cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, "_IndirectArgs", drawIndirectArgsBuffer);
        }
        public void OnCameraBeginRendering(GrassController c, CommandBuffer cmd)
        {
            bool grassChanged = false;
            bool consumeChanged = false;
            while (localInstructions.Count != 0)
            {
                localInstructions.Dequeue().Execute(c, this, cmd, ref grassChanged, ref consumeChanged);
            }

            if (consumeChanged)
            {
                UpdateConsumeBufferBindings(c, cmd);
            }

            if (grassChanged)
            {
                //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
                cmd.CopyCounterValue(meshPropertiesConsumeBuffer, drawIndirectArgsBuffer, sizeof(uint));
            }

            if (layerType == LayerType.Main)
            {
                cmd.SetComputeIntParam(c.compute, "pushers", c.pushersData.Length);
            }
            else
            {
                cmd.SetComputeIntParam(c.compute, "pushers", 0);
            }

            //Copies Properties -> Output with processing
            cmd.DispatchCompute(c.compute, c.mainKernel, threadGroups.x, threadGroups.y, threadGroups.z);
        }
        public void DrawGrassLayer(GrassController c)
        {

            Rect playerVision = new Rect(
                (GrassPusher.mainPusher.transform.position.x - c.transform.position.x - c.viewDistance * viewDistanceScalar) / (c.range * 2),
                (GrassPusher.mainPusher.transform.position.z - c.transform.position.z - c.viewDistance * viewDistanceScalar) / (c.range * 2),
                c.viewDistance * viewDistanceScalar / c.range,
                c.viewDistance * viewDistanceScalar / c.range);

            var chunksInView = chunkTree.GetLeavesInRect(playerVision).ToArray();

            IEnumerable<QuadTreeEnd> addedChunks = chunksInView.Except(endsInRange);
            IEnumerable<QuadTreeEnd> removedChunks = endsInRange.Except(chunksInView);

            foreach (var chunk in removedChunks)
            {
                localInstructions.Enqueue(new DestroyGrassInChunkInstruction(chunk.id,
                       chunk.cellsWidth * chunk.cellsWidth));
            }
            foreach (var chunk in addedChunks)
            {

                localInstructions.Enqueue(new CreateGrassInstruction(chunk.id, chunk.rect,
                        chunk.cellsWidth * chunk.cellsWidth));
            }

            endsInRange = chunksInView;


            if (inited)
            {
                Graphics.DrawMeshInstancedIndirect(
                    mesh, 0, c.material, c.bounds, drawIndirectArgsBuffer,
                    castShadows: shadowCastingMode, receiveShadows: true, properties: block);
            }
        }

        public void UpdateThreadGroupSizes(GrassController c)
        {
            c.compute.GetKernelThreadGroupSizes(0, out uint x, out uint y, out uint z);
            threadGroupSize = new Vector3Int((int)x, (int)y, (int)z);
            totalPopulation = threadGroups.x * threadGroups.y * threadGroups.z * (int)x * (int)y * (int)z;


        }

        public void UpdateConsumeBufferBindings(GrassController c, CommandBuffer cmd)
        {
            //Update the main grass with the new append buffer
            cmd.SetComputeBufferParam(c.compute, 0, "_Properties", meshPropertiesConsumeBuffer);
            cmd.SetComputeBufferParam(c.createGrassInBoundsCompute, 0, "_Grass", meshPropertiesConsumeBuffer);

        }
        public void DisposeBuffers()
        {
            DisposeBuffer(ref meshPropertiesConsumeBuffer);
            DisposeBuffer(ref meshPropertiesAppendBuffer);
            DisposeBuffer(ref matrixesBuffer);
            DisposeBuffer(ref drawIndirectArgsBuffer);
        }

        private static void DisposeBuffer(ref ComputeBuffer buffer)
        {
            buffer?.Release();
            buffer = null;
        }

    }

    public GrassLayer[] layers = new GrassLayer[2];


    private const string k_RenderGrassTag = "Render Grass";
    private ProfilingSampler m_Grass_Profile;
    public static List<GrassPusher> pushers = new List<GrassPusher>();
    public static GrassController singleton;



    [Header("Grass Creation")]
    public float range;
    public float offset;
    public ComputeShader createGrassInBoundsCompute;
    public Texture2D gradientTexture;
    public Terrain terrain;

    Queue<GrassInstruction> grassInstructions = new Queue<GrassInstruction>();

    [Header("Grass Rendering")]
    public float viewDistance = 10;
    public ComputeShader compute;
    public ComputeShader destroyGrassInBounds;
    public ComputeShader destroyGrassInChunk;

    public Material material;
    private Bounds bounds;


    Camera mainCamera;
    int mainKernel;

    public float boundsYRot = 45f;
    public Vector3 testPoint;
    public Bounds killingBounds;


    public interface GrassInstruction
    {
        void Execute(GrassController controller, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged);
    }

    public readonly struct CreateGrassInstruction : GrassInstruction
    {
        public readonly int chunkID;
        public readonly Rect textureRect;
        public readonly int cellsArea;
        public CreateGrassInstruction(int chunkID, Rect textureRect, int cells)
        {
            this.chunkID = chunkID;
            this.textureRect = textureRect;
            this.cellsArea = cells;
        }

        public void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged)
        {
            if (consumeBufferChanged)
            {
                layer.UpdateConsumeBufferBindings(c, cmd);
                consumeBufferChanged = false;
            }

            //These passes could be done once
            cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, "grassDensityUVMinMax",
                new Vector4(textureRect.min.x, textureRect.min.y, textureRect.max.x, textureRect.max.y));

            cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, "grassPositionBoundsMinMax",
                new Vector4(
                    textureRect.min.x * c.range * 2 - c.range,
                    textureRect.min.y * c.range * 2 - c.range,
                    textureRect.max.x * c.range * 2 - c.range,
                    textureRect.max.y * c.range * 2 - c.range));


            cmd.SetComputeVectorParam(c.createGrassInBoundsCompute, "grassSizeMinMax",
                new Vector4(layer.quadWidthRange.x, layer.quadHeightRange.x, layer.quadWidthRange.y, layer.quadHeightRange.y));

            cmd.SetComputeIntParam(c.createGrassInBoundsCompute, "chunkID", chunkID);
            cmd.SetComputeIntParam(c.createGrassInBoundsCompute, "seed", layer.seed);
            cmd.SetComputeIntParam(c.createGrassInBoundsCompute, "densityLayer", layer.splatMapLayer);

            int dispatch = cellsArea * layer.groupsOf8PerCellGroup;

            cmd.DispatchCompute(c.createGrassInBoundsCompute, 0, dispatch, 1, 1);

            //Only the max amount - rejection sampling makes this lower
            layer.currentGrassCellCapacity += dispatch * 8;
            //Update the sizes
            //cmd.SetComputeBufferData(c.drawIndirectArgsBuffer, new uint[] { (uint)c.currentGrassCellCapacity }, 0, 1, 1);

            grassCountChanged = true;
        }
    }
    public readonly struct DestroyGrassInBoundsInstruction : GrassInstruction
    {
        public readonly Bounds bounds;
        public readonly float rotation;

        public DestroyGrassInBoundsInstruction(Bounds bounds, float rotation)
        {
            this.bounds = bounds;
            this.rotation = rotation;
        }

        public void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged)
        {
            grassCountChanged = true;
            if (grassCountChanged)
            {
                //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
                cmd.CopyCounterValue(layer.meshPropertiesConsumeBuffer, layer.drawIndirectArgsBuffer, sizeof(uint));
            }

            consumeBufferChanged = true;

            //Send the data needed and destroy grass
            cmd.SetComputeVectorParam(c.destroyGrassInBounds, "boundsTransform",
                new Vector4(bounds.center.x - c.bounds.center.x,
                            bounds.center.y - c.bounds.center.y,
                            bounds.center.z - c.bounds.center.z,
                            rotation));

            cmd.SetComputeVectorParam(c.destroyGrassInBounds, "boundsExtents", bounds.extents);



            //dispatch a compute shader that will take in buffer of all mesh data
            //And return an append buffer of mesh data remaining
            //Then use this buffer as the main buffer


            cmd.SetComputeBufferCounterValue(layer.meshPropertiesAppendBuffer, 0);
            //  cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

            // destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
            // destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

            cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, "_Grass", layer.meshPropertiesConsumeBuffer);
            cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, "_CulledGrass", layer.meshPropertiesAppendBuffer);
            cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, "_ArgsData", layer.drawIndirectArgsBuffer);


            cmd.DispatchCompute(c.destroyGrassInBounds, 0, layer.threadGroups.x, layer.threadGroups.y, layer.threadGroups.z);



            //Swap the buffers around
            (layer.meshPropertiesConsumeBuffer, layer.meshPropertiesAppendBuffer) = (layer.meshPropertiesAppendBuffer, layer.meshPropertiesConsumeBuffer);
        }
    }

    public readonly struct DestroyGrassInChunkInstruction : GrassInstruction
    {
        public readonly int chunkID;
        public readonly int chunkArea;

        public DestroyGrassInChunkInstruction(int chunkID, int area)
        {
            this.chunkID = chunkID;
            this.chunkArea = area;
        }

        public void Execute(GrassController c, GrassLayer layer, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged)
        {


            if (grassCountChanged)
            {
                //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
                cmd.CopyCounterValue(layer.meshPropertiesConsumeBuffer, layer.drawIndirectArgsBuffer, sizeof(uint));
            }


            grassCountChanged = true;
            consumeBufferChanged = true;

            //Send the data needed and destroy grass

            cmd.SetComputeIntParam(c.destroyGrassInChunk, "chunkID", chunkID);

            //dispatch a compute shader that will take in buffer of all mesh data
            //And return an append buffer of mesh data remaining
            //Then use this buffer as the main buffer

            cmd.SetComputeBufferCounterValue(layer.meshPropertiesAppendBuffer, 0);
            //  cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

            // destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
            // destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

            cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, "_Grass", layer.meshPropertiesConsumeBuffer);
            cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, "_CulledGrass", layer.meshPropertiesAppendBuffer);
            cmd.SetComputeBufferParam(c.destroyGrassInChunk, 0, "_ArgsData", layer.drawIndirectArgsBuffer);

            cmd.DispatchCompute(c.destroyGrassInChunk, 0, layer.threadGroups.x, layer.threadGroups.y, layer.threadGroups.z);

            layer.currentGrassCellCapacity -= chunkArea * layer.groupsOf8PerCellGroup * 8;

            //Swap the buffers around
            (layer.meshPropertiesConsumeBuffer, layer.meshPropertiesAppendBuffer) = (layer.meshPropertiesAppendBuffer, layer.meshPropertiesConsumeBuffer);


        }

    }



    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties
    {

        //  rotation, position,size
        public const int size = sizeof(float) * (3 + 1 + 2 + 3) + sizeof(int);
    }

    private struct MatricesStruct
    {
        public const int size = sizeof(float) * (4 * 4 + 3);
    }



    private void Setup()
    {
        InitializeBuffers();
    }


    private void InitializeBuffers()
    {
        UpdateBounds();


        mainKernel = compute.FindKernel("CSMain");

        //int frustumKernel = frustumCuller.FindKernel("CSMain");
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateThreadGroupSizes(this);
            layers[i].InitLayer(this, i);
        }


        //matrixesBuffer.SetCounterValue(0);

        PlaceBlades();
        //frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);

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

            //Set common variables for movement

            cmd.SetComputeVectorArrayParam(compute, "_PusherPositions", pushersData);



            cmd.SetComputeFloatParam(compute, "deltatime", Time.deltaTime);
            cmd.SetComputeFloatParam(compute, "time", Time.time);

            int maxInstructionIterations = 8;

            List<GrassInstruction> instructionsThisFrame = new List<GrassInstruction>(Mathf.Min(grassInstructions.Count, maxInstructionIterations));
            while (grassInstructions.Count != 0 && maxInstructionIterations > 0)
            {
                instructionsThisFrame.Add(grassInstructions.Dequeue());
                maxInstructionIterations--;
            }


            for (int i = 0; i < layers.Length; i++)
            {
                if (!layers[i].inited)
                {
                    layers[i].InitComputeShaders(this, cmd);

                }
                else if (layers[i].enabled)
                {

                    bool grassCountChanged = false;
                    bool consumeBufferChanged = false;

                    //Execute all the commands queued from the last frame



                    //Update references on compute shaders
                    layers[i].SetBuffers(this, cmd);
                    //Execute global instructions
                    for (int ii = 0; ii < instructionsThisFrame.Count; ii++)
                    {
                        instructionsThisFrame[ii].Execute(this, layers[i], cmd, ref grassCountChanged, ref consumeBufferChanged);
                    }
                    //Execute local instructions

                    if (consumeBufferChanged)
                    {
                        layers[i].UpdateConsumeBufferBindings(this, cmd);
                    }

                    if (grassCountChanged)
                    {
                        //Also copy the new number of blades to the rendering args of instance count (1 uint into the array)
                        cmd.CopyCounterValue(layers[i].meshPropertiesConsumeBuffer, layers[i].drawIndirectArgsBuffer, sizeof(uint));
                    }

                    layers[i].OnCameraBeginRendering(this, cmd);


                }
            }


            context.ExecuteCommandBuffer(cmd);
        }

        CommandBufferPool.Release(cmd);

        // material.SetBuffer("_Properties", meshPropertiesBuffer);
    }





    private void DisposeBuffers()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].DisposeBuffers();
        }
    }

    void UpdateBounds()
    {
        if (terrain != null)
        {
            offset = terrain.terrainData.bounds.extents.x;
            range = terrain.terrainData.bounds.extents.x;
        }
        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position + new Vector3(offset, 0, offset), Vector3.one * (range * 2 + 1));
    }





    private void PlaceBlades()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateChunkTree(this);
        }


        // void CreateGrassInTree(QuadTree tree)
        // {
        //     foreach (QuadTreeLeaf leaf in tree)
        //     {
        //         if (leaf is QuadTree t)
        //         {
        //             CreateGrassInTree(t);
        //         }
        //         else if (leaf is QuadTreeEnd end && end.enabled)
        //         {
        //             grassInstructions.Enqueue(new CreateGrassInstruction(end.id, end.rect, end.cellsWidth * end.cellsWidth));
        //         }
        //     }
        // }
        //CreateGrassInTree(chunkTree);

        //grassInstructions.Enqueue(new CreateGrassInstruction(0, new Rect(-range, -range, range * 2, range * 2), texSize * texSize));
    }

    private void Start()
    {
        mainCamera = Camera.main;
        m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);

        Setup();
        singleton = this;
    }

    private void OnDestroy()
    {
        DisposeBuffers();
        singleton = null;
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
    }

    Vector4[] pushersData = new Vector4[0];


    private void Update()
    {
        if (pushers.Count > 10)
        {
            (Vector4 data, float priority)[] pushingQueue = new (Vector4, float)[pushers.Count];

            if (pushersData == null || pushersData.Length != 10)
                pushersData = new Vector4[10];

            for (int i = 0; i < pushers.Count; i++)
            {
                pushingQueue[i].data = pushers[i].Data;
                pushingQueue[i].priority = Vector3.SqrMagnitude(pushers[i].transform.position - GrassPusher.mainPusher.transform.position);
            }
            //Order by distance to main pusher
            pushingQueue.OrderBy(x => x.priority);

            for (int i = 0; i < 10; i++)
            {
                pushersData[i] = pushingQueue[i].data;
            }
        }
        else
        {
            //Big enough
            if (pushersData == null || pushersData.Length != pushers.Count)
                pushersData = new Vector4[pushers.Count];

            for (int i = 0; i < pushers.Count; i++)
            {
                pushersData[i] = pushers[i].Data;
            }
        }

        for (int i = 0; i < pushersData.Length; i++)
        {
            pushersData[i] -= new Vector4(bounds.center.x, transform.position.y, bounds.center.z);
        }



        //Setup the call to draw the grass when the time comes
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].DrawGrassLayer(this);
        }


        // Debug.Log(inited);

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
        grassInstructions.Enqueue(new DestroyGrassInBoundsInstruction(bounds, angleRad));
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

        Gizmos.matrix = Matrix4x4.TRS(transform.position + Vector3.up * 0.1f, Quaternion.identity, Vector3.one * range * 2);



        // UpdateChunkTree();
        if (Application.isPlaying)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                foreach (var item in layers[i].endsInRange)
                {
                    item.DrawGizmos();
                }
            }
        }
    }
}