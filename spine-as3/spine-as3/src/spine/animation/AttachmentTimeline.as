/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

package spine.animation {
	import spine.Slot;
	import spine.Event;
	import spine.Skeleton;

	public class AttachmentTimeline extends Timeline implements SlotTimeline {
		private var slotIndex : int;

		/** The attachment name for each key frame. May contain null values to clear the attachment. */
		public var attachmentNames : Vector.<String>;

		public function AttachmentTimeline (frameCount : int, slotIndex : int) {
			super(frameCount, [
				Property.attachment + "|" + slotIndex
			]);
			this.slotIndex = slotIndex;
			attachmentNames = new Vector.<String>(frameCount, true);
		}

		public override function getFrameCount () : int {
			return frames.length;
		}

		public function getSlotIndex() : int {
			return slotIndex;
		}

		/** Sets the time in seconds and the attachment name for the specified key frame. */
		public function setFrame (frame : int, time : Number, attachmentName : String) : void {
			frames[frame] = time;
			attachmentNames[frame] = attachmentName;
		}

		public override function apply (skeleton : Skeleton, lastTime : Number, time : Number, events : Vector.<Event>, alpha : Number, blend : MixBlend, direction : MixDirection) : void {
			var slot : Slot = skeleton.slots[slotIndex];
			if (!slot.bone.active) return;

			if (direction == MixDirection.mixOut) {
				if (blend == MixBlend.setup) setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}

			if (time < frames[0]) {
				if (blend == MixBlend.setup || blend == MixBlend.first) setAttachment(skeleton, slot, slot.data.attachmentName);
				return;
			}

			setAttachment(skeleton, slot, attachmentNames[search1(frames, time)]);
		}

		private function setAttachment(skeleton : Skeleton, slot : Slot, attachmentName : String) : void {
			slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
		}
	}
}
