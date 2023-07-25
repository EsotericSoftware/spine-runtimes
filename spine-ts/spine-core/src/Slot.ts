/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Attachment, VertexAttachment } from "./attachments/Attachment";
import { Bone } from "./Bone";
import { Skeleton } from "./Skeleton";
import { SlotData } from "./SlotData";
import { Color } from "./Utils";

/** Stores a slot's current pose. Slots organize attachments for {@link Skeleton#drawOrder} purposes and provide a place to store
 * state for an attachment. State cannot be stored in an attachment itself because attachments are stateless and may be shared
 * across multiple skeletons. */
export class Slot {
	/** The slot's setup pose data. */
	data: SlotData;

	/** The bone this slot belongs to. */
	bone: Bone;

	/** The color used to tint the slot's attachment. If {@link #getDarkColor()} is set, this is used as the light color for two
	 * color tinting. */
	color: Color;

	/** The dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
	 * color's alpha is not used. */
	darkColor: Color | null = null;

	attachment: Attachment | null = null;

	attachmentState: number = 0;

	/** The index of the texture region to display when the slot's attachment has a {@link Sequence}. -1 represents the
	 * {@link Sequence#getSetupIndex()}. */
	sequenceIndex: number = -1;

	/** Values to deform the slot's attachment. For an unweighted mesh, the entries are local positions for each vertex. For a
	 * weighted mesh, the entries are an offset for each vertex which will be added to the mesh's local vertex positions.
	 *
	 * See {@link VertexAttachment#computeWorldVertices()} and {@link DeformTimeline}. */
	deform = new Array<number>();

	constructor (data: SlotData, bone: Bone) {
		if (!data) throw new Error("data cannot be null.");
		if (!bone) throw new Error("bone cannot be null.");
		this.data = data;
		this.bone = bone;
		this.color = new Color();
		this.darkColor = !data.darkColor ? null : new Color();
		this.setToSetupPose();
	}

	/** The skeleton this slot belongs to. */
	getSkeleton (): Skeleton {
		return this.bone.skeleton;
	}

	/** The current attachment for the slot, or null if the slot has no attachment. */
	getAttachment (): Attachment | null {
		return this.attachment;
	}

	/** Sets the slot's attachment and, if the attachment changed, resets {@link #sequenceIndex} and clears the {@link #deform}.
	 * The deform is not cleared if the old attachment has the same {@link VertexAttachment#getTimelineAttachment()} as the
	 * specified attachment. */
	setAttachment (attachment: Attachment | null) {
		if (this.attachment == attachment) return;
		if (!(attachment instanceof VertexAttachment) || !(this.attachment instanceof VertexAttachment)
			|| (<VertexAttachment>attachment).timelineAttachment != (<VertexAttachment>this.attachment).timelineAttachment) {
			this.deform.length = 0;
		}
		this.attachment = attachment;
		this.sequenceIndex = -1;
	}

	/** Sets this slot to the setup pose. */
	setToSetupPose () {
		this.color.setFromColor(this.data.color);
		if (this.darkColor) this.darkColor.setFromColor(this.data.darkColor!);
		if (!this.data.attachmentName)
			this.attachment = null;
		else {
			this.attachment = null;
			this.setAttachment(this.bone.skeleton.getAttachment(this.data.index, this.data.attachmentName));
		}
	}
}
