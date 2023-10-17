Shader "Universal Render Pipeline/Spine/Blend Modes/Skeleton Additive" {
	Properties {
		_Color ("Tint Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
	}

	SubShader {
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One One
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

			#pragma vertex ShadowPassVertexSkeletonLit
			#pragma fragment ShadowPassFragmentSkeletonLit

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "../Include/Spine-Input-BlendModes-URP.hlsl"
			#include "../Include/Spine-SkeletonLit-ShadowCasterPass-URP.hlsl"

			ENDHLSL
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
