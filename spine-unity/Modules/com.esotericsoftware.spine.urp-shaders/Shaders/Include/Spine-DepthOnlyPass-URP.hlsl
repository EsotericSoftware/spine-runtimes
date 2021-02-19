#ifndef SPRITES_DEPTH_ONLY_PASS_URP_INCLUDED
#define SPRITES_DEPTH_ONLY_PASS_URP_INCLUDED

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
	float4 positionCS   : SV_POSITION;
	float4 texcoordAndAlpha: TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsSpine DepthOnlyVertex(AttributesSpine input)
{
	VaryingsSpine output = (VaryingsSpine)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);
	output.texcoordAndAlpha.a = input.vertexColor.a;
	output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
	return output;
}

half4 DepthOnlyFragment(VaryingsSpine input) : SV_TARGET
{
	fixed4 texureColor = tex2D(_MainTex, input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _Cutoff);
	return 0;
}

#endif
