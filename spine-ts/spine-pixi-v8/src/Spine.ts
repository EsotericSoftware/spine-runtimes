/** ****************************************************************************
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

import {
	Assets,
	Bounds,
	Cache,
	Container,
	ContainerOptions,
	DEG_TO_RAD,
	DestroyOptions,
	fastCopy,
	PointData,
	Texture,
	Ticker,
	ViewContainer,
} from 'pixi.js';
import { ISpineDebugRenderer } from './SpineDebugRenderer';
import {
	AnimationState,
	AnimationStateData,
	AtlasAttachmentLoader,
	Attachment,
	Bone,
	ClippingAttachment,
	Color,
	MeshAttachment,
	Physics,
	RegionAttachment,
	Skeleton,
	SkeletonBinary,
	SkeletonBounds,
	SkeletonClipping,
	SkeletonData,
	SkeletonJson,
	Slot,
	type TextureAtlas,
	TrackEntry,
	Vector2,
} from '@esotericsoftware/spine-core';

export type SpineFromOptions = {
	skeleton: string;
	atlas: string;
	scale?: number;
};

const vectorAux = new Vector2();
const lightColor = new Color();
const darkColor = new Color();

Skeleton.yDown = true;

const clipper = new SkeletonClipping();

export interface SpineOptions extends ContainerOptions {
	skeletonData: SkeletonData;
	autoUpdate?: boolean;
}

export interface SpineEvents {
	complete: [trackEntry: TrackEntry];
	dispose: [trackEntry: TrackEntry];
	end: [trackEntry: TrackEntry];
	event: [trackEntry: TrackEntry, event: Event];
	interrupt: [trackEntry: TrackEntry];
	start: [trackEntry: TrackEntry];
}

export interface AttachmentCacheData {
	id: string;
	clipped: boolean;
	vertices: Float32Array;
	uvs: Float32Array;
	indices: number[];
	color: Color;
	darkColor: Color | null;
	darkTint: boolean;
	skipRender: boolean;
	texture: Texture;
	clippedData?: {
		vertices: Float32Array;
		uvs: Float32Array;
		indices: Uint16Array;
		vertexCount: number;
		indicesCount: number;
	};
}

export class Spine extends ViewContainer {
	// Pixi properties
	public batched = true;
	public buildId = 0;
	public override readonly renderPipeId = 'spine';
	public _didSpineUpdate = false;

	public beforeUpdateWorldTransforms: (object: Spine) => void = () => { /** */ };
	public afterUpdateWorldTransforms: (object: Spine) => void = () => { /** */ };

	// Spine properties
	public skeleton: Skeleton;
	public state: AnimationState;
	public skeletonBounds?: SkeletonBounds;
	private _debug?: ISpineDebugRenderer | undefined = undefined;

	readonly _slotsObject: Record<string, { slot: Slot, container: Container } | null> = Object.create(null);

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

	public set debug (value: ISpineDebugRenderer | undefined) {
		if (this._debug) {
			this._debug.unregisterSpine(this);
		}
		if (value) {
			value.registerSpine(this);
		}
		this._debug = value;
	}

	private autoUpdateWarned = false;
	private _autoUpdate = true;

	public get autoUpdate (): boolean {
		return this._autoUpdate;
	}

	public set autoUpdate (value: boolean) {
		if (value) {
			Ticker.shared.add(this.internalUpdate, this);
			this.autoUpdateWarned = false;
		}
		else {
			Ticker.shared.remove(this.internalUpdate, this);
		}

		this._autoUpdate = value;
	}

	constructor (options: SpineOptions | SkeletonData) {
		if (options instanceof SkeletonData) {
			options = {
				skeletonData: options,
			};
		}

		super();

		const skeletonData = options instanceof SkeletonData ? options : options.skeletonData;

		this.skeleton = new Skeleton(skeletonData);
		this.state = new AnimationState(new AnimationStateData(skeletonData));
		this.autoUpdate = options?.autoUpdate ?? true;

		const slots = this.skeleton.slots;

		for (let i = 0; i < slots.length; i++) {
			this.attachmentCacheData[i] = Object.create(null);
		}

		this._updateState(0);
	}

	public update (dt: number): void {
		if (this.autoUpdate && !this.autoUpdateWarned) {
			console.warn(
				// eslint-disable-next-line max-len
				'You are calling update on a Spine instance that has autoUpdate set to true. This is probably not what you want.',
			);
			this.autoUpdateWarned = true;
		}

		this.internalUpdate(0, dt);
	}

	protected internalUpdate (_deltaFrame: any, deltaSeconds?: number): void {
		// Because reasons, pixi uses deltaFrames at 60fps.
		// We ignore the default deltaFrames and use the deltaSeconds from pixi ticker.
		this._updateState(deltaSeconds ?? Ticker.shared.deltaMS / 1000);
	}

	get bounds () {
		if (this._boundsDirty) {
			this.updateBounds();
		}

		return this._bounds;
	}

	public setBonePosition (bone: string | Bone, position: PointData): void {
		const boneAux = bone;

		if (typeof bone === 'string') {
			bone = this.skeleton.findBone(bone) as Bone;
		}

		if (!bone) throw Error(`Cant set bone position, bone ${String(boneAux)} not found`);
		vectorAux.set(position.x, position.y);

		if (bone.parent) {
			const aux = bone.parent.worldToLocal(vectorAux);

			bone.x = aux.x;
			bone.y = -aux.y;
		}
		else {
			bone.x = vectorAux.x;
			bone.y = vectorAux.y;
		}
	}

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

		outPos.x = bone.worldX;
		outPos.y = bone.worldY;

		return outPos;
	}

	/**
	 * Will update the state based on the specified time, this will not apply the state to the skeleton
	 * as this is differed until the `applyState` method is called.
	 *
	 * @param time the time at which to set the state
	 * @internal
	 */
	_updateState (time: number) {
		this.state.update(time);
		this.skeleton.update(time);

		this._stateChanged = true;

		this._boundsDirty = true;

		this.onViewUpdate();
	}

	/**
	 * Applies the state to this spine instance.
	 * - updates the state to the skeleton
	 * - updates its world transform (spine world transform)
	 * - validates the attachments - to flag if the attachments have changed this state
	 * - transforms the attachments - to update the vertices of the attachments based on the new positions
	 * - update the slot attachments - to update the position, rotation, scale, and visibility of the attached containers
	 * @internal
	 */
	_applyState () {
		if (!this._stateChanged) return;
		this._stateChanged = false;

		const { skeleton } = this;

		this.state.apply(skeleton);

		this.beforeUpdateWorldTransforms(this);
		skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);

		this.validateAttachments();

		this.transformAttachments();

		this.updateSlotObjects();
	}

	private validateAttachments () {
		const currentDrawOrder = this.skeleton.drawOrder;

		const lastAttachments = this._lastAttachments;

		let index = 0;

		let spineAttachmentsDirty = false;

		for (let i = 0; i < currentDrawOrder.length; i++) {
			const slot = currentDrawOrder[i];
			const attachment = slot.getAttachment();

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

		this.spineAttachmentsDirty = spineAttachmentsDirty;
	}

	private transformAttachments () {
		const currentDrawOrder = this.skeleton.drawOrder;

		for (let i = 0; i < currentDrawOrder.length; i++) {
			const slot = currentDrawOrder[i];

			const attachment = slot.getAttachment();

			if (attachment) {
				if (attachment instanceof MeshAttachment || attachment instanceof RegionAttachment) {
					const cacheData = this._getCachedData(slot, attachment);

					if (attachment instanceof RegionAttachment) {
						attachment.computeWorldVertices(slot, cacheData.vertices, 0, 2);
					}
					else {
						attachment.computeWorldVertices(
							slot,
							0,
							attachment.worldVerticesLength,
							cacheData.vertices,
							0,
							2,
						);
					}

					fastCopy((attachment.uvs as Float32Array).buffer, cacheData.uvs.buffer);

					const skeleton = slot.bone.skeleton;
					const skeletonColor = skeleton.color;
					const slotColor = slot.color;

					const attachmentColor = attachment.color;

					cacheData.color.set(
						skeletonColor.r * slotColor.r * attachmentColor.r,
						skeletonColor.g * slotColor.g * attachmentColor.g,
						skeletonColor.b * slotColor.b * attachmentColor.b,
						skeletonColor.a * slotColor.a * attachmentColor.a,
					);

					cacheData.darkTint = !!slot.darkColor;

					if (slot.darkColor) {
						cacheData.darkColor!.setFromColor(slot.darkColor);
					}

					cacheData.skipRender = cacheData.clipped = false;

					const texture = attachment.region?.texture.texture || Texture.EMPTY;

					if (cacheData.texture !== texture) {
						cacheData.texture = texture;
						this.spineTexturesDirty = true;
					}

					if (clipper.isClipping()) {
						this.updateClippingData(cacheData);
					}
				}
				else if (attachment instanceof ClippingAttachment) {
					clipper.clipStart(slot, attachment);
					continue;
				}
			}
			clipper.clipEndWithSlot(slot);
		}
		clipper.clipEnd();
	}

	private updateClippingData (cacheData: AttachmentCacheData) {
		cacheData.clipped = true;

		clipper.clipTriangles(
			cacheData.vertices,
			cacheData.indices,
			cacheData.indices.length,
			cacheData.uvs,
			lightColor,
			darkColor,
			cacheData.darkTint,
		);

		const { clippedVertices, clippedTriangles } = clipper;

		const verticesCount = clippedVertices.length / 8;
		const indicesCount = clippedTriangles.length;

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

		for (let i = 0; i < verticesCount; i++) {
			vertices[i * 2] = clippedVertices[i * 8];
			vertices[(i * 2) + 1] = clippedVertices[(i * 8) + 1];

			uvs[i * 2] = clippedVertices[(i * 8) + 6];
			uvs[(i * 2) + 1] = clippedVertices[(i * 8) + 7];
		}

		clippedData.vertexCount = verticesCount;

		for (let i = 0; i < indices.length; i++) {
			indices[i] = clippedTriangles[i];
		}

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

	private updateSlotObject (slotAttachment: { slot: Slot, container: Container }) {
		const { slot, container } = slotAttachment;

		container.visible = this.skeleton.drawOrder.includes(slot);

		if (container.visible) {
			const bone = slot.bone;

			container.position.set(bone.worldX, bone.worldY);

			container.scale.x = bone.getWorldScaleX();
			container.scale.y = bone.getWorldScaleY();

			container.rotation = bone.getWorldRotationX() * DEG_TO_RAD;

			container.alpha = this.skeleton.color.a * slot.color.a;
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

			this.attachmentCacheData[slot.data.index][attachment.name] = {
				id: `${slot.data.index}-${attachment.name}`,
				vertices,
				clipped: false,
				indices: [0, 1, 2, 0, 2, 3],
				uvs: new Float32Array(attachment.uvs.length),
				color: new Color(1, 1, 1, 1),
				darkColor: new Color(0, 0, 0, 0),
				darkTint: false,
				skipRender: false,
				texture: attachment.region?.texture.texture,
			};
		}
		else {
			vertices = new Float32Array(attachment.worldVerticesLength);

			this.attachmentCacheData[slot.data.index][attachment.name] = {
				id: `${slot.data.index}-${attachment.name}`,
				vertices,
				clipped: false,
				indices: attachment.triangles,
				uvs: new Float32Array(attachment.uvs.length),
				color: new Color(1, 1, 1, 1),
				darkColor: new Color(0, 0, 0, 0),
				darkTint: false,
				skipRender: false,
				texture: attachment.region?.texture.texture,
			};
		}

		return this.attachmentCacheData[slot.data.index][attachment.name];
	}

	protected onViewUpdate () {
		// increment from the 12th bit!
		this._didChangeId += 1 << 12;

		this._boundsDirty = true;

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
	 */
	public addSlotObject (slot: number | string | Slot, container: Container) {
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

		const slotObject = { container, slot };
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
	 * Returns a container attached to a slot, or undefined if no container is attached.
	 *
	 * @param slotRef - The slot id or slot to get the attachment from
	 * @returns - The container attached to the slot
	 */
	public getSlotObject (slot: number | string | Slot) {
		slot = this.getSlotFromRef(slot);

		return this._slotsObject[slot.data.name]?.container;
	}

	private updateBounds () {
		this._boundsDirty = false;

		this.skeletonBounds ||= new SkeletonBounds();

		const skeletonBounds = this.skeletonBounds;

		skeletonBounds.update(this.skeleton, true);

		if (skeletonBounds.minX === Infinity) {
			this._applyState();

			const drawOrder = this.skeleton.drawOrder;
			const bounds = this._bounds;

			bounds.clear();

			for (let i = 0; i < drawOrder.length; i++) {
				const slot = drawOrder[i];

				const attachment = slot.getAttachment();

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

		Ticker.shared.remove(this.internalUpdate, this);
		this.state.clearListeners();
		this.debug = undefined;
		this.skeleton = null as any;
		this.state = null as any;
		(this._slotsObject as any) = null;
		this._lastAttachments.length = 0;
		this.attachmentCacheData = null as any;
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
			bone.parent.worldToLocal(point as Vector2);
		}
		else {
			bone.worldToLocal(point as Vector2);
		}
	}

	static from ({ skeleton, atlas, scale = 1 }: SpineFromOptions) {
		const cacheKey = `${skeleton}-${atlas}-${scale}`;

		if (Cache.has(cacheKey)) {
			return new Spine(Cache.get<SkeletonData>(cacheKey));
		}

		const skeletonAsset = Assets.get<any | Uint8Array>(skeleton);

		const atlasAsset = Assets.get<TextureAtlas>(atlas);
		const attachmentLoader = new AtlasAttachmentLoader(atlasAsset);
		// eslint-disable-next-line max-len
		const parser
			= skeletonAsset instanceof Uint8Array
				? new SkeletonBinary(attachmentLoader)
				: new SkeletonJson(attachmentLoader);

		// TODO scale?
		parser.scale = scale;
		const skeletonData = parser.readSkeletonData(skeletonAsset);

		Cache.set(cacheKey, skeletonData);

		return new Spine({
			skeletonData,
		});
	}
}
