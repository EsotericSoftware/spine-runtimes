Shader "Universal Render Pipeline/Spine/Skeleton Lit" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive Shadows", Int) = 0
		[Toggle(_DOUBLE_SIDED_LIGHTING)] _DoubleSidedLighting("Double-Sided Lighting", Int) = 0
		[MaterialToggle(_LIGHT_AFFECTS_ADDITIVE)] _LightAffectsAdditive("Light Affects Additive", Float) = 0
		[MaterialToggle(_TINT_BLACK_ON)]  _TintBlack("Tint Black", Float) = 0
		_Color("    Light Color", Color) = (1,1,1,1)
		_Black("    Dark Color", Color) = (0,0,0,0)
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Compare", Float) = 8 // Set to Always as default
	}

	SubShader {
		// Lightweight Pipeline tag is required. If Lightweight render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			// -------------------------------------
			// Lightweight Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _LIGHT_AFFECTS_ADDITIVE
			#pragma shader_feature _TINT_BLACK_ON
			// Farward+ renderer keywords
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile _ _FORWARD_PLUS
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer

			//--------------------------------------
			// Spine related keywords
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _DOUBLE_SIDED_LIGHTING
			#pragma shader_feature _RECEIVE_SHADOWS_OFF _RECEIVE_SHADOWS
			#pragma vertex vert
			#pragma fragment frag

			#undef LIGHTMAP_ON

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-URP.hlsl"
			#include "Include/Spine-SkeletonLit-ForwardPass-URP.hlsl"
			ENDHLSL
	 	}

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ColorMask 0
			ZTest LEqual
			Cull Off

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature _ _DOUBLE_SIDED_LIGHTING

			#pragma vertex ShadowPassVertexSkeletonLit
			#pragma fragment ShadowPassFragmentSkeletonLit

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-URP.hlsl"
			#include "Include/Spine-SkeletonLit-ShadowCasterPass-URP.hlsl"

			ENDHLSL
		}

			Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask R
			Cull Off

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-URP.hlsl"
			#include "Include/Spine-DepthOnlyPass-URP.hlsl"
			ENDHLSL
		}
	}

	FallBack "Hidden/InternalErrorShader"
}
