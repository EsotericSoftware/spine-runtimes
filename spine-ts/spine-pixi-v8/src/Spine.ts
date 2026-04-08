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
	AnimationState,
	AnimationStateData,
	AtlasAttachmentLoader,
	type Attachment,
	type Bone,
	ClippingAttachment,
	Color,
	MeshAttachment,
	Physics,
	Pool,
	RegionAttachment,
	Skeleton,
	SkeletonBinary,
	SkeletonBounds,
	SkeletonClipping,
	SkeletonData,
	SkeletonJson,
	Skin,
	type Slot,
	type TextureAtlas,
	type TrackEntry,
	Vector2,
} from '@esotericsoftware/spine-core';
import {
	Assets,
	type Bounds,
	Cache,
	Container,
	type ContainerOptions,
	type DestroyOptions,
	Graphics,
	type PointData,
	Texture,
	Ticker,
	ViewContainer,
} from 'pixi.js';
import type { ISpineDebugRenderer } from './SpineDebugRenderer.js';
import type { SpineTexture } from './SpineTexture.js';

/**
 * Options to create a {@link Spine} using {@link Spine.from}.
 */
export interface SpineFromOptions {
	/** the asset name for the skeleton `.skel` or `.json` file previously loaded into the Assets */
	skeleton: string;

	/** the asset name for the atlas file previously loaded into the Assets */
	atlas: string;

	/**  The value passed to the skeleton reader. If omitted, 1 is passed. See {@link SkeletonBinary.scale} for details. */
	scale?: number;

	/**  Set the {@link Spine.autoUpdate} value. If omitted, it is set to `true`. */
	autoUpdate?: boolean;

	/**
	 * If `true`, use the dark tint renderer to render the skeleton
	 * If `false`, use the default pixi renderer to render the skeleton
	 * If `undefined`, use the dark tint renderer if at least one slot has tint black
	 */
	darkTint?: boolean;

	/** The bounds provider to use. If undefined the bounds will be dynamic, calculated when requested and based on the current frame. */
	boundsProvider?: SpineBoundsProvider,

	/** Set {@link AtlasAttachmentLoader.allowMissingRegions} property on the AtlasAttachmentLoader. */
	allowMissingRegions?: boolean;

	/** The ticker to use when {@link autoUpdate} is `true`. Defaults to {@link Ticker.shared}. */
	ticker?: Ticker,
};

const vectorAux = new Vector2();

Skeleton.yDown = true;

const clipper = new SkeletonClipping();

/** A bounds provider calculates the bounding box for a skeleton, which is then assigned as the size of the SpineGameObject. */
export interface SpineBoundsProvider {
	/** Returns the bounding box for the skeleton, in skeleton space. */
	calculateBounds (gameObject: Spine): {
		x: number;
		y: number;
		width: number;
		height: number;
	};
}

/** A bounds provider that provides a fixed size given by the user. */
export class AABBRectangleBoundsProvider implements SpineBoundsProvider {
	constructor (
		private x: number,
		private y: number,
		private width: number,
		private height: number,
	) { }
	calculateBounds () {
		return { x: this.x, y: this.y, width: this.width, height: this.height };
	}
}

/** A bounds provider that calculates the bounding box from the setup pose. */
export class SetupPoseBoundsProvider implements SpineBoundsProvider {
	/**
	 * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
	 */
	constructor (
		private clipping = false,
	) { }

