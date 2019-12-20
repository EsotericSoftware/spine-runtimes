/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {

	/** Stores the setup pose for a {@link Slot}. */
	export class SlotData {

		/** The index of the slot in {@link Skeleton#getSlots()}. */
		index: number;

		/** The name of the slot, which is unique across all slots in the skeleton. */
		name: string;

		/** The bone this slot belongs to. */
		boneData: BoneData;

		/** The color used to tint the slot's attachment. If {@link #getDarkColor()} is set, this is used as the light color for two
		 * color tinting. */
		color = new Color(1, 1, 1, 1);

		/** The dark color used to tint the slot's attachment for two color tinting, or null if two color tinting is not used. The dark
		 * color's alpha is not used. */
		darkColor: Color;

		/** The name of the attachment that is visible for this slot in the setup pose, or null if no attachment is visible. */
		attachmentName: string;

		/** The blend mode for drawing the slot's attachment. */
		blendMode: BlendMode;

		constructor (index: number, name: string, boneData: BoneData) {
			if (index < 0) throw new Error("index must be >= 0.");
			if (name == null) throw new Error("name cannot be null.");
			if (boneData == null) throw new Error("boneData cannot be null.");
			this.index = index;
			this.name = name;
			this.boneData = boneData;
		}
	}
}
