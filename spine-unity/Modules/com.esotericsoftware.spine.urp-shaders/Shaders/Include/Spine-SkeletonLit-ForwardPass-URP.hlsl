#ifndef SKELETONLIT_FORWARD_PASS_URP_INCLUDED
#define SKELETONLIT_FORWARD_PASS_URP_INCLUDED

struct appdata {
	float3 pos : POSITION;
	float3 normal : NORMAL;
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

half3 LightweightLightVertexSimplified(float3 positionWS, half3 normalWS) {
	Light mainLight = GetMainLight();

	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	half3 diffuseLightColor = LightingLambert(attenuatedLightColor, mainLight.direction, normalWS);

	// Note: we don't add any lighting in the fragment shader, thus we include both variants below
#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
	for (int i = 0; i < GetAdditionalLightsCount(); ++i)
	{
		Light light = GetAdditionalLight(i, positionWS);
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		diffuseLightColor += LightingLambert(attenuatedLightColor, light.direction, normalWS);
	}
#endif
	return diffuseLightColor;
}

VertexOutput vert(appdata v) {
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	half4 color = v.color;
	float3 positionWS = TransformObjectToWorld(v.pos);
	half3 fixedNormal = half3(0, 0, -1);
	half3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, fixedNormal));
	color.rgb = LightweightLightVertexSimplified(positionWS, normalWS);

	// Note: ambient light is also handled via SH.
	half3 vertexSH;
	OUTPUT_SH(normalWS.xyz, vertexSH);
	color.rgb += SAMPLE_GI(input.lightmapUV, vertexSH, normalWS);

	o.color = color;
	o.uv0 = v.uv0;
	o.pos = TransformWorldToHClip(positionWS);
	return o;
}

sampler2D _MainTex;

half4 frag(VertexOutput i) : SV_Target{
	half4 tex = tex2D(_MainTex, i.uv0);
	half4 col;

	#if defined(_STRAIGHT_ALPHA_INPUT)
	col.rgb = tex.rgb * i.color.rgb * tex.a;
	#else
	col.rgb = tex.rgb * i.color.rgb;
	#endif

	col.a = tex.a * i.color.a;
	return col;
}

#endif
