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

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
		rotateMix = 0;

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained translations. */
		translateMix = 0;

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained scales. */
		scaleMix = 0;

		/** A percentage (0-1) that controls the mix between the constrained and unconstrained scales. */
		shearMix = 0;

		temp = new Vector2();
		active = false;

		constructor (data: TransformConstraintData, skeleton: Skeleton) {
			if (data == null) throw new Error("data cannot be null.");
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			this.data = data;
			this.rotateMix = data.rotateMix;
			this.translateMix = data.translateMix;
			this.scaleMix = data.scaleMix;
			this.shearMix = data.shearMix;
			this.bones = new Array<Bone>();
			for (let i = 0; i < data.bones.length; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findBone(data.target.name);
		}

		isActive () {
			return this.active;
		}

		/** Applies the constraint to the constrained bones. */
		apply () {
			this.update();
		}

		update () {
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
			let rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			let target = this.target;
			let ta = target.a, tb = target.b, tc = target.c, td = target.d;
			let degRadReflect = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
			let offsetRotation = this.data.offsetRotation * degRadReflect;
			let offsetShearY = this.data.offsetShearY * degRadReflect;
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				let modified = false;

				if (rotateMix != 0) {
					let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					let r = Math.atan2(tc, ta) - Math.atan2(c, a) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI)
						r += MathUtils.PI2;
					r *= rotateMix;
					let cos = Math.cos(r), sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
					modified = true;
				}

				if (translateMix != 0) {
					let temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += (temp.x - bone.worldX) * translateMix;
					bone.worldY += (temp.y - bone.worldY) * translateMix;
					modified = true;
				}

				if (scaleMix > 0) {
					let s = Math.sqrt(bone.a * bone.a + bone.c * bone.c);
					let ts = Math.sqrt(ta * ta + tc * tc);
					if (s > 0.00001) s = (s + (ts - s + this.data.offsetScaleX) * scaleMix) / s;
					bone.a *= s;
					bone.c *= s;
					s = Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					ts = Math.sqrt(tb * tb + td * td);
					if (s > 0.00001) s = (s + (ts - s + this.data.offsetScaleY) * scaleMix) / s;
					bone.b *= s;
					bone.d *= s;
					modified = true;
				}

				if (shearMix > 0) {
					let b = bone.b, d = bone.d;
					let by = Math.atan2(d, b);
					let r = Math.atan2(td, tb) - Math.atan2(tc, ta) - (by - Math.atan2(bone.c, bone.a));
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI)
						r += MathUtils.PI2;
					r = by + (r + offsetShearY) * shearMix;
					let s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
					modified = true;
				}

				if (modified) bone.appliedValid = false;
			}
		}

		applyRelativeWorld () {
			let rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			let target = this.target;
			let ta = target.a, tb = target.b, tc = target.c, td = target.d;
			let degRadReflect = ta * td - tb * tc > 0 ? MathUtils.degRad : -MathUtils.degRad;
			let offsetRotation = this.data.offsetRotation * degRadReflect, offsetShearY = this.data.offsetShearY * degRadReflect;
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				let modified = false;

				if (rotateMix != 0) {
					let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					let r = Math.atan2(tc, ta) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) r += MathUtils.PI2;
					r *= rotateMix;
					let cos = Math.cos(r), sin = Math.sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
					modified = true;
				}

				if (translateMix != 0) {
					let temp = this.temp;
					target.localToWorld(temp.set(this.data.offsetX, this.data.offsetY));
					bone.worldX += temp.x * translateMix;
					bone.worldY += temp.y * translateMix;
					modified = true;
				}

				if (scaleMix > 0) {
					let s = (Math.sqrt(ta * ta + tc * tc) - 1 + this.data.offsetScaleX) * scaleMix + 1;
					bone.a *= s;
					bone.c *= s;
					s = (Math.sqrt(tb * tb + td * td) - 1 + this.data.offsetScaleY) * scaleMix + 1;
					bone.b *= s;
					bone.d *= s;
					modified = true;
				}

				if (shearMix > 0) {
					let r = Math.atan2(td, tb) - Math.atan2(tc, ta);
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) r += MathUtils.PI2;
					let b = bone.b, d = bone.d;
					r = Math.atan2(d, b) + (r - MathUtils.PI / 2 + offsetShearY) * shearMix;
					let s = Math.sqrt(b * b + d * d);
					bone.b = Math.cos(r) * s;
					bone.d = Math.sin(r) * s;
					modified = true;
				}

				if (modified) bone.appliedValid = false;
			}
		}

		applyAbsoluteLocal () {
			let rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			let target = this.target;
			if (!target.appliedValid) target.updateAppliedTransform();
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				if (!bone.appliedValid) bone.updateAppliedTransform();

				let rotation = bone.arotation;
				if (rotateMix != 0) {
					let r = target.arotation - rotation + this.data.offsetRotation;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					rotation += r * rotateMix;
				}

				let x = bone.ax, y = bone.ay;
				if (translateMix != 0) {
					x += (target.ax - x + this.data.offsetX) * translateMix;
					y += (target.ay - y + this.data.offsetY) * translateMix;
				}

				let scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (scaleMix != 0) {
					if (scaleX > 0.00001) scaleX = (scaleX + (target.ascaleX - scaleX + this.data.offsetScaleX) * scaleMix) / scaleX;
					if (scaleY > 0.00001) scaleY = (scaleY + (target.ascaleY - scaleY + this.data.offsetScaleY) * scaleMix) / scaleY;
				}

				let shearY = bone.ashearY;
				if (shearMix != 0) {
					let r = target.ashearY - shearY + this.data.offsetShearY;
					r -= (16384 - ((16384.499999999996 - r / 360) | 0)) * 360;
					bone.shearY += r * shearMix;
				}

				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		applyRelativeLocal () {
			let rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			let target = this.target;
			if (!target.appliedValid) target.updateAppliedTransform();
			let bones = this.bones;
			for (let i = 0, n = bones.length; i < n; i++) {
				let bone = bones[i];
				if (!bone.appliedValid) bone.updateAppliedTransform();

				let rotation = bone.arotation;
				if (rotateMix != 0) rotation += (target.arotation + this.data.offsetRotation) * rotateMix;

				let x = bone.ax, y = bone.ay;
				if (translateMix != 0) {
					x += (target.ax + this.data.offsetX) * translateMix;
					y += (target.ay + this.data.offsetY) * translateMix;
				}

				let scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (scaleMix != 0) {
					if (scaleX > 0.00001) scaleX *= ((target.ascaleX - 1 + this.data.offsetScaleX) * scaleMix) + 1;
					if (scaleY > 0.00001) scaleY *= ((target.ascaleY - 1 + this.data.offsetScaleY) * scaleMix) + 1;
				}

				let shearY = bone.ashearY;
				if (shearMix != 0) shearY += (target.ashearY + this.data.offsetShearY) * shearMix;

				bone.updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}
	}
}
