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

package spine.animation {
	import spine.Slot;
	import spine.Event;
	import spine.Skeleton;

	public class AttachmentTimeline implements Timeline {
		public var slotIndex : int;
		public var frames : Vector.<Number>; // time, ...
		public var attachmentNames : Vector.<String>;

		public function AttachmentTimeline(frameCount : int) {
			frames = new Vector.<Number>(frameCount, true);
			attachmentNames = new Vector.<String>(frameCount, true);
		}

		public function get frameCount() : int {
			return frames.length;
		}

		public function getPropertyId() : int {
			return (TimelineType.attachment.ordinal << 24) + slotIndex;
		}

		/** Sets the time and value of the specified keyframe. */
		public function setFrame(frameIndex : int, time : Number, attachmentName : String) : void {
			frames[frameIndex] = time;
			attachmentNames[frameIndex] = attachmentName;
		}

		public function apply(skeleton : Skeleton, lastTime : Number, time : Number, firedEvents : Vector.<Event>, alpha : Number, pose : MixPose, direction : MixDirection) : void {
			var attachmentName : String;
			var slot : Slot = skeleton.slots[slotIndex];
			if (direction == MixDirection.Out && pose == MixPose.setup) {
				attachmentName = slot.data.attachmentName;
				slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
				return;
			}
			var frames : Vector.<Number> = this.frames;
			if (time < frames[0]) {
				if (pose == MixPose.setup) {
					attachmentName = slot.data.attachmentName;
					slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
				}
				return;
			}

			var frameIndex : int;
			if (time >= frames[frames.length - 1]) // Time is after last frame.
				frameIndex = frames.length - 1;
			else
				frameIndex = Animation.binarySearch(frames, time, 1) - 1;

			attachmentName = attachmentNames[frameIndex];
			skeleton.slots[slotIndex].attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
		}
	}
}