#ifndef SPRITES_DEPTH_NORMALS_PASS_URP_INCLUDED
#define SPRITES_DEPTH_NORMALS_PASS_URP_INCLUDED

#include "Include/Spine-Sprite-Common-URP.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct AttributesSprite
{
	float4 positionOS   : POSITION;
	float3 normalOS     : NORMAL;
	float4 vertexColor : COLOR;
	float2 texcoord     : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsSprite
{
	float4 positionCS   : SV_POSITION;
	float4 texcoordAndAlpha: TEXCOORD0;
	float3 normalWS     : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsSprite DepthNormalVertexSprite(AttributesSprite input)
{
	VaryingsSprite output = (VaryingsSprite)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);
	output.texcoordAndAlpha.a = input.vertexColor.a * _Color.a;
	output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
	output.normalWS = NormalizeNormalPerVertex(input.normalOS);
	return output;
}

half4 DepthNormalFragmentSprite(VaryingsSprite input) : SV_TARGET
{
	fixed4 texureColor = tex2D(_MainTex, input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _Cutoff);
	return half4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS, true)), 0.0, 0.0);
}

#endif
