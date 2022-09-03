// Outline shader variant of "Spine/Sprite/Vertex Lit"

Shader "Spine/Outline/Sprite/Vertex Lit"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)

		_BumpScale("Scale", Float) = 1.0
		_BumpMap ("Normal Map", 2D) = "bump" {}

		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

		_EmissionColor("Color", Color) = (0,0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		_EmissionPower("Emission Power", Float) = 2.0

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_DiffuseRamp ("Diffuse Ramp Texture", 2D) = "gray" {}

		_FixedNormal ("Fixed Normal", Vector) = (0,0,1,1)
		_ZWrite ("Depth Write", Float) = 0.0
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.0
		_ShadowAlphaCutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		_CustomRenderQueue ("Custom Render Queue", Float) = 0.0

		_OverlayColor ("Overlay Color", Color) = (0,0,0,0)
		_Hue("Hue", Range(-0.5,0.5)) = 0.0
		_Saturation("Saturation", Range(0,2)) = 1.0
		_Brightness("Brightness", Range(0,2)) = 1.0

		_RimPower("Rim Power", Float) = 2.0
		_RimColor ("Rim Color", Color) = (1,1,1,1)

		_BlendTex ("Blend Texture", 2D) = "white" {}
		_BlendAmount ("Blend", Range(0,1)) = 0.0

		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _RenderQueue ("__queue", Float) = 0.0
		[HideInInspector] _Cull ("__cull", Float) = 0.0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Sprite" "AlphaDepth"="False" "CanUseSpriteAtlas"="True" "IgnoreProjector"="True" }
		LOD 150

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		UsePass "Spine/Outline/Skeleton/OUTLINE"

		UsePass "Spine/Sprite/Vertex Lit/VERTEX"

		UsePass "Spine/Sprite/Vertex Lit/SHADOWCASTER"
	}

	FallBack "Spine/Sprite/Vertex Lit"
	CustomEditor "SpineSpriteShaderGUI"
}
