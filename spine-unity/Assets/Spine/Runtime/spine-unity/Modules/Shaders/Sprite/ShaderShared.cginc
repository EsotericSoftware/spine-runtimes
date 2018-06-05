#ifndef SHADER_SHARED_INCLUDED
#define SHADER_SHARED_INCLUDED

#include "UnityCG.cginc"

////////////////////////////////////////
// Space functions
//

inline float4 calculateWorldPos(float4 vertex)
{
	return mul(unity_ObjectToWorld, vertex);
}

inline float4 calculateLocalPos(float4 vertex)
{
	return UnityObjectToClipPos(vertex);
}

inline half3 calculateWorldNormal(float3 normal)
{
	return UnityObjectToWorldNormal(normal);
}

////////////////////////////////////////
// Maths functions
//

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

////////////////////////////////////////
// Normal map functions
//

#if defined(_NORMALMAP)

uniform sampler2D _BumpMap;

inline half3 calculateWorldTangent(float4 tangent)
{
	return UnityObjectToWorldDir(tangent);
}

inline half3 calculateWorldBinormal(half3 normalWorld, half3 tangentWorld, float tangentW)
{
	// For odd-negative scale transforms we need to flip the binormal
	return cross(normalWorld, tangentWorld.xyz) * tangentW * unity_WorldTransformParams.w;
}

inline half3 calculateNormalFromBumpMap(float2 texUV, half3 tangentWorld, half3 binormalWorld, half3 normalWorld)
{
	half3 localNormal = UnpackNormal(tex2D(_BumpMap, texUV));
	half3x3 rotation = half3x3(tangentWorld, binormalWorld, normalWorld);
	half3 normal = normalize(mul(localNormal, rotation));
	return normal;
}

#endif // _NORMALMAP

////////////////////////////////////////
// Blending functions
//

inline fixed4 calculateLitPixel(fixed4 texureColor, fixed4 color, fixed3 lighting) : SV_Target
{
	fixed4 finalPixel;
	
#if defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel = texureColor * color;
	finalPixel.rgb *= lighting * color.a;
#elif defined(_MULTIPLYBLEND)
	//Multiply
	finalPixel = color * texureColor;
	finalPixel.rgb *= lighting;
	finalPixel = lerp(fixed4(1,1,1,1), finalPixel, finalPixel.a);
#elif defined(_MULTIPLYBLEND_X2)
	//Multiply x2
	finalPixel.rgb = texureColor.rgb * color.rgb * lighting * 2.0f;
	finalPixel.a = color.a * texureColor.a;
	finalPixel = lerp(fixed4(0.5f,0.5f,0.5f,0.5f), finalPixel, finalPixel.a);
#elif defined(_ADDITIVEBLEND)
	//Additive
	finalPixel = texureColor * 2.0f * color;
	finalPixel.rgb *= lighting * color.a;
#elif defined(_ADDITIVEBLEND_SOFT)
	//Additive soft
	finalPixel = texureColor * color;
	finalPixel.rgb *= lighting * finalPixel.a;
#else
	finalPixel.a = texureColor.a * color.a;
	finalPixel.rgb = texureColor.rgb * color.rgb * (lighting * finalPixel.a);
#endif
	
	return finalPixel;
}

inline fixed4 calculateLitPixel(fixed4 texureColor, fixed3 lighting) : SV_Target
{
	fixed4 finalPixel;
	
#if defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel = texureColor;
	finalPixel.rgb *= lighting;
#elif defined(_MULTIPLYBLEND)
	//Multiply
	finalPixel = texureColor;
	finalPixel.rgb *= lighting;
	finalPixel = lerp(fixed4(1,1,1,1), finalPixel, finalPixel.a);
#elif defined(_MULTIPLYBLEND_X2)
	//Multiply x2
	finalPixel.rgb = texureColor.rgb * lighting * 2.0f;
	finalPixel.a = texureColor.a;
	finalPixel = lerp(fixed4(0.5f,0.5f,0.5f,0.5f), finalPixel, finalPixel.a);
#elif defined(_ADDITIVEBLEND)
	//Additive
	finalPixel = texureColor * 2.0f;
	finalPixel.rgb *= lighting;
#elif defined(_ADDITIVEBLEND_SOFT)
	//Additive soft
	finalPixel = texureColor;
	finalPixel.rgb *= lighting * finalPixel.a;
#else
	finalPixel.a = texureColor.a;
	finalPixel.rgb = texureColor.rgb * (lighting * finalPixel.a);
#endif
	
	return finalPixel;
}

inline fixed4 calculateAdditiveLitPixel(fixed4 texureColor, fixed4 color, fixed3 lighting) : SV_Target
{
	fixed4 finalPixel;
	
#if defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel.rgb = texureColor.rgb * lighting * color.rgb * color.a;
	finalPixel.a = 1.0;
#else
	//All other alpha
	finalPixel.rgb = (texureColor.rgb * lighting * color.rgb) * (texureColor.a * color.a);
	finalPixel.a = 1.0;
#endif
	
	return finalPixel;
}

