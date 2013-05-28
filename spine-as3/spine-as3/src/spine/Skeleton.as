package spine {
import spine.attachments.Attachment;

public class Skeleton {
	internal var _data:SkeletonData;
	internal var _bones:Vector.<Bone>;
	internal var _slots:Vector.<Slot>;
	internal var _drawOrder:Vector.<Slot>;
	internal var _skin:Skin;
	public var r:int = 1;
	public var g:int = 1;
	public var b:int = 1;
	public var a:int = 1;
	public var time:Number;
	public var flipX:Boolean;
	public var flipY:Boolean;
	public var x:Number = 0;
	public var y:Number = 0;

	public function Skeleton (data:SkeletonData) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		_data = data;

		_bones = new Vector.<Bone>();
		for each (var boneData:BoneData in data.bones) {
			var parent:Bone = boneData.parent == null ? null : _bones[data.bones.indexOf(boneData.parent)];
			_bones.push(new Bone(boneData, parent));
		}

		_slots = new Vector.<Slot>();
		_drawOrder = new Vector.<Slot>();
		for each (var slotData:SlotData in data.slots) {
			var bone:Bone  = _bones[data.bones.indexOf(slotData.boneData)];
			var slot:Slot  = new Slot(slotData, this, bone);
			_slots.push(slot);
			_drawOrder.push(slot);
		}
	}

	/** Updates the world transform for each bone. */
	public function updateWorldTransform () : void {
		for each (var bone:Bone in _bones)
			bone.updateWorldTransform(flipX, flipY);
	}

	/** Sets the bones and slots to their setup pose values. */
	public function setToSetupPose () : void {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	}

	public function setBonesToSetupPose () : void {
		for each (var bone:Bone in _bones)
			bone.setToSetupPose();
	}

	public function setSlotsToSetupPose () : void {
		for each (var slot:Slot in _slots)
			slot.setToSetupPose();
	}

	public function get data () : SkeletonData {
		return _data;
	}

	public function get bones () : Vector.<Bone> {
		return _bones;
	}

	public function get rootBone () : Bone {
		if (_bones.length == 0)
			return null;
		return _bones[0];
	}

	/** @return May be null. */
	public function findBone (boneName:String) : Bone {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		for each (var bone:Bone in _bones)
			if (bone.data.name == boneName)
				return bone;
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex (boneName:String) : int {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		var i:int = 0;
		for each (var bone:Bone in _bones) {
			if (bone.data.name == boneName)
				return i;
			i++;
		}
		return -1;
	}

	public function get slots () : Vector.<Slot> {
		return _slots;
	}

	/** @return May be null. */
	public function findSlot (slotName:String) : Slot {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		for each (var slot:Slot in _slots)
			if (slot.data.name == slotName)
				return slot;
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findSlotIndex (slotName:String) : int {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		var i:int = 0;
		for each (var slot:Slot in _slots) {
			if (slot.data.name == slotName)
				return i;
			i++;
		}
		return -1;
	}

	public function get drawOrder () : Vector.<Slot> {
		return _drawOrder;
	}

	public function get skin () : Skin {
		return _skin;
	}

	public function set skinName (skinName:String) : void {
		var skin:Skin = data.findSkin(skinName);
		if (skin == null)
			throw new ArgumentError("Skin not found: " + skinName);
		this.skin = skin;
	}

	/** Sets the skin used to look up attachments not found in the {@link SkeletonData#getDefaultSkin() default skin}. Attachments
	 * from the new skin are attached if the corresponding attachment from the old skin was attached.
	 * @param newSkin May be null. */
	public function set skin (newSkin:Skin) : void {
		if (skin != null && newSkin != null)
			newSkin.attachAll(this, skin);
		_skin = newSkin;
	}

	/** @return May be null. */
	public function getAttachmentForSlotName (slotName:String, attachmentName:String) : Attachment {
		return getAttachmentForSlotIndex(data.findSlotIndex(slotName), attachmentName);
	}

	/** @return May be null. */
	public function getAttachmentForSlotIndex (slotIndex:int, attachmentName:String) : Attachment {
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
	public function setAttachment (slotName:String, attachmentName:String) : void {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		var i:int = 0;
		for each (var slot:Slot in _slots) {
			if (slot.data.name == slotName) {
				var attachment:Attachment = null;
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

	public function update (delta:Number) : void {
		time += delta;
	}

	public function toString () : String {
		return _data.name != null ? _data.name : super.toString();
	}
}

}
