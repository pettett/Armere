using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class VolumetricLightingPass : ScriptableRenderPass
{
    // used to label this pass in Unity's Frame Debug utility
    string profilerTag;

    RenderTargetIdentifier cameraColorTargetIdent;
    RenderTargetHandle sampleMap;

    readonly Vector3 noiseOffset;
    readonly float noiseScale;
    readonly float scattering = 0.1f;
    readonly float extinction;
    readonly float skyboxExtinction;
    readonly int textureDownscale = 1;


    // This isn't part of the ScriptableRenderPass class and is our own addition.
    // For this custom pass we need the camera's color target, so that gets passed in.

    public VolumetricLightingPass(string profilerTag, RenderPassEvent renderPassEvent, Vector3 noiseOffset, float noiseScale, float scattering, float extinction, float skyboxExtinction, int textureDownscale)
    {
        this.profilerTag = profilerTag;
        this.noiseOffset = noiseOffset;
        this.noiseScale = noiseScale;
        this.scattering = scattering;
        this.extinction = extinction;
        this.renderPassEvent = renderPassEvent;
        this.skyboxExtinction = skyboxExtinction;
        this.textureDownscale = textureDownscale;
    }

    // called each frame before Execute, use it to set up things the pass will need
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {


        // cameraTextureDescriptor.width /= textureDownscale;
        // cameraTextureDescriptor.width /= textureDownscale;

        ConfigureTarget(cameraColorTargetIdent);
        // create a temporary render texture that matches the camera
        cmd.GetTemporaryRT(sampleMap.id, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);

        
    }
    public void Setup(in RenderTargetIdentifier cameraColorTargetIdent)
    {
        this.cameraColorTargetIdent = cameraColorTargetIdent;
    }


    // Execute is called for every eligible camera every frame. It's not called at the moment that
    // rendering is actually taking place, so don't directly execute rendering commands here.
    // Instead use the methods on ScriptableRenderContext to set up instructions.
    // RenderingData provides a bunch of (not very well documented) information about the scene
    // and what's being rendered.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {



        // // the actual content of our custom render pass!
        // // we apply our material while blitting to a temporary texture
        // cmd.Blit(cameraColorTargetIdent, sampleMap.Identifier(), materialToBlit, 0);

        // // ...then blit it back again 
        // cmd.Blit(sampleMap.Identifier(), cameraColorTargetIdent);


        ref CameraData cameraData = ref renderingData.cameraData;
        if (cameraData.isSceneViewCamera) return;


        // fetch a command buffer to use
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

        cmd.Clear();


        Camera camera = cameraData.camera;



        Material volumetricSheet = new Material(Shader.Find("Hidden/Custom/VolumetricPostProcess"));


        var p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        //Magic? get the world to shadow map matrix
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        var clipToWorld = Matrix4x4.Inverse(p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);

        //        volumetricSheet.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        volumetricSheet.SetMatrix("_ClipToWorld", clipToWorld);



        volumetricSheet.SetVector("_VolumetricLight", new Vector4(scattering, extinction, 0, skyboxExtinction));

        volumetricSheet.SetVector("_NoiseSettings",
         new Vector4(noiseOffset.x, noiseOffset.y, noiseOffset.z, noiseScale));

        // volumetricSheet.properties.SetVector("_MieG", new Vector4(1 - settings.MieG * settings.MieG, 1 + settings.MieG * settings.MieG, settings.MieG * 2, Mathf.PI * 0.25f));

        //cmd.SetGlobalTexture("_SampleMap", cameraColorTargetIdent);
        // //Set color of main light
        Material additiveSheet = new Material(Shader.Find("Hidden/Custom/VolumetricAdditivePass"));
        additiveSheet.SetColor("_RayColor", RenderSettings.sun.color);

        cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

        //Draw the god rays onto the sample texture
        //cmd.Blit(null, sampleMap.Identifier(), volumetricSheet);




        //cmd.SetRenderTarget(sampleMap.Identifier());

        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, volumetricSheet, 0, 0);



        //Debug.Log(cameraTarget);
    //cmd.SetRenderTarget();
        //cmd.SetRenderTarget(cameraColorTargetIdent);

    


        cmd.SetGlobalTexture("_SampleMap", sampleMap.Identifier());
        cmd.SetGlobalTexture("_MainTex", cameraColorTargetIdent);

        //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, additiveSheet, 0, 0);




        // //Add samples to the final image with upscaling
        // cmd.Blit(cameraColorTargetIdent, cameraColorTargetIdent, additiveSheet);
        // cmd.Blit(cameraColorTargetIdent, colourMap.Identifier());
        // cmd.Blit(sampleMap.Identifier(), cameraColorTargetIdent, additiveSheet);


        // don't forget to tell ScriptableRenderContext to actually execute the commands
        context.ExecuteCommandBuffer(cmd);

        // tidy up after ourselves
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    // called after Execute, use it to clean up anything allocated in Configure
    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(sampleMap.id);
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Experimental.Rendering;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
// public class VolumetricLightingPass : ScriptableRenderPass
// {
//     // used to label this pass in Unity's Frame Debug utility
//     string profilerTag;

