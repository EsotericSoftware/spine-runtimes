#ifndef SPRITE_SPECULAR_INCLUDED
#define SPRITE_SPECULAR_INCLUDED

#include "ShaderMaths.cginc"

////////////////////////////////////////
// Specular functions
//

#if defined(_SPECULAR) || defined(_SPECULAR_GLOSSMAP)

#define SPECULAR


//ALL THESE FUNCTIONS ARE TAKEN AND ADAPTED FROM UNITY'S OWN PHYSICS BASED STANDARD SHADER

uniform float _Metallic;
uniform float _Glossiness;
uniform float _GlossMapScale;
uniform sampler2D _MetallicGlossMap;

struct SpecularLightData
{
	half3 lighting;	
	half3 specular;
};

struct SpecularCommonData
{
	half3 diffColor, specColor;
	// Note: smoothness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
	// Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
	half oneMinusReflectivity, smoothness;
	half alpha;
};

inline half2 getMetallicGloss(float2 uv)
{
	half2 mg;
	
#ifdef _SPECULAR_GLOSSMAP
	mg = tex2D(_MetallicGlossMap, uv).ra;
	mg.g *= _GlossMapScale;
#else
	mg.r = _Metallic;
	mg.g = _Glossiness;
#endif
	
	return mg;
}

inline half getOneMinusReflectivityFromMetallic(half metallic)
{
	// We'll need oneMinusReflectivity, so
	//   1-reflectivity = 1-lerp(dielectricSpec, 1, metallic) = lerp(1-dielectricSpec, 0, metallic)
	// store (1-dielectricSpec) in unity_ColorSpaceDielectricSpec.a, then
	//	 1-reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) = 
	//                  = alpha - metallic * alpha
	half oneMinusDielectricSpec = unity_ColorSpaceDielectricSpec.a;
	return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

inline SpecularCommonData getSpecularData(float2 uv, half4 texureColor, fixed4 color)
{
	half2 metallicGloss = getMetallicGloss(uv);
	half metallic = metallicGloss.x;
	half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.
	
	fixed4 albedo = calculatePixel(texureColor, color);
	
	half3 specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
	half oneMinusReflectivity = getOneMinusReflectivityFromMetallic(metallic);
	half3 diffColor = albedo * oneMinusReflectivity;
	
	SpecularCommonData o = (SpecularCommonData)0;
	o.diffColor = diffColor;
	o.specColor = specColor;
	o.oneMinusReflectivity = oneMinusReflectivity;
	o.smoothness = smoothness;
	
#if defined(_ALPHAPREMULTIPLY_ON) && (SHADER_TARGET >= 30)
	// Reflectivity 'removes' from the rest of components, including Transparency
	// outAlpha = 1-(1-alpha)*(1-reflectivity) = 1-(oneMinusReflectivity - alpha*oneMinusReflectivity) =
	//          = 1-oneMinusReflectivity + alpha*oneMinusReflectivity
	//o.alpha = 1-oneMinusReflectivity + albedo.a*oneMinusReflectivity;
	o.alpha = albedo.a;
#else
	o.alpha = albedo.a;
#endif
	
	return o;
}
inline half SmoothnessToPerceptualRoughness(half smoothness)
{
	return (1 - smoothness);
}

inline half PerceptualRoughnessToRoughness(half perceptualRoughness)
{
	return perceptualRoughness * perceptualRoughness;
}

// Ref: http://jcgt.org/published/0003/02/03/paper.pdf
inline half SmithJointGGXVisibilityTerm (half NdotL, half NdotV, half roughness)
{
#if 0
	// Original formulation:
	//	lambda_v	= (-1 + sqrt(a2 * (1 - NdotL2) / NdotL2 + 1)) * 0.5f;
	//	lambda_l	= (-1 + sqrt(a2 * (1 - NdotV2) / NdotV2 + 1)) * 0.5f;
	//	G			= 1 / (1 + lambda_v + lambda_l);

	// Reorder code to be more optimal
	half a			= roughness;
	half a2			= a * a;

	half lambdaV	= NdotL * sqrt((-NdotV * a2 + NdotV) * NdotV + a2);
	half lambdaL	= NdotV * sqrt((-NdotL * a2 + NdotL) * NdotL + a2);

	// Simplify visibility term: (2.0f * NdotL * NdotV) /  ((4.0f * NdotL * NdotV) * (lambda_v + lambda_l + 1e-5f));
	return 0.5f / (lambdaV + lambdaL + 1e-5f);	// This function is not intended to be running on Mobile,
												// therefore epsilon is smaller than can be represented by half
#else
    // Approximation of the above formulation (simplify the sqrt, not mathematically correct but close enough)
	half a = roughness;
	half lambdaV = NdotL * (NdotV * (1 - a) + a);
	half lambdaL = NdotV * (NdotL * (1 - a) + a);

	return 0.5f / (lambdaV + lambdaL + 1e-5f);
#endif
}

inline half GGXTerm (half NdotH, half roughness)
{
	half a2 = roughness * roughness;
	half d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
	return UNITY_INV_PI * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,
											// therefore epsilon is smaller than what can be represented by half
}

inline half3 FresnelTerm (half3 F0, half cosA)
{
	half t = pow5 (1 - cosA);	// ala Schlick interpoliation
	return F0 + (1-F0) * t;
}

inline half3 FresnelLerp (half3 F0, half F90, half cosA)
{
	half t = pow5 (1 - cosA);	// ala Schlick interpoliation
	return lerp (F0, F90, t);
}

// Note: Disney diffuse must be multiply by diffuseAlbedo / PI. This is done outside of this function.
inline half DisneyDiffuse(half NdotV, half NdotL, half LdotH, half perceptualRoughness)
{
	half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
	// Two schlick fresnel term
	half lightScatter	= (1 + (fd90 - 1) * pow5(1 - NdotL));
	half viewScatter	= (1 + (fd90 - 1) * pow5(1 - NdotV));

	return lightScatter * viewScatter;
}

// Main Physically Based BRDF
// Derived from Disney work and based on Torrance-Sparrow micro-facet model
//
//   BRDF = kD / pi + kS * (D * V * F) / 4
//   I = BRDF * NdotL
//
// * NDF (depending on UNITY_BRDF_GGX):
//  a) Normalized BlinnPhong
//  b) GGX
// * Smith for Visiblity term
// * Schlick approximation for Fresnel
SpecularLightData calculatePhysicsBasedSpecularLight(half3 specColor, half oneMinusReflectivity, half smoothness, half3 normal, half3 viewDir, half3 lightdir, half3 lightColor, half3 indirectDiffuse, half3 indirectSpecular)
{
	half perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
	half3 halfDir = safeNormalize (lightdir + viewDir);

// NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
// In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
// but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
// Following define allow to control this. Set it to 0 if ALU is critical on your platform.
// This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
// Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
#define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0 

#if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
	// The amount we shift the normal toward the view vector is defined by the dot product.
	half shiftAmount = dot(normal, viewDir);
	normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;
	// A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
	//normal = normalize(normal);

	half nv = saturate(dot(normal, viewDir)); // TODO: this saturate should no be necessary here
#else
	half nv = abs(dot(normal, viewDir));	// This abs allow to limit artifact
#endif

	half nl = saturate(dot(normal, lightdir));
	half nh = saturate(dot(normal, halfDir));

	half lv = saturate(dot(lightdir, viewDir));
	half lh = saturate(dot(lightdir, halfDir));

	// Diffuse term
	half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;

	// Specular term
	// HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
	half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
	half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
	half D = GGXTerm (nh, roughness);

	half specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later

#	ifdef UNITY_COLORSPACE_GAMMA
		specularTerm = sqrt(max(1e-4h, specularTerm));
#	endif

	// specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
	specularTerm = max(0, specularTerm * nl);

	// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
	half surfaceReduction;
#	ifdef UNITY_COLORSPACE_GAMMA
		surfaceReduction = 1.0 - 0.28f * roughness * perceptualRoughness;		// 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
#	else
		surfaceReduction = 1.0 / (roughness*roughness + 1.0);			// fade \in [0.5;1]
#	endif

	// To provide true Lambert lighting, we need to be able to kill specular completely.
	specularTerm *= any(specColor) ? 1.0 : 0.0;

	half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
	
	SpecularLightData outData = (SpecularLightData)0;
	outData.lighting = indirectDiffuse + lightColor * diffuseTerm;
	outData.specular = (specularTerm * lightColor * FresnelTerm (specColor, lh)) + (surfaceReduction * indirectSpecular * FresnelLerp (specColor, grazingTerm, nv));
	return outData;
}

#endif // _SPECULAR  && _SPECULAR_GLOSSMAP 

#endif // SPRITE_SPECULAR_INCLUDED