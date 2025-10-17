
import type { SDKInstanceClass } from "./instance.ts";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Cnds =
{
	OnSkeletonLoaded (this: SDKInstanceClass) {
		return true;
	},
	OnAnimationEvent (this: SDKInstanceClass, event: string, track: number, animation: string) {
		return this.triggeredEventName === event
			&& this.triggeredEventTrack === track
			&& this.triggeredEventAnimation === animation;
	},
};
