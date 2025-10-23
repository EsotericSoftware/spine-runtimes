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

import type { Animation } from "./Animation.js";
import type { SkeletonData } from "./SkeletonData.js";
import type { StringMap } from "./Utils.js";


/** Stores mix (crossfade) durations to be applied when {@link AnimationState} animations are changed. */
export class AnimationStateData {
	/** The SkeletonData to look up animations when they are specified by name. */
	skeletonData: SkeletonData;

	animationToMixTime: StringMap<number> = {};

	/** The mix duration to use when no mix duration has been defined between two animations. */
	defaultMix = 0;

	constructor (skeletonData: SkeletonData) {
		if (!skeletonData) throw new Error("skeletonData cannot be null.");
		this.skeletonData = skeletonData;
	}

	/** Sets a mix duration by animation name.
	 *
	 * See {@link #setMix()}. */
	setMix (fromName: string, to: string, duration: number): void;

	/** Sets the mix duration when changing from the specified animation to the other.
	 *
	 * See {@link TrackEntry#mixDuration}. */
	setMix (from: Animation, to: Animation, duration: number): void;

	setMix (from: string | Animation, to: string | Animation, duration: number) {
		if (typeof from === "string")
			return this.setMix1(from, to as string, duration);
		return this.setMix2(from, to as Animation, duration);
	}

	private setMix1 (fromName: string, toName: string, duration: number) {
		const from = this.skeletonData.findAnimation(fromName);
		if (!from) throw new Error(`Animation not found: ${fromName}`);
		const to = this.skeletonData.findAnimation(toName);
		if (!to) throw new Error(`Animation not found: ${toName}`);
		this.setMix2(from, to, duration);
	}

	private setMix2 (from: Animation, to: Animation, duration: number) {
		if (!from) throw new Error("from cannot be null.");
		if (!to) throw new Error("to cannot be null.");
		const key = `${from.name}.${to.name}`;
		this.animationToMixTime[key] = duration;
	}

	/** Returns the mix duration to use when changing from the specified animation to the other, or the {@link #defaultMix} if
	  * no mix duration has been set. */
	getMix (from: Animation, to: Animation) {
		const key = `${from.name}.${to.name}`;
		const value = this.animationToMixTime[key];
		return value === undefined ? this.defaultMix : value;
	}
}
