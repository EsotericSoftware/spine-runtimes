#ifndef SKELETONLIT_FORWARD_PASS_URP_INCLUDED
#define SKELETONLIT_FORWARD_PASS_URP_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "SpineCoreShaders/Spine-Common.cginc"

#if (defined(_MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)) && !defined(_RECEIVE_SHADOWS_OFF)
#define SKELETONLIT_RECEIVE_SHADOWS
#endif

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

#if defined(SKELETONLIT_RECEIVE_SHADOWS)
	float4 shadowCoord : TEXCOORD1;
	half3 shadowedColor : TEXCOORD2;
#endif

	UNITY_VERTEX_OUTPUT_STEREO
};

half3 LightweightLightVertexSimplified(float3 positionWS, half3 normalWS, out half3 shadowedColor) {
	Light mainLight = GetMainLight();
	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	half3 mainLightColor = LightingLambert(attenuatedLightColor, mainLight.direction, normalWS);

	half3 additionalLightColor = half3(0, 0, 0);
	// Note: we don't add any lighting in the fragment shader, thus we include both variants below
#if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
	for (int i = 0; i < GetAdditionalLightsCount(); ++i)
	{
		Light light = GetAdditionalLight(i, positionWS);
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		additionalLightColor += LightingLambert(attenuatedLightColor, light.direction, normalWS);
	}
#endif
	shadowedColor = additionalLightColor;
	return mainLightColor + additionalLightColor;
}

VertexOutput vert(appdata v) {
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	half4 color = PMAGammaToTargetSpace(v.color);
	float3 positionWS = TransformObjectToWorld(v.pos);
	half3 fixedNormal = half3(0, 0, -1);
	half3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, fixedNormal));
	o.uv0 = v.uv0;
	o.pos = TransformWorldToHClip(positionWS);

#ifdef _DOUBLE_SIDED_LIGHTING
	// unfortunately we have to compute the sign here in the vertex shader
	// instead of using VFACE in fragment shader stage.
	half3 viewDirWS = UNITY_MATRIX_V[2].xyz;
	half faceSign = sign(dot(viewDirWS, normalWS));
	normalWS *= faceSign;
#endif

	half3 shadowedColor;
#if !defined(_LIGHT_AFFECTS_ADDITIVE)
	if (color.a == 0) {
		o.color = color;
#if defined(SKELETONLIT_RECEIVE_SHADOWS)
		o.shadowedColor = color;
		o.shadowCoord = float4(0, 0, 0, 0);
#endif
		return o;
	}
#endif // !defined(_LIGHT_AFFECTS_ADDITIVE)

	color.rgb *= LightweightLightVertexSimplified(positionWS, normalWS, shadowedColor);
#if defined(SKELETONLIT_RECEIVE_SHADOWS)
	o.shadowedColor = shadowedColor;
#endif

	// Note: ambient light is also handled via SH.
	half3 vertexSH;
	OUTPUT_SH(normalWS.xyz, vertexSH);
	color.rgb += SAMPLE_GI(input.lightmapUV, vertexSH, normalWS);
	o.color = color;

#if defined(SKELETONLIT_RECEIVE_SHADOWS)
	VertexPositionInputs vertexInput;
	vertexInput.positionWS = positionWS;
	vertexInput.positionCS = o.pos;
	o.shadowCoord = GetShadowCoord(vertexInput);
#endif
	return o;
}

half4 frag(VertexOutput i) : SV_Target{
	half4 tex = tex2D(_MainTex, i.uv0);
	#if defined(_STRAIGHT_ALPHA_INPUT)
	tex.rgb *= tex.a;
	#endif

	if (i.color.a == 0)
		return tex * i.color;

#if defined(SKELETONLIT_RECEIVE_SHADOWS)
	half shadowAttenuation = MainLightRealtimeShadow(i.shadowCoord);
	i.color.rgb = lerp(i.shadowedColor, i.color.rgb, shadowAttenuation);
#endif
	return tex * i.color;
}

#endif
