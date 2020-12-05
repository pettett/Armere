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
    public float offset;
    public ComputeShader createGrassInBoundsCompute;
    public Texture2D gradientTexture;

    public Terrain terrain;
    public Texture2D grassDensity;
    public RenderTexture grassHeight;


    public int groupsOf8PerCell = 3;
    Queue<GrassInstruction> grassInstructions = new Queue<GrassInstruction>();

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

    public Vector2 quadWidthRange = new Vector2(0.5f, 1f);
    public Vector2 quadHeightRange = new Vector2(0.5f, 1f);
    Camera mainCamera;
    int mainKernel;
    public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
    public float boundsYRot = 45f;
    public Vector3 testPoint;
    public Bounds killingBounds;

    QuadTree chunkTree;
    bool[,] cells;

    public bool inited = false;
    public interface GrassInstruction
    {
        void Execute(GrassController controller, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged);
    }

    public readonly struct CreateGrassInstruction : GrassInstruction
    {
        public readonly int chunkID;
        public readonly Rect textureRect;
        public readonly int cellsMultiplier;
        public CreateGrassInstruction(int chunkID, Rect textureRect, int cells)
        {
            this.chunkID = chunkID;
            this.textureRect = textureRect;
            this.cellsMultiplier = cells;
        }

        public void Execute(GrassController c, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged)
        {
            if (consumeBufferChanged)
            {
                c.UpdateConsumeBufferBindings(cmd);
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
                new Vector4(c.quadWidthRange.x, c.quadHeightRange.x, c.quadWidthRange.y, c.quadHeightRange.y));

            cmd.SetComputeIntParam(c.createGrassInBoundsCompute, "chunkID", chunkID);

            int dispatch = cellsMultiplier * c.groupsOf8PerCell;

            cmd.DispatchCompute(c.createGrassInBoundsCompute, 0, dispatch, 1, 1);

            //Only the max amount - rejection sampling makes this lower
            c.currentGrassCellCapacity += dispatch * 8;
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

        public void Execute(GrassController c, CommandBuffer cmd, ref bool grassCountChanged, ref bool consumeBufferChanged)
        {
            grassCountChanged = true;
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


            cmd.SetComputeBufferCounterValue(c.meshPropertiesAppendBuffer, 0);
            //  cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

            // destroyGrass.SetVector("boundsMin", killingBounds.min - transform.position);
            // destroyGrass.SetVector("boundsMax", killingBounds.max - transform.position);

            cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, "_Grass", c.meshPropertiesConsumeBuffer);
            cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, "_CulledGrass", c.meshPropertiesAppendBuffer);
            cmd.SetComputeBufferParam(c.destroyGrassInBounds, 0, "_ArgsData", c.drawIndirectArgsBuffer);


            cmd.DispatchCompute(c.destroyGrassInBounds, 0, c.threadGroups.x, c.threadGroups.y, c.threadGroups.z);



            //Swap the buffers around
            (c.meshPropertiesConsumeBuffer, c.meshPropertiesAppendBuffer) = (c.meshPropertiesAppendBuffer, c.meshPropertiesConsumeBuffer);


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
        Mesh mesh = CreateQuad();
        this.mesh = mesh;

        InitializeBuffers();
    }


    private void InitializeBuffers()
    {
        UpdateBounds();
        UpdateThreadGroupSizes();
        if (mesh == null) return;

        mainKernel = compute.FindKernel("CSMain");

        //int frustumKernel = frustumCuller.FindKernel("CSMain");

        drawIndirectArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

        meshPropertiesConsumeBuffer = new ComputeBuffer(totalPopulation, MeshProperties.size, ComputeBufferType.Append, ComputeBufferMode.Immutable);
        meshPropertiesAppendBuffer = new ComputeBuffer(totalPopulation, MeshProperties.size, ComputeBufferType.Append, ComputeBufferMode.Immutable);

        matrixesBuffer = new ComputeBuffer(totalPopulation, MatricesStruct.size, ComputeBufferType.Default, ComputeBufferMode.Immutable);

        //matrixesBuffer.SetCounterValue(0);

        PlaceBlades();
        //frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

        // cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);
        material.SetBuffer("_Properties", matrixesBuffer);
    }



    public void InitComputeShaders(CommandBuffer cmd)
    {
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = mesh.GetIndexCount(0);
        args[1] = 0;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);

        cmd.SetComputeBufferData(drawIndirectArgsBuffer, args);

        cmd.SetComputeBufferCounterValue(meshPropertiesAppendBuffer, 0);
        cmd.SetComputeBufferCounterValue(meshPropertiesConsumeBuffer, 0);

        SetDispatchSize(compute, cmd);
        SetDispatchSize(destroyGrassInBounds, cmd);
        SetDispatchSize(destroyGrassInSector, cmd);

        cmd.SetComputeBufferParam(compute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(compute, mainKernel, "_Output", matrixesBuffer);

        cmd.SetComputeBufferParam(createGrassInBoundsCompute, 0, "_Grass", meshPropertiesConsumeBuffer);
        cmd.SetComputeTextureParam(createGrassInBoundsCompute, 0, "_Gradient", gradientTexture);

        if (grassDensity == null && terrain != null) grassDensity = terrain.terrainData.alphamapTextures[0];

        cmd.SetComputeTextureParam(createGrassInBoundsCompute, 0, "_Density", grassDensity);

        if (terrain != null)
        {
            grassHeight = terrain.terrainData.heightmapTexture;

            cmd.SetComputeTextureParam(createGrassInBoundsCompute, 0, "_Height", terrain.terrainData.heightmapTexture);
            cmd.SetComputeFloatParam(createGrassInBoundsCompute, "grassHeightScale", terrain.terrainData.heightmapScale.y / 128f);
        }

        cmd.SetComputeBufferParam(createGrassInBoundsCompute, 0, "_IndirectArgs", drawIndirectArgsBuffer);

    }


    private void SetDispatchSize(ComputeShader shader, CommandBuffer cmd)
    {
        cmd.SetComputeIntParams(shader, "dispatchSize", threadGroups.x, threadGroups.y);
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

            if (!inited)
            {
                InitComputeShaders(cmd);

                inited = true;
            }
            else
            {

                bool grassCountChanged = false;
                bool consumeBufferChanged = false;

                //Execute all the commands queued from the last frame

                int maxInstructionIterations = 8;

                while (grassInstructions.Count != 0 && maxInstructionIterations > 0)
                {
                    grassInstructions.Dequeue().Execute(this, cmd, ref grassCountChanged, ref consumeBufferChanged);

                    maxInstructionIterations--;
                }

                if (consumeBufferChanged)
                {
                    UpdateConsumeBufferBindings(cmd);
                }

                if (grassCountChanged)
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

            }

            context.ExecuteCommandBuffer(cmd);
        }

        CommandBufferPool.Release(cmd);

        // material.SetBuffer("_Properties", meshPropertiesBuffer);
    }


    public void UpdateConsumeBufferBindings(CommandBuffer cmd)
    {
        //Update the main grass with the new append buffer
        cmd.SetComputeBufferParam(compute, mainKernel, "_Properties", meshPropertiesConsumeBuffer);
        cmd.SetComputeBufferParam(createGrassInBoundsCompute, 0, "_Grass", meshPropertiesConsumeBuffer);

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
        if (terrain != null)
        {
            offset = terrain.terrainData.bounds.extents.x;
            range = terrain.terrainData.bounds.extents.x;
        }
        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position + new Vector3(offset, 0, offset), Vector3.one * (range * 2 + 1));
    }




    void UpdateThreadGroupSizes()
    {
        compute.GetKernelThreadGroupSizes(mainKernel, out uint x, out uint y, out uint z);
        threadGroupSize = new Vector3Int((int)x, (int)y, (int)z);
        totalPopulation = threadGroups.x * threadGroups.y * threadGroups.z * (int)x * (int)y * (int)z;


    }


    public void UpdateChunkTree()
    {
        if (grassDensity == null && terrain != null) grassDensity = terrain.terrainData.alphamapTextures[0];

        int texSize = grassDensity.width;

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
        chunkTree = new QuadTree(cells, Vector2.one * 0.5f, Vector2.one, ref tempID);
    }


    private void PlaceBlades()
    {

        UpdateChunkTree();

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
                    grassInstructions.Enqueue(new CreateGrassInstruction(end.id, end.rect, end.cellsWidth * end.cellsWidth));
                }
            }
        }
        CreateGrassInTree(chunkTree);

        //grassInstructions.Enqueue(new CreateGrassInstruction(0, new Rect(-range, -range, range * 2, range * 2), texSize * texSize));
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

        if (inited)
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, drawIndirectArgsBuffer, castShadows: shadowCastingMode, receiveShadows: true);
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

        Gizmos.matrix = Matrix4x4.Translate(transform.position + Vector3.up * 0.1f);


        UpdateChunkTree();
        if (chunkTree != null)
            chunkTree.DrawGizmos();
    }
}