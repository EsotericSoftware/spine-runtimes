/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import {
	BlendMode,
	ClippingAttachment,
	MeshAttachment,
	type NumberArrayLike,
	RegionAttachment,
	SkeletonClipping,
	type Slot,
	type TextureAtlasRegion,
} from "@esotericsoftware/spine-core";
import * as Phaser from "phaser";
import type { SpineGameObject, SpineSlotObjectEntry } from "../SpineGameObject.js";
import type { SpineGameObjectRenderer } from "./SpineGameObjectRenderer.js";

const REGION_TRIANGLES = [0, 1, 2, 0, 2, 3];

type Mesh2DObject = Phaser.GameObjects.Mesh2D & {
	renderWebGL: (renderer: Phaser.Renderer.WebGL.WebGLRenderer, src: Mesh2DObject, drawingContext: Phaser.Renderer.WebGL.DrawingContext, parentMatrix?: Phaser.GameObjects.Components.TransformMatrix) => void;
	setTint: (color: number) => Mesh2DObject;
	setTint2: (color: number) => Mesh2DObject;
	setTintMode: (mode: number) => Mesh2DObject;
	setFlipV: (value: boolean) => Mesh2DObject;
};

interface SlotMeshState {
	mesh: Mesh2DObject;
	attachment: RegionAttachment | MeshAttachment | null;
	triangles: NumberArrayLike | Uint16Array | null;
	trianglesLength: number;
	worldVerticesLength: number;
	clipped: boolean;
}

interface ClipStencilState {
	stencil: Phaser.GameObjects.Stencil;
	graphics: Phaser.GameObjects.Graphics;
	inverse: boolean;
	version: number;
}

type BlendModeGameObject = Phaser.GameObjects.GameObject & { blendMode: number };

export class PhaserMesh2DRenderer implements SpineGameObjectRenderer {
	readonly type = "phaser";
	private meshStates = new Map<Slot, SlotMeshState>();
	private clipper = new SkeletonClipping();
	private activeClipSlot: Slot | null = null;
	private activeClipAttachment: ClippingAttachment | null = null;
	private activeClipVersion = 0;
	private clipStencilState: ClipStencilState | null = null;
	private clipStencilPoints: Phaser.Math.Vector2[] = [];
	private worldVertices = new Float32Array(8);
	private spineRenderMatrix: Phaser.GameObjects.Components.TransformMatrix = new Phaser.GameObjects.Components.TransformMatrix();
	private boneRenderMatrix: Phaser.GameObjects.Components.TransformMatrix = new Phaser.GameObjects.Components.TransformMatrix();
	private slotObjectRenderMatrix: Phaser.GameObjects.Components.TransformMatrix = new Phaser.GameObjects.Components.TransformMatrix();

	constructor (private owner: SpineGameObject) { }

