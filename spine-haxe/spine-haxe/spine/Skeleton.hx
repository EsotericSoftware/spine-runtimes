package spine;

import openfl.errors.ArgumentError;
import openfl.utils.Dictionary;
import openfl.Vector;
import spine.attachments.Attachment;
import spine.attachments.MeshAttachment;
import spine.attachments.PathAttachment;
import spine.attachments.RegionAttachment;

class Skeleton {
	private var _data:SkeletonData;

	public var bones:Vector<Bone>;
	public var slots:Vector<Slot>;
	public var drawOrder:Vector<Slot>;
	public var ikConstraints:Vector<IkConstraint>;
	public var transformConstraints:Vector<TransformConstraint>;
	public var pathConstraints:Vector<PathConstraint>;

	private var _updateCache:Vector<Updatable> = new Vector<Updatable>();
	private var _skin:Skin;

	public var color:Color = new Color(1, 1, 1, 1);
	public var scaleX:Float = 1;
	public var scaleY:Float = 1;
	public var x:Float = 0;
	public var y:Float = 0;

	public function new(data:SkeletonData) {
		if (data == null) {
			throw new ArgumentError("data cannot be null.");
		}
		_data = data;

		bones = new Vector<Bone>();
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

		slots = new Vector<Slot>();
		drawOrder = new Vector<Slot>();
		for (slotData in data.slots) {
			var bone = bones[slotData.boneData.index];
			var slot:Slot = new Slot(slotData, bone);
			slots.push(slot);
			drawOrder.push(slot);
		}

		ikConstraints = new Vector<IkConstraint>();
		for (ikConstraintData in data.ikConstraints) {
			ikConstraints.push(new IkConstraint(ikConstraintData, this));
		}

		transformConstraints = new Vector<TransformConstraint>();
		for (transformConstraintData in data.transformConstraints) {
			transformConstraints.push(new TransformConstraint(transformConstraintData, this));
		}

		pathConstraints = new Vector<PathConstraint>();
		for (pathConstraintData in data.pathConstraints) {
			pathConstraints.push(new PathConstraint(pathConstraintData, this));
		}

		updateCache();
	}

	/** Caches information about bones and constraints. Must be called if bones, constraints, or weighted path attachments are
	 * added or removed. */
	public function updateCache():Void {
		_updateCache.length = 0;

		for (bone in bones) {
			bone.sorted = bone.data.skinRequired;
			bone.active = !bone.sorted;
		}

		if (skin != null) {
			var skinBones:Vector<BoneData> = skin.bones;
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
		var constraintCount:Int = ikCount + transformCount + pathCount;

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
		}

		for (bone in bones) {
			sortBone(bone);
		}
	}

	private static function contains(list:Vector<ConstraintData>, element:ConstraintData):Bool {
		return list.indexOf(element) != -1;
	}

	private function sortIkConstraint(constraint:IkConstraint):Void {
		constraint.active = constraint.target.isActive()
			&& (!constraint.data.skinRequired || (this.skin != null && contains(this.skin.constraints, constraint.data)));
		if (!constraint.active)
			return;

		var target:Bone = constraint.target;
		sortBone(target);

		var constrained:Vector<Bone> = constraint.bones;
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

		var constrainedBones:Vector<Bone> = constraint.bones;
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

		var constrainedBones:Vector<Bone> = constraint.bones;
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
		var dict:Dictionary<String, Attachment> = skin.attachments[slotIndex];
		if (dict != null) {
			for (attachment in dict.each()) {
				sortPathConstraintAttachment2(attachment, slotBone);
			}
		}
	}

	private function sortPathConstraintAttachment2(attachment:Attachment, slotBone:Bone):Void {
		var pathAttachment:PathAttachment = cast(attachment, PathAttachment);
		if (pathAttachment == null)
			return;
		var pathBones:Vector<Int> = pathAttachment.bones;
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

	private function sortBone(bone:Bone):Void {
		if (bone.sorted)
			return;
		var parent:Bone = bone.parent;
		if (parent != null)
			sortBone(parent);
		bone.sorted = true;
		_updateCache.push(bone);
	}

	private function sortReset(bones:Vector<Bone>):Void {
		for (bone in bones) {
			if (!bone.active)
				continue;
			if (bone.sorted)
				sortReset(bone.children);
			bone.sorted = false;
		}
	}

	/** Updates the world transform for each bone and applies constraints. */
	public function updateWorldTransform():Void {
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
			updatable.update();
		}
	}

	public function updateWorldTransformWith(parent:Bone):Void {
		// Apply the parent bone transform to the root bone. The root bone always inherits scale, rotation and reflection.
		var rootBone:Bone = rootBone;
		var pa:Float = parent.a,
			pb:Float = parent.b,
			pc:Float = parent.c,
			pd:Float = parent.d;
		rootBone.worldX = pa * x + pb * y + parent.worldX;
		rootBone.worldY = pc * x + pd * y + parent.worldY;

		var rotationY:Float = rootBone.rotation + 90 + rootBone.shearY;
		var la:Float = MathUtils.cosDeg(rootBone.rotation + rootBone.shearX) * rootBone.scaleX;
		var lb:Float = MathUtils.cosDeg(rotationY) * rootBone.scaleY;
		var lc:Float = MathUtils.sinDeg(rootBone.rotation + rootBone.shearX) * rootBone.scaleX;
		var ld:Float = MathUtils.sinDeg(rotationY) * rootBone.scaleY;
		rootBone.a = (pa * la + pb * lc) * scaleX;
		rootBone.b = (pa * lb + pb * ld) * scaleX;
		rootBone.c = (pc * la + pd * lc) * scaleY;
		rootBone.d = (pc * lb + pd * ld) * scaleY;

		// Update everything except root bone.
		for (updatable in _updateCache) {
			if (updatable != rootBone)
				updatable.update();
		}
	}

	/** Sets the bones, constraints, and slots to their setup pose values. */
	public function setToSetupPose():Void {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	}

	/** Sets the bones and constraints to their setup pose values. */
	public function setBonesToSetupPose():Void {
		for (bone in bones) {
			bone.setToSetupPose();
		}

		for (ikConstraint in ikConstraints) {
			ikConstraint.mix = ikConstraint.data.mix;
			ikConstraint.softness = ikConstraint.data.softness;
			ikConstraint.bendDirection = ikConstraint.data.bendDirection;
			ikConstraint.compress = ikConstraint.data.compress;
			ikConstraint.stretch = ikConstraint.data.stretch;
		}

		for (transformConstraint in transformConstraints) {
			transformConstraint.mixRotate = transformConstraint.data.mixRotate;
			transformConstraint.mixX = transformConstraint.data.mixX;
			transformConstraint.mixY = transformConstraint.data.mixY;
			transformConstraint.mixScaleX = transformConstraint.data.mixScaleX;
			transformConstraint.mixScaleY = transformConstraint.data.mixScaleY;
			transformConstraint.mixShearY = transformConstraint.data.mixShearY;
		}

		for (pathConstraint in pathConstraints) {
			pathConstraint.position = pathConstraint.data.position;
			pathConstraint.spacing = pathConstraint.data.spacing;
			pathConstraint.mixRotate = pathConstraint.data.mixRotate;
			pathConstraint.mixX = pathConstraint.data.mixX;
			pathConstraint.mixY = pathConstraint.data.mixY;
		}
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

	public var getUpdateCache(get, never):Vector<Updatable>;

	private function get_getUpdateCache():Vector<Updatable> {
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
			throw new ArgumentError("boneName cannot be null.");
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
			throw new ArgumentError("boneName cannot be null.");
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
			throw new ArgumentError("slotName cannot be null.");
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
			throw new ArgumentError("Skin not found: " + skinName);
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
			throw new ArgumentError("attachmentName cannot be null.");
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
			throw new ArgumentError("slotName cannot be null.");
		var i:Int = 0;
		for (slot in slots) {
			if (slot.data.name == slotName) {
				var attachment:Attachment = null;
				if (attachmentName != null) {
					attachment = getAttachmentForSlotIndex(i, attachmentName);
					if (attachment == null) {
						throw new ArgumentError("Attachment not found: " + attachmentName + ", for slot: " + slotName);
					}
				}
				slot.attachment = attachment;
				return;
			}
			i++;
		}
		throw new ArgumentError("Slot not found: " + slotName);
	}

	/** @return May be null. */
	public function findIkConstraint(constraintName:String):IkConstraint {
		if (constraintName == null)
			throw new ArgumentError("constraintName cannot be null.");
		for (ikConstraint in ikConstraints) {
			if (ikConstraint.data.name == constraintName)
				return ikConstraint;
		}
		return null;
	}

	/** @return May be null. */
	public function findTransformConstraint(constraintName:String):TransformConstraint {
		if (constraintName == null)
			throw new ArgumentError("constraintName cannot be null.");
		for (transformConstraint in transformConstraints) {
			if (transformConstraint.data.name == constraintName)
				return transformConstraint;
		}
		return null;
	}

	/** @return May be null. */
	public function findPathConstraint(constraintName:String):PathConstraint {
		if (constraintName == null)
			throw new ArgumentError("constraintName cannot be null.");
		for (pathConstraint in pathConstraints) {
			if (pathConstraint.data.name == constraintName)
				return pathConstraint;
		}
		return null;
	}

	public function toString():String {
		return _data.name != null ? _data.name : "Skeleton?";
	}

	public function getBounds(offset:Vector<Float>, size:Vector<Float>, temp:Vector<Float>):Void {
		if (offset == null)
			throw new ArgumentError("offset cannot be null.");
		if (size == null)
			throw new ArgumentError("size cannot be null.");
		var minX:Float = Math.POSITIVE_INFINITY;
		var minY:Float = Math.POSITIVE_INFINITY;
		var maxX:Float = Math.NEGATIVE_INFINITY;
		var maxY:Float = Math.NEGATIVE_INFINITY;
		for (slot in drawOrder) {
			var verticesLength:Int = 0;
			var vertices:Vector<Float> = null;
			var attachment:Attachment = slot.attachment;
			if (Std.isOfType(attachment, RegionAttachment)) {
				verticesLength = 8;
				temp.length = verticesLength;
				vertices = temp;
				cast(attachment, RegionAttachment).computeWorldVertices(slot, vertices, 0, 2);
			} else if (Std.isOfType(attachment, MeshAttachment)) {
				var mesh:MeshAttachment = cast(attachment, MeshAttachment);
				verticesLength = mesh.worldVerticesLength;
				temp.length = verticesLength;
				vertices = temp;
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
		offset[0] = minX;
		offset[1] = minY;
		size[0] = maxX - minX;
		size[1] = maxY - minY;
	}
}
