#ifndef SpineSharedStructs_h
#define SpineSharedStructs_h

#include <simd/simd.h>

// Buffer index values shared between shader and C code to ensure Metal shader buffer inputs
// match Metal API buffer set calls.
typedef enum AAPLVertexInputIndex
{
    AAPLVertexInputIndexVertices     = 0,
    AAPLVertexInputIndexTransform    = 1,
    AAPLVertexInputIndexViewportSize = 2,
} AAPLVertexInputIndex;

// Texture index values shared between shader and C code to ensure Metal shader buffer inputs match
//   Metal API texture set calls
typedef enum AAPLTextureIndex
{
    AAPLTextureIndexBaseColor = 0,
} AAPLTextureIndex;

//  This structure defines the layout of vertices sent to the vertex
//  shader. This header is shared between the .metal shader and C code, to guarantee that
//  the layout of the vertex array in the C code matches the layout that the .metal
//  vertex shader expects.
typedef struct
{
    vector_float2 position;
    vector_float4 color;
    vector_float2 uv;
} AAPLVertex;

typedef struct {
    vector_float2 translation;
    vector_float2 scale;
    vector_float2 offset;
} AAPLTransform;

#endif /* SpineSharedStructs_h */
