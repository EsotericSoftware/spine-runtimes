#ifndef SKELETONLIT_SHADOW_CASTER_PASS_LW_INCLUDED
#define SKELETONLIT_SHADOW_CASTER_PASS_LW_INCLUDED

#include "Spine-Common-ShadowCasterPass-LW.hlsl"

VaryingsSpine ShadowPassVertexSkeletonLit(AttributesSpine input)
{
	VaryingsSpine output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);
	output.positionCS = GetShadowPositionHClip(input);
	output.texcoordAndAlpha.a = input.vertexColor.a;
	return output;
}

half4 ShadowPassFragmentSkeletonLit(VaryingsSpine input) : SV_TARGET
{
	fixed4 texureColor = calculateTexturePixel(input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _Cutoff);
	return 0;
}

#endif
