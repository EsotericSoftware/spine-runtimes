Shader "Universal Render Pipeline/Spine/Outline/Skeleton-OutlineOnly ZWrite" {
	Properties {
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex("Main Texture", 2D) = "black" {}
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

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
		// Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags { "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Cull Off
		ZWrite On
		Blend One OneMinusSrcAlpha

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

 		Pass {
 			Name "Outline"
 
 			ZWrite On
 
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
 
 			#define USE_URP
 			#define fixed4 half4
 			#define fixed3 half3
 			#define fixed half
 			#define NO_CUTOFF_PARAM
 			#include "../Include/Spine-Input-Outline-URP.hlsl"
 			#include "../Include/Spine-Outline-Pass-URP.hlsl"
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
			#include "../Include/Spine-Input-URP.hlsl"
			#include "../Include/Spine-DepthOnlyPass-URP.hlsl"
			ENDHLSL
		}

		// This pass is used when drawing to a _CameraNormalsTexture texture
		Pass
		{
			Name "DepthNormals"
			Tags{"LightMode" = "DepthNormals"}

			ZWrite On

			HLSLPROGRAM
			#pragma vertex DepthNormalsVertex
			#pragma fragment DepthNormalsFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ _DOUBLE_SIDED_LIGHTING

			// -------------------------------------
			// Universal Pipeline keywords
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "../Include/Spine-Input-URP.hlsl"
			#include "../Include/Spine-DepthNormalsPass-URP.hlsl"
			ENDHLSL
		}
	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "SpineShaderWithOutlineGUI"
}
