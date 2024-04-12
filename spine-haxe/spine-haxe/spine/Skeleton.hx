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

package spine;

import lime.math.Rectangle;
import haxe.ds.StringMap;
import spine.attachments.Attachment;
import spine.attachments.MeshAttachment;
import spine.attachments.PathAttachment;
import spine.attachments.RegionAttachment;

class Skeleton {
	private var _data:SkeletonData;

	public var bones:Array<Bone>;
	public var slots:Array<Slot>; // Setup pose draw order.
	public var drawOrder:Array<Slot>;
	public var ikConstraints:Array<IkConstraint>;
	public var transformConstraints:Array<TransformConstraint>;
	public var pathConstraints:Array<PathConstraint>;
	public var physicsConstraints:Array<PhysicsConstraint>;

	private var _updateCache:Array<Updatable> = new Array<Updatable>();
	private var _skin:Skin;

	public var color:Color = new Color(1, 1, 1, 1);
	public var scaleX:Float = 1;
	public var scaleY:Float = 1;
	public var x:Float = 0;
	public var y:Float = 0;
	public var time:Float = 0;

	public function new(data:SkeletonData) {
		if (data == null) {
			throw new SpineException("data cannot be null.");
		}
		_data = data;

		bones = new Array<Bone>();
		for (boneData in data.bones) {
			var bone:Bone;
			if (boneData.parent == null) {
				bone = new Bone(boneData, this, null);
			} else {
				var parent:Bone = bones[boneData.parent.index];
				bone = new Bone(boneData, this, parent);
				parent.children.push(bone);
			}
			bones.push(bone);
		}

		slots = new Array<Slot>();
		drawOrder = new Array<Slot>();
		for (slotData in data.slots) {
			var bone = bones[slotData.boneData.index];
			var slot:Slot = new Slot(slotData, bone);
			slots.push(slot);
			drawOrder.push(slot);
		}

		ikConstraints = new Array<IkConstraint>();
		for (ikConstraintData in data.ikConstraints) {
			ikConstraints.push(new IkConstraint(ikConstraintData, this));
		}

		transformConstraints = new Array<TransformConstraint>();
		for (transformConstraintData in data.transformConstraints) {
			transformConstraints.push(new TransformConstraint(transformConstraintData, this));
		}

		pathConstraints = new Array<PathConstraint>();
		for (pathConstraintData in data.pathConstraints) {
			pathConstraints.push(new PathConstraint(pathConstraintData, this));
		}

		physicsConstraints = new Array<PhysicsConstraint>();
		for (physicConstraintData in data.physicsConstraints) {
			physicsConstraints.push(new PhysicsConstraint(physicConstraintData, this));
		}

		updateCache();
	}

	/** Caches information about bones and constraints. Must be called if bones, constraints, or weighted path attachments are
	 * added or removed. */
	public function updateCache():Void {
		_updateCache.resize(0);

		for (bone in bones) {
			bone.sorted = bone.data.skinRequired;
			bone.active = !bone.sorted;
		}

		if (skin != null) {
			var skinBones:Array<BoneData> = skin.bones;
			for (i in 0...skin.bones.length) {
				var bone:Bone = bones[skinBones[i].index];
				do {
					bone.sorted = false;
					bone.active = true;
					bone = bone.parent;
				} while (bone != null);
			}
		}

		// IK first, lowest hierarchy depth first.
		var ikCount:Int = ikConstraints.length;
		var transformCount:Int = transformConstraints.length;
		var pathCount:Int = pathConstraints.length;
		var physicCount:Int = physicsConstraints.length;
		var constraintCount:Int = ikCount + transformCount + pathCount + physicCount;

		var continueOuter:Bool;
		for (i in 0...constraintCount) {
			continueOuter = false;
			for (ikConstraint in ikConstraints) {
				if (ikConstraint.data.order == i) {
					sortIkConstraint(ikConstraint);
					continueOuter = true;
					break;
				}
			}
			if (continueOuter)
				continue;
			for (transformConstraint in transformConstraints) {
				if (transformConstraint.data.order == i) {
					sortTransformConstraint(transformConstraint);
					continueOuter = true;
					break;
				}
			}
			if (continueOuter)
				continue;
			for (pathConstraint in pathConstraints) {
				if (pathConstraint.data.order == i) {
					sortPathConstraint(pathConstraint);
					break;
				}
			}
			if (continueOuter)
				continue;
			for (physicConstraint in physicsConstraints) {
				if (physicConstraint.data.order == i) {
					sortPhysicsConstraint(physicConstraint);
					break;
				}
			}
		}

		for (bone in bones) {
			sortBone(bone);
		}
	}

	private static function contains(list:Array<ConstraintData>, element:ConstraintData):Bool {
		return list.indexOf(element) != -1;
	}

