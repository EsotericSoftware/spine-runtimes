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
	MathUtils,
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
import type { ISpineDebugRenderer, SpineDebugRenderer } from "./SpineDebugRenderer.js";
import { Assets } from "@pixi/assets";
import type { IPointData } from "@pixi/core";
import { Ticker } from "@pixi/core";
import type { IDestroyOptions, DisplayObject } from "@pixi/display";
import { Container } from "@pixi/display";
import { Graphics } from "@pixi/graphics";

/**
 * Options to configure a {@link Spine} game object.
 */
export interface ISpineOptions {
	/**  Set the {@link Spine.autoUpdate} value. If omitted, it is set to `true`. */
	autoUpdate?: boolean;
	/**  The value passed to the skeleton reader. If omitted, 1 is passed. See {@link SkeletonBinary.scale} for details. */
	scale?: number;
	/**
	 * A factory to override the default ones to render Spine meshes ({@link DarkSlotMesh} or {@link SlotMesh}).
	 * If omitted, a factory returning a ({@link DarkSlotMesh} or {@link SlotMesh}) will be used depending on the presence of
	 * a dark tint mesh in the skeleton.
	 * */
	slotMeshFactory?: () => ISlotMesh;
}

/**
 * AnimationStateListener {@link https://en.esotericsoftware.com/spine-api-reference#AnimationStateListener events} exposed for Pixi.
 */
export interface SpineEvents {
	complete: [trackEntry: TrackEntry];
	dispose: [trackEntry: TrackEntry];
	end: [trackEntry: TrackEntry];
	event: [trackEntry: TrackEntry, event: Event];
	interrupt: [trackEntry: TrackEntry];
	start: [trackEntry: TrackEntry];
}

/**
 * The class to instantiate a {@link Spine} game object in Pixi.
 * The static method {@link Spine.from} should be used to instantiate a Spine game object.
 */
export class Spine extends Container {
	/** The skeleton for this Spine game object. */
	public skeleton: Skeleton;
	/** The animation state for this Spine game object. */
	public state: AnimationState;

	private _debug?: ISpineDebugRenderer | undefined = undefined;
	public get debug (): ISpineDebugRenderer | undefined {
		return this._debug;
	}
	/** Pass a {@link SpineDebugRenderer} or create your own {@link ISpineDebugRenderer} to render bones, meshes, ...
	 * @example spineGO.debug = new SpineDebugRenderer();
	 */
	public set debug (value: ISpineDebugRenderer | undefined) {
		if (this._debug) {
			this._debug.unregisterSpine(this);
		}
		if (value) {
			value.registerSpine(this);
		}
		this._debug = value;
	}

	protected slotMeshFactory: () => ISlotMesh = () => new SlotMesh();

	beforeUpdateWorldTransforms: (object: Spine) => void = () => { };
	afterUpdateWorldTransforms: (object: Spine) => void = () => { };

	private autoUpdateWarned: boolean = false;
	private _autoUpdate: boolean = true;
	public get autoUpdate (): boolean {
		return this._autoUpdate;
	}
	/** When `true`, the Spine AnimationState and the Skeleton will be automatically updated using the {@link Ticker.shared} instance. */
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

	private initializeMeshFactory (options?: ISpineOptions) {
		if (options?.slotMeshFactory) {
			this.slotMeshFactory = options?.slotMeshFactory;
		} else {
			for (let i = 0; i < this.skeleton.slots.length; i++) {
				if (this.skeleton.slots[i].data.darkColor) {
					this.slotMeshFactory = options?.slotMeshFactory ?? (() => new DarkSlotMesh());
					break;
				}
			}
		}
	}

