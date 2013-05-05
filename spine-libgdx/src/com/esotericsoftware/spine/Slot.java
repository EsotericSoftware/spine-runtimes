/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
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
 ******************************************************************************/

package com.esotericsoftware.spine;

import com.esotericsoftware.spine.attachments.Attachment;

import com.badlogic.gdx.graphics.Color;

public class Slot {
	final SlotData data;
	final Bone bone;
	private final Skeleton skeleton;
	final Color color;
	Attachment attachment;
	private float attachmentTime;

	Slot () {
		data = null;
		bone = null;
		skeleton = null;
		color = new Color(1, 1, 1, 1);
	}

	public Slot (SlotData data, Skeleton skeleton, Bone bone) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		this.data = data;
		this.skeleton = skeleton;
		this.bone = bone;
		color = new Color();
		setToSetupPose();
	}

	/** Copy constructor. */
	public Slot (Slot slot, Skeleton skeleton, Bone bone) {
		if (slot == null) throw new IllegalArgumentException("slot cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		data = slot.data;
		this.skeleton = skeleton;
		this.bone = bone;
		color = new Color(slot.color);
		attachment = slot.attachment;
		attachmentTime = slot.attachmentTime;
	}

	public SlotData getData () {
		return data;
	}

	public Skeleton getSkeleton () {
		return skeleton;
	}

	public Bone getBone () {
		return bone;
	}

	public Color getColor () {
		return color;
	}

	/** @return May be null. */
	public Attachment getAttachment () {
		return attachment;
	}

	/** Sets the attachment and resets {@link #getAttachmentTime()}.
	 * @param attachment May be null. */
	public void setAttachment (Attachment attachment) {
		this.attachment = attachment;
		attachmentTime = skeleton.time;
	}

	public void setAttachmentTime (float time) {
		attachmentTime = skeleton.time - time;
	}

	/** Returns the time since the attachment was set. */
	public float getAttachmentTime () {
		return skeleton.time - attachmentTime;
	}

	void setToSetupPose (int slotIndex) {
		color.set(data.color);
		setAttachment(data.attachmentName == null ? null : skeleton.getAttachment(slotIndex, data.attachmentName));
	}

	public void setToSetupPose () {
		setToSetupPose(skeleton.data.slots.indexOf(data, true));
	}

	public String toString () {
		return data.name;
	}
}
