Shader "Universal Render Pipeline/Spine/Sprite"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0

		_EmissionColor("Color", Color) = (0,0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		_EmissionPower("Emission Power", Float) = 2.0

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_DiffuseRamp("Diffuse Ramp Texture", 2D) = "gray" {}

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

		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _RenderQueue("__queue", Float) = 0.0
		[HideInInspector] _Cull("__cull", Float) = 0.0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Compare", Float) = 0.0 // Disabled stencil test by default
	}

	SubShader
	{
		// Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Sprite" "AlphaDepth"="False" "CanUseSpriteAtlas"="True" "IgnoreProjector"="True" }
		LOD 150

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		// ------------------------------------------------------------------
		//  Forward pass.
		Pass
		{
			// Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
			// no LightMode tag are also rendered by Universal Render Pipeline
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull[_Cull]

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAPREMULTIPLY_VERTEX_ONLY _ADDITIVEBLEND _ADDITIVEBLEND_SOFT _MULTIPLYBLEND _MULTIPLYBLEND_X2
			#pragma shader_feature _ _FIXED_NORMALS_VIEWSPACE _FIXED_NORMALS_VIEWSPACE_BACKFACE _FIXED_NORMALS_MODELSPACE _FIXED_NORMALS_MODELSPACE_BACKFACE _FIXED_NORMALS_WORLDSPACE
			#pragma shader_feature _ _SPECULAR _SPECULAR_GLOSSMAP
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ALPHA_CLIP
			#pragma shader_feature _EMISSION
			#pragma shader_feature _DIFFUSE_RAMP
			#pragma shader_feature _ _FULLRANGE_HARD_RAMP _FULLRANGE_SOFT_RAMP _OLD_HARD_RAMP _OLD_SOFT_RAMP
			#pragma shader_feature _COLOR_ADJUST
			#pragma shader_feature _RIM_LIGHTING
			#pragma shader_feature _TEXTURE_BLEND
			#pragma shader_feature _FOG
			#pragma shader_feature _RECEIVE_SHADOWS_OFF
			#pragma shader_feature _LIGHT_AFFECTS_ADDITIVE

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fog
			#pragma multi_compile _ PIXELSNAP_ON

			// -------------------------------------
			// Universal Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			// Farward+ renderer keywords
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile _ _FORWARD_PLUS
			#pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer

			//--------------------------------------
			// Spine related keywords
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex ForwardPassVertexSprite
			#pragma fragment ForwardPassFragmentSprite

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-Sprite-URP.hlsl"
			#include "Include/Spine-Sprite-ForwardPass-URP.hlsl"
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

			#pragma vertex ShadowPassVertexSprite
			#pragma fragment ShadowPassFragmentSprite

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-Sprite-URP.hlsl"
			#include "Include/Spine-Sprite-ShadowCasterPass-URP.hlsl"
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

			#pragma vertex DepthOnlyVertexSprite
			#pragma fragment DepthOnlyFragmentSprite

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
			#include "Include/Spine-Input-Sprite-URP.hlsl"
			#include "Include/Spine-Sprite-DepthOnlyPass-URP.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "Unlit"
			Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent"}

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAPREMULTIPLY_VERTEX_ONLY _ADDITIVEBLEND _ADDITIVEBLEND_SOFT _MULTIPLYBLEND _MULTIPLYBLEND_X2
			#if defined(_ALPHAPREMULTIPLY_VERTEX_ONLY) || defined(_ALPHABLEND_ON)
			#define _STRAIGHT_ALPHA_INPUT
			#endif

			#pragma prefer_hlslcc gles
			#pragma vertex vert
			#pragma fragment frag

			#undef LIGHTMAP_ON

			#define USE_URP
			#define fixed4 half4
			#define fixed3 half3
			#define fixed half
			#include "Include/Spine-Input-URP.hlsl"
			#include "Include/Spine-Skeleton-ForwardPass-URP.hlsl"
			ENDHLSL
		}
	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "SpineSpriteShaderGUI"
}
