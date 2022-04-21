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

import { Attachment } from "./attachments/Attachment";
import { MeshAttachment } from "./attachments/MeshAttachment";
import { PathAttachment } from "./attachments/PathAttachment";
import { RegionAttachment } from "./attachments/RegionAttachment";
import { Bone } from "./Bone";
import { IkConstraint } from "./IkConstraint";
import { PathConstraint } from "./PathConstraint";
import { SkeletonData } from "./SkeletonData";
import { Skin } from "./Skin";
import { Slot } from "./Slot";
import { TransformConstraint } from "./TransformConstraint";
import { Updatable } from "./Updatable";
import { Color, Utils, MathUtils, Vector2, NumberArrayLike } from "./Utils";

/** Stores the current pose for a skeleton.
 *
 * See [Instance objects](http://esotericsoftware.com/spine-runtime-architecture#Instance-objects) in the Spine Runtimes Guide. */
export class Skeleton {
	/** The skeleton's setup pose data. */
	data: SkeletonData;

	/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
	bones: Array<Bone>;

	/** The skeleton's slots. */
	slots: Array<Slot>;

	/** The skeleton's slots in the order they should be drawn. The returned array may be modified to change the draw order. */
	drawOrder: Array<Slot>;

	/** The skeleton's IK constraints. */
	ikConstraints: Array<IkConstraint>;

	/** The skeleton's transform constraints. */
	transformConstraints: Array<TransformConstraint>;

	/** The skeleton's path constraints. */
	pathConstraints: Array<PathConstraint>;

	/** The list of bones and constraints, sorted in the order they should be updated, as computed by {@link #updateCache()}. */
	_updateCache = new Array<Updatable>();

	/** The skeleton's current skin. May be null. */
	skin: Skin | null = null;

	/** The color to tint all the skeleton's attachments. */
	color: Color;

	/** Scales the entire skeleton on the X axis. This affects all bones, even if the bone's transform mode disallows scale
	  * inheritance. */
	scaleX = 1;

	/** Scales the entire skeleton on the Y axis. This affects all bones, even if the bone's transform mode disallows scale
	  * inheritance. */
	scaleY = 1;

	/** Sets the skeleton X position, which is added to the root bone worldX position. */
	x = 0;

	/** Sets the skeleton Y position, which is added to the root bone worldY position. */
	y = 0;

	constructor (data: SkeletonData) {
		if (!data) throw new Error("data cannot be null.");
		this.data = data;

		this.bones = new Array<Bone>();
		for (let i = 0; i < data.bones.length; i++) {
			let boneData = data.bones[i];
			let bone: Bone;
			if (!boneData.parent)
				bone = new Bone(boneData, this, null);
			else {
				let parent = this.bones[boneData.parent.index];
				bone = new Bone(boneData, this, parent);
				parent.children.push(bone);
			}
			this.bones.push(bone);
		}

		this.slots = new Array<Slot>();
		this.drawOrder = new Array<Slot>();
		for (let i = 0; i < data.slots.length; i++) {
			let slotData = data.slots[i];
			let bone = this.bones[slotData.boneData.index];
			let slot = new Slot(slotData, bone);
			this.slots.push(slot);
			this.drawOrder.push(slot);
		}

		this.ikConstraints = new Array<IkConstraint>();
		for (let i = 0; i < data.ikConstraints.length; i++) {
			let ikConstraintData = data.ikConstraints[i];
			this.ikConstraints.push(new IkConstraint(ikConstraintData, this));
		}

		this.transformConstraints = new Array<TransformConstraint>();
		for (let i = 0; i < data.transformConstraints.length; i++) {
			let transformConstraintData = data.transformConstraints[i];
			this.transformConstraints.push(new TransformConstraint(transformConstraintData, this));
		}

		this.pathConstraints = new Array<PathConstraint>();
		for (let i = 0; i < data.pathConstraints.length; i++) {
			let pathConstraintData = data.pathConstraints[i];
			this.pathConstraints.push(new PathConstraint(pathConstraintData, this));
		}

		this.color = new Color(1, 1, 1, 1);
		this.updateCache();
	}

