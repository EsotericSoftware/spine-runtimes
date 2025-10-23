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

import type { BoneData } from "./BoneData.js";
import type { BonePose } from "./BonePose.js";
import { ConstraintData } from "./ConstraintData.js";
import type { Skeleton } from "./Skeleton.js";
import { TransformConstraint } from "./TransformConstraint.js";
import { TransformConstraintPose } from "./TransformConstraintPose.js";
import { MathUtils } from "./Utils.js";

/** Stores the setup pose for a {@link TransformConstraint}.
 *
 * See [Transform constraints](http://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide. */
export class TransformConstraintData extends ConstraintData<TransformConstraint, TransformConstraintPose> {
	public static readonly ROTATION = 0;
	public static readonly X = 1;
	public static readonly Y = 2;
	public static readonly SCALEX = 3;
	public static readonly SCALEY = 4;
	public static readonly SHEARY = 5;

	/** The bones that will be modified by this transform constraint. */
	bones = [] as BoneData[];

	/** The bone whose world transform will be copied to the constrained bones. */
	public set source (source: BoneData) { this._source = source; }
	public get source () {
		if (!this._source) throw new Error("BoneData not set.")
		else return this._source;
	}
	private _source: BoneData | null = null;

	offsets = [0, 0, 0, 0, 0, 0];

	/** An offset added to the constrained bone X translation. */
	offsetX = 0;

	/** An offset added to the constrained bone Y translation. */
	offsetY = 0;

	/** Reads the source bone's local transform instead of its world transform. */
	localSource = false;

	/** Sets the constrained bones' local transforms instead of their world transforms. */
	localTarget = false;

	/** Adds the source bone transform to the constrained bones instead of setting it absolutely. */
	additive = false;

	/** Prevents constrained bones from exceeding the ranged defined by {@link ToProperty.offset} and {@link ToProperty.max}. */
	clamp = false;

	/** The mapping of transform properties to other transform properties. */
	readonly properties: Array<FromProperty> = [];

	constructor (name: string) {
		super(name, new TransformConstraintPose());
	}

	public create (skeleton: Skeleton) {
		return new TransformConstraint(this, skeleton);
	}

	/** An offset added to the constrained bone rotation. */
	getOffsetRotation () {
		return this.offsets[TransformConstraintData.ROTATION];
	}

	setOffsetRotation (offsetRotation: number) {
		this.offsets[TransformConstraintData.ROTATION] = offsetRotation;
	}

	/** An offset added to the constrained bone X translation. */
	getOffsetX () {
		return this.offsets[TransformConstraintData.X];
	}

	setOffsetX (offsetX: number) {
		this.offsets[TransformConstraintData.X] = offsetX;
	}

	/** An offset added to the constrained bone Y translation. */
	getOffsetY () {
		return this.offsets[TransformConstraintData.Y];
	}

	setOffsetY (offsetY: number) {
		this.offsets[TransformConstraintData.Y] = offsetY;
	}

	/** An offset added to the constrained bone scaleX. */
	getOffsetScaleX () {
		return this.offsets[TransformConstraintData.SCALEX];
	}

	setOffsetScaleX (offsetScaleX: number) {
		this.offsets[TransformConstraintData.SCALEX] = offsetScaleX;
	}

	/** An offset added to the constrained bone scaleY. */
	getOffsetScaleY () {
		return this.offsets[TransformConstraintData.SCALEY];
	}

	setOffsetScaleY (offsetScaleY: number) {
		this.offsets[TransformConstraintData.SCALEY] = offsetScaleY;
	}

	/** An offset added to the constrained bone shearY. */
	getOffsetShearY () {
		return this.offsets[TransformConstraintData.SHEARY];
	}

	setOffsetShearY (offsetShearY: number) {
		this.offsets[TransformConstraintData.SHEARY] = offsetShearY;
	}

}