inline fixed4 calculatePixel(fixed4 texureColor, fixed4 color) : SV_Target
{
	fixed4 finalPixel;
	
#if defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel = texureColor * color;
	finalPixel.rgb *= color.a;
#elif defined(_MULTIPLYBLEND)
	//Multiply
	finalPixel = color * texureColor;
	finalPixel = lerp(fixed4(1,1,1,1), finalPixel, finalPixel.a);
#elif defined(_MULTIPLYBLEND_X2)
	//Multiply x2
	finalPixel.rgb = texureColor.rgb * color.rgb * 2.0f;
	finalPixel.a = color.a * texureColor.a;
	finalPixel = lerp(fixed4(0.5f,0.5f,0.5f,0.5f), finalPixel, finalPixel.a);
#elif defined(_ADDITIVEBLEND)
	//Additive
	finalPixel = texureColor * 2.0f * color;
#elif defined(_ADDITIVEBLEND_SOFT)
	//Additive soft
	finalPixel = color * texureColor;
	finalPixel.rgb *= finalPixel.a;
#else
	//Standard alpha
	finalPixel.a = texureColor.a * color.a;
	finalPixel.rgb = (texureColor.rgb * color.rgb) * finalPixel.a;
#endif 
	
	return finalPixel;
}

inline fixed4 calculatePixel(fixed4 texureColor) : SV_Target
{
	fixed4 finalPixel;
	
#if defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel = texureColor;
#elif defined(_MULTIPLYBLEND)
	//Multiply
	finalPixel = texureColor;
	finalPixel = lerp(fixed4(1,1,1,1), finalPixel, finalPixel.a);
#elif defined(_MULTIPLYBLEND_X2)
	//Multiply x2
	finalPixel.rgb = texureColor.rgb * 2.0f;
	finalPixel.a = texureColor.a;
	finalPixel = lerp(fixed4(0.5f,0.5f,0.5f,0.5f), finalPixel, finalPixel.a);
#elif defined(_ADDITIVEBLEND)
	//Additive
	finalPixel = texureColor * 2.0f;
#elif defined(_ADDITIVEBLEND_SOFT)
	//Additive soft
	finalPixel = texureColor;
	finalPixel.rgb *= finalPixel.a;
#else
	//Standard alpha
	finalPixel.a = texureColor.a;
	finalPixel.rgb = texureColor.rgb * finalPixel.a;
#endif 

	return finalPixel;
}

////////////////////////////////////////
// Alpha Clipping
//

#if defined(_ALPHA_CLIP)

uniform fixed _Cutoff;

#define ALPHA_CLIP(pixel, color) clip((pixel.a * color.a) - _Cutoff);

#else

#define ALPHA_CLIP(pixel, color)

#endif

////////////////////////////////////////
// Color functions
//

uniform fixed4 _Color;

inline fixed4 calculateVertexColor(fixed4 color)
{
	return color * _Color;
}

#if defined(_COLOR_ADJUST)

uniform float _Hue;
uniform float _Saturation;
uniform float _Brightness;
uniform fixed4 _OverlayColor;

float3 rgb2hsv(float3 c)
{
  float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
  float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
  float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

  float d = q.x - min(q.w, q.y);
  float e = 1.0e-10;
  return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsv2rgb(float3 c) 
{
  c = float3(c.x, clamp(c.yz, 0.0, 1.0));
  float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
  float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
  return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

inline fixed4 adjustColor(fixed4 color)
{
	float3 hsv = rgb2hsv(color.rgb);
	
	hsv.x += _Hue; 
	hsv.y *= _Saturation; 
	hsv.z *= _Brightness;
	
	color.rgb = hsv2rgb(hsv);
	
	return color;
}

#define COLORISE(pixel) pixel.rgb = lerp(pixel.rgb, _OverlayColor.rgb, _OverlayColor.a * pixel.a);
#define COLORISE_ADDITIVE(pixel) pixel.rgb = ((1.0-_OverlayColor.a) * pixel.rgb);

#else  // !_COLOR_ADJUST

#define COLORISE(pixel)
#define COLORISE_ADDITIVE(pixel)

#endif // !_COLOR_ADJUST

////////////////////////////////////////
// Texture functions
//

uniform sampler2D _MainTex;

#if _TEXTURE_BLEND
uniform sampler2D _BlendTex;
uniform float _BlendAmount;

fixed4 calculateBlendedTexturePixel(float2 texcoord)
{
	return (1.0-_BlendAmount) * tex2D(_MainTex, texcoord) + _BlendAmount * tex2D(_BlendTex, texcoord);
}
#endif // _TEXTURE_BLEND

inline fixed4 calculateTexturePixel(float2 texcoord)
{
	fixed4 pixel;
	
#if _TEXTURE_BLEND
	pixel = calculateBlendedTexturePixel(texcoord);
#else
	pixel = tex2D(_MainTex, texcoord);
#endif // !_TEXTURE_BLEND

#if defined(_COLOR_ADJUST)
	pixel = adjustColor(pixel);
#endif // _COLOR_ADJUST

	return pixel;
}

uniform fixed4 _MainTex_ST;

inline float2 calculateTextureCoord(float4 texcoord)
{
	return TRANSFORM_TEX(texcoord, _MainTex);
}

#endif // SHADER_SHARED_INCLUDED