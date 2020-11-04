using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class VolumetricLightingRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class VolumetricLightingSettings
    {
        // we're free to put whatever we want here, public fields will be exposed in the inspector
        public bool IsEnabled = true;
        public RenderPassEvent WhenToInsert = RenderPassEvent.AfterRendering;
        public Vector3 noiseOffset;
        public float noiseScale;
        [Range(0,1)]
        public float scattering = 0.1f;
        public float extinction;
        public float skyboxExtinction;
        public int textureDownscale = 1;
    }

    // MUST be named "settings" (lowercase) to be shown in the Render Features inspector
    public VolumetricLightingSettings settings = new VolumetricLightingSettings();

    RenderTargetHandle renderTextureHandle;
    VolumetricLightingPass myRenderPass;

    public override void Create()
    {
        myRenderPass = new VolumetricLightingPass(
          "Volumetric Lighting",
                                settings.WhenToInsert,
                                 settings.noiseOffset,
                                 settings.noiseScale,
                                 settings.scattering,
                                 settings.extinction,
                                 settings.skyboxExtinction,
                                 settings.textureDownscale
        );
    }

    // called every frame once per camera
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.IsEnabled)
        {
            // we can do nothing this frame if we want
            return;
        }

        // Gather up and pass any extra information our pass will need.
        // In this case we're getting the camera's color buffer target
        var cameraColorTargetIdent = renderer.cameraColorTarget;

        myRenderPass.Setup(cameraColorTargetIdent);

        // Ask the renderer to add our pass.
        // Could queue up multiple passes and/or pick passes to use
        renderer.EnqueuePass(myRenderPass);
    }
}