	renderWebGL (renderer: Phaser.Renderer.WebGL.WebGLRenderer, src: SpineGameObject, drawingContext: Phaser.Renderer.WebGL.DrawingContext, parentMatrix?: Phaser.GameObjects.Components.TransformMatrix) {
		drawingContext.camera?.addToRenderList(src);
		const baseContext = drawingContext;
		let currentContext = baseContext;

		for (const slot of src.skeleton.drawOrder.appliedPose) {
			const pose = slot.getAppliedPose();
			const attachment = pose.getAttachment();
			const slotObject = src.getSlotObjectEntry(slot);
			if (!slot.bone.isActive()) {
				this.endClip(slot);
				continue;
			}

			if (slotObject?.placement === "before" && (attachment || !slotObject.followAttachmentTimeline)) {
				this.renderSlotObject(renderer, slot, slotObject, currentContext, parentMatrix);
			}

			if (!attachment) {
				if (slotObject?.placement === "after" && !slotObject.followAttachmentTimeline) this.renderSlotObject(renderer, slot, slotObject, currentContext, parentMatrix);
				this.endClip(slot);
				continue;
			}

			if (attachment instanceof ClippingAttachment) {
				this.startClip(slot, attachment);
				if (slotObject?.placement === "after") this.renderSlotObject(renderer, slot, slotObject, currentContext, parentMatrix);
				continue;
			}

			let region: TextureAtlasRegion | null = null;
			let world: NumberArrayLike;
			let uvs: NumberArrayLike;
			let triangles: NumberArrayLike | Uint16Array;
			let vertexCount = 0;
			let clipped = false;

			if (attachment instanceof RegionAttachment) {
				const sequenceIndex = attachment.sequence.resolveIndex(pose);
				region = attachment.sequence.regions[sequenceIndex] as TextureAtlasRegion;
				if (this.worldVertices.length < 8) this.worldVertices = new Float32Array(8);
				attachment.computeWorldVertices(slot, attachment.getOffsets(pose), this.worldVertices, 0, 2);
				world = this.worldVertices;
				uvs = attachment.sequence.getUVs(sequenceIndex);
				triangles = REGION_TRIANGLES;
				vertexCount = 4;
			} else if (attachment instanceof MeshAttachment) {
				const sequenceIndex = attachment.sequence.resolveIndex(pose);
				region = attachment.sequence.regions[sequenceIndex] as TextureAtlasRegion;
				if (this.worldVertices.length < attachment.worldVerticesLength) this.worldVertices = new Float32Array(attachment.worldVerticesLength);
				attachment.computeWorldVertices(src.skeleton, slot, 0, attachment.worldVerticesLength, this.worldVertices, 0, 2);
				world = this.worldVertices;
				uvs = attachment.sequence.getUVs(sequenceIndex);
				triangles = attachment.triangles;
				vertexCount = attachment.worldVerticesLength / 2;
			} else {
				this.endClip(slot);
				continue;
			}

			if (!region?.page) {
				this.endClip(slot);
				continue;
			}

			let indicesCount = triangles.length;
			if (this.clipper.isClipping()) {
				this.clipper.clipTrianglesUnpacked(world, 0, triangles, triangles.length, uvs, 2);
				world = this.clipper.clippedVerticesTyped;
				uvs = this.clipper.clippedUVsTyped;
				triangles = this.clipper.clippedTrianglesTyped;
				vertexCount = this.clipper.clippedVerticesLength >> 1;
				indicesCount = this.clipper.clippedTrianglesLength;
				clipped = true;
				if (vertexCount === 0 || indicesCount === 0) {
					this.endClip(slot);
					continue;
				}
			}

			const meshState = this.getMeshStateForSlot(slot);
			const mesh = meshState.mesh;
			mesh.vertices.length = 0;
			mesh.setTexture(`${src.plugin.atlasKeyForSkeleton(src.skeleton)}!${region.page.name}`);
			mesh.blendMode = this.toPhaserBlendMode(slot.data.blendMode);

			const offsetX = src.renderOffsetX;
			const offsetY = src.renderOffsetY;
			for (let i = 0; i < vertexCount; i++) {
				mesh.vertices.push(world[i * 2] + offsetX, world[i * 2 + 1] + offsetY, uvs[i * 2], uvs[i * 2 + 1]);
			}

			if (clipped) {
				mesh.indices.length = 0;
				for (let i = 0; i < indicesCount; i += 3) mesh.indices.push(triangles[i], triangles[i + 1], triangles[i + 2], 0);
				mesh.setRenderAsTriangles(false);
				mesh.setUseOrderedIndices(false);
				meshState.attachment = attachment;
				meshState.triangles = null;
				meshState.trianglesLength = indicesCount;
				meshState.worldVerticesLength = vertexCount * 2;
				meshState.clipped = true;
			} else {
				const worldVerticesLength = vertexCount * 2;
				const topologyDirty = meshState.clipped || meshState.attachment !== attachment || meshState.triangles !== triangles || meshState.trianglesLength !== indicesCount || meshState.worldVerticesLength !== worldVerticesLength;
				if (topologyDirty) {
					mesh.indices.length = 0;
					for (let i = 0; i < indicesCount; i += 3) mesh.indices.push(triangles[i], triangles[i + 1], triangles[i + 2], 0);
					mesh.setRenderAsTriangles(false);
					mesh.buildOrderedIndices(2, true);
					meshState.attachment = attachment;
					meshState.triangles = triangles;
					meshState.trianglesLength = indicesCount;
					meshState.worldVerticesLength = worldVerticesLength;
					meshState.clipped = false;
				} else {
					mesh.setUseOrderedIndices(true);
				}
			}

			this.applyMeshState(mesh, slot, attachment);
			currentContext = this.renderMesh(renderer, mesh, baseContext, currentContext, parentMatrix);

			if (slotObject?.placement === "after") {
				this.renderSlotObject(renderer, slot, slotObject, currentContext, parentMatrix);
			}

			this.endClip(slot);
		}

		if (currentContext !== baseContext) {
			renderer.renderNodes.finishBatch();
			currentContext.release();
			baseContext.beginDraw();
		}
		this.endClip();
	}