/** Source property for a {@link TransformConstraint}. */
export abstract class FromProperty {
	/** The value of this property that corresponds to {@link ToProperty#offset}. */
	offset = 0;

	/** Constrained properties. */
	readonly to: Array<ToProperty> = [];

	/** Reads this property from the specified bone. */
	abstract value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number;
}

/** Constrained property for a {@link TransformConstraint}. */
export abstract class ToProperty {
	/** The value of this property that corresponds to {@link FromProperty#offset}. */
	offset = 0;

	/** The maximum value of this property when {@link TransformConstraintData#clamp clamped}. */
	max = 0;

	/** The scale of the {@link FromProperty} value in relation to this property. */
	scale = 0;

	/** Reads the mix for this property from the specified constraint. */
	abstract mix (pose: TransformConstraintPose): number;

	/** Applies the value to this property. */
	abstract apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void;
}

export class FromRotate extends FromProperty {
	value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number {
		if (local) return source.rotation + offsets[TransformConstraintData.ROTATION];
		const sx = skeleton.scaleX, sy = skeleton.scaleY;
		let value = Math.atan2(source.c / sy, source.a / sx) * MathUtils.radDeg
			+ ((source.a * source.d - source.b * source.c) * sx * sy > 0 ? offsets[TransformConstraintData.ROTATION] : -offsets[TransformConstraintData.ROTATION]);
		if (value < 0) value += 360;
		return value;
	}
}

export class ToRotate extends ToProperty {
	mix (pose: TransformConstraintPose): number {
		return pose.mixRotate;
	}

	apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void {
		if (local)
			bone.rotation += (additive ? value : value - bone.rotation) * pose.mixRotate;
		else {
			const sx = skeleton.scaleX, sy = skeleton.scaleY, ix = 1 / sx, iy = 1 / sy;
			const a = bone.a * ix, b = bone.b * ix, c = bone.c * iy, d = bone.d * iy;
			value *= MathUtils.degRad;
			if (!additive) value -= Math.atan2(c, a);
			if (value > MathUtils.PI)
				value -= MathUtils.PI2;
			else if (value < -MathUtils.PI) //
				value += MathUtils.PI2;
			value *= pose.mixRotate;
			const cos = Math.cos(value), sin = Math.sin(value);
			bone.a = (cos * a - sin * c) * sx;
			bone.b = (cos * b - sin * d) * sx;
			bone.c = (sin * a + cos * c) * sy;
			bone.d = (sin * b + cos * d) * sy;
		}
	}
}

export class FromX extends FromProperty {
	value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number {
		return local
			? source.x + offsets[TransformConstraintData.X]
			: (offsets[TransformConstraintData.X] * source.a + offsets[TransformConstraintData.Y] * source.b + source.worldX) / skeleton.scaleX;
	}
}

export class ToX extends ToProperty {
	mix (pose: TransformConstraintPose): number {
		return pose.mixX;
	}

	apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void {
		if (local)
			bone.x += (additive ? value : value - bone.x) * pose.mixX;
		else {
			if (!additive) value -= bone.worldX / skeleton.scaleX;
			bone.worldX += value * pose.mixX * skeleton.scaleX;
		}
	}
}

export class FromY extends FromProperty {
	value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number {
		return local
			? source.y + offsets[TransformConstraintData.Y]
			: (offsets[TransformConstraintData.X] * source.c + offsets[TransformConstraintData.Y] * source.d + source.worldY) / skeleton.scaleY;
	}
}

export class ToY extends ToProperty {
	mix (pose: TransformConstraintPose): number {
		return pose.mixY;
	}

	apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void {
		if (local)
			bone.y += (additive ? value : value - bone.y) * pose.mixY;
		else {
			if (!additive) value -= bone.worldY / skeleton.scaleY;
			bone.worldY += value * pose.mixY * skeleton.scaleY;
		}
	}
}

