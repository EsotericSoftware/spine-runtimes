Shader "Spine/Sprite/Vertex Lit"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_BumpMap ("Normal Map", 2D) = "bump" {}
		
		_EmissionColor("Color", Color) = (0,0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}
		_EmissionPower("Emission Power", Float) = 2.0	
		
		_DiffuseRamp ("Diffuse Ramp Texture", 2D) = "gray" {}
		
		_FixedNormal ("Fixed Normal", Vector) = (0,0,-1,1)
		_ZWrite ("Depth Write", Float) = 0.0
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.0
		_ShadowAlphaCutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		
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
	}
	
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Sprite" }
		LOD 150
		
		Pass
		{
			Name "Vertex" 
			Tags { "LightMode" = "Vertex" }
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			ZTest LEqual
			Cull [_Cull]
			Lighting On
			
			CGPROGRAM
				#pragma target 3.0
				
				#pragma shader_feature _ _ALPHAPREMULTIPLY_ON _ADDITIVEBLEND _ADDITIVEBLEND_SOFT _MULTIPLYBLEND _MULTIPLYBLEND_X2
				#pragma shader_feature _NORMALMAP
				#pragma shader_feature _ _FIXED_NORMALS _FIXED_NORMALS_BACK_RENDERING
				#pragma shader_feature _ALPHA_CLIP
				#pragma shader_feature _EMISSION
				#pragma shader_feature _DIFFUSE_RAMP
				#pragma shader_feature _COLOR_ADJUST
				#pragma shader_feature _RIM_LIGHTING
				#pragma shader_feature _TEXTURE_BLEND
				#pragma shader_feature _SPHERICAL_HARMONICS
				#pragma shader_feature _FOG
				
                #pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				
				#pragma vertex vert
				#pragma fragment frag
				
				#include "SpriteVertexLighting.cginc"	
			ENDCG
		}
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1
			
			Fog { Mode Off }
			ZWrite On
			ZTest LEqual
			Cull Off
			Lighting Off
			
			CGPROGRAM		
				#pragma multi_compile_shadowcaster
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#pragma vertex vert
				#pragma fragment frag
				
				#include "SpriteShadows.cginc"
			ENDCG
		}
	}
	
	FallBack "Spine/Sprite/Unlit"
	CustomEditor "SpineSpriteShaderGUI"
}
