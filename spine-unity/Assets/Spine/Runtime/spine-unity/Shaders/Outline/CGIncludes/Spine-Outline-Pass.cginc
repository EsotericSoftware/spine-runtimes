#ifndef SPINE_OUTLINE_PASS_INCLUDED
#define SPINE_OUTLINE_PASS_INCLUDED

#include "UnityCG.cginc"

#ifdef SKELETON_GRAPHIC
#include "UnityUI.cginc"
#endif

#include "../../CGIncludes/Spine-Outline-Common.cginc"

sampler2D _MainTex;

float _OutlineWidth;
float4 _OutlineColor;
float4 _MainTex_TexelSize;
float _ThresholdEnd;
float _OutlineSmoothness;
float _OutlineMipLevel;
int _OutlineReferenceTexWidth;

#ifdef SKELETON_GRAPHIC
float4 _ClipRect;
#endif

struct VertexInput {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float4 vertexColor : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float vertexColorAlpha : COLOR;
#ifdef SKELETON_GRAPHIC
	float4 worldPosition : TEXCOORD1;
#endif
	UNITY_VERTEX_OUTPUT_STEREO
};


#ifdef SKELETON_GRAPHIC

VertexOutput vertOutlineGraphic(VertexInput v) {
	VertexOutput o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.worldPosition = v.vertex;
	o.pos = UnityObjectToClipPos(o.worldPosition);
	o.uv = v.uv;

#ifdef UNITY_HALF_TEXEL_OFFSET
	o.pos.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
#endif

	o.vertexColorAlpha = v.vertexColor.a;
	return o;
}

#else // !SKELETON_GRAPHIC

VertexOutput vertOutline(VertexInput v) {
	VertexOutput o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	o.vertexColorAlpha = v.vertexColor.a;
	return o;
}
#endif

float4 fragOutline(VertexOutput i) : SV_Target {

	float4 texColor = computeOutlinePixel(_MainTex, _MainTex_TexelSize.xy, i.uv, i.vertexColorAlpha,
		_OutlineWidth, _OutlineReferenceTexWidth, _OutlineMipLevel,
		_OutlineSmoothness, _ThresholdEnd, _OutlineColor);

#ifdef SKELETON_GRAPHIC
	texColor *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
#endif

	return texColor;
}

#endif // SPINE_OUTLINE_PASS_INCLUDED
