// This shader fills the mesh shape with a color predefined in the code.
Shader "Example/URPUnlitShaderBasic"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {    
		_SrcBlend("Src", Float) = 1.0
        _DstBlend("Dst", Float) = 0.0
        _ZWrite("ZWrite", Float) = 1.0
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull", Float) = 2.0
        [Enum(Off,0,Front,1,Back,2)]_ShadowCull("Shadow Cull", Float) = 2.0

        _ShadowCasterCutoff("Shadow Caster Cutoff", Range(0,1)) = 0.5
        _NoiseScale("Simplex Noise Scale", Vector) = (0.5,0.5,0,0)
        _DistanceWeightCutoffs("Distance Weight Cutoffs", Vector) = (0.5,0.75,0,0)

		}

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags {"RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 300

        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]


        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex CloudVertex
            // This line defines the name of the fragment shader.
            #pragma fragment CloudFragment

            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "CloudForward.hlsl"

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.


            // The fragment shader definition.

            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}


            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_ShadowCull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex CloudShadowPassVertex
            #pragma fragment CloudShadowFragment

            #include "CloudShadows.hlsl"
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing



            ENDHLSL
        }













        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex CloudVertex
            #pragma fragment CloudShadowFragment


            #include "CloudShadows.hlsl"
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing


            ENDHLSL
        }



		Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex CloudVertex
            #pragma fragment CloudShadowFragment


            #include "CloudShadows.hlsl"
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            ENDHLSL
        }

    }


	
}