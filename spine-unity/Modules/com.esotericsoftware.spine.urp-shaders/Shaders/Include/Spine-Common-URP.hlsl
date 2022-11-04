#ifndef SPINE_COMMON_URP_INCLUDED
#define SPINE_COMMON_URP_INCLUDED

#ifdef _LIGHT_LAYERS
uint GetMeshRenderingLayerBackwardsCompatible()
{
    return GetMeshRenderingLayer();
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
