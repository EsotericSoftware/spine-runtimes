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

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;

/** Organizes attachments for {@link Skeleton#drawOrder} purposes and provide a place to store state for an attachment.
 * <p>
 * State cannot be stored in an attachment itself because attachments are stateless and may be shared across multiple
 * skeletons. */
public class Slot extends Posed<SlotData, SlotPose> {
	final Skeleton skeleton;
	final Bone bone;
	int attachmentState;

	public Slot (SlotData data, Skeleton skeleton) {
		super(data, new SlotPose(), new SlotPose());
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.skeleton = skeleton;
		bone = skeleton.bones.items[data.boneData.index];
		if (data.setupPose.darkColor != null) {
			pose.darkColor = new Color();
			constrainedPose.darkColor = new Color();
		}
		setupPose();
	}

	/** Copy constructor. */
	public Slot (Slot slot, Bone bone, Skeleton skeleton) {
		super(slot.data, new SlotPose(), new SlotPose());
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.bone = bone;
		this.skeleton = skeleton;
		if (data.setupPose.darkColor != null) {
			pose.darkColor = new Color();
			constrainedPose.darkColor = new Color();
		}
		pose.set(slot.pose);
	}

	/** The bone this slot belongs to. */
	public Bone getBone () {
		return bone;
	}

	public void setupPose () {
		pose.color.set(data.setupPose.color);
		if (pose.darkColor != null) pose.darkColor.set(data.setupPose.darkColor);
		pose.sequenceIndex = data.setupPose.sequenceIndex;
		if (data.attachmentName == null)
			pose.setAttachment(null);
		else {
			pose.attachment = null;
			pose.setAttachment(skeleton.getAttachment(data.index, data.attachmentName));
		}
	}
}
