Shader "Spine/Outline/OutlineOnly-ZWrite" {
	Properties {
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.1
		_ZWriteOffset ("Depth offset", Range(0,1)) = 0.01
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector][MaterialToggle(_USE_SCREENSPACE_OUTLINE_WIDTH)] _UseScreenSpaceOutlineWidth("Width in Screen Space", Float) = 0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass
		{
			Name "DepthOnly"

			ZWrite On
			ColorMask 0
			Cull Off

			CGPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			#include "../CGIncludes/Spine-DepthOnlyPass.cginc"
			ENDCG
		}

		Pass {
			Name "Outline"
			CGPROGRAM
			#pragma vertex vertOutline
			#pragma fragment fragOutline
			#pragma shader_feature _ _USE8NEIGHBOURHOOD_ON
			#pragma shader_feature _ _USE_SCREENSPACE_OUTLINE_WIDTH
			#include "CGIncludes/Spine-Outline-Pass.cginc"
			ENDCG
		}
	}
	FallBack "Spine/Skeleton"
	CustomEditor "SpineShaderWithOutlineGUI"
}
