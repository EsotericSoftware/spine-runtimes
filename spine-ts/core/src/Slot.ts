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

module spine {
	export class Slot {
		data: SlotData;
		bone: Bone;
		color: Color;
		darkColor: Color;
		private attachment: Attachment;
		private attachmentTime: number;
		deform = new Array<number>();

		constructor (data: SlotData, bone: Bone) {
			if (data == null) throw new Error("data cannot be null.");
			if (bone == null) throw new Error("bone cannot be null.");
			this.data = data;
			this.bone = bone;
			this.color = new Color();
			this.darkColor = data.darkColor == null ? null : new Color();
			this.setToSetupPose();
		}

		/** @return May be null. */
		getAttachment (): Attachment {
			return this.attachment;
		}

		/** Sets the attachment and if it changed, resets {@link #getAttachmentTime()} and clears {@link #getAttachmentVertices()}.
		 * @param attachment May be null. */
		setAttachment (attachment: Attachment) {
			if (this.attachment == attachment) return;
			this.attachment = attachment;
			this.attachmentTime = this.bone.skeleton.time;
			this.deform.length = 0;
		}

		setAttachmentTime (time: number) {
			this.attachmentTime = this.bone.skeleton.time - time;
		}

		/** Returns the time since the attachment was set. */
		getAttachmentTime (): number {
			return this.bone.skeleton.time - this.attachmentTime;
		}

		setToSetupPose () {
			this.color.setFromColor(this.data.color);
			if (this.darkColor != null) this.darkColor.setFromColor(this.data.darkColor);
			if (this.data.attachmentName == null)
				this.attachment = null;
			else {
				this.attachment = null;
				this.setAttachment(this.bone.skeleton.getAttachment(this.data.index, this.data.attachmentName));
			}
		}
	}
}
