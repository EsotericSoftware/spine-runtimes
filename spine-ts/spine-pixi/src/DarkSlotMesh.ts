/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { SpineTexture } from "./SpineTexture";
import type { BlendMode, NumberArrayLike } from "@esotericsoftware/spine-core";
import { DarkTintMesh } from "./darkTintMesh/DarkTintMesh";
import type { ISlotMesh } from "./Spine";

export class DarkSlotMesh extends DarkTintMesh implements ISlotMesh {
	public name: string = "";

	private static auxColor = [0, 0, 0, 0];

	constructor () {
		super();
	}
	public updateFromSpineData (
		slotTexture: SpineTexture,
		slotBlendMode: BlendMode,
		slotName: string,
		finalVertices: NumberArrayLike,
		finalVerticesLength: number,
		finalIndices: NumberArrayLike,
		finalIndicesLength: number,
		darkTint: boolean
	): void {
		this.texture = slotTexture.texture;

		const vertLenght = (finalVerticesLength / (darkTint ? 12 : 8)) * 2;

		if (this.geometry.getBuffer("aTextureCoord").data?.length !== vertLenght) {
			this.geometry.getBuffer("aTextureCoord").data = new Float32Array(vertLenght);
		}

		if (this.geometry.getBuffer("aVertexPosition").data?.length !== vertLenght) {
			this.geometry.getBuffer("aVertexPosition").data = new Float32Array(vertLenght);
		}

		let vertIndex = 0;

		for (let i = 0; i < finalVerticesLength; i += darkTint ? 12 : 8) {
			let auxi = i;

			this.geometry.getBuffer("aVertexPosition").data[vertIndex] = finalVertices[auxi++];
			this.geometry.getBuffer("aVertexPosition").data[vertIndex + 1] = finalVertices[auxi++];

			auxi += 4; // color

			this.geometry.getBuffer("aTextureCoord").data[vertIndex] = finalVertices[auxi++];
			this.geometry.getBuffer("aTextureCoord").data[vertIndex + 1] = finalVertices[auxi++];

			vertIndex += 2;
		}

		if (darkTint) {
			DarkSlotMesh.auxColor[0] = finalVertices[8];
			DarkSlotMesh.auxColor[1] = finalVertices[9];
			DarkSlotMesh.auxColor[2] = finalVertices[10];
			DarkSlotMesh.auxColor[3] = finalVertices[11];
			this.darkTint = DarkSlotMesh.auxColor;

			DarkSlotMesh.auxColor[0] = finalVertices[2];
			DarkSlotMesh.auxColor[1] = finalVertices[3];
			DarkSlotMesh.auxColor[2] = finalVertices[4];
			DarkSlotMesh.auxColor[3] = finalVertices[5];
			this.tint = DarkSlotMesh.auxColor;
		} else {
			DarkSlotMesh.auxColor[0] = finalVertices[2];
			DarkSlotMesh.auxColor[1] = finalVertices[3];
			DarkSlotMesh.auxColor[2] = finalVertices[4];
			DarkSlotMesh.auxColor[3] = finalVertices[5];

			this.tint = DarkSlotMesh.auxColor;
		}
		this.blendMode = SpineTexture.toPixiBlending(slotBlendMode);

		if (this.geometry.indexBuffer.data.length !== finalIndices.length) {
			this.geometry.indexBuffer.data = new Uint32Array(finalIndices);
		} else {
			for (let i = 0; i < finalIndicesLength; i++) {
				this.geometry.indexBuffer.data[i] = finalIndices[i];
			}
		}

		this.name = slotName;

		this.geometry.getBuffer("aVertexPosition").update();
		this.geometry.getBuffer("aTextureCoord").update();
		this.geometry.indexBuffer.update();
	}
}
