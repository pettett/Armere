Shader "Hidden/Custom/GodRaySamplePass"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

        float2 gSunPos;
        float gInitDecay;
        float gDistDecay;
        float gMaxDeltaLen;


        static const int NUM_STEPS = 64;
        static const float NUM_DELTA = 1.0 / 64.0f;

        float4 Frag(VaryingsDefault In) : SV_TARGET
        {

            // Find the direction and distance to the sun
            float2 dirToSun = (gSunPos - In.texcoord);
            float lengthToSun = length(dirToSun);
            dirToSun = normalize(dirToSun);

            // Find the ray delta
            float deltaLen = min(gMaxDeltaLen, lengthToSun * NUM_DELTA);
            float2 rayDelta = dirToSun * deltaLen;

            // Each step decay
            float stepDecay = gDistDecay * deltaLen;

            // Initial values
            float2 rayOffset = float2(0.0, 0.0);
            float decay = gInitDecay;
            float rayIntensity = 0.0f;

            // Ray march towards the sun
            for (int i = 0; i < NUM_STEPS; i++)
            {
                // Sample at the current location
                float2 sampPos = In.texcoord + rayOffset;
                //Main tex is occlusion map
                float fCurIntensity = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampPos);

                // Sum the intensity taking decay into account
                rayIntensity += fCurIntensity * decay;

                // Advance to the next position
                rayOffset += rayDelta;

                // Update the decay
                decay = saturate(decay - stepDecay);
            }
            // The resultant intensity of the pixel.
            return rayIntensity;
        }


    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}