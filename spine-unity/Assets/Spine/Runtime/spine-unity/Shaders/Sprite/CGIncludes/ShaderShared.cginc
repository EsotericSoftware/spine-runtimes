// Upgrade NOTE: upgraded instancing buffer 'PerDrawSprite' to new syntax.

// Upgrade NOTE: upgraded instancing buffer 'PerDrawSprite' to new syntax.

#ifndef SHADER_SHARED_INCLUDED
#define SHADER_SHARED_INCLUDED

#if defined(USE_LWRP)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#elif defined(USE_URP)
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
#include "UnityCG.cginc"
#endif

#ifdef UNITY_INSTANCING_ENABLED

    UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
        // SpriteRenderer.Color while Non-Batched/Instanced.
        fixed4 unity_SpriteRendererColorArray[UNITY_INSTANCED_ARRAY_SIZE];
        // this could be smaller but that's how bit each entry is regardless of type
        float4 unity_SpriteFlipArray[UNITY_INSTANCED_ARRAY_SIZE];
    UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

    #define _RendererColor unity_SpriteRendererColorArray[unity_InstanceID]
    #define _Flip unity_SpriteFlipArray[unity_InstanceID]

#endif // instancing

CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
    fixed4 _RendererColor;
    float4 _Flip;
#endif
    float _EnableExternalAlpha;
CBUFFER_END

////////////////////////////////////////
// Space functions
//

inline float4 calculateWorldPos(float4 vertex)
{
	return mul(unity_ObjectToWorld, vertex);
}

#if defined(USE_LWRP) || defined(USE_URP)
// snaps post-transformed position to screen pixels
inline float4 UnityPixelSnap(float4 pos)
{
	float2 hpc = _ScreenParams.xy * 0.5f;
#if  SHADER_API_PSSL
	// sdk 4.5 splits round into v_floor_f32(x+0.5) ... sdk 5.0 uses v_rndne_f32, for compatabilty we use the 4.5 version
	float2 temp = ((pos.xy / pos.w) * hpc) + float2(0.5f, 0.5f);
	float2 pixelPos = float2(__v_floor_f32(temp.x), __v_floor_f32(temp.y));
#else
	float2 pixelPos = round((pos.xy / pos.w) * hpc);
#endif
	pos.xy = pixelPos / hpc * pos.w;
	return pos;
}
#endif

inline float4 calculateLocalPos(float4 vertex)
{
#if !defined(USE_LWRP) && !defined(USE_URP)
#ifdef UNITY_INSTANCING_ENABLED
    vertex.xy *= _Flip.xy;
#endif
#endif

#if defined(USE_LWRP) || defined(USE_URP)
	float4 pos = TransformObjectToHClip(vertex.xyz);
#else
	float4 pos = UnityObjectToClipPos(vertex);
#endif

#ifdef PIXELSNAP_ON
	pos = UnityPixelSnap(pos);
#endif

	return pos;
}

inline half3 calculateWorldNormal(float3 normal)
{
#if defined(USE_LWRP) || defined(USE_URP)
	return TransformObjectToWorldNormal(normal);
#else
	return UnityObjectToWorldNormal(normal);
#endif
}

////////////////////////////////////////
// Normal map functions
//

#if defined(_NORMALMAP)

uniform sampler2D _BumpMap;
uniform half _BumpScale;

half3 UnpackScaleNormal(half4 packednormal, half bumpScale)
{
	#if defined(UNITY_NO_DXT5nm)
		return packednormal.xyz * 2 - 1;
	#else
		half3 normal;
		normal.xy = (packednormal.wy * 2 - 1);
		// Note: we allow scaled normals in LWRP since we might be using fewer instructions.
		#if (SHADER_TARGET >= 30) || defined(USE_LWRP) || defined(USE_URP)
			// SM2.0: instruction count limitation
			// SM2.0: normal scaler is not supported
			normal.xy *= bumpScale;
		#endif
		normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
		return normal;
	#endif
}


inline half3 calculateWorldTangent(float4 tangent)
{
#if defined(USE_LWRP) || defined(USE_URP)
	return TransformObjectToWorldDir(tangent.xyz);
#else
	return UnityObjectToWorldDir(tangent);
#endif
}

