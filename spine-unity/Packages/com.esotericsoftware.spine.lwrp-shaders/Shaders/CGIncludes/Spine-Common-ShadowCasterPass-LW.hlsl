#ifndef COMMON_SHADOW_CASTER_PASS_LW_INCLUDED
#define COMMON_SHADOW_CASTER_PASS_LW_INCLUDED

#include "SpineCoreShaders/ShaderShared.cginc"

struct AttributesSpine
{
	float4 positionOS   : POSITION;
	float3 normalOS     : NORMAL;
	float4 vertexColor : COLOR;
	float2 texcoord     : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsSpine
{
	float4 positionCS   : SV_POSITION;
	float4 texcoordAndAlpha: TEXCOORD0;
};

float4 GetShadowPositionHClip(AttributesSpine input)
{
	float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
	float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	return positionCS;
}

#endif
