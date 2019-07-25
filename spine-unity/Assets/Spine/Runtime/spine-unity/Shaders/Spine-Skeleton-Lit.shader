// - Vertex Lit + ShadowCaster
// - Premultiplied Alpha Blending (Optional straight alpha input)
// - Double-sided, no depth

Shader "Spine/Skeleton Lit" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector] [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
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
			Tags { "LightMode"="Vertex" "Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent" }

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

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
			
			#define SHADOW_CUTOFF _Cutoff
			#include "CGIncludes/Spine-Skeleton-Lit-Common-Shadow.cginc"

			ENDCG
		}
	}
}