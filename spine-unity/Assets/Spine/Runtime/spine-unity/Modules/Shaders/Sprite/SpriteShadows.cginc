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
	float4 texcoordAndAlpha : TEXCOORD1;
};

////////////////////////////////////////
// Vertex program
//

uniform sampler2D _MainTex;
uniform fixed4 _MainTex_ST;

vertexOutput vert(vertexInput v, float4 vertexColor : COLOR)
{
	vertexOutput o;
	TRANSFER_SHADOW_CASTER(o)
	o.texcoordAndAlpha.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.texcoordAndAlpha.z = 0;
	o.texcoordAndAlpha.a = vertexColor.a;
	return o;
}

////////////////////////////////////////
// Fragment program
//


uniform fixed _ShadowAlphaCutoff;

fixed4 frag(vertexOutput IN) : COLOR 
{
	fixed4 texureColor = tex2D(_MainTex, IN.texcoordAndAlpha.xy);
	clip(texureColor.a * IN.texcoordAndAlpha.a - _ShadowAlphaCutoff);
	
	SHADOW_CASTER_FRAGMENT(IN)
}

#endif // SPRITE_SHADOWS_INCLUDED