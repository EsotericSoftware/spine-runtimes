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

package spine;

import spine.attachments.Attachment;
import spine.attachments.VertexAttachment;

class Slot {
	private var _data:SlotData;
	private var _bone:Bone;

	public var color:Color;
	public var darkColor:Color;

	private var _attachment:Attachment;

	public var sequenceIndex = -1;

	public var attachmentState:Int = 0;
	public var deform:Array<Float> = new Array<Float>();

	public function new(data:SlotData, bone:Bone) {
		if (data == null)
			throw new SpineException("data cannot be null.");
		if (bone == null)
			throw new SpineException("bone cannot be null.");
		_data = data;
		_bone = bone;
		this.color = new Color(1, 1, 1, 1);
		this.darkColor = data.darkColor == null ? null : new Color(1, 1, 1, 1);
		setToSetupPose();
	}

	public var data(get, never):SlotData;

	private function get_data():SlotData {
		return _data;
	}

	public var bone(get, never):Bone;

	private function get_bone():Bone {
		return _bone;
	}

	public var skeleton(get, never):Skeleton;

	private function get_skeleton():Skeleton {
		return _bone.skeleton;
	}

	/** @return May be null. */
	public var attachment(get, set):Attachment;

	private function get_attachment():Attachment {
		return _attachment;
	}

	/** Sets the slot's attachment and, if the attachment changed, resets {@link #attachmentTime} and clears the {@link #deform}.
	 * The deform is not cleared if the old attachment has the same {@link VertexAttachment#getDeformAttachment()} as the specified attachment.
	 * @param attachment May be null. */
	public function set_attachment(attachmentNew:Attachment):Attachment {
		if (attachment == attachmentNew)
			return attachmentNew;
		if (!Std.isOfType(attachmentNew, VertexAttachment)
			|| !Std.isOfType(attachment, VertexAttachment)
			|| cast(attachmentNew, VertexAttachment).timelineAttachment != cast(attachment, VertexAttachment).timelineAttachment) {
			deform = new Array<Float>();
		}
		_attachment = attachmentNew;
		sequenceIndex = -1;
		return attachmentNew;
	}

	public function setToSetupPose():Void {
		color.setFromColor(data.color);
		if (darkColor != null)
			darkColor.setFromColor(data.darkColor);
		if (_data.attachmentName == null) {
			attachment = null;
		} else {
			_attachment = null;
			attachment = skeleton.getAttachmentForSlotIndex(data.index, data.attachmentName);
		}
	}

	public function toString():String {
		return _data.name != null ? _data.name : "Slot?";
	}
}
