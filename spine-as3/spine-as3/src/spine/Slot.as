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

public class Slot {
	internal var _data:SlotData;
	internal var _bone:Bone;
	public var r:Number;
	public var g:Number;
	public var b:Number;
	public var a:Number;
	internal var _attachment:Attachment;
	private var _attachmentTime:Number;
	public var attachmentVertices:Vector.<Number> = new Vector.<Number>();

	public function Slot (data:SlotData, bone:Bone) {
		if (data == null) throw new ArgumentError("data cannot be null.");
		if (bone == null) throw new ArgumentError("bone cannot be null.");
		_data = data;
		_bone = bone;
		setToSetupPose();
	}

	public function get data () : SlotData {
		return _data;
	}
	
	public function get bone () : Bone {
		return _bone;
	}
	
	public function get skeleton () : Skeleton {
		return _bone._skeleton;
	}

	/** @return May be null. */
	public function get attachment () : Attachment {
		return _attachment;
	}

	/** Sets the attachment and resets {@link #getAttachmentTime()}.
	 * @param attachment May be null. */
	public function set attachment (attachment:Attachment) : void {
		if (_attachment == attachment) return;
		_attachment = attachment;
		_attachmentTime = _bone._skeleton.time;
		attachmentVertices.length = 0;
	}

	public function set attachmentTime (time:Number) : void {
		_attachmentTime = _bone._skeleton.time - time;
	}

	/** Returns the time since the attachment was set. */
	public function get attachmentTime () : Number {
		return _bone._skeleton.time - _attachmentTime;
	}

	public function setToSetupPose () : void {		
		r = _data.r;
		g = _data.g;
		b = _data.b;
		a = _data.a;
		if (_data.attachmentName == null)
			attachment = null;
		else {
			_attachment = null;
			attachment = _bone._skeleton.getAttachmentForSlotIndex(data.index, data.attachmentName);
		}
	}

	public function toString () : String {
		return _data.name;
	}
}

}