	private function sortIkConstraint(constraint:IkConstraint):Void {
		constraint.active = constraint.target.isActive()
			&& (!constraint.data.skinRequired || (this.skin != null && contains(this.skin.constraints, constraint.data)));
		if (!constraint.active)
			return;

		var target:Bone = constraint.target;
		sortBone(target);

		var constrained:Array<Bone> = constraint.bones;
		var parent:Bone = constrained[0];
		sortBone(parent);

		if (constrained.length == 1) {
			_updateCache.push(constraint);
			sortReset(parent.children);
		} else {
			var child:Bone = constrained[constrained.length - 1];
			sortBone(child);

			_updateCache.push(constraint);

			sortReset(parent.children);
			child.sorted = true;
		}

		_updateCache.push(constraint);

		sortReset(parent.children);
		constrained[constrained.length - 1].sorted = true;
	}

	private function sortPathConstraint(constraint:PathConstraint):Void {
		constraint.active = constraint.target.bone.isActive()
			&& (!constraint.data.skinRequired || (this.skin != null && contains(this.skin.constraints, constraint.data)));
		if (!constraint.active)
			return;

		var slot:Slot = constraint.target;
		var slotIndex:Int = slot.data.index;
		var slotBone:Bone = slot.bone;
		if (skin != null)
			sortPathConstraintAttachment(skin, slotIndex, slotBone);
		if (data.defaultSkin != null && data.defaultSkin != skin) {
			sortPathConstraintAttachment(data.defaultSkin, slotIndex, slotBone);
		}
		for (i in 0...data.skins.length) {
			sortPathConstraintAttachment(data.skins[i], slotIndex, slotBone);
		}

		var attachment:Attachment = slot.attachment;
		if (Std.isOfType(attachment, PathAttachment))
			sortPathConstraintAttachment2(attachment, slotBone);

		var constrainedBones:Array<Bone> = constraint.bones;
		for (bone in constrainedBones) {
			sortBone(bone);
		}

		_updateCache.push(constraint);

		for (bone in constrainedBones) {
			sortReset(bone.children);
		}
		for (bone in constrainedBones) {
			bone.sorted = true;
		}
	}

	private function sortTransformConstraint(constraint:TransformConstraint):Void {
		constraint.active = constraint.target.isActive()
			&& (!constraint.data.skinRequired || (this.skin != null && contains(this.skin.constraints, constraint.data)));
		if (!constraint.active)
			return;

		sortBone(constraint.target);

		var constrainedBones:Array<Bone> = constraint.bones;
		if (constraint.data.local) {
			for (bone in constrainedBones) {
				sortBone(bone.parent);
				sortBone(bone);
			}
		} else {
			for (bone in constrainedBones) {
				sortBone(bone);
			}
		}

		_updateCache.push(constraint);
		for (bone in constrainedBones) {
			sortReset(bone.children);
		}
		for (bone in constrainedBones) {
			bone.sorted = true;
		}
	}

	private function sortPathConstraintAttachment(skin:Skin, slotIndex:Int, slotBone:Bone):Void {
		var dict:StringMap<Attachment> = skin.attachments[slotIndex];
		if (dict != null) {
			for (attachment in dict.keyValueIterator()) {
				sortPathConstraintAttachment2(attachment.value, slotBone);
			}
		}
	}

	private function sortPathConstraintAttachment2(attachment:Attachment, slotBone:Bone):Void {
		var pathAttachment:PathAttachment = cast(attachment, PathAttachment);
		if (pathAttachment == null)
			return;
		var pathBones:Array<Int> = pathAttachment.bones;
		if (pathBones == null) {
			sortBone(slotBone);
		} else {
			var i:Int = 0;
			var n:Int = pathBones.length;
			while (i < n) {
				var nn:Int = pathBones[i++];
				nn += i;
				while (i < nn) {
					sortBone(bones[pathBones[i++]]);
				}
			}
		}
	}

	private function sortPhysicsConstraint (constraint: PhysicsConstraint) {
		var bone:Bone = constraint.bone;
		constraint.active = bone.active && (!constraint.data.skinRequired || (skin != null && contains(skin.constraints, constraint.data)));
		if (!constraint.active) return;

		sortBone(bone);

		_updateCache.push(constraint);

		sortReset(bone.children);
		bone.sorted = true;
	}

	private function sortBone(bone:Bone):Void {
		if (bone.sorted)
			return;
		var parent:Bone = bone.parent;
		if (parent != null)
			sortBone(parent);
		bone.sorted = true;
		_updateCache.push(bone);
	}

	private function sortReset(bones:Array<Bone>):Void {
		for (bone in bones) {
			if (!bone.active)
				continue;
			if (bone.sorted)
				sortReset(bone.children);
			bone.sorted = false;
		}
	}

	/** Updates the world transform for each bone and applies constraints. */
	public function updateWorldTransform(physics:Physics):Void {
		if (physics == null) throw new SpineException("physics is undefined");
		for (bone in bones) {
			bone.ax = bone.x;
			bone.ay = bone.y;
			bone.arotation = bone.rotation;
			bone.ascaleX = bone.scaleX;
			bone.ascaleY = bone.scaleY;
			bone.ashearX = bone.shearX;
			bone.ashearY = bone.shearY;
		}

		for (updatable in _updateCache) {
			updatable.update(physics);
		}
	}

	public function updateWorldTransformWith(physics:Physics, parent:Bone):Void {
		// Apply the parent bone transform to the root bone. The root bone always inherits scale, rotation and reflection.
		var rootBone:Bone = rootBone;
		var pa:Float = parent.a,
			pb:Float = parent.b,
			pc:Float = parent.c,
			pd:Float = parent.d;
		rootBone.worldX = pa * x + pb * y + parent.worldX;
		rootBone.worldY = pc * x + pd * y + parent.worldY;

		var rx:Float = (rootBone.rotation + rootBone.shearX) * MathUtils.degRad;
		var ry:Float = (rootBone.rotation + 90 + rootBone.shearY) * MathUtils.degRad;
		var la:Float = Math.cos(rx) * rootBone.scaleX;
		var lb:Float = Math.cos(ry) * rootBone.scaleY;
		var lc:Float = Math.sin(rx) * rootBone.scaleX;
		var ld:Float = Math.sin(ry) * rootBone.scaleY;
		rootBone.a = (pa * la + pb * lc) * scaleX;
		rootBone.b = (pa * lb + pb * ld) * scaleX;
		rootBone.c = (pc * la + pd * lc) * scaleY;
		rootBone.d = (pc * lb + pd * ld) * scaleY;

		// Update everything except root bone.
		for (updatable in _updateCache) {
			if (updatable != rootBone)
				updatable.update(physics);
		}
	}

	/** Sets the bones, constraints, and slots to their setup pose values. */
	public function setToSetupPose():Void {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	}

	/** Sets the bones and constraints to their setup pose values. */
	public function setBonesToSetupPose():Void {
		for (bone in this.bones) bone.setToSetupPose();
		for (constraint in this.ikConstraints) constraint.setToSetupPose();
		for (constraint in this.transformConstraints) constraint.setToSetupPose();
		for (constraint in this.pathConstraints) constraint.setToSetupPose();
		for (constraint in this.physicsConstraints) constraint.setToSetupPose();
	}

	public function setSlotsToSetupPose():Void {
		var i:Int = 0;
		for (slot in slots) {
			drawOrder[i++] = slot;
			slot.setToSetupPose();
		}
	}

	public var data(get, never):SkeletonData;

	private function get_data():SkeletonData {
		return _data;
	}

	public var getUpdateCache(get, never):Array<Updatable>;

	private function get_getUpdateCache():Array<Updatable> {
		return _updateCache;
	}

	public var rootBone(get, never):Bone;

	private function get_rootBone():Bone {
		if (bones.length == 0)
			return null;
		return bones[0];
	}

