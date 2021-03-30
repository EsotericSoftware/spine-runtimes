// This is a premultiply-alpha adaptation of the built-in Unity shader "UI/Default" to allow Unity UI stencil masking.

Shader "Spine/SkeletonGraphic Tint Black"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible("CanvasGroup Compatible", Int) = 1

		_Color ("Tint Color", Color) = (1,1,1,1)
		_Black ("Dark Color", Color) = (0,0,0,0)
		[Toggle(_DARK_COLOR_ALPHA_ADDITIVE)] _DarkColorAlphaAdditive("Additive DarkColor.A", Int) = 0

		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Normal"

		CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
			#pragma shader_feature _ _DARK_COLOR_ALPHA_ADDITIVE
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "../CGIncludes/Spine-Common.cginc"

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

			VertexOutput vert (VertexInput IN) {
				VertexOutput OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;

				OUT.darkColor = float4(IN.uv1.r, IN.uv1.g, IN.uv2.r, IN.uv2.g);
				OUT.darkColor.rgb = GammaToTargetSpace(OUT.darkColor.rgb) + _Black.rgb;

			#ifdef _CANVAS_GROUP_COMPATIBLE
				// CanvasGroup alpha multiplies existing vertex color alpha, but
				// does not premultiply it to rgb components. This causes problems
				// with additive blending (alpha = 0), which is why we store the
				// alpha value in uv2.g (darkColor.a).
				float originalAlpha = OUT.darkColor.a;
				OUT.canvasAlpha = (originalAlpha == 0) ? IN.color.a : IN.color.a / originalAlpha;
			#else
				float originalAlpha = IN.color.a;
			#endif
				// Note: CanvasRenderer performs a GammaToTargetSpace conversion on vertex color already,
				// however incorrectly assuming straight alpha color.
				float4 vertexColor = PMAGammaToTargetSpace(half4(TargetToGammaSpace(IN.color.rgb), originalAlpha));

				OUT.color = vertexColor * float4(_Color.rgb * _Color.a, _Color.a);
				return OUT;
			}

			sampler2D _MainTex;
			#include "../CGIncludes/Spine-Skeleton-Tint-Common.cginc"

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
		ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
