/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.animation;

import spine.Event;
import spine.Skeleton;
import spine.Slot;

class AttachmentTimeline extends Timeline implements SlotTimeline {
	public var slotIndex:Int = 0;

	/** The attachment name for each key frame. May contain null values to clear the attachment. */
	public var attachmentNames:Array<String>;

	public function new(frameCount:Int, slotIndex:Int) {
		super(frameCount, [Property.attachment + "|" + slotIndex]);
		this.slotIndex = slotIndex;
		attachmentNames = new Array<String>();
		attachmentNames.resize(frameCount);
	}

	public override function getFrameCount():Int {
		return frames.length;
	}

	public function getSlotIndex():Int {
		return slotIndex;
	}

	/** Sets the time in seconds and the attachment name for the specified key frame. */
	public function setFrame(frame:Int, time:Float, attachmentName:String):Void {
		frames[frame] = time;
		attachmentNames[frame] = attachmentName;
	}

	public override function apply(skeleton:Skeleton, lastTime:Float, time:Float, events:Array<Event>, alpha:Float, blend:MixBlend,
			direction:MixDirection):Void {
		var slot:Slot = skeleton.slots[slotIndex];
		if (!slot.bone.active)
			return;

		if (direction == MixDirection.mixOut) {
			if (blend == MixBlend.setup) {
				setAttachment(skeleton, slot, slot.data.attachmentName);
			}
			return;
		}

		if (time < frames[0]) {
			if (blend == MixBlend.setup || blend == MixBlend.first) {
				setAttachment(skeleton, slot, slot.data.attachmentName);
			}
			return;
		}

		setAttachment(skeleton, slot, attachmentNames[Timeline.search1(frames, time)]);
	}

	private function setAttachment(skeleton:Skeleton, slot:Slot, attachmentName:String):Void {
		slot.attachment = attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, attachmentName);
	}
}
