#ifndef SPRITES_SHADOW_CASTER_PASS_URP_INCLUDED
#define SPRITES_SHADOW_CASTER_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
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

VaryingsSpine ShadowPassVertexSprite(AttributesSpine input)
{
	VaryingsSpine output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);

	half3 fixedNormalOS = half3(0, 0, -1);
	half3 normalWS = normalize(TransformObjectToWorldNormal(fixedNormalOS));
	// flip normal for shadow bias if necessary
	// unfortunately we have to compute the sign here in the vertex shader
	// instead of using VFACE in fragment shader stage.
	half3 viewDirWS = UNITY_MATRIX_V[2].xyz;
	half faceSign = sign(dot(viewDirWS, normalWS));
	normalWS *= faceSign;

	output.positionCS = GetShadowPositionHClip(input.positionOS.xyz, normalWS);
	output.texcoordAndAlpha.a = input.vertexColor.a * _Color.a;
	return output;
}

#include "SpineCoreShaders/ShaderShared.cginc"

half4 ShadowPassFragmentSprite(VaryingsSpine input) : SV_TARGET
{
	fixed4 texureColor = calculateTexturePixel(input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _ShadowAlphaCutoff);

	return 0;
}

#endif
