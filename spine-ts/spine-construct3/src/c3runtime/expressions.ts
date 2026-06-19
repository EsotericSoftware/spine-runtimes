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

import type { Attachment, Skin } from "@esotericsoftware/spine-construct3-lib";
import type { SDKInstanceClass as SpineC3Instance } from "./instance";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Exps =
{
	SlotAttachment (this: SpineC3Instance, slotName: string) {
		const { skeleton } = this;
		if (!skeleton) return "";
		const slot = skeleton.findSlot(slotName);
		if (!slot) return "";
		const attachment = slot.pose.getAttachment();
		return attachment ? attachment.name : "";
	},

	SlotAttachmentPlaceholder (this: SpineC3Instance, slotName: string) {
		const { skeleton } = this;
		if (!skeleton) return "";
		const slot = skeleton.findSlot(slotName);
		if (!slot) return "";
		const attachment = slot.pose.getAttachment();
		if (!attachment) return "";
		const slotIndex = slot.data.index;
		const skin = skeleton.skin;
		if (skin) {
			const name = findPlaceholderName(skin, slotIndex, attachment);
			if (name) return name;
		}
		const defaultSkin = skeleton.data.defaultSkin;
		if (defaultSkin) {
			const name = findPlaceholderName(defaultSkin, slotIndex, attachment);
			if (name) return name;
		}
		return attachment.name;
	},

	BoneX (this: SpineC3Instance, boneName: string) {
		return this.getBoneX(boneName);
	},

	BoneY (this: SpineC3Instance, boneName: string) {
		return this.getBoneY(boneName);
	},

	BoneRotation (this: SpineC3Instance, boneName: string) {
		return this.getBoneRotation(boneName);
	},

	BoneLength (this: SpineC3Instance, boneName: string) {
		const { skeleton } = this;
		if (!skeleton) return 0;
		const bone = skeleton.findBone(boneName);
		if (!bone) return 0;
		return bone.data.length;
	},

	BoneWorldX (this: SpineC3Instance, boneName: string) {
		return this.getBoneWorldX(boneName);
	},

	BoneWorldY (this: SpineC3Instance, boneName: string) {
		return this.getBoneWorldY(boneName);
	},

	BoneLocalX (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalX(boneName);
	},

	BoneLocalY (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalY(boneName);
	},

	BoneLocalRotation (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalRotation(boneName);
	},

	BoneLocalScaleX (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalScaleX(boneName);
	},

	BoneLocalScaleY (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalScaleY(boneName);
	},

	BoneLocalShearX (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalShearX(boneName);
	},

	BoneLocalShearY (this: SpineC3Instance, boneName: string) {
		return this.getBoneLocalShearY(boneName);
	},

	CurrentSkin (this: SpineC3Instance) {
		return this.getCurrentSkin();
	},

	Animations (this: SpineC3Instance) {
		return this.getAnimations();
	},

	AnimationsCount (this: SpineC3Instance) {
		return this.getAnimationsCount();
	},

	AnimationName (this: SpineC3Instance, index: number) {
		return this.getAnimationName(index);
	},

	CurrentAnimation (this: SpineC3Instance, trackIndex: number) {
		return this.getCurrentAnimation(trackIndex);
	},
	GetEventData (this: SpineC3Instance, field: "float" | "int" | "string" | "balance" | "volume" | "audiopath" | "event" | "track" | "animation") {
		if (field === "float") return this.triggeredEventData?.floatValue ?? 0;
		if (field === "int") return this.triggeredEventData?.intValue ?? 0;
		if (field === "string") return this.triggeredEventData?.stringValue ?? "";
		if (field === "balance") return this.triggeredEventData?.balance ?? 0;
		if (field === "volume") return this.triggeredEventData?.volume ?? 0;
		if (field === "audiopath") return this.triggeredEventData?.data.audioPath ?? "";
		if (field === "event") return this.triggeredEventData?.data.name ?? "";
		if (field === "animation") return this.triggeredEventAnimation;
		if (field === "track") return this.triggeredEventTrack;
		return "";
	},

	TimeScale (this: SpineC3Instance, track: number) {
		if (!this.state) return 1;
		if (track < 0) return this.state.timeScale;
		const entry = this.state.getTrack(track);
		return entry?.timeScale ?? 1;
	},

	Loop (this: SpineC3Instance, track: number) {
		if (!this.state) return 0;
		const entry = this.state.getTrack(track);
		return entry?.loop ? 1 : 0;
	},

	CurrentAnimationStart (this: SpineC3Instance, trackIndex: number) {
		const track = this.state?.getTrack(trackIndex);
		return track?.animationStart ?? 0;
	},

	CurrentAnimationEnd (this: SpineC3Instance, trackIndex: number) {
		const track = this.state?.getTrack(trackIndex);
		return track?.animationEnd ?? 0;
	},

	CurrentAnimationLast (this: SpineC3Instance, trackIndex: number) {
		const track = this.state?.getTrack(trackIndex);
		return track?.animationLast ?? 0;
	},

	BoundsX (this: SpineC3Instance) {
		return this.getBounds().x;
	},

	BoundsY (this: SpineC3Instance) {
		return this.getBounds().y;
	},

	BoundsWidth (this: SpineC3Instance) {
		return this.getBounds().width;
	},

	BoundsHeight (this: SpineC3Instance) {
		return this.getBounds().height;
	},

	CollisionBodyUID (this: SpineC3Instance) {
		return this.getCollisionBodyUid();
	},

	BoundingBoxPointCount (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxPointCount(slotName, attachmentName);
	},

	BoundingBoxPointX (this: SpineC3Instance, slotName: string, attachmentName: string, index: number) {
		return this.getBoundingBoxPointX(slotName, attachmentName, index);
	},

	BoundingBoxPointY (this: SpineC3Instance, slotName: string, attachmentName: string, index: number) {
		return this.getBoundingBoxPointY(slotName, attachmentName, index);
	},

	BoundingBoxCenterX (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxCenterX(slotName, attachmentName);
	},

	BoundingBoxCenterY (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxCenterY(slotName, attachmentName);
	},

	BoundingBoxLeft (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxLeft(slotName, attachmentName);
	},

	BoundingBoxTop (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxTop(slotName, attachmentName);
	},

	BoundingBoxRight (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxRight(slotName, attachmentName);
	},

	BoundingBoxBottom (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxBottom(slotName, attachmentName);
	},

	BoundingBoxPolygonJson (this: SpineC3Instance, slotName: string, attachmentName: string) {
		return this.getBoundingBoxPolygonJson(slotName, attachmentName);
	}
};

function findPlaceholderName (skin: Skin, slotIndex: number, attachment: Attachment): string | null {
	const dictionary = skin.attachments[slotIndex];
	if (!dictionary) return null;
	for (const name in dictionary) {
		if (dictionary[name] === attachment) return name;
	}
	return null;
}
