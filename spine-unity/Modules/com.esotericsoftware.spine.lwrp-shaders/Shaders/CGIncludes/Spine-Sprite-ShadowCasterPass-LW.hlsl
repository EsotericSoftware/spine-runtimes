#ifndef SPRITES_SHADOW_CASTER_PASS_LW_INCLUDED
#define SPRITES_SHADOW_CASTER_PASS_LW_INCLUDED

#include "Spine-Common-ShadowCasterPass-LW.hlsl"

uniform fixed _ShadowAlphaCutoff;

VaryingsSpine ShadowPassVertexSprite(AttributesSpine input)
{
	VaryingsSpine output;
	UNITY_SETUP_INSTANCE_ID(input);

	output.texcoordAndAlpha.xyz = float3(TRANSFORM_TEX(input.texcoord, _MainTex).xy, 0);
	output.positionCS = GetShadowPositionHClip(input);
	output.texcoordAndAlpha.a = input.vertexColor.a * _Color.a;
	return output;
}

half4 ShadowPassFragmentSprite(VaryingsSpine input) : SV_TARGET
{
	fixed4 texureColor = calculateTexturePixel(input.texcoordAndAlpha.xy);
	clip(texureColor.a * input.texcoordAndAlpha.a - _ShadowAlphaCutoff);

	return 0;
}

#endif
