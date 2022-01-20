#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

sampler TextureSampler : register(s0);

// TODO: add effect parameters here.

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

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texColor = tex2D(TextureSampler, input.TextureCoordinate);
	float alpha = texColor.a * input.Color.a;
	float4 output;
	output.a = alpha;
	output.rgb = ((texColor.a - 1.0) * input.Color2.a + 1.0 - texColor.rgb) * input.Color2.rgb + texColor.rgb * input.Color.rgb;

	return output;
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};