
import type { SDKInstanceClass as SpineC3Instance } from "./instance.ts";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Exps =
{
	Double (this: SpineC3Instance, num: number) {
		return num * 2;
	},

	SlotAttachment (this: SpineC3Instance, slotName: string) {
		if (!this.skeleton) return "";
		const slot = this.skeleton.findSlot(slotName);
		if (!slot) return "";
		const attachment = slot.pose.getAttachment();
		return attachment ? attachment.name : "";
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

	BoneWorldX (this: SpineC3Instance, boneName: string) {
		return this.getBoneWorldX(boneName);
	},

	BoneWorldY (this: SpineC3Instance, boneName: string) {
		return this.getBoneWorldY(boneName);
	},

	CurrentSkin (this: SpineC3Instance) {
		return this.getCurrentSkin();
	},

	CurrentAnimation (this: SpineC3Instance, trackIndex: number) {
		return this.getCurrentAnimation(trackIndex);
	},
	GetEventData (this: SpineC3Instance, field: "float" | "int" | "string" | "balance" | "volume" | "audiopath" | "event" | "track" | "animation") {
		if (field === "float") return this.triggeredEventData?.floatValue;
		if (field === "int") return this.triggeredEventData?.intValue;
		if (field === "string") return this.triggeredEventData?.stringValue;
		if (field === "balance") return this.triggeredEventData?.balance;
		if (field === "volume") return this.triggeredEventData?.volume;
		if (field === "audiopath") return this.triggeredEventData?.data.audioPath;
		if (field === "event") return this.triggeredEventData?.data.name;
		if (field === "animation") return this.triggeredEventData?.animation;
		if (field === "track") return this.triggeredEventData?.track;
		return "";
	}
};

