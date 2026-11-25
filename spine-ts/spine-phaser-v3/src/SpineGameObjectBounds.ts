import {
	AnimationState,
	AnimationStateData,
	Physics,
	Skeleton,
	SkeletonClipping,
	Skin,
	type Skeleton as SpineSkeleton,
} from "@esotericsoftware/spine-core";

/** A bounds provider calculates the bounding box for a skeleton.
 *
 * @remarks Returned bounds are in skeleton-local coordinates and will be used as
 * the Spine GameObject bounds.
 */
export interface SpineGameObjectBoundsProvider {
	/**
	 * Calculates bounds in skeleton-local coordinates.
	 * @param gameObject Object containing the skeleton to measure.
	 * @returns The calculated bounds rectangle.
	 */
	calculateBounds (gameObject: { skeleton?: SpineSkeleton }): { x: number; y: number; width: number; height: number };
}

/** A bounds provider that provides a fixed rectangle size. */
export class AABBRectangleBoundsProvider implements SpineGameObjectBoundsProvider {
	/**
	 * @param x The bounds x-coordinate in skeleton-local coordinates.
	 * @param y The bounds y-coordinate in skeleton-local coordinates.
	 * @param width The bounds width.
	 * @param height The bounds height.
	 */
	constructor (private x: number, private y: number, private width: number, private height: number) { }
	/** @returns The fixed bounds rectangle. */
	calculateBounds () { return { x: this.x, y: this.y, width: this.width, height: this.height }; }
}

/** A bounds provider that calculates the bounds from the setup pose. */
export class SetupPoseBoundsProvider implements SpineGameObjectBoundsProvider {
	/** @param clipping If true, clipping attachments are used while calculating bounds. */
	constructor (private clipping = false) { }
	/**
	 * Calculates bounds from the skeleton setup pose.
	 * @param gameObject Object containing the skeleton to measure.
	 * @returns The calculated bounds rectangle.
	 */
	calculateBounds (gameObject: { skeleton?: SpineSkeleton }) {
		if (!gameObject.skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		const skeleton = new Skeleton(gameObject.skeleton.data);
		skeleton.setupPose();
		skeleton.updateWorldTransform(Physics.update);
		const bounds = skeleton.getBoundsRect(this.clipping ? new SkeletonClipping() : undefined);
		return bounds.width === Number.NEGATIVE_INFINITY ? { x: 0, y: 0, width: 0, height: 0 } : bounds;
	}
}

/** A bounds provider that calculates bounds over the supplied skins and animation. */
export class SkinsAndAnimationBoundsProvider implements SpineGameObjectBoundsProvider {
	/**
	 * @param animation Animation name to sample, or `null` to use the setup pose.
	 * @param skins Skin names to combine before measuring. Empty uses the skeleton's default skin.
	 * @param timeStep Sampling time step in seconds.
	 * @param clipping If true, clipping attachments are used while calculating bounds.
	 */
	constructor (private animation: string | null, private skins: string[] = [], private timeStep = 0.05, private clipping = false) { }
	/**
	 * Calculates bounds by sampling the configured skins and animation.
	 * @param gameObject Object containing the skeleton to measure.
	 * @returns The calculated bounds rectangle.
	 */
	calculateBounds (gameObject: { skeleton?: SpineSkeleton }) {
		if (!gameObject.skeleton) return { x: 0, y: 0, width: 0, height: 0 };
		const animationState = new AnimationState(new AnimationStateData(gameObject.skeleton.data));
		const skeleton = new Skeleton(gameObject.skeleton.data);
		const clipper = this.clipping ? new SkeletonClipping() : undefined;
		const data = skeleton.data;
		if (this.skins.length > 0) {
			const customSkin = new Skin("custom-skin");
			for (const skinName of this.skins) {
				const skin = data.findSkin(skinName);
				if (skin) customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}
		skeleton.setupPose();
		const animation = this.animation != null ? data.findAnimation(this.animation) : null;
		if (!animation) {
			skeleton.updateWorldTransform(Physics.update);
			const bounds = skeleton.getBoundsRect(clipper);
			return bounds.width === Number.NEGATIVE_INFINITY ? { x: 0, y: 0, width: 0, height: 0 } : bounds;
		}
		let minX = Number.POSITIVE_INFINITY, minY = Number.POSITIVE_INFINITY;
		let maxX = Number.NEGATIVE_INFINITY, maxY = Number.NEGATIVE_INFINITY;
		animationState.setAnimation(0, animation, false);
		for (let i = 0, n = Math.max(animation.duration / this.timeStep, 1); i < n; i++) {
			animationState.update(i > 0 ? this.timeStep : 0);
			animationState.apply(skeleton);
			skeleton.update(i > 0 ? this.timeStep : 0);
			skeleton.updateWorldTransform(Physics.update);
			const bounds = skeleton.getBoundsRect(clipper);
			minX = Math.min(minX, bounds.x); minY = Math.min(minY, bounds.y);
			maxX = Math.max(maxX, bounds.x + bounds.width); maxY = Math.max(maxY, bounds.y + bounds.height);
		}
		return { x: minX, y: minY, width: maxX - minX, height: maxY - minY };
	}
}
