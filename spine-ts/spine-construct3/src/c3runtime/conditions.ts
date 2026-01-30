import type { SDKInstanceClass as SpineC3Instance } from "./instance";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds =
{
	OnSkeletonLoaded (this: SpineC3Instance) {
		return true;
	},
	OnAnimationEvent (this: SpineC3Instance, event: string, track: number, animation: string) {
		const eventMatches = event === "" || this.triggeredEventName === event;
		const trackMatches = track === -1 || this.triggeredEventTrack === track;
		const animationMatches = animation === "" || this.triggeredEventAnimation === animation;
		return eventMatches && trackMatches && animationMatches;
	},
	IsFlippedX (this: SpineC3Instance) {
		return this.isFlippedX;
	},
	IsSkeletonLoaded (this: SpineC3Instance) {
		return this.skeletonLoaded;
	},
	IsPlaying (this: SpineC3Instance) {
		return this.isPlaying;
	},
	IsInsideSlot (this: SpineC3Instance, x: number, y: number, slotName: string) {
		return this.isInsideSlot(x, y, slotName);
	},
	IsAnimationPlaying (this: SpineC3Instance, animationName: string, trackIndex: number) {
		return this.isAnimationPlaying(animationName, trackIndex);
	},
};