export class FromScaleX extends FromProperty {
	value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number {
		if (local) return source.scaleX + offsets[TransformConstraintData.SCALEX];
		const a = source.a / skeleton.scaleX, c = source.c / skeleton.scaleY;
		return Math.sqrt(a * a + c * c) + offsets[TransformConstraintData.SCALEX];
	}
}

export class ToScaleX extends ToProperty {
	mix (pose: TransformConstraintPose): number {
		return pose.mixScaleX;
	}

	apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (additive)
				bone.scaleX *= 1 + (value - 1) * pose.mixScaleX;
			else if (bone.scaleX !== 0) //
				bone.scaleX += (value - bone.scaleX) * pose.mixScaleX;
		} else if (additive) {
			const s = 1 + (value - 1) * pose.mixScaleX;
			bone.a *= s;
			bone.c *= s;
		} else {
			let a = bone.a / skeleton.scaleX, c = bone.c / skeleton.scaleY, s = Math.sqrt(a * a + c * c);
			if (s !== 0) {
				s = 1 + (value - s) * pose.mixScaleX / s;
				bone.a *= s;
				bone.c *= s;
			}
		}
	}
}

export class FromScaleY extends FromProperty {
	value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number {
		if (local) return source.scaleY + offsets[TransformConstraintData.SCALEY];
		const b = source.b / skeleton.scaleX, d = source.d / skeleton.scaleY;
		return Math.sqrt(b * b + d * d) + offsets[TransformConstraintData.SCALEY];
	}
}

export class ToScaleY extends ToProperty {
	mix (pose: TransformConstraintPose): number {
		return pose.mixScaleY;
	}

	apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (additive)
				bone.scaleY *= 1 + (value - 1) * pose.mixScaleY;
			else if (bone.scaleY !== 0) //
				bone.scaleY += (value - bone.scaleY) * pose.mixScaleY;
		} else if (additive) {
			const s = 1 + (value - 1) * pose.mixScaleY;
			bone.b *= s;
			bone.d *= s;
		} else {
			let b = bone.b / skeleton.scaleX, d = bone.d / skeleton.scaleY, s = Math.sqrt(b * b + d * d);
			if (s !== 0) {
				s = 1 + (value - s) * pose.mixScaleY / s;
				bone.b *= s;
				bone.d *= s;
			}
		}
	}
}

export class FromShearY extends FromProperty {
	value (skeleton: Skeleton, source: BonePose, local: boolean, offsets: Array<number>): number {
		if (local) return source.shearY + offsets[TransformConstraintData.SHEARY];
		const ix = 1 / skeleton.scaleX, iy = 1 / skeleton.scaleY;
		return (Math.atan2(source.d * iy, source.b * ix) - Math.atan2(source.c * iy, source.a * ix)) * MathUtils.radDeg - 90 + offsets[TransformConstraintData.SHEARY];
	}
}

export class ToShearY extends ToProperty {
	mix (pose: TransformConstraintPose): number {
		return pose.mixShearY;
	}

	apply (skeleton: Skeleton, pose: TransformConstraintPose, bone: BonePose, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (!additive) value -= bone.shearY;
			bone.shearY += value * pose.mixShearY;
		} else {
			const sx = skeleton.scaleX, sy = skeleton.scaleY, b = bone.b / sx, d = bone.d / sy, by = Math.atan2(d, b);
			value = (value + 90) * MathUtils.degRad;
			if (additive)
				value -= MathUtils.PI / 2;
			else {
				value -= by - Math.atan2(bone.c / sx, bone.a / sy);
				if (value > MathUtils.PI)
					value -= MathUtils.PI2;
				else if (value < -MathUtils.PI)
					value += MathUtils.PI2;
			}
			value = by + value * pose.mixShearY;
			const s = Math.sqrt(b * b + d * d);
			bone.b = Math.cos(value) * s * sy;
			bone.d = Math.sin(value) * s * sx;
		}
	}
}
