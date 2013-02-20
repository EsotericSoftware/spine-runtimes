
package com.esotericsoftware.spine;

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
		color = new Color(1, 1, 1, 1);
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

	void setToBindPose (int slotIndex) {
		color.set(data.color);
		setAttachment(data.attachmentName == null ? null : skeleton.getAttachment(slotIndex, data.attachmentName));
	}

	public void setToBindPose () {
		setToBindPose(skeleton.slots.indexOf(this, true));
	}

	public String toString () {
		return data.name;
	}
}
