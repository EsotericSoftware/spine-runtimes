Shader "Universal Render Pipeline/2D/Spine/Skeleton Lit" {
	Properties {
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[NoScaleOffset] _MaskTex("Mask", 2D) = "white" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[MaterialToggle(_LIGHT_AFFECTS_ADDITIVE)] _LightAffectsAdditive("Light Affects Additive", Float) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Compare", Float) = 0.0 // Disabled stencil test by default
	}

	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	ENDHLSL

	SubShader {
		// UniversalPipeline tag is required. If Universal render pipeline is not set in the graphics settings
		// this Subshader will fail.
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
		Cull Off
		ZWrite Off

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
			#pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
			#pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
			#pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
			#pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
			#pragma multi_compile _ _LIGHT_AFFECTS_ADDITIVE

			struct Attributes {
				float3 positionOS : POSITION;
				half4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
				half4 color : COLOR0;
				float2 uv : TEXCOORD0;
				float2 lightingUV : TEXCOORD1;
			};

			// Spine related keywords
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex CombinedShapeLightVertex
			#pragma fragment CombinedShapeLightFragment

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
			#define USE_URP
			#include "../Include/SpineCoreShaders/Spine-Common.cginc"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_MaskTex);
			SAMPLER(sampler_MaskTex);

			#if USE_SHAPE_LIGHT_TYPE_0
			SHAPE_LIGHT(0)
			#endif

			#if USE_SHAPE_LIGHT_TYPE_1
			SHAPE_LIGHT(1)
			#endif

			#if USE_SHAPE_LIGHT_TYPE_2
			SHAPE_LIGHT(2)
			#endif

			#if USE_SHAPE_LIGHT_TYPE_3
			SHAPE_LIGHT(3)
			#endif

			Varyings CombinedShapeLightVertex(Attributes v)
			{
				Varyings o = (Varyings)0;

				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv = v.uv;
				float4 clipVertex = o.positionCS / o.positionCS.w;
				o.lightingUV = ComputeScreenPos(clipVertex).xy;
				o.color = PMAGammaToTargetSpace(v.color);
				// un-premultiply for additive lights in CombinedShapeLightShared, reapply afterwards
				o.color.rgb = o.color.a == 0 ? o.color.rgb : o.color.rgb / o.color.a;
				return o;
			}

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

			half4 CombinedShapeLightFragment(Varyings i) : SV_Target
			{
				half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
			#if !defined(_STRAIGHT_ALPHA_INPUT)
				// un-premultiply for additive lights in CombinedShapeLightShared, reapply afterwards
				tex.rgb = tex.a == 0 ? tex.rgb : tex.rgb / tex.a;
			#endif

				half4 main = tex * i.color;
			#if !defined(_LIGHT_AFFECTS_ADDITIVE)
				if (i.color.a == 0)
					return half4(main.rgb * main.a, main.a);
			#endif
				half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
			#if UNITY_VERSION  < 202120
				return half4(CombinedShapeLightShared(half4(main.rgb, 1), mask, i.lightingUV).rgb * main.a, main.a);
			#else
				SurfaceData2D surfaceData;
				InputData2D inputData;
				surfaceData.albedo = main.rgb;
				surfaceData.alpha = 1;
				surfaceData.mask = mask;
				inputData.uv = i.uv;
				inputData.lightingUV = i.lightingUV;
				return half4(CombinedShapeLightShared(surfaceData, inputData).rgb * main.a, main.a);
			#endif
			}

			ENDHLSL
	 	}

		Pass
		{
			Tags { "LightMode" = "NormalsRendering"}

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
			#pragma vertex NormalsRenderingVertex
			#pragma fragment NormalsRenderingFragment

			struct Attributes
			{
				float3 positionOS   : POSITION;
				float4 color		: COLOR;
				float2 uv			: TEXCOORD0;
			};

			struct Varyings
			{
				float4  positionCS		: SV_POSITION;
				float4  color			: COLOR;
				float2	uv				: TEXCOORD0;
				float3  normalWS		: TEXCOORD1;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			Varyings NormalsRenderingVertex(Attributes attributes)
			{
				Varyings o = (Varyings)0;

				o.positionCS = TransformObjectToHClip(attributes.positionOS);
				o.uv = attributes.uv;
				o.color = attributes.color;
				o.normalWS = TransformObjectToWorldDir(float3(0, 0, -1));
				return o;
			}

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

			float4 NormalsRenderingFragment(Varyings i) : SV_Target
			{
				float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				half3 normalTS = half3(0, 0, 1);
				half3 tangentWS = half3(0, 0, 0);
				half3 bitangentWS = half3(0, 0, 0);
				return NormalsRenderingShared(mainTex, normalTS, tangentWS, bitangentWS, i.normalWS);
			}
			ENDHLSL
		}

		Pass
		{
			Name "Unlit"
			Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma prefer_hlslcc gles
			#pragma vertex UnlitVertex
			#pragma fragment UnlitFragment

			#include "Include/Spine-SkeletonLit-UnlitPass-URP-2D.hlsl"
			ENDHLSL
		}
	}
	FallBack "Universal Render Pipeline/2D/Sprite-Lit-Default"
}
