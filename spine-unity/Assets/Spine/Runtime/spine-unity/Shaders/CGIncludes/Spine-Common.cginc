#ifndef SPINE_COMMON_INCLUDED
#define SPINE_COMMON_INCLUDED

#if defined(USE_LWRP)
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#define GammaToLinearSpace SRGBToLinear
#define LinearToGammaSpace LinearToSRGB
#elif defined(USE_URP)
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#define GammaToLinearSpace SRGBToLinear
#define LinearToGammaSpace LinearToSRGB
#else
#include "UnityCG.cginc"
#endif

inline half3 GammaToTargetSpace(half3 gammaColor) {
#if UNITY_COLORSPACE_GAMMA
	return gammaColor;
#else
	return GammaToLinearSpace(gammaColor);
#endif
}

inline half3 TargetToGammaSpace(half3 targetColor) {
#if UNITY_COLORSPACE_GAMMA
	return targetColor;
#else
	return LinearToGammaSpace(targetColor);
#endif
}

inline half4 PMAGammaToTargetSpace(half4 gammaPMAColor) {
#if UNITY_COLORSPACE_GAMMA
	return gammaPMAColor;
#else
	return gammaPMAColor.a == 0 ?
		half4(GammaToLinearSpace(gammaPMAColor.rgb), gammaPMAColor.a) :
		half4(GammaToLinearSpace(gammaPMAColor.rgb / gammaPMAColor.a) * gammaPMAColor.a, gammaPMAColor.a);
#endif
}

#endif
