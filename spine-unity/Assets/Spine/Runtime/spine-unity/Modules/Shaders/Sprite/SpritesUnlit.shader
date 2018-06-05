Shader "Spine/Sprite/Unlit"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		
		_ZWrite ("Depth Write", Float) = 0.0
		_Cutoff ("Depth alpha cutoff", Range(0,1)) = 0.0
		_ShadowAlphaCutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		_CustomRenderQueue ("Custom Render Queue", Float) = 0.0
		
		_OverlayColor ("Overlay Color", Color) = (0,0,0,0)
		_Hue("Hue", Range(-0.5,0.5)) = 0.0
		_Saturation("Saturation", Range(0,2)) = 1.0	
		_Brightness("Brightness", Range(0,2)) = 1.0	
		
		_BlendTex ("Blend Texture", 2D) = "white" {}
		_BlendAmount ("Blend", Range(0,1)) = 0.0
		
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _RenderQueue ("__queue", Float) = 0.0
		[HideInInspector] _Cull ("__cull", Float) = 0.0
	}
	
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Sprite" "AlphaDepth"="False" "CanUseSpriteAtlas"="True" "IgnoreProjector"="True" }
		LOD 100
		
		Pass
		{
			Blend [_SrcBlend] [_DstBlend]
			Lighting Off
			ZWrite [_ZWrite]
			ZTest LEqual
			Cull [_Cull]
			Lighting Off
			
			CGPROGRAM			
				#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ADDITIVEBLEND _ADDITIVEBLEND_SOFT _MULTIPLYBLEND _MULTIPLYBLEND_X2
				#pragma shader_feature _ALPHA_CLIP
				#pragma shader_feature _TEXTURE_BLEND
				#pragma shader_feature _COLOR_ADJUST
				#pragma shader_feature _FOG
				
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				
				#pragma vertex vert
				#pragma fragment frag
				
				#include "CGIncludes/SpriteUnlit.cginc"
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
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_shadowcaster
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				
				#pragma vertex vert
				#pragma fragment frag
				
				#include "CGIncludes/SpriteShadows.cginc"
			ENDCG
		}
	}
	
	CustomEditor "SpineSpriteShaderGUI"
}
