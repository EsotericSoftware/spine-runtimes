#ifndef SKELETONLIT_SHADOW_CASTER_PASS_URP_INCLUDED
#define SKELETONLIT_SHADOW_CASTER_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;

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

float4 GetShadowPositionHClip(float3 positionOS, half3 normalWS)
{
	float3 positionWS = TransformObjectToWorld(positionOS);
	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

	return positionCS;
}

VaryingsSpine ShadowPassVertexSkeletonLit(AttributesSpine input)
{
	VaryingsSpine output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);

	half3 fixedNormalOS = half3(0, 0, -1);
	half3 normalWS = normalize(TransformObjectToWorldNormal(fixedNormalOS));
#ifdef _DOUBLE_SIDED_LIGHTING
	// flip normal for shadow bias if necessary
	// unfortunately we have to compute the sign here in the vertex shader
	// instead of using VFACE in fragment shader stage.
	half3 viewDirWS = UNITY_MATRIX_V[2].xyz;
	half faceSign = sign(dot(viewDirWS, normalWS));
	normalWS *= faceSign;
#endif

	output.positionCS = GetShadowPositionHClip(input.positionOS.xyz, normalWS);
	output.texcoordAndAlpha.a = input.vertexColor.a;
	return output;
}

half4 ShadowPassFragmentSkeletonLit(VaryingsSpine input) : SV_TARGET
{
	fixed4 texureColor = tex2D(_MainTex, input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _Cutoff);
	return 0;
}

#endif
