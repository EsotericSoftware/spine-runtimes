#ifndef SKELETON_LIT_COMMON_INCLUDED
#define SKELETON_LIT_COMMON_INCLUDED

#include "UnityCG.cginc"
#include "CGIncludes/Spine-Common.cginc"

// ES2.0/WebGL/3DS can not do loops with non-constant-expression iteration counts :(
#if defined(SHADER_API_GLES)
#define LIGHT_LOOP_LIMIT 8
#elif defined(SHADER_API_N3DS)
#define LIGHT_LOOP_LIMIT 4
#else
#define LIGHT_LOOP_LIMIT unity_VertexLightParams.x
#endif


////////////////////////////////////////
// Alpha Clipping
//

#if defined(_ALPHA_CLIP)
	uniform fixed _Cutoff;
	#define ALPHA_CLIP(pixel, color) clip((pixel.a * color.a) - _Cutoff);
#else
	#define ALPHA_CLIP(pixel, color)
#endif

half3 computeLighting (int idx, half3 dirToLight, half3 eyeNormal, half4 diffuseColor, half atten) {
	half NdotL = max(dot(eyeNormal, dirToLight), 0.0);
	// diffuse
	half3 color = NdotL * diffuseColor.rgb * unity_LightColor[idx].rgb;
	return color * atten;
}

half3 computeOneLight (int idx, float3 eyePosition, half3 eyeNormal, half4 diffuseColor) {
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
	return min (computeLighting (idx, dirToLight, eyeNormal, diffuseColor, att), 1.0);
}

int4 unity_VertexLightParams; // x: light count, y: zero, z: one (y/z needed by d3d9 vs loop instruction)

struct appdata {
	float3 pos : POSITION;
	float3 normal : NORMAL;
	half4 color : COLOR;
	float2 uv0 : TEXCOORD0;
#if defined(_TINT_BLACK_ON)
	float2 tintBlackRG : TEXCOORD1;
	float2 tintBlackB : TEXCOORD2;
#endif
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

	half4 color = PMAGammaToTargetSpace(v.color);
	float3 eyePos = UnityObjectToViewPos(float4(v.pos, 1)).xyz; //mul(UNITY_MATRIX_MV, float4(v.pos,1)).xyz;
	half3 fixedNormal = half3(0,0,-1);
	half3 eyeNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, fixedNormal));
	o.uv0 = v.uv0;
	o.pos = UnityObjectToClipPos(v.pos);

#ifdef _DOUBLE_SIDED_LIGHTING
	// unfortunately we have to compute the sign here in the vertex shader
	// instead of using VFACE in fragment shader stage.
	half faceSign = sign(eyeNormal.z);
	eyeNormal *= faceSign;
#endif

	half3 shadowedColor;
#if !defined(_LIGHT_AFFECTS_ADDITIVE)
	if (color.a == 0) {
		o.color = color;
		return o;
	}
#endif // !defined(_LIGHT_AFFECTS_ADDITIVE)

	// Lights
	half3 lcolor = half4(0,0,0,1).rgb + color.rgb * glstate_lightmodel_ambient.rgb;
	for (int il = 0; il < LIGHT_LOOP_LIMIT; ++il) {
		lcolor += computeOneLight(il, eyePos, eyeNormal, color);
	}

	color.rgb = lcolor.rgb;
	o.color = saturate(color);
	return o;
}

sampler2D _MainTex;

fixed4 frag (VertexOutput i) : SV_Target {
	fixed4 tex = tex2D(_MainTex, i.uv0);
	ALPHA_CLIP(tex, i.color);
#if defined(_STRAIGHT_ALPHA_INPUT)
	tex.rgb *= tex.a;
#endif
	fixed4 col = tex * i.color;
	col.rgb *= 2;
	return col;
}

#endif
