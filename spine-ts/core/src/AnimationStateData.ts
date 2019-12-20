/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

module spine {

	/** Stores mix (crossfade) durations to be applied when {@link AnimationState} animations are changed. */
	export class AnimationStateData {
		/** The SkeletonData to look up animations when they are specified by name. */
		skeletonData: SkeletonData;

		animationToMixTime: Map<number> = { };

		/** The mix duration to use when no mix duration has been defined between two animations. */
		defaultMix = 0;

		constructor (skeletonData: SkeletonData) {
			if (skeletonData == null) throw new Error("skeletonData cannot be null.");
			this.skeletonData = skeletonData;
		}

		/** Sets a mix duration by animation name.
		 *
		 * See {@link #setMixWith()}. */
		setMix (fromName: string, toName: string, duration: number) {
			let from = this.skeletonData.findAnimation(fromName);
			if (from == null) throw new Error("Animation not found: " + fromName);
			let to = this.skeletonData.findAnimation(toName);
			if (to == null) throw new Error("Animation not found: " + toName);
			this.setMixWith(from, to, duration);
		}

		/** Sets the mix duration when changing from the specified animation to the other.
		 *
		 * See {@link TrackEntry#mixDuration}. */
		setMixWith (from: Animation, to: Animation, duration: number) {
			if (from == null) throw new Error("from cannot be null.");
			if (to == null) throw new Error("to cannot be null.");
			let key = from.name + "." + to.name;
			this.animationToMixTime[key] = duration;
		}

		/** Returns the mix duration to use when changing from the specified animation to the other, or the {@link #defaultMix} if
	 	* no mix duration has been set. */
		getMix (from: Animation, to: Animation) {
			let key = from.name + "." + to.name;
			let value = this.animationToMixTime[key];
			return value === undefined ? this.defaultMix : value;
		}
	}
}
