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

import { AnimationState, Physics, Skeleton, SkeletonClipping, Skin } from "@esotericsoftware/spine-core";

interface Rectangle {
	x: number;
	y: number;
	width: number;
	height: number;
}

interface GameObject {
	skeleton?: Skeleton,
	state?: AnimationState,
}

export type SpineBoundsProviderType = "setup" | "animation-skin" | "AABB";

export interface SpineBoundsProvider {
	/** Returns the bounding box for the skeleton, in skeleton space. */
	calculateBounds (gameObject: GameObject): Rectangle;
}

export class AABBRectangleBoundsProvider implements SpineBoundsProvider {
	constructor (
		private x: number,
		private y: number,
		private width: number,
		private height: number,
	) { }
	calculateBounds () {
		return validateBounds({ x: this.x, y: this.y, width: this.width, height: this.height });
	}
}

export class SetupPoseBoundsProvider implements SpineBoundsProvider {
	/**
	 * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
	 */
	constructor (private clipping = false) { }

	calculateBounds (gameObject: GameObject) {
		if (!gameObject.skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		const skeleton = new Skeleton(gameObject.skeleton.data);
		skeleton.setupPose();
		skeleton.updateWorldTransform(Physics.update);
		const bounds = skeleton.getBoundsRect(this.clipping ? new SkeletonClipping() : undefined);
		return validateBounds(bounds);
	}
}

export class SkinsAndAnimationBoundsProvider implements SpineBoundsProvider {
	/**
	 * @param animation The animation to use for calculating the bounds. If null, the setup pose is used.
	 * @param skins The skins to use for calculating the bounds. If empty, the default skin is used.
	 * @param timeStep The time step to use for calculating the bounds. A smaller time step means more precision, but slower calculation.
	 * @param clipping If true, clipping attachments are used to compute the bounds. False, by default.
	 */
	constructor (
		private animation?: string,
		private skins: string[] = [],
		private timeStep: number = 0.05,
		private clipping = false,
	) {
		if (!Number.isFinite(timeStep) || timeStep <= 0)
			throw new Error("timeStep must be a finite number greater than zero.");
	}

	calculateBounds (gameObject: GameObject): {
		x: number;
		y: number;
		width: number;
		height: number;
	} {
		if (!gameObject.skeleton || !gameObject.state)
			return { x: 0, y: 0, width: 0, height: 0 };
		// Make a copy of animation state and skeleton as this might be called while
		// the skeleton in the GameObject has already been heavily modified. We can not
		// reconstruct that state.
		const animationState = new AnimationState(gameObject.state.data);
		const skeleton = new Skeleton(gameObject.skeleton.data);
		const clipper = this.clipping ? new SkeletonClipping() : undefined;
		const data = skeleton.data;
		if (this.skins.length > 0) {
			const customSkin = new Skin("custom-skin");
			for (const skinName of this.skins) {
				const skin = data.findSkin(skinName);
				if (skin === null) continue;
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}
		skeleton.setupPose();

		const animation = this.animation ? data.findAnimation(this.animation) : null;

		if (animation == null) {
			skeleton.updateWorldTransform(Physics.update);
			return validateBounds(skeleton.getBoundsRect(clipper));
		}

		const duration = animation.duration;
		if (!Number.isFinite(duration) || duration < 0)
			return { x: 0, y: 0, width: 0, height: 0 };

		const steps = duration === 0
			? 0
			: Math.min(Math.max(Math.ceil(duration / this.timeStep), 1), 10_000);
		let minX = Number.POSITIVE_INFINITY;
		let minY = Number.POSITIVE_INFINITY;
		let maxX = Number.NEGATIVE_INFINITY;
		let maxY = Number.NEGATIVE_INFINITY;
		let previousTime = 0;

		animationState.clearTracks();
		animationState.setAnimation(0, animation, false);
		for (let i = 0; i <= steps; i++) {
			const time = i === steps ? duration : duration * (i / steps);
			const delta = time - previousTime;
			previousTime = time;
			animationState.update(delta);
			animationState.apply(skeleton);
			skeleton.update(delta);
			skeleton.updateWorldTransform(Physics.update);

			const bounds = skeleton.getBoundsRect(clipper);
			if (!isValidBounds(bounds)) continue;
			minX = Math.min(minX, bounds.x);
			minY = Math.min(minY, bounds.y);
			maxX = Math.max(maxX, bounds.x + bounds.width);
			maxY = Math.max(maxY, bounds.y + bounds.height);
		}

		return validateBounds({ x: minX, y: minY, width: maxX - minX, height: maxY - minY });
	}
}

export function isValidBounds (bounds: Rectangle) {
	return Number.isFinite(bounds.x)
		&& Number.isFinite(bounds.y)
		&& Number.isFinite(bounds.width)
		&& Number.isFinite(bounds.height)
		&& bounds.width > 0
		&& bounds.height > 0;
}

function validateBounds (bounds: Rectangle): Rectangle {
	return isValidBounds(bounds)
		? bounds
		: { x: 0, y: 0, width: 0, height: 0 };
}