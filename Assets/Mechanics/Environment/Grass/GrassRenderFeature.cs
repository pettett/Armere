// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Rendering.Universal;

// public class GrassRenderFeature : ScriptableRendererFeature
// {
//     [System.Serializable]
//     public class GrassSettings
//     {
//         // we're free to put whatever we want here, public fields will be exposed in the inspector
//         public bool IsEnabled = true;
//         public ComputeShader compute;
//         public Material test;

//     }

//     // MUST be named "settings" (lowercase) to be shown in the Render Features inspector
//     public GrassSettings settings = new GrassSettings();

//     GrassPass grassPass;

//     public override void Create()
//     {
//         grassPass = new GrassPass(settings.compute, settings.test);
//     }



//     // called every frame once per camera
//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {


//         if (!settings.IsEnabled)
//         {
//             // we can do nothing this frame if we want
//             return;
//         }

//         // Gather up and pass any extra information our pass will need.
//         // In this case we're getting the camera's color buffer target
//         var cameraColorTargetIdent = renderer.cameraColorTarget;

//         grassPass.Setup(cameraColorTargetIdent);

//         // Ask the renderer to add our pass.
//         // Could queue up multiple passes and/or pick passes to use
//         renderer.EnqueuePass(grassPass);


//     }
// }
