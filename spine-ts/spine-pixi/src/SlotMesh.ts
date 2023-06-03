import { SpineTexture } from "./SpineTexture";
import type { BlendMode, NumberArrayLike } from "@esotericsoftware/spine-core";
import type { ISlotMesh } from "./Spine";
import { Mesh, MeshGeometry, MeshMaterial } from "@pixi/mesh";
import { Texture } from "@pixi/core";

export class SlotMesh extends Mesh implements ISlotMesh {
	public name: string = "";

	private static readonly auxColor = [0, 0, 0, 0];
	private warnedTwoTint: boolean = false;

	constructor() {
		const geometry = new MeshGeometry();

		geometry.getBuffer("aVertexPosition").static = false;
		geometry.getBuffer("aTextureCoord").static = false;

		const meshMaterial = new MeshMaterial(Texture.EMPTY);
		super(geometry, meshMaterial);
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

		// console.log(vertLenght, auxVert.length);

		if (darkTint && !this.warnedTwoTint) {
			console.warn("DarkTint is not enabled by default. To enable use a DarkSlotMesh factory while creating the Spine object.");
			this.warnedTwoTint = true;
		}

		SlotMesh.auxColor[0] = finalVertices[2];
		SlotMesh.auxColor[1] = finalVertices[3];
		SlotMesh.auxColor[2] = finalVertices[4];
		SlotMesh.auxColor[3] = finalVertices[5];

		this.tint = SlotMesh.auxColor;
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
