#ifndef SPRITE_STANDARD_PASS_URP_INCLUDED
#define SPRITE_STANDARD_PASS_URP_INCLUDED

#include "../Include/SpineCoreShaders/ShaderShared.cginc"
#include "../Include/SpineCoreShaders/SpriteLighting.cginc"
#if defined(_ALPHAPREMULTIPLY_ON)
	#undef _STRAIGHT_ALPHA_INPUT
#elif !defined(_STRAIGHT_ALPHA_INPUT)
	#define _STRAIGHT_ALPHA_INPUT
#endif
#include "../Include/SpineCoreShaders/Spine-Skeleton-Tint-Common.cginc"

#if USE_SHAPE_LIGHT_TYPE_0
SHAPE_LIGHT(0)
#endif

#if USE_SHAPE_LIGHT_TYPE_1
SHAPE_LIGHT(1)
#endif

#if USE_SHAPE_LIGHT_TYPE_2
SHAPE_LIGHT(2)
#endif

#if USE_SHAPE_LIGHT_TYPE_3
SHAPE_LIGHT(3)
#endif

TEXTURE2D(_MaskTex);
SAMPLER(sampler_MaskTex);

struct VertexOutputSpriteURP2D
{
	float4 pos : SV_POSITION;
	half4 vertexColor : COLOR;
	float3 texcoord : TEXCOORD0;
	float2 lightingUV : TEXCOORD1;

	half3 viewDirectionWS : TEXCOORD2;

#if defined(_NORMALMAP)
	half4 normalWorld : TEXCOORD4;
	half4 tangentWorld : TEXCOORD5;
	half4 binormalWorld : TEXCOORD6;
#else
	half3 normalWorld : TEXCOORD4;
#endif
#if defined(_RIM_LIGHTING)
	float4 positionWS : TEXCOORD8;
#endif

#if defined(_TINT_BLACK_ON)
	float3 darkColor : TEXCOORD9;
#endif
};

VertexOutputSpriteURP2D CombinedShapeLightVertex(VertexInput input)
{
	VertexOutputSpriteURP2D output = (VertexOutputSpriteURP2D)0;

	UNITY_SETUP_INSTANCE_ID(input);

	output.pos = calculateLocalPos(input.vertex);
	float4 clipVertex = output.pos / output.pos.w;
	output.lightingUV = ComputeScreenPos(clipVertex).xy;

	output.vertexColor = calculateVertexColor(input.color);
#if defined(_TINT_BLACK_ON)
	output.darkColor = GammaToTargetSpace(
		half3(input.tintBlackRG.r, input.tintBlackRG.g, input.tintBlackB.r)) + _Black.rgb;
#endif

	output.texcoord = float3(calculateTextureCoord(input.texcoord), 0);

	float3 positionWS = TransformObjectToWorld(input.vertex.xyz);

	float backFaceSign = 1;
#if defined(FIXED_NORMALS_BACKFACE_RENDERING)
	backFaceSign = calculateBackfacingSign(positionWS.xyz);
#endif
	output.viewDirectionWS = GetCameraPositionWS() - positionWS;

#if defined(_RIM_LIGHTING)
	output.positionWS = float4(positionWS, 1);
#endif
	half3 normalWS = calculateSpriteWorldNormal(input, -backFaceSign);
	output.normalWorld.xyz = normalWS;

#if defined(_RIM_LIGHTING)
	#if defined(_NORMALMAP)
	output.tangentWorld.xyz = calculateWorldTangent(input.tangent);
	output.binormalWorld.xyz = calculateSpriteWorldBinormal(input, output.normalWorld.xyz, output.tangentWorld.xyz, backFaceSign);
	#endif
#endif
	return output;
}

#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

half4 CombinedShapeLightFragment(VertexOutputSpriteURP2D input) : SV_Target
{
	fixed4 texureColor = calculateTexturePixel(input.texcoord.xy);
	RETURN_UNLIT_IF_ADDITIVE_SLOT_TINT(texureColor, input.vertexColor, input.darkColor, _Color.a, _Black.a) // shall be called before ALPHA_CLIP
	ALPHA_CLIP(texureColor, input.vertexColor)
#if defined(_TINT_BLACK_ON)
	half4 main = fragTintedColor(texureColor, input.darkColor, input.vertexColor, _Color.a, _Black.a);
#else
	half4 main = texureColor * input.vertexColor;
#endif

	half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.texcoord.xy);
	main.rgb = main.a == 0 ? main.rgb : main.rgb / main.a; // un-premultiply for additive lights in CombinedShapeLightShared, reapply afterwards
#if UNITY_VERSION  < 202120
	half4 pixel = half4(CombinedShapeLightShared(half4(main.rgb, 1), mask, input.lightingUV).rgb * main.a, main.a);
#else
	SurfaceData2D surfaceData;
	InputData2D inputData;
	surfaceData.albedo = main.rgb;
	surfaceData.alpha = 1;
	surfaceData.mask = mask;
	inputData.uv = input.texcoord.xy;
	inputData.lightingUV = input.lightingUV;
	half4 pixel = half4(CombinedShapeLightShared(surfaceData, inputData).rgb * main.a, main.a);
#endif

#if defined(_RIM_LIGHTING)
	#if defined(_NORMALMAP)
	half3 normalWS = calculateNormalFromBumpMap(input.texcoord.xy, input.tangentWorld.xyz, input.binormalWorld.xyz, input.normalWorld.xyz);
	#else
	half3 normalWS = input.normalWorld.xyz;
	#endif

	pixel.rgb = applyRimLighting(input.positionWS.xyz, normalWS, pixel);
#endif

	APPLY_EMISSION(pixel.rgb, input.texcoord.xy)
	pixel = prepareLitPixelForOutput(pixel, texureColor.a, input.vertexColor.a);
	COLORISE(pixel)
	return pixel;
}

#endif
