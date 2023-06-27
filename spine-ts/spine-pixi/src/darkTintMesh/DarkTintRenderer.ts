import type { IDarkTintElement } from "./DarkTintMesh";
import { DarkTintBatchGeometry } from "./DarkTintBatchGeom";
import type { ExtensionMetadata, Renderer, ViewableBuffer } from "@pixi/core";
import { BatchRenderer, ExtensionType, BatchShaderGenerator, Color } from "@pixi/core";

const vertex = `
precision highp float;
attribute vec2 aVertexPosition;
attribute vec2 aTextureCoord;
attribute vec4 aColor;
attribute vec4 aDarkColor;
attribute float aTextureId;

uniform mat3 projectionMatrix;
uniform mat3 translationMatrix;
uniform vec4 tint;

varying vec2 vTextureCoord;
varying vec4 vColor;
varying vec4 vDarkColor;
varying float vTextureId;

void main(void){
    gl_Position = vec4((projectionMatrix * translationMatrix * vec3(aVertexPosition, 1.0)).xy, 0.0, 1.0);

    vTextureCoord = aTextureCoord;
    vTextureId = aTextureId;
    vColor = aColor * tint;
    vDarkColor = aDarkColor * tint;

}
`;

const fragment = `
varying vec2 vTextureCoord;
varying vec4 vColor;
varying vec4 vDarkColor;
varying float vTextureId;
uniform sampler2D uSamplers[%count%];

void main(void){
    vec4 color;
    %forloop%


    gl_FragColor.a = color.a * vColor.a;
    gl_FragColor.rgb = ((color.a - 1.0) * vDarkColor.a + 1.0 - color.rgb) * vDarkColor.rgb + color.rgb * vColor.rgb;
}
`;

export class DarkTintRenderer extends BatchRenderer {
	public static override extension: ExtensionMetadata = {
		name: "darkTintBatch",
		type: ExtensionType.RendererPlugin,
	};

	constructor(renderer: Renderer) {
		super(renderer);
		this.shaderGenerator = new BatchShaderGenerator(vertex, fragment);
		this.geometryClass = DarkTintBatchGeometry;
		// Pixi's default 6 + 1 for uDarkTint. (this is size in _floats_. color is 4 bytes which roughly equals one float :P )
		this.vertexSize = 7;
	}

	public override packInterleavedGeometry(element: IDarkTintElement, attributeBuffer: ViewableBuffer, indexBuffer: Uint16Array, aIndex: number, iIndex: number): void {
		const { uint32View, float32View } = attributeBuffer;
		const packedVertices = aIndex / this.vertexSize;
		const uvs = element.uvs;
		const indicies = element.indices;
		const vertexData = element.vertexData;
		const textureId = element._texture.baseTexture._batchLocation;
		const alpha = Math.min(element.worldAlpha, 1.0);
		const argb = Color.shared.setValue(element._tintRGB).toPremultiplied(alpha, (element._texture.baseTexture.alphaMode ?? 0) > 0);
		const darkargb = Color.shared.setValue(element._darkTintRGB).toPremultiplied(alpha, (element._texture.baseTexture.alphaMode ?? 0) > 0);

		// lets not worry about tint! for now..
		for (let i = 0; i < vertexData.length; i += 2) {
			float32View[aIndex++] = vertexData[i];
			float32View[aIndex++] = vertexData[i + 1];
			float32View[aIndex++] = uvs[i];
			float32View[aIndex++] = uvs[i + 1];
			uint32View[aIndex++] = argb;
			uint32View[aIndex++] = darkargb;
			float32View[aIndex++] = textureId;
		}
		for (let i = 0; i < indicies.length; i++) {
			indexBuffer[iIndex++] = packedVertices + indicies[i];
		}
	}
}
