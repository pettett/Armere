// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.Experimental.Rendering;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
// public class GrassPass : ScriptableRenderPass
// {
//     private struct MeshProperties
//     {
//         public Vector3 position;
//         public Vector3 rotation;
//         public Vector2 size;
//         public static int Size()
//         {
//             return
//                 //           rotation, position,size
//                 sizeof(float) * (3 + 3 + 2);
//         }
//     }
//     private struct MatricesStruct
//     {
//         public Matrix4x4 matrix;
//         public Vector4 color;
//     }

//     private const string k_RenderGrassTag = "Render Grass";
//     private ProfilingSampler m_Grass_Profile = new ProfilingSampler(k_RenderGrassTag);

//     RenderTargetIdentifier cameraColorTargetIdent;
//     //RenderTargetHandle sampleMap;

//     private Mesh mesh;



//     readonly ComputeShader compute;
//     private ComputeBuffer meshPropertiesBuffer;
//     private ComputeBuffer matrixesBuffer;
//     private ComputeBuffer argsBuffer;

//     readonly int kernel;
//     bool inited = false;
//     Material test;
//     // This isn't part of the ScriptableRenderPass class and is our own addition.
//     // For this custom pass we need the camera's color target, so that gets passed in.

//     public GrassPass(ComputeShader compute, Material test)
//     {
//         //Render grass after opaque objects
//         this.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
//         this.compute = compute;

//         Mesh mesh = CreateQuad();
//         this.mesh = mesh;
//         this.test = test;

//         kernel = compute.FindKernel("CSMain");

//         if (GrassController.singleton == null) return;

//         InitializeBuffers();

//     }

//     //Called once per frame before executing
//     public void Setup(in RenderTargetIdentifier cameraColorTargetIdent)
//     {
//         this.cameraColorTargetIdent = cameraColorTargetIdent;

//     }

//     private Mesh CreateQuad(float width = 1f, float height = 1f)
//     {
//         // Create a quad mesh.
//         var mesh = new Mesh();

//         float w = width * .5f;
//         float h = height;
//         var vertices = new Vector3[] {
//             new Vector3(-w, 0, 0),
//             new Vector3(w, 0, 0),
//             new Vector3(-w, h, 0),
//             new Vector3(w, h, 0),
//             //back plane
//             new Vector3(-w, 0, 0),
//             new Vector3(w, 0, 0),
//             new Vector3(-w, h, 0),
//             new Vector3(w, h, 0),
//             //left plane
//             new Vector3(0, 0, -w),
//             new Vector3(0, 0, w),
//             new Vector3(0, h, -w),
//             new Vector3(0, h, w),
//             //right plane
//             new Vector3(0, 0, -w),
//             new Vector3(0, 0, w),
//             new Vector3(0, h, -w),
//             new Vector3(0, h, w),
//         };

//         var tris = new int[] {
//             // lower left tri.
//             0, 2, 1,
//             // lower right tri
//             2, 3, 1,
// //backplane
//             // lower left tri.
//             4, 5, 6,
//             // lower right tri
//             6, 5, 7,
// //left plane
//             // lower left tri.
//             8, 9, 10,
//             // lower right tri
//             10, 9, 11,
// //right plane

//             // lower left tri.
//             8, 10, 9,
//             // lower right tri
//             10, 11, 9,
//         };

//         var normals = new Vector3[] {
//             -Vector3.forward,
//             -Vector3.forward,
//             -Vector3.forward,
//             -Vector3.forward,
//             //back plane
//             Vector3.forward,
//             Vector3.forward,
//             Vector3.forward,
//             Vector3.forward,

//             //left plane
//             Vector3.right,
//             Vector3.right,
//             Vector3.right,
//             Vector3.right,

//             //right plane
//             -Vector3.right,
//             -Vector3.right,
//             -Vector3.right,
//             -Vector3.right,
//         };

//         var uv = new Vector2[] {
//             new Vector2(0, 0),
//             new Vector2(1, 0),
//             new Vector2(0, 1),
//             new Vector2(1, 1),

//             new Vector2(0, 0),
//             new Vector2(1, 0),
//             new Vector2(0, 1),
//             new Vector2(1, 1),

//             new Vector2(0, 0),
//             new Vector2(1, 0),
//             new Vector2(0, 1),
//             new Vector2(1, 1),

//             new Vector2(0, 0),
//             new Vector2(1, 0),
//             new Vector2(0, 1),
//             new Vector2(1, 1),
//         };

//         mesh.vertices = vertices;
//         mesh.triangles = tris;
//         mesh.normals = normals;
//         mesh.uv = uv;

//         return mesh;
//     }

//     public float SampleRandomRange(Vector2 range) => Random.Range(range.x, range.y);
//     private void InitializeBuffers()
//     {
//         if (inited) return;
//         inited = true;
//         //int frustumKernel = frustumCuller.FindKernel("CSMain");

