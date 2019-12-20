#ifndef SKELETON_FORWARD_PASS_URP_INCLUDED
#define SKELETON_FORWARD_PASS_URP_INCLUDED

struct appdata {
	float3 pos : POSITION;
	half4 color : COLOR;
	float2 uv0 : TEXCOORD0;
	
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	half4 color : COLOR0;
	float2 uv0 : TEXCOORD0;
	float4 pos : SV_POSITION;
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(appdata v) {
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float3 positionWS = TransformObjectToWorld(v.pos);
	o.pos = TransformWorldToHClip(positionWS);
	o.uv0 = v.uv0;
	o.color = v.color;
	return o;
}

sampler2D _MainTex;

half4 frag(VertexOutput i) : SV_Target{
	float4 texColor = tex2D(_MainTex, i.uv0);

	#if defined(_STRAIGHT_ALPHA_INPUT)
	texColor.rgb *= texColor.a;
	#endif

	return (texColor * i.color);
}

#endif
