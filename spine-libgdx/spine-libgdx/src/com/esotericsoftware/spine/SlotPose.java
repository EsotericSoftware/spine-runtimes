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
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.Null;

import com.esotericsoftware.spine.Animation.DeformTimeline;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.Sequence;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Stores a slot's pose. */
public class SlotPose implements Pose<SlotPose> {
	final Color color = new Color(1, 1, 1, 1);
	@Null Color darkColor;
	@Null Attachment attachment; // Not used in setup pose.
	int sequenceIndex;
	final FloatArray deform = new FloatArray(0);

	SlotPose () {
	}

	public void set (SlotPose pose) {
		if (pose == null) throw new IllegalArgumentException("pose cannot be null.");
		color.set(pose.color);
		if (darkColor != null) darkColor.set(pose.darkColor);
		attachment = pose.attachment;
		sequenceIndex = pose.sequenceIndex;
		deform.size = 0;
		deform.addAll(pose.deform);
	}

	/** The color used to tint the slot's attachment. If {@link #darkColor} is set, this is used as the light color for two color
	 * tinting. */
	public Color getColor () {
		return color;
	}

	/** The dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
	 * color's alpha is not used. */
	public @Null Color getDarkColor () {
		return darkColor;
	}

	/** The current attachment for the slot, or null if the slot has no attachment. */
	public @Null Attachment getAttachment () {
		return attachment;
	}

	/** Sets the slot's attachment and, if the attachment changed, resets {@link #sequenceIndex} and clears the {@link #deform}.
	 * The deform is not cleared if the old attachment has the same {@link VertexAttachment#getTimelineAttachment()} as the
	 * specified attachment. */
	public void setAttachment (@Null Attachment attachment) {
		if (this.attachment == attachment) return;
		if (!(attachment instanceof VertexAttachment newAttachment) || !(this.attachment instanceof VertexAttachment oldAttachment)
			|| newAttachment.getTimelineAttachment() != oldAttachment.getTimelineAttachment()) {
			deform.clear();
		}
		this.attachment = attachment;
		sequenceIndex = -1;
	}

	/** The index of the texture region to display when the slot's attachment has a {@link Sequence}. -1 represents the
	 * {@link Sequence#getSetupIndex()}. */
	public int getSequenceIndex () {
		return sequenceIndex;
	}

	public void setSequenceIndex (int sequenceIndex) {
		this.sequenceIndex = sequenceIndex;
	}

	/** Values to deform the slot's attachment. For an unweighted mesh, the entries are local positions for each vertex. For a
	 * weighted mesh, the entries are an offset for each vertex which will be added to the mesh's local vertex positions.
	 * <p>
	 * See {@link VertexAttachment#computeWorldVertices(Skeleton, Slot, int, int, float[], int, int)} and
	 * {@link DeformTimeline}. */
	public FloatArray getDeform () {
		return deform;
	}
}
