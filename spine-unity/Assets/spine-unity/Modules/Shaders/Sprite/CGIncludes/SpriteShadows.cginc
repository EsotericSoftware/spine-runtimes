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
	float2 texcoord : TEXCOORD1;
};

////////////////////////////////////////
// Vertex program
//

vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	TRANSFER_SHADOW_CASTER(o)
	o.texcoord = calculateTextureCoord(v.texcoord);
	return o;
}

////////////////////////////////////////
// Fragment program
//


uniform fixed _ShadowAlphaCutoff;

fixed4 frag(vertexOutput IN) : COLOR 
{
	fixed4 texureColor = calculateTexturePixel(IN.texcoord);
	clip(texureColor.a - _ShadowAlphaCutoff);
	
	SHADOW_CASTER_FRAGMENT(IN)
}

#endif // SPRITE_SHADOWS_INCLUDED