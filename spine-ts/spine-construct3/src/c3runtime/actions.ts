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

import type { SDKInstanceClass } from "./instance";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Acts =
{
	SetSkin (this: SDKInstanceClass, skinList: string) {
		this.setSkin(skinList === "" ? [] : skinList.split(","));
	},

	Mirror (this: SDKInstanceClass, isMirrored: boolean) {
		this.mirror(isMirrored);
	},

	Flip (this: SDKInstanceClass, isFlipped: boolean) {
		this.flip(isFlipped);
	},

	SetAnimation (this: SDKInstanceClass, track: number, animation: string, loop = false, additive: 0 | 1 = 0) {
		this.setAnimation(track, animation, loop, additive === 1);
	},

	AddAnimation (this: SDKInstanceClass, track: number, animation: string, loop = false, delay = 0, additive: 0 | 1 = 0) {
		this.addAnimation(track, animation, loop, delay, additive === 1);
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

	SetTimeScale (this: SDKInstanceClass, track: number, timeScale: number) {
		this.setTimeScale(track, timeScale);
	},

	SetAnimationTime (this: SDKInstanceClass, track: number, time: number, units: 0 | 1) {
		this.setAnimationTime(units, time, track);
	},

	SetAnimationMix (this: SDKInstanceClass, fromName: string, toName: string, duration: number) {
		this.setAnimationMix(fromName, toName, duration);
	},

	SetDefaultMix (this: SDKInstanceClass, duration: number) {
		this.setDefaultMix(duration);
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


	ClearTrack (this: SDKInstanceClass, trackIndex: number) {
		this.clearTrack(trackIndex);
	},

	SetSlotColor (this: SDKInstanceClass, slotName: string, color: string) {
		this.setSlotColor(slotName, color);
	},

	ResetSlotColors (this: SDKInstanceClass, slotName: string) {
		this.resetSlotColors(slotName);
	},

	SetBonePose (this: SDKInstanceClass, boneName: string, mode: 0 | 1, applyMode: 0 | 1, x: number | string, y: number | string, rotation: number | string, scaleX: number | string, scaleY: number | string) {
		this.setBonePose(
			boneName,
			mode === 0 ? "local" : "game",
			applyMode === 0 ? "once" : "hold",
			toNumberOrUndefined(x),
			toNumberOrUndefined(y),
			toNumberOrUndefined(rotation),
			toNumberOrUndefined(scaleX),
			toNumberOrUndefined(scaleY),
		);
	},

	ReleaseBoneHold (this: SDKInstanceClass, boneName: string, resetToSetup: boolean) {
		this.releaseBoneHold(boneName, resetToSetup);
	},

	SetupPose (this: SDKInstanceClass, target: 0 | 1 | 2) {
		this.setupPose(target);
	},

	SetupBoneSlotPose (this: SDKInstanceClass, type: 0 | 1, name: string) {
		this.setupBoneSlotPose(type === 0 ? "bone" : "slot", name);
	},

	AttachInstanceToBone (this: SDKInstanceClass, uid: number, boneName: string, offsetX: number, offsetY: number, offsetAngle: number, offsetScaleX = 1, offsetScaleY = 1) {
		this.attachInstanceToBone(uid, boneName, offsetX, offsetY, offsetAngle, offsetScaleX, offsetScaleY);
	},

	AttachObjectToBone (this: SDKInstanceClass, objectClass: IObjectType, boneName: string, offsetX: number, offsetY: number, offsetAngle: number, offsetScaleX = 1, offsetScaleY = 1) {
		const pickedInstances = objectClass.getPickedInstances();
		for (const instance of pickedInstances) {
			this.attachInstanceToBone(instance.uid, boneName, offsetX, offsetY, offsetAngle, offsetScaleX, offsetScaleY);
		}
	},

	DetachObjectFromBone (this: SDKInstanceClass, objectClass: IObjectType, boneName: string) {
		const pickedInstances = objectClass.getPickedInstances();
		for (const instance of pickedInstances) {
			this.detachInstanceFromBoneByUid(instance.uid, boneName);
		}
	},

	DetachAllFromBone (this: SDKInstanceClass, boneName: string) {
		this.detachAllFromBone(boneName);
	},

	AddHandle (this: SDKInstanceClass, type: 0 | 1, name: string, radius: number, debug: boolean) {
		this.addDragHandle(type, name, radius, debug);
	},

	RemoveHandle (this: SDKInstanceClass, type: 0 | 1, name: string) {
		this.removeDragHandle(type, name);
	},

	SetCollisionBoundingBox (this: SDKInstanceClass, slotName: string, attachmentName: string) {
		this.setCollisionBoundingBox(slotName, attachmentName);
	},

	ClearCollisionBoundingBox (this: SDKInstanceClass) {
		this.clearCollisionBoundingBox();
	},

	SetCollisionBoundingBoxDebug (this: SDKInstanceClass, enabled: boolean) {
		this.setCollisionBoundingBoxDebug(enabled);
	},

	SetCollisionBodyDrivesObject (this: SDKInstanceClass, enabled: boolean) {
		this.setCollisionBodyDrivesObject(enabled);
	},

	SetRuntimeAssetCacheRetainedWhenUnused (this: SDKInstanceClass, enabled: boolean, scope: 0 | 1) {
		this.setRuntimeAssetCacheRetainedWhenUnused(enabled, scope === 0 ? "object-type" : "all");
	},

	ReleaseCachedSpineAssets (this: SDKInstanceClass) {
		this.releaseCachedSpineAssets();
	},

	ReleaseAllCachedSpineAssets (this: SDKInstanceClass) {
		this.releaseAllCachedSpineAssets();
	},

	SetBounds (this: SDKInstanceClass, x: number, y: number, width: number, height: number) {
		this.setBounds(x, y, width, height);
	},

	SetBoundsForSkinAnimation (this: SDKInstanceClass, skins: string, animation: string) {
		this.setBoundsForSkinAnimation(skins === "" ? [] : skins.split(","), animation);
	},

	SetBoundsForSetupPose (this: SDKInstanceClass) {
		this.setBoundsForSetupPose();
	}

};

function toNumberOrUndefined (x: number | string): number | undefined {
	if (typeof x === "number") {
		return Number.isFinite(x) ? x : undefined;
	}

	if (typeof x === "string") {
		const trimmed = x.trim();
		if (trimmed === "") return undefined;

		const n = Number(trimmed);
		return Number.isFinite(n) ? n : undefined;
	}

	return undefined;
}