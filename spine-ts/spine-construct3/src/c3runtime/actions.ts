import type { SDKInstanceClass } from "./instance";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Acts =
{
	SetSkin (this: SDKInstanceClass, skinList: string) {
		this.setSkin(skinList.split(","));
	},

	FlipX (this: SDKInstanceClass, isFlippedX: boolean) {
		this.flipX(isFlippedX);
	},

	SetAnimation (this: SDKInstanceClass, track: number, animation: string, loop = false) {
		this.setAnimation(track, animation, loop);
	},

	AddAnimation (this: SDKInstanceClass, track: number, animation: string, loop = false, delay = 0) {
		this.addAnimation(track, animation, loop, delay);
	},

	Play (this: SDKInstanceClass) {
		this.play();
	},

	Stop (this: SDKInstanceClass) {
		this.stop();
	},

	AddEmptyAnimation (this: SDKInstanceClass, track: number, mixDuration: number, delay: number) {
		this.addEmptyAnimation(track, mixDuration, delay);
	},

	SetEmptyAnimation (this: SDKInstanceClass, track: number, mixDuration: number) {
		this.setEmptyAnimation(track, mixDuration);
	},

	SetAttachment (this: SDKInstanceClass, slotName: string, attachmentName: string) {
		this.setAttachment(slotName, attachmentName === "" ? null : attachmentName);
	},

	CreateCustomSkin (this: SDKInstanceClass, skinName: string) {
		this.createCustomSkin(skinName);
	},

	AddCustomSkin (this: SDKInstanceClass, customSkinName: string, skinToAddName: string) {
		this.addCustomSkin(customSkinName, skinToAddName);
	},

	SetCustomSkin (this: SDKInstanceClass, skinName: string) {
		this.setCustomSkin(skinName);
	},

	SetAnimationSpeed (this: SDKInstanceClass, speed: number) {
		this.setAnimationSpeed(speed);
	},

	SetAnimationTime (this: SDKInstanceClass, units: 0 | 1, track: number, time: number) {
		this.setAnimationTime(units, time, track);
	},

	SetAnimationMix (this: SDKInstanceClass, fromName: string, toName: string, duration: number) {
		this.setAnimationMix(fromName, toName, duration);
	},

	SetPhysicsMode (this: SDKInstanceClass, mode: 0 | 1 | 2 | 3) {
		this.setPhysicsMode(mode);
	},

	SetSkeletonColor (this: SDKInstanceClass, color: string) {
		this.setSkeletonColor(color);
	},

	SetTrackAlpha (this: SDKInstanceClass, alpha: number, trackIndex: number) {
		this.setTrackAlpha(alpha, trackIndex);
	},

	SetTrackMixBlend (this: SDKInstanceClass, mixBlend: 0 | 1 | 2 | 3, trackIndex: number) {
		this.setTrackMixBlend(mixBlend, trackIndex);
	},

	ClearTrack (this: SDKInstanceClass, trackIndex: number) {
		this.clearTrack(trackIndex);
	},

	SetSlotColor (this: SDKInstanceClass, slotName: string, color: string) {
		this.setSlotColor(slotName, color);
	},

	ResetSlotColors (this: SDKInstanceClass, slotName: string) {
		this.resetSlotColors(slotName);
	},

	UpdateBonePose (this: SDKInstanceClass, x: number, y: number, boneName: string) {
		this.updateBonePose(x, y, boneName);
	},

	AttachInstanceToBone (this: SDKInstanceClass, uid: number, boneName: string, offsetX: number, offsetY: number, offsetAngle: number) {
		this.attachInstanceToBone(uid, boneName, offsetX, offsetY, offsetAngle);
	},

	DetachInstanceFromBone (this: SDKInstanceClass, boneName: string) {
		this.detachInstanceFromBone(boneName);
	},

	AddHandle (this: SDKInstanceClass, type: 0 | 1, name: string, radius: number, debug: boolean) {
		this.addDragHandle(type, name, radius, debug);
	},

	RemoveHandle (this: SDKInstanceClass, type: 0 | 1, name: string) {
		this.removeDragHandle(type, name);
	}

};
