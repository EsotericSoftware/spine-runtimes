/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class AnimationStateData {
		skeletonData: SkeletonData;
		animationToMixTime: Map<number> = { };
		defaultMix = 0;

		constructor (skeletonData: SkeletonData) {
			if (skeletonData == null) throw new Error("skeletonData cannot be null.");
			this.skeletonData = skeletonData;
		}

		setMix (fromName: string, toName: string, duration: number) {
			let from = this.skeletonData.findAnimation(fromName);
			if (from == null) throw new Error("Animation not found: " + fromName);
			let to = this.skeletonData.findAnimation(toName);
			if (to == null) throw new Error("Animation not found: " + toName);
			this.setMixWith(from, to, duration);
		}

		setMixWith (from: Animation, to: Animation, duration: number) {
			if (from == null) throw new Error("from cannot be null.");
			if (to == null) throw new Error("to cannot be null.");
			let key = from.name + "." + to.name;
			this.animationToMixTime[key] = duration;
		}

		getMix (from: Animation, to: Animation) {
			let key = from.name + "." + to.name;
			let value = this.animationToMixTime[key];
			return value === undefined ? this.defaultMix : value;
		}
	}
}
