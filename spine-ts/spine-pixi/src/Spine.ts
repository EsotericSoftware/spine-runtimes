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

import type { BlendMode, Bone, Event, NumberArrayLike, SkeletonData, Slot, TextureAtlas, TrackEntry } from "@esotericsoftware/spine-core";
import {
	AnimationState,
	AnimationStateData,
	AtlasAttachmentLoader,
	ClippingAttachment,
	Color,
	MeshAttachment,
	Physics,
	RegionAttachment,
	Skeleton,
	SkeletonBinary,
	SkeletonClipping,
	SkeletonJson,
	Utils,
	Vector2,
} from "@esotericsoftware/spine-core";
import type { SpineTexture } from "./SpineTexture.js";
import { SlotMesh } from "./SlotMesh.js";
import { DarkSlotMesh } from "./DarkSlotMesh.js";
import type { ISpineDebugRenderer } from "./SpineDebugRenderer.js";
import { Assets } from "@pixi/assets";
import type { IPointData } from "@pixi/core";
import { Ticker, utils } from "@pixi/core";
import type { IDestroyOptions, DisplayObject } from "@pixi/display";
import { Container } from "@pixi/display";

export interface ISpineOptions {
	autoUpdate?: boolean;
	slotMeshFactory?: () => ISlotMesh;
}

export interface SpineEvents {
	complete: [trackEntry: TrackEntry];
	dispose: [trackEntry: TrackEntry];
	end: [trackEntry: TrackEntry];
	event: [trackEntry: TrackEntry, event: Event];
	interrupt: [trackEntry: TrackEntry];
	start: [trackEntry: TrackEntry];
}

export class Spine extends Container {
	public skeleton: Skeleton;
	public state: AnimationState;

	private _debug?: ISpineDebugRenderer | undefined = undefined;
	public get debug (): ISpineDebugRenderer | undefined {
		return this._debug;
	}
	public set debug (value: ISpineDebugRenderer | undefined) {
		if (this._debug) {
			this._debug.unregisterSpine(this);
		}
		if (value) {
			value.registerSpine(this);
		}
		this._debug = value;
	}

	protected slotMeshFactory: () => ISlotMesh = ((): ISlotMesh => new SlotMesh());

	private autoUpdateWarned: boolean = false;
	private _autoUpdate: boolean = true;
	public get autoUpdate (): boolean {
		return this._autoUpdate;
	}
	public set autoUpdate (value: boolean) {
		if (value) {
			Ticker.shared.add(this.internalUpdate, this);
			this.autoUpdateWarned = false;
		} else {
			Ticker.shared.remove(this.internalUpdate, this);
		}
		this._autoUpdate = value;
	}

	private meshesCache = new Map<Slot, ISlotMesh>();

	private static vectorAux: Vector2 = new Vector2();
	private static clipper: SkeletonClipping = new SkeletonClipping();

