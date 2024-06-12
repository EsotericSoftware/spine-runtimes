#ifndef SpineShadersStructs_h
#define SpineShadersStructs_h

#include <simd/simd.h>

typedef enum SpineVertexInputIndex {
    SpineVertexInputIndexVertices     = 0,
    SpineVertexInputIndexTransform    = 1,
    SpineVertexInputIndexViewportSize = 2,
} SpineVertexInputIndex;

typedef enum SpineTextureIndex {
    SpineTextureIndexBaseColor = 0,
} SpineTextureIndex;

typedef struct {
    vector_float2 position;
    vector_float4 color;
    vector_float2 uv;
} SpineVertex;

typedef struct {
    vector_float2 translation;
    vector_float2 scale;
    vector_float2 offset;
} SpineTransform;

#endif /* SpineShadersStructs_h */
