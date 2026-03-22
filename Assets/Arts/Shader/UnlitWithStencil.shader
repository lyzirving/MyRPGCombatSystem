Shader "Universal Render Pipeline/Custom/UnlitWithStencil"
{
    Properties
    {
        [Header(Color)]
        [Space(5)]
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        
        [Header(Stencil Test)]
        [Space(5)]
        [IntRange] _StencilRef("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Int) = 8  // Always = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass("Stencil Pass Operation", Int) = 0    // Keep = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail("Stencil Fail Operation", Int) = 0    // Keep = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail("Stencil ZFail Operation", Int) = 0  // Keep = 0
        [IntRange] _StencilReadMask("Stencil Read Mask", Range(0, 255)) = 255
        [IntRange] _StencilWriteMask("Stencil Write Mask", Range(0, 255)) = 255

        [Header(Alpha Clipping)]
        [Space(5)]
        [Toggle(_ALPHATEST_ON)] _AlphaClip("Alpha Clipping", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5

        [HideInInspector] _SurfaceType("Surface Type", Float) = 0
        [HideInInspector] _SrcBlend("Src Blend", Float) = 1
        [HideInInspector] _DstBlend("Dst Blend", Float) = 0
        [HideInInspector] _ZWrite("Z Write", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
            "IgnoreProjector" = "True"            
        }        

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Stencil
            {
                Ref [_StencilRef]            
                ReadMask [_StencilReadMask]  
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]          
                Pass [_StencilPass]          
                Fail [_StencilFail]          
                ZFail [_StencilZFail]        
            }           

            ZWrite [_ZWrite]                                   
            Blend  [_SrcBlend] [_DstBlend] 
            ZTest  [_ZTest]
            Cull   [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ALPHA_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                float  _Cutoff;
            CBUFFER_END
            
            // Input vertex attributes
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }           
            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                #if !_ALPHA_ON
                    color.a = 1;
                #endif

                #if _ALPHATEST_ON
                    clip(color.a - _Cutoff);
                #endif                                     

                return color;
            }
            ENDHLSL
        }
    }
    
    // Fallback Shader
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnlitWithStencilShaderGUI"
}