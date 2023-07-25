#ifndef SPINE_OUTLINE_PASS_URP_INCLUDED
#define SPINE_OUTLINE_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#include "SpineCoreShaders/Spine-Outline-Common.cginc"

struct VertexInput {
	float4 positionOS : POSITION;
	float2 uv : TEXCOORD0;
	float4 vertexColor : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float vertexColorAlpha : COLOR;
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vertOutline(VertexInput v) {
	VertexOutput o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.pos = TransformObjectToHClip(v.positionOS.xyz);
	o.uv = v.uv;
	o.vertexColorAlpha = v.vertexColor.a;
	return o;
}

float4 fragOutline(VertexOutput i) : SV_Target {

	float4 texColor = computeOutlinePixel(_MainTex, _MainTex_TexelSize.xy, i.uv, i.vertexColorAlpha,
		_OutlineWidth, _OutlineReferenceTexWidth, _OutlineMipLevel,
		_OutlineSmoothness, _ThresholdEnd, _OutlineOpaqueAlpha, _OutlineColor);
	return texColor;
}

#endif
