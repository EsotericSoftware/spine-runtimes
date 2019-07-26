#ifndef SKELETON_LIT_COMMON_SHADOW_INCLUDED
#define SKELETON_LIT_COMMON_SHADOW_INCLUDED

#include "UnityCG.cginc"
struct v2f { 
	V2F_SHADOW_CASTER;
	float4 uvAndAlpha : TEXCOORD1;
};

uniform float4 _MainTex_ST;

v2f vertShadow(appdata_base v, float4 vertexColor : COLOR) {
	v2f o;
	TRANSFER_SHADOW_CASTER(o)
	o.uvAndAlpha.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.uvAndAlpha.z = 0;
	o.uvAndAlpha.a = vertexColor.a;
	return o;
}

uniform sampler2D _MainTex;
uniform fixed SHADOW_CUTOFF;

float4 fragShadow (v2f i) : SV_Target {
	fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
	clip(texcol.a * i.uvAndAlpha.a - SHADOW_CUTOFF);
	SHADOW_CASTER_FRAGMENT(i)
}

#endif
