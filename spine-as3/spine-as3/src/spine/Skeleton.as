/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
import spine.attachments.Attachment;

public class Skeleton {
	internal var _data:SkeletonData;
	internal var _bones:Vector.<Bone>;
	internal var _slots:Vector.<Slot>;
	internal var _drawOrder:Vector.<Slot>;
	internal var _skin:Skin;
	public var r:Number = 1;
	public var g:Number = 1;
	public var b:Number = 1;
	public var a:Number = 1;
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
			_bones[_bones.length] = new Bone(boneData, parent);
		}

		_slots = new Vector.<Slot>();
		_drawOrder = new Vector.<Slot>();
		for each (var slotData:SlotData in data.slots) {
			var bone:Bone  = _bones[data.bones.indexOf(slotData.boneData)];
			var slot:Slot  = new Slot(slotData, this, bone);
			_slots[_slots.length] = slot;
			_drawOrder[_drawOrder.length] = slot;
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
		var i:int = 0;
		for each (var slot:Slot in _slots) { 
			drawOrder[i++] = slot;
			slot.setToSetupPose();
		}
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
	 * from the new skin are attached if the corresponding attachment from the old skin was attached. If there was no old skin,
	 * each slot's setup mode attachment is attached from the new skin.
	 * @param newSkin May be null. */
	public function set skin (newSkin:Skin) : void {
		if (!skin) {
			var i:int = 0;
			for each (var slot:Slot in _slots) {
				var name:String = slot.data.attachmentName;
				if (name) {
					var attachment:Attachment = newSkin.getAttachment(i, name);
					if (attachment) slot.attachment = attachment;
				}
				i++;
			}
		} else if (newSkin)
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
