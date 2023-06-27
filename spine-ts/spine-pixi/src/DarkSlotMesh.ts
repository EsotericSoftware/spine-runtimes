import { SpineTexture } from "./SpineTexture";
import type { BlendMode, NumberArrayLike } from "@esotericsoftware/spine-core";
import { DarkTintMesh } from "./darkTintMesh/DarkTintMesh";
import type { ISlotMesh } from "./Spine";

export class DarkSlotMesh extends DarkTintMesh implements ISlotMesh {
	public name: string = "";

	private static auxColor = [0, 0, 0, 0];

	constructor() {
		super();
	}
	public updateFromSpineData(
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
