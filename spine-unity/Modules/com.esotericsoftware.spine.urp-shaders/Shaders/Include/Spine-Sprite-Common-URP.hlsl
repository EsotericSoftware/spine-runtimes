#ifndef URP_SPRITE_COMMON_INCLUDED
#define URP_SPRITE_COMMON_INCLUDED

#undef LIGHTMAP_ON

#if defined(_SPECULAR) || defined(_SPECULAR_GLOSSMAP)
#define SPECULAR
#endif

//Have to process lighting per pixel if using normal maps or a diffuse ramp or rim lighting or specular
#if defined(_NORMALMAP) || defined(_DIFFUSE_RAMP) || defined(_RIM_LIGHTING) || defined(SPECULAR)
#define PER_PIXEL_LIGHTING
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "SpineCoreShaders/ShaderShared.cginc"

#if defined(SPECULAR)

sampler2D _MetallicGlossMap;

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
#endif

#endif // URP_SPRITE_COMMON_INCLUDED
