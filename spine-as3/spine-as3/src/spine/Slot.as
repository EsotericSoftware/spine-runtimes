/******************************************************************************
 * Spine Runtime Software License - Version 1.0
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Single User License or Spine Professional License must be
 *    purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine {
import spine.attachments.Attachment;

public class Slot {
	internal var _data:SlotData;
	internal var _bone:Bone;
	internal var _skeleton:Skeleton;
	public var r:Number;
	public var g:Number;
	public var b:Number;
	public var a:Number;
	internal var _attachment:Attachment;
	private var _attachmentTime:Number;

	public function Slot (data:SlotData, skeleton:Skeleton, bone:Bone) {
		if (data == null)
			throw new ArgumentError("data cannot be null.");
		if (skeleton == null)
			throw new ArgumentError("skeleton cannot be null.");
		if (bone == null)
			throw new ArgumentError("bone cannot be null.");
		_data = data;
		_skeleton = skeleton;
		_bone = bone;
		setToSetupPose();
	}

	public function get data () : SlotData {
		return _data;
	}

	public function get skeleton () : Skeleton {
		return _skeleton;
	}

	public function get bone () : Bone {
		return _bone;
	}

	/** @return May be null. */
	public function get attachment () : Attachment {
		return _attachment;
	}

	/** Sets the attachment and resets {@link #getAttachmentTime()}.
	 * @param attachment May be null. */
	public function set attachment (attachment:Attachment) : void {
		_attachment = attachment;
		_attachmentTime = _skeleton.time;
	}

	public function set attachmentTime (time:Number) : void {
		_attachmentTime = skeleton.time - time;
	}

	/** Returns the time since the attachment was set. */
	public function get attachmentTime () : Number {
		return skeleton.time - _attachmentTime;
	}

	public function setToSetupPose () : void {
		var slotIndex:int = skeleton.data.slots.indexOf(data);
		r = _data.r;
		g = _data.g;
		b = _data.b;
		a = _data.a;
		attachment = _data.attachmentName == null ? null : skeleton.getAttachmentForSlotIndex(slotIndex, data.attachmentName);
	}

	public function toString () : String {
		return _data.name;
	}
}

}
