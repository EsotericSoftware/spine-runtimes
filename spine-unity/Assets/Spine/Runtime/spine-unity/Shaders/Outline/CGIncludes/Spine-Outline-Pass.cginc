#ifndef SPINE_OUTLINE_PASS_INCLUDED
#define SPINE_OUTLINE_PASS_INCLUDED

#include "UnityCG.cginc"

#ifdef SKELETON_GRAPHIC
#include "UnityUI.cginc"
#endif

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

	float4 texColor = fixed4(0,0,0,0);

	float outlineWidthCompensated = _OutlineWidth / (_OutlineReferenceTexWidth * _MainTex_TexelSize.x);
	float xOffset = _MainTex_TexelSize.x * outlineWidthCompensated;
	float yOffset = _MainTex_TexelSize.y * outlineWidthCompensated;
	float xOffsetDiagonal = _MainTex_TexelSize.x * outlineWidthCompensated * 0.7;
	float yOffsetDiagonal = _MainTex_TexelSize.y * outlineWidthCompensated * 0.7;

	float pixelCenter = tex2D(_MainTex, i.uv).a;

	float4 uvCenterWithLod = float4(i.uv, 0, _OutlineMipLevel);
	float pixelTop = tex2Dlod(_MainTex, uvCenterWithLod + float4(0,  yOffset, 0, 0)).a;
	float pixelBottom = tex2Dlod(_MainTex, uvCenterWithLod + float4(0, -yOffset, 0, 0)).a;
	float pixelLeft = tex2Dlod(_MainTex, uvCenterWithLod + float4(-xOffset, 0, 0, 0)).a;
	float pixelRight = tex2Dlod(_MainTex, uvCenterWithLod + float4(xOffset, 0, 0, 0)).a;
#if _USE8NEIGHBOURHOOD_ON
	float numSamples = 8;
	float pixelTopLeft = tex2Dlod(_MainTex, uvCenterWithLod + float4(-xOffsetDiagonal, yOffsetDiagonal, 0, 0)).a;
	float pixelTopRight = tex2Dlod(_MainTex, uvCenterWithLod + float4(xOffsetDiagonal, yOffsetDiagonal, 0, 0)).a;
	float pixelBottomLeft = tex2Dlod(_MainTex, uvCenterWithLod + float4(-xOffsetDiagonal, -yOffsetDiagonal, 0, 0)).a;
	float pixelBottomRight = tex2Dlod(_MainTex, uvCenterWithLod + float4(xOffsetDiagonal, -yOffsetDiagonal, 0, 0)).a;
	float average = (pixelTop + pixelBottom + pixelLeft + pixelRight +
		pixelTopLeft + pixelTopRight + pixelBottomLeft + pixelBottomRight)
		* i.vertexColorAlpha / numSamples;
#else // 4 neighbourhood
	float numSamples = 1;
	float average = (pixelTop + pixelBottom + pixelLeft + pixelRight) * i.vertexColorAlpha / numSamples;
#endif

	float thresholdStart = _ThresholdEnd * (1.0 - _OutlineSmoothness);
	float outlineAlpha = saturate((average - thresholdStart) / (_ThresholdEnd - thresholdStart)) - pixelCenter;
	texColor.rgba = lerp(texColor, _OutlineColor, outlineAlpha);

#ifdef SKELETON_GRAPHIC
	texColor *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
#endif

	return texColor;
}

#endif // SPINE_OUTLINE_PASS_INCLUDED
