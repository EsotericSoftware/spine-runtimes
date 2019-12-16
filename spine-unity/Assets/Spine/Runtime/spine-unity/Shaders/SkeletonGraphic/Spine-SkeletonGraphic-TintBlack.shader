// This is a premultiply-alpha adaptation of the built-in Unity shader "UI/Default" to allow Unity UI stencil masking.

Shader "Spine/SkeletonGraphic Tint Black"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible("CanvasGroup Compatible", Int) = 0

		_Color ("Tint", Color) = (1,1,1,1)
		_Black ("Black Point", Color) = (0,0,0,0)

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
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

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
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float4 worldPosition : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _Color;
			fixed4 _Black;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			VertexOutput vert (VertexInput IN) {
				VertexOutput OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;

				OUT.color = IN.color * float4(_Color.rgb * _Color.a, _Color.a); // Combine a PMA version of _Color with vertexColor.
				OUT.uv1 = IN.uv1;
				OUT.uv2 = IN.uv2;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag (VertexOutput IN) : SV_Target
			{
				half4 texColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

				#if defined(_STRAIGHT_ALPHA_INPUT)
				texColor.rgb *= texColor.a;
				#endif

				texColor *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#ifdef UNITY_UI_ALPHACLIP
				clip (texColor.a - 0.001);
				#endif

				half4 color = (texColor * IN.color) + float4(((1-texColor.rgb) * (_Black.rgb + float3(IN.uv1.r, IN.uv1.g, IN.uv2.r)) * texColor.a * _Color.a * IN.color.a), 0);
				#ifdef _CANVAS_GROUP_COMPATIBLE
				// CanvasGroup alpha sets vertex color alpha, but does not premultiply it to rgb components.
				color.rgb *= IN.color.a;
				#endif
				return color;
			}
		ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
