//Shader written by Alex Dixon
Shader "Spine/Special/SkeletonGhost" 
{
    Properties 
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_TextureFade ("Texture Fade Out", Range(0,1)) = 0
    }
    SubShader 
    {
    
      Tags {"Queue"="Transparent" "IgnoreProjector"="False" "RenderType"="Transparent"}
      Fog { Mode Off }
      Blend One OneMinusSrcAlpha
      ZWrite Off
	  Cull Off
      
        Pass 
        {
            Tags {"LightMode" = "Always"}                      // This Pass tag is important or Unity may not give it the correct light information.
           		CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                //#pragma multi_compile_fwdbase                       // This line tells Unity to compile this pass for forward base.
                
                #include "UnityCG.cginc"
                //#include "AutoLight.cginc"
                    
               	struct vertex_input
               	{
               		float4 vertex : POSITION;
               		float2 texcoord : TEXCOORD0;
					float4 color : COLOR;
               	};
                
                struct vertex_output
                {
                    float4  pos         : SV_POSITION;
                    float2  uv          : TEXCOORD0;
					float4 color : COLOR;
                };
                
                sampler2D _MainTex;
                fixed4 _Color;
				fixed _TextureFade;
                
                vertex_output vert (vertex_input v)
                {
                    vertex_output o;
                    o.pos = mul( UNITY_MATRIX_MVP, v.vertex);
                    o.uv = v.texcoord.xy;
                    o.color = v.color;
					
						         
                    return o;
                }
                
                fixed4 frag(vertex_output i) : COLOR
                {
                    fixed4 tex = tex2D(_MainTex, i.uv);

					tex = fixed4(max(_TextureFade, tex.r), max(_TextureFade, tex.g), max(_TextureFade, tex.b), tex.a);

					return tex * ((i.color * _Color) * tex.a);



					//float finalAlpha = tex.a * i.color.a * _Color.a;

                    /*
                    TODO:  Add basic lighting stuff in later?

                    fixed4 c;
					c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * tex.rgb);       // Ambient term. Only do this in Forward Base. It only needs calculating once.
                    c.rgb += tex.rgb; // Diffuse and specular.
					//Unity 4: c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * tex.rgb * 2);       // Ambient term. Only do this in Forward Base. It only needs calculating once.
					//Unity 4: c.rgb += (tex.rgb * _LightColor0.rgb * diff) * (atten * 2); // Diffuse and specular.
                    c.a = tex.a;  // + _LightColor0.a * atten;

                    return c;
					*/
                }
            ENDCG
        }
             
        
    }
    //FallBack "Transparent/Cutout/VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}