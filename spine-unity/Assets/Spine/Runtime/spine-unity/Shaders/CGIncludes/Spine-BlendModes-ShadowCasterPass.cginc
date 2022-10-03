#ifndef BLENDMODES_SHADOWCASTER_PASS_INCLUDED
#define BLENDMODES_SHADOWCASTER_PASS_INCLUDED

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_shadowcaster
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"
struct v2f {
	V2F_SHADOW_CASTER;
	float4 uvAndAlpha : TEXCOORD1;
};

uniform float4 _MainTex_ST;

v2f vert(appdata_base v, float4 vertexColor : COLOR) {
	v2f o;
	TRANSFER_SHADOW_CASTER(o)
		o.uvAndAlpha.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.uvAndAlpha.z = 0;
	o.uvAndAlpha.a = vertexColor.a;
	return o;
}

uniform sampler2D _MainTex;
uniform fixed _Cutoff;

float4 frag(v2f i) : SV_Target{
	fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
	clip(texcol.a* i.uvAndAlpha.a - _Cutoff);
	SHADOW_CASTER_FRAGMENT(i)
}

#endif
