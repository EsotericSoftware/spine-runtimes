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

import type { Attachment } from "./attachments/Attachment.js";
import { ClippingAttachment } from "./attachments/ClippingAttachment.js";
import { MeshAttachment } from "./attachments/MeshAttachment.js";
import { RegionAttachment } from "./attachments/RegionAttachment.js";
import { Bone } from "./Bone.js";
import type { Constraint } from "./Constraint.js";
import type { Physics } from "./Physics.js";
import { PhysicsConstraint } from "./PhysicsConstraint.js";
import type { Posed } from "./Posed.js";
import type { SkeletonClipping } from "./SkeletonClipping.js";
import type { SkeletonData } from "./SkeletonData.js";
import type { Skin } from "./Skin.js";
import { Slot } from "./Slot.js";
import { Color, type NumberArrayLike, Utils, Vector2 } from "./Utils.js";

/** Stores the current pose for a skeleton.
 *
 * See [Instance objects](http://esotericsoftware.com/spine-runtime-architecture#Instance-objects) in the Spine Runtimes Guide. */
export class Skeleton {
	private static quadTriangles = [0, 1, 2, 2, 3, 0];
	static yDown = false;
	static get yDir (): number {
		return Skeleton.yDown ? -1 : 1;
	}

	/** The skeleton's setup pose data. */
	readonly data: SkeletonData;

	/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
	readonly bones: Array<Bone>;

	/** The skeleton's slots. */
	readonly slots: Array<Slot>;

	/** The skeleton's slots in the order they should be drawn. The returned array may be modified to change the draw order. */
	drawOrder: Array<Slot>;

	/** The skeleton's constraints. */
	// biome-ignore lint/suspicious/noExplicitAny: reference runtime does not restrict to specific types
	readonly constraints: Array<Constraint<any, any, any>>;

	/** The skeleton's physics constraints. */
	readonly physics: Array<PhysicsConstraint>;

	/** The list of bones and constraints, sorted in the order they should be updated, as computed by {@link updateCache()}. */
	// biome-ignore lint/suspicious/noExplicitAny: reference runtime does not restrict to specific types
	readonly _updateCache = [] as any[];

	// biome-ignore lint/suspicious/noExplicitAny: reference runtime does not restrict to specific types
	readonly resetCache: Array<Posed<any, any, any>> = [];

	/** The skeleton's current skin. May be null. */
	skin: Skin | null = null;

	/** The color to tint all the skeleton's attachments. */
	readonly color: Color;

	/** Scales the entire skeleton on the X axis.
	 *
	 * Bones that do not inherit scale are still affected by this property. */
	scaleX = 1;

	private _scaleY = 1;

	/** Scales the entire skeleton on the Y axis.
	 *
	 * Bones that do not inherit scale are still affected by this property. */
	public get scaleY () {
		return this._scaleY * Skeleton.yDir;
	}

	public set scaleY (scaleY: number) {
		this._scaleY = scaleY;
	}

	/** Sets the skeleton X position, which is added to the root bone worldX position.
	 *
	 * Bones that do not inherit translation are still affected by this property. */
	x = 0;

	/** Sets the skeleton Y position, which is added to the root bone worldY position.
	 *
	 * Bones that do not inherit translation are still affected by this property. */
	y = 0;

	/** Returns the skeleton's time. This is used for time-based manipulations, such as {@link PhysicsConstraint}.
	 *
	 * See {@link _update()}. */
	time = 0;

	windX = 1;
	windY = 0;
	gravityX = 0;
	gravityY = 1;

	_update = 0;

	constructor (data: SkeletonData) {
		if (!data) throw new Error("data cannot be null.");
		this.data = data;

		this.bones = [] as Bone[];
		for (let i = 0; i < data.bones.length; i++) {
			const boneData = data.bones[i];
			let bone: Bone;
			if (!boneData.parent)
				bone = new Bone(boneData, null);
			else {
				const parent = this.bones[boneData.parent.index];
				bone = new Bone(boneData, parent);
				parent.children.push(bone);
			}
			this.bones.push(bone);
		}

		this.slots = [] as Slot[];
		this.drawOrder = [] as Slot[];
		for (const slotData of this.data.slots) {
			const slot = new Slot(slotData, this);
			this.slots.push(slot);
			this.drawOrder.push(slot);
		}

		this.physics = [] as PhysicsConstraint[];
		// biome-ignore lint/suspicious/noExplicitAny: reference runtime does not restrict to specific types
		this.constraints = [] as Constraint<any, any, any>[];
		for (const constraintData of this.data.constraints) {
			const constraint = constraintData.create(this);
			if (constraint instanceof PhysicsConstraint) this.physics.push(constraint);
			this.constraints.push(constraint);
		}

		this.color = new Color(1, 1, 1, 1);

		this.updateCache();
	}

