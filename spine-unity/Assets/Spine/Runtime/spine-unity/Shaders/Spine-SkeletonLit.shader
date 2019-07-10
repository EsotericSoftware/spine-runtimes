// - Vertex Lit + ShadowCaster
// - Premultiplied Alpha Blending (Optional straight alpha input)
// - Double-sided, no depth

Shader "Spine/Skeleton Lit" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		
		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

//		Pass {
//			Tags { "LightMode"="Vertex" }
//			ColorMaterial AmbientAndDiffuse
//			Lighting On
//			SetTexture [_MainTex] {
//				Combine texture * primary DOUBLE, texture * primary
//			}
//		}

		Pass {
			Tags {
				"LIGHTMODE"="Vertex"
				"QUEUE"="Transparent"
				"IGNOREPROJECTOR"="true"
				"RenderType"="Transparent"
			}

			ZWrite Off
			Cull Off
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			// ES2.0/WebGL/3DS can not do loops with non-constant-expression iteration counts :(
			#if defined(SHADER_API_GLES)
			#define LIGHT_LOOP_LIMIT 8
			#elif defined(SHADER_API_N3DS)
			#define LIGHT_LOOP_LIMIT 4
			#else
			#define LIGHT_LOOP_LIMIT unity_VertexLightParams.x
			#endif

			#pragma multi_compile __ POINT SPOT

			half3 computeLighting (int idx, half3 dirToLight, half3 eyeNormal, half3 viewDir, half4 diffuseColor, half atten) {
				half NdotL = max(dot(eyeNormal, dirToLight), 0.0);
				// diffuse
				half3 color = NdotL * diffuseColor.rgb * unity_LightColor[idx].rgb;
				return color * atten;
			}

			half3 computeOneLight (int idx, float3 eyePosition, half3 eyeNormal, half3 viewDir, half4 diffuseColor) {
				float3 dirToLight = unity_LightPosition[idx].xyz;
				half att = 1.0;

				#if defined(POINT) || defined(SPOT)
				dirToLight -= eyePosition * unity_LightPosition[idx].w;

				// distance attenuation
				float distSqr = dot(dirToLight, dirToLight);
				att /= (1.0 + unity_LightAtten[idx].z * distSqr);
				if (unity_LightPosition[idx].w != 0 && distSqr > unity_LightAtten[idx].w) att = 0.0; // set to 0 if outside of range
				distSqr = max(distSqr, 0.000001); // don't produce NaNs if some vertex position overlaps with the light
				dirToLight *= rsqrt(distSqr);
				#if defined(SPOT)

				// spot angle attenuation
				half rho = max(dot(dirToLight, unity_SpotDirection[idx].xyz), 0.0);
				half spotAtt = (rho - unity_LightAtten[idx].x) * unity_LightAtten[idx].y;
				att *= saturate(spotAtt);
				#endif
				#endif

				att *= 0.5; // passed in light colors are 2x brighter than what used to be in FFP
				return min (computeLighting (idx, dirToLight, eyeNormal, viewDir, diffuseColor, att), 1.0);
			}

			int4 unity_VertexLightParams; // x: light count, y: zero, z: one (y/z needed by d3d9 vs loop instruction)

			struct appdata {
				float3 pos : POSITION;
				float3 normal : NORMAL;
				half4 color : COLOR;
				float3 uv0 : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput {
				fixed4 color : COLOR0;
				float2 uv0 : TEXCOORD0;
				float4 pos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert (appdata v) {
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				half4 color = v.color;
				float3 eyePos = UnityObjectToViewPos(float4(v.pos, 1)).xyz; //mul(UNITY_MATRIX_MV, float4(v.pos,1)).xyz;
				half3 fixedNormal = half3(0,0,-1);
				half3 eyeNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, fixedNormal));
				//half3 eyeNormal = half3(0,0,1);
				half3 viewDir = 0.0;

				// Lights
				half3 lcolor = half4(0,0,0,1).rgb + color.rgb * glstate_lightmodel_ambient.rgb;
				for (int il = 0; il < LIGHT_LOOP_LIMIT; ++il) {
					lcolor += computeOneLight(il, eyePos, eyeNormal, viewDir, color);
				}

				color.rgb = lcolor.rgb;
				o.color = saturate(color);
				o.uv0 = v.uv0;
				o.pos = UnityObjectToClipPos(v.pos);
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag (VertexOutput i) : SV_Target {
				fixed4 tex = tex2D(_MainTex, i.uv0);

				fixed4 col;

				#if defined(_STRAIGHT_ALPHA_INPUT)
				col.rgb = tex * i.color * tex.a;
				#else
				col.rgb = tex * i.color;
				#endif
				
				col *= 2;
				col.a = tex.a * i.color.a;
				return col;
			}
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
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			struct v2f { 
				V2F_SHADOW_CASTER;
				float2  uv : TEXCOORD1;
			};

			uniform float4 _MainTex_ST;

			v2f vert (appdata_base v) {
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			uniform sampler2D _MainTex;
			uniform fixed _Cutoff;

			float4 frag (v2f i) : SV_Target {
				fixed4 texcol = tex2D(_MainTex, i.uv);
				clip(texcol.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}