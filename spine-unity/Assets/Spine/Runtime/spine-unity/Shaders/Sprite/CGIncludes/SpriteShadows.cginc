#ifndef SPRITE_SHADOWS_INCLUDED
#define SPRITE_SHADOWS_INCLUDED

#include "ShaderShared.cginc"

////////////////////////////////////////
// Vertex structs
//

struct vertexInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
};

struct vertexOutput
{ 
	V2F_SHADOW_CASTER;
	float4 texcoordAndAlpha : TEXCOORD1;
};

////////////////////////////////////////
// Vertex program
//

vertexOutput vert(vertexInput v, float4 vertexColor : COLOR)
{
	vertexOutput o;
	TRANSFER_SHADOW_CASTER(o)
	o.texcoordAndAlpha.xy = calculateTextureCoord(v.texcoord);
	o.texcoordAndAlpha.z = 0;
	o.texcoordAndAlpha.a = vertexColor.a;
	return o;
}

////////////////////////////////////////
// Fragment program
//


uniform fixed _ShadowAlphaCutoff;

fixed4 frag(vertexOutput IN) : SV_Target
{
	fixed4 texureColor = calculateTexturePixel(IN.texcoordAndAlpha.xy);
	clip(texureColor.a * IN.texcoordAndAlpha.a - _ShadowAlphaCutoff);
	
	SHADOW_CASTER_FRAGMENT(IN)
}

#endif // SPRITE_SHADOWS_INCLUDED