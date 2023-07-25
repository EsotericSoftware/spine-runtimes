#ifndef SPINE_OUTLINE_COMMON_INCLUDED
#define SPINE_OUTLINE_COMMON_INCLUDED

float4 computeOutlinePixel(sampler2D mainTexture, float2 mainTextureTexelSize,
	float2 uv, float vertexColorAlpha,
	float OutlineWidth, float OutlineReferenceTexWidth, float OutlineMipLevel,
	float OutlineSmoothness, float ThresholdEnd, float OutlineOpaqueAlpha, float4 OutlineColor) {

	float4 texColor = fixed4(0, 0, 0, 0);

	float outlineWidthCompensated = OutlineWidth / (OutlineReferenceTexWidth * mainTextureTexelSize.x);
	float xOffset = mainTextureTexelSize.x * outlineWidthCompensated;
	float yOffset = mainTextureTexelSize.y * outlineWidthCompensated;
	float xOffsetDiagonal = mainTextureTexelSize.x * outlineWidthCompensated * 0.7;
	float yOffsetDiagonal = mainTextureTexelSize.y * outlineWidthCompensated * 0.7;

	float pixelCenter = tex2D(mainTexture, uv).a;

	float4 uvCenterWithLod = float4(uv, 0, OutlineMipLevel);
	float pixelTop = tex2Dlod(mainTexture, uvCenterWithLod + float4(0, yOffset, 0, 0)).a;
	float pixelBottom = tex2Dlod(mainTexture, uvCenterWithLod + float4(0, -yOffset, 0, 0)).a;
	float pixelLeft = tex2Dlod(mainTexture, uvCenterWithLod + float4(-xOffset, 0, 0, 0)).a;
	float pixelRight = tex2Dlod(mainTexture, uvCenterWithLod + float4(xOffset, 0, 0, 0)).a;
#if _USE8NEIGHBOURHOOD_ON
	float numSamples = 8;
	float pixelTopLeft = tex2Dlod(mainTexture, uvCenterWithLod + float4(-xOffsetDiagonal, yOffsetDiagonal, 0, 0)).a;
	float pixelTopRight = tex2Dlod(mainTexture, uvCenterWithLod + float4(xOffsetDiagonal, yOffsetDiagonal, 0, 0)).a;
	float pixelBottomLeft = tex2Dlod(mainTexture, uvCenterWithLod + float4(-xOffsetDiagonal, -yOffsetDiagonal, 0, 0)).a;
	float pixelBottomRight = tex2Dlod(mainTexture, uvCenterWithLod + float4(xOffsetDiagonal, -yOffsetDiagonal, 0, 0)).a;
	float average = (pixelTop + pixelBottom + pixelLeft + pixelRight +
		pixelTopLeft + pixelTopRight + pixelBottomLeft + pixelBottomRight)
		* vertexColorAlpha / numSamples;
#else // 4 neighbourhood
	float numSamples = 4;
	float average = (pixelTop + pixelBottom + pixelLeft + pixelRight) * vertexColorAlpha / numSamples;
#endif
	float thresholdStart = ThresholdEnd * (1.0 - OutlineSmoothness);
	float outlineAlpha = saturate(saturate((average - thresholdStart) / (ThresholdEnd - thresholdStart)) - pixelCenter);
	outlineAlpha = pixelCenter > OutlineOpaqueAlpha ? 0 : outlineAlpha;
	return lerp(texColor, OutlineColor, outlineAlpha);
}

float4 computeOutlinePixel(sampler2D mainTexture, float2 mainTextureTexelSize,
	float2 uv, float vertexColorAlpha,
	float OutlineWidth, float OutlineReferenceTexWidth, float OutlineMipLevel,
	float OutlineSmoothness, float ThresholdEnd, float4 OutlineColor) {

	return computeOutlinePixel(mainTexture, mainTextureTexelSize,
		uv, vertexColorAlpha, OutlineWidth, OutlineReferenceTexWidth, OutlineMipLevel,
		OutlineSmoothness, ThresholdEnd, 1.0, OutlineColor);
}

#endif
