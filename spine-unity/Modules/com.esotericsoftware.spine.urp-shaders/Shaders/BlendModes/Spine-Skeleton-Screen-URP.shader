Shader "Universal Render Pipeline/Spine/Blend Modes/Skeleton Screen" {
	Properties {
		_Color ("Tint Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader {
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcColor
		Lighting Off

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "Forward"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT

			#undef LIGHTMAP_ON

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "../Include/Spine-Input-BlendModes-URP.hlsl"
			#include "../Include/Spine-BlendModes-NormalPass-URP.hlsl"
			ENDHLSL
		}

		UsePass "Universal Render Pipeline/Spine/Blend Modes/Skeleton Additive/SHADOWCASTER"
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