inline half3 calculateWorldBinormal(half3 normalWorld, half3 tangentWorld, float tangentSign)
{
	//When calculating the binormal we have to flip it when the mesh is scaled negatively.
	//Normally this would just be unity_WorldTransformParams.w but this isn't set correctly by Unity for its SpriteRenderer meshes so get from objectToWorld matrix scale instead.
	half worldTransformSign = sign(unity_ObjectToWorld[0][0] * unity_ObjectToWorld[1][1] * unity_ObjectToWorld[2][2]);
	half sign = tangentSign * worldTransformSign;
	return cross(normalWorld, tangentWorld) * sign;
}

inline half3 calculateNormalFromBumpMap(float2 texUV, half3 tangentWorld, half3 binormalWorld, half3 normalWorld)
{
	half3 localNormal = UnpackScaleNormal(tex2D(_BumpMap, texUV), _BumpScale);
	half3x3 rotation = half3x3(tangentWorld, binormalWorld, normalWorld);
	half3 normal = normalize(mul(localNormal, rotation));
	return normal;
}

#endif // _NORMALMAP

////////////////////////////////////////
// Blending functions
//

inline fixed4 prepareLitPixelForOutput(fixed4 finalPixel, fixed4 color) : SV_Target
{
#if defined(_ALPHABLEND_ON)
	//Normal Alpha
	finalPixel.rgb *= finalPixel.a;
#elif defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel.rgb *= color.a;
#elif defined(_MULTIPLYBLEND)
	//Multiply
	finalPixel = lerp(fixed4(1,1,1,1), finalPixel, finalPixel.a);
#elif defined(_MULTIPLYBLEND_X2)
	//Multiply x2
	finalPixel.rgb *= 2.0f;
	finalPixel = lerp(fixed4(0.5f,0.5f,0.5f,0.5f), finalPixel, finalPixel.a);
#elif defined(_ADDITIVEBLEND)
	//Additive
	finalPixel *= 2.0f;
	finalPixel.rgb *= color.a;
#elif defined(_ADDITIVEBLEND_SOFT)
	//Additive soft
	finalPixel.rgb *= finalPixel.a;
#else
	//Opaque
	finalPixel.a = 1;
#endif
	return finalPixel;
}

inline fixed4 calculateLitPixel(fixed4 texureColor, fixed4 color, fixed3 lighting) : SV_Target
{
	fixed4 finalPixel = texureColor * color * fixed4(lighting, 1);
	finalPixel = prepareLitPixelForOutput(finalPixel, color);
	return finalPixel;
}

inline fixed4 calculateLitPixel(fixed4 texureColor, fixed3 lighting) : SV_Target
{
	// note: we let the optimizer work, removed duplicate code.
	return calculateLitPixel(texureColor, fixed4(1, 1, 1, 1), lighting);
}

inline fixed4 calculateAdditiveLitPixel(fixed4 texureColor, fixed4 color, fixed3 lighting) : SV_Target
{
	fixed4 finalPixel;

#if defined(_ALPHABLEND_ON)	|| defined(_MULTIPLYBLEND)	|| defined(_MULTIPLYBLEND_X2) || defined(_ADDITIVEBLEND) || defined(_ADDITIVEBLEND_SOFT)
	//Normal Alpha, Additive and Multiply modes
	finalPixel.rgb = (texureColor.rgb * lighting * color.rgb) * (texureColor.a * color.a);
	finalPixel.a = 1.0;
#elif defined(_ALPHAPREMULTIPLY_ON)
	//Pre multiplied alpha
	finalPixel.rgb = texureColor.rgb * lighting * color.rgb * color.a;
	finalPixel.a = 1.0;
#else
	//Opaque
	finalPixel.rgb = texureColor.rgb * lighting * color.rgb;
	finalPixel.a = 1.0;
#endif

	return finalPixel;
}

