Shader "Spine/SkeletonScreenBlend" {
	Properties{
		_Cutoff("Shadow alpha cutoff", Range(0,1)) = 0.1
		_MainTex("Texture to blend", 2D) = "black" {}
		_Color("Main Color", Color) = (1,1,1,1)
	}
		// 2 texture stage GPUs
		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		fixed4 color : COLOR;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		fixed4 color : COLOR;
		UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	uniform fixed _Cutoff;
	fixed4 _Color;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.color = v.color;
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 col = tex2D(_MainTex, i.uv);
		fixed4 vcolor = i.color;
		fixed4 base = col * vcolor;
		fixed ca = _Color.a;
		clip(col.a - _Cutoff);
		fixed4 color = _Color * ca;


		fixed4 ret = base + color - (base * color);
		ret.a = base.a;

	// apply fog
	UNITY_APPLY_FOG(i.fogCoord, ret);
	return ret;
	}
		ENDCG



	}

		Pass{
		Name "Caster"
		Tags{ "LightMode" = "ShadowCaster" }
		Offset 1, 1

		Fog{ Mode Off }
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

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
	};

	uniform float4 _MainTex_ST;

	v2f vert(appdata v) {
		v2f o;
		TRANSFER_SHADOW_CASTER(o)
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	uniform sampler2D _MainTex;
	uniform fixed _Cutoff;

	float4 frag(v2f i) : COLOR{
		fixed4 texcol = tex2D(_MainTex, i.uv);
	clip(texcol.a - _Cutoff);
	SHADOW_CASTER_FRAGMENT(i)
	}
		ENDCG
	}

	}
		// 1 texture stage GPUs
		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass{
		ColorMaterial AmbientAndDiffuse
		SetTexture[_MainTex]{
		Combine texture * primary DOUBLE, texture * primary
	}
	}
	}
}