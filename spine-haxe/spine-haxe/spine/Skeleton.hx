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

package spine;

import spine.Rectangle;
import spine.attachments.Attachment;
import spine.attachments.ClippingAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;

/** Stores the current pose for a skeleton.
 *
 * @see https://esotericsoftware.com/spine-runtime-architecture#Instance-objects Instance objects in the Spine Runtimes Guide
 */
class Skeleton {
	private static var quadTriangles:Array<Int> = [0, 1, 2, 2, 3, 0];

	/** The skeleton's setup pose data. */
	public final data:SkeletonData;

	/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
	public final bones:Array<Bone>;

	/** The skeleton's slots. */
	public final slots:Array<Slot>; // Setup pose draw order.

	/** The skeleton's draw order. Use drawOrder.appliedPose for rendering and drawOrder.pose for changing the draw order. */
	public var drawOrder:DrawOrder;

	/** The skeleton's constraints. */
	public final constraints:Array<Constraint<Dynamic, Dynamic, Dynamic>>;

	/** The skeleton's physics constraints. */
	public final physics:Array<PhysicsConstraint>;

	/** The list of bones and constraints, sorted in the order they should be updated, as computed by Skeleton.updateCache(). */
	public final _updateCache = new Array<Dynamic>();

	private final resetCache = new Array<Posed<Dynamic, Dynamic>>();

	/** The skeleton's current skin. */
	public var skin(default, set):Skin = null;

	/** The color to tint all the skeleton's attachments. */
	public final color:Color;

	/** Sets the skeleton X position, which is added to the root bone worldX position.
	 *
	 * Bones that do not inherit translation are still affected by this property. */
	public var x:Float = 0;

	/** Sets the skeleton Y position, which is added to the root bone worldY position.
	 *
	 * Bones that do not inherit translation are still affected by this property. */
	public var y:Float = 0;

	/** Scales the entire skeleton on the X axis.
	 *
	 * Bones that do not inherit scale are still affected by this property. */
	public var scaleX:Float = 1;

	/** Scales the entire skeleton on the Y axis.
	 *
	 * Bones that do not inherit scale are still affected by this property. */
	public var scaleY(get, default):Float = 1;

	function get_scaleY() {
		return scaleY * Bone.yDir;
	}

	/** Returns the skeleton's time. This is used for time-based manipulations, such as spine.PhysicsConstraint.
	 *
	 * See Skeleton.update(). */
	public var time:Float = 0;

	public var windX:Float = 1;
	public var windY:Float = 0;
	public var gravityX:Float = 0;
	public var gravityY:Float = 1;

	public var _update = 0;

	/** Creates a new skeleton with the specified skeleton data. */
	public function new(data:SkeletonData) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		this.data = data;

		bones = new Array<Bone>();
		for (boneData in data.bones) {
			var bone:Bone;
			if (boneData.parent == null) {
				bone = new Bone(boneData, null);
			} else {
				var parent:Bone = bones[boneData.parent.index];
				bone = new Bone(boneData, parent);
				parent.children.push(bone);
			}
			bones.push(bone);
		}

		slots = new Array<Slot>();
		for (slotData in data.slots) {
			var slot = new Slot(slotData, this);
			slots.push(slot);
		}
		drawOrder = new DrawOrder(slots);

		physics = new Array<PhysicsConstraint>();
		constraints = new Array<Constraint<Dynamic, Dynamic, Dynamic>>();
		for (constraintData in data.constraints) {
			var constraint = constraintData.create(this);
			if (Std.isOfType(constraint, PhysicsConstraint))
				physics.push(cast(constraint, PhysicsConstraint));
			constraints.push(constraint);
		}

		color = new Color(1, 1, 1, 1);

		updateCache();
	}

	/** Caches information about bones and constraints. Must be called if the Skeleton.skin is modified or if bones,
	 * constraints, or weighted path attachments are added or removed. */
	public function updateCache():Void {
		_updateCache.resize(0);
		resetCache.resize(0);

		drawOrder.unconstrained();
		for (slot in slots)
			slot.unconstrained();

		for (bone in bones) {
			bone.sorted = bone.data.skinRequired;
			bone.active = !bone.sorted;
			bone.unconstrained();
		}

		if (skin != null) {
			var skinBones = skin.bones;
			for (i in 0...skin.bones.length) {
				var bone:Bone = bones[skinBones[i].index];
				do {
					bone.sorted = false;
					bone.active = true;
					bone = bone.parent;
				} while (bone != null);
			}
		}

		for (constraint in constraints)
			constraint.unconstrained();
		for (c in constraints) {
			var constraint:Constraint<Dynamic, Dynamic, Dynamic> = c;
			constraint.active = constraint.isSourceActive()
				&& (!constraint.data.skinRequired || (skin != null && contains(skin.constraints, cast constraint.data)));
			if (constraint.active)
				constraint.sort(this);
		}

		for (bone in bones)
			sortBone(bone);

		var updateCache = this._updateCache;
		var n = updateCache.length;
		for (i in 0...n) {
			var updatable = updateCache[i];
			if (Std.isOfType(updatable, Bone)) {
				var b:Bone = cast updatable;
				updateCache[i] = b.appliedPose;
			}
		}
	}

	private static function contains(list:Array<ConstraintData<Dynamic, Dynamic>>, element:Dynamic):Bool {
		return list.indexOf(element) != -1;
	}

	public function constrained(object:Posed<Dynamic, Dynamic>) {
		if (object.pose == object.appliedPose) {
			object.constrained();
			resetCache.push(object);
		}
	}

	public function sortBone(bone:Bone):Void {
		if (bone.sorted || !bone.active)
			return;
		var parent = bone.parent;
		if (parent != null)
			sortBone(parent);
		bone.sorted = true;
		_updateCache.push(bone);
	}

	public function sortReset(bones:Array<Bone>):Void {
		for (bone in bones) {
			if (bone.active) {
				if (bone.sorted)
					sortReset(bone.children);
				bone.sorted = false;
			}
		}
	}

	/** Updates the world transform for each bone and applies all constraints.
	 *
	 * @see https://esotericsoftware.com/spine-runtime-skeletons#World-transforms World transforms in the Spine Runtimes Guide
	 */
	public function updateWorldTransform(physics:Physics):Void {
		_update++;

		drawOrder.resetConstrained();
		for (resetable in resetCache)
			resetable.resetConstrained();

		for (updatable in _updateCache)
			updatable.update(this, physics);
	}

	/** Sets the bones, constraints, slots, and draw order to their setup pose values. */
	public function setupPose():Void {
		setupPoseBones();
		setupPoseSlots();
	}

	/** Sets the bones and constraints to their setup pose values. */
	public function setupPoseBones():Void {
		for (bone in this.bones)
			bone.setupPose();
		for (constraint in this.constraints)
			constraint.setupPose();
	}

	/** Sets the slots and draw order to their setup pose values. */
	public function setupPoseSlots():Void {
		drawOrder.setupPose();
		for (slot in slots)
			slot.setupPose();
	}

	/** Returns the root bone, or null if the skeleton has no bones. */
	public var rootBone(get, never):Bone;

	private function get_rootBone():Bone {
		return bones.length == 0 ? null : bones[0];
	}

	/** Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly. */
	public function findBone(boneName:String):Bone {
		if (boneName == null)
			throw new SpineException("boneName cannot be null.");
		for (bone in bones)
			if (bone.data.name == boneName)
				return bone;
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex(boneName:String):Int {
		if (boneName == null)
			throw new SpineException("boneName cannot be null.");
		var i:Int = 0;
		for (bone in bones) {
			if (bone.data.name == boneName)
				return i;
			i++;
		}
		return -1;
	}

	/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
	 * repeatedly. */
	public function findSlot(slotName:String):Slot {
		if (slotName == null)
			throw new SpineException("slotName cannot be null.");
		for (slot in slots)
			if (slot.data.name == slotName)
				return slot;
		return null;
	}

	/** The skeleton's current skin. */
	public var skinName(get, set):String;

	/** Sets a skin by name.
	 *
	 * See Skeleton.skin. */
	private function set_skinName(skinName:String):String {
		var skin:Skin = data.findSkin(skinName);
		if (skin == null)
			throw new SpineException("Skin not found: " + skinName);
		this.skin = skin;
		return skinName;
	}

	/** @return May be null. */
	private function get_skinName():String {
		return skin == null ? null : skin.name;
	}

	/** Sets the skin used to look up attachments before looking in the spine.SkeletonData default skin. If the
	 * skin is changed, Skeleton.updateCache() is called.
	 *
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no
	 * old skin, each slot's setup mode attachment is attached from the new skin.
	 *
	 * After changing the skin, the visible attachments can be reset to those attached in the setup pose by calling
	 * Skeleton.setSlotsToSetupPose(). Also, often spine.AnimationState.apply() is called before the next time the
	 * skeleton is rendered to allow any attachment keys in the current animation(s) to hide or show attachments from the new
	 * skin. */
	private function set_skin(newSkin:Skin):Skin {
		if (newSkin == skin)
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
							slot.pose.attachment = attachment;
					}
					i++;
				}
			}
		}
		skin = newSkin;
		updateCache();
		return skin;
	}

	/** Finds an attachment by looking in the Skeleton.skin and spine.SkeletonData defaultSkin using the slot name and attachment
	 * name.
	 *
	 * See Skeleton.getAttachmentForSlotIndex(). */
	public function getAttachmentForSlotName(slotName:String, attachmentName:String):Attachment {
		return getAttachmentForSlotIndex(data.findSlot(slotName).index, attachmentName);
	}

	/** Finds an attachment by looking in the Skeleton.skin and spine.SkeletonData defaultSkin using the slot index and
	 * attachment name. First the skin is checked and if the attachment was not found, the default skin is checked.
	 *
	 * @see https://esotericsoftware.com/spine-runtime-skins Runtime skins in the Spine Runtimes Guide
	 */
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

	/** A convenience method to set an attachment by finding the slot with Skeleton.findSlot(), finding the attachment with
	 * Skeleton.getAttachmentForSlotIndex(), then setting the slot's spine.Slot attachment.
	 * @param attachmentName May be null to clear the slot's attachment. */
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
				slot.pose.attachment = attachment;
				return;
			}
			i++;
		}
		throw new SpineException("Slot not found: " + slotName);
	}

	public function findConstraint<T:Constraint<Dynamic, Dynamic, Dynamic>>(constraintName:String, type:Class<T>):Null<T> {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		if (type == null)
			throw new SpineException("type cannot be null.");
		for (constraint in constraints)
			if (Std.isOfType(constraint, type) && constraint.data.name == constraintName)
				return Std.downcast(constraint, type);
		return null;
	}

	private var _tempVertices = new Array<Float>();
	private var _bounds = new Rectangle();

	/** Returns the axis aligned bounding box (AABB) of the region and mesh attachments for the current pose. Optionally applies
	 * clipping. */
	public function getBounds(clipper:SkeletonClipping = null):Rectangle {
		var minX = Math.POSITIVE_INFINITY;
		var minY = Math.POSITIVE_INFINITY;
		var maxX = Math.NEGATIVE_INFINITY;
		var maxY = Math.NEGATIVE_INFINITY;
		for (slot in drawOrder.appliedPose) {
			var verticesLength:Int = 0;
			var vertices:Array<Float> = null;
			var triangles:Array<Int> = null;
			var attachment:Attachment = slot.appliedPose.attachment;
			if (attachment != null) {
				if (Std.isOfType(attachment, RegionAttachment)) {
					verticesLength = 8;
					_tempVertices.resize(verticesLength);
					vertices = _tempVertices;
					var region:RegionAttachment = cast(attachment, RegionAttachment);
					region.computeWorldVertices(slot, region.getOffsets(slot.appliedPose), vertices, 0, 2);
					triangles = Skeleton.quadTriangles;
				} else if (Std.isOfType(attachment, MeshAttachment)) {
					var mesh:MeshAttachment = cast(attachment, MeshAttachment);
					verticesLength = mesh.worldVerticesLength;
					_tempVertices.resize(verticesLength);
					vertices = _tempVertices;
					mesh.computeWorldVertices(this, slot, 0, verticesLength, vertices, 0, 2);
					triangles = mesh.triangles;
				} else if (Std.isOfType(attachment, ClippingAttachment) && clipper != null) {
					clipper.clipEnd(slot);
					clipper.clipStart(this, slot, cast(attachment, ClippingAttachment));
					continue;
				}
				if (vertices != null) {
					if (clipper != null && clipper.isClipping() && clipper.clipTriangles(vertices, triangles, triangles.length)) {
						vertices = clipper.clippedVertices;
						verticesLength = clipper.clippedVertices.length;
					}
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
				if (clipper != null)
					clipper.clipEnd(slot);
			}
		}
		if (clipper != null)
			clipper.clipEnd();
		_bounds.x = minX;
		_bounds.y = minY;
		_bounds.width = maxX - minX;
		_bounds.height = maxY - minY;
		return _bounds;
	}

	/** Increments the skeleton's Skeleton.time. */
	public function update(delta:Float):Void {
		time += delta;
	}

	/** Calls spine.PhysicsConstraint.translate() for each physics constraint. */
	public function physicsTranslate(x:Float, y:Float):Void {
		for (physicsConstraint in physics)
			physicsConstraint.translate(x, y);
	}

	/** Calls spine.PhysicsConstraint.rotate() for each physics constraint. */
	public function physicsRotate(x:Float, y:Float, degrees:Float):Void {
		for (physicsConstraint in physics)
			physicsConstraint.rotate(x, y, degrees);
	}

	public function toString():String {
		return data.name != null ? data.name : "Skeleton?";
	}
}
