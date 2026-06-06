Shader "Custom/UI/BattlePassScrollingPattern"
{
    Properties
    {
        [Header(Pattern Art Controls)]
        [MainTexture] [NoScaleOffset] _MainTex("Pattern Texture", 2D) = "white" {}
        [MainColor] _TintColor("Pattern Tint + Opacity", Color) = (1, 1, 1, 0.18)

        _PatternTiling("Pattern Density", Range(0.25, 12)) = 5
        _PatternRotation("Pattern Rotation", Range(-180, 180)) = -15

        [Space(8)]
        [Header(Motion)]
        _ScrollSpeedX("Scroll Speed Horizontal", Range(-0.25, 0.25)) = 0.015
        _ScrollSpeedY("Scroll Speed Vertical", Range(-0.25, 0.25)) = -0.008

        // Hidden technical UI compatibility controls.
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "BattlePassScrollingPattern"

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
                float2 patternUv : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
                half4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float _PatternTiling;
                float _PatternRotation;
                float _ScrollSpeedX;
                float _ScrollSpeedY;
                float4 _ClipRect;
            CBUFFER_END

            float2 RotateAroundOrigin(float2 value, float degrees)
            {
                float angle = radians(degrees);
                float sineValue = sin(angle);
                float cosineValue = cos(angle);

                float2 rotated;
                rotated.x = value.x * cosineValue - value.y * sineValue;
                rotated.y = value.x * sineValue + value.y * cosineValue;

                return rotated;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.localPosition = input.positionOS;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                // Use local UI position instead of normalized UV.
                // This keeps square pattern textures visually square even when the RawImage is stretched.
                float2 localPatternUv = input.positionOS.xy * (_PatternTiling / 1000.0);
                float2 rotatedUv = RotateAroundOrigin(localPatternUv, _PatternRotation);
                float2 scrollOffset = _Time.y * float2(_ScrollSpeedX, _ScrollSpeedY);

                output.patternUv = rotatedUv + scrollOffset;
                output.color = input.color * _TintColor;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 pattern = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.patternUv);

                half4 color = pattern * input.color;

                #ifdef UNITY_UI_CLIP_RECT
                    float2 position = input.localPosition.xy;
                    float2 inside = step(_ClipRect.xy, position) * step(position, _ClipRect.zw);
                    color.a *= inside.x * inside.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif

                return color;
            }

            ENDHLSL
        }
    }

    FallBack Off
}