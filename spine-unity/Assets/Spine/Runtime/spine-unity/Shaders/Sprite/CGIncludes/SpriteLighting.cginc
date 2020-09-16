#ifndef SPRITE_LIGHTING_INCLUDED
#define SPRITE_LIGHTING_INCLUDED

//Check for using mesh normals
#if !defined(_FIXED_NORMALS_VIEWSPACE) && !defined(_FIXED_NORMALS_VIEWSPACE_BACKFACE) && !defined(_FIXED_NORMALS_MODELSPACE) && !defined(_FIXED_NORMALS_MODELSPACE_BACKFACE) && !defined(_FIXED_NORMALS_WORLDSPACE)
#define MESH_NORMALS
#endif

//Check for fixing backfacing tangents
#if defined(_FIXED_NORMALS_VIEWSPACE_BACKFACE) || defined(_FIXED_NORMALS_MODELSPACE_BACKFACE)
#define FIXED_NORMALS_BACKFACE_RENDERING
#endif

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
#endif // _FIXED_NORMALS
#if defined(_NORMALMAP)
	float4 tangent : TANGENT;
#endif // _NORMALMAP
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

////////////////////////////////////////
// Normal functions
//

uniform float4 _FixedNormal = float4(0, 0, 1, 1);

inline float3 getFixedNormal()
{
	return _FixedNormal.xyz;
}

inline float calculateBackfacingSign(float3 worldPos)
{
	//If we're using fixed normals and mesh is facing away from camera, flip tangentSign
	//Unity uses a left handed coordinate system so camera always looks down the negative z axis
	float3 cameraForward = float3(0,0,-1);
	float3 meshWorldForward = mul((float3x3)unity_ObjectToWorld, cameraForward);
	float3 toCamera = _WorldSpaceCameraPos - worldPos;
	return sign(dot(toCamera, meshWorldForward));
}

inline half3 calculateSpriteWorldNormal(VertexInput vertex, float backFaceSign)
{
#if defined(MESH_NORMALS)

	return calculateWorldNormal(vertex.normal);

#else // !MESH_NORMALS

	float3 normal = getFixedNormal();

#if defined(_FIXED_NORMALS_VIEWSPACE) || defined(_FIXED_NORMALS_VIEWSPACE_BACKFACE)
	//View space fixed normal
	//Rotate fixed normal by inverse view matrix to convert the fixed normal into world space
	float3x3 invView = transpose((float3x3)UNITY_MATRIX_V);
	return normalize(mul(invView, normal));
#elif defined (_FIXED_NORMALS_WORLDSPACE)
	//World space fixed normal
	return normal;
#else
	//Model space fixed normal.
#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	//If back face rendering is enabled and the sprite is facing away from the camera (ie we're rendering the backface) then need to flip the normal
	normal *= backFaceSign;
#endif
	return calculateWorldNormal(normal);
#endif

#endif // !MESH_NORMALS
}

inline half3 calculateSpriteViewNormal(VertexInput vertex, float backFaceSign)
{
#if defined(MESH_NORMALS)

	return normalize(mul((float3x3)UNITY_MATRIX_IT_MV, vertex.normal));

#else // !MESH_NORMALS

	float3 normal = getFixedNormal();

#if defined(_FIXED_NORMALS_VIEWSPACE) || defined(_FIXED_NORMALS_VIEWSPACE_BACKFACE)
	//View space fixed normal
	return normal;
#elif defined (_FIXED_NORMALS_WORLDSPACE)
	//World space fixed normal
	return normalize(mul((float3x3)UNITY_MATRIX_V, normal));
#else
	//Model space fixed normal
#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	//If back face rendering is enabled and the sprite is facing away from the camera (ie we're rendering the backface) then need to flip the normal
	normal *= backFaceSign;
#endif
	return normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));
#endif

#endif // !MESH_NORMALS
}

////////////////////////////////////////
// Normal map functions
//

#if defined(_NORMALMAP)

inline half3 calculateSpriteWorldBinormal(VertexInput vertex, half3 normalWorld, half3 tangentWorld, float backFaceSign)
{
	float tangentSign = vertex.tangent.w;

#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	tangentSign *= backFaceSign;
#endif

	return calculateWorldBinormal(normalWorld, tangentWorld, tangentSign);
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


#define APPLY_EMISSION(diffuse, uv) diffuse += tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _EmissionPower;
#define APPLY_EMISSION_SPECULAR(pixel, uv) pixel.rgb += (tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb * _EmissionPower) * pixel.a;

#else //!_EMISSION

#define APPLY_EMISSION(diffuse, uv)
#define APPLY_EMISSION_SPECULAR(pixel, uv)

#endif  //!_EMISSION

#endif // SPRITE_LIGHTING_INCLUDED
