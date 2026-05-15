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

package com.esotericsoftware.spine.attachments;

import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.Slot;

/** The base class for all attachments. Multiple {@link Skeleton} instances, slots, or skins can use the same attachments. */
abstract public class Attachment {
	static private final int[] empty = new int[0];

	final String name;
	@Null Attachment timelineAttachment;
	int[] timelineSlots = empty;

	public Attachment (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		timelineAttachment = this;
	}

	/** Copy constructor. */
	protected Attachment (Attachment other) {
		name = other.name;
		timelineAttachment = other.timelineAttachment;
		timelineSlots = other.timelineSlots;
	}

	/** Timelines for the timeline attachment are also applied to this attachment.
	 * @return May be null if no attachment-specific timelines should be applied. */
	public @Null Attachment getTimelineAttachment () {
		return timelineAttachment;
	}

	/** @param timelineAttachment May be null if no attachment-specific timelines should be applied. */
	public void setTimelineAttachment (@Null Attachment timelineAttachment) {
		this.timelineAttachment = timelineAttachment;
	}

	/** Slots that can have attachments whose {@link #timelineAttachment} is this attachment. */
	public int[] getTimelineSlots () {
		return timelineSlots;
	}

	public void setTimelineSlots (int[] timelineSlots) {
		this.timelineSlots = timelineSlots;
	}

	/** Returns true if the <code>slotIndex</code> or any {@link #timelineSlots} have an attachment whose
	 * {@link #timelineAttachment} is this attachment.
	 * @param slots The {@link Skeleton#getSlots()}.
	 * @param slotIndex The timeline's primary slot index. */
	public boolean isTimelineActive (Slot[] slots, int slotIndex, boolean appliedPose) {
		Slot slot = slots[slotIndex];
		if (slot.getBone().isActive()) {
			Attachment other = (appliedPose ? slot.getAppliedPose() : slot.getPose()).getAttachment();
			if (other != null && other.timelineAttachment == this) return true;
		}
		for (int i = 0, n = timelineSlots.length; i < n; i++) {
			slot = slots[timelineSlots[i]];
			if (!slot.getBone().isActive()) continue;
			Attachment other = (appliedPose ? slot.getAppliedPose() : slot.getPose()).getAttachment();
			if (other != null && other.timelineAttachment == this) return true;
		}
		return false;
	}

	/** The attachment's name. */
	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}

	/** Returns a copy of the attachment. */
	abstract public Attachment copy ();
}
