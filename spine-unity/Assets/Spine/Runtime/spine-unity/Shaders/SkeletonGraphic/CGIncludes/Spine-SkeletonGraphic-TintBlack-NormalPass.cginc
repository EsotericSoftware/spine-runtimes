#ifndef SKELETON_GRAPHIC_TINTBLACK_NORMALPASS_INCLUDED
#define SKELETON_GRAPHIC_TINTBLACK_NORMALPASS_INCLUDED

#include "UnityCG.cginc"
#include "UnityUI.cginc"
#include "../../CGIncludes/Spine-Common.cginc"

#pragma multi_compile __ UNITY_UI_ALPHACLIP

struct VertexInput {
	float4 vertex   : POSITION;
	float4 color    : COLOR;
	float2 texcoord : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 vertex   : SV_POSITION;
	half4 color    : COLOR;
	half2 texcoord  : TEXCOORD0;
	float4 darkColor : TEXCOORD1;
	float4 worldPosition : TEXCOORD2;
#ifdef _CANVAS_GROUP_COMPATIBLE
	float canvasAlpha : TEXCOORD3;
#endif
	UNITY_VERTEX_OUTPUT_STEREO
};

half4 _Color;
half4 _Black;
half4 _TextureSampleAdd;
float4 _ClipRect;

VertexOutput vert(VertexInput IN) {
	VertexOutput OUT;

	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

	OUT.worldPosition = IN.vertex;
	OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
	OUT.texcoord = IN.texcoord;

	OUT.darkColor = float4(IN.uv1.r, IN.uv1.g, IN.uv2.r, IN.uv2.g);

#ifdef _CANVAS_GROUP_COMPATIBLE
	// CanvasGroup alpha multiplies existing vertex color alpha, but
	// does not premultiply it to rgb components. This causes problems
	// with additive blending (alpha = 0), which is why we store the
	// alpha value in uv2.g (darkColor.a) and store 1.0 in vertex color alpha.
	float originalAlpha = IN.uv2.g;
	OUT.canvasAlpha = IN.color.a;
#else
	float originalAlpha = IN.color.a;
#endif

	OUT.darkColor.rgb = GammaToTargetSpace(OUT.darkColor.rgb) + (_Black.rgb * originalAlpha);

	// Note: CanvasRenderer performs a GammaToTargetSpace conversion on vertex color already,
	// however incorrectly assuming straight alpha color.
	float4 vertexColor = PMAGammaToTargetSpace(half4(TargetToGammaSpace(IN.color.rgb), originalAlpha));

	OUT.color = vertexColor * float4(_Color.rgb * _Color.a, _Color.a);
	return OUT;
}

sampler2D _MainTex;
#include "../../CGIncludes/Spine-Skeleton-Tint-Common.cginc"

half4 frag(VertexOutput IN) : SV_Target
{
	half4 texColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
	texColor *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
	#ifdef UNITY_UI_ALPHACLIP
	clip(texColor.a - 0.001);
	#endif

	float4 fragColor = fragTintedColor(texColor, IN.darkColor.rgb, IN.color, _Color.a, _Black.a);
#ifdef _CANVAS_GROUP_COMPATIBLE
	fragColor.rgba *= IN.canvasAlpha;
#endif
	return fragColor;
}

#endif
