/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package spine.attachments;

import spine.Slot;

/** The base class for all attachments. */
class Attachment {
	private static final empty:Array<Int> = new Array<Int>();

	private var _name:String;

	/** Timelines for the timeline attachment are also applied to this attachment.
	 * May be null if no attachment-specific timelines should be applied. */
	public var timelineAttachment:Attachment;

	/** Slots that can have attachments whose timelineAttachment is this attachment. */
	public var timelineSlots:Array<Int> = empty;

	public function new(name:String) {
		if (name == null) {
			throw new SpineException("name cannot be null.");
		}
		_name = name;
		timelineAttachment = this;
	}

	/** Returns true if the `slotIndex` or any timelineSlots have an attachment whose timelineAttachment is
	 * this attachment.
	 * @param slots The Skeleton.slots.
	 * @param slotIndex The timeline's primary slot index. */
	public function isTimelineActive(slots:Array<Slot>, slotIndex:Int, appliedPose:Bool):Bool {
		var slot = slots[slotIndex];
		if (slot.bone.active) {
			var other = (appliedPose ? slot.appliedPose : slot.pose).attachment;
			if (other != null && other.timelineAttachment == this)
				return true;
		}
		for (i in 0...timelineSlots.length) {
			slot = slots[timelineSlots[i]];
			if (!slot.bone.active)
				continue;
			var other = (appliedPose ? slot.appliedPose : slot.pose).attachment;
			if (other != null && other.timelineAttachment == this)
				return true;
		}
		return false;
	}

	/** The attachment's name. */
	public var name(get, never):String;

	private function get_name():String {
		return _name;
	}

	public function toString():String {
		return name;
	}

	/** Returns a copy of the attachment. */
	public function copy():Attachment {
		throw new SpineException("Not implemented");
	}
}
