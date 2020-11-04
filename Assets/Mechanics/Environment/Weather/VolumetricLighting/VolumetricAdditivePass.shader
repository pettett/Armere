Shader "Hidden/Custom/VolumetricAdditivePass"
{
    HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        TEXTURE2D_X(_MainTex); 
        TEXTURE2D_X(_SampleMap);

        float4 _MainTex_TexelSize;
        float3 _RayColor;

        float SampleTexture(float2 uv, float2 offset){
            uv += _MainTex_TexelSize * offset;

            return SAMPLE_TEXTURE2D_X(_SampleMap, sampler_LinearClamp, uv).r;
        }

        float4 Frag(Varyings i) : SV_Target
        {
            float3 color = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv);

            float raySample = SampleTexture(i.uv,float2(0,0)) ;

            raySample += SampleTexture( i.uv, float2(1,0) ) ;
            raySample += SampleTexture( i.uv ,float2(-1,0));
            raySample += SampleTexture( i.uv, float2(0,1));
            raySample += SampleTexture( i.uv, float2(0,-1));

            raySample += SampleTexture( i.uv ,float2(1,1));
            raySample += SampleTexture( i.uv, float2(1,-1));
            raySample += SampleTexture( i.uv ,float2(-1,1));
            raySample += SampleTexture( i.uv ,float2(-1,-1));

            const float recip = rcp(9);

            raySample *= recip;
            //return float4(raySample,raySample,raySample,1);
            return float4(raySample * _RayColor + color,1);
        }



    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}