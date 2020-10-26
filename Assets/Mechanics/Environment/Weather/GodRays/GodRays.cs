using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(GodRaysRenderer), PostProcessEvent.AfterStack, "Custom/GodRays")]
public sealed class GodRays : PostProcessEffectSettings
{

    public FloatParameter gInitDecay = new FloatParameter { value = 1f };
    public FloatParameter gDistDecay = new FloatParameter { value = 1f };
    public ColorParameter RayColor = new ColorParameter { value = Color.white };
    public FloatParameter gMaxDeltaLen = new FloatParameter { value = 1f };
    [Range(1, 5)]
    public IntParameter textureDownscale = new IntParameter { value = 1 };
}

public sealed class GodRaysRenderer : PostProcessEffectRenderer<GodRays>
{
    public override DepthTextureMode GetCameraFlags() => DepthTextureMode.Depth;
    public int _occlusionMapID;
    public int _sampleMapID;
    public override void Init()
    {
        _occlusionMapID = Shader.PropertyToID("_OcclusionMap");
        _sampleMapID = Shader.PropertyToID("_SampleMap");
    }

    Vector2 GetScreenSpaceSunPos(Camera camera, int w, int h)
    {
        var v = camera.transform.position - RenderSettings.sun.transform.forward;
        return ((Vector2)camera.WorldToScreenPoint(v)) / new Vector2(w, h);
    }

    public override void Render(PostProcessRenderContext context)
    {
        var occlusionSheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/GodRayOcclusionPass"));


        //Render the occlusion map
        int sampleWidth = context.screenWidth / settings.textureDownscale;
        int sampleHeight = context.screenHeight / settings.textureDownscale;

        context.command.GetTemporaryRT(_occlusionMapID, sampleWidth, sampleHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
        context.command.GetTemporaryRT(_sampleMapID, sampleWidth, sampleHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8);

        context.command.BlitFullscreenTriangle(context.source, _occlusionMapID, occlusionSheet, 0);


        var godRaySamplePassSheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/GodRaySamplePass"));

        Vector2 sunPos = GetScreenSpaceSunPos(context.camera, context.screenWidth, context.screenHeight); // context.camera.WorldToScreenPoint(context.camera.transform.position + RenderSettings.sun.transform.forward * 1000);


        //Perform the tracing on the occlusion


        godRaySamplePassSheet.properties.SetVector("gSunPos", sunPos);
        godRaySamplePassSheet.properties.SetFloat("gInitDecay", settings.gInitDecay);
        godRaySamplePassSheet.properties.SetFloat("gDistDecay", settings.gDistDecay);

        godRaySamplePassSheet.properties.SetFloat("gMaxDeltaLen", settings.gMaxDeltaLen);


        context.command.BlitFullscreenTriangle(_occlusionMapID, _sampleMapID, godRaySamplePassSheet, 0);

        context.command.ReleaseTemporaryRT(_occlusionMapID);

        var godRayAdditivePassSheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/GodRayAdditivePass"));
        godRayAdditivePassSheet.properties.SetColor("_RayColor", settings.RayColor);

        context.command.BlitFullscreenTriangle(context.source, context.destination, godRayAdditivePassSheet, 0);

        context.command.ReleaseTemporaryRT(_sampleMapID);
    }
}