Shader "Universal Render Pipeline/2D/Spine/Outline/Sprite"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_MaskTex("Mask", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0

		_EmissionColor("Color", Color) = (0,0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		_EmissionPower("Emission Power", Float) = 2.0

		_FixedNormal("Fixed Normal", Vector) = (0,0,1,1)
		_ZWrite("Depth Write", Float) = 0.0
		_Cutoff("Depth alpha cutoff", Range(0,1)) = 0.0
		_ShadowAlphaCutoff("Shadow alpha cutoff", Range(0,1)) = 0.1
		_CustomRenderQueue("Custom Render Queue", Float) = 0.0

		_OverlayColor("Overlay Color", Color) = (0,0,0,0)
		_Hue("Hue", Range(-0.5,0.5)) = 0.0
		_Saturation("Saturation", Range(0,2)) = 1.0
		_Brightness("Brightness", Range(0,2)) = 1.0

		_RimPower("Rim Power", Float) = 2.0
		_RimColor("Rim Color", Color) = (1,1,1,1)

		_BlendTex("Blend Texture", 2D) = "white" {}
		_BlendAmount("Blend", Range(0,1)) = 0.0

		[MaterialToggle(_LIGHT_AFFECTS_ADDITIVE)] _LightAffectsAdditive("Light Affects Additive", Float) = 0
		[MaterialToggle(_TINT_BLACK_ON)]  _TintBlack("Tint Black", Float) = 0
		_Black("Dark Color", Color) = (0,0,0,0)

		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _RenderQueue("__queue", Float) = 0.0
		[HideInInspector] _Cull("__cull", Float) = 0.0
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

	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	ENDHLSL

	SubShader
	{
		// UniversalPipeline tag is required. If Universal render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags {"Queue" = "Transparent" "RenderType" = "Sprite" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "AlphaDepth" = "False"  "CanUseSpriteAtlas" = "True" }

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Blend One OneMinusSrcAlpha
		ZWrite[_ZWrite]
		Cull[_Cull]

		Pass {
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
			#include "../../Include/Spine-Input-Outline-URP.hlsl"
			#include "../../Include/Spine-Outline-Pass-URP.hlsl"
			ENDHLSL
		}

		UsePass "Universal Render Pipeline/2D/Spine/Sprite/UNIVERSAL2D"

		UsePass "Universal Render Pipeline/2D/Spine/Sprite/NORMALS"

		UsePass "Universal Render Pipeline/2D/Spine/Sprite/UNLIT"
	}

	FallBack "Universal Render Pipeline/2D/Spine/Skeleton Lit"
	CustomEditor "SpineSpriteShaderGUI"
}
