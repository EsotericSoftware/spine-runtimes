Shader "Spine/Skeleton" {
    Properties {
        _MainTex ("Texture to blend", 2D) = "black" {}
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

        Pass {
        	ColorMaterial AmbientAndDiffuse
            SetTexture [_MainTex] {
            	Combine texture * primary
            }
        }
    }
}