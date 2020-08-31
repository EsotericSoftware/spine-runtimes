#ifndef SPRITES_DEPTH_ONLY_PASS_INCLUDED
#define SPRITES_DEPTH_ONLY_PASS_INCLUDED

#include "UnityCG.cginc"

sampler2D _MainTex;
float _Cutoff;
float _ZWriteOffset;

struct VertexInput {
	float4 positionOS : POSITION;
	float2 texcoord : TEXCOORD0;
	float4 vertexColor : COLOR;
};

struct VertexOutput {
	float4 positionCS : SV_POSITION;
	float4 texcoordAndAlpha: TEXCOORD0;
};

VertexOutput DepthOnlyVertex (VertexInput v) {
	VertexOutput o;
	o.positionCS = UnityObjectToClipPos(v.positionOS - float4(0, 0, _ZWriteOffset, 0));
	o.texcoordAndAlpha.xy = v.texcoord;
	o.texcoordAndAlpha.z = 0;
	o.texcoordAndAlpha.a = v.vertexColor.a;
	return o;
}

float4 DepthOnlyFragment (VertexOutput input) : SV_Target{
	float4 texColor = tex2D(_MainTex, input.texcoordAndAlpha.rg);

	#if defined(_STRAIGHT_ALPHA_INPUT)
	texColor.rgb *= texColor.a;
	#endif

	clip(texColor.a * input.texcoordAndAlpha.a - _Cutoff);
	return 0;
}

#endif
