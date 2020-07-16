using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

[Serializable]
[PostProcess(typeof(VolumetricFogRenderer), PostProcessEvent.AfterStack, "Custom/Volumetric Fog")]
public sealed class VolumetricFog : PostProcessEffectSettings
{

    public FloatParameter gInitDecay = new FloatParameter { value = 1f };
    public FloatParameter gDistDecay = new FloatParameter { value = 1f };
    public ColorParameter RayColor = new ColorParameter { value = Color.white };
    public FloatParameter gMaxDeltaLen = new FloatParameter { value = 1f };
    [Range(1, 5)]
    public IntParameter textureDownscale = new IntParameter { value = 1 };
}

public sealed class VolumetricFogRenderer : PostProcessEffectRenderer<VolumetricFog>
{
    public override DepthTextureMode GetCameraFlags() => DepthTextureMode.Depth;

    public override void Init()
    {

    }


    public override void Render(PostProcessRenderContext context)
    {
        var godRayAdditivePassSheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/VolumetricFog"));
        godRayAdditivePassSheet.properties.SetVector("_LightDir", RenderSettings.sun.transform.forward);

        


        context.command.BlitFullscreenTriangle(context.source, context.destination, godRayAdditivePassSheet, 0);
    }
}