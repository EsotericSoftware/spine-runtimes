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
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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

import type { Skeleton } from "./Skeleton.js";

/** A host runtime object's world position and rotation used for skeleton physics movement inheritance. */
export interface SkeletonPhysicsMovementTransform {
	/** The host object's X position in world coordinates. */
	x: number;

	/** The host object's Y position in world coordinates. */
	y: number;

	/** The host object's Z position in world coordinates. Set to `0` in 2D runtimes. */
	z: number;

	/** The host object's rotation around the skeleton plane normal, in degrees and in the direction expected by Spine physics. */
	rotation: number;
}

/** Initial movement inheritance values for {@link SkeletonPhysicsMovement}. */
export interface SkeletonPhysicsMovementOptions {
	/** Initial horizontal position inheritance. Defaults to `0`. */
	positionInheritanceX?: number;

	/** Initial vertical position inheritance. Defaults to `0`. */
	positionInheritanceY?: number;

	/** Initial rotation inheritance. Defaults to `0`. */
	rotationInheritance?: number;
}

/** Runtime-specific hooks needed by {@link SkeletonPhysicsMovement}. */
export interface SkeletonPhysicsMovementAdapter {
	/**
	 * Writes the current host object world position to `out` and, when `readRotation` is true, its rotation.
	 * Implementations may skip calculating and writing `out.rotation` when `readRotation` is false.
	 * @param out Receives the current host object transform.
	 * @param readRotation Whether the current rotation is needed.
	 */
	readTransform (out: SkeletonPhysicsMovementTransform, readRotation: boolean): void;

	/**
	 * Converts `point` in-place from host world coordinates to skeleton coordinates.
	 * The converted skeleton coordinates are returned in `point.x` and `point.y`.
	 */
	worldToSkeleton (point: { x: number; y: number; z: number }): void;
}

/**
 * Tracks movement of a host runtime object and applies that movement to skeleton physics constraints.
 *
 * This is useful for runtimes where a Spine skeleton is displayed by a movable object or container.
 * When enabled, changes in the host object's position are converted to skeleton coordinates and passed to
 * {@link Skeleton.physicsTranslate}. Changes in its rotation are passed to {@link Skeleton.physicsRotate}.
 *
 * Movement inheritance is opt-in. Position and rotation inheritance default to `0`, so
 * {@link applyTransformMovement} returns immediately until one of the inheritance values is non-zero.
 */
export class SkeletonPhysicsMovement {
	private positionInheritanceFactorX = 0;
	private positionInheritanceFactorY = 0;
	private rotationInheritanceFactor = 0;
	private hasLastTransform = false;
	private lastX = 0;
	private lastY = 0;
	private lastZ = 0;
	private lastRotation = 0;
	private readonly currentTransform: SkeletonPhysicsMovementTransform = { x: 0, y: 0, z: 0, rotation: 0 };
	private readonly currentPosition = { x: 0, y: 0, z: 0 };
	private readonly lastPosition = { x: 0, y: 0, z: 0 };

	/**
	 * Creates a movement tracker for a skeleton displayed by a host runtime object.
	 * @param skeleton The skeleton whose physics constraints receive inherited movement.
	 * @param adapter Runtime-specific hooks used to read and convert the host object's transform.
	 * @param options Initial movement inheritance values.
	 */
	constructor (
		private skeleton: Skeleton,
		private adapter: SkeletonPhysicsMovementAdapter,
		options: SkeletonPhysicsMovementOptions = {},
	) {
		this.positionInheritanceFactorX = options.positionInheritanceX ?? 0;
		this.positionInheritanceFactorY = options.positionInheritanceY ?? 0;
		this.rotationInheritanceFactor = options.rotationInheritance ?? 0;
	}

	/** Horizontal position inheritance factor. `0` disables horizontal position inheritance. */
	get positionInheritanceX (): number {
		return this.positionInheritanceFactorX;
	}

	/** Vertical position inheritance factor. `0` disables vertical position inheritance. */
	get positionInheritanceY (): number {
		return this.positionInheritanceFactorY;
	}

	/**
	 * Sets how much host object translation is inherited by skeleton physics constraints.
	 * Use `(1, 1)` for normal inheritance, or `(0, 0)` to disable position inheritance.
	 * @param x The horizontal position inheritance factor.
	 * @param y The vertical position inheritance factor.
	 */
	setPositionInheritance (x: number, y: number): void {
		const wasDisabled = this.positionInheritanceFactorX === 0 && this.positionInheritanceFactorY === 0;
		const isEnabled = x !== 0 || y !== 0;
		this.positionInheritanceFactorX = x;
		this.positionInheritanceFactorY = y;
		if (wasDisabled && isEnabled) this.resetPosition();
	}

	/** Rotation inheritance factor. `0` disables rotation inheritance. */
	get rotationInheritance (): number {
		return this.rotationInheritanceFactor;
	}

	/**
	 * Sets how much host object rotation is inherited by skeleton physics constraints.
	 * @param value The rotation inheritance factor.
	 */
	set rotationInheritance (value: number) {
		const wasDisabled = this.rotationInheritanceFactor === 0;
		this.rotationInheritanceFactor = value;
		if (wasDisabled && value !== 0) this.resetRotation();
	}

	/** Resets the previous position used to calculate inherited translation. */
	resetPosition (): void {
		const transform = this.currentTransform;
		const readRotation = !this.hasLastTransform && this.rotationInheritanceFactor !== 0;
		this.adapter.readTransform(transform, readRotation);
		this.lastX = transform.x;
		this.lastY = transform.y;
		this.lastZ = transform.z;
		if (readRotation) this.lastRotation = transform.rotation;
		this.hasLastTransform = true;
	}

	/** Resets the previous rotation used to calculate inherited rotation. */
	resetRotation (): void {
		const transform = this.currentTransform;
		this.adapter.readTransform(transform, true);
		this.lastRotation = transform.rotation;
		if (!this.hasLastTransform) {
			this.lastX = transform.x;
			this.lastY = transform.y;
			this.lastZ = transform.z;
		}
		this.hasLastTransform = true;
	}

	/** Resets both previous position and previous rotation. */
	resetTransform (): void {
		const transform = this.currentTransform;
		this.adapter.readTransform(transform, true);
		this.setLastTransform(transform.x, transform.y, transform.z, transform.rotation);
	}

	/**
	 * Applies host object transform movement since the previous call to the skeleton's physics constraints.
	 *
	 * The first call records the current transform as the baseline and does not apply movement.
	 */
	applyTransformMovement (): void {
		const inheritPosition = this.positionInheritanceFactorX !== 0 || this.positionInheritanceFactorY !== 0;
		const inheritRotation = this.rotationInheritanceFactor !== 0;
		if (!inheritPosition && !inheritRotation) return;

		const transform = this.currentTransform;
		this.adapter.readTransform(transform, inheritRotation);
		const { x, y, z } = transform;
		const currentRotation = inheritRotation ? transform.rotation : this.lastRotation;
		const positionChanged = x !== this.lastX || y !== this.lastY || z !== this.lastZ;

		if (this.hasLastTransform) {
			if (!positionChanged && currentRotation === this.lastRotation) return;
			if (inheritPosition && positionChanged) this.applyPositionMovement(x, y, z);
			if (inheritRotation && currentRotation !== this.lastRotation) this.applyRotationMovement(currentRotation);
		}

		this.setLastTransform(x, y, z, currentRotation);
	}

	private applyPositionMovement (currentX: number, currentY: number, currentZ: number): void {
		const currentPosition = this.currentPosition;
		currentPosition.x = currentX;
		currentPosition.y = currentY;
		currentPosition.z = currentZ;
		this.adapter.worldToSkeleton(currentPosition);

		const lastPosition = this.lastPosition;
		lastPosition.x = this.lastX;
		lastPosition.y = this.lastY;
		lastPosition.z = this.lastZ;
		this.adapter.worldToSkeleton(lastPosition);

		this.skeleton.physicsTranslate(
			(currentPosition.x - lastPosition.x) * this.positionInheritanceFactorX,
			(currentPosition.y - lastPosition.y) * this.positionInheritanceFactorY
		);
	}

	private applyRotationMovement (currentRotation: number): void {
		const rotationFactor = this.rotationInheritanceFactor;
		if (rotationFactor === 0) return;
		this.skeleton.physicsRotate(0, 0, this.getRotationDelta(currentRotation, this.lastRotation) * rotationFactor);
	}

	private setLastTransform (x: number, y: number, z: number, rotation: number): void {
		this.lastX = x;
		this.lastY = y;
		this.lastZ = z;
		this.lastRotation = rotation;
		this.hasLastTransform = true;
	}

	private getRotationDelta (current: number, previous: number): number {
		let delta = current - previous;
		delta = (delta + 180) % 360 - 180;
		return delta < -180 ? delta + 360 : delta;
	}
}
