package spine;

import spine.attachments.AtlasAttachmentLoader;
import openfl.utils.Assets;
import spine.atlas.TextureAtlas;
import openfl.errors.ArgumentError;
import openfl.Vector;
import spine.animation.Animation;

class SkeletonData {
	/** May be null. */
	public var name:String;

	public var bones:Vector<BoneData> = new Vector<BoneData>(); // Ordered parents first.
	public var slots:Vector<SlotData> = new Vector<SlotData>(); // Setup pose draw order.
	public var skins:Vector<Skin> = new Vector<Skin>();
	public var defaultSkin:Skin;
	public var events:Vector<EventData> = new Vector<EventData>();
	public var animations:Vector<Animation> = new Vector<Animation>();
	public var ikConstraints:Vector<IkConstraintData> = new Vector<IkConstraintData>();
	public var transformConstraints:Vector<TransformConstraintData> = new Vector<TransformConstraintData>();
	public var pathConstraints:Vector<PathConstraintData> = new Vector<PathConstraintData>();
	public var x:Float = 0;
	public var y:Float = 0;
	public var width:Float = 0;
	public var height:Float = 0;
	public var version:String;
	public var hash:String;
	public var fps:Float = 0;
	public var imagesPath:String;
	public var audioPath:String;

	public static function fromAssets(path:String, atlas:TextureAtlas, scale:Float = 1.0):SkeletonData {
		if (StringTools.endsWith(path, ".skel")) {
			var byteData = Assets.getBytes(path);
			var loader = new SkeletonBinary(new AtlasAttachmentLoader(atlas));
			loader.scale = scale;
			return loader.readSkeletonData(byteData);
		} else if (StringTools.endsWith(path, ".json")) {
			var jsonData = Assets.getText(path);
			var loader = new SkeletonJson(new AtlasAttachmentLoader(atlas));
			loader.scale = scale;
			return loader.readSkeletonData(jsonData);
		} else {
			throw new SpineException("Path of skeleton data file must end with .json or .skel");
		}
	}

	public function new() {}

	// --- Bones.

	/** @return May be null. */
	public function findBone(boneName:String):BoneData {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
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
			throw new ArgumentError("boneName cannot be null.");
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
			throw new ArgumentError("slotName cannot be null.");
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
			throw new ArgumentError("skinName cannot be null.");
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
			throw new ArgumentError("eventName cannot be null.");
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
			throw new ArgumentError("animationName cannot be null.");
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
			throw new ArgumentError("constraintName cannot be null.");
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
			throw new ArgumentError("constraintName cannot be null.");
		for (transformConstraintData in transformConstraints) {
			if (transformConstraintData.name == constraintName)
				return transformConstraintData;
		}
		return null;
	}

	/** @return -1 if the transform constraint was not found. */
	public function findTransformConstraintIndex(transformConstraintName:String):Int {
		if (transformConstraintName == null)
			throw new ArgumentError("transformConstraintName cannot be null.");
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
			throw new ArgumentError("constraintName cannot be null.");
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
			throw new ArgumentError("pathConstraintName cannot be null.");
		for (i in 0...pathConstraints.length) {
			if (pathConstraints[i].name == pathConstraintName)
				return i;
		}
		return -1;
	}

	public function toString():String {
		return name;
	}
}
