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

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.FloatArray;

public class Slot {
	final SlotData data;
	final Bone bone;
	final Color color;
	Attachment attachment;
	private float attachmentTime;
	private FloatArray attachmentVertices = new FloatArray();

	Slot (SlotData data) {
		this.data = data;
		bone = null;
		color = new Color(1, 1, 1, 1);
	}

	public Slot (SlotData data, Bone bone) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		this.data = data;
		this.bone = bone;
		color = new Color();
		setToSetupPose();
	}

	/** Copy constructor. */
	public Slot (Slot slot, Bone bone) {
		if (slot == null) throw new IllegalArgumentException("slot cannot be null.");
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		data = slot.data;
		this.bone = bone;
		color = new Color(slot.color);
		attachment = slot.attachment;
		attachmentTime = slot.attachmentTime;
	}

	public SlotData getData () {
		return data;
	}

	public Bone getBone () {
		return bone;
	}

	public Skeleton getSkeleton () {
		return bone.skeleton;
	}

	public Color getColor () {
		return color;
	}

	/** @return May be null. */
	public Attachment getAttachment () {
		return attachment;
	}

	/** Sets the attachment, resets {@link #getAttachmentTime()}, and clears {@link #getAttachmentVertices()}.
	 * @param attachment May be null. */
	public void setAttachment (Attachment attachment) {
		if (this.attachment == attachment) return;
		this.attachment = attachment;
		attachmentTime = bone.skeleton.time;
		attachmentVertices.clear();
	}

	public void setAttachmentTime (float time) {
		attachmentTime = bone.skeleton.time - time;
	}

	/** Returns the time since the attachment was set. */
	public float getAttachmentTime () {
		return bone.skeleton.time - attachmentTime;
	}

	public void setAttachmentVertices (FloatArray attachmentVertices) {
		this.attachmentVertices = attachmentVertices;
	}

	public FloatArray getAttachmentVertices () {
		return attachmentVertices;
	}

	void setToSetupPose (int slotIndex) {
		color.set(data.color);
		setAttachment(data.attachmentName == null ? null : bone.skeleton.getAttachment(slotIndex, data.attachmentName));
	}

	public void setToSetupPose () {
		setToSetupPose(bone.skeleton.data.slots.indexOf(data, true));
	}

	public String toString () {
		return data.name;
	}
}
