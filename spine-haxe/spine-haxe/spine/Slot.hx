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

package spine;

import spine.attachments.Attachment;
import spine.attachments.VertexAttachment;

/** Stores a slot's current pose. Slots organize attachments for Skeleton.drawOrder purposes and provide a place to store
 * state for an attachment. State cannot be stored in an attachment itself because attachments are stateless and may be shared
 * across multiple skeletons. */
class Slot extends Posed<SlotData, SlotPose> {
	public var skeleton:Skeleton;

	/** The bone this slot belongs to. */
	public var bone:Bone;

	public var attachmentState:Int;

	public function new(data:SlotData, skeleton:Skeleton) {
		super(data, new SlotPose(), new SlotPose());
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		this.skeleton = skeleton;
		bone = skeleton.bones[data.boneData.index];
		if (data.setupPose.darkColor != null) {
			pose.darkColor = new Color(1, 1, 1, 1);
			constrainedPose.darkColor = new Color(1, 1, 1, 1);
		}
		setupPose();
	}

	/** Copy method. */
	public function copy(slot:Slot, bone:Bone, skeleton:Skeleton):Slot {
		var copy = new Slot(slot.data, skeleton);
		if (bone == null)
			throw new SpineException("bone cannot be null.");
		if (skeleton == null)
			throw new SpineException("skeleton cannot be null.");
		this.bone = bone;
		if (data.setupPose.darkColor != null) {
			pose.darkColor = new Color(1, 1, 1, 1);
			constrainedPose.darkColor = new Color(1, 1, 1, 1);
		}
		copy.pose.set(slot.pose);
		return copy;
	}

	/** Sets this slot to the setup pose. */
	override public function setupPose():Void {
		pose.color.setFromColor(data.setupPose.color);
		if (pose.darkColor != null)
			pose.darkColor.setFromColor(data.setupPose.darkColor);
		pose.sequenceIndex = data.setupPose.sequenceIndex;
		if (data.attachmentName == null) {
			pose.attachment = null;
		} else {
			pose.attachment = null;
			pose.attachment = skeleton.getAttachmentForSlotIndex(data.index, data.attachmentName);
		}
	}
}
