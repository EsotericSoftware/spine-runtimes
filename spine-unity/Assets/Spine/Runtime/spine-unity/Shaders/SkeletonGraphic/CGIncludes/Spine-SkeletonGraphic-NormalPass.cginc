#ifndef SKELETON_GRAPHIC_NORMALPASS_INCLUDED
#define SKELETON_GRAPHIC_NORMALPASS_INCLUDED

#include "UnityCG.cginc"
#include "UnityUI.cginc"
#include "../../CGIncludes/Spine-Common.cginc"

#pragma multi_compile __ UNITY_UI_ALPHACLIP

struct VertexInput {
	float4 vertex   : POSITION;
	float4 color    : COLOR;
	float2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 vertex   : SV_POSITION;
	fixed4 color    : COLOR;
	half2 texcoord  : TEXCOORD0;
	float4 worldPosition : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO
};

#ifndef ENABLE_GRAYSCALE
fixed4 _Color;
#endif
fixed4 _TextureSampleAdd;
float4 _ClipRect;

#ifdef ENABLE_FILL
float4 _FillColor;
float _FillPhase;
#endif
#ifdef ENABLE_GRAYSCALE
float _GrayPhase;
#endif

VertexOutput vert (VertexInput IN) {
	VertexOutput OUT;

	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

	OUT.worldPosition = IN.vertex;
	OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
	OUT.texcoord = IN.texcoord;

	#ifdef UNITY_HALF_TEXEL_OFFSET
	OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
	#endif

#ifdef _CANVAS_GROUP_COMPATIBLE
	half4 vertexColor = IN.color;
	// CanvasGroup alpha sets vertex color alpha, but does not premultiply it to rgb components.
	vertexColor.rgb *= vertexColor.a;
	// Unfortunately we cannot perform the TargetToGamma and PMAGammaToTarget transformations,
	// as these would be wrong with modified alpha.
#else
	// Note: CanvasRenderer performs a GammaToTargetSpace conversion on vertex color already,
	// however incorrectly assuming straight alpha color.
	// Saturated version used to prevent numerical issues of certain low-alpha values.
	float4 vertexColor = PMAGammaToTargetSpaceSaturated(half4(TargetToGammaSpace(IN.color.rgb), IN.color.a));
#endif
	OUT.color = vertexColor;
#ifndef ENABLE_GRAYSCALE
	OUT.color *= float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
#endif

	return OUT;
}

sampler2D _MainTex;

fixed4 frag (VertexOutput IN) : SV_Target
{
	half4 texColor = tex2D(_MainTex, IN.texcoord);

	#if defined(_STRAIGHT_ALPHA_INPUT)
	texColor.rgb *= texColor.a;
	#endif

	half4 color = (texColor + _TextureSampleAdd) * IN.color;
	color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

	#ifdef UNITY_UI_ALPHACLIP
	clip (color.a - 0.001);
	#endif

	#ifdef ENABLE_FILL
	color.rgb = lerp(color.rgb, (_FillColor.rgb * color.a), _FillPhase); // make sure to PMA _FillColor.
	#endif
	#ifdef ENABLE_GRAYSCALE
	color.rgb = lerp(color.rgb, dot(color.rgb, float3(0.3, 0.59, 0.11)), _GrayPhase);
	#endif
	return color;
}

#endif
