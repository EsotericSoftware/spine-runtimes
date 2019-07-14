#ifndef SPRITES_DEPTH_ONLY_PASS_LW_INCLUDED
#define SPRITES_DEPTH_ONLY_PASS_LW_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
#include "SpineCoreShaders/ShaderShared.cginc"

struct AttributesSprite
{
	float4 positionOS   : POSITION;
	float4 vertexColor : COLOR;
	float2 texcoord     : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsSprite
{
	float4 positionCS   : SV_POSITION;
	float4 texcoordAndAlpha: TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsSprite DepthOnlyVertexSprite(AttributesSprite input)
{
	VaryingsSprite output = (VaryingsSprite)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);
	output.texcoordAndAlpha.a = input.vertexColor.a * _Color.a;
	output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
	return output;
}

half4 DepthOnlyFragmentSprite(VaryingsSprite input) : SV_TARGET
{
	fixed4 texureColor = calculateTexturePixel(input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _Cutoff);
	return 0;
}

#endif