	calculateBounds (gameObject: Spine) {
		if (!gameObject.skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		const skeleton = new Skeleton(gameObject.skeleton.data);
		skeleton.setupPose();
		skeleton.updateWorldTransform(Physics.update);
		const bounds = skeleton.getBoundsRect(this.clipping ? new SkeletonClipping() : undefined);
		return bounds.width === Number.NEGATIVE_INFINITY
			? { x: 0, y: 0, width: 0, height: 0 }
			: bounds;
	}
}

/** A bounds provider that calculates the bounding box by taking the maximumg bounding box for a combination of skins and specific animation. */
export class SkinsAndAnimationBoundsProvider
	implements SpineBoundsProvider {
	/**
	 * @param animation The animation to use for calculating the bounds. If null, the setup pose is used.
	 * @param skins The skins to use for calculating the bounds. If empty, the default skin is used.
	 * @param timeStep The time step to use for calculating the bounds. A smaller time step means more precision, but slower calculation.
	 * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
	 */
	constructor (
		private animation: string | null,
		private skins: string[] = [],
		private timeStep: number = 0.05,
		private clipping = false,
	) { }

	calculateBounds (gameObject: Spine): {
		x: number;
		y: number;
		width: number;
		height: number;
	} {
		if (!gameObject.skeleton || !gameObject.state)
			return { x: 0, y: 0, width: 0, height: 0 };
		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		const animationState = new AnimationState(gameObject.state.data);
		const skeleton = new Skeleton(gameObject.skeleton.data);
		const clipper = this.clipping ? new SkeletonClipping() : undefined;
		const data = skeleton.data;
		if (this.skins.length > 0) {
			const customSkin = new Skin("custom-skin");
			for (const skinName of this.skins) {
				const skin = data.findSkin(skinName);
				if (skin == null) continue;
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}
		skeleton.setupPose();

		const animation = this.animation != null ? data.findAnimation(this.animation) : null;

		if (animation == null) {
			skeleton.updateWorldTransform(Physics.update);
			const bounds = skeleton.getBoundsRect(clipper);
			return bounds.width === Number.NEGATIVE_INFINITY
				? { x: 0, y: 0, width: 0, height: 0 }
				: bounds;
		} else {
			let minX = Number.POSITIVE_INFINITY,
				minY = Number.POSITIVE_INFINITY,
				maxX = Number.NEGATIVE_INFINITY,
				maxY = Number.NEGATIVE_INFINITY;
			animationState.clearTracks();
			animationState.setAnimation(0, animation, false);
			const steps = Math.max(animation.duration / this.timeStep, 1.0);
			for (let i = 0; i < steps; i++) {
				const delta = i > 0 ? this.timeStep : 0;
				animationState.update(delta);
				animationState.apply(skeleton);
				skeleton.update(delta);
				skeleton.updateWorldTransform(Physics.update);

				const bounds = skeleton.getBoundsRect(clipper);
				minX = Math.min(minX, bounds.x);
				minY = Math.min(minY, bounds.y);
				maxX = Math.max(maxX, bounds.x + bounds.width);
				maxY = Math.max(maxY, bounds.y + bounds.height);
			}
			const bounds = {
				x: minX,
				y: minY,
				width: maxX - minX,
				height: maxY - minY,
			};
			return bounds.width === Number.NEGATIVE_INFINITY
				? { x: 0, y: 0, width: 0, height: 0 }
				: bounds;
		}
	}
}

export interface SpineOptions extends ContainerOptions {
	/** the {@link SkeletonData} used to instantiate the skeleton */
	skeletonData: SkeletonData;

	/**  See {@link SpineFromOptions.autoUpdate}. */
	autoUpdate?: boolean;

	/**  See {@link SpineFromOptions.darkTint}. */
	darkTint?: boolean;

	/**  See {@link SpineFromOptions.boundsProvider}. */
	boundsProvider?: SpineBoundsProvider,

	/** See {@link SpineFromOptions.ticker}. */
	ticker?: Ticker,
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

export interface ClippedData {
	vertices: Float32Array;
	uvs: Float32Array;
	indices: Uint16Array;
	vertexCount: number;
	indicesCount: number;
}

export interface AttachmentCacheData {
	id: string;
	clipped: boolean;
	vertices: Float32Array;
	uvs: Float32Array;
	indices: number[];
	color: Color;
	darkColor: Color;
	darkTint: boolean;
	skipRender: boolean;
	texture: Texture;
	clippedData?: ClippedData;
}

interface SlotsToClipping {
	slot: Slot,
	mask?: Graphics,
	maskComputed?: boolean,
	vertices: Array<number>,
};

const maskPool = new Pool<Graphics>(() => new Graphics);

/**
 * The class to instantiate a {@link Spine} game object in Pixi.
 * Create and customize the default configuration using the static method {@link Spine.createOptions},
 * then pass it to the constructor.
 */
export class Spine extends ViewContainer {
	// Pixi properties
	public batched = true;
	public buildId = 0;
	public override readonly renderPipeId = 'spine';
	public _didSpineUpdate = false;

	public beforeUpdateWorldTransforms: (object: Spine) => void = () => { /** */ };
	public afterUpdateWorldTransforms: (object: Spine) => void = () => { /** */ };

	// Spine properties
	/** The skeleton for this Spine game object. */
	public skeleton: Skeleton;
	/** The animation state for this Spine game object. */
	public state: AnimationState;
	public skeletonBounds?: SkeletonBounds;

	private darkTint = false;
	private _debug?: ISpineDebugRenderer | undefined = undefined;

	readonly _slotsObject: Record<string, { slot: Slot, container: Container, followAttachmentTimeline: boolean } | null> = Object.create(null);
	private clippingSlotToPixiMasks: Record<string, SlotsToClipping> = Object.create(null);

	private getSlotFromRef (slotRef: number | string | Slot): Slot {
		let slot: Slot | null;

		if (typeof slotRef === 'number') slot = this.skeleton.slots[slotRef];
		else if (typeof slotRef === 'string') slot = this.skeleton.findSlot(slotRef);
		else slot = slotRef;

		if (!slot) throw new Error(`No slot found with the given slot reference: ${slotRef}`);

		return slot;
	}

	public spineAttachmentsDirty = true;
	public spineTexturesDirty = true;

	private _lastAttachments: Attachment[] = [];

	private _stateChanged = true;
	private attachmentCacheData: Record<string, AttachmentCacheData>[] = [];

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

	private _autoUpdate = false;
	private _ticker: Ticker = Ticker.shared;

	public get autoUpdate (): boolean {
		return this._autoUpdate;
	}
	/** When `true`, the Spine AnimationState and the Skeleton will be automatically updated using the {@link ticker}. */
	public set autoUpdate (value: boolean) {
		if (value && !this._autoUpdate) {
			this._ticker.add(this.internalUpdate, this);
		} else if (!value && this._autoUpdate) {
			this._ticker.remove(this.internalUpdate, this);
		}

		this._autoUpdate = value;
	}

	/** The ticker to use when {@link autoUpdate} is `true`. Defaults to {@link Ticker.shared}. */
	public get ticker (): Ticker {
		return this._ticker;
	}
	/** Sets the ticker to use when {@link autoUpdate} is `true`. If `autoUpdate` is already `true`, the update callback will be moved from the old ticker to the new one. */
	public set ticker (value: Ticker) {
		value = value ?? Ticker.shared;
		if (this._ticker === value) return;

		if (this._autoUpdate) {
			this._ticker.remove(this.internalUpdate, this);
			value.add(this.internalUpdate, this);
		}

		this._ticker = value;
	}

	private _boundsProvider?: SpineBoundsProvider;
	/** The bounds provider to use. If undefined the bounds will be dynamic, calculated when requested and based on the current frame. */
	public get boundsProvider (): SpineBoundsProvider | undefined {
		return this._boundsProvider;
	}
	public set boundsProvider (value: SpineBoundsProvider | undefined) {
		this._boundsProvider = value;
		if (value) {
			this._boundsDirty = false;
		}
		this.updateBounds();
	}

	private hasNeverUpdated = true;
	constructor (options: SkeletonData | SpineOptions | SpineFromOptions) {
		super({});

		if (options instanceof SkeletonData)
			options = { skeletonData: options };
		else if ("skeleton" in options)
			options = new.target.createOptions(options);

		this.allowChildren = true;

		const { autoUpdate, boundsProvider, darkTint, skeletonData, ticker } = options;
		this.skeleton = new Skeleton(skeletonData);
		this.state = new AnimationState(new AnimationStateData(skeletonData));
		if (ticker) this._ticker = ticker;
		this.autoUpdate = autoUpdate ?? true;
		this._boundsProvider = boundsProvider;

		// dark tint can be enabled by options, otherwise is enable if at least one slot has tint black
		this.darkTint = darkTint === undefined
			? this.skeleton.slots.some(slot => !!slot.data.setupPose.darkColor)
			: darkTint;

		const slots = this.skeleton.slots;
		for (let i = 0; i < slots.length; i++)
			this.attachmentCacheData[i] = Object.create(null);
	}

	/** If {@link Spine.autoUpdate} is `false`, this method allows to update the AnimationState and the Skeleton with the given delta. */
	public update (dt: number): void {
		this.internalUpdate(undefined, dt);
	}

	protected internalUpdate (ticker?: Ticker, deltaSeconds?: number): void {
		this._updateAndApplyState(deltaSeconds ?? this._ticker.deltaMS / 1000);
	}

	override get bounds () {
		if (this._boundsDirty) {
			this.updateBounds();
		}

		return this._bounds;
	}

	/**
	 * Set the position of the bone given in input through a {@link IPointData}.
	 * @param bone: the bone name or the bone instance to set the position
	 * @param outPos: the new position of the bone.
	 * @throws {Error}: if the given bone is not found in the skeleton, an error is thrown
	 */
	public setBonePosition (bone: string | Bone, position: PointData): void {
		const boneAux = bone;

		if (typeof bone === 'string') {
			bone = this.skeleton.findBone(bone) as Bone;
		}

		if (!bone) throw Error(`Cant set bone position, bone ${String(boneAux)} not found`);
		vectorAux.set(position.x, position.y);

		const applied = bone.appliedPose;
		if (bone.parent) {
			const aux = bone.parent.appliedPose.worldToLocal(vectorAux);

			applied.x = aux.x;
			applied.y = -aux.y;
		}
		else {
			applied.x = vectorAux.x;
			applied.y = vectorAux.y;
		}
	}

	/**
	 * Return the position of the bone given in input into an {@link IPointData}.
	 * @param bone: the bone name or the bone instance to get the position from
	 * @param outPos: an optional {@link IPointData} to use to return the bone position, rathern than instantiating a new object.
	 * @returns {IPointData | undefined}: the position of the bone, or undefined if no matching bone is found in the skeleton
	 */
	public getBonePosition (bone: string | Bone, outPos?: PointData): PointData | undefined {
		const boneAux = bone;

		if (typeof bone === 'string') {
			bone = this.skeleton.findBone(bone) as Bone;
		}

		if (!bone) {
			console.error(`Cant set bone position! Bone ${String(boneAux)} not found`);

			return outPos;
		}

		if (!outPos) {
			outPos = { x: 0, y: 0 };
		}

		outPos.x = bone.appliedPose.worldX;
		outPos.y = bone.appliedPose.worldY;

		return outPos;
	}

	/**
	 * Advance the state and skeleton by the given time, then update slot objects too.
	 * The container transform is not updated.
	 *
	 * @param time the time at which to set the state
	 */
	private _updateAndApplyState (time: number) {
		this.hasNeverUpdated = false;

		this.state.update(time);
		this.skeleton.update(time);

		const { skeleton } = this;

		this.state.apply(skeleton);

		this.beforeUpdateWorldTransforms(this);
		skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);

		this.updateSlotObjects();

		this._stateChanged = true;

		this.onViewUpdate();
	}

	/**
	 * - validates the attachments - to flag if the attachments have changed this state
	 * - transforms the attachments - to update the vertices of the attachments based on the new positions
	 * @internal
	 */
	_validateAndTransformAttachments () {
		if (!this._stateChanged) return;
		this._stateChanged = false;

		this.validateAttachments();

		this.transformAttachments();
	}

	private validateAttachments () {

		const currentDrawOrder = this.skeleton.drawOrder.appliedPose;

		const lastAttachments = this._lastAttachments;

		let index = 0;

		let spineAttachmentsDirty = false;

		for (let i = 0; i < currentDrawOrder.length; i++) {
			const slot = currentDrawOrder[i];
			const attachment = slot.appliedPose.attachment;

			if (attachment) {
				if (attachment !== lastAttachments[index]) {
					spineAttachmentsDirty = true;
					lastAttachments[index] = attachment;
				}

				index++;
			}
		}

		if (index !== lastAttachments.length) {
			spineAttachmentsDirty = true;
			lastAttachments.length = index;
		}

		this.spineAttachmentsDirty ||= spineAttachmentsDirty;
	}

	private currentClippingSlot: SlotsToClipping | undefined;
	private updateAndSetPixiMask (slot: Slot, last: boolean) {
		// assign/create the currentClippingSlot
		const pose = slot.appliedPose;
		const attachment = pose.attachment;
		if (attachment && attachment instanceof ClippingAttachment) {
			const clip = (this.clippingSlotToPixiMasks[slot.data.name] ||= { slot, vertices: [] as number[] });
			clip.maskComputed = false;
			this.currentClippingSlot = this.clippingSlotToPixiMasks[slot.data.name];
			return;
		}

		// assign the currentClippingSlot mask to the slot object
		const currentClippingSlot = this.currentClippingSlot;
		const slotObject = this._slotsObject[slot.data.name];
		if (currentClippingSlot && slotObject) {
			const slotClipping = currentClippingSlot.slot;
			const clippingAttachment = slotClipping.pose.attachment as ClippingAttachment;

			// create the pixi mask, only the first time and if the clipped slot is the first one clipped by this currentClippingSlot
			let mask = currentClippingSlot.mask;
			if (!mask) {
				mask = maskPool.obtain();
				currentClippingSlot.mask = mask;
				this.addChild(mask);
			}

			// compute the pixi mask polygon, if the clipped slot is the first one clipped by this currentClippingSlot
			if (!currentClippingSlot.maskComputed) {
				currentClippingSlot.maskComputed = true;
				const worldVerticesLength = clippingAttachment.worldVerticesLength;
				const vertices = currentClippingSlot.vertices;
				clippingAttachment.computeWorldVertices(this.skeleton, slotClipping, 0, worldVerticesLength, vertices, 0, 2);
				mask.clear().poly(vertices).stroke({ width: 0 }).fill({ alpha: .25 });
			}
			slotObject.container.mask = mask;
		} else if (slotObject?.container.mask) {
			// remove the mask, if slot object has a mask, but currentClippingSlot is undefined
			slotObject.container.mask = null;
		}

		// if current slot is the ending one of the currentClippingSlot mask, set currentClippingSlot to undefined
		if (currentClippingSlot && (currentClippingSlot.slot.appliedPose.attachment as ClippingAttachment).endSlot === slot.data) {
			this.currentClippingSlot = undefined;
		}

		// clean up unused masks
		if (last) {
			for (const key in this.clippingSlotToPixiMasks) {
				const clippingSlotToPixiMask = this.clippingSlotToPixiMasks[key];
				if ((!(clippingSlotToPixiMask.slot.appliedPose.attachment instanceof ClippingAttachment) || !clippingSlotToPixiMask.maskComputed) && clippingSlotToPixiMask.mask) {
					this.removeChild(clippingSlotToPixiMask.mask);
					maskPool.free(clippingSlotToPixiMask.mask);
					clippingSlotToPixiMask.mask = undefined;
				}
			}
			this.currentClippingSlot = undefined;
		}
	}

	private transformAttachments () {
		const currentDrawOrder = this.skeleton.drawOrder.appliedPose;
		const skeleton = this.skeleton;

		for (let i = 0; i < currentDrawOrder.length; i++) {
			const slot = currentDrawOrder[i];

			this.updateAndSetPixiMask(slot, i === currentDrawOrder.length - 1);

			const pose = slot.appliedPose;
			const attachment = pose.attachment;

			if (attachment) {
				if (attachment instanceof MeshAttachment || attachment instanceof RegionAttachment) {
					const cacheData = this._getCachedData(slot, attachment);

					const sequence = attachment.sequence;
					const sequenceIndex = sequence.resolveIndex(pose);

					if (attachment instanceof RegionAttachment) {
						attachment.computeWorldVertices(slot, attachment.getOffsets(pose), cacheData.vertices, 0, 2);
					}
					else {
						attachment.computeWorldVertices(
							skeleton,
							slot,
							0,
							attachment.worldVerticesLength,
							cacheData.vertices,
							0,
							2,
						);
					}

					cacheData.uvs = sequence.getUVs(sequenceIndex);

					const skeletonColor = skeleton.color;
					const slotColor = pose.color;
					const attachmentColor = attachment.color;
					const alpha = skeletonColor.a * slotColor.a * attachmentColor.a;

					if (this.alpha === 0 || alpha === 0) {
						if (!cacheData.skipRender) this.spineAttachmentsDirty = true;
						cacheData.skipRender = true;
					} else {
						if (cacheData.skipRender) this.spineAttachmentsDirty = true;
						cacheData.skipRender = cacheData.clipped = false;

						cacheData.color.set(
							skeletonColor.r * slotColor.r * attachmentColor.r,
							skeletonColor.g * slotColor.g * attachmentColor.g,
							skeletonColor.b * slotColor.b * attachmentColor.b,
							alpha,
						);

						if (pose.darkColor) {
							cacheData.darkColor.setFromColor(pose.darkColor);
						}

						const texture = (sequence.regions[sequenceIndex]?.texture as SpineTexture)?.texture || Texture.EMPTY;

						if (cacheData.texture !== texture) {
							cacheData.texture = texture;
							this.spineTexturesDirty = true;
						}

						if (clipper.isClipping()) {
							this.updateClippingData(cacheData);
						}
					}
				}
				else if (attachment instanceof ClippingAttachment) {
					clipper.clipEnd(slot);
					clipper.clipStart(skeleton, slot, attachment);
					continue;
				}
			}
			clipper.clipEnd(slot);
		}
		clipper.clipEnd();
	}

	private updateClippingData (cacheData: AttachmentCacheData) {
		cacheData.clipped = true;

		clipper.clipTrianglesUnpacked(
			cacheData.vertices,
			0,
			cacheData.indices,
			cacheData.indices.length,
			cacheData.uvs,
		);

		const { clippedVerticesTyped, clippedUVsTyped, clippedTrianglesTyped } = clipper;

		const verticesCount = clipper.clippedVerticesLength / 2;
		const indicesCount = clipper.clippedTrianglesLength;

		if (!cacheData.clippedData) {
			cacheData.clippedData = {
				vertices: new Float32Array(verticesCount * 2),
				uvs: new Float32Array(verticesCount * 2),
				vertexCount: verticesCount,
				indices: new Uint16Array(indicesCount),
				indicesCount,
			};

			this.spineAttachmentsDirty = true;
		}

		const clippedData = cacheData.clippedData;

		const sizeChange = clippedData.vertexCount !== verticesCount || indicesCount !== clippedData.indicesCount;

		cacheData.skipRender = verticesCount === 0;

		if (sizeChange) {
			this.spineAttachmentsDirty = true;

			if (clippedData.vertexCount < verticesCount) {
				// buffer reuse!
				clippedData.vertices = new Float32Array(verticesCount * 2);
				clippedData.uvs = new Float32Array(verticesCount * 2);
			}

			if (clippedData.indices.length < indicesCount) {
				clippedData.indices = new Uint16Array(indicesCount);
			}
		}

		const { vertices, uvs, indices } = clippedData;
		vertices.set(clippedVerticesTyped);
		uvs.set(clippedUVsTyped);
		indices.set(clippedTrianglesTyped);
		clippedData.vertexCount = verticesCount;
		clippedData.indicesCount = indicesCount;
	}

	/**
	 * ensure that attached containers map correctly to their slots
	 * along with their position, rotation, scale, and visibility.
	 */
	private updateSlotObjects () {
		for (const i in this._slotsObject) {
			const slotAttachment = this._slotsObject[i];

			if (!slotAttachment) continue;

			this.updateSlotObject(slotAttachment);
		}
	}

	private updateSlotObject (slotAttachment: { slot: Slot, container: Container, followAttachmentTimeline: boolean }) {
		const { slot, container } = slotAttachment;

		const pose = slot.appliedPose;
		const followAttachmentValue = slotAttachment.followAttachmentTimeline ? Boolean(pose.attachment) : true;
		const slotAlpha = this.skeleton.color.a * pose.color.a;

		container.visible = this.skeleton.drawOrder.appliedPose.includes(slot) && followAttachmentValue
			&& this.alpha > 0 && slotAlpha > 0;

		if (container.visible) {
			const applied = slot.bone.appliedPose;

			const matrix = container.localTransform;
			matrix.a = applied.a;
			matrix.b = applied.c;
			matrix.c = -applied.b;
			matrix.d = -applied.d;
			matrix.tx = applied.worldX;
			matrix.ty = applied.worldY;
			container.setFromMatrix(matrix);

			container.alpha = slotAlpha;
		}
	}

	/** @internal */
	_getCachedData (slot: Slot, attachment: RegionAttachment | MeshAttachment): AttachmentCacheData {
		return this.attachmentCacheData[slot.data.index][attachment.name] || this.initCachedData(slot, attachment);
	}

	private initCachedData (slot: Slot, attachment: RegionAttachment | MeshAttachment): AttachmentCacheData {
		let vertices: Float32Array;

		if (attachment instanceof RegionAttachment) {
			vertices = new Float32Array(8);

			const sequence = attachment.sequence;
			this.attachmentCacheData[slot.data.index][attachment.name] = {
				id: `${slot.data.index}-${attachment.name}`,
				vertices,
				clipped: false,
				indices: [0, 1, 2, 0, 2, 3],
				uvs: new Float32Array(sequence.getUVs(0).length),
				color: new Color(1, 1, 1, 1),
				darkColor: new Color(0, 0, 0, 0),
				darkTint: this.darkTint,
				skipRender: false,
				texture: (sequence.regions[0]?.texture as SpineTexture)?.texture,
			};
		}
		else {
			vertices = new Float32Array(attachment.worldVerticesLength);

			const sequence = attachment.sequence;
			this.attachmentCacheData[slot.data.index][attachment.name] = {
				id: `${slot.data.index}-${attachment.name}`,
				vertices,
				clipped: false,
				indices: attachment.triangles,
				uvs: new Float32Array(sequence.getUVs(0).length),
				color: new Color(1, 1, 1, 1),
				darkColor: new Color(0, 0, 0, 0),
				darkTint: this.darkTint,
				skipRender: false,
				texture: (sequence.regions[0]?.texture as SpineTexture)?.texture,
			};
		}

		return this.attachmentCacheData[slot.data.index][attachment.name];
	}

	protected onViewUpdate () {
		// increment from the 12th bit!
		this._didViewChangeTick++;
		if (!this._boundsProvider) {
			this._boundsDirty = true;
		}

		if (this.didViewUpdate) return;
		this.didViewUpdate = true;

		const renderGroup = this.renderGroup || this.parentRenderGroup;

		if (renderGroup) {
			renderGroup.onChildViewUpdate(this);
		}

		this.debug?.renderDebug(this);
	}

	/**
	 * Attaches a PixiJS container to a specified slot. This will map the world transform of the slots bone
	 * to the attached container. A container can only be attached to one slot at a time.
	 *
	 * @param container - The container to attach to the slot
	 * @param slotRef - The slot id or  slot to attach to
	 * @param options - Optional settings for the attachment.
	 * @param options.followAttachmentTimeline - If true, the attachment will follow the slot's attachment timeline.
	 */
	public addSlotObject (slot: number | string | Slot, container: Container, options?: { followAttachmentTimeline?: boolean }) {
		slot = this.getSlotFromRef(slot);

		// need to check in on the container too...
		for (const i in this._slotsObject) {
			if (this._slotsObject[i]?.container === container) {
				this.removeSlotObject(this._slotsObject[i].slot);
			}
		}

		this.removeSlotObject(slot);

		container.includeInBuild = false;

		// TODO only add once??
		this.addChild(container);

		const slotObject = {
			container,
			slot,
			followAttachmentTimeline: options?.followAttachmentTimeline || false,
		};
		this._slotsObject[slot.data.name] = slotObject;

		this.updateSlotObject(slotObject);
	}

	/**
	 * Removes a PixiJS container from the slot it is attached to.
	 *
	 * @param container - The container to detach from the slot
	 * @param slotOrContainer - The container, slot id or slot to detach from
	 */
	public removeSlotObject (slotOrContainer: number | string | Slot | Container) {
		let containerToRemove: Container | undefined;

		if (slotOrContainer instanceof Container) {
			for (const i in this._slotsObject) {
				if (this._slotsObject[i]?.container === slotOrContainer) {
					this._slotsObject[i] = null;

					containerToRemove = slotOrContainer;
					break;
				}
			}
		}
		else {
			const slot = this.getSlotFromRef(slotOrContainer);

			containerToRemove = this._slotsObject[slot.data.name]?.container;
			this._slotsObject[slot.data.name] = null;
		}

		if (containerToRemove) {
			this.removeChild(containerToRemove);

			containerToRemove.includeInBuild = true;
		}
	}

	/**
	 * Removes all PixiJS containers attached to any slot.
	 */
	public removeSlotObjects () {
		Object.entries(this._slotsObject).forEach(([slotName, slotObject]) => {
			if (slotObject) slotObject.container.removeFromParent();
			delete this._slotsObject[slotName];
		});
	}

	/**
	 * Returns a container attached to a slot, or undefined if no container is attached.
	 *
	 * @param slotRef - The slot id or slot to get the attachment from
	 * @returns - The container attached to the slot
	 */
	public getSlotObject (slot: number | string | Slot) {
		slot = this.getSlotFromRef(slot);

		return this._slotsObject[slot.data.name]?.container;
	}

	protected updateBounds () {
		this._boundsDirty = false;

		this.skeletonBounds ||= new SkeletonBounds();

		const skeletonBounds = this.skeletonBounds;

		skeletonBounds.update(this.skeleton, true);

		if (this._boundsProvider) {
			const boundsSpine = this._boundsProvider.calculateBounds(this);

			const bounds = this._bounds;
			bounds.clear();

			bounds.x = boundsSpine.x;
			bounds.y = boundsSpine.y;
			bounds.width = boundsSpine.width;
			bounds.height = boundsSpine.height;

		} else if (skeletonBounds.minX === Infinity) {
			if (this.hasNeverUpdated) {
				this._updateAndApplyState(0);
				this._boundsDirty = false;
			}
			this._validateAndTransformAttachments();

			const drawOrder = this.skeleton.drawOrder.appliedPose;
			const bounds = this._bounds;

			bounds.clear();

			for (let i = 0; i < drawOrder.length; i++) {
				const slot = drawOrder[i];

				const attachment = slot.appliedPose.attachment;

				if (attachment && (attachment instanceof RegionAttachment || attachment instanceof MeshAttachment)) {
					const cacheData = this._getCachedData(slot, attachment);

					bounds.addVertexData(cacheData.vertices, 0, cacheData.vertices.length);
				}
			}
		}
		else {
			this._bounds.minX = skeletonBounds.minX;
			this._bounds.minY = skeletonBounds.minY;
			this._bounds.maxX = skeletonBounds.maxX;
			this._bounds.maxY = skeletonBounds.maxY;
		}
	}

	/** @internal */
	addBounds (bounds: Bounds) {
		bounds.addBounds(this.bounds);
	}

	/**
	 * Destroys this sprite renderable and optionally its texture.
	 * @param options - Options parameter. A boolean will act as if all options
	 *  have been set to that value
	 * @param {boolean} [options.texture=false] - Should it destroy the current texture of the renderable as well
	 * @param {boolean} [options.textureSource=false] - Should it destroy the textureSource of the renderable as well
	 */
	public override destroy (options: DestroyOptions = false) {
		super.destroy(options);

		this._ticker.remove(this.internalUpdate, this);
		(this._ticker as unknown) = null;
		this.state.clearListeners();
		this.debug = undefined;
		(this.skeleton as unknown) = null;
		(this.state as unknown) = null;
		(this._slotsObject as unknown) = null;
		(this.attachmentCacheData as unknown) = null;
		this._lastAttachments.length = 0;
	}

	/** Converts a point from the skeleton coordinate system to the Pixi world coordinate system. */
	public skeletonToPixiWorldCoordinates (point: { x: number; y: number }) {
		this.worldTransform.apply(point, point);
	}

	/** Converts a point from the Pixi world coordinate system to the skeleton coordinate system. */
	public pixiWorldCoordinatesToSkeleton (point: { x: number; y: number }) {
		this.worldTransform.applyInverse(point, point);
	}

	/** Converts a point from the Pixi world coordinate system to the bone's local coordinate system. */
	public pixiWorldCoordinatesToBone (point: { x: number; y: number }, bone: Bone) {
		this.pixiWorldCoordinatesToSkeleton(point);
		if (bone.parent) {
			bone.parent.appliedPose.worldToLocal(point as Vector2);
		}
		else {
			bone.appliedPose.worldToLocal(point as Vector2);
		}
	}

	/**
	 * Get a convenient initialization configuration for your Spine game object.
	 * Before instantiating a Spine game object, the skeleton (`.skel` or `.json`) and the atlas text files must be loaded into the {@link Assets}. For example:
	 * ```
	 * PIXI.Assets.add("sackData", "/assets/sack-pro.skel");
	 * PIXI.Assets.add("sackAtlas", "/assets/sack-pma.atlas");
	 * await PIXI.Assets.load(["sackData", "sackAtlas"]);
	 * ```
	 * Once a Spine game object is created, its skeleton data is cached into {@link Cache} using the key:
	 * `${skeletonAssetName}-${atlasAssetName}-${options?.scale ?? 1}`
	 *
	 * @param options - Options to configure the Spine game object. See {@link SpineFromOptions}
	 * @returns {SpineOptions} The configuration ready to be passed to the Spine constructor
	 */
	static createOptions ({ skeleton, atlas, scale = 1, darkTint, autoUpdate = true, boundsProvider, allowMissingRegions = false, ticker }: SpineFromOptions): SpineOptions {
		const cacheKey = `${skeleton}-${atlas}-${scale}`;

		if (Cache.has(cacheKey)) {
			return {
				skeletonData: Cache.get<SkeletonData>(cacheKey),
				darkTint,
				autoUpdate,
				boundsProvider,
				ticker,
			};
		}

		const atlasAsset = Assets.get<TextureAtlas>(atlas);
		const attachmentLoader = new AtlasAttachmentLoader(atlasAsset, allowMissingRegions);

		// biome-ignore lint/suspicious/noExplicitAny: json skeleton data is any
		const skeletonAsset = Assets.get<any | Uint8Array>(skeleton);
		const parser = skeletonAsset instanceof Uint8Array
			? new SkeletonBinary(attachmentLoader)
			: new SkeletonJson(attachmentLoader);

		parser.scale = scale;
		const skeletonData = parser.readSkeletonData(skeletonAsset);

		Cache.set(cacheKey, skeletonData);

		return {
			skeletonData,
			darkTint,
			autoUpdate,
			boundsProvider,
			ticker,
		};
	}

	/**
	 * @deprecated Use directly the Spine constructor or {@link createOptions} to make options and customize it to pass to the constructor
	 * Use this method to instantiate a Spine game object.
	 * Before instantiating a Spine game object, the skeleton (`.skel` or `.json`) and the atlas text files must be loaded into the Assets. For example:
	 * ```
	 * PIXI.Assets.add("sackData", "/assets/sack-pro.skel");
	 * PIXI.Assets.add("sackAtlas", "/assets/sack-pma.atlas");
	 * await PIXI.Assets.load(["sackData", "sackAtlas"]);
	 * ```
	 * Once a Spine game object is created, its skeleton data is cached into {@link Cache} using the key:
	 * `${skeletonAssetName}-${atlasAssetName}-${options?.scale ?? 1}`
	 *
	 * @param options - Options to configure the Spine game object. See {@link SpineFromOptions}
	 * @returns {Spine} The Spine game object instantiated
	 */
	static from (options: SpineFromOptions) {
		return new Spine(Spine.createOptions(options));
	}
}
