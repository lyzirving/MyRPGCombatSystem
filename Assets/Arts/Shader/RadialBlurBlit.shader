Shader "Hidden/RadialBlurBlit"
{
    Properties
    {
        _FocusRadius("Focus Radius", Range(0, 0.5)) = 0.1
        _Fade("Fade", Range(0, 0.2)) = 0.05
        _BlurStrength("Blur Strength", Range(0, 1)) = 1.0                
        _DirectionalBias("DirectionalBias", Range(0, 1)) = 0.5
        [IntRange] _BlurAmount("Blur Amount", Range(0, 20)) = 2
        [IntRange] _SampleCount("Sample Count", Range(2, 32)) = 2
    }

    SubShader
    {        
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "RadialBlurBlit"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #ifndef MAX_SAMPLE_COUNT
                #define MAX_SAMPLE_COUNT 32
            #endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Major Parameters
            float2 _FocusCenter;     
            float  _FocusRadius;  
            float  _Fade;
            float  _BlurStrength;    
            int    _SampleCount;     
            int   _BlurAmount;      

            float2 _MovingDirection; // moving direction projected on screen
            float  _DirectionalBias; // 0 - 1  

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;
                half4 original = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);               

                float2 dir = uv - _FocusCenter;
                float dist = length(dir);
                
                //The further away from the center point, the more blurred it becomes.
                //If dist < _FocusRadius, it's not blurred.
                float baseMask = smoothstep(_FocusRadius, _FocusRadius + _Fade, dist);
                
                // Directional Mask: reverse sprint direction has much more weight
                float directionalFactor = 0.0;
                if (_DirectionalBias > 0.001)
                {
                    float dotVal = dot(normalize(dir + 0.0001), _MovingDirection);
                    // arccos(0.3) б╓ 72.54бу
                    // directionalFactor is value between [0, _DirectionalBias].
                    directionalFactor = smoothstep(-0.3, 0.0, -dotVal) * _DirectionalBias;
                }
                
                float mask = saturate(baseMask + directionalFactor);
                float blurWeight = mask * _BlurStrength;
                
                if (blurWeight < 0.001)
                    return original;
                
                float2 blurDir = normalize(dir);
                if (_DirectionalBias > 0.001)
                {
                    blurDir = lerp(blurDir, -_MovingDirection, directionalFactor * 0.5);
                }
                
                half4 blurred = half4(0,0,0,0);
                int sampleCount = min(_SampleCount, MAX_SAMPLE_COUNT);
                float stepOffset = (float)_BlurAmount / (float)sampleCount;
                
                [loop]
                for (int i = 0; i < sampleCount; i++)
                {
                    float offset = (i + 0.5) * stepOffset;
                    float2 sampleUV = uv + blurDir * offset * _BlitTexture_TexelSize.xy;
                    blurred += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, sampleUV, _BlitMipLevel);  
                }
                blurred /= sampleCount;
                
                return lerp(original, blurred, blurWeight);                
            }
            ENDHLSL
        }
    }
}