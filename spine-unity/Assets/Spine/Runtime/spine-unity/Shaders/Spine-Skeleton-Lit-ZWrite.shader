// - Vertex Lit + ShadowCaster
// - Premultiplied Alpha Blending (Optional straight alpha input)
// - Double-sided, ZWrite

Shader "Spine/Skeleton Lit ZWrite" {
	Properties {
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.1
		_ShadowAlphaCutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_DOUBLE_SIDED_LIGHTING)] _DoubleSidedLighting("Double-Sided Lighting", Int) = 0
		[MaterialToggle(_LIGHT_AFFECTS_ADDITIVE)] _LightAffectsAdditive("Light Affects Additive", Float) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector][MaterialToggle(_USE_SCREENSPACE_OUTLINE_WIDTH)] _UseScreenSpaceOutlineWidth("Width in Screen Space", Float) = 0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector][MaterialToggle(_OUTLINE_FILL_INSIDE)]_Fill("Fill", Float) = 0
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "Normal"

			Tags { "LightMode"="Vertex" "Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent" }

			ZWrite On
			Cull Off
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _DOUBLE_SIDED_LIGHTING
			#pragma shader_feature _ _LIGHT_AFFECTS_ADDITIVE
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#define _ALPHA_CLIP
			#pragma multi_compile __ POINT SPOT
			#include "CGIncludes/Spine-Skeleton-Lit-Common.cginc"
			ENDCG

	 	}

		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1

			Fog { Mode Off }
			ZWrite On
			ZTest LEqual
			Cull Off
			Lighting Off

			CGPROGRAM
			#pragma vertex vertShadow
			#pragma fragment fragShadow
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest

			#define SHADOW_CUTOFF _ShadowAlphaCutoff
			#include "CGIncludes/Spine-Skeleton-Lit-Common-Shadow.cginc"

			ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
