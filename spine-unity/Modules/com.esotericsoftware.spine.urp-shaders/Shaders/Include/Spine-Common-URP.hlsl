#ifndef SPINE_COMMON_URP_INCLUDED
#define SPINE_COMMON_URP_INCLUDED

#ifdef USE_FORWARD_PLUS
    #define IS_URP_14_OR_NEWER 1
    #define IS_URP_12_OR_NEWER 1
#else
    #define IS_URP_14_OR_NEWER 0
    #ifdef UNIVERSAL_REALTIME_LIGHTS_INCLUDED
        #define IS_URP_12_OR_NEWER 1
    #else
        #define IS_URP_12_OR_NEWER 0
    #endif
#endif
#if IS_URP_14_OR_NEWER && !defined(_USE_WEBGL1_LIGHTS)
    #define IS_URP_15_OR_NEWER 1
#else
    #define IS_URP_15_OR_NEWER 0
#endif

#if defined(_WRITE_RENDERING_LAYERS) && IS_URP_14_OR_NEWER
#define USE_WRITE_RENDERING_LAYERS
#endif

#if defined(_LIGHT_LAYERS) && IS_URP_12_OR_NEWER
#define USE_LIGHT_LAYERS
#endif

#if defined(_LIGHT_COOKIES) && IS_URP_12_OR_NEWER
#define USE_LIGHT_COOKIES
#endif

#ifdef USE_LIGHT_LAYERS
uint GetMeshRenderingLayerBackwardsCompatible()
{
    #if IS_URP_14_OR_NEWER
    return GetMeshRenderingLayer();
    #elif IS_URP_12_OR_NEWER
    return GetMeshRenderingLightLayer();
    #else
    return 0;
    #endif
}
#else
uint GetMeshRenderingLayerBackwardsCompatible()
{
    return 0;
}
#endif

#if USE_FORWARD_PLUS
// note: LIGHT_LOOP_BEGIN accesses inputData.normalizedScreenSpaceUV and inputData.positionWS.
#define LIGHT_LOOP_BEGIN_SPINE LIGHT_LOOP_BEGIN
#define LIGHT_LOOP_END_SPINE LIGHT_LOOP_END
#elif !_USE_WEBGL1_LIGHTS
#define LIGHT_LOOP_BEGIN_SPINE(lightCount) \
    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex) {

#define LIGHT_LOOP_END_SPINE }
#else
// WebGL 1 doesn't support variable for loop conditions
#define LIGHT_LOOP_BEGIN_SPINE(lightCount) \
    for (int lightIndex = 0; lightIndex < _WEBGL1_MAX_LIGHTS; ++lightIndex) { \
        if (lightIndex >= (int)lightCount) break;

#define LIGHT_LOOP_END_SPINE }
#endif

#endif
