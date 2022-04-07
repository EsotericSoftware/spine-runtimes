/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

import { Bone } from "./Bone";
import { Skeleton } from "./Skeleton";
import { TransformConstraintData } from "./TransformConstraintData";
import { Updatable } from "./Updatable";
import { Vector2, MathUtils } from "./Utils";


/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the target bone.
 *
 * See [Transform constraints](http://esotericsoftware.com/spine-transform-constraints) in the Spine User Guide. */
export class TransformConstraint implements Updatable {

	/** The transform constraint's setup pose data. */
	data: TransformConstraintData;

	/** The bones that will be modified by this transform constraint. */
	bones: Array<Bone>;

	/** The target bone whose world transform will be copied to the constrained bones. */
	target: Bone;

	mixRotate = 0; mixX = 0; mixY = 0; mixScaleX = 0; mixScaleY = 0; mixShearY = 0;

	temp = new Vector2();
	active = false;

	constructor (data: TransformConstraintData, skeleton: Skeleton) {
		if (!data) throw new Error("data cannot be null.");
		if (!skeleton) throw new Error("skeleton cannot be null.");
		this.data = data;
		this.mixRotate = data.mixRotate;
		this.mixX = data.mixX;
		this.mixY = data.mixY;
		this.mixScaleX = data.mixScaleX;
		this.mixScaleY = data.mixScaleY;
		this.mixShearY = data.mixShearY;
		this.bones = new Array<Bone>();
		for (let i = 0; i < data.bones.length; i++) {
			let bone = skeleton.findBone(data.bones[i].name);
			if (!bone) throw new Error(`Couldn't find bone ${data.bones[i].name}.`);
			this.bones.push(bone);
		}
		let target = skeleton.findBone(data.target.name);
		if (!target) throw new Error(`Couldn't find target bone ${data.target.name}.`);
		this.target = target;
	}

	isActive () {
		return this.active;
	}

	update () {
		if (this.mixRotate == 0 && this.mixX == 0 && this.mixY == 0 && this.mixScaleX == 0 && this.mixScaleX == 0 && this.mixShearY == 0) return;

		if (this.data.local) {
			if (this.data.relative)
				this.applyRelativeLocal();
			else
				this.applyAbsoluteLocal();
		} else {
			if (this.data.relative)
				this.applyRelativeWorld();
			else
				this.applyAbsoluteWorld();
		}
	}

	applyAbsoluteWorld () {
		let mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
		let translate = mixX != 0 || mixY != 0;

		let target = this.target;
		let ta = target.a, tb = target.b, tc = target.c, td = target.d;
		let degRadReflect = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
		let offsetRotation = this.data.offsetRotation * degRadReflect;
		let offsetShearY = this.data.offsetShearY * degRadReflect;

		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];

			if (mixRotate != 0) {
				let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				let r = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) //
					r += MathUtils.PI2;
				r *= mixRotate;
				let cos = Math.cos(r), sin = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (translate) {
				let temp = this.temp;
				target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
				bone.worldX += (temp.x - bone.worldX) * mixX;
				bone.worldY += (temp.y - bone.worldY) * mixY;
			}

			if (mixScaleX != 0) {
				let s = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				if (s != 0) s = (s + (Math.sqrt(ta * ta + tc * tc) - s + this.data.offsetScaleX) * mixScaleX) / s;
				bone.a *= s;
				bone.c *= s;
			}
			if (mixScaleY != 0) {
				let s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				if (s != 0) s = (s + (Math.sqrt(tb * tb + td * td) - s + this.data.offsetScaleY) * mixScaleY) / s;
				bone.b *= s;
				bone.d *= s;
			}

			if (mixShearY > 0) {
				let b = bone.b, d = bone.d;
				let by = Math.atan2(d, b);
				let r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) //
					r += MathUtils.PI2;
				r = by + (r + offsetShearY) * mixShearY;
				let s = Math.sqrt(b * b + d * d);
				bone.b = Math.cos(r) * s;
				bone.d = Math.sin(r) * s;
			}

			bone.updateAppliedTransform();
		}
	}

	applyRelativeWorld () {
		let mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
		let translate = mixX != 0 || mixY != 0;

		let target = this.target;
		let ta = target.a, tb = target.b, tc = target.c, td = target.d;
		let degRadReflect = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
		let offsetRotation = this.data.offsetRotation * degRadReflect, offsetShearY = this.data.offsetShearY * degRadReflect;

		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];

			if (mixRotate != 0) {
				let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				let r = Math.atan2(tc, ta) + offsetRotation;
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) //
					r += MathUtils.PI2;
				r *= mixRotate;
				let cos = Math.cos(r), sin = Math.sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (translate) {
				let temp = this.temp;
				target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
				bone.worldX += temp.x * mixX;
				bone.worldY += temp.y * mixY;
			}

			if (mixScaleX != 0) {
				let s = (Math.sqrt(ta * ta + tc * tc) - 1 + this.data.offsetScaleX) * mixScaleX + 1;
				bone.a *= s;
				bone.c *= s;
			}
			if (mixScaleY != 0) {
				let s = (Math.sqrt(tb * tb + td * td) - 1 + this.data.offsetScaleY) * mixScaleY + 1;
				bone.b *= s;
				bone.d *= s;
			}

			if (mixShearY > 0) {
				let r = Math.atan2(td, tb) - Math.atan2(tc, ta);
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) //
					r += MathUtils.PI2;
				let b = bone.b, d = bone.d;
				r = Math.atan2(d, b) + (r - MathUtils.PI / 2 + offsetShearY) * mixShearY;
				let s = Math.sqrt(b * b + d * d);
				bone.b = Math.cos(r) * s;
				bone.d = Math.sin(r) * s;
			}

			bone.updateAppliedTransform();
		}
	}

	applyAbsoluteLocal () {
		let mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;

		let target = this.target;

		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];

			let rotation = bone.arotation;
			if (mixRotate != 0) {
				let r = target.arotation - rotation + this.data.offsetRotation;
				r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
				rotation += r * mixRotate;
			}

			let x = bone.ax, y = bone.ay;
			x += (target.ax - x + this.data.offsetX) * mixX;
			y += (target.ay - y + this.data.offsetY) * mixY;

			let scaleX = bone.ascaleX, scaleY = bone.ascaleY;
			if (mixScaleX != 0 && scaleX != 0)
				scaleX = (scaleX + (target.ascaleX - scaleX + this.data.offsetScaleX) * mixScaleX) / scaleX;
			if (mixScaleY != 0 && scaleY != 0)
				scaleY = (scaleY + (target.ascaleY - scaleY + this.data.offsetScaleY) * mixScaleY) / scaleY;

			let shearY = bone.ashearY;
			if (mixShearY != 0) {
				let r = target.ashearY - shearY + this.data.offsetShearY;
				r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
				shearY += r * mixShearY;
			}

			bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	applyRelativeLocal () {
		let mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;

		let target = this.target;

		let bones = this.bones;
		for (let i = 0, n = bones.length; i < n; i++) {
			let bone = bones[i];

			let rotation = bone.arotation + (target.arotation + this.data.offsetRotation) * mixRotate;
			let x = bone.ax + (target.ax + this.data.offsetX) * mixX;
			let y = bone.ay + (target.ay + this.data.offsetY) * mixY;
			let scaleX = bone.ascaleX * (((target.ascaleX - 1 + this.data.offsetScaleX) * mixScaleX) + 1);
			let scaleY = bone.ascaleY * (((target.ascaleY - 1 + this.data.offsetScaleY) * mixScaleY) + 1);
			let shearY = bone.ashearY + (target.ashearY + this.data.offsetShearY) * mixShearY;

			bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}
}
