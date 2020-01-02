Shader "Universal Render Pipeline/2D/Spine/Skeleton Lit" {
	Properties {
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[NoScaleOffset] _MaskTex("Mask", 2D) = "white" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
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
				o.color = v.color;
				return o;
			}

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

			half4 CombinedShapeLightFragment(Varyings i) : SV_Target
			{
				half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				half4 main;
				#if defined(_STRAIGHT_ALPHA_INPUT)
				main.rgb = tex.rgb * i.color.rgb * tex.a;
				#else
				main.rgb = tex.rgb * i.color.rgb;
				#endif
				main.a = tex.a * i.color.a;

				half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
				return CombinedShapeLightShared(main, mask, i.lightingUV);
			}

			ENDHLSL
	 	}

		Pass
		{
			Tags { "LightMode" = "NormalsRendering"}

			Blend SrcAlpha OneMinusSrcAlpha

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
				float3  normalVS		: TEXCOORD1;
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			Varyings NormalsRenderingVertex(Attributes attributes)
			{
				Varyings o = (Varyings)0;

				o.positionCS = TransformObjectToHClip(attributes.positionOS);
				o.uv = attributes.uv;
				o.color = attributes.color;
				float3 normalWS = TransformObjectToWorldDir(float3(0, 0, -1));
				o.normalVS = TransformWorldToViewDir(normalWS);
				return o;
			}

			#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

			float4 NormalsRenderingFragment(Varyings i) : SV_Target
			{
				float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				float4 normalColor;
				normalColor.rgb = 0.5 * ((i.normalVS)+1);
				normalColor.a = mainTex.a;
				return normalColor;
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
			#pragma prefer_hlslcc gles
			#pragma vertex UnlitVertex
			#pragma fragment UnlitFragment

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
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			float4 _MainTex_ST;

			Varyings UnlitVertex(Attributes attributes)
			{
				Varyings o = (Varyings)0;

				o.positionCS = TransformObjectToHClip(attributes.positionOS);
				o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
				o.uv = attributes.uv;
				o.color = attributes.color;
				return o;
			}

			float4 UnlitFragment(Varyings i) : SV_Target
			{
				half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				half4 main;
				#if defined(_STRAIGHT_ALPHA_INPUT)
				main.rgb = tex.rgb * i.color.rgb * tex.a;
				#else
				main.rgb = tex.rgb * i.color.rgb;
				#endif
				main.a = tex.a * i.color.a;

				return main;
			}
			ENDHLSL
		}
	}
	FallBack "Universal Render Pipeline/2D/Sprite-Lit-Default"
}
