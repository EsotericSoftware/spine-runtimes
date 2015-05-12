/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
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
	public var bones:Vector.<Bone>;
	public var slots:Vector.<Slot>;
	public var drawOrder:Vector.<Slot>;
	public var ikConstraints:Vector.<IkConstraint>;
	private var _boneCache:Vector.<Vector.<Bone>> = new Vector.<Vector.<Bone>>();
	private var _skin:Skin;
	public var r:Number = 1, g:Number = 1, b:Number = 1, a:Number = 1;
	public var time:Number = 0;
	public var flipX:Boolean, flipY:Boolean;
	public var x:Number = 0, y:Number = 0;

	public function Skeleton (data:SkeletonData) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		_data = data;

		bones = new Vector.<Bone>();
		for each (var boneData:BoneData in data.bones) {
			var parent:Bone = boneData.parent == null ? null : bones[data.bones.indexOf(boneData.parent)];
			bones[bones.length] = new Bone(boneData, this, parent);
		}

		slots = new Vector.<Slot>();
		drawOrder = new Vector.<Slot>();
		for each (var slotData:SlotData in data.slots) {
			var bone:Bone = bones[data.bones.indexOf(slotData.boneData)];
			var slot:Slot = new Slot(slotData, bone);
			slots[slots.length] = slot;
			drawOrder[drawOrder.length] = slot;
		}
		
		ikConstraints = new Vector.<IkConstraint>()
		for each (var ikConstraintData:IkConstraintData in data.ikConstraints)
			ikConstraints[ikConstraints.length] = new IkConstraint(ikConstraintData, this);

		updateCache();
	}

	/** Caches information about bones and IK constraints. Must be called if bones or IK constraints are added or removed. */
	public function updateCache () : void {
		var ikConstraintsCount:int = ikConstraints.length;

		var arrayCount:int = ikConstraintsCount + 1;
		if (_boneCache.length > arrayCount) _boneCache.splice(arrayCount, _boneCache.length - arrayCount);
		for each (var cachedBones:Vector.<Bone> in _boneCache)
			cachedBones.length = 0;
		while (_boneCache.length < arrayCount)
			_boneCache[_boneCache.length] = new Vector.<Bone>();

		var nonIkBones:Vector.<Bone> = _boneCache[0];

		outer:
		for each (var bone:Bone in bones) {
			var current:Bone = bone;
			do {
				var ii:int = 0;
				for each (var ikConstraint:IkConstraint in ikConstraints) {
					var parent:Bone = ikConstraint.bones[0];
					var child:Bone = ikConstraint.bones[int(ikConstraint.bones.length - 1)];
					while (true) {
						if (current == child) {
							_boneCache[ii].push(bone);
							_boneCache[int(ii + 1)].push(bone);
							continue outer;
						}
						if (child == parent) break;
						child = child.parent;
					}
					ii++;
				}
				current = current.parent;
			} while (current != null);
			nonIkBones[nonIkBones.length] = bone;
		}
	}

	/** Updates the world transform for each bone and applies IK constraints. */
	public function updateWorldTransform () : void {
		var bone:Bone;
		for each (bone in bones)
			bone.rotationIK = bone.rotation;
		var i:int = 0, last:int = _boneCache.length - 1;
		while (true) {
			for each (bone in _boneCache[i])
				bone.updateWorldTransform();
			if (i == last) break;
			ikConstraints[i].apply();
			i++;
		}
	}

	/** Sets the bones and slots to their setup pose values. */
	public function setToSetupPose () : void {
		setBonesToSetupPose();
		setSlotsToSetupPose();
	}

	public function setBonesToSetupPose () : void {
		for each (var bone:Bone in bones)
			bone.setToSetupPose();

		for each (var ikConstraint:IkConstraint in ikConstraints) {
			ikConstraint.bendDirection = ikConstraint._data.bendDirection;
			ikConstraint.mix = ikConstraint._data.mix;
		}
	}

	public function setSlotsToSetupPose () : void {
		var i:int = 0;
		for each (var slot:Slot in slots) { 
			drawOrder[i++] = slot;
			slot.setToSetupPose();
		}
	}

	public function get data () : SkeletonData {
		return _data;
	}

	public function get rootBone () : Bone {
		if (bones.length == 0) return null;
		return bones[0];
	}

	/** @return May be null. */
	public function findBone (boneName:String) : Bone {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		for each (var bone:Bone in bones)
			if (bone._data._name == boneName) return bone;
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findBoneIndex (boneName:String) : int {
		if (boneName == null)
			throw new ArgumentError("boneName cannot be null.");
		var i:int = 0;
		for each (var bone:Bone in bones) {
			if (bone._data._name == boneName) return i;
			i++;
		}
		return -1;
	}

	/** @return May be null. */
	public function findSlot (slotName:String) : Slot {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		for each (var slot:Slot in slots)
			if (slot._data._name == slotName) return slot;
		return null;
	}

	/** @return -1 if the bone was not found. */
	public function findSlotIndex (slotName:String) : int {
		if (slotName == null)
			throw new ArgumentError("slotName cannot be null.");
		var i:int = 0;
		for each (var slot:Slot in slots) {
			if (slot._data._name == slotName) return i;
			i++;
		}
		return -1;
	}

	public function get skin () : Skin {
		return _skin;
	}

	public function set skinName (skinName:String) : void {
		var skin:Skin = data.findSkin(skinName);
		if (skin == null) throw new ArgumentError("Skin not found: " + skinName);
		this.skin = skin;
	}

	/** @return May be null. */
	public function get skinName () : String {
		return _skin == null ? null : _skin._name;
	}

	/** Sets the skin used to look up attachments before looking in the {@link SkeletonData#getDefaultSkin() default skin}. 
	 * Attachments from the new skin are attached if the corresponding attachment from the old skin was attached. If there was 
	 * no old skin, each slot's setup mode attachment is attached from the new skin.
	 * @param newSkin May be null. */
	public function set skin (newSkin:Skin) : void {
		if (newSkin) {
			if (skin)
				newSkin.attachAll(this, skin);
			else {
				var i:int = 0;
				for each (var slot:Slot in slots) {
					var name:String = slot._data.attachmentName;
					if (name) {
						var attachment:Attachment = newSkin.getAttachment(i, name);
						if (attachment) slot.attachment = attachment;
					}
					i++;
				}
			}
		}
		_skin = newSkin;
	}

	/** @return May be null. */
	public function getAttachmentForSlotName (slotName:String, attachmentName:String) : Attachment {
		return getAttachmentForSlotIndex(data.findSlotIndex(slotName), attachmentName);
	}

	/** @return May be null. */
	public function getAttachmentForSlotIndex (slotIndex:int, attachmentName:String) : Attachment {
		if (attachmentName == null) throw new ArgumentError("attachmentName cannot be null.");
		if (skin != null) {
			var attachment:Attachment = skin.getAttachment(slotIndex, attachmentName);
			if (attachment != null) return attachment;
		}
		if (data.defaultSkin != null) return data.defaultSkin.getAttachment(slotIndex, attachmentName);
		return null;
	}

	/** @param attachmentName May be null. */
	public function setAttachment (slotName:String, attachmentName:String) : void {
		if (slotName == null) throw new ArgumentError("slotName cannot be null.");
		var i:int = 0;
		for each (var slot:Slot in slots) {
			if (slot._data._name == slotName) {
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

	/** @return May be null. */
	public function findIkConstraint (ikConstraintName:String) : IkConstraint {
		if (ikConstraintName == null) throw new ArgumentError("ikConstraintName cannot be null.");
		for each (var ikConstraint:IkConstraint in ikConstraints)
			if (ikConstraint._data._name == ikConstraintName) return ikConstraint;
		return null;
	}

	public function update (delta:Number) : void {
		time += delta;
	}

	public function toString () : String {
		return _data.name != null ? _data.name : super.toString();
	}
}

}
