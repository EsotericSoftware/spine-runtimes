
import type { SDKInstanceClass } from "./instance.ts";

const C3 = globalThis.C3;

C3.Plugins.EsotericSoftware_SpineConstruct3.Acts =
{
	Alert (this: SDKInstanceClass) {
		alert("Test property = " + this._getTestProperty());
	},

	SetSkin (this: SDKInstanceClass, skinList: string) {
		this.setSkin(skinList.split(","));
	},

	SetAnimation (this: SDKInstanceClass, track: number, animation: string, loop = false) {
		this.setAnimation(track, animation, loop);
	}

};
