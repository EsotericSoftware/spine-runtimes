float4x4 World;
float4x4 View;
float4x4 Projection;

sampler TextureSampler : register(s0);

float _OutlineWidth = 3;
float4 _OutlineColor = float4(1, 1, 0, 1);
float _ThresholdEnd = 0.25;
float _OutlineSmoothness = 1.0;
float _OutlineMipLevel = 0;

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

// Outline pass
#define _USE8NEIGHBOURHOOD_ON

float4 PixelShaderFunctionOutline(VertexShaderOutput i) : COLOR0
{
	float4 texColor = float4(0,0,0,0);

	float uvDeltaPerPixel = max(ddx(i.TextureCoordinate), ddy(i.TextureCoordinate));
	float outlineWidthCompensated = _OutlineWidth * uvDeltaPerPixel;
	float xOffset = outlineWidthCompensated;
	float yOffset = outlineWidthCompensated;
	float xOffsetDiagonal = outlineWidthCompensated * 0.7;
	float yOffsetDiagonal = outlineWidthCompensated * 0.7;

	float pixelCenter = tex2D(TextureSampler, i.TextureCoordinate).a;

	float4 uvCenterWithLod = float4(i.TextureCoordinate.xy, 0, _OutlineMipLevel);
	float pixelTop = tex2Dlod(TextureSampler, uvCenterWithLod + float4(0,  yOffset, 0, 0)).a;
	float pixelBottom = tex2Dlod(TextureSampler, uvCenterWithLod + float4(0, -yOffset, 0, 0)).a;
	float pixelLeft = tex2Dlod(TextureSampler, uvCenterWithLod + float4(-xOffset, 0, 0, 0)).a;
	float pixelRight = tex2Dlod(TextureSampler, uvCenterWithLod + float4(xOffset, 0, 0, 0)).a;
#ifdef _USE8NEIGHBOURHOOD_ON
	float numSamples = 8;
	float pixelTopLeft = tex2Dlod(TextureSampler, uvCenterWithLod + float4(-xOffsetDiagonal, yOffsetDiagonal, 0, 0)).a;
	float pixelTopRight = tex2Dlod(TextureSampler, uvCenterWithLod + float4(xOffsetDiagonal, yOffsetDiagonal, 0, 0)).a;
	float pixelBottomLeft = tex2Dlod(TextureSampler, uvCenterWithLod + float4(-xOffsetDiagonal, -yOffsetDiagonal, 0, 0)).a;
	float pixelBottomRight = tex2Dlod(TextureSampler, uvCenterWithLod + float4(xOffsetDiagonal, -yOffsetDiagonal, 0, 0)).a;
	float average = (pixelTop + pixelBottom + pixelLeft + pixelRight +
		pixelTopLeft + pixelTopRight + pixelBottomLeft + pixelBottomRight)
		* i.Color.a / numSamples;
#else // 4 neighbourhood
	float numSamples = 1;
	float average = (pixelTop + pixelBottom + pixelLeft + pixelRight) * i.Color.a / numSamples;
#endif

	float thresholdStart = _ThresholdEnd * (1.0 - _OutlineSmoothness);
	float outlineAlpha = saturate((average - thresholdStart) / (_ThresholdEnd - thresholdStart)) - pixelCenter;
	texColor.rgba = lerp(texColor, _OutlineColor, outlineAlpha);
	return texColor;
}

technique Technique1
{
    pass OutlinePass
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionOutline();
    }
	pass Pass2
	{
		// TODO: set renderstates here.

		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