	/** Caches information about bones and constraints. Must be called if the {@link getSkin()} is modified or if bones,
	 * constraints, or weighted path attachments are added or removed. */
	updateCache () {
		this._updateCache.length = 0;
		this.resetCache.length = 0;

		const slots = this.slots;
		for (let i = 0, n = slots.length; i < n; i++)
			slots[i].usePose();

		const bones = this.bones;
		const boneCount = bones.length;
		for (let i = 0, n = boneCount; i < n; i++) {
			const bone = bones[i];
			bone.sorted = bone.data.skinRequired;
			bone.active = !bone.sorted;
			bone.usePose();
		}
		if (this.skin) {
			const skinBones = this.skin.bones;
			for (let i = 0, n = this.skin.bones.length; i < n; i++) {
				let bone: Bone | null = this.bones[skinBones[i].index];
				do {
					bone.sorted = false;
					bone.active = true;
					bone = bone.parent;
				} while (bone);
			}
		}

		const constraints = this.constraints;
		let n = this.constraints.length;
		for (let i = 0; i < n; i++)
			constraints[i].usePose();
		for (let i = 0; i < n; i++) {
			const constraint = constraints[i];
			constraint.active = constraint.isSourceActive()
				// biome-ignore lint/complexity/useOptionalChain: changing to this might return undefined
				&& (!constraint.data.skinRequired || (this.skin != null && this.skin.constraints.includes(constraint.data)));
			if (constraint.active) constraint.sort(this);
		}

		for (let i = 0; i < boneCount; i++)
			this.sortBone(bones[i]);

		n = this._updateCache.length;
		for (let i = 0; i < n; i++) {
			const updateable = this._updateCache[i];
			if (updateable instanceof Bone) this._updateCache[i] = updateable.applied;
		}

	}

	// biome-ignore lint/suspicious/noExplicitAny: reference runtime does not restrict to specific types
	constrained (object: Posed<any, any, any>) {
		if (object.pose === object.applied) {
			object.useConstrained();
			this.resetCache.push(object);
		}
	}

	sortBone (bone: Bone) {
		if (bone.sorted || !bone.active) return;
		const parent = bone.parent;
		if (parent) this.sortBone(parent);
		bone.sorted = true;
		this._updateCache.push(bone);
	}

	sortReset (bones: Array<Bone>) {
		for (let i = 0, n = bones.length; i < n; i++) {
			const bone = bones[i];
			if (bone.active) {
				if (bone.sorted) this.sortReset(bone.children);
				bone.sorted = false;
			}
		}
	}

	/** Updates the world transform for each bone and applies all constraints.
	 * <p>
	 * See <a href="https://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide. */
	updateWorldTransform (physics: Physics): void {
		this._update++;

		const resetCache = this.resetCache;
		for (let i = 0, n = this.resetCache.length; i < n; i++)
			resetCache[i].resetConstrained();

		const updateCache = this._updateCache;
		for (let i = 0, n = this._updateCache.length; i < n; i++)
			updateCache[i].update(this, physics);
	}

	/** Sets the bones, constraints, and slots to their setup pose values. */
	setupPose () {
		this.setupPoseBones();
		this.setupPoseSlots();
	}

	/** Sets the bones and constraints to their setup pose values. */
	setupPoseBones () {
		const bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++)
			bones[i].setupPose();