	/** @return May be null. */
	public function findBone(boneName:String):Bone {
		if (boneName == null) {
			throw new SpineException("boneName cannot be null.");
		}
		for (bone in bones) {
			if (bone.data.name == boneName)
				return bone;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex(boneName:String):Int {
		if (boneName == null) {
			throw new SpineException("boneName cannot be null.");
		}
		var i:Int = 0;
		for (bone in bones) {
			if (bone.data.name == boneName)
				return i;
			i++;
		}
		return -1;
	}

	/** @return May be null. */
	public function findSlot(slotName:String):Slot {
		if (slotName == null) {
			throw new SpineException("slotName cannot be null.");
		}
		for (slot in slots) {
			if (slot.data.name == slotName)
				return slot;
		}
		return null;
	}

	public var skinName(get, set):String;

	private function set_skinName(skinName:String):String {
		var skin:Skin = data.findSkin(skinName);
		if (skin == null)
			throw new SpineException("Skin not found: " + skinName);
		this.skin = skin;
		return skinName;
	}

	/** @return May be null. */
	private function get_skinName():String {
		return _skin == null ? null : _skin.name;
	}

	public var skin(get, set):Skin;

	private function get_skin():Skin {
		return _skin;
	}

	/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}.
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was
	 * no old skin, each slot's setup mode attachment is attached from the new skin.
	 * @param newSkin May be null. */
	private function set_skin(newSkin:Skin):Skin {
		if (newSkin == _skin)
			return null;
		if (newSkin != null) {
			if (skin != null) {
				newSkin.attachAll(this, skin);
			} else {
				var i:Int = 0;
				for (slot in slots) {
					var name:String = slot.data.attachmentName;
					if (name != null) {
						var attachment:Attachment = newSkin.getAttachment(i, name);
						if (attachment != null)
							slot.attachment = attachment;
					}
					i++;
				}
			}
		}
		_skin = newSkin;
		updateCache();
		return _skin;
	}

	/** @return May be null. */
	public function getAttachmentForSlotName(slotName:String, attachmentName:String):Attachment {
		return getAttachmentForSlotIndex(data.findSlot(slotName).index, attachmentName);
	}

	/** @return May be null. */
	public function getAttachmentForSlotIndex(slotIndex:Int, attachmentName:String):Attachment {
		if (attachmentName == null)
			throw new SpineException("attachmentName cannot be null.");
		if (skin != null) {
			var attachment:Attachment = skin.getAttachment(slotIndex, attachmentName);
			if (attachment != null)
				return attachment;
		}
		if (data.defaultSkin != null)
			return data.defaultSkin.getAttachment(slotIndex, attachmentName);
		return null;
	}

	/** @param attachmentName May be null. */
	public function setAttachment(slotName:String, attachmentName:String):Void {
		if (slotName == null)
			throw new SpineException("slotName cannot be null.");
		var i:Int = 0;
		for (slot in slots) {
			if (slot.data.name == slotName) {
				var attachment:Attachment = null;
				if (attachmentName != null) {
					attachment = getAttachmentForSlotIndex(i, attachmentName);
					if (attachment == null) {
						throw new SpineException("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
				}
				slot.attachment = attachment;
				return;
			}
			i++;
		}
		throw new SpineException("Slot not found: " + slotName);
	}

	/** @return May be null. */
	public function findIkConstraint(constraintName:String):IkConstraint {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (ikConstraint in ikConstraints) {
			if (ikConstraint.data.name == constraintName)
				return ikConstraint;
		}
		return null;
	}

	/** @return May be null. */
	public function findTransformConstraint(constraintName:String):TransformConstraint {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (transformConstraint in transformConstraints) {
			if (transformConstraint.data.name == constraintName)
				return transformConstraint;
		}
		return null;
	}

	/** @return May be null. */
	public function findPathConstraint(constraintName:String):PathConstraint {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (pathConstraint in pathConstraints) {
			if (pathConstraint.data.name == constraintName)
				return pathConstraint;
		}
		return null;
	}

	/** @return May be null. */
	public function findPhysicsConstraint(constraintName:String):PhysicsConstraint {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (physicsConstraint in physicsConstraints) {
			if (physicsConstraint.data.name == constraintName)
				return physicsConstraint;
		}
		return null;
	}

	public function toString():String {
		return _data.name != null ? _data.name : "Skeleton?";
	}

	private var _tempVertices = new Array<Float>();
	private var _bounds = new Rectangle();

	public function getBounds():Rectangle {
		var minX:Float = Math.POSITIVE_INFINITY;
		var minY:Float = Math.POSITIVE_INFINITY;
		var maxX:Float = Math.NEGATIVE_INFINITY;
		var maxY:Float = Math.NEGATIVE_INFINITY;
		for (slot in drawOrder) {
			var verticesLength:Int = 0;
			var vertices:Array<Float> = null;
			var attachment:Attachment = slot.attachment;
			if (Std.isOfType(attachment, RegionAttachment)) {
				verticesLength = 8;
				_tempVertices.resize(verticesLength);
				vertices = _tempVertices;
				cast(attachment, RegionAttachment).computeWorldVertices(slot, vertices, 0, 2);
			} else if (Std.isOfType(attachment, MeshAttachment)) {
				var mesh:MeshAttachment = cast(attachment, MeshAttachment);
				verticesLength = mesh.worldVerticesLength;
				_tempVertices.resize(verticesLength);
				vertices = _tempVertices;
				mesh.computeWorldVertices(slot, 0, verticesLength, vertices, 0, 2);
			}
			if (vertices != null) {
				var ii:Int = 0;
				var nn:Int = vertices.length;
				while (ii < nn) {
					var x:Float = vertices[ii], y:Float = vertices[ii + 1];
					minX = Math.min(minX, x);
					minY = Math.min(minY, y);
					maxX = Math.max(maxX, x);
					maxY = Math.max(maxY, y);
					ii += 2;
				}
			}
		}
		_bounds.x = minX;
		_bounds.y = minY;
		_bounds.width = maxX - minX;
		_bounds.height = maxY - minY;
		return _bounds;
	}

	public function update (delta:Float):Void {
		time += delta;
	}

	public function physicsTranslate (x:Float, y:Float):Void {
		for (physicsConstraint in physicsConstraints)
			physicsConstraint.translate(x, y);
	}

	public function physicsRotate (x:Float, y:Float, degrees:Float):Void {
		for (physicsConstraint in physicsConstraints)
			physicsConstraint.rotate(x, y, degrees);
	}
}
