#ifndef SPRITE_UNLIT_INCLUDED
#define SPRITE_UNLIT_INCLUDED

#include "ShaderShared.cginc"
#if defined(_ALPHAPREMULTIPLY_ON)
	#undef _STRAIGHT_ALPHA_INPUT
#else
	#define _STRAIGHT_ALPHA_INPUT
#endif
#include "../../CGIncludes/Spine-Skeleton-Tint-Common.cginc"

////////////////////////////////////////
// Vertex structs
//

struct VertexInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
#if defined(_TINT_BLACK_ON)
	float2 tintBlackRG : TEXCOORD1;
	float2 tintBlackB : TEXCOORD2;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
	float4 pos : SV_POSITION;
	float2 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
#if defined(_FOG)
	UNITY_FOG_COORDS(1)
#endif // _FOG

#if defined(_TINT_BLACK_ON)
	float3 darkColor : TEXCOORD2;
#endif

	UNITY_VERTEX_OUTPUT_STEREO
};

////////////////////////////////////////
// Vertex program
//

VertexOutput vert(VertexInput input)
{
	VertexOutput output;

	UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.pos = calculateLocalPos(input.vertex);
	output.texcoord = calculateTextureCoord(input.texcoord);
	output.color = calculateVertexColor(input.color);
#if defined(_TINT_BLACK_ON)
	output.darkColor = GammaToTargetSpace(half3(input.tintBlackRG.r, input.tintBlackRG.g, input.tintBlackB.r))
		+ (_Black.rgb * input.color.a);
#endif

#if defined(_FOG)
	UNITY_TRANSFER_FOG(output,output.pos);
#endif // _FOG

	return output;
}

////////////////////////////////////////
// Fragment program
//
fixed4 frag(VertexOutput input) : SV_Target
{
	fixed4 texureColor = calculateTexturePixel(input.texcoord.xy);
	RETURN_UNLIT_IF_ADDITIVE_SLOT_TINT(texureColor, input.color, input.darkColor, _Color.a, _Black.a) // shall be called before ALPHA_CLIP
	ALPHA_CLIP(texureColor, input.color)

#if defined(_TINT_BLACK_ON)
	texureColor = fragTintedColor(texureColor, input.darkColor, input.color, _Color.a, _Black.a);
#endif

	fixed4 pixel = calculatePixel(texureColor, input.color);

	COLORISE(pixel)
	APPLY_FOG(pixel, input)

	return pixel;
}

#endif // SPRITE_UNLIT_INCLUDED
