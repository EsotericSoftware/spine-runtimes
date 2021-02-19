#ifndef URP_INPUT_SPRITE_INCLUDED
#define URP_INPUT_SPRITE_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerMaterial)

float4 _MainTex_ST;
half4 _Color;
half _Cutoff;
half _ShadowAlphaCutoff;

half _Metallic;
half _Glossiness;
half _GlossMapScale;

half _BumpScale;

float _BlendAmount;

float _Hue;
float _Saturation;
float _Brightness;
half4 _OverlayColor;

half4 _EmissionColor;
float _EmissionPower;

float4 _FixedNormal;

float _RimPower;
half4 _RimColor;

CBUFFER_END

#endif // URP_INPUT_SPRITE_INCLUDED
