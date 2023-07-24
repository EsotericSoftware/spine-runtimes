#ifndef SPRITES_DEPTH_NORMALS_PASS_URP_INCLUDED
#define SPRITES_DEPTH_NORMALS_PASS_URP_INCLUDED

#include "Include/Spine-Sprite-Common-URP.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "SpineCoreShaders/SpriteLighting.cginc"
#include "SpineCoreShaders/Spine-Common.cginc"
#include "Spine-Common-URP.hlsl"

//#include "Include/Spine-Sprite-Common-URP.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct VaryingsSprite
{
	float4 pos : SV_POSITION;
	fixed4 vertexColor : COLOR;
	float3 texcoord : TEXCOORD0;

#if defined(_NORMALMAP)
	half4 normalWorld : TEXCOORD4;
	half4 tangentWorld : TEXCOORD5;
	half4 binormalWorld : TEXCOORD6;
#else
	half3 normalWorld : TEXCOORD4;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsSprite DepthNormalsVertexSprite(VertexInput input)
{
	VaryingsSprite output = (VaryingsSprite)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.pos = calculateLocalPos(input.vertex);
	output.vertexColor = calculateVertexColor(input.color);
	output.texcoord = float3(calculateTextureCoord(input.texcoord), 0);

	float backFaceSign = 1;
#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	backFaceSign = calculateBackfacingSign(positionWS.xyz);
#endif

	half3 normalWS = calculateSpriteWorldNormal(input, -backFaceSign);
	output.normalWorld.xyz = normalWS;
#if defined(_NORMALMAP)
	output.tangentWorld.xyz = calculateWorldTangent(input.tangent);
	output.binormalWorld.xyz = calculateSpriteWorldBinormal(input, output.normalWorld.xyz, output.tangentWorld.xyz, backFaceSign);
#endif

	return output;
}

void DepthNormalsFragmentSprite(VaryingsSprite input,
	out half4 outNormalWS : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
	, out float4 outRenderingLayers : SV_Target1
#endif
	)
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	fixed4 texureColor = calculateTexturePixel(input.texcoord.xy);
	ALPHA_CLIP(texureColor, input.vertexColor)

#if defined(PER_PIXEL_LIGHTING) && defined(_NORMALMAP)
	half3 normalWS = calculateNormalFromBumpMap(input.texcoord.xy, input.tangentWorld.xyz, input.binormalWorld.xyz, input.normalWorld.xyz);
#else
	half3 normalWS = input.normalWorld.xyz;
#endif

#if defined(_GBUFFER_NORMALS_OCT)
	float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms.
	float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
	half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
	outNormalWS = half4(packedNormalWS, 0.0);
#else
	outNormalWS = half4(normalWS, 0.0);
#endif

#ifdef USE_WRITE_RENDERING_LAYERS
	uint renderingLayers = GetMeshRenderingLayerBackwardsCompatible();
	outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
