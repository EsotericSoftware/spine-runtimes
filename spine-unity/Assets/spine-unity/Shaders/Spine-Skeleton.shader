Shader "Spine/Skeleton" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass {
			Fog { Mode Off }
			ColorMaterial AmbientAndDiffuse
			SetTexture [_MainTex] {
				Combine texture * primary
			}
		}

		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1
			ZWrite On
			ZTest LEqual

			Fog { Mode Off }
			Cull Off
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			fixed _Cutoff;

			struct v2f { 
				V2F_SHADOW_CASTER;
				float2  uv : TEXCOORD1;
			};

			v2f vert (appdata_base v) {
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				o.uv = v.texcoord;
				return o;
			}

			float4 frag (v2f i) : COLOR {
				fixed4 texcol = tex2D(_MainTex, i.uv);
				clip(texcol.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass {
			ColorMaterial AmbientAndDiffuse
			SetTexture [_MainTex] {
				Combine texture * primary DOUBLE, texture * primary
			}
		}
	}
}