	/** Caches information about bones and constraints. Must be called if the {@link #getSkin()} is modified or if bones,
	 * constraints, or weighted path attachments are added or removed. */
	updateCache () {
		let updateCache = this._updateCache;
		updateCache.length = 0;

		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];
			bone.sorted = bone.data.skinRequired;
			bone.active = !bone.sorted;
		}

		if (this.skin) {
			let skinBones = this.skin.bones;
			for (let i = 0, n = this.skin.bones.length; i < n; i++) {
				let bone: Bone | null = this.bones[skinBones[i].index];
				do {
					bone.sorted = false;
					bone.active = true;
					bone = bone.parent;
				} while (bone);
			}
		}

		// IK first, lowest hierarchy depth first.
		let ikConstraints = this.ikConstraints;
		let transformConstraints = this.transformConstraints;
		let pathConstraints = this.pathConstraints;
		let ikCount = ikConstraints.length, transformCount = transformConstraints.length, pathCount = pathConstraints.length;
		let constraintCount = ikCount + transformCount + pathCount;

		outer:
		for (let i = 0; i < constraintCount; i++) {
			for (let ii = 0; ii < ikCount; ii++) {
				let constraint = ikConstraints[ii];
				if (constraint.data.order == i) {
					this.sortIkConstraint(constraint);
					continue outer;
				}
			}
			for (let ii = 0; ii < transformCount; ii++) {
				let constraint = transformConstraints[ii];
				if (constraint.data.order == i) {
					this.sortTransformConstraint(constraint);
					continue outer;
				}
			}
			for (let ii = 0; ii < pathCount; ii++) {
				let constraint = pathConstraints[ii];
				if (constraint.data.order == i) {
					this.sortPathConstraint(constraint);
					continue outer;
				}
			}
		}

		for (let i = 0, n = bones.length; i < n; i++)
			this.sortBone(bones[i]);
	}

	sortIkConstraint (constraint: IkConstraint) {
		constraint.active = constraint.target.isActive() && (!constraint.data.skinRequired || (this.skin && Utils.contains(this.skin.constraints, constraint.data, true)))!;
		if (!constraint.active) return;

		let target = constraint.target;
		this.sortBone(target);

		let constrained = constraint.bones;
		let parent = constrained[0];
		this.sortBone(parent);

		if (constrained.length == 1) {
			this._updateCache.push(constraint);
			this.sortReset(parent.children);
		} else {
			let child = constrained[constrained.length - 1];
			this.sortBone(child);

			this._updateCache.push(constraint);

			this.sortReset(parent.children);
			child.sorted = true;
		}
	}

	sortPathConstraint (constraint: PathConstraint) {
		constraint.active = constraint.target.bone.isActive() && (!constraint.data.skinRequired || (this.skin && Utils.contains(this.skin.constraints, constraint.data, true)))!;
		if (!constraint.active) return;

		let slot = constraint.target;
		let slotIndex = slot.data.index;
		let slotBone = slot.bone;
		if (this.skin) this.sortPathConstraintAttachment(this.skin, slotIndex, slotBone);
		if (this.data.defaultSkin && this.data.defaultSkin != this.skin)
			this.sortPathConstraintAttachment(this.data.defaultSkin, slotIndex, slotBone);
		for (let i = 0, n = this.data.skins.length; i < n; i++)
			this.sortPathConstraintAttachment(this.data.skins[i], slotIndex, slotBone);

		let attachment = slot.getAttachment();
		if (attachment instanceof PathAttachment) this.sortPathConstraintAttachmentWith(attachment, slotBone);

		let constrained = constraint.bones;
		let boneCount = constrained.length;
		for (let i = 0; i < boneCount; i++)
			this.sortBone(constrained[i]);

		this._updateCache.push(constraint);

		for (let i = 0; i < boneCount; i++)
			this.sortReset(constrained[i].children);
		for (let i = 0; i < boneCount; i++)
			constrained[i].sorted = true;
	}

	sortTransformConstraint (constraint: TransformConstraint) {
		constraint.active = constraint.target.isActive() && (!constraint.data.skinRequired || (this.skin && Utils.contains(this.skin.constraints, constraint.data, true)))!;
		if (!constraint.active) return;

		this.sortBone(constraint.target);

		let constrained = constraint.bones;
		let boneCount = constrained.length;
		if (constraint.data.local) {
			for (let i = 0; i < boneCount; i++) {
				let child = constrained[i];
				this.sortBone(child.parent!);
				this.sortBone(child);
			}
		} else {
			for (let i = 0; i < boneCount; i++) {
				this.sortBone(constrained[i]);
			}
		}

		this._updateCache.push(constraint);

		for (let i = 0; i < boneCount; i++)
			this.sortReset(constrained[i].children);
		for (let i = 0; i < boneCount; i++)
			constrained[i].sorted = true;
	}

	sortPathConstraintAttachment (skin: Skin, slotIndex: number, slotBone: Bone) {
		let attachments = skin.attachments[slotIndex];
		if (!attachments) return;
		for (let key in attachments) {
			this.sortPathConstraintAttachmentWith(attachments[key], slotBone);
		}
	}

	sortPathConstraintAttachmentWith (attachment: Attachment, slotBone: Bone) {
		if (!(attachment instanceof PathAttachment)) return;
		let pathBones = (<PathAttachment>attachment).bones;
		if (!pathBones)
			this.sortBone(slotBone);
		else {
			let bones = this.bones;
			for (let i = 0, n = pathBones.length; i < n;) {
				let nn = pathBones[i++];
				nn += i;
				while (i < nn)
					this.sortBone(bones[pathBones[i++]]);
			}
		}
	}

	sortBone (bone: Bone) {
		if (!bone) return;
		if (bone.sorted) return;
		let parent = bone.parent;
		if (parent) this.sortBone(parent);
		bone.sorted = true;
		this._updateCache.push(bone);
	}

	sortReset (bones: Array<Bone>) {
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];
			if (!bone.active) continue;
			if (bone.sorted) this.sortReset(bone.children);
			bone.sorted = false;
		}
	}

	/** Updates the world transform for each bone and applies all constraints.
	 *
	 * See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
	 * Runtimes Guide. */
	updateWorldTransform () {
		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];
			bone.ax = bone.x;
			bone.ay = bone.y;
			bone.arotation = bone.rotation;
			bone.ascaleX = bone.scaleX;
			bone.ascaleY = bone.scaleY;
			bone.ashearX = bone.shearX;
			bone.ashearY = bone.shearY;
		}

		let updateCache = this._updateCache;
		for (let i = 0, n = updateCache.length; i < n; i++)
			updateCache[i].update();
	}

	updateWorldTransformWith (parent: Bone) {
		// Apply the parent bone transform to the root bone. The root bone always inherits scale, rotation and reflection.
		let rootBone = this.getRootBone();
		if (!rootBone) throw new Error("Root bone must not be null.");
		let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		rootBone.worldX = pa * this.x + pb * this.y + parent.worldX;
		rootBone.worldY = pc * this.x + pd * this.y + parent.worldY;

		let rotationY = rootBone.rotation + 90 + rootBone.shearY;
		let la = MathUtils.cosDeg(rootBone.rotation + rootBone.shearX) * rootBone.scaleX;
		let lb = MathUtils.cosDeg(rotationY) * rootBone.scaleY;
		let lc = MathUtils.sinDeg(rootBone.rotation + rootBone.shearX) * rootBone.scaleX;
		let ld = MathUtils.sinDeg(rotationY) * rootBone.scaleY;
		rootBone.a = (pa * la + pb * lc) * this.scaleX;
		rootBone.b = (pa * lb + pb * ld) * this.scaleX;
		rootBone.c = (pc * la + pd * lc) * this.scaleY;
		rootBone.d = (pc * lb + pd * ld) * this.scaleY;

		// Update everything except root bone.
		let updateCache = this._updateCache;
		for (let i = 0, n = updateCache.length; i < n; i++) {
			let updatable = updateCache[i];
			if (updatable != rootBone) updatable.update();
		}
	}

	/** Sets the bones, constraints, and slots to their setup pose values. */
	setToSetupPose () {
		this.setBonesToSetupPose();
		this.setSlotsToSetupPose();
	}

	/** Sets the bones and constraints to their setup pose values. */
	setBonesToSetupPose () {
		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++)
			bones[i].setToSetupPose();

		let ikConstraints = this.ikConstraints;
		for (let i = 0, n = ikConstraints.length; i < n; i++) {
			let constraint = ikConstraints[i];
			constraint.mix = constraint.data.mix;
			constraint.softness = constraint.data.softness;
			constraint.bendDirection = constraint.data.bendDirection;
			constraint.compress = constraint.data.compress;
			constraint.stretch = constraint.data.stretch;
		}

		let transformConstraints = this.transformConstraints;
		for (let i = 0, n = transformConstraints.length; i < n; i++) {
			let constraint = transformConstraints[i];
			let data = constraint.data;
			constraint.mixRotate = data.mixRotate;
			constraint.mixX = data.mixX;
			constraint.mixY = data.mixY;
			constraint.mixScaleX = data.mixScaleX;
			constraint.mixScaleY = data.mixScaleY;
			constraint.mixShearY = data.mixShearY;
		}

		let pathConstraints = this.pathConstraints;
		for (let i = 0, n = pathConstraints.length; i < n; i++) {
			let constraint = pathConstraints[i];
			let data = constraint.data;
			constraint.position = data.position;
			constraint.spacing = data.spacing;
			constraint.mixRotate = data.mixRotate;
			constraint.mixX = data.mixX;
			constraint.mixY = data.mixY;
		}
	}

	/** Sets the slots and draw order to their setup pose values. */
	setSlotsToSetupPose () {
		let slots = this.slots;
		Utils.arrayCopy(slots, 0, this.drawOrder, 0, slots.length);
		for (let i = 0, n = slots.length; i < n; i++)
			slots[i].setToSetupPose();
	}

	/** @returns May return null. */
	getRootBone () {
		if (this.bones.length == 0) return null;
		return this.bones[0];
	}

	/** @returns May be null. */
	findBone (boneName: string) {
		if (!boneName) throw new Error("boneName cannot be null.");
		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];
			if (bone.data.name == boneName) return bone;
		}
		return null;
	}

	/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly.
	 * @returns May be null. */
	findSlot (slotName: string) {
		if (!slotName) throw new Error("slotName cannot be null.");
		let slots = this.slots;
		for (let i = 0, n = slots.length; i < n; i++) {
			let slot = slots[i];
			if (slot.data.name == slotName) return slot;
		}
		return null;
	}

	/** Sets a skin by name.
	 *
	 * See {@link #setSkin()}. */
	setSkinByName (skinName: string) {
		let skin = this.data.findSkin(skinName);
		if (!skin) throw new Error("Skin not found: " + skinName);
		this.setSkin(skin);
	}

	/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#defaultSkin default skin}. If the
	 * skin is changed, {@link #updateCache()} is called.
	 *
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
	 * old skin, each slot's setup mode attachment is attached from the new skin.
	 *
	 * After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
	 * {@link #setSlotsToSetupPose()}. Also, often {@link AnimationState#apply()} is called before the next time the
	 * skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new skin.
	 * @param newSkin May be null. */
	setSkin (newSkin: Skin) {
		if (newSkin == this.skin) return;
		if (newSkin) {
			if (this.skin)
				newSkin.attachAll(this, this.skin);
			else {
				let slots = this.slots;
				for (let i = 0, n = slots.length; i < n; i++) {
					let slot = slots[i];
					let name = slot.data.attachmentName;
					if (name) {
						let attachment = newSkin.getAttachment(i, name);
						if (attachment) slot.setAttachment(attachment);
					}
				}
			}
		}
		this.skin = newSkin;
		this.updateCache();
	}


	/** Finds an attachment by looking in the {@link #skin} and {@link SkeletonData#defaultSkin} using the slot name and attachment
	 * name.
	 *
	 * See {@link #getAttachment()}.
	 * @returns May be null. */
	getAttachmentByName (slotName: string, attachmentName: string): Attachment | null {
		let slot = this.data.findSlot(slotName);
		if (!slot) throw new Error(`Can't find slot with name ${slotName}`);
		return this.getAttachment(slot.index, attachmentName);
	}

	/** Finds an attachment by looking in the {@link #skin} and {@link SkeletonData#defaultSkin} using the slot index and
	 * attachment name. First the skin is checked and if the attachment was not found, the default skin is checked.
	 *
	 * See [Runtime skins](http://esotericsoftware.com/spine-runtime-skins) in the Spine Runtimes Guide.
	 * @returns May be null. */
	getAttachment (slotIndex: number, attachmentName: string): Attachment | null {
		if (!attachmentName) throw new Error("attachmentName cannot be null.");
		if (this.skin) {
			let attachment = this.skin.getAttachment(slotIndex, attachmentName);
			if (attachment) return attachment;
		}
		if (this.data.defaultSkin) return this.data.defaultSkin.getAttachment(slotIndex, attachmentName);
		return null;
	}

	/** A convenience method to set an attachment by finding the slot with {@link #findSlot()}, finding the attachment with
	 * {@link #getAttachment()}, then setting the slot's {@link Slot#attachment}.
	 * @param attachmentName May be null to clear the slot's attachment. */
	setAttachment (slotName: string, attachmentName: string) {
		if (!slotName) throw new Error("slotName cannot be null.");
		let slots = this.slots;
		for (let i = 0, n = slots.length; i < n; i++) {
			let slot = slots[i];
			if (slot.data.name == slotName) {
				let attachment: Attachment | null = null;
				if (attachmentName) {
					attachment = this.getAttachment(i, attachmentName);
					if (!attachment) throw new Error("Attachment not found: " + attachmentName + ", for slot: " + slotName);
				}
				slot.setAttachment(attachment);
				return;
			}
		}
		throw new Error("Slot not found: " + slotName);
	}


	/** Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
	 * than to call it repeatedly.
	 * @return May be null. */
	findIkConstraint (constraintName: string) {
		if (!constraintName) throw new Error("constraintName cannot be null.");
		let ikConstraints = this.ikConstraints;
		for (let i = 0, n = ikConstraints.length; i < n; i++) {
			let ikConstraint = ikConstraints[i];
			if (ikConstraint.data.name == constraintName) return ikConstraint;
		}
		return null;
	}

	/** Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
	 * this method than to call it repeatedly.
	 * @return May be null. */
	findTransformConstraint (constraintName: string) {
		if (!constraintName) throw new Error("constraintName cannot be null.");
		let transformConstraints = this.transformConstraints;
		for (let i = 0, n = transformConstraints.length; i < n; i++) {
			let constraint = transformConstraints[i];
			if (constraint.data.name == constraintName) return constraint;
		}
		return null;
	}

	/** Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
	 * than to call it repeatedly.
	 * @return May be null. */
	findPathConstraint (constraintName: string) {
		if (!constraintName) throw new Error("constraintName cannot be null.");
		let pathConstraints = this.pathConstraints;
		for (let i = 0, n = pathConstraints.length; i < n; i++) {
			let constraint = pathConstraints[i];
			if (constraint.data.name == constraintName) return constraint;
		}
		return null;
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose as `{ x: number, y: number, width: number, height: number }`.
	 * Note that this method will create temporary objects which can add to garbage collection pressure. Use `getBounds()` if garbage collection is a concern. */
	getBoundsRect () {
		let offset = new Vector2();
		let size = new Vector2();
		this.getBounds(offset, size);
		return { x: offset.x, y: offset.y, width: size.x, height: size.y };
	}

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
	 * @param offset An output value, the distance from the skeleton origin to the bottom left corner of the AABB.
	 * @param size An output value, the width and height of the AABB.
	 * @param temp Working memory to temporarily store attachments' computed world vertices. */
	getBounds (offset: Vector2, size: Vector2, temp: Array<number> = new Array<number>(2)) {
		if (!offset) throw new Error("offset cannot be null.");
		if (!size) throw new Error("size cannot be null.");
		let drawOrder = this.drawOrder;
		let minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
		for (let i = 0, n = drawOrder.length; i < n; i++) {
			let slot = drawOrder[i];
			if (!slot.bone.active) continue;
			let verticesLength = 0;
			let vertices: NumberArrayLike | null = null;
			let attachment = slot.getAttachment();
			if (attachment instanceof RegionAttachment) {
				verticesLength = 8;
				vertices = Utils.setArraySize(temp, verticesLength, 0);
				(<RegionAttachment>attachment).computeWorldVertices(slot, vertices, 0, 2);
			} else if (attachment instanceof MeshAttachment) {
				let mesh = (<MeshAttachment>attachment);
				verticesLength = mesh.worldVerticesLength;
				vertices = Utils.setArraySize(temp, verticesLength, 0);
				mesh.computeWorldVertices(slot, 0, verticesLength, vertices, 0, 2);
			}
			if (vertices) {
				for (let ii = 0, nn = vertices.length; ii < nn; ii += 2) {
					let x = vertices[ii], y = vertices[ii + 1];
					minX = Math.min(minX, x);
					minY = Math.min(minY, y);
					maxX = Math.max(maxX, x);
					maxY = Math.max(maxY, y);
				}
			}
		}
		offset.set(minX, minY);
		size.set(maxX - minX, maxY - minY);
	}
}
