#ifndef SPRITE_NORMALS_PASS_URP_INCLUDED
#define SPRITE_NORMALS_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

#include "../Include/SpineCoreShaders/ShaderShared.cginc"
#include "../Include/SpineCoreShaders/SpriteLighting.cginc"

struct Varyings
{
	float4  positionCS		: SV_POSITION;
	float4  color			: COLOR;
	float2	uv				: TEXCOORD0;
	float3  normalWS		: TEXCOORD1;
	float3  tangentWS		: TEXCOORD2;
	float3  bitangentWS		: TEXCOORD3;
};

SAMPLER(sampler_BumpMap);
float4 _BumpMap_ST;

Varyings NormalsRenderingVertex(VertexInput attributes)
{
	Varyings o = (Varyings)0;

	o.positionCS = calculateLocalPos(attributes.vertex);
	o.uv = attributes.texcoord.xy;
	o.color = attributes.color;
	o.normalWS = TransformObjectToWorldDir(float3(0, 0, -1));

	float3 positionWS = TransformObjectToWorld(attributes.vertex.xyz);

	float backFaceSign = 1;
#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	backFaceSign = calculateBackfacingSign(positionWS.xyz);
#endif

	half3 normalWS = calculateSpriteWorldNormal(attributes, -backFaceSign);
	o.normalWS.xyz = normalWS;

#if defined(_NORMALMAP)
	o.tangentWS.xyz = calculateWorldTangent(attributes.tangent);
	o.bitangentWS.xyz = calculateSpriteWorldBinormal(attributes, o.normalWS.xyz, o.tangentWS.xyz, backFaceSign);
#endif
	return o;
}

#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"
half4 NormalsRenderingFragment(Varyings i) : SV_Target
{
	half4 mainTex = i.color * tex2D(_MainTex, i.uv);

#if defined(_NORMALMAP)
	half3 normalTS = normalize(UnpackScaleNormal(tex2D(_BumpMap, i.uv.xy), _BumpScale));
	return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
#else
	half3 normalTS = half3(0, 0, 1);
	half3 tangentWS = half3(0, 0, 0);
	half3 bitangentWS = half3(0, 0, 0);
	half3 normalWS = i.normalWS.xyz;
	return NormalsRenderingShared(mainTex, normalTS, tangentWS, bitangentWS, normalWS);
#endif
}

#endif