		const constraints = this.constraints;
		for (let i = 0, n = constraints.length; i < n; i++)
			constraints[i].setupPose();
	}

	/** Sets the slots and draw order to their setup pose values. */
	setupPoseSlots () {
		const slots = this.slots;
		Utils.arrayCopy(slots, 0, this.drawOrder, 0, slots.length);
		for (let i = 0, n = slots.length; i < n; i++)
			slots[i].setupPose();
	}

	/** Returns the root bone, or null if the skeleton has no bones. */
	getRootBone () {
		if (this.bones.length === 0) return null;
		return this.bones[0];
	}

	/** Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly. */
	findBone (boneName: string) {
		if (!boneName) throw new Error("boneName cannot be null.");
		const bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++)
			if (bones[i].data.name === boneName) return bones[i];
		return null;
	}

	/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly. */
	findSlot (slotName: string) {
		if (!slotName) throw new Error("slotName cannot be null.");
		const slots = this.slots;
		for (let i = 0, n = slots.length; i < n; i++)
			if (slots[i].data.name === slotName) return slots[i];
		return null;
	}

	/** Sets a skin by name.
	 *
	 * See {@link setSkin()}. */
	setSkin (skinName: string): void;

	/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}. If the
	 * skin is changed, {@link updateCache} is called.
	 * <p>
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
	 * old skin, each slot's setup mode attachment is attached from the new skin.
	 * <p>
	 * After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
	 * {@link setupPoseSlots()}. Also, often {@link AnimationState.apply(Skeleton)} is called before the next time the skeleton is
	 * rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin. */
	setSkin (newSkin: Skin | null): void;

	setSkin (newSkin: Skin | null | string): void {
		if (typeof newSkin === "string")
			this.setSkinByName(newSkin);
		else
			this.setSkinBySkin(newSkin);
	};

	private setSkinByName (skinName: string) {
		const skin = this.data.findSkin(skinName);
		if (!skin) throw new Error(`Skin not found: ${skinName}`);
		this.setSkin(skin);
	}

	private setSkinBySkin (newSkin: Skin | null) {
		if (newSkin === this.skin) return;
		if (newSkin) {
			if (this.skin)
				newSkin.attachAll(this, this.skin);
			else {
				const slots = this.slots;
				for (let i = 0, n = slots.length; i < n; i++) {
					const slot = slots[i];
					const name = slot.data.attachmentName;
					if (name) {
						const attachment = newSkin.getAttachment(i, name);
						if (attachment) slot.pose.setAttachment(attachment);
					}
				}
			}
		}
		this.skin = newSkin;
		this.updateCache();
	}

	/** Finds an attachment by looking in the {@link skin} and {@link SkeletonData.defaultSkin} using the slot name and attachment
	 * name.
	 *
	 * See {@link getAttachment(number, string)}. */
	getAttachment (slotName: string, attachmentName: string): Attachment | null;

	/** Finds an attachment by looking in the {@link skin} and {@link SkeletonData.defaultSkin} using the slot index and
	 * attachment name. First the skin is checked and if the attachment was not found, the default skin is checked.
	 *
	 * See <a href="https://esotericsoftware.com/spine-runtime-skins">Runtime skins</a> in the Spine Runtimes Guide. */
	getAttachment (slotIndex: number, attachmentName: string): Attachment | null;

	getAttachment (slotNameOrIndex: string | number, attachmentName: string): Attachment | null {
		if (typeof slotNameOrIndex === 'string')
			return this.getAttachmentByName(slotNameOrIndex, attachmentName);
		return this.getAttachmentByIndex(slotNameOrIndex, attachmentName);
	}

	/** Finds an attachment by looking in the {@link #skin} and {@link SkeletonData#defaultSkin} using the slot name and attachment
	 * name.
	 *
	 * See {@link #getAttachment()}.
	 * @returns May be null. */
	private getAttachmentByName (slotName: string, attachmentName: string): Attachment | null {
		const slot = this.data.findSlot(slotName);
		if (!slot) throw new Error(`Can't find slot with name ${slotName}`);
		return this.getAttachment(slot.index, attachmentName);
	}

	/** Finds an attachment by looking in the {@link #skin} and {@link SkeletonData#defaultSkin} using the slot index and
	 * attachment name. First the skin is checked and if the attachment was not found, the default skin is checked.
	 *
	 * See [Runtime skins](http://esotericsoftware.com/spine-runtime-skins) in the Spine Runtimes Guide.
	 * @returns May be null. */
	private getAttachmentByIndex (slotIndex: number, attachmentName: string): Attachment | null {
		if (!attachmentName) throw new Error("attachmentName cannot be null.");
		if (this.skin) {
			const attachment = this.skin.getAttachment(slotIndex, attachmentName);
			if (attachment) return attachment;
		}
		if (this.data.defaultSkin) return this.data.defaultSkin.getAttachment(slotIndex, attachmentName);
		return null;
	}

	/** A convenience method to set an attachment by finding the slot with {@link findSlot()}, finding the attachment with
	 * {@link getAttachment()}, then setting the slot's {@link Slot.attachment}.
	 * @param attachmentName May be null to clear the slot's attachment. */
	setAttachment (slotName: string, attachmentName: string) {
		if (!slotName) throw new Error("slotName cannot be null.");
		const slot = this.findSlot(slotName);
		if (!slot) throw new Error(`Slot not found: ${slotName}`);
		let attachment: Attachment | null = null;
		if (attachmentName) {
			attachment = this.getAttachment(slot.data.index, attachmentName);
			if (!attachment)
				throw new Error(`Attachment not found: ${attachmentName}, for slot: ${slotName}`);
		}
		slot.pose.setAttachment(attachment);
	}

	// biome-ignore lint/suspicious/noExplicitAny: reference runtime does not restrict to specific types
	findConstraint<T extends Constraint<any, any, any>> (constraintName: string, type: new () => T): T | null {
		if (constraintName == null) throw new Error("constraintName cannot be null.");
		if (type == null) throw new Error("type cannot be null.");
		const constraints = this.constraints;
		for (let i = 0, n = constraints.length; i < n; i++) {
			const constraint = constraints[i];
			if (constraint instanceof type && constraint.data.name === constraintName) return constraint as T;
		}
		return null;
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose as `{ x: number, y: number, width: number, height: number }`.
	 * Note that this method will create temporary objects which can add to garbage collection pressure. Use `getBounds()` if garbage collection is a concern. */
	getBoundsRect (clipper?: SkeletonClipping) {
		const offset = new Vector2();
		const size = new Vector2();
		this.getBounds(offset, size, undefined, clipper);
		return { x: offset.x, y: offset.y, width: size.x, height: size.y };
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
	 * @param offset An output value, the distance from the skeleton origin to the bottom left corner of the AABB.
	 * @param size An output value, the width and height of the AABB.
	 * @param temp Working memory to temporarily store attachments' computed world vertices.
	 * @param clipper {@link SkeletonClipping} to use. If <code>null</code>, no clipping is applied. */
	getBounds (offset: Vector2, size: Vector2, temp: Array<number> = new Array<number>(2), clipper: SkeletonClipping | null = null) {
		if (!offset) throw new Error("offset cannot be null.");
		if (!size) throw new Error("size cannot be null.");
		const drawOrder = this.drawOrder;
		let minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
		for (let i = 0, n = drawOrder.length; i < n; i++) {
			const slot = drawOrder[i];
			if (!slot.bone.active) continue;
			let verticesLength = 0;
			let vertices: NumberArrayLike | null = null;
			let triangles: NumberArrayLike | null = null;
			const attachment = slot.pose.attachment;
			if (attachment) {
				if (attachment instanceof RegionAttachment) {
					verticesLength = 8;
					vertices = Utils.setArraySize(temp, verticesLength, 0);
					attachment.computeWorldVertices(slot, vertices, 0, 2);
					triangles = Skeleton.quadTriangles;
				} else if (attachment instanceof MeshAttachment) {
					verticesLength = attachment.worldVerticesLength;
					vertices = Utils.setArraySize(temp, verticesLength, 0);
					attachment.computeWorldVertices(this, slot, 0, verticesLength, vertices, 0, 2);
					triangles = attachment.triangles;
				} else if (attachment instanceof ClippingAttachment && clipper) {
					clipper.clipEnd(slot);
					clipper.clipStart(this, slot, attachment);
					continue;
				}
				if (vertices && triangles) {
					if (clipper?.isClipping() && clipper.clipTriangles(vertices, triangles, triangles.length)) {
						vertices = clipper.clippedVertices;
						verticesLength = clipper.clippedVertices.length;
					}
					for (let ii = 0, nn = vertices.length; ii < nn; ii += 2) {
						const x = vertices[ii], y = vertices[ii + 1];
						minX = Math.min(minX, x);
						minY = Math.min(minY, y);
						maxX = Math.max(maxX, x);
						maxY = Math.max(maxY, y);
					}
				}
			}
			if (clipper) clipper.clipEnd(slot);
		}
		if (clipper) clipper.clipEnd();
		offset.set(minX, minY);
		size.set(maxX - minX, maxY - minY);
	}

	/** Scales the entire skeleton on the X and Y axes.
	 *
	 * Bones that do not inherit scale are still affected by this property. */
	public setScale (scaleX: number, scaleY: number) {
		this.scaleX = scaleX;
		this.scaleY = scaleY;
	}

	/** Sets the skeleton X and Y position, which is added to the root bone worldX and worldY position.
	 *
	 * Bones that do not inherit translation are still affected by this property. */
	public setPosition (x: number, y: number) {
		this.x = x;
		this.y = y;
	}

	/** Increments the skeleton's {@link #time}. */
	update (delta: number) {
		this.time += delta;
	}

	/** Calls {@link PhysicsConstraint.translate} for each physics constraint. */
	physicsTranslate (x: number, y: number) {
		const constraints = this.physics;
		for (let i = 0, n = constraints.length; i < n; i++)
			constraints[i].translate(x, y);
	}

	/** Calls {@link PhysicsConstraint.rotate} for each physics constraint. */
	physicsRotate (x: number, y: number, degrees: number) {
		const constraints = this.physics;
		for (let i = 0, n = constraints.length; i < n; i++)
			constraints[i].rotate(x, y, degrees);
	}
}