	preDestroy () {
		for (const meshState of this.meshStates.values()) {
			meshState.mesh.destroy();
		}
		this.meshStates.clear();
		this.clipStencilState?.stencil.destroy();
		this.clipStencilState = null;
		this.clipStencilPoints.length = 0;
	}

	private startClip (slot: Slot, attachment: ClippingAttachment): void {
		this.clipper.clipEnd(slot);
		if (this.clipper.isClipping()) return;
		this.clipper.clipStart(this.owner.skeleton, slot, attachment);
		this.activeClipSlot = slot;
		this.activeClipAttachment = attachment;
		this.activeClipVersion++;
	}

	private endClip (slot?: Slot): void {
		this.clipper.clipEnd(slot);
		if (!this.clipper.isClipping()) {
			this.activeClipSlot = null;
			this.activeClipAttachment = null;
		}
	}

	private getActiveClipStencil (): ClipStencilState {
		if (!this.activeClipSlot || !this.activeClipAttachment) {
			throw new Error("Active Spine clip stencil requested without active clip attachment.");
		}

		let state = this.clipStencilState;
		if (!state) {
			const graphics = new Phaser.GameObjects.Graphics(this.owner.scene);
			const stencil = new Phaser.GameObjects.Stencil(this.owner.scene, 0, 0, [graphics], {
				stencilInvert: false,
				stencilCompositeCheck: false,
				stencilValueWrap: false,
			});
			state = this.clipStencilState = { stencil, graphics, inverse: false, version: -1 };
		}

		if (state.version !== this.activeClipVersion) {
			const attachment = this.activeClipAttachment;

			const vertices = this.clipStencilPoints;
			const length = attachment.worldVerticesLength;
			vertices.length = length >> 1;
			if (this.worldVertices.length < length) this.worldVertices = new Float32Array(length);
			attachment.computeWorldVertices(this.owner.skeleton, this.activeClipSlot, 0, length, this.worldVertices, 0, 2);
			const offsetX = this.owner.renderOffsetX;
			const offsetY = this.owner.renderOffsetY;
			for (let i = 0, p = 0; i < length; i += 2, p++) {
				let point = vertices[p];
				if (!point) point = vertices[p] = new Phaser.Math.Vector2();
				point.x = this.worldVertices[i] + offsetX;
				point.y = this.worldVertices[i + 1] + offsetY;
			}

			state.graphics.clear();
			state.graphics.fillStyle(0xffffff, 1);
			state.graphics.fillPoints(vertices, true, true);
			state.inverse = attachment.inverse;
			state.version = this.activeClipVersion;
			state.stencil.stencilInvert = false;
		}

		return state;
	}

