// - Unlit + no shadow
// - Premultiplied Alpha Blending (Optional straight alpha input)
// - Double-sided, no depth

Shader "Spine/Skeleton Fill" {
	Properties {
		_FillColor ("FillColor", Color) = (1,1,1,1)
		_FillPhase ("FillPhase", Range(0, 1)) = 0
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "white" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		Blend One OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		Lighting Off

		Pass {
			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			float4 _FillColor;
			float _FillPhase;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			VertexOutput vert (VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.uv = v.uv;
				o.vertexColor = v.vertexColor;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			float4 frag (VertexOutput i) : COLOR {
				float4 rawColor = tex2D(_MainTex,i.uv);
				float finalAlpha = (rawColor.a * i.vertexColor.a);

				#if defined(_STRAIGHT_ALPHA_INPUT)
				rawColor.rgb *= rawColor.a;
				#endif

				float3 finalColor = lerp((rawColor.rgb * i.vertexColor.rgb), (_FillColor.rgb * finalAlpha), _FillPhase); // make sure to PMA _FillColor.
				return fixed4(finalColor, finalAlpha);
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}