#ifndef VERTEX_LIT_FORWARD_PASS_URP_INCLUDED
#define VERTEX_LIT_FORWARD_PASS_URP_INCLUDED

#include "Include/Spine-Sprite-Common-URP.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "SpineCoreShaders/SpriteLighting.cginc"
#include "SpineCoreShaders/Spine-Common.cginc"
#include "Spine-Common-URP.hlsl"

#if defined(_RIM_LIGHTING) || defined(_ADDITIONAL_LIGHTS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	#define NEEDS_POSITION_WS
#endif

////////////////////////////////////////
// Vertex output struct
//
struct VertexOutputLWRP
{
	float4 pos : SV_POSITION;
	fixed4 vertexColor : COLOR;
	float3 texcoord : TEXCOORD0;

	half4 fogFactorAndVertexLight : TEXCOORD1;
	half3 viewDirectionWS : TEXCOORD2;

	DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 3);

#if defined(_NORMALMAP)
	half4 normalWorld : TEXCOORD4;
	half4 tangentWorld : TEXCOORD5;
	half4 binormalWorld : TEXCOORD6;
#else
	half3 normalWorld : TEXCOORD4;
#endif
#if (defined(_MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)) && !defined(_RECEIVE_SHADOWS_OFF)
	float4 shadowCoord : TEXCOORD7;
#endif
#if defined(NEEDS_POSITION_WS)
	float4 positionWS : TEXCOORD8;
#endif

	UNITY_VERTEX_OUTPUT_STEREO
};

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////
#if defined(_ADDITIONAL_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
half4 CalculateShadowMaskBackwardsCompatible(InputData inputData)
{
	// To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
	half4 shadowMask = inputData.shadowMask;
#elif !defined (LIGHTMAP_ON)
	half4 shadowMask = unity_ProbesOcclusion;
#else
	half4 shadowMask = half4(1, 1, 1, 1);
#endif

	return shadowMask;
}
#endif

half3 LightweightLightVertexSimplified(float3 positionWS, half3 normalWS) {
#ifdef _MAIN_LIGHT_VERTEX
	Light mainLight = GetMainLight();
	half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
	half3 diffuseLightColor = LightingLambert(attenuatedLightColor, mainLight.direction, normalWS);
#else
	half3 diffuseLightColor = half3(0, 0, 0);
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, positionWS);
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
		diffuseLightColor += LightingLambert(attenuatedLightColor, light.direction, normalWS);
	}
#endif // _ADDITIONAL_LIGHTS_VERTEX
	return diffuseLightColor;
}

#ifdef _DIFFUSE_RAMP
half3 LightingLambertRamped(half3 lightColor, float attenuation, half3 lightDir, half3 normal)
{
	half angleDot = max(0, dot(lightDir, normal));
	return calculateRampedDiffuse(lightColor, attenuation, angleDot);
}
#endif

#if defined(SPECULAR)

half3 ProcessLightPBRSimplified(InputData inputData, BRDFData brdfData, half4 shadowMask, uint meshRenderingLayers, int lightIndex)
{
#if defined(_ADDITIONAL_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
	Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
#else
	Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
#endif
#ifdef _LIGHT_LAYERS
	if (!IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		return half3(0, 0, 0);
#endif
	return LightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS);
}

half4 LightweightFragmentPBRSimplified(InputData inputData, half4 texAlbedoAlpha, half metallic, half3 specular,
	half smoothness, half3 emission, half4 vertexColor)
{
	half4 albedo = texAlbedoAlpha * vertexColor;

	BRDFData brdfData;
	half ignoredAlpha = 1; // ignore alpha, otherwise
	InitializeBRDFData(albedo.rgb, metallic, specular, smoothness, ignoredAlpha, brdfData);
	brdfData.specular *= albedo.a;

#ifndef _MAIN_LIGHT_VERTEX
#if (defined(_MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)) && !defined(_RECEIVE_SHADOWS_OFF)
	Light mainLight = GetMainLight(inputData.shadowCoord);
#else
	Light mainLight = GetMainLight();
#endif

	half3 finalColor = inputData.bakedGI * albedo.rgb;
	finalColor += LightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS);
#else // _MAIN_LIGHT_VERTEX
	half3 finalColor = inputData.bakedGI * albedo.rgb;
#endif // _MAIN_LIGHT_VERTEX

#ifdef _ADDITIONAL_LIGHTS
	uint meshRenderingLayers = GetMeshRenderingLayerBackwardsCompatible();

#if defined(_ADDITIONAL_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
	half4 shadowMask = CalculateShadowMaskBackwardsCompatible(inputData);
#else
	half4 shadowMask = half4(1, 1, 1, 1);
#endif

#if USE_FORWARD_PLUS
	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
	{
		FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
		finalColor += ProcessLightPBRSimplified(inputData, brdfData, shadowMask, meshRenderingLayers, lightIndex);
	}
#endif
	uint pixelLightCount = GetAdditionalLightsCount();
	LIGHT_LOOP_BEGIN_SPINE(pixelLightCount)
		finalColor += ProcessLightPBRSimplified(inputData, brdfData, shadowMask, meshRenderingLayers, lightIndex);
	LIGHT_LOOP_END_SPINE

#endif // _ADDITIONAL_LIGHTS

#ifdef _ADDITIONAL_LIGHTS_VERTEX
	finalColor += inputData.vertexLighting * brdfData.diffuse;
#endif
	finalColor += emission;
	return prepareLitPixelForOutput(half4(finalColor, albedo.a), texAlbedoAlpha.a, vertexColor.a);
}

#else // !SPECULAR

half3 ProcessLightLambert(InputData inputData, half4 shadowMask, uint meshRenderingLayers, int lightIndex)
{
#if defined(_ADDITIONAL_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
	Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
#else
	Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
#endif

#ifdef _LIGHT_LAYERS
	if (!IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
		return half3(0, 0, 0);
#endif

	half3 attenuation = (light.distanceAttenuation * light.shadowAttenuation);
	half3 attenuatedLightColor = light.color * attenuation;
#ifndef _DIFFUSE_RAMP
	return LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
#else
	return LightingLambertRamped(light.color, attenuation, light.direction, inputData.normalWS);
#endif
}

half4 LightweightFragmentBlinnPhongSimplified(InputData inputData, half4 texDiffuseAlpha, half3 emission, half4 vertexColor)
{
	half4 diffuse = texDiffuseAlpha * vertexColor;

#ifndef _MAIN_LIGHT_VERTEX
#if (defined(_MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)) && !defined(_RECEIVE_SHADOWS_OFF)
	Light mainLight = GetMainLight(inputData.shadowCoord);
#else
	Light mainLight = GetMainLight();
#endif
	half3 diffuseLighting = inputData.bakedGI;

	half3 attenuation = mainLight.distanceAttenuation* mainLight.shadowAttenuation;
	half3 attenuatedLightColor = mainLight.color * attenuation;
#ifndef _DIFFUSE_RAMP
	diffuseLighting += LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS);
#else
	diffuseLighting += LightingLambertRamped(mainLight.color, attenuation, mainLight.direction, inputData.normalWS);
#endif
#else // _MAIN_LIGHT_VERTEX
	half3 diffuseLighting = inputData.bakedGI;
#endif // _MAIN_LIGHT_VERTEX

#ifdef _ADDITIONAL_LIGHTS
	uint meshRenderingLayers = GetMeshRenderingLayerBackwardsCompatible();

#if defined(_ADDITIONAL_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
	half4 shadowMask = CalculateShadowMaskBackwardsCompatible(inputData);
#else
	half4 shadowMask = half4(1, 1, 1, 1);
#endif
#if USE_FORWARD_PLUS
	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
	{
		FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
		diffuseLighting += ProcessLightLambert(inputData, shadowMask, meshRenderingLayers, lightIndex);
	}
#endif
	uint pixelLightCount = GetAdditionalLightsCount();
	LIGHT_LOOP_BEGIN_SPINE(pixelLightCount)
		diffuseLighting += ProcessLightLambert(inputData, shadowMask, meshRenderingLayers, lightIndex);
	LIGHT_LOOP_END_SPINE

#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
	diffuseLighting += inputData.vertexLighting;
#endif
	diffuseLighting += emission;
	//half3 finalColor = diffuseLighting * diffuse + emission;
	half3 finalColor = diffuseLighting * diffuse.rgb;
	return prepareLitPixelForOutput(half4(finalColor, diffuse.a), texDiffuseAlpha.a, vertexColor.a);
}
#endif // SPECULAR

VertexOutputLWRP ForwardPassVertexSprite(VertexInput input)
{
	VertexOutputLWRP output = (VertexOutputLWRP)0;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.pos = calculateLocalPos(input.vertex);
	output.vertexColor = calculateVertexColor(input.color);
	output.texcoord = float3(calculateTextureCoord(input.texcoord), 0);

	float3 positionWS = TransformObjectToWorld(input.vertex.xyz);

	float backFaceSign = 1;
#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	backFaceSign = calculateBackfacingSign(positionWS.xyz);
#endif
	output.viewDirectionWS = GetCameraPositionWS() - positionWS;
#if defined(NEEDS_POSITION_WS)
	output.positionWS = float4(positionWS, 1);
#endif

	half3 normalWS = calculateSpriteWorldNormal(input, -backFaceSign);
	output.normalWorld.xyz = normalWS;
#if defined(_NORMALMAP)
	output.tangentWorld.xyz = calculateWorldTangent(input.tangent);
	output.binormalWorld.xyz = calculateSpriteWorldBinormal(input, output.normalWorld.xyz, output.tangentWorld.xyz, backFaceSign);
#endif

	output.fogFactorAndVertexLight.yzw = LightweightLightVertexSimplified(positionWS, normalWS);

#if (defined(_MAIN_LIGHT_SHADOWS) || defined(MAIN_LIGHT_CALCULATE_SHADOWS)) && !defined(_RECEIVE_SHADOWS_OFF)
	VertexPositionInputs vertexInput;
	vertexInput.positionWS = positionWS;
	vertexInput.positionCS = output.pos;
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif

#if defined(_FOG)
	half fogFactor = ComputeFogFactor(output.pos.z);
	output.fogFactorAndVertexLight.x = fogFactor;
#endif

	OUTPUT_SH(normalWS.xyz, output.vertexSH);
	return output;
}

half4 ForwardPassFragmentSprite(VertexOutputLWRP input
#ifdef USE_WRITE_RENDERING_LAYERS
	, out float4 outRenderingLayers : SV_Target1
#endif
) : SV_Target0
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	fixed4 texureColor = calculateTexturePixel(input.texcoord.xy);
	RETURN_UNLIT_IF_ADDITIVE_SLOT(texureColor, input.vertexColor) // shall be called before ALPHA_CLIP
	ALPHA_CLIP(texureColor, input.vertexColor)

	// fill out InputData struct
	InputData inputData;
#if !defined(_RECEIVE_SHADOWS_OFF)
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
		inputData.shadowCoord = input.shadowCoord;
	#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
		inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS.xyz);
	#elif defined(_MAIN_LIGHT_SHADOWS)
		inputData.shadowCoord = input.shadowCoord;
	#else
		inputData.shadowCoord = float4(0, 0, 0, 0);
	#endif
#endif

	inputData.viewDirectionWS = input.viewDirectionWS;
	inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;

#if defined(PER_PIXEL_LIGHTING) && defined(_NORMALMAP)
	half3 normalWS = calculateNormalFromBumpMap(input.texcoord.xy, input.tangentWorld.xyz, input.binormalWorld.xyz, input.normalWorld.xyz);
#else
	half3 normalWS = input.normalWorld.xyz;
#endif

	inputData.normalWS = normalWS;
	inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
#if defined(_RIM_LIGHTING) || defined(_ADDITIONAL_LIGHTS)
	inputData.positionWS = input.positionWS.rgb;
#endif
#if defined(_ADDITIONAL_LIGHTS) && USE_FORWARD_PLUS
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.pos);
#else
	inputData.normalizedScreenSpaceUV = 0;
#endif

#if defined(SPECULAR)
	half2 metallicGloss = getMetallicGloss(input.texcoord.xy);
	half metallic = metallicGloss.x;
	half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

	half3 specular = half3(0, 0, 0);
	half4 emission = half4(0, 0, 0, 1);
	APPLY_EMISSION_SPECULAR(emission, input.texcoord.xy)
	half4 pixel = LightweightFragmentPBRSimplified(inputData, texureColor, metallic, specular, smoothness, emission.rgb, input.vertexColor);
#else
	half3 emission = half3(0, 0, 0);
	APPLY_EMISSION(emission, input.texcoord.xy)
	half4 pixel = LightweightFragmentBlinnPhongSimplified(inputData, texureColor, emission, input.vertexColor);
#endif

#if defined(_RIM_LIGHTING)
	pixel.rgb = applyRimLighting(input.positionWS.xyz, normalWS, pixel);
#endif

	COLORISE(pixel)
	APPLY_FOG_LWRP(pixel, input.fogFactorAndVertexLight.x)

#ifdef USE_WRITE_RENDERING_LAYERS
	uint renderingLayers = GetMeshRenderingLayerBackwardsCompatible();
	outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif

	return pixel;
}

#endif
