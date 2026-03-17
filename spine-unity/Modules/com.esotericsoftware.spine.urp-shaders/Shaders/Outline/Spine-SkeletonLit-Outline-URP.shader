Shader "Universal Render Pipeline/Spine/Outline/Skeleton Lit" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_ZWRITE)] _ZWrite("Depth Write", Float) = 0.0
		[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive Shadows", Int) = 0
		[Toggle(_DOUBLE_SIDED_LIGHTING)] _DoubleSidedLighting("Double-Sided Lighting", Int) = 0
		[MaterialToggle(_LIGHT_AFFECTS_ADDITIVE)] _LightAffectsAdditive("Light Affects Additive", Float) = 0
		[MaterialToggle(_TINT_BLACK_ON)]  _TintBlack("Tint Black", Float) = 0
		_Color("    Light Color", Color) = (1,1,1,1)
		_Black("    Dark Color", Color) = (0,0,0,0)
		[MaterialToggle(_ADAPTIVE_PROBE_VOLUMES_PER_PIXEL)]  _AdaptiveProbeVolumesPerPixel("APV per Pixel", Float) = 1
		[MaterialToggle(_FOG)] _Fog("Fog", Float) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Compare", Float) = 8 // Set to Always as default

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
		// Lightweight Pipeline tag is required. If Lightweight render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Cull Off
		ZWrite[_ZWrite]
		Blend One OneMinusSrcAlpha

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Blend One OneMinusSrcAlpha
			ZWrite Off
			Cull Off

			Name "Outline"
			Tags { "LightMode" = "SRPDefaultUnlit" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex vertOutline
			#pragma fragment fragOutline
			#pragma shader_feature _ _USE8NEIGHBOURHOOD_ON
			#pragma shader_feature _ _USE_SCREENSPACE_OUTLINE_WIDTH
			#pragma shader_feature _ _OUTLINE_FILL_INSIDE

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_local _ PIXELSNAP_ON

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#define NO_CUTOFF_PARAM
			#include "../Include/Spine-Input-Outline-URP.hlsl"
			#include "../Include/Spine-Outline-Pass-URP.hlsl"
			ENDHLSL
		}

		UsePass "Universal Render Pipeline/Spine/Skeleton Lit/FORWARDLIT"

		UsePass "Universal Render Pipeline/Spine/Skeleton Lit/SHADOWCASTER"

		UsePass "Universal Render Pipeline/Spine/Skeleton Lit/DEPTHONLY"

		UsePass "Universal Render Pipeline/Spine/Skeleton Lit/DEPTHNORMALS"
	}

	FallBack "Universal Render Pipeline/Spine/Skeleton"
	CustomEditor "SpineShaderWithOutlineGUI"
}
