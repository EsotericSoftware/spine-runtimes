/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import type { IDarkTintElement } from "./DarkTintMesh.js";
import { DarkTintBatchGeometry } from "./DarkTintBatchGeom.js";
import type { ExtensionMetadata, Renderer, ViewableBuffer } from "@pixi/core";
import { extensions, BatchRenderer, ExtensionType, BatchShaderGenerator, Color } from "@pixi/core";

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
		const worldAlpha = Math.min(element.worldAlpha, 1.0);
		const argb = Color.shared.setValue(element._tintRGB).toPremultiplied(worldAlpha, true);
		const darkargb = Color.shared.setValue(element._darkTintRGB).premultiply(worldAlpha, true).toPremultiplied(1, false);

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

extensions.add(DarkTintRenderer);