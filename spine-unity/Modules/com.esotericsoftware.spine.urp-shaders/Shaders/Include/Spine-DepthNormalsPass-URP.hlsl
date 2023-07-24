#ifndef SPRITES_DEPTH_NORMALS_PASS_URP_INCLUDED
#define SPRITES_DEPTH_NORMALS_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
	float3 normalWS     : NORMAL;
	float4 positionCS   : SV_POSITION;
	float4 texcoordAndAlpha: TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsSpine DepthNormalsVertex(AttributesSpine input)
{
	VaryingsSpine output = (VaryingsSpine)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	half3 fixedNormal = half3(0, 0, -1);
	half3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, fixedNormal));

#ifdef _DOUBLE_SIDED_LIGHTING
	// unfortunately we have to compute the sign here in the vertex shader
	// instead of using VFACE in fragment shader stage.
	half3 viewDirWS = UNITY_MATRIX_V[2].xyz;
	half faceSign = sign(dot(viewDirWS, normalWS));
	normalWS *= faceSign;
#endif
	output.normalWS = normalWS;

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);
	output.texcoordAndAlpha.a = input.vertexColor.a;
	output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
	return output;
}

void DepthNormalsFragment(VaryingsSpine input,
	out half4 outNormalWS : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
	, out float4 outRenderingLayers : SV_Target1
#endif
	)
{
	fixed4 texureColor = tex2D(_MainTex, input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _Cutoff);

	float3 normalWS = input.normalWS;
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
