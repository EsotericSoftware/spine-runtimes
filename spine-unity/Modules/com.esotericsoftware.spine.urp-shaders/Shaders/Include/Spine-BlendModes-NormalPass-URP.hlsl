#ifndef BLENDMODES_NORMAL_PASS_URP_INCLUDED
#define BLENDMODES_NORMAL_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "SpineCoreShaders/Spine-Common.cginc"
#include "SpineCoreShaders/Spine-Skeleton-Tint-Common.cginc"

struct VertexInput {
	float3 pos : POSITION;
	float2 uv : TEXCOORD0;
	float4 vertexColor : COLOR;
};

struct VertexOutput {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float4 vertexColor : COLOR;
};

VertexOutput vert(VertexInput v) {
	VertexOutput o;
	float3 positionWS = TransformObjectToWorld(v.pos);
	o.pos = TransformWorldToHClip(positionWS);
	o.uv = v.uv;
	o.vertexColor = PMAGammaToTargetSpace(v.vertexColor) * float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
	return o;
}

float4 frag(VertexOutput i) : SV_Target{
	float4 texColor = tex2D(_MainTex, i.uv);

	#if defined(_STRAIGHT_ALPHA_INPUT)
	texColor.rgb *= texColor.a;
	#endif

	return (texColor * i.vertexColor);
}

#endif
