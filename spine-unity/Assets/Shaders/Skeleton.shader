Shader "Skeleton" {
    Properties {
        _MainTex ("Texture to blend", 2D) = "black" {}
    }
    SubShader {
        Tags { "Queue" = "Transparent" }
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
        Pass {
        	ColorMaterial AmbientAndDiffuse
            SetTexture [_MainTex] {
            	combine texture * primary
            }
        }
    }
}