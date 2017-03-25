/******************************************************************************
 * Spine Runtimes Software License v2.5
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

package spine {
	import spine.attachments.MeshAttachment;
	import spine.attachments.RegionAttachment;

	import flash.utils.Dictionary;

	import spine.attachments.PathAttachment;
	import spine.attachments.Attachment;

	public class Skeleton {
		internal var _data : SkeletonData;
		public var bones : Vector.<Bone>;
		public var slots : Vector.<Slot>;
		public var drawOrder : Vector.<Slot>;
		public var ikConstraints : Vector.<IkConstraint>;
		public var transformConstraints : Vector.<TransformConstraint>;
		public var pathConstraints : Vector.<PathConstraint>;
		private var _updateCache : Vector.<Updatable> = new Vector.<Updatable>();
		private var _updateCacheReset : Vector.<Bone> = new Vector.<Bone>();
		private var _skin : Skin;
		public var color : Color = new Color(1, 1, 1, 1);
		public var time : Number = 0;
		public var flipX : Boolean, flipY : Boolean;
		public var x : Number = 0, y : Number = 0;

		public function Skeleton(data : SkeletonData) {
			if (data == null)
				throw new ArgumentError("data cannot be null.");
			_data = data;

			bones = new Vector.<Bone>();
			for each (var boneData : BoneData in data.bones) {
				var bone : Bone;
				if (boneData.parent == null)
					bone = new Bone(boneData, this, null);
				else {
					var parent : Bone = bones[boneData.parent.index];
					bone = new Bone(boneData, this, parent);
					parent.children.push(bone);
				}
				bones.push(bone);
			}

			slots = new Vector.<Slot>();
			drawOrder = new Vector.<Slot>();
			for each (var slotData : SlotData in data.slots) {
				bone = bones[slotData.boneData.index];
				var slot : Slot = new Slot(slotData, bone);
				slots.push(slot);
				drawOrder[drawOrder.length] = slot;
			}

			ikConstraints = new Vector.<IkConstraint>();
			for each (var ikConstraintData : IkConstraintData in data.ikConstraints)
				ikConstraints.push(new IkConstraint(ikConstraintData, this));

			transformConstraints = new Vector.<TransformConstraint>();
			for each (var transformConstraintData : TransformConstraintData in data.transformConstraints)
				transformConstraints.push(new TransformConstraint(transformConstraintData, this));

			pathConstraints = new Vector.<PathConstraint>();
			for each (var pathConstraintData : PathConstraintData in data.pathConstraints)
				pathConstraints.push(new PathConstraint(pathConstraintData, this));

			updateCache();
		}

		/** Caches information about bones and constraints. Must be called if bones, constraints, or weighted path attachments are
		 * added or removed. */
		public function updateCache() : void {
			var updateCache : Vector.<Updatable> = this._updateCache;
			updateCache.length = 0;
			this._updateCacheReset.length = 0;

			var bones : Vector.<Bone> = this.bones;
			var i : Number = 0;
			var n : Number = 0;
			for (i = 0, n = bones.length; i < n; i++)
				bones[i]._sorted = false;

			// IK first, lowest hierarchy depth first.
			var ikConstraints : Vector.<IkConstraint> = this.ikConstraints;
			var transformConstraints : Vector.<TransformConstraint> = this.transformConstraints;
			var pathConstraints : Vector.<PathConstraint> = this.pathConstraints;
			var ikCount : Number = ikConstraints.length, transformCount : Number = transformConstraints.length, pathCount : Number = pathConstraints.length;
			var constraintCount : Number = ikCount + transformCount + pathCount;

			outer:
			for (i = 0; i < constraintCount; i++) {
				var ii : Number = 0;
				for (ii = 0; ii < ikCount; ii++) {
					var ikConstraint : IkConstraint = ikConstraints[ii];
					if (ikConstraint.data.order == i) {
						sortIkConstraint(ikConstraint);
						continue outer;
					}
				}
				for (ii = 0; ii < transformCount; ii++) {
					var transformConstraint : TransformConstraint = transformConstraints[ii];
					if (transformConstraint.data.order == i) {
						sortTransformConstraint(transformConstraint);
						continue outer;
					}
				}
				for (ii = 0; ii < pathCount; ii++) {
					var pathConstraint : PathConstraint = pathConstraints[ii];
					if (pathConstraint.data.order == i) {
						sortPathConstraint(pathConstraint);
						continue outer;
					}
				}
			}

			for (i = 0, n = bones.length; i < n; i++)
				sortBone(bones[i]);
		}

		private function sortIkConstraint(constraint : IkConstraint) : void {
			var target : Bone = constraint.target;
			sortBone(target);

			var constrained : Vector.<Bone> = constraint.bones;
			var parent : Bone = constrained[0];
			sortBone(parent);

			if (constrained.length > 1) {
				var child : Bone = constrained[constrained.length - 1];
				if (!(_updateCache.indexOf(child) > -1)) _updateCacheReset.push(child);
			}

			_updateCache.push(constraint);

			sortReset(parent.children);
			constrained[constrained.length - 1]._sorted = true;
		}

		private function sortPathConstraint(constraint : PathConstraint) : void {
			var slot : Slot = constraint.target;
			var slotIndex : Number = slot.data.index;
			var slotBone : Bone = slot.bone;
			if (skin != null) sortPathConstraintAttachment(skin, slotIndex, slotBone);
			if (data.defaultSkin != null && data.defaultSkin != skin)
				sortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);
			var i : Number = 0;
			var n : Number = 0;
			for (i = 0, n = data.skins.length; i < n; i++)
				sortPathConstraintAttachment(data.skins[i], slotIndex, slotBone);

			var attachment : Attachment = slot.attachment;
			if (attachment is PathAttachment) sortPathConstraintAttachment2(attachment, slotBone);

			var constrained : Vector.<Bone> = constraint.bones;
			var boneCount : Number = constrained.length;
			for (i = 0; i < boneCount; i++)
				sortBone(constrained[i]);

			_updateCache.push(constraint);

			for (i = 0; i < boneCount; i++)
				sortReset(constrained[i].children);
			for (i = 0; i < boneCount; i++)
				constrained[i]._sorted = true;
		}

		private function sortTransformConstraint(constraint : TransformConstraint) : void {
			sortBone(constraint.target);

			var constrained : Vector.<Bone> = constraint.bones;
			var boneCount : Number = constrained.length;
			var i : Number = 0;
			if (constraint.data.local) {
				for (i = 0; i < boneCount; i++) {
					var child : Bone = constrained[i];
					sortBone(child.parent);
					if (!(_updateCache.indexOf(child) > -1)) _updateCacheReset.push(child);
				}
			} else {
				for (i = 0; i < boneCount; i++)
					sortBone(constrained[i]);
			}

			_updateCache.push(constraint);

			for (i = 0; i < boneCount; i++)
				sortReset(constrained[i].children);
			for (i = 0; i < boneCount; i++)
				constrained[i]._sorted = true;
		}

		private function sortPathConstraintAttachment(skin : Skin, slotIndex : int, slotBone : Bone) : void {
			var dict : Dictionary = skin.attachments[slotIndex];
			if (!dict) return;

			for each (var value : Attachment in dict) {
				sortPathConstraintAttachment2(value, slotBone);
			}
		}

		private function sortPathConstraintAttachment2(attachment : Attachment, slotBone : Bone) : void {
			var pathAttachment : PathAttachment = attachment as PathAttachment;
			if (!pathAttachment) return;
			var pathBones : Vector.<int> = pathAttachment.bones;
			if (pathBones == null)
				sortBone(slotBone);
			else {
				var bones : Vector.<Bone> = this.bones;
				var i : int = 0;
				while (i < pathBones.length) {
					var boneCount : int = pathBones[i++];
					for (var n : int = i + boneCount; i < n; i++) {
						sortBone(bones[pathBones[i]]);
					}
				}
			}
		}

		private function sortBone(bone : Bone) : void {
			if (bone._sorted) return;
			var parent : Bone = bone.parent;
			if (parent != null) sortBone(parent);
			bone._sorted = true;
			_updateCache.push(bone);
		}

		private function sortReset(bones : Vector.<Bone>) : void {
			for (var i : int = 0, n : int = bones.length; i < n; i++) {
				var bone : Bone = bones[i];
				if (bone._sorted) sortReset(bone.children);
				bone._sorted = false;
			}
		}

		/** Updates the world transform for each bone and applies constraints. */
		public function updateWorldTransform() : void {
			var updateCacheReset : Vector.<Bone> = this._updateCacheReset;
			for each (var bone : Bone in updateCacheReset) {
				bone.ax = bone.x;
				bone.ay = bone.y;
				bone.arotation = bone.rotation;
				bone.ascaleX = bone.scaleX;
				bone.ascaleY = bone.scaleY;
				bone.ashearX = bone.shearX;
				bone.ashearY = bone.shearY;
				bone.appliedValid = true;
			}
			for each (var updatable : Updatable in _updateCache)
				updatable.update();
		}

		/** Sets the bones, constraints, and slots to their setup pose values. */
		public function setToSetupPose() : void {
			setBonesToSetupPose();
			setSlotsToSetupPose();
		}

		/** Sets the bones and constraints to their setup pose values. */
		public function setBonesToSetupPose() : void {
			for each (var bone : Bone in bones)
				bone.setToSetupPose();

			for each (var ikConstraint : IkConstraint in ikConstraints) {
				ikConstraint.bendDirection = ikConstraint._data.bendDirection;
				ikConstraint.mix = ikConstraint._data.mix;
			}

			for each (var transformConstraint : TransformConstraint in transformConstraints) {
				transformConstraint.rotateMix = transformConstraint._data.rotateMix;
				transformConstraint.translateMix = transformConstraint._data.translateMix;
				transformConstraint.scaleMix = transformConstraint._data.scaleMix;
				transformConstraint.shearMix = transformConstraint._data.shearMix;
			}

			for each (var pathConstraint : PathConstraint in pathConstraints) {
				pathConstraint.position = pathConstraint._data.position;
				pathConstraint.spacing = pathConstraint._data.spacing;
				pathConstraint.rotateMix = pathConstraint._data.rotateMix;
				pathConstraint.translateMix = pathConstraint._data.translateMix;
			}
		}

		public function setSlotsToSetupPose() : void {
			var i : int = 0;
			for each (var slot : Slot in slots) {
				drawOrder[i++] = slot;
				slot.setToSetupPose();
			}
		}

		public function get data() : SkeletonData {
			return _data;
		}

		public function get getUpdateCache() : Vector.<Updatable> {
			return _updateCache;
		}

		public function get rootBone() : Bone {
			if (bones.length == 0) return null;
			return bones[0];
		}

		/** @return May be null. */
		public function findBone(boneName : String) : Bone {
			if (boneName == null)
				throw new ArgumentError("boneName cannot be null.");
			for each (var bone : Bone in bones)
				if (bone._data._name == boneName) return bone;
			return null;
		}

		/** @return -1 if the bone was not found. */
		public function findBoneIndex(boneName : String) : int {
			if (boneName == null)
				throw new ArgumentError("boneName cannot be null.");
			var i : int = 0;
			for each (var bone : Bone in bones) {
				if (bone._data._name == boneName) return i;
				i++;
			}
			return -1;
		}

		/** @return May be null. */
		public function findSlot(slotName : String) : Slot {
			if (slotName == null)
				throw new ArgumentError("slotName cannot be null.");
			for each (var slot : Slot in slots)
				if (slot._data._name == slotName) return slot;
			return null;
		}

		/** @return -1 if the bone was not found. */
		public function findSlotIndex(slotName : String) : int {
			if (slotName == null)
				throw new ArgumentError("slotName cannot be null.");
			var i : int = 0;
			for each (var slot : Slot in slots) {
				if (slot._data._name == slotName) return i;
				i++;
			}
			return -1;
		}

		public function get skin() : Skin {
			return _skin;
		}

		public function set skinName(skinName : String) : void {
			var skin : Skin = data.findSkin(skinName);
			if (skin == null) throw new ArgumentError("Skin not found: " + skinName);
			this.skin = skin;
		}

		/** @return May be null. */
		public function get skinName() : String {
			return _skin == null ? null : _skin._name;
		}

		/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}.
		 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was
		 * no old skin, each slot's setup mode attachment is attached from the new skin.
		 * @param newSkin May be null. */
		public function set skin(newSkin : Skin) : void {
			if (newSkin) {
				if (skin)
					newSkin.attachAll(this, skin);
				else {
					var i : int = 0;
					for each (var slot : Slot in slots) {
						var name : String = slot._data.attachmentName;
						if (name) {
							var attachment : Attachment = newSkin.getAttachment(i, name);
							if (attachment) slot.attachment = attachment;
						}
						i++;
					}
				}
			}
			_skin = newSkin;
		}

		/** @return May be null. */
		public function getAttachmentForSlotName(slotName : String, attachmentName : String) : Attachment {
			return getAttachmentForSlotIndex(data.findSlotIndex(slotName), attachmentName);
		}

		/** @return May be null. */
		public function getAttachmentForSlotIndex(slotIndex : int, attachmentName : String) : Attachment {
			if (attachmentName == null) throw new ArgumentError("attachmentName cannot be null.");
			if (skin != null) {
				var attachment : Attachment = skin.getAttachment(slotIndex, attachmentName);
				if (attachment != null) return attachment;
			}
			if (data.defaultSkin != null) return data.defaultSkin.getAttachment(slotIndex, attachmentName);
			return null;
		}

		/** @param attachmentName May be null. */
		public function setAttachment(slotName : String, attachmentName : String) : void {
			if (slotName == null) throw new ArgumentError("slotName cannot be null.");
			var i : int = 0;
			for each (var slot : Slot in slots) {
				if (slot._data._name == slotName) {
					var attachment : Attachment = null;
					if (attachmentName != null) {
						attachment = getAttachmentForSlotIndex(i, attachmentName);
						if (attachment == null)
							throw new ArgumentError("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
					slot.attachment = attachment;
					return;
				}
				i++;
			}
			throw new ArgumentError("Slot not found: " + slotName);
		}

		/** @return May be null. */
		public function findIkConstraint(constraintName : String) : IkConstraint {
			if (constraintName == null) throw new ArgumentError("constraintName cannot be null.");
			for each (var ikConstraint : IkConstraint in ikConstraints)
				if (ikConstraint._data._name == constraintName) return ikConstraint;
			return null;
		}

		/** @return May be null. */
		public function findTransformConstraint(constraintName : String) : TransformConstraint {
			if (constraintName == null) throw new ArgumentError("constraintName cannot be null.");
			for each (var transformConstraint : TransformConstraint in transformConstraints)
				if (transformConstraint._data._name == constraintName) return transformConstraint;
			return null;
		}

		/** @return May be null. */
		public function findPathConstraint(constraintName : String) : PathConstraint {
			if (constraintName == null) throw new ArgumentError("constraintName cannot be null.");
			for each (var pathConstraint : PathConstraint in pathConstraints)
				if (pathConstraint._data._name == constraintName) return pathConstraint;
			return null;
		}

		public function update(delta : Number) : void {
			time += delta;
		}

		public function toString() : String {
			return _data.name != null ? _data.name : super.toString();
		}

		public function getBounds(offset : Vector.<Number>, size : Vector.<Number>, temp : Vector.<Number>) : void {
			if (offset == null) throw new ArgumentError("offset cannot be null.");
			if (size == null) throw new ArgumentError("size cannot be null.");
			var drawOrder : Vector.<Slot> = this.drawOrder;
			var minX : Number = Number.POSITIVE_INFINITY, minY : Number = Number.POSITIVE_INFINITY, maxX : Number = Number.NEGATIVE_INFINITY, maxY : Number = Number.NEGATIVE_INFINITY;
			for (var i : int = 0, n : int = drawOrder.length; i < n; i++) {
				var slot : Slot = drawOrder[i];
				var verticesLength : int = 0;
				var vertices : Vector.<Number> = null;
				var attachment : Attachment = slot.attachment;
				if (attachment is RegionAttachment) {
					verticesLength = 8;
					temp.length = verticesLength;
					vertices = temp;
					(attachment as RegionAttachment).computeWorldVertices(slot.bone, vertices, 0, 2);
				} else if (attachment is MeshAttachment) {
					var mesh : MeshAttachment = attachment as MeshAttachment;
					verticesLength = mesh.worldVerticesLength;
					temp.length = verticesLength;
					vertices = temp;
					mesh.computeWorldVertices(slot, 0, verticesLength, vertices, 0, 2);
				}
				if (vertices != null) {
					for (var ii : int = 0, nn : int = vertices.length; ii < nn; ii += 8) {
						var x : Number = vertices[ii], y : Number = vertices[ii + 1];
						minX = Math.min(minX, x);
						minY = Math.min(minY, y);
						maxX = Math.max(maxX, x);
						maxY = Math.max(maxY, y);
					}
				}
			}
			offset[0] = minX;
			offset[1] = minY;
			size[0] = maxX - minX;
			size[1] = maxY - minY;
		}
	}
}