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

import { ConstraintData } from "./ConstraintData.js";
import { BoneData } from "./BoneData.js";
import { Bone } from "./Bone.js";
import { TransformConstraint } from "./TransformConstraint.js";
import { MathUtils } from "./Utils.js";

/** Stores the setup pose for a {@link TransformConstraint}.
 *
 * See [Transform constraints](https://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide. */
export class TransformConstraintData extends ConstraintData {

	/** The bones that will be modified by this transform constraint. */
	bones = new Array<BoneData>();

	/** The bone whose world transform will be copied to the constrained bones. */
	private _source: BoneData | null = null;
	public set source (source: BoneData) { this._source = source; }
	public get source () {
		if (!this._source) throw new Error("BoneData not set.")
		else return this._source;
	}

	/** An offset added to the constrained bone X translation. */
	offsetX = 0;

	/** An offset added to the constrained bone Y translation. */
	offsetY = 0;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotation. */
	mixRotate = 0;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation X. */
	mixX = 0;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y. */
	mixY = 0;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale X. */
	mixScaleX = 0;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale Y. */
	mixScaleY = 0;

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y. */
	mixShearY = 0;

	/** Reads the source bone's local transform instead of its world transform. */
	localSource = false;

	/** Sets the constrained bones' local transforms instead of their world transforms. */
	localTarget = false;

	/** Adds the source bone transform to the constrained bones instead of setting it absolutely. */
	additive = false;

	/** Prevents constrained bones from exceeding the ranged defined by {@link ToProperty#offset} and {@link ToProperty#max}. */
	clamp = false;

	/** The mapping of transform properties to other transform properties. */
	readonly properties: Array<FromProperty> = [];

	constructor (name: string) {
		super(name, 0, false);
	}

}

/** Source property for a {@link TransformConstraint}. */
export abstract class FromProperty {
	/** The value of this property that corresponds to {@link ToProperty#offset}. */
	offset = 0;

	/** Constrained properties. */
	readonly to: Array<ToProperty> = [];

	/** Reads this property from the specified bone. */
	abstract value (data: TransformConstraintData, source: Bone, local: boolean): number;
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
	abstract mix (constraint: TransformConstraint): number;

	/** Applies the value to this property. */
	abstract apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void;
}

export class FromRotate extends FromProperty {
	value (data: TransformConstraintData, source: Bone, local: boolean): number {
		return local ? source.arotation : Math.atan2(source.c, source.a) * MathUtils.radDeg;
	}
}

export class ToRotate extends ToProperty {
	mix (constraint: TransformConstraint): number {
		return constraint.mixRotate;
	}

	apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (!additive) value -= bone.arotation;
			bone.arotation += value * constraint.mixRotate;
		} else {
			const a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			value *= MathUtils.degRad;
			if (!additive) value -= Math.atan2(c, a);
			if (value > MathUtils.PI)
				value -= MathUtils.PI2;
			else if (value < -MathUtils.PI) //
				value += MathUtils.PI2;
			value *= constraint.mixRotate;
			const cos = Math.cos(value), sin = Math.sin(value);
			bone.a = cos * a - sin * c;
			bone.b = cos * b - sin * d;
			bone.c = sin * a + cos * c;
			bone.d = sin * b + cos * d;
		}
	}
}

export class FromX extends FromProperty {
	value (data: TransformConstraintData, source: Bone, local: boolean): number {
		return local ? source.ax + data.offsetX : data.offsetX * source.a + data.offsetY * source.b + source.worldX;
	}
}

export class ToX extends ToProperty {
	mix (constraint: TransformConstraint): number {
		return constraint.mixX;
	}

	apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (!additive) value -= bone.ax;
			bone.ax += value * constraint.mixX;
		} else {
			if (!additive) value -= bone.worldX;
			bone.worldX += value * constraint.mixX;
		}
	}
}

export class FromY extends FromProperty {
	value (data: TransformConstraintData, source: Bone, local: boolean): number {
		return local ? source.ay + data.offsetY : data.offsetX * source.c + data.offsetY * source.d + source.worldY;
	}
}

export class ToY extends ToProperty {
	mix (constraint: TransformConstraint): number {
		return constraint.mixY;
	}

	apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (!additive) value -= bone.ay;
			bone.ay += value * constraint.mixY;
		} else {
			if (!additive) value -= bone.worldY;
			bone.worldY += value * constraint.mixY;
		}
	}
}

export class FromScaleX extends FromProperty {
	value (data: TransformConstraintData, source: Bone, local: boolean): number {
		return local ? source.ascaleX : Math.sqrt(source.a * source.a + source.c * source.c);
	}
}

export class ToScaleX extends ToProperty {
	mix (constraint: TransformConstraint): number {
		return constraint.mixScaleX;
	}

	apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (additive)
				bone.ascaleX *= 1 + ((value - 1) * constraint.mixScaleX);
			else if (bone.ascaleX != 0)
				bone.ascaleX = 1 + (value / bone.ascaleX - 1) * constraint.mixScaleX;
		} else {
			let s: number;
			if (additive)
				s = 1 + (value - 1) * constraint.mixScaleX;
			else {
				s = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				if (s != 0) s = 1 + (value / s - 1) * constraint.mixScaleX;
			}
			bone.a *= s;
			bone.c *= s;
		}
	}
}

export class FromScaleY extends FromProperty {
	value (data: TransformConstraintData, source: Bone, local: boolean): number {
		return local ? source.ascaleY : Math.sqrt(source.b * source.b + source.d * source.d);
	}
}

export class ToScaleY extends ToProperty {
	mix (constraint: TransformConstraint): number {
		return constraint.mixScaleY;
	}

	apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (additive)
				bone.ascaleY *= 1 + ((value - 1) * constraint.mixScaleY);
			else if (bone.ascaleY != 0) //
				bone.ascaleY = 1 + (value / bone.ascaleY - 1) * constraint.mixScaleY;
		} else {
			let s: number;
			if (additive)
				s = 1 + (value - 1) * constraint.mixScaleY;
			else {
				s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				if (s != 0) s = 1 + (value / s - 1) * constraint.mixScaleY;
			}
			bone.b *= s;
			bone.d *= s;
		}
	}
}

export class FromShearY extends FromProperty {
	value (data: TransformConstraintData, source: Bone, local: boolean): number {
		return local ? source.ashearY : (Math.atan2(source.d, source.b) - Math.atan2(source.c, source.a)) * MathUtils.radDeg - 90;
	}
}

export class ToShearY extends ToProperty {
	mix (constraint: TransformConstraint): number {
		return constraint.mixShearY;
	}

	apply (constraint: TransformConstraint, bone: Bone, value: number, local: boolean, additive: boolean): void {
		if (local) {
			if (!additive) value -= bone.ashearY;
			bone.ashearY += value * constraint.mixShearY;
		} else {
			const b = bone.b, d = bone.d, by = Math.atan2(d, b);
			value = (value + 90) * MathUtils.degRad;
			if (additive)
				value -= MathUtils.PI / 2;
			else {
				value -= by - Math.atan2(bone.c, bone.a);
				if (value > MathUtils.PI)
					value -= MathUtils.PI2;
				else if (value < -MathUtils.PI)
					value += MathUtils.PI2;
			}
			value = by + value * constraint.mixShearY;
			const s = Math.sqrt(b * b + d * d);
			bone.b = Math.cos(value) * s;
			bone.d = Math.sin(value) * s;
		}
	}
}
