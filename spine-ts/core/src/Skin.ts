/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class Skin {
		name: string;
		attachments = new Array<Map<Attachment>>();

		constructor (name: string) {
			if (name == null) throw new Error("name cannot be null.");
			this.name = name;
		}

		addAttachment (slotIndex: number, name: string, attachment: Attachment) {
			if (attachment == null) throw new Error("attachment cannot be null.");
			let attachments = this.attachments;
			if (slotIndex >= attachments.length) attachments.length = slotIndex + 1;
			if (!attachments[slotIndex]) attachments[slotIndex] = { };
			attachments[slotIndex][name] = attachment;
		}

		/** @return May be null. */
		getAttachment (slotIndex: number, name: string): Attachment {
			let dictionary = this.attachments[slotIndex];
			return dictionary ? dictionary[name] : null;
		}

		/** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
		attachAll (skeleton: Skeleton, oldSkin: Skin) {
			let slotIndex = 0;
			for (let i = 0; i < skeleton.slots.length; i++) {
				let slot = skeleton.slots[i];
				let slotAttachment = slot.getAttachment();
				if (slotAttachment && slotIndex < oldSkin.attachments.length) {
					let dictionary = oldSkin.attachments[slotIndex];
					for (let key in dictionary) {
						let skinAttachment:Attachment = dictionary[key];
						if (slotAttachment == skinAttachment) {
							let attachment = this.getAttachment(slotIndex, key);
							if (attachment != null) slot.setAttachment(attachment);
							break;
						}
					}
				}
				slotIndex++;
			}
		}
	}
}