	private renderClipStencil (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		stencil: Phaser.GameObjects.Stencil,
		drawingContext: Phaser.Renderer.WebGL.DrawingContext,
		parentMatrix: Phaser.GameObjects.Components.TransformMatrix | undefined,
		mode: "addLayer" | "subtractLayer"
	): void {
		stencil.stencilLayerMode = mode;
		const matrix = this.applySpineObjectRenderMatrix(this.spineRenderMatrix, parentMatrix);
		stencil.renderWebGLStep(renderer, stencil, drawingContext, matrix, 0, [stencil], 0);
		renderer.renderNodes.finishBatch();
	}

	private applySpineObjectRenderMatrix (
		matrix: Phaser.GameObjects.Components.TransformMatrix,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix
	): Phaser.GameObjects.Components.TransformMatrix {
		const owner = this.owner;
		const scaleX = owner.scaleX * (owner.flipX ? -1 : 1);
		const scaleY = owner.scaleY * (owner.flipY ? -1 : 1);
		if (parentMatrix) {
			matrix.copyFrom(parentMatrix);
			matrix.translate(owner.x, owner.y);
			matrix.rotate(owner.rotation);
			matrix.scale(scaleX, scaleY);
		} else {
			matrix.applyITRS(owner.x, owner.y, owner.rotation, scaleX, scaleY);
		}
		return matrix;
	}

	private getSlotObjectRenderMatrix (slot: Slot, parentMatrix?: Phaser.GameObjects.Components.TransformMatrix): Phaser.GameObjects.Components.TransformMatrix {
		const bone = slot.bone.appliedPose;
		const boneMatrix = this.boneRenderMatrix.matrix;
		boneMatrix[0] = bone.a;
		boneMatrix[1] = bone.c;
		boneMatrix[2] = -bone.b;
		boneMatrix[3] = -bone.d;
		boneMatrix[4] = bone.worldX;
		boneMatrix[5] = bone.worldY;

		const matrix = this.applySpineObjectRenderMatrix(this.slotObjectRenderMatrix, parentMatrix);
		matrix.translate(this.owner.renderOffsetX, this.owner.renderOffsetY);
		matrix.multiply(this.boneRenderMatrix);
		return matrix;
	}

	private renderSlotObject (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		slot: Slot,
		entry: SpineSlotObjectEntry,
		drawingContext: Phaser.Renderer.WebGL.DrawingContext,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix
	): void {
		const matrix = this.getSlotObjectRenderMatrix(slot, parentMatrix);
		if (!entry.clipping || !this.clipper.isClipping()) {
			this.renderPhaserGameObject(renderer, entry.gameObject, drawingContext, matrix);
			return;
		}

		const clipStencil = this.getActiveClipStencil();

		this.renderClipStencil(renderer, clipStencil.stencil, drawingContext, parentMatrix, "addLayer");

		const gl = renderer.gl;
		const readContext = drawingContext.getClone();
		readContext.setStencil(true, clipStencil.inverse ? gl.EQUAL : gl.NOTEQUAL, 0, 0xff, gl.KEEP, gl.KEEP, gl.KEEP, 0, 0x00);
		readContext.beginDraw();

		this.renderPhaserGameObject(renderer, entry.gameObject, readContext, matrix);

		renderer.renderNodes.finishBatch();
		readContext.release();

		this.renderClipStencil(renderer, clipStencil.stencil, drawingContext, parentMatrix, "subtractLayer");
	}

	private renderMesh (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		mesh: Mesh2DObject,
		baseContext: Phaser.Renderer.WebGL.DrawingContext,
		currentContext: Phaser.Renderer.WebGL.DrawingContext,
		parentMatrix?: Phaser.GameObjects.Components.TransformMatrix
	): Phaser.Renderer.WebGL.DrawingContext {
		const blendMode = mesh.blendMode as number;
		let context = currentContext;
		if (blendMode !== Phaser.BlendModes.SKIP_CHECK && blendMode !== currentContext.blendMode) {
			if (currentContext !== baseContext) {
				renderer.renderNodes.finishBatch();
				currentContext.release();
				baseContext.beginDraw();
			}

			context = baseContext;
			if (blendMode !== baseContext.blendMode) {
				renderer.renderNodes.finishBatch();
				context = baseContext.getClone();
				context.setBlendMode(blendMode);
				context.use();
			}
		}

		mesh.renderWebGL(renderer, mesh, context, parentMatrix);
		return context;
	}

