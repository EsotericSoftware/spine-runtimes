/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.FloatArray;

import com.esotericsoftware.spine.Animation.DeformTimeline;
import com.esotericsoftware.spine.attachments.Attachment;
import com.esotericsoftware.spine.attachments.VertexAttachment;

/** Stores a slot's current pose. Slots organize attachments for {@link Skeleton#drawOrder} purposes and provide a place to store
 * state for an attachment. State cannot be stored in an attachment itself because attachments are stateless and may be shared
 * across multiple skeletons. */
public class Slot {
	final SlotData data;
	final Bone bone;
	final Color color = new Color(), darkColor;
	Attachment attachment;
	private float attachmentTime;
	private FloatArray deform = new FloatArray();

	public Slot (SlotData data, Bone bone) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		this.data = data;
		this.bone = bone;
		darkColor = data.darkColor == null ? null : new Color();
		setToSetupPose();
	}

	/** Copy constructor. */
	public Slot (Slot slot, Bone bone) {
		if (slot == null) throw new IllegalArgumentException("slot cannot be null.");
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		data = slot.data;
		this.bone = bone;
		color.set(slot.color);
		darkColor = slot.darkColor == null ? null : new Color(slot.darkColor);
		attachment = slot.attachment;
		attachmentTime = slot.attachmentTime;
		deform.addAll(slot.deform);
	}

	/** The slot's setup pose data. */
	public SlotData getData () {
		return data;
	}

	/** The bone this slot belongs to. */
	public Bone getBone () {
		return bone;
	}

	/** The skeleton this slot belongs to. */
	public Skeleton getSkeleton () {
		return bone.skeleton;
	}

	/** The color used to tint the slot's attachment. If {@link #getDarkColor()} is set, this is used as the light color for two
	 * color tinting. */
	public Color getColor () {
		return color;
	}

	/** The dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
	 * color's alpha is not used. */
	public Color getDarkColor () {
		return darkColor;
	}

	/** The current attachment for the slot, or null if the slot has no attachment. */
	public Attachment getAttachment () {
		return attachment;
	}

	/** Sets the slot's attachment and, if the attachment changed, resets {@link #attachmentTime} and clears {@link #deform}.
	 * @param attachment May be null. */
	public void setAttachment (Attachment attachment) {
		if (this.attachment == attachment) return;
		this.attachment = attachment;
		attachmentTime = bone.skeleton.time;
		deform.clear();
	}

	/** The time that has elapsed since the last time the attachment was set or cleared. Relies on Skeleton
	 * {@link Skeleton#time}. */
	public float getAttachmentTime () {
		return bone.skeleton.time - attachmentTime;
	}

	public void setAttachmentTime (float time) {
		attachmentTime = bone.skeleton.time - time;
	}

	/** Values to deform the slot's attachment. For an unweighted mesh, the entries are local positions for each vertex. For a
	 * weighted mesh, the entries are an offset for each vertex which will be added to the mesh's local vertex positions.
	 * <p>
	 * See {@link VertexAttachment#computeWorldVertices(Slot, int, int, float[], int, int)} and {@link DeformTimeline}. */
	public FloatArray getDeform () {
		return deform;
	}

	public void setDeform (FloatArray deform) {
		if (deform == null) throw new IllegalArgumentException("deform cannot be null.");
		this.deform = deform;
	}

	/** Sets this slot to the setup pose. */
	public void setToSetupPose () {
		color.set(data.color);
		if (darkColor != null) darkColor.set(data.darkColor);
		if (data.attachmentName == null)
			setAttachment(null);
		else {
			attachment = null;
			setAttachment(bone.skeleton.getAttachment(data.index, data.attachmentName));
		}
	}

	public String toString () {
		return data.name;
	}
}