//         // Argument buffer used by DrawMeshInstancedIndirect.
//         uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
//         // Arguments for drawing mesh.
//         // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
//         args[0] = mesh.GetIndexCount(0);
//         args[1] = (uint)GrassController.singleton.totalPopulation;


//         argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
//         argsBuffer.SetData(args);

//         // Initialize buffer with the given population.
//         MeshProperties[] properties = new MeshProperties[GrassController.singleton.totalPopulation];
//         MatricesStruct[] output = new MatricesStruct[GrassController.singleton.totalPopulation];
//         for (int i = 0; i < GrassController.singleton.totalPopulation; i++)
//         {
//             MeshProperties props = new MeshProperties();
//             Vector3 position = new Vector3(Random.Range(-GrassController.singleton.range, GrassController.singleton.range), 0, Random.Range(-GrassController.singleton.range, GrassController.singleton.range));

//             if (GrassController.singleton.terrain != null)
//                 position.y = GrassController.singleton.terrain.SampleHeight(position);

//             Quaternion rotation = Quaternion.Euler(Mathf.PI, Random.Range(-Mathf.PI, Mathf.PI), 0);
//             Vector3 scale = Vector3.one;
//             props.position = position;
//             props.rotation = rotation.eulerAngles;

//             props.size = new Vector2(SampleRandomRange(GrassController.singleton.quadWidthRange), SampleRandomRange(GrassController.singleton.quadHeightRange));

//             output[i] = new MatricesStruct() { color = GrassController.singleton.colorGradient.Evaluate(Random.value) };

//             properties[i] = props;
//         }

//         meshPropertiesBuffer = new ComputeBuffer(GrassController.singleton.totalPopulation, MeshProperties.Size());
//         meshPropertiesBuffer.SetData(properties);

//         matrixesBuffer = new ComputeBuffer(GrassController.singleton.totalPopulation, sizeof(float) * 20);
//         matrixesBuffer.SetData(output);

//         //frustumCuller.SetBuffer(frustumKernel, "_Properties", meshPropertiesBuffer);

//         compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
//         compute.SetBuffer(kernel, "_Output", matrixesBuffer);

//         GrassController.singleton.material.SetBuffer("_Properties", matrixesBuffer);
//     }



//     // called each frame before Execute, use it to set up things the pass will need
//     public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
//     {


//         // cameraTextureDescriptor.width /= textureDownscale;
//         // cameraTextureDescriptor.width /= textureDownscale;


//         // create a temporary render texture that matches the camera
//         // cmd.GetTemporaryRT(sampleMap.id, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);


//     }






//     // Execute is called for every eligible camera every frame. It's not called at the moment that
//     // rendering is actually taking place, so don't directly execute rendering commands here.
//     // Instead use the methods on ScriptableRenderContext to set up instructions.
//     // RenderingData provides a bunch of (not very well documented) information about the scene
//     // and what's being rendered.
//     public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//     {
//         var cam = renderingData.cameraData.camera;
//         // Stop the pass rendering in the preview

//         if (GrassController.singleton == null || compute == null || cam.cameraType == CameraType.Preview) return;
//         InitializeBuffers(); //Not actually doing this every frame, only until inited is true




//         // // the actual content of our custom render pass!
//         // // we apply our material while blitting to a temporary texture

//         // fetch a command buffer to use
//         CommandBuffer cmd = CommandBufferPool.Get(k_RenderGrassTag);
//         using (new ProfilingScope(cmd, m_Grass_Profile))
//         {
//             context.ExecuteCommandBuffer(cmd);
//             cmd.Clear();


//             cmd.SetComputeVectorParam(compute, "_PusherPosition", GrassController.singleton.pusher.position);
//             cmd.SetComputeFloatParam(compute, "deltatime", Time.deltaTime);
//             cmd.SetComputeFloatParam(compute, "time", Time.time);

//             cmd.DispatchCompute(compute, kernel, GrassController.singleton.population.x * 64, GrassController.singleton.population.y, GrassController.singleton.population.z);

//             cmd.SetRenderTarget(cameraColorTargetIdent);

//             GrassController.singleton.material.enableInstancing = true;

//             cmd.DrawMeshInstancedIndirect(mesh, 0, GrassController.singleton.material, 0, argsBuffer, 0);

//             context.ExecuteCommandBuffer(cmd);

//             // Graphics.DrawMeshInstancedIndirect(mesh, 0, GrassController.singleton.material, GrassController.singleton.bounds, argsBuffer);

//         }

//         // don't forget to tell ScriptableRenderContext to actually execute the commands
//         context.ExecuteCommandBuffer(cmd);
//         CommandBufferPool.Release(cmd);
//     }

//     // called after Execute, use it to clean up anything allocated in Configure
//     public override void FrameCleanup(CommandBuffer cmd)
//     {
//         //cmd.ReleaseTemporaryRT(sampleMap.id);
//     }
// }