//     RenderTargetIdentifier cameraColorTargetIdent;
//     RenderTargetHandle sampleMap;

//     readonly Vector3 noiseOffset;
//     readonly float noiseScale;
//     readonly float scattering = 0.1f;
//     readonly float extinction;
//     readonly float skyboxExtinction;
//     readonly int textureDownscale = 1;

//     public VolumetricLightingPass(string profilerTag, RenderPassEvent renderPassEvent, Vector3 noiseOffset, float noiseScale, float scattering, float extinction, float skyboxExtinction, int textureDownscale)
//     {
//         this.profilerTag = profilerTag;
//         this.noiseOffset = noiseOffset;
//         this.noiseScale = noiseScale;
//         this.scattering = scattering;
//         this.extinction = extinction;
//         this.renderPassEvent = renderPassEvent;
//         this.skyboxExtinction = skyboxExtinction;
//         this.textureDownscale = textureDownscale;
//     }

//     // This isn't part of the ScriptableRenderPass class and is our own addition.
//     // For this custom pass we need the camera's color target, so that gets passed in.
//     public void Setup(RenderTargetIdentifier cameraColorTargetIdent)
//     {
//         this.cameraColorTargetIdent = cameraColorTargetIdent;
//     }

//     // called each frame before Execute, use it to set up things the pass will need
//     public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
//     {


//         cameraTextureDescriptor.width /= textureDownscale;
//         cameraTextureDescriptor.width /= textureDownscale;


//         // create a temporary render texture that matches the camera
//         cmd.GetTemporaryRT(sampleMap.id, cameraTextureDescriptor);
//     }

//     // Execute is called for every eligible camera every frame. It's not called at the moment that
//     // rendering is actually taking place, so don't directly execute rendering commands here.
//     // Instead use the methods on ScriptableRenderContext to set up instructions.
//     // RenderingData provides a bunch of (not very well documented) information about the scene
//     // and what's being rendered.
//     public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//     {
//         // fetch a command buffer to use
//         CommandBuffer cmd = CommandBufferPool.Get(profilerTag);



//         cmd.Clear();

//         // // the actual content of our custom render pass!
//         // // we apply our material while blitting to a temporary texture
//         // cmd.Blit(cameraColorTargetIdent, sampleMap.Identifier(), materialToBlit, 0);

//         // // ...then blit it back again 
//         // cmd.Blit(sampleMap.Identifier(), cameraColorTargetIdent);


//         ref CameraData cameraData = ref renderingData.cameraData;
//         Camera camera = cameraData.camera;



//         Material volumetricSheet = new Material(Shader.Find("Hidden/Custom/VolumetricPostProcess"));
//         Material additiveSheet = new Material(Shader.Find("Hidden/Custom/VolumetricAdditivePass"));

//         var p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
//         //Magic? get the world to shadow map matrix
//         p[2, 3] = p[3, 2] = 0.0f;
//         p[3, 3] = 1.0f;
//         var clipToWorld = Matrix4x4.Inverse(p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);


//         volumetricSheet.SetMatrix("_ClipToWorld", clipToWorld);



//         volumetricSheet.SetVector("_VolumetricLight", new Vector4(scattering, extinction, 0, skyboxExtinction));

//         volumetricSheet.SetVector("_NoiseSettings",
//          new Vector4(noiseOffset.x, noiseOffset.y, noiseOffset.z, noiseScale));

//         // volumetricSheet.properties.SetVector("_MieG", new Vector4(1 - settings.MieG * settings.MieG, 1 + settings.MieG * settings.MieG, settings.MieG * 2, Mathf.PI * 0.25f));


//         //Draw the god rays onto the sample texture
//         cmd.Blit(cameraColorTargetIdent, sampleMap.Identifier(), volumetricSheet);

//         //cmd.SetGlobalTexture("_SampleMap", sampleMap.Identifier());
//         //Set color of main light
//         additiveSheet.SetColor("_RayColor", RenderSettings.sun.color);

//         //Add samples to the final image with upscaling
//         //cmd.Blit(cameraColorTargetIdent, cameraColorTargetIdent, additiveSheet, 0);
//         cmd.Blit(sampleMap.Identifier(), cameraColorTargetIdent);







//         // don't forget to tell ScriptableRenderContext to actually execute the commands
//         context.ExecuteCommandBuffer(cmd);

//         // tidy up after ourselves
//         cmd.Clear();
//         CommandBufferPool.Release(cmd);
//     }

//     // called after Execute, use it to clean up anything allocated in Configure
//     public override void FrameCleanup(CommandBuffer cmd)
//     {
//         cmd.ReleaseTemporaryRT(sampleMap.id);
//     }
// }
