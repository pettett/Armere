using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(VolumetricLightingRenderer), PostProcessEvent.BeforeStack, "Custom/Volumetric Lighting")]
public sealed class VolumetricLighting : PostProcessEffectSettings
{
    [Range(1, 5)]
    public IntParameter textureDownscale = new IntParameter { value = 1 };
    [Range(0f, 1f)]
    public FloatParameter debug = new FloatParameter { value = 0 };

    [Range(0, 1)]
    public FloatParameter scattering = new FloatParameter { value = 0 };
    [Range(0f, 0.1f)]
    public FloatParameter extinction = new FloatParameter { value = 0 };

    [Range(0, 1)]
    public FloatParameter skyboxExtinction = new FloatParameter { value = 0 };
    [Range(0, 0.999f)]
    public FloatParameter MieG = new FloatParameter { value = 0 };

    public FloatParameter noiseScale = new FloatParameter { value = 0.25f };
    public Vector3Parameter noiseOffset = new Vector3Parameter { value = Vector3.zero };
}

public sealed class VolumetricLightingRenderer : PostProcessEffectRenderer<VolumetricLighting>
{
    public override DepthTextureMode GetCameraFlags() => DepthTextureMode.Depth;


    public override void Init()
    {

    }

    int _sampleMapID;

    public override void Render(PostProcessRenderContext context)
    {
        var volumetricSheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/VolumetricPostProcess"));
        var additiveSheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/VolumetricAdditivePass"));

        var p = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        var clipToWorld = Matrix4x4.Inverse(p * context.camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);

        volumetricSheet.properties.SetMatrix("_CameraToWorld", context.camera.cameraToWorldMatrix);
        volumetricSheet.properties.SetMatrix("_ClipToWorld", clipToWorld);

        volumetricSheet.properties.SetFloat("debug", settings.debug);

        volumetricSheet.properties.SetVector("_VolumetricLight", new Vector4(settings.scattering, settings.extinction, 0, settings.skyboxExtinction));

        volumetricSheet.properties.SetVector("_NoiseSettings",
         new Vector4(settings.noiseOffset.value.x, settings.noiseOffset.value.y, settings.noiseOffset.value.z, settings.noiseScale));

        volumetricSheet.properties.SetVector("_MieG", new Vector4(1 - settings.MieG * settings.MieG, 1 + settings.MieG * settings.MieG, settings.MieG * 2, Mathf.PI * 0.25f));

        int sampleWidth = context.screenWidth / settings.textureDownscale;
        int sampleHeight = context.screenHeight / settings.textureDownscale;

        context.command.GetTemporaryRT(_sampleMapID, sampleWidth, sampleHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8);

        context.command.BlitFullscreenTriangle(context.source, _sampleMapID, volumetricSheet, 0);

        context.command.SetGlobalTexture("_SampleMap", _sampleMapID);
        //Set color of main light
        additiveSheet.properties.SetColor("_RayColor", RenderSettings.sun.color);

        context.command.BlitFullscreenTriangle(context.source, context.destination, additiveSheet, 0);

        context.command.ReleaseTemporaryRT(_sampleMapID);
    }
}