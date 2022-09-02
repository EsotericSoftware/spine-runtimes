#ifndef URP_INPUT_OUTLINE_INCLUDED
#define URP_INPUT_OUTLINE_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

////////////////////////////////////////
// Defines
//
#undef LIGHTMAP_ON

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
half _Cutoff;

float _OutlineWidth;
float4 _OutlineColor;
float4 _MainTex_TexelSize;
float _ThresholdEnd;
float _OutlineSmoothness;
float _OutlineOpaqueAlpha;
float _OutlineMipLevel;
int _OutlineReferenceTexWidth;
CBUFFER_END

sampler2D _MainTex;

#endif // URP_INPUT_OUTLINE_INCLUDED
