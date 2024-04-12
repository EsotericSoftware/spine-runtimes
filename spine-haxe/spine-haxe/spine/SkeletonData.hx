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

import haxe.io.Bytes;
import openfl.utils.Assets;
import spine.animation.Animation;
import spine.atlas.TextureAtlas;
import spine.attachments.AtlasAttachmentLoader;

class SkeletonData {
	/** May be null. */
	public var name:String;

	public var bones:Array<BoneData> = new Array<BoneData>(); // Ordered parents first.
	public var slots:Array<SlotData> = new Array<SlotData>(); // Setup pose draw order.
	public var skins:Array<Skin> = new Array<Skin>();
	public var defaultSkin:Skin;
	public var events:Array<EventData> = new Array<EventData>();
	public var animations:Array<Animation> = new Array<Animation>();
	public var ikConstraints:Array<IkConstraintData> = new Array<IkConstraintData>();
	public var transformConstraints:Array<TransformConstraintData> = new Array<TransformConstraintData>();
	public var pathConstraints:Array<PathConstraintData> = new Array<PathConstraintData>();
	public var physicsConstraints:Array<PhysicsConstraintData> = new Array<PhysicsConstraintData>();
	public var x:Float = 0;
	public var y:Float = 0;
	public var width:Float = 0;
	public var height:Float = 0;
	public var referenceScale:Float = 100;
	public var version:String;
	public var hash:String;
	public var fps:Float = 0;
	public var imagesPath:String;
	public var audioPath:String;

	public static function from(data:Dynamic, atlas:TextureAtlas, scale:Float = 1.0):SkeletonData {
		if (Std.isOfType(data, Bytes)) {
			var loader = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
			loader.scale = scale;
			return loader.readSkeletonData(cast(data, Bytes));
		} else if (Std.isOfType(data, String)) {
			var loader = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			loader.scale = scale;
			return loader.readSkeletonData(cast(data, String));
		} else {
			throw new SpineException("Data must either be a String (.json) or Bytes (.skel) instance.");
		}
	}

	public function new() {}

	// --- Bones.

	/** @return May be null. */
	public function findBone(boneName:String):BoneData {
		if (boneName == null)
			throw new SpineException("boneName cannot be null.");
		for (i in 0...bones.length) {
			var bone:BoneData = bones[i];
			if (bone.name == boneName)
				return bone;
		}
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex(boneName:String):Int {
		if (boneName == null)
			throw new SpineException("boneName cannot be null.");
		for (i in 0...bones.length) {
			if (bones[i].name == boneName)
				return i;
		}
		return -1;
	}

	// --- Slots.

	/** @return May be null. */
	public function findSlot(slotName:String):SlotData {
		if (slotName == null)
			throw new SpineException("slotName cannot be null.");
		for (i in 0...slots.length) {
			var slot:SlotData = slots[i];
			if (slot.name == slotName)
				return slot;
		}
		return null;
	}

	// --- Skins.

	/** @return May be null. */
	public function findSkin(skinName:String):Skin {
		if (skinName == null)
			throw new SpineException("skinName cannot be null.");
		for (skin in skins) {
			if (skin.name == skinName)
				return skin;
		}
		return null;
	}

	// --- Events.

	/** @return May be null. */
	public function findEvent(eventName:String):EventData {
		if (eventName == null)
			throw new SpineException("eventName cannot be null.");
		for (eventData in events) {
			if (eventData.name == eventName)
				return eventData;
		}
		return null;
	}

	// --- Animations.

	/** @return May be null. */
	public function findAnimation(animationName:String):Animation {
		if (animationName == null)
			throw new SpineException("animationName cannot be null.");
		for (animation in animations) {
			if (animation.name == animationName)
				return animation;
		}
		return null;
	}

	// --- IK constraints.

	/** @return May be null. */
	public function findIkConstraint(constraintName:String):IkConstraintData {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (ikConstraintData in ikConstraints) {
			if (ikConstraintData.name == constraintName)
				return ikConstraintData;
		}
		return null;
	}

	// --- Transform constraints.

	/** @return May be null. */
	public function findTransformConstraint(constraintName:String):TransformConstraintData {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (transformConstraintData in transformConstraints) {
			if (transformConstraintData.name == constraintName)
				return transformConstraintData;
		}
		return null;
	}

	/** @return -1 if the transform constraint was not found. */
	public function findTransformConstraintIndex(transformConstraintName:String):Int {
		if (transformConstraintName == null)
			throw new SpineException("transformConstraintName cannot be null.");
		for (i in 0...transformConstraints.length) {
			if (transformConstraints[i].name == transformConstraintName)
				return i;
		}
		return -1;
	}

	// --- Path constraints.

	/** @return May be null. */
	public function findPathConstraint(constraintName:String):PathConstraintData {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (i in 0...pathConstraints.length) {
			var constraint:PathConstraintData = pathConstraints[i];
			if (constraint.name == constraintName)
				return constraint;
		}
		return null;
	}

	/** @return -1 if the path constraint was not found. */
	public function findPathConstraintIndex(pathConstraintName:String):Int {
		if (pathConstraintName == null)
			throw new SpineException("pathConstraintName cannot be null.");
		for (i in 0...pathConstraints.length) {
			if (pathConstraints[i].name == pathConstraintName)
				return i;
		}
		return -1;
	}

	// --- Physics constraints.

	/** @return May be null. */
	public function findPhysicsConstraint(constraintName:String):PhysicsConstraintData {
		if (constraintName == null)
			throw new SpineException("physicsConstraintName cannot be null.");
		for (i in 0...physicsConstraints.length) {
			var constraint:PhysicsConstraintData = physicsConstraints[i];
			if (constraint.name == constraintName)
				return constraint;
		}
		return null;
	}
	
	/** @return -1 if the path constraint was not found. */
	public function findPhysicsConstraintIndex(constraintName:String):Int {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (i in 0...physicsConstraints.length) {
			if (physicsConstraints[i].name == constraintName)
				return i;
		}
		return -1;
	}

	public function toString():String {
		return name;
	}
}
