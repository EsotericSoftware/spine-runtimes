#ifndef SKELETONLIT_UNLIT_PASS_INCLUDED
#define SKELETONLIT_UNLIT_PASS_INCLUDED

struct Attributes
{
	float3 positionOS   : POSITION;
	float4 color		: COLOR;
	float2 uv			: TEXCOORD0;
};

struct Varyings
{
	float4  positionCS		: SV_POSITION;
	float4  color			: COLOR;
	float2	uv				: TEXCOORD0;
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
float4 _MainTex_ST;

Varyings UnlitVertex(Attributes attributes)
{
	Varyings o = (Varyings)0;

	o.positionCS = TransformObjectToHClip(attributes.positionOS);
	o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
	o.uv = attributes.uv;
	o.color = attributes.color;
	return o;
}

float4 UnlitFragment(Varyings i) : SV_Target
{
	half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
	half4 main;
	#if defined(_STRAIGHT_ALPHA_INPUT)
	main.rgb = tex.rgb * i.color.rgb * tex.a;
	#else
	main.rgb = tex.rgb * i.color.rgb;
	#endif
	main.a = tex.a * i.color.a;

	return main;
}

#endif
