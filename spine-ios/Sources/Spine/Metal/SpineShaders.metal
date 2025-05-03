#include <metal_stdlib>
#include <simd/simd.h> 
using namespace metal;

#if __has_include("spine-ios/Sources/SpineShadersStructs/SpineShadersStructs.h")
// Cocoapods Target
    #include "spine-ios/Sources/SpineShadersStructs/SpineShadersStructs.h"
#elif  __has_include("../../SpineShadersStructs/SpineShadersStructs.h")
// Swift Package target
    #include "../../SpineShadersStructs/SpineShadersStructs.h"
#else
    #error "Header not found. Please correct Header search path"
#endif

struct RasterizerData {
    simd_float4 position [[position]];
    simd_float4 color;
    simd_float2 textureCoordinate;
};

vertex RasterizerData
vertexShader(uint vertexID [[vertex_id]],
             constant SpineVertex *vertices [[buffer(SpineVertexInputIndexVertices)]],
             constant SpineTransform *transform [[buffer(SpineVertexInputIndexTransform)]],
             constant vector_uint2 *viewportSizePointer [[buffer(SpineVertexInputIndexViewportSize)]])
{
    RasterizerData out;

    simd_float2 pixelSpacePosition = vertices[vertexID].position.xy;

    simd_float2 viewportSize = simd_float2(*viewportSizePointer);

    out.position = simd_float4(0.0, 0.0, 0.0, 1.0);

    out.position.xy = pixelSpacePosition;
    out.position.xy *= transform->scale;
    out.position.xy += transform->translation * transform->scale + transform->offset;
    out.position.xy /= viewportSize / 2;
    out.position.y *= -1;
    
    out.color = vertices[vertexID].color;
    
    out.textureCoordinate = vertices[vertexID].uv;
    
    return out;
}

fragment simd_float4
fragmentShader(RasterizerData in [[stage_in]],
               texture2d<half> colorTexture [[ texture(SpineTextureIndexBaseColor) ]])
{
    constexpr sampler textureSampler (mag_filter::nearest,
                                      min_filter::nearest);
    
    const half4 colorSample = colorTexture.sample(textureSampler, in.textureCoordinate);
    
    return simd_float4(colorSample) * in.color;
}
