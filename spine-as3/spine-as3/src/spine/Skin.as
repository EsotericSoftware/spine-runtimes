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
	import flash.utils.Dictionary;

	import spine.attachments.Attachment;

	/** Stores attachments by slot index and attachment name. */
	public class Skin {
		internal var _name : String;
		private var _attachments : Vector.<Dictionary> = new Vector.<Dictionary>();

		public function Skin(name : String) {
			if (name == null) throw new ArgumentError("name cannot be null.");
			_name = name;
		}

		public function addAttachment(slotIndex : int, name : String, attachment : Attachment) : void {
			if (attachment == null) throw new ArgumentError("attachment cannot be null.");
			if (slotIndex >= attachments.length) attachments.length = slotIndex + 1;
			if (!attachments[slotIndex]) attachments[slotIndex] = new Dictionary();
			attachments[slotIndex][name] = attachment;
		}

		/** @return May be null. */
		public function getAttachment(slotIndex : int, name : String) : Attachment {
			if (slotIndex >= attachments.length) return null;
			var dictionary : Dictionary = attachments[slotIndex];
			return dictionary ? dictionary[name] : null;
		}

		public function get attachments() : Vector.<Dictionary> {
			return _attachments;
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
				if (slotAttachment && slotIndex < oldSkin.attachments.length) {
					var dictionary : Dictionary = oldSkin.attachments[slotIndex];
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
