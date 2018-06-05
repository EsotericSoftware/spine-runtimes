#ifndef SPRITE_SHADOWS_INCLUDED
#define SPRITE_SHADOWS_INCLUDED

#include "UnityCG.cginc"

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
	float4 texcoord : TEXCOORD1;
};

////////////////////////////////////////
// Vertex program
//

uniform sampler2D _MainTex;
uniform fixed4 _MainTex_ST;

vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	TRANSFER_SHADOW_CASTER(o)
	o.texcoord = float4(TRANSFORM_TEX(v.texcoord, _MainTex), 0, 0);
	return o;
}

////////////////////////////////////////
// Fragment program
//


uniform fixed _ShadowAlphaCutoff;

fixed4 frag(vertexOutput IN) : COLOR 
{
	fixed4 texureColor = tex2D(_MainTex, IN.texcoord.xy);
	clip(texureColor.a - _ShadowAlphaCutoff);
	
	SHADOW_CASTER_FRAGMENT(IN)
}

#endif // SPRITE_SHADOWS_INCLUDED