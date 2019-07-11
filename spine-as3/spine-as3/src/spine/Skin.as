/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
	import spine.attachments.MeshAttachment;
	import flash.utils.Dictionary;

	import spine.attachments.Attachment;

	/** Stores attachments by slot index and attachment name. */
	public class Skin {
		internal var _name : String;
		private var _attachments : Vector.<Dictionary> = new Vector.<Dictionary>();
		private var _bones: Vector.<BoneData> = new Vector.<BoneData>();
		private var _constraints: Vector.<ConstraintData> = new Vector.<ConstraintData>();

		public function Skin(name : String) {
			if (name == null) throw new ArgumentError("name cannot be null.");
			_name = name;
		}

		public function setAttachment(slotIndex : int, name : String, attachment : Attachment) : void {
			if (attachment == null) throw new ArgumentError("attachment cannot be null.");
			if (slotIndex >= _attachments.length) _attachments.length = slotIndex + 1;
			if (!_attachments[slotIndex]) _attachments[slotIndex] = new Dictionary();
			_attachments[slotIndex][name] = attachment;
		}
		
		public function addSkin (skin: Skin) : void {
			var i : Number = 0, j : Number = 0;
			var contained : Boolean = false;
			
			for(i = 0; i < skin._bones.length; i++) {
				var bone : BoneData = skin._bones[i];
				contained = false;
				for (j = 0; j < _bones.length; j++) {
					if (_bones[j] == bone) {
						contained = true;
						break;
					}
				}
				if (!contained) _bones.push(bone);
			}

			for(i = 0; i < skin._constraints.length; i++) {
				var constraint : ConstraintData = skin._constraints[i];
				contained = false;
				for (j = 0; j < this._constraints.length; j++) {
					if (_constraints[j] == constraint) {
						contained = true;
						break;
					}
				}
				if (!contained) _constraints.push(constraint);
			}

			var attachments : Vector.<SkinEntry> = skin.getAttachments();
			for (i = 0; i < attachments.length; i++) {
				var attachment : SkinEntry = attachments[i];
				setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
			}
		}
		
		public function copySkin (skin: Skin) : void {
			var i : Number = 0, j : Number = 0;
			var contained : Boolean = false;
			var attachment : SkinEntry;
			
			for(i = 0; i < skin._bones.length; i++) {
				var bone : BoneData = skin._bones[i];
				contained = false;
				for (j = 0; j < _bones.length; j++) {
					if (_bones[j] == bone) {
						contained = true;
						break;
					}
				}
				if (!contained) _bones.push(bone);
			}

			for(i = 0; i < skin._constraints.length; i++) {
				var constraint : ConstraintData = skin._constraints[i];
				contained = false;
				for (j = 0; j < this._constraints.length; j++) {
					if (_constraints[j] == constraint) {
						contained = true;
						break;
					}
				}
				if (!contained) _constraints.push(constraint);
			}

			var attachments : Vector.<SkinEntry> = skin.getAttachments();
			for (i = 0; i < attachments.length; i++) {
				attachment = attachments[i];
				if (attachment.attachment == null) continue;
				if (attachment.attachment is MeshAttachment) {
					attachment.attachment = MeshAttachment(attachment.attachment).newLinkedMesh();
					setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
				} else {
					attachment.attachment = attachment.attachment.copy();
					setAttachment(attachment.slotIndex, attachment.name, attachment.attachment);
				}
			}
		}
				
		public function getAttachment(slotIndex : int, name : String) : Attachment {
			if (slotIndex >= _attachments.length) return null;
			var dictionary : Dictionary = _attachments[slotIndex];
			return dictionary ? dictionary[name] : null;
		}
		
		public function removeAttachment (slotIndex : Number, name : String) : void {
			var dictionary : Dictionary = _attachments[slotIndex];
			if (dictionary) dictionary[name] = null;
		}
		
		public function getAttachments() : Vector.<SkinEntry> {
			var entries : Vector.<SkinEntry> = new Vector.<SkinEntry>();
			for (var slotIndex : int = 0; slotIndex < _attachments.length; slotIndex++) {
				var attachments : Dictionary = _attachments[slotIndex];
				if (attachments) {
					for (var name : String in attachments) {
						var attachment : Attachment = attachments[name];
						if (attachment) entries.push(new SkinEntry(slotIndex, name, attachment));
					}
				}
			}
			return entries;
		}
		
		public function getAttachmentsForSlot(slotIndex: int) : Vector.<SkinEntry> {
			var entries : Vector.<SkinEntry> = new Vector.<SkinEntry>();			
			var attachments : Dictionary = _attachments[slotIndex];
			if (attachments) {
				for (var name : String in attachments) {
					var attachment : Attachment = attachments[name];
					if (attachment) entries.push(new SkinEntry(slotIndex, name, attachment));
				}
			}			
			return entries;
		}
		
		public function clear () : void {
			_attachments.length = 0;
			_bones.length = 0;
			_constraints.length = 0;
		}
		
		public function get attachments() : Vector.<Dictionary> {
			return _attachments;
		}
		
		public function get bones() : Vector.<BoneData> {
			return _bones;
		}
		
		public function get constraints() : Vector.<ConstraintData> {
			return _constraints;
		}

		public function get name() : String {
			return _name;
		}

		public function toString() : String {
			return _name;
		}

		/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
		public function attachAll(skeleton : Skeleton, oldSkin : Skin) : void {
			var slotIndex : int = 0;
			for each (var slot : Slot in skeleton.slots) {
				var slotAttachment : Attachment = slot.attachment;
				if (slotAttachment && slotIndex < oldSkin._attachments.length) {
					var dictionary : Dictionary = oldSkin._attachments[slotIndex];
					for (var name : String in dictionary) {
						var skinAttachment : Attachment = dictionary[name];
						if (slotAttachment == skinAttachment) {
							var attachment : Attachment = getAttachment(slotIndex, name);
							if (attachment != null) slot.attachment = attachment;
							break;
						}
					}
				}
				slotIndex++;
			}
		}
	}	
}