	/** If {@link Spine.autoUpdate} is `false`, this method allows to update the AnimationState and the Skeleton with the given delta. */
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
		this.state.apply(this.skeleton);
		this.beforeUpdateWorldTransforms(this);
		this.skeleton.update(delta);
		this.skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);
	}

	/** Render the meshes based on the current skeleton state, render debug information, then call {@link Container.updateTransform}. */
	public override updateTransform (): void {
		this.renderMeshes();
		this.sortChildren();
		this.debug?.renderDebug(this);
		super.updateTransform();
	}

	/** Destroy Spine game object elements, then call the {@link Container.destroy} with the given options */
	public override destroy (options?: boolean | IDestroyOptions | undefined): void {
		for (const [, mesh] of this.meshesCache) {
			mesh?.destroy();
		}
		this.state.clearListeners();
		this.debug = undefined;
		this.meshesCache.clear();
		this.slotsObject.clear();

		for (let maskKey in this.clippingSlotToPixiMasks) {
			const mask = this.clippingSlotToPixiMasks[maskKey];
			mask.destroy();
			delete this.clippingSlotToPixiMasks[maskKey];
		}

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

	private slotsObject = new Map<Slot, Container>();
	private getSlotFromRef (slotRef: number | string | Slot): Slot {
		let slot: Slot | null;
		if (typeof slotRef === 'number') slot = this.skeleton.slots[slotRef];
		else if (typeof slotRef === 'string') slot = this.skeleton.findSlot(slotRef);
		else slot = slotRef;

		if (!slot) throw new Error(`No slot found with the given slot reference: ${slotRef}`);

		return slot;
	}
	/**
	 * Add a pixi Container as a child of the Spine object.
	 * The Container will be rendered coherently with the draw order of the slot.
	 * If an attachment is active on the slot, the pixi Container will be rendered on top of it.
	 * If the Container is already attached to the given slot, nothing will happen.
	 * If the Container is already attached to another slot, it will be removed from that slot
	 * before adding it to the given one.
	 * If another Container is already attached to this slot, the old one will be removed from this
	 * slot before adding it to the current one.
	 * @param slotRef - The slot index, or the slot name, or the Slot where the pixi object will be added to.
	 * @param pixiObject - The pixi Container to add.
	 */
	addSlotObject (slotRef: number | string | Slot, pixiObject: Container): void {
		let slot = this.getSlotFromRef(slotRef);
		let oldPixiObject = this.slotsObject.get(slot);
		if (oldPixiObject === pixiObject) return;

		// search if the pixiObject was already in another slotObject
		for (const [otherSlot, oldPixiObjectAnotherSlot] of this.slotsObject) {
			if (otherSlot !== slot && oldPixiObjectAnotherSlot === pixiObject) {
				this.removeSlotObject(otherSlot, pixiObject);
				break;
			}
		}

		if (oldPixiObject) this.removeChild(oldPixiObject);

		this.slotsObject.set(slot, pixiObject);
		this.addChild(pixiObject);
	}
	/**
	 * Return the Container connected to the given slot, if any.
	 * Otherwise return undefined
	 * @param pixiObject - The slot index, or the slot name, or the Slot to get the Container from.
	 * @returns a Container if any, undefined otherwise.
	 */
	getSlotObject (slotRef: number | string | Slot): Container | undefined {
		return this.slotsObject.get(this.getSlotFromRef(slotRef));
	}
	/**
	 * Remove a slot object from the given slot.
	 * If `pixiObject` is passed and attached to the given slot, remove it from the slot.
	 * If `pixiObject` is not passed and the given slot has an attached Container, remove it from the slot.
	 * @param slotRef - The slot index, or the slot name, or the Slot where the pixi object will be remove from.
	 * @param pixiObject - Optional, The pixi Container to remove.
	 */
	removeSlotObject (slotRef: number | string | Slot, pixiObject?: Container): void {
		let slot = this.getSlotFromRef(slotRef);
		let slotObject = this.slotsObject.get(slot);
		if (!slotObject) return;

		// if pixiObject is passed, remove only if it is equal to the given one
		if (pixiObject && pixiObject !== slotObject) return;

		this.removeChild(slotObject);
		this.slotsObject.delete(slot);
	}

	private verticesCache: NumberArrayLike = Utils.newFloatArray(1024);
	private clippingSlotToPixiMasks: Record<string, Graphics> = {};
	private pixiMaskCleanup (slot: Slot) {
		let mask = this.clippingSlotToPixiMasks[slot.data.name];
		if (mask) {
			delete this.clippingSlotToPixiMasks[slot.data.name];
			mask.destroy();
		}
	}
	private updatePixiObject (pixiObject: Container, slot: Slot, zIndex: number) {
		pixiObject.setTransform(slot.bone.worldX, slot.bone.worldY, slot.bone.getWorldScaleX(), slot.bone.getWorldScaleX(), slot.bone.getWorldRotationX() * MathUtils.degRad);
		pixiObject.zIndex = zIndex + 1;
		pixiObject.alpha = this.skeleton.color.a * slot.color.a;
	}
	private updateAndSetPixiMask (pixiMaskSource: PixiMaskSource | null, pixiObject: Container) {
		if (Spine.clipper.isClipping() && pixiMaskSource) {
			let mask = this.clippingSlotToPixiMasks[pixiMaskSource.slot.data.name] as Graphics;
			if (!mask) {
				mask = new Graphics();
				this.clippingSlotToPixiMasks[pixiMaskSource.slot.data.name] = mask;
				this.addChild(mask);
			}
			if (!pixiMaskSource.computed) {
				pixiMaskSource.computed = true;
				const clippingAttachment = pixiMaskSource.slot.attachment as ClippingAttachment;
				const world = Array.from(clippingAttachment.vertices);
				clippingAttachment.computeWorldVertices(pixiMaskSource.slot, 0, clippingAttachment.worldVerticesLength, world, 0, 2);
				mask.clear().lineStyle(0).beginFill(0x000000).drawPolygon(world);
			}
			pixiObject.mask = mask;
		} else if (pixiObject.mask) {
			pixiObject.mask = null;
		}
	}

	/* 
	* Colors in pixi are premultiplied.
	* Pixi blending modes are modified to work with premultiplied colors. We cannot create custom blending modes.
	* Textures are loaded as premultiplied (see assers/atlasLoader.ts: alphaMode: `page.pma ? ALPHA_MODES.PMA : ALPHA_MODES.UNPACK`):
	* - textures non premultiplied are premultiplied on GPU on upload
	* - textures premultiplied are uploaded on GPU as is since they are already premultiplied
	* 
	* We need to take this into consideration and calculates final colors for both light and dark color as if textures were always premultiplied.
	* This implies for example that alpha for dark tint is always 1. This is way in DarkTintRenderer we have only the alpha of the light color.
	* We implies alpha of dark color as 1 and just respective alpha byte to 1.
	* (see DarkTintRenderer: const darkargb = (0xFF << 24) | darkTintRGB;)
	* If we ever want to load texture as non premultiplied on GPU, we must add a new dark alpha parameter to the TintMaterial and set the alpha.
	*/
	private renderMeshes (): void {
		this.resetMeshes();

		let triangles: Array<number> | null = null;
		let uvs: NumberArrayLike | null = null;
		let pixiMaskSource: PixiMaskSource | null = null;
		const drawOrder = this.skeleton.drawOrder;

		for (let i = 0, n = drawOrder.length, slotObjectsCounter = 0; i < n; i++) {
			const slot = drawOrder[i];

			// render pixi object on the current slot on top of the slot attachment
			let pixiObject = this.slotsObject.get(slot);
			let zIndex = i + slotObjectsCounter;
			if (pixiObject) {
				this.updatePixiObject(pixiObject, slot, zIndex + 1);
				slotObjectsCounter++;
				this.updateAndSetPixiMask(pixiMaskSource, pixiObject);
			}

			const useDarkColor = slot.darkColor != null;
			const vertexSize = Spine.clipper.isClipping() ? 2 : useDarkColor ? Spine.DARK_VERTEX_SIZE : Spine.VERTEX_SIZE;
			if (!slot.bone.active) {
				Spine.clipper.clipEndWithSlot(slot);
				this.pixiMaskCleanup(slot);
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
				pixiMaskSource = { slot, computed: false };
				continue;
			} else {
				Spine.clipper.clipEndWithSlot(slot);
				this.pixiMaskCleanup(slot);
				continue;
			}
			if (texture != null) {
				const skeleton = slot.bone.skeleton;
				const skeletonColor = skeleton.color;
				const slotColor = slot.color;
				const alpha = skeletonColor.a * slotColor.a * attachmentColor.a;
				this.lightColor.set(
					skeletonColor.r * slotColor.r * attachmentColor.r * alpha,
					skeletonColor.g * slotColor.g * attachmentColor.g * alpha,
					skeletonColor.b * slotColor.b * attachmentColor.b * alpha,
					alpha
				);
				if (slot.darkColor != null) {
					this.darkColor.set(
						slot.darkColor.r * alpha,
						slot.darkColor.g * alpha,
						slot.darkColor.b * alpha,
						1,
					);
				} else {
					this.darkColor.set(0, 0, 0, 1);
				}

				let finalVertices: NumberArrayLike;
				let finalVerticesLength: number;
				let finalIndices: NumberArrayLike;
				let finalIndicesLength: number;

				if (Spine.clipper.isClipping()) {
					Spine.clipper.clipTriangles(this.verticesCache, triangles, triangles.length, uvs, this.lightColor, this.darkColor, useDarkColor);

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
							verts[tempV++] = this.darkColor.a;
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
				mesh.zIndex = zIndex;
				mesh.updateFromSpineData(texture, slot.data.blendMode, slot.data.name, finalVertices, finalVerticesLength, finalIndices, finalIndicesLength, useDarkColor);
			}

			Spine.clipper.clipEndWithSlot(slot);
			this.pixiMaskCleanup(slot);
		}
		Spine.clipper.clipEnd();
	}

	/**
	 * Set the position of the bone given in input through a {@link IPointData}.
	 * @param bone: the bone name or the bone instance to set the position
	 * @param outPos: the new position of the bone.
	 * @throws {Error}: if the given bone is not found in the skeleton, an error is thrown
	 */
	public setBonePosition (bone: string | Bone, position: IPointData): void {
		const boneAux = bone;
		if (typeof bone === "string") {
			bone = this.skeleton.findBone(bone)!;
		}

		if (!bone) throw Error(`Cannot set bone position, bone ${String(boneAux)} not found`);
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

	/**
	 * Return the position of the bone given in input into an {@link IPointData}.
	 * @param bone: the bone name or the bone instance to get the position from
	 * @param outPos: an optional {@link IPointData} to use to return the bone position, rathern than instantiating a new object.
	 * @returns {IPointData | undefined}: the position of the bone, or undefined if no matching bone is found in the skeleton
	 */
	public getBonePosition (bone: string | Bone, outPos?: IPointData): IPointData | undefined {
		const boneAux = bone;
		if (typeof bone === "string") {
			bone = this.skeleton.findBone(bone)!;
		}

		if (!bone) {
			console.error(`Cannot get bone position! Bone ${String(boneAux)} not found`);
			return outPos;
		}

		if (!outPos) {
			outPos = { x: 0, y: 0 };
		}

		outPos.x = bone.worldX;
		outPos.y = bone.worldY;
		return outPos;
	}

	/** Converts a point from the skeleton coordinate system to the Pixi world coordinate system. */
	skeletonToPixiWorldCoordinates (point: { x: number; y: number }) {
		this.worldTransform.apply(point, point);
	}

	/** Converts a point from the Pixi world coordinate system to the skeleton coordinate system. */
	pixiWorldCoordinatesToSkeleton (point: { x: number; y: number }) {
		this.worldTransform.applyInverse(point, point);
	}

	/** Converts a point from the Pixi world coordinate system to the bone's local coordinate system. */
	pixiWorldCoordinatesToBone (point: { x: number; y: number }, bone: Bone) {
		this.pixiWorldCoordinatesToSkeleton(point);
		if (bone.parent) {
			bone.parent.worldToLocal(point as Vector2);
		} else {
			bone.worldToLocal(point as Vector2);
		}
	}

	/** A cache containing skeleton data and atlases already loaded by {@link Spine.from}. */
	public static readonly skeletonCache: Record<string, SkeletonData> = Object.create(null);

	/**
	 * Use this method to instantiate a Spine game object.
	 * Before instantiating a Spine game object, the skeleton (`.skel` or `.json`) and the atlas text files must be loaded into the Assets. For example:
	 * ```
	 * PIXI.Assets.add("sackData", "./assets/sack-pro.skel");
	 * PIXI.Assets.add("sackAtlas", "./assets/sack-pma.atlas");
	 * await PIXI.Assets.load(["sackData", "sackAtlas"]);
	 * ```
	 * Once a Spine game object is created, its skeleton data is cached into {@link Spine.skeletonCache} using the key:
	 * `${skeletonAssetName}-${atlasAssetName}-${options?.scale ?? 1}`
	 *
	 * @param skeletonAssetName - the asset name for the skeleton `.skel` or `.json` file previously loaded into the Assets
	 * @param atlasAssetName - the asset name for the atlas file previously loaded into the Assets
	 * @param options - Options to configure the Spine game object
	 * @returns {Spine} The Spine game object instantiated
	 */
	public static from (skeletonAssetName: string, atlasAssetName: string, options?: ISpineOptions): Spine {
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

	public get tint (): number {
		return this.skeleton.color.toRgb888();
	}
	public set tint (value: number) {
		Color.rgb888ToColor(this.skeleton.color, value);
	}
}

type PixiMaskSource = {
	slot: Slot,
	computed: boolean, // prevent to reculaculate vertices for a mask clipping multiple pixi objects
}

Skeleton.yDown = true;

/**
 * Represents the mesh type used in a Spine objects. Available implementations are {@link DarkSlotMesh} and {@link SlotMesh}.
 */
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
