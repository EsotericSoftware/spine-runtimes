// Outline shader variant of "Spine/Skeleton Lit ZWrite"

Shader "Spine/Outline/Skeleton Lit ZWrite" {
	Properties {
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.1
		_ShadowAlphaCutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_DOUBLE_SIDED_LIGHTING)] _DoubleSidedLighting("Double-Sided Lighting", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		UsePass "Spine/Outline/Skeleton/OUTLINE"

		UsePass "Spine/Skeleton Lit ZWrite/NORMAL"

		UsePass "Spine/Skeleton Lit ZWrite/CASTER"
	}
	FallBack "Spine/Skeleton Lit ZWrite"
	CustomEditor "SpineShaderWithOutlineGUI"
}
