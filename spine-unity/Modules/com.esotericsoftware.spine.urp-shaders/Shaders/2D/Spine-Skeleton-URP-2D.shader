Shader "Universal Render Pipeline/2D/Spine/Skeleton" {
	Properties {
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[MaterialToggle(_TINT_BLACK_ON)]  _TintBlack("Tint Black", Float) = 0
		_Color("    Light Color", Color) = (1,1,1,1)
		_Black("    Dark Color", Color) = (0,0,0,0)
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
	}

	SubShader {
		// Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
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
			Tags { "LightMode" = "Universal2D" }

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			//--------------------------------------
			// Spine related keywords
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _TINT_BLACK_ON
			#pragma vertex vert
			#pragma fragment frag

			#undef LIGHTMAP_ON

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "../Include/Spine-Input-URP.hlsl"
			#include "../Include/Spine-Skeleton-ForwardPass-URP.hlsl"
			ENDHLSL
		}
	}

	FallBack "Hidden/InternalErrorShader"
}
