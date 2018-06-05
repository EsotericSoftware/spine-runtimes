#ifndef SPRITE_LIGHTING_INCLUDED
#define SPRITE_LIGHTING_INCLUDED

//Check for using mesh normals
#if !defined(_FIXED_NORMALS) && !defined(_FIXED_NORMALS_BACK_RENDERING)
#define MESH_NORMALS
#endif // _FIXED_NORMALS || _FIXED_NORMALS_BACK_RENDERING

////////////////////////////////////////
// Vertex structs
//

struct VertexInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
	float4 color : COLOR;
#if defined(MESH_NORMALS)
	float3 normal : NORMAL;
#endif // MESH_NORMALS
#if defined(_NORMALMAP)
	float4 tangent : TANGENT;
#endif // _NORMALMAP

};

////////////////////////////////////////
// Normal functions
//

//Fixed Normal defined in view space
uniform float4 _FixedNormal = float4(0, 0, -1, 1);

inline half3 calculateSpriteWorldNormal(VertexInput vertex)
{
#if defined(MESH_NORMALS)
	return calculateWorldNormal(vertex.normal);
#else //MESH_NORMALS
	//Rotate fixed normal by inverse camera matrix to convert the fixed normal into world space
	float3x3 invView = transpose((float3x3)UNITY_MATRIX_VP);
	float3 normal = _FixedNormal.xyz;
#if UNITY_REVERSED_Z
	normal.z = -normal.z;
#endif
	return normalize(mul(invView, normal));
#endif // !MESH_NORMALS
}

inline half3 calculateSpriteViewNormal(VertexInput vertex)
{
#if defined(MESH_NORMALS)
	return normalize(mul((float3x3)UNITY_MATRIX_IT_MV, vertex.normal));
#else // !MESH_NORMALS
	float3 normal = _FixedNormal.xyz;
#if UNITY_REVERSED_Z
	normal.z = -normal.z;
#endif
	return normal;
#endif // !MESH_NORMALS
}

////////////////////////////////////////
// Normal map functions
//

#if defined(_NORMALMAP)

inline half3 calculateSpriteWorldBinormal(half3 normalWorld, half3 tangentWorld, float tangentW)
{
#if defined(_FIXED_NORMALS_BACK_RENDERING)
	//If we're using fixed normals and sprite is facing away from camera, flip tangentW
	float3 zAxis = float3(0.0, 0.0, 1.0);
	float3 modelForward = mul((float3x3)unity_ObjectToWorld, zAxis);
	float3 cameraForward = mul((float3x3)UNITY_MATRIX_VP, zAxis);
	float directionDot = dot(modelForward, cameraForward);
	//Don't worry if directionDot is zero, sprite will be side on to camera so invisible meaning it doesnt matter that tangentW will be zero too 
	tangentW *= sign(directionDot);
#endif // _FIXED_NORMALS_BACK_RENDERING

	return calculateWorldBinormal(normalWorld, tangentWorld, tangentW);
}

#endif // _NORMALMAP

#if defined(_DIFFUSE_RAMP)


////////////////////////////////////////
// Diffuse ramp functions
//

//Disable for softer, more traditional diffuse ramping
#define HARD_DIFFUSE_RAMP

uniform sampler2D _DiffuseRamp;

inline fixed3 calculateDiffuseRamp(float ramp)
{
	return tex2D(_DiffuseRamp, float2(ramp, ramp)).rgb;
}

inline fixed3 calculateRampedDiffuse(fixed3 lightColor, float attenuation, float angleDot)
{
	float d = angleDot * 0.5 + 0.5;
#if defined(HARD_DIFFUSE_RAMP)
	half3 ramp = calculateDiffuseRamp(d * attenuation * 2);
	return lightColor * ramp;
#else
	half3 ramp = calculateDiffuseRamp(d);
	return lightColor * ramp * (attenuation * 2);
#endif
}
#endif // _DIFFUSE_RAMP

////////////////////////////////////////
// Rim Lighting functions
//

#ifdef _RIM_LIGHTING

uniform float _RimPower;
uniform fixed4 _RimColor;

inline fixed3 applyRimLighting(fixed3 posWorld, fixed3 normalWorld, fixed4 pixel) : SV_Target
{
	fixed3 viewDir = normalize(_WorldSpaceCameraPos - posWorld);
	float invDot =  1.0 - saturate(dot(normalWorld, viewDir));
	float rimPower = pow(invDot, _RimPower);
	float rim = saturate(rimPower * _RimColor.a);
	
#if defined(_DIFFUSE_RAMP)
	rim = calculateDiffuseRamp(rim).r;
#endif
	
	return lerp(pixel.rgb, _RimColor.xyz * pixel.a, rim);
}

#endif  //_RIM_LIGHTING

////////////////////////////////////////
// Emission functions
//

#ifdef _EMISSION

uniform sampler2D _EmissionMap;
uniform fixed4 _EmissionColor;
uniform float _EmissionPower;


#define APPLY_EMISSION(diffuse, uv) \
	{ \
		diffuse += tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _EmissionPower; \
	}

#else //!_EMISSION

#define APPLY_EMISSION(diffuse, uv)

#endif  //!_EMISSION

#endif // SPRITE_LIGHTING_INCLUDED