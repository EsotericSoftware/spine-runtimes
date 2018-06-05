#ifndef SPRITE_PIXEL_LIGHTING_INCLUDED
#define SPRITE_PIXEL_LIGHTING_INCLUDED
	
#include "ShaderShared.cginc"
#include "SpriteLighting.cginc"
#include "AutoLight.cginc"

////////////////////////////////////////
// Defines
//

////////////////////////////////////////
// Vertex output struct
//

#if defined(_NORMALMAP)
	#define _VERTEX_LIGHTING_INDEX TEXCOORD5
	#define _LIGHT_COORD_INDEX_0 6
	#define _LIGHT_COORD_INDEX_1 7
	#define _FOG_COORD_INDEX 8
#else
	#define _VERTEX_LIGHTING_INDEX TEXCOORD3
	#define _LIGHT_COORD_INDEX_0 4
	#define _LIGHT_COORD_INDEX_1 5
	#define _FOG_COORD_INDEX 6
#endif // _NORMALMAP	

struct VertexOutput
{
	float4 pos : SV_POSITION;				
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
	float4 posWorld : TEXCOORD1;
	half3 normalWorld : TEXCOORD2;
#if defined(_NORMALMAP)
	half3 tangentWorld : TEXCOORD3;  
	half3 binormalWorld : TEXCOORD4;
#endif // _NORMALMAP
	fixed3 vertexLighting : _VERTEX_LIGHTING_INDEX;
	LIGHTING_COORDS(_LIGHT_COORD_INDEX_0, _LIGHT_COORD_INDEX_1)
#if defined(_FOG)
	UNITY_FOG_COORDS(_FOG_COORD_INDEX)
#endif // _FOG	
};

////////////////////////////////////////
// Light calculations
//

uniform fixed4 _LightColor0;

inline fixed3 calculateLightDiffuse(VertexOutput input, float3 normalWorld)
{
	//For directional lights _WorldSpaceLightPos0.w is set to zero
	float3 lightWorldDirection = normalize(_WorldSpaceLightPos0.xyz - input.posWorld.xyz * _WorldSpaceLightPos0.w);
	
	float attenuation = LIGHT_ATTENUATION(input);
	float angleDot = max(0, dot(normalWorld, lightWorldDirection));
	
#if defined(_DIFFUSE_RAMP)
	fixed3 lightDiffuse = calculateRampedDiffuse(_LightColor0.rgb, attenuation, angleDot);
#else
	fixed3 lightDiffuse = _LightColor0.rgb * (attenuation * angleDot);
#endif // _DIFFUSE_RAMP
	
	return lightDiffuse;
}

inline float3 calculateNormalWorld(VertexOutput input)
{
#if defined(_NORMALMAP)
	return calculateNormalFromBumpMap(input.texcoord, input.tangentWorld, input.binormalWorld, input.normalWorld);
#else
	return input.normalWorld;
#endif
}

fixed3 calculateVertexLighting(float3 posWorld, float3 normalWorld)
{
	fixed3 vertexLighting = fixed3(0,0,0);

#ifdef VERTEXLIGHT_ON
	//Get approximated illumination from non-important point lights
	vertexLighting = Shade4PointLights (	unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
											unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
											unity_4LightAtten0, posWorld, normalWorld) * 0.5;
#endif

	return vertexLighting;
}

fixed3 calculateAmbientLight(half3 normalWorld)
{
#if defined(_SPHERICAL_HARMONICS)
	fixed3 ambient = ShadeSH9(half4(normalWorld, 1.0)) * 0.75f;
#else 
	fixed3 ambient = unity_AmbientSky.rgb * 0.75;
#endif
	return ambient;
}

////////////////////////////////////////
// Vertex program
//

VertexOutput vert(VertexInput v)
{
	VertexOutput output;
	
	output.pos = calculateLocalPos(v.vertex);
	output.color = calculateVertexColor(v.color);
	output.texcoord = calculateTextureCoord(v.texcoord);
	output.posWorld = calculateWorldPos(v.vertex);
	output.normalWorld = calculateSpriteWorldNormal(v);
	output.vertexLighting = calculateVertexLighting(output.posWorld, output.normalWorld);
	
#if defined(_NORMALMAP)
	output.tangentWorld = calculateWorldTangent(v.tangent);
	output.binormalWorld = calculateSpriteWorldBinormal(output.normalWorld, output.tangentWorld, v.tangent.w);
#endif

	TRANSFER_VERTEX_TO_FRAGMENT(output)
	
#if defined(_FOG)
	UNITY_TRANSFER_FOG(output,output.pos);
#endif // _FOG	
	
	return output;
}

////////////////////////////////////////
// Fragment programs
//

fixed4 fragBase(VertexOutput input) : SV_Target
{
	fixed4 texureColor = calculateTexturePixel(input.texcoord);
	ALPHA_CLIP(texureColor, input.color)
	
	//Get normal direction
	fixed3 normalWorld = calculateNormalWorld(input);

	//Get Ambient diffuse
	fixed3 ambient = calculateAmbientLight(normalWorld);
	
	//Get primary pixel light diffuse
	fixed3 diffuse = calculateLightDiffuse(input, normalWorld);
	
	//Combine along with vertex lighting for the base lighting pass
	fixed3 lighting = ambient + diffuse + input.vertexLighting;
	
	APPLY_EMISSION(lighting, input.texcoord)
	
	fixed4 pixel = calculateLitPixel(texureColor, input.color, lighting);
	
#if defined(_RIM_LIGHTING)
	pixel.rgb = applyRimLighting(input.posWorld, normalWorld, pixel);
#endif
	
	COLORISE(pixel)
	
#if defined(_FOG)
	fixed4 fogColor = lerp(fixed4(0,0,0,0), unity_FogColor, pixel.a);
	UNITY_APPLY_FOG_COLOR(input.fogCoord, pixel, fogColor);
#endif // _FOG
	
	return pixel;
}

fixed4 fragAdd(VertexOutput input) : SV_Target
{
	fixed4 texureColor = calculateTexturePixel(input.texcoord);
	
#if defined(_COLOR_ADJUST)
	texureColor = adjustColor(texureColor);
#endif // _COLOR_ADJUST	

	ALPHA_CLIP(texureColor, input.color)
	
	//Get normal direction
	fixed3 normalWorld = calculateNormalWorld(input);

	//Get light diffuse
	fixed3 lighting = calculateLightDiffuse(input, normalWorld);
	
	fixed4 pixel = calculateAdditiveLitPixel(texureColor, input.color, lighting);
	
	COLORISE_ADDITIVE(pixel)
	
#if defined(_FOG)
	UNITY_APPLY_FOG_COLOR(input.fogCoord, pixel.rgb, fixed4(0,0,0,0)); // fog towards black in additive pass
#endif // _FOG	
	
	return pixel;
}


#endif // SPRITE_PIXEL_LIGHTING_INCLUDED