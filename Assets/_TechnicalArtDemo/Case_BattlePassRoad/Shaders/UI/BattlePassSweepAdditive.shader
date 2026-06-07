Shader "Custom/UI/BattlePassSweepAdditiveAuto"
{
    Properties
    {
        [HideInInspector] [PerRendererData] _MainTex("UI Texture", 2D) = "white" {}

        [Header(Texture)]
        [MainTexture] [NoScaleOffset] _SweepTex("Sweep Texture", 2D) = "white" {}

        [Header(Look)]
        _SweepStrength("Overall Strength", Range(0, 1)) = 0.45
        _Intensity("Brightness Boost", Range(0, 2)) = 0.45

        [Header(Timing)]
        _SweepDuration("Travel Time", Range(0.1, 5)) = 0.85
        _RestDuration("Pause Between Sweeps", Range(0, 10)) = 3.0
        _PhaseOffset("Global Timing Offset", Range(0, 1)) = 0.0
        _EdgeFade("Fade In / Out", Range(0, 0.45)) = 0.12

        [Header(Position)]
        _StartOffsetX("Start X", Range(-3, 3)) = -1.25
        _EndOffsetX("End X", Range(-3, 3)) = 1.25
        _OffsetY("Vertical Offset", Range(-2, 2)) = 0.0

        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
        [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend DstColor One, Zero One
        ColorMask [_ColorMask]

        Pass
        {
            Name "BattlePassSweepValueBoostAuto"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 localPosition : TEXCOORD1;
                half sweepFade : TEXCOORD2;
            };

            TEXTURE2D(_SweepTex);
            SAMPLER(sampler_SweepTex);

            CBUFFER_START(UnityPerMaterial)
                half _SweepStrength;
                half _Intensity;
                half _SweepDuration;
                half _RestDuration;
                half _PhaseOffset;
                half _EdgeFade;
                half _StartOffsetX;
                half _EndOffsetX;
                half _OffsetY;
                float4 _ClipRect;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.localPosition = input.positionOS.xy;

                float sweepDuration = max((float)_SweepDuration, 0.01);
                float restDuration = max((float)_RestDuration, 0.0);
                float totalDuration = max(sweepDuration + restDuration, 0.01);

                float perImagePhase = frac(input.color.r);
                float finalPhase = frac((float)_PhaseOffset + perImagePhase);
                float cyclePosition = frac((_Time.y / totalDuration) + finalPhase);
                float cycleTime = cyclePosition * totalDuration;

                half isSweeping = 1.0h - step((half)sweepDuration, (half)cycleTime);
                half progress = saturate((half)(cycleTime / sweepDuration));

                half fadeWidth = max(_EdgeFade, 0.001h);
                half fadeIn = smoothstep(0.0h, fadeWidth, progress);
                half fadeOut = 1.0h - smoothstep(1.0h - fadeWidth, 1.0h, progress);

                output.sweepFade = fadeIn * fadeOut * isSweeping;
                output.uv = input.uv + float2(lerp(_StartOffsetX, _EndOffsetX, progress), _OffsetY);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half insideX = step(0.0h, input.uv.x) * step(input.uv.x, 1.0h);
                half insideY = step(0.0h, input.uv.y) * step(input.uv.y, 1.0h);
                half inside = insideX * insideY;

                half textureMask = SAMPLE_TEXTURE2D(_SweepTex, sampler_SweepTex, input.uv).a;
                half boost = textureMask * _SweepStrength * _Intensity * inside * input.sweepFade;

                #ifdef UNITY_UI_CLIP_RECT
                    float2 clipInside = step(_ClipRect.xy, input.localPosition) * step(input.localPosition, _ClipRect.zw);
                    boost *= clipInside.x * clipInside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(boost - 0.001h);
                #endif

                return half4(boost.xxx, 1.0h);
            }

            ENDHLSL
        }
    }

    FallBack Off
}