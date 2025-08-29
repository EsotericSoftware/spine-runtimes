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

import haxe.io.Bytes;
import spine.animation.Animation;
import spine.atlas.TextureAtlas;
import spine.attachments.AtlasAttachmentLoader;

/** Stores the setup pose and all of the stateless data for a skeleton.
 *
 * 
 * @see https://esotericsoftware.com/spine-runtime-architecture#Data-objects Data objects in the Spine Runtimes
 * Guide. */
class SkeletonData {
	/** The skeleton's name, which by default is the name of the skeleton data file when possible, or null when a name hasn't been
	 * set. */
	public var name:String;

	/** The skeleton's bones, sorted parent first. The root bone is always the first bone. */
	public var bones:Array<BoneData> = new Array<BoneData>(); // Ordered parents first.
	/** The skeleton's slots in the setup pose draw order. */
	public var slots:Array<SlotData> = new Array<SlotData>(); // Setup pose draw order.
	/** All skins, including the default skin. */
	public var skins:Array<Skin> = new Array<Skin>();
	/** The skeleton's default skin. By default this skin contains all attachments that were not in a skin in Spine.
	 *
	 * See Skeleton#getAttachment(int, String). */
	public var defaultSkin:Skin;
	/** The skeleton's events. */
	public var events:Array<EventData> = new Array<EventData>();
	/** The skeleton's animations. */
	public var animations:Array<Animation> = new Array<Animation>();
	/** The skeleton's IK constraints. */
	public var ikConstraints:Array<IkConstraintData> = new Array<IkConstraintData>();
	/** The skeleton's transform constraints. */
	public var transformConstraints:Array<TransformConstraintData> = new Array<TransformConstraintData>();
	/** The skeleton's path constraints. */
	public var pathConstraints:Array<PathConstraintData> = new Array<PathConstraintData>();
	/** The skeleton's physics constraints. */
	public var physicsConstraints:Array<PhysicsConstraintData> = new Array<PhysicsConstraintData>();
	/** The X coordinate of the skeleton's axis aligned bounding box in the setup pose. */
	public var x:Float = 0;
	/** The Y coordinate of the skeleton's axis aligned bounding box in the setup pose. */
	public var y:Float = 0;
	/** The width of the skeleton's axis aligned bounding box in the setup pose. */
	public var width:Float = 0;
	/** The height of the skeleton's axis aligned bounding box in the setup pose. */
	public var height:Float = 0;
	/** Baseline scale factor for applying physics and other effects based on distance to non-scalable properties, such as angle or
	 * scale. Default is 100. */
	public var referenceScale:Float = 100;
	/** The Spine version used to export the skeleton data, or null. */
	public var version:String;
	/** The skeleton data hash. This value will change if any of the skeleton data has changed. */
	public var hash:String;
	/** The dopesheet FPS in Spine, or zero if nonessential data was not exported. */
	public var fps:Float = 0;
	/** The path to the images directory as defined in Spine, or null if nonessential data was not exported. */
	public var imagesPath:String;
	/** The path to the audio directory as defined in Spine, or null if nonessential data was not exported. */
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

	/** Finds a bone by comparing each bone's name. It is more efficient to cache the results of this method than to call it
	 * multiple times.
	 * @param boneName The name of the bone to find.
	 * @return May be null. */
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

	/** Finds the index of a bone by comparing each bone's name.
	 * @param boneName The name of the bone to find.
	 * @return -1 if the bone was not found. */
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

	/** Finds a slot by comparing each slot's name. It is more efficient to cache the results of this method than to call it
	 * multiple times.
	 * @param slotName The name of the slot to find.
	 * @return May be null. */
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

	/** Finds a skin by comparing each skin's name. It is more efficient to cache the results of this method than to call it
	 * multiple times.
	 * @param skinName The name of the skin to find.
	 * @return May be null. */
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

	/** Finds an event by comparing each events's name. It is more efficient to cache the results of this method than to call it
	 * multiple times.
	 * @param eventName The name of the event to find.
	 * @return May be null. */
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

	/** Finds an animation by comparing each animation's name. It is more efficient to cache the results of this method than to
	 * call it multiple times.
	 * @param animationName The name of the animation to find.
	 * @return May be null. */
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

	/** Finds an IK constraint by comparing each IK constraint's name. It is more efficient to cache the results of this method
	 * than to call it multiple times.
	 * @param constraintName The name of the IK constraint to find.
	 * @return May be null. */
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

	/** Finds a transform constraint by comparing each transform constraint's name. It is more efficient to cache the results of
	 * this method than to call it multiple times.
	 * @param constraintName The name of the transform constraint to find.
	 * @return May be null. */
	public function findTransformConstraint(constraintName:String):TransformConstraintData {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (transformConstraintData in transformConstraints) {
			if (transformConstraintData.name == constraintName)
				return transformConstraintData;
		}
		return null;
	}

	/** Finds the index of a transform constraint by comparing each transform constraint's name.
	 * @param transformConstraintName The name of the transform constraint to find.
	 * @return -1 if the transform constraint was not found. */
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

	/** Finds a path constraint by comparing each path constraint's name. It is more efficient to cache the results of this method
	 * than to call it multiple times.
	 * @param constraintName The name of the path constraint to find.
	 * @return May be null. */
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

	/** Finds the index of a path constraint by comparing each path constraint's name.
	 * @param pathConstraintName The name of the path constraint to find.
	 * @return -1 if the path constraint was not found. */
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

	/** Finds a physics constraint by comparing each physics constraint's name. It is more efficient to cache the results of this
	 * method than to call it multiple times.
	 * @param constraintName The name of the physics constraint to find.
	 * @return May be null. */
	public function findPhysicsConstraint(constraintName:String):PhysicsConstraintData {
		if (constraintName == null)
			throw new SpineException("constraintName cannot be null.");
		for (i in 0...physicsConstraints.length) {
			var constraint:PhysicsConstraintData = physicsConstraints[i];
			if (constraint.name == constraintName)
				return constraint;
		}
		return null;
	}
	
	/** Finds the index of a physics constraint by comparing each physics constraint's name.
	 * @param constraintName The name of the physics constraint to find.
	 * @return -1 if the physics constraint was not found. */
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
