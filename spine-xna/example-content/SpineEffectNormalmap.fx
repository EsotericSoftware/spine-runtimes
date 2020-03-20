float4x4 World;
float4x4 View;
float4x4 Projection;

// Light0 parameters.
// Default values set below, change them via spineEffect.Parameters["Light0_Direction"] and similar.
float3 Light0_Direction = float3(-0.5265408f, -0.5735765f, -0.6275069f);
float3 Light0_Diffuse = float3(1, 1, 1);
float3 Light0_Specular = float3(1, 1, 1);
float Light0_SpecularExponent = 2.0; // also called "shininess", "specular hardness"

sampler TextureSampler : register(s0);
sampler NormalmapSampler : register(s1);

// TODO: add effect parameters here.

float NormalmapIntensity = 1;

float3 GetNormal(sampler normalmapSampler, float2 uv, float3 worldPos, float3 vertexNormal)
{
	// Reconstruct tangent space TBN matrix
	float3 pos_dx = ddx(worldPos);
	float3 pos_dy = ddy(worldPos);
	float3 tex_dx = float3(ddx(uv), 0.0);
	float3 tex_dy = float3(ddy(uv), 0.0);
	float divisor = (tex_dx.x * tex_dy.y - tex_dy.x * tex_dx.y);
	float3 t = (tex_dy.y * pos_dx - tex_dx.y * pos_dy) / divisor;

	float divisorBinormal = (tex_dy.y * tex_dx.x - tex_dx.y * tex_dy.x);
	float3 b = (tex_dx.x * pos_dy - tex_dy.x * pos_dx) / divisorBinormal;

	t = normalize(t - vertexNormal * dot(vertexNormal, t));
	b = normalize(b - vertexNormal * dot(vertexNormal, b));
	float3x3 tbn = float3x3(t, b, vertexNormal);

	float3 n = 2.0 * tex2D(normalmapSampler, uv).rgb - 1.0;
#ifdef INVERT_NORMALMAP_Y
	n.y = -n.y;
#endif
	n = normalize(mul(n * float3(NormalmapIntensity, NormalmapIntensity, 1.0), tbn));
	return n;
}

void GetLightContributionBlinnPhong(inout float3 diffuseResult, inout float3 specularResult,
	float3 lightDirection, float3 lightDiffuse, float3 lightSpecular, float specularExponent, float3 normal, float3 viewDirection)
{
	diffuseResult += lightDiffuse * max(0.0, dot(normal, -lightDirection));
	half3 halfVector = normalize(-lightDirection + viewDirection);
	float nDotH = max(0, dot(normal, halfVector));
	specularResult += lightSpecular * pow(nDotH, specularExponent);
}

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 TextureCoordinate : TEXCOORD0;
	float4 Color2 : COLOR1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float4 TextureCoordinate : TEXCOORD0;
	float4 Color2 : COLOR1;
	float3 WorldNormal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2; // for tangent reconstruction
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.TextureCoordinate = input.TextureCoordinate;
	output.Color = input.Color;
	output.Color2 = input.Color2;

	output.WorldNormal = mul(transpose(View), float4(0, 0, 1, 0)).xyz;
	output.WorldPosition = worldPosition;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texColor = tex2D(TextureSampler, input.TextureCoordinate);
	float3 normal = GetNormal(NormalmapSampler, input.TextureCoordinate, input.WorldPosition, input.WorldNormal);
	float3 viewDirection = -input.WorldNormal;

	float alpha = texColor.a * input.Color.a;
	float4 output;
	output.a = alpha;
	output.rgb = ((texColor.a - 1.0) * input.Color2.a + 1.0 - texColor.rgb) * input.Color2.rgb + texColor.rgb * input.Color.rgb;

	float3 diffuseLight = float3(0, 0, 0);
	float3 specularLight = float3(0, 0, 0);
	GetLightContributionBlinnPhong(diffuseLight, specularLight,
		Light0_Direction, Light0_Diffuse, Light0_Specular, Light0_SpecularExponent, normal, viewDirection);
	output.rgb = diffuseLight * output.rgb + specularLight;
	return output;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
