#ifndef BLENDMODES_NORMAL_PASS_INCLUDED
#define BLENDMODES_NORMAL_PASS_INCLUDED

#include "UnityCG.cginc"
#include "../CGIncludes/Spine-Common.cginc"
uniform sampler2D _MainTex;
uniform float4 _Color;

struct VertexInput {
	float4 vertex : POSITION;
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
	o.pos = UnityObjectToClipPos(v.vertex);
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
