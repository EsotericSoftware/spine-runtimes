#ifndef SKELETON_FORWARD_PASS_URP_INCLUDED
#define SKELETON_FORWARD_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "SpineCoreShaders/Spine-Common.cginc"
#include "SpineCoreShaders/Spine-Skeleton-Tint-Common.cginc"

struct appdata {
	float3 pos : POSITION;
	half4 color : COLOR;
	float2 uv0 : TEXCOORD0;
#if defined(_TINT_BLACK_ON)
	float2 tintBlackRG : TEXCOORD1;
	float2 tintBlackB : TEXCOORD2;
#endif

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	half4 color : COLOR0;
	float2 uv0 : TEXCOORD0;
	float4 pos : SV_POSITION;
#if defined(_TINT_BLACK_ON)
	float3 darkColor : TEXCOORD1;
#endif
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(appdata v) {
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float3 positionWS = TransformObjectToWorld(v.pos);
	o.pos = TransformWorldToHClip(positionWS);
	o.uv0 = v.uv0;
	o.color = PMAGammaToTargetSpace(v.color);
#if defined(_TINT_BLACK_ON)
	o.color *= _Color;
	o.darkColor = GammaToTargetSpace(
		half3(v.tintBlackRG.r, v.tintBlackRG.g, v.tintBlackB.r)) + _Black.rgb;
#endif
	return o;
}

half4 frag(VertexOutput i) : SV_Target{
	float4 texColor = tex2D(_MainTex, i.uv0);
#if defined(_TINT_BLACK_ON)
	return fragTintedColor(texColor, i.darkColor, i.color, _Color.a, _Black.a);
#else
	#if defined(_STRAIGHT_ALPHA_INPUT)
	texColor.rgb *= texColor.a;
	#endif
	return (texColor * i.color);
#endif
}

#endif
