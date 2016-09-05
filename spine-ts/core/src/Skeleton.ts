/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
 * 
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class Skeleton {
		data: SkeletonData;
		bones: Array<Bone>;
		slots: Array<Slot>;
		drawOrder: Array<Slot>;
		ikConstraints: Array<IkConstraint>; ikConstraintsSorted: Array<IkConstraint>;
		transformConstraints: Array<TransformConstraint>;
		pathConstraints: Array<PathConstraint>;
		_updateCache = new Array<Updatable>();
		skin: Skin;
		color: Color;
		time = 0;
		flipX = false; flipY = false;
		x = 0; y = 0;

		constructor (data: SkeletonData) {
			if (data == null) throw new Error("data cannot be null.");
			this.data = data;

			this.bones = new Array<Bone>();
			for (let i = 0; i < data.bones.length; i++) {
				let boneData = data.bones[i];
				let bone: Bone;
				if (boneData.parent == null)
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
			this.ikConstraintsSorted = new Array<IkConstraint>();
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

		updateCache () {
			let updateCache = this._updateCache;
			updateCache.length = 0;

			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++)
				bones[i].sorted = false;

			// IK first, lowest hierarchy depth first.
			let ikConstraints = this.ikConstraintsSorted;
			ikConstraints.length = 0;
			for (let i = 0; i < this.ikConstraints.length; i++)
				ikConstraints.push(this.ikConstraints[i]);
			let ikCount = ikConstraints.length;
			for (let i = 0, level = 0, n = ikCount; i < n; i++) {
				let ik = ikConstraints[i];
				let bone = ik.bones[0].parent;
				for (level = 0; bone != null; level++)
					bone = bone.parent;
				ik.level = level;
			}
			for (let i = 1, ii = 0; i < ikCount; i++) {
				let ik = ikConstraints[i];
				let level = ik.level;
				for (ii = i - 1; ii >= 0; ii--) {
					let other = ikConstraints[ii];
					if (other.level < level) break;
					ikConstraints[ii + 1] = other;
				}
				ikConstraints[ii + 1] = ik;
			}
			for (let i = 0, n = ikConstraints.length; i < n; i++) {
				let constraint = ikConstraints[i];
				let target = constraint.target;
				this.sortBone(target);

				let constrained = constraint.bones;
				let parent = constrained[0];
				this.sortBone(parent);

				updateCache.push(constraint);

				this.sortReset(parent.children);
				constrained[constrained.length - 1].sorted = true;
			}

			let pathConstraints = this.pathConstraints;
			for (let i = 0, n = pathConstraints.length; i < n; i++) {
				let constraint = pathConstraints[i];

				let slot = constraint.target;
				let slotIndex = slot.data.index;
				let slotBone = slot.bone;
				if (this.skin != null) this.sortPathConstraintAttachment(this.skin, slotIndex, slotBone);
				if (this.data.defaultSkin != null && this.data.defaultSkin != this.skin)
					this.sortPathConstraintAttachment(this.data.defaultSkin, slotIndex, slotBone);
				for (let ii = 0, nn = this.data.skins.length; ii < nn; ii++)
					this.sortPathConstraintAttachment(this.data.skins[ii], slotIndex, slotBone);

				let attachment = slot.getAttachment();
				if (attachment instanceof PathAttachment) this.sortPathConstraintAttachmentWith(attachment, slotBone);

				let constrained = constraint.bones;
				let boneCount = constrained.length;
				for (let ii = 0; ii < boneCount; ii++)
					this.sortBone(constrained[ii]);

				updateCache.push(constraint);

				for (let ii = 0; ii < boneCount; ii++)
					this.sortReset(constrained[ii].children);
				for (let ii = 0; ii < boneCount; ii++)
					constrained[ii].sorted = true;
			}

			let transformConstraints = this.transformConstraints;
			for (let i = 0, n = transformConstraints.length; i < n; i++) {
				let constraint = transformConstraints[i];

				this.sortBone(constraint.target);

				let constrained = constraint.bones;
				let boneCount = constrained.length;
				for (let ii = 0; ii < boneCount; ii++)
					this.sortBone(constrained[ii]);

				updateCache.push(constraint);

				for (let ii = 0; ii < boneCount; ii++)
					this.sortReset(constrained[ii].children);
				for (let ii = 0; ii < boneCount; ii++)
					constrained[ii].sorted = true;
			}

			for (let i = 0, n = bones.length; i < n; i++)
				this.sortBone(bones[i]);
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
			if (pathBones == null)
				this.sortBone(slotBone);
			else {
				let bones = this.bones;
				for (let i = 0; i < pathBones.length; i++) {
					let boneIndex = pathBones[i];
					this.sortBone(bones[boneIndex]);
				}
			}
		}

		sortBone (bone: Bone) {
			if (bone.sorted) return;
			let parent = bone.parent;
			if (parent != null) this.sortBone(parent);
			bone.sorted = true;
			this._updateCache.push(bone);
		}

		sortReset (bones: Array<Bone>) {
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				if (bone.sorted) this.sortReset(bone.children);
				bone.sorted = false;
			}
		}

		/** Updates the world transform for each bone and applies constraints. */
		updateWorldTransform () {
			let updateCache = this._updateCache;
			for (let i = 0, n = updateCache.length; i < n; i++)
				updateCache[i].update();
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
				constraint.bendDirection = constraint.data.bendDirection;
				constraint.mix = constraint.data.mix;
			}

			let transformConstraints = this.transformConstraints;
			for (let i = 0, n = transformConstraints.length; i < n; i++) {
				let constraint = transformConstraints[i];
				let data = constraint.data;
				constraint.rotateMix = data.rotateMix;
				constraint.translateMix = data.translateMix;
				constraint.scaleMix = data.scaleMix;
				constraint.shearMix = data.shearMix;
			}

			let pathConstraints = this.pathConstraints;
			for (let i = 0, n = pathConstraints.length; i < n; i++) {
				let constraint = pathConstraints[i];
				let data = constraint.data;
				constraint.position = data.position;
				constraint.spacing = data.spacing;
				constraint.rotateMix = data.rotateMix;
				constraint.translateMix = data.translateMix;
			}
		}

		setSlotsToSetupPose () {
			let slots = this.slots;
			Utils.arrayCopy(slots, 0, this.drawOrder, 0, slots.length);
			for (let i = 0, n = slots.length; i < n; i++)
				slots[i].setToSetupPose();
		}

		/** @return May return null. */
		getRootBone () {
			if (this.bones.length == 0) return null;
			return this.bones[0];
		}

		/** @return May be null. */
		findBone (boneName: string) {
			if (boneName == null) throw new Error("boneName cannot be null.");
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				if (bone.data.name == boneName) return bone;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		findBoneIndex (boneName: string) {
			if (boneName == null) throw new Error("boneName cannot be null.");
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++)
				if (bones[i].data.name == boneName) return i;
			return -1;
		}

		/** @return May be null. */
		findSlot (slotName: string) {
			if (slotName == null) throw new Error("slotName cannot be null.");
			let slots = this.slots;
			for (let i = 0, n = slots.length; i < n; i++) {
				let slot = slots[i];
				if (slot.data.name == slotName) return slot;
			}
			return null;
		}

		/** @return -1 if the bone was not found. */
		findSlotIndex (slotName: string) {
			if (slotName == null) throw new Error("slotName cannot be null.");
			let slots = this.slots;
			for (let i = 0, n = slots.length; i < n; i++)
				if (slots[i].data.name == slotName) return i;
			return -1;
		}

		/** Sets a skin by name.
		 * @see #setSkin(Skin) */
		setSkinByName (skinName: string) {
			let skin = this.data.findSkin(skinName);
			if (skin == null) throw new Error("Skin not found: " + skinName);
			this.setSkin(skin);
		}

		/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}.
		 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
		 * old skin, each slot's setup mode attachment is attached from the new skin.
		 * @param newSkin May be null. */
		setSkin (newSkin: Skin) {
			if (newSkin != null) {
				if (this.skin != null)
					newSkin.attachAll(this, this.skin);
				else {
					let slots = this.slots;
					for (let i = 0, n = slots.length; i < n; i++) {
						let slot = slots[i];
						let name = slot.data.attachmentName;
						if (name != null) {
							let attachment: Attachment = newSkin.getAttachment(i, name);
							if (attachment != null) slot.setAttachment(attachment);
						}
					}
				}
			}
			this.skin = newSkin;
		}

		/** @return May be null. */
		getAttachmentByName (slotName: string, attachmentName: string): Attachment {
			return this.getAttachment(this.data.findSlotIndex(slotName), attachmentName);
		}

		/** @return May be null. */
		getAttachment (slotIndex: number, attachmentName: string): Attachment {
			if (attachmentName == null) throw new Error("attachmentName cannot be null.");
			if (this.skin != null) {
				let attachment: Attachment = this.skin.getAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}
			if (this.data.defaultSkin != null) return this.data.defaultSkin.getAttachment(slotIndex, attachmentName);
			return null;
		}

		/** @param attachmentName May be null. */
		setAttachment (slotName: string, attachmentName: string) {
			if (slotName == null) throw new Error("slotName cannot be null.");
			let slots = this.slots;
			for (let i = 0, n = slots.length; i < n; i++) {
				let slot = slots[i];
				if (slot.data.name == slotName) {
					let attachment: Attachment = null;
					if (attachmentName != null) {
						attachment = this.getAttachment(i, attachmentName);
						if (attachment == null)
							throw new Error("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.setAttachment(attachment);
					return;
				}
			}
			throw new Error("Slot not found: " + slotName);
		}

		/** @return May be null. */
		findIkConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let ikConstraints = this.ikConstraints;
			for (let i = 0, n = ikConstraints.length; i < n; i++) {
				let ikConstraint = ikConstraints[i];
				if (ikConstraint.data.name == constraintName) return ikConstraint;
			}
			return null;
		}

		/** @return May be null. */
		findTransformConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let transformConstraints = this.transformConstraints;
			for (let i = 0, n = transformConstraints.length; i < n; i++) {
				let constraint = transformConstraints[i];
				if (constraint.data.name == constraintName) return constraint;
			}
			return null;
		}

		/** @return May be null. */
		findPathConstraint (constraintName: string) {
			if (constraintName == null) throw new Error("constraintName cannot be null.");
			let pathConstraints = this.pathConstraints;
			for (let i = 0, n = pathConstraints.length; i < n; i++) {
				let constraint = pathConstraints[i];
				if (constraint.data.name == constraintName) return constraint;
			}
			return null;
		}

		/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose.
		 * @param offset The distance from the skeleton origin to the bottom left corner of the AABB.
		 * @param size The width and height of the AABB. */
		getBounds (offset: Vector2, size: Vector2) {
			if (offset == null) throw new Error("offset cannot be null.");
			if (size == null) throw new Error("size cannot be null.");
			let drawOrder = this.drawOrder;
			let minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY, maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
			for (let i = 0, n = drawOrder.length; i < n; i++) {
				let slot = drawOrder[i];
				let vertices: ArrayLike<number> = null;
				let attachment = slot.getAttachment();
				if (attachment instanceof RegionAttachment)
					vertices = (<RegionAttachment>attachment).updateWorldVertices(slot, false);
				else if (attachment instanceof MeshAttachment) //
					vertices = (<MeshAttachment>attachment).updateWorldVertices(slot, true);
				if (vertices != null) {
					for (let ii = 0, nn = vertices.length; ii < nn; ii += 8) {
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

		update (delta: number) {
			this.time += delta;
		}
	}
}
