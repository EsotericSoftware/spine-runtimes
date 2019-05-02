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
	import spine.attachments.Attachment;

	public class Slot {
		internal var _data : SlotData;
		internal var _bone : Bone;
		public var color : Color;
		public var darkColor : Color;
		internal var _attachment : Attachment;
		private var _attachmentTime : Number;
		public var deform : Vector.<Number> = new Vector.<Number>();

		public function Slot(data : SlotData, bone : Bone) {
			if (data == null) throw new ArgumentError("data cannot be null.");
			if (bone == null) throw new ArgumentError("bone cannot be null.");
			_data = data;
			_bone = bone;
			this.color = new Color(1, 1, 1, 1);
			this.darkColor = data.darkColor == null ? null : new Color(1, 1, 1, 1);
			setToSetupPose();
		}

		public function get data() : SlotData {
			return _data;
		}

		public function get bone() : Bone {
			return _bone;
		}

		public function get skeleton() : Skeleton {
			return _bone._skeleton;
		}

		/** @return May be null. */
		public function get attachment() : Attachment {
			return _attachment;
		}

		/** Sets the attachment and resets {@link #getAttachmentTime()}.
		 * @param attachment May be null. */
		public function set attachment(attachment : Attachment) : void {
			if (_attachment == attachment) return;
			_attachment = attachment;
			_attachmentTime = _bone._skeleton.time;
			deform.length = 0;
		}

		public function set attachmentTime(time : Number) : void {
			_attachmentTime = _bone._skeleton.time - time;
		}

		/** Returns the time since the attachment was set. */
		public function get attachmentTime() : Number {
			return _bone._skeleton.time - _attachmentTime;
		}

		public function setToSetupPose() : void {
			color.setFromColor(data.color);
			if (darkColor != null) darkColor.setFromColor(this.data.darkColor);
			if (_data.attachmentName == null)
				attachment = null;
			else {
				_attachment = null;
				attachment = _bone._skeleton.getAttachmentForSlotIndex(data.index, data.attachmentName);
			}
		}

		public function toString() : String {
			return _data.name;
		}
	}
}
