/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
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