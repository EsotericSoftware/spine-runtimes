#ifndef URP_INPUT_BLEND_MODES_INCLUDED
#define URP_INPUT_BLEND_MODES_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

////////////////////////////////////////
// Defines
//
#undef LIGHTMAP_ON

CBUFFER_START(UnityPerMaterial)

float4 _MainTex_ST;
half _Cutoff;
half4 _Color;

CBUFFER_END

sampler2D _MainTex;

#endif
