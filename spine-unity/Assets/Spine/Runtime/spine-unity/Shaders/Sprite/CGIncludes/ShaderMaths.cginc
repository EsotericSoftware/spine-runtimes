#ifndef SHADER_MATHS_INCLUDED
#define SHADER_MATHS_INCLUDED

#if defined(USE_LWRP)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#elif defined(USE_URP)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
#include "UnityCG.cginc"
#endif

////////////////////////////////////////
// Maths functions
//

inline half3 safeNormalize(half3 inVec)
{
	half dp3 = max(0.001f, dot(inVec, inVec));
	return inVec * rsqrt(dp3);
}

inline float dotClamped(float3 a, float3 b)
{
	#if (SHADER_TARGET < 30 || defined(SHADER_API_PS3))
		return saturate(dot(a, b));
	#else
		return max(0.0h, dot(a, b));
	#endif
}

inline float oneDividedBy(float value)
{
	//Catches NANs
	float sign_value = sign(value);
	float sign_value_squared = sign_value*sign_value;
	return sign_value_squared / ( value + sign_value_squared - 1.0);
}

inline half pow5 (half x)
{
	return x*x*x*x*x;
}

inline float4 quat_from_axis_angle(float3 axis, float angleRadians)
{
  float4 qr;
  float half_angle = (angleRadians * 0.5);
  qr.x = axis.x * sin(half_angle);
  qr.y = axis.y * sin(half_angle);
  qr.z = axis.z * sin(half_angle);
  qr.w = cos(half_angle);
  return qr;
}

inline float3 rotate_vertex_position(float3 position, float3 axis, float angleRadians)
{
  float4 q = quat_from_axis_angle(axis, angleRadians);
  float3 v = position.xyz;
  return v + 2.0 * cross(q.xyz, cross(q.xyz, v) + q.w * v);
}

float3 EncodeFloatRGB(float value)
{
   const float max24int = 256*256*256-1;
   float3 decomp = floor( value * float3( max24int/(256*256), max24int/256, max24int ) ) / 255.0;
   decomp.z -= decomp.y * 256.0;
   decomp.y -= decomp.x * 256.0;
   return decomp;
}

float DecodeFloatRGB(float3 decomp)
{
   return dot( decomp.xyz, float3( 255.0/256, 255.0/(256*256), 255.0/(256*256*256) ) );
}

#endif // SHADER_MATHS_INCLUDED