	private renderPhaserGameObject (
		renderer: Phaser.Renderer.WebGL.WebGLRenderer,
		gameObject: Phaser.GameObjects.GameObject,
		drawingContext: Phaser.Renderer.WebGL.DrawingContext,
		parentMatrix: Phaser.GameObjects.Components.TransformMatrix
	): void {
		const camera = drawingContext.camera;
		if (!camera) return;

		if (!gameObject.willRender(camera)) return;

		const blendMode = (gameObject as BlendModeGameObject).blendMode;
		let context = drawingContext;
		if (blendMode !== Phaser.BlendModes.SKIP_CHECK && blendMode !== drawingContext.blendMode) {
			renderer.renderNodes.finishBatch();
			context = drawingContext.getClone();
			context.setBlendMode(blendMode);
			context.use();
		}

		gameObject.renderWebGLStep(renderer, gameObject, context, parentMatrix, undefined, [gameObject], 0);
		if (context !== drawingContext) {
			renderer.renderNodes.finishBatch();
			context.release();
			drawingContext.beginDraw();
		}
	}

	private getMeshStateForSlot (slot: Slot): SlotMeshState {
		let meshState = this.meshStates.get(slot);
		if (!meshState) {
			const mesh = new Phaser.GameObjects.Mesh2D(this.owner.scene, 0, 0, "__DEFAULT", [], [], true) as Mesh2DObject;
			mesh.setFlipV(true);
			mesh.setRenderAsTriangles(false);
			mesh.setUseOrderedIndices(false);
			meshState = {
				mesh,
				attachment: null,
				triangles: null,
				trianglesLength: 0,
				worldVerticesLength: 0,
				clipped: false,
			};
			this.meshStates.set(slot, meshState);
		}
		return meshState;
	}

	private toPhaserBlendMode (blendMode: BlendMode): number {
		switch (blendMode) {
			case BlendMode.Additive: return this.owner.plugin.spineAdditiveBlendMode;
			case BlendMode.Multiply: return Phaser.BlendModes.MULTIPLY;
			case BlendMode.Screen: return Phaser.BlendModes.SCREEN;
			default: return Phaser.BlendModes.NORMAL;
		}
	}

	private applyMeshState (mesh: Mesh2DObject, slot: Slot, attachment: RegionAttachment | MeshAttachment) {
		const owner = this.owner;
		mesh.x = owner.x;
		mesh.y = owner.y;
		mesh.rotation = owner.rotation;
		mesh.scaleX = owner.scaleX * (owner.flipX ? -1 : 1);
		mesh.scaleY = owner.scaleY * (owner.flipY ? -1 : 1);
		mesh.scrollFactorX = owner.scrollFactorX;
		mesh.scrollFactorY = owner.scrollFactorY;
		const skeletonColor = owner.skeleton.color;
		const pose = slot.getAppliedPose();
		const color = pose.color;
		mesh.alpha = owner.alpha * skeletonColor.a * color.a * attachment.color.a;
		const dark = pose.darkColor;
		mesh.setTint(rgb(skeletonColor.r * color.r * attachment.color.r, skeletonColor.g * color.g * attachment.color.g, skeletonColor.b * color.b * attachment.color.b));
		if (dark) {
			mesh.setTint2(rgb(dark.r, dark.g, dark.b));
			mesh.setTintMode(Phaser.TintModes.MULTIPLY_TWO);
		} else {
			mesh.setTint2(0x000000);
			mesh.setTintMode(Phaser.TintModes.MULTIPLY);
		}
	}
}

function rgb (r: number, g: number, b: number): number {
	return (Math.max(0, Math.min(255, (r * 255) | 0)) << 16) | (Math.max(0, Math.min(255, (g * 255) | 0)) << 8) | Math.max(0, Math.min(255, (b * 255) | 0));
}