inline fixed4 calculateAdditiveLitPixel(fixed4 texureColor, fixed3 lighting) : SV_Target
{
	fixed4 finalPixel;

#if defined(_ALPHABLEND_ON)	|| defined(_MULTIPLYBLEND) || defined(_MULTIPLYBLEND_X2) || defined(_ADDITIVEBLEND) || defined(_ADDITIVEBLEND_SOFT)
	//Normal Alpha, Additive and Multiply modes
	finalPixel.rgb = (texureColor.rgb * lighting) * texureColor.a;
	finalPixel.a = 1.0;
#else
	//Pre multiplied alpha and Opaque
	finalPixel.rgb = texureColor.rgb * lighting;
	finalPixel.a = 1.0;
#endif

	return finalPixel;
}

inline fixed4 calculatePixel(fixed4 texureColor, fixed4 color) : SV_Target
{
	// note: we let the optimizer work, removed duplicate code.
	return calculateLitPixel(texureColor, color, fixed3(1, 1, 1));
}

inline fixed4 calculatePixel(fixed4 texureColor) : SV_Target
{
	// note: we let the optimizer work, removed duplicate code.
	return calculateLitPixel(texureColor, fixed4(1, 1, 1, 1), fixed3(1, 1, 1));
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
// Fog
//

#if defined(_FOG) && (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

inline fixed4 applyFog(fixed4 pixel, float fogCoordOrFactorAtLWRP)
{
#if defined(_ADDITIVEBLEND) || defined(_ADDITIVEBLEND_SOFT)
	//In additive mode blend from clear to black based on luminance
	float luminance = pixel.r * 0.3 + pixel.g * 0.59 + pixel.b * 0.11;
	fixed4 fogColor = lerp(fixed4(0,0,0,0), fixed4(0,0,0,1), luminance);
#elif defined(_MULTIPLYBLEND)
	//In multiplied mode fade to white based on inverse luminance
	float luminance = pixel.r * 0.3 + pixel.g * 0.59 + pixel.b * 0.11;
	fixed4 fogColor = lerp(fixed4(1,1,1,1), fixed4(0,0,0,0), luminance);
#elif defined(_MULTIPLYBLEND_X2)
	//In multipliedx2 mode fade to grey based on inverse luminance
	float luminance = pixel.r * 0.3 + pixel.g * 0.59 + pixel.b * 0.11;
	fixed4 fogColor = lerp(fixed4(0.5f,0.5f,0.5f,0.5f), fixed4(0,0,0,0), luminance);
#elif defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
	//In alpha blended modes blend to fog color based on pixel alpha
	fixed4 fogColor = lerp(fixed4(0,0,0,0), unity_FogColor, pixel.a);
#else
	//In opaque mode just return fog color;
	fixed4 fogColor = unity_FogColor;
#endif

	#if defined(USE_LWRP) || defined(USE_URP)
	pixel.rgb = MixFogColor(pixel.rgb, fogColor.rgb, fogCoordOrFactorAtLWRP);
	#else
	UNITY_APPLY_FOG_COLOR(fogCoordOrFactorAtLWRP, pixel, fogColor);
	#endif

	return pixel;
}

#define APPLY_FOG(pixel, input) pixel = applyFog(pixel, input.fogCoord);
#define APPLY_FOG_LWRP(pixel, fogFactor) pixel = applyFog(pixel, fogFactor);

#define APPLY_FOG_ADDITIVE(pixel, input) \
	UNITY_APPLY_FOG_COLOR(input.fogCoord, pixel.rgb, fixed4(0,0,0,0)); // fog towards black in additive pass

#else

#define APPLY_FOG(pixel, input)
#define APPLY_FOG_LWRP(pixel, fogFactor)
#define APPLY_FOG_ADDITIVE(pixel, input)

#endif

////////////////////////////////////////
// Texture functions
//

uniform sampler2D _MainTex;

#if ETC1_EXTERNAL_ALPHA
//External alpha texture for ETC1 compression
uniform sampler2D _AlphaTex;
#endif //ETC1_EXTERNAL_ALPHA

#if _TEXTURE_BLEND
uniform sampler2D _BlendTex;
uniform float _BlendAmount;

inline fixed4 calculateBlendedTexturePixel(float2 texcoord)
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

#if ETC1_EXTERNAL_ALPHA
    fixed4 alpha = tex2D (_AlphaTex, texcoord);
    pixel.a = lerp (pixel.a, alpha.r, _EnableExternalAlpha);
#endif

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