	private static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];
	private static VERTEX_SIZE = 2 + 2 + 4;
	private static DARK_VERTEX_SIZE = 2 + 2 + 4 + 4;

	private lightColor = new Color();
	private darkColor = new Color();


	constructor (skeletonData: SkeletonData, options?: ISpineOptions) {
		super();

		this.skeleton = new Skeleton(skeletonData);
		const animData = new AnimationStateData(skeletonData);
		this.state = new AnimationState(animData);
		this.autoUpdate = options?.autoUpdate ?? true;
		this.initializeMeshFactory(options);
		this.skeleton.setToSetupPose();
		this.skeleton.updateWorldTransform(Physics.update);
	}

	private initializeMeshFactory(options?: ISpineOptions) {
		if (options?.slotMeshFactory) {
			this.slotMeshFactory = options?.slotMeshFactory;
		} else {
			for (let i = 0; i < this.skeleton.slots.length; i++) {
				if (this.skeleton.slots[i].data.darkColor) {
					this.slotMeshFactory = options?.slotMeshFactory ?? ((): ISlotMesh => new DarkSlotMesh());
					break;
				}
			}
		}
	}

	public update (deltaSeconds: number): void {
		if (this.autoUpdate && !this.autoUpdateWarned) {
			console.warn("You are calling update on a Spine instance that has autoUpdate set to true. This is probably not what you want.");
			this.autoUpdateWarned = true;
		}
		this.internalUpdate(0, deltaSeconds);
	}

	protected internalUpdate (_deltaFrame: number, deltaSeconds?: number): void {
		// Because reasons, pixi uses deltaFrames at 60fps. We ignore the default deltaFrames and use the deltaSeconds from pixi ticker.
		const delta = deltaSeconds ?? Ticker.shared.deltaMS / 1000;
		this.state.update(delta);
		this.skeleton.update(delta);
	}

	public override updateTransform (): void {
		this.updateSpineTransform();
		this.debug?.renderDebug(this);
		super.updateTransform();
	}

	protected updateSpineTransform (): void {
		// if I ever create the linked spines, this will be useful.

		this.state.apply(this.skeleton);
		this.skeleton.updateWorldTransform(Physics.update);
		this.updateGeometry();
		this.sortChildren();
	}

	public override destroy (options?: boolean | IDestroyOptions | undefined): void {
		for (const [, mesh] of this.meshesCache) {
			mesh?.destroy();
		}
		this.state.clearListeners();
		this.debug = undefined;
		this.meshesCache.clear();
		super.destroy(options);
	}

	private resetMeshes (): void {
		for (const [, mesh] of this.meshesCache) {
			mesh.zIndex = -1;
			mesh.visible = false;
		}
	}

	/**
	 * If you want to manually handle which meshes go on which slot and how you cache, overwrite this method.
	 */
	protected getMeshForSlot (slot: Slot): ISlotMesh {
		if (!this.meshesCache.has(slot)) {
			let mesh = this.slotMeshFactory();
			this.addChild(mesh);
			this.meshesCache.set(slot, mesh);
			return mesh;
		} else {
			let mesh = this.meshesCache.get(slot)!;
			mesh.visible = true;
			return mesh;
		}
	}

	private verticesCache: NumberArrayLike = Utils.newFloatArray(1024);

	private updateGeometry (): void {
		this.resetMeshes();

		let triangles: Array<number> | null = null;
		let uvs: NumberArrayLike | null = null;
		const drawOrder = this.skeleton.drawOrder;

		for (let i = 0, n = drawOrder.length; i < n; i++) {
			const slot = drawOrder[i];
			const useDarkColor = slot.darkColor != null;
			const vertexSize = Spine.clipper.isClipping() ? 2 : useDarkColor ? Spine.DARK_VERTEX_SIZE : Spine.VERTEX_SIZE;
			if (!slot.bone.active) {
				Spine.clipper.clipEndWithSlot(slot);
				continue;
			}
			const attachment = slot.getAttachment();
			let attachmentColor: Color | null;
			let texture: SpineTexture | null;
			let numFloats = 0;
			if (attachment instanceof RegionAttachment) {
				const region = attachment;
				attachmentColor = region.color;
				numFloats = vertexSize * 4;
				region.computeWorldVertices(slot, this.verticesCache, 0, vertexSize);
				triangles = Spine.QUAD_TRIANGLES;
				uvs = region.uvs;
				texture = <SpineTexture>region.region?.texture;
			} else if (attachment instanceof MeshAttachment) {
				const mesh = attachment;
				attachmentColor = mesh.color;
				numFloats = (mesh.worldVerticesLength >> 1) * vertexSize;
				if (numFloats > this.verticesCache.length) {
					this.verticesCache = Utils.newFloatArray(numFloats);
				}
				mesh.computeWorldVertices(slot, 0, mesh.worldVerticesLength, this.verticesCache, 0, vertexSize);
				triangles = mesh.triangles;
				uvs = mesh.uvs;
				texture = <SpineTexture>mesh.region?.texture;
			} else if (attachment instanceof ClippingAttachment) {
				Spine.clipper.clipStart(slot, attachment);
				continue;
			} else {
				Spine.clipper.clipEndWithSlot(slot);
				continue;
			}
			if (texture != null) {
				const skeleton = slot.bone.skeleton;
				const skeletonColor = skeleton.color;
				const slotColor = slot.color;
				const alpha = skeletonColor.a * slotColor.a * attachmentColor.a;
				this.lightColor.set(
					skeletonColor.r * slotColor.r * attachmentColor.r,
					skeletonColor.g * slotColor.g * attachmentColor.g,
					skeletonColor.b * slotColor.b * attachmentColor.b,
					alpha
				);
				if (slot.darkColor != null) {
					this.darkColor.setFromColor(slot.darkColor);
				} else {
					this.darkColor.set(0, 0, 0, 0);
				}

				let finalVertices: NumberArrayLike;
				let finalVerticesLength: number;
				let finalIndices: NumberArrayLike;
				let finalIndicesLength: number;

				if (Spine.clipper.isClipping()) {
					Spine.clipper.clipTriangles(this.verticesCache, numFloats, triangles, triangles.length, uvs, this.lightColor, this.darkColor, useDarkColor);

					finalVertices = Spine.clipper.clippedVertices;
					finalVerticesLength = finalVertices.length;

					finalIndices = Spine.clipper.clippedTriangles;
					finalIndicesLength = finalIndices.length;
				} else {
					const verts = this.verticesCache;
					for (let v = 2, u = 0, n = numFloats; v < n; v += vertexSize, u += 2) {
						let tempV = v;
						verts[tempV++] = this.lightColor.r;
						verts[tempV++] = this.lightColor.g;
						verts[tempV++] = this.lightColor.b;
						verts[tempV++] = this.lightColor.a;

						verts[tempV++] = uvs[u];
						verts[tempV++] = uvs[u + 1];

						if (useDarkColor) {
							verts[tempV++] = this.darkColor.r;
							verts[tempV++] = this.darkColor.g;
							verts[tempV++] = this.darkColor.b;
						}
					}
					finalVertices = this.verticesCache;
					finalVerticesLength = numFloats;
					finalIndices = triangles;
					finalIndicesLength = triangles.length;
				}

				if (finalVerticesLength == 0 || finalIndicesLength == 0) {
					Spine.clipper.clipEndWithSlot(slot);
					continue;
				}

				const mesh = this.getMeshForSlot(slot);
				mesh.zIndex = i;
				mesh.updateFromSpineData(texture, slot.data.blendMode, slot.data.name, finalVertices, finalVerticesLength, finalIndices, finalIndicesLength, useDarkColor);
			}

			Spine.clipper.clipEndWithSlot(slot);
		}
		Spine.clipper.clipEnd();
	}

	public setBonePosition (bone: string | Bone, position: IPointData): void {
		const boneAux = bone;
		if (typeof bone === "string") {
			bone = this.skeleton.findBone(bone)!;
		}

		if (!bone) throw Error(`Cant set bone position, bone ${String(boneAux)} not found`);
		Spine.vectorAux.set(position.x, position.y);

		if (bone.parent) {
			const aux = bone.parent.worldToLocal(Spine.vectorAux);
			bone.x = aux.x;
			bone.y = aux.y;
		}
		else {
			bone.x = Spine.vectorAux.x;
			bone.y = Spine.vectorAux.y;
		}
	}

	public getBonePosition (bone: string | Bone, outPos?: IPointData): IPointData | undefined {
		const boneAux = bone;
		if (typeof bone === "string") {
			bone = this.skeleton.findBone(bone)!;
		}

		if (!bone) {
			console.error(`Cant set bone position! Bone ${String(boneAux)} not found`);
			return outPos;
		}

		if (!outPos) {
			outPos = { x: 0, y: 0 };
		}

		outPos.x = bone.worldX;
		outPos.y = bone.worldY;
		return outPos;
	}

	public static readonly skeletonCache: Record<string, SkeletonData> = Object.create(null);

	public static from (skeletonAssetName: string, atlasAssetName: string, options?: ISpineOptions & { scale?: number }): Spine {
		const cacheKey = `${skeletonAssetName}-${atlasAssetName}-${options?.scale ?? 1}`;
		let skeletonData = Spine.skeletonCache[cacheKey];
		if (skeletonData) {
			return new Spine(skeletonData, options);
		}
		const skeletonAsset = Assets.get<any | Uint8Array>(skeletonAssetName);
		const atlasAsset = Assets.get<TextureAtlas>(atlasAssetName);
		const attachmentLoader = new AtlasAttachmentLoader(atlasAsset);
		let parser = skeletonAsset instanceof Uint8Array ? new SkeletonBinary(attachmentLoader) : new SkeletonJson(attachmentLoader);
		parser.scale = options?.scale ?? 1;
		skeletonData = parser.readSkeletonData(skeletonAsset);
		Spine.skeletonCache[cacheKey] = skeletonData;
		return new this(skeletonData, options);
	}
}

Skeleton.yDown = true;

export interface ISlotMesh extends DisplayObject {
	name: string;
	updateFromSpineData (
		slotTexture: SpineTexture,
		slotBlendMode: BlendMode,
		slotName: string,
		finalVertices: NumberArrayLike,
		finalVerticesLength: number,
		finalIndices: NumberArrayLike,
		finalIndicesLength: number,
		darkTint: boolean
	): void;
}
