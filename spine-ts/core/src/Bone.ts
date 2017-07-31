/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine {
	export class Bone implements Updatable {
		data: BoneData;
		skeleton: Skeleton;
		parent: Bone;
		children = new Array<Bone>();
		x = 0; y = 0; rotation = 0; scaleX = 0; scaleY = 0; shearX = 0; shearY = 0;
		ax = 0; ay = 0; arotation = 0; ascaleX = 0; ascaleY = 0; ashearX = 0; ashearY = 0;
		appliedValid = false;

		a = 0; b = 0; worldX = 0;
		c = 0; d = 0; worldY = 0;

		sorted = false;

		/** @param parent May be null. */
		constructor (data: BoneData, skeleton: Skeleton, parent: Bone) {
			if (data == null) throw new Error("data cannot be null.");
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			this.setToSetupPose();
		}

		/** Same as {@link #updateWorldTransform()}. This method exists for Bone to implement {@link Updatable}. */
		update () {
			this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
		}

		/** Computes the world transform using the parent bone and this bone's local transform. */
		updateWorldTransform () {
			this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
		}

		/** Computes the world transform using the parent bone and the specified local transform. */
		updateWorldTransformWith (x: number, y: number, rotation: number, scaleX: number, scaleY: number, shearX: number, shearY: number) {
			this.ax = x;
			this.ay = y;
			this.arotation = rotation;
			this.ascaleX = scaleX;
			this.ascaleY = scaleY;
			this.ashearX = shearX;
			this.ashearY = shearY;
			this.appliedValid = true;

			let parent = this.parent;
			if (parent == null) { // Root bone.
				let rotationY = rotation + 90 + shearY;
				let la = MathUtils.cosDeg(rotation + shearX) * scaleX;
				let lb = MathUtils.cosDeg(rotationY) * scaleY;
				let lc = MathUtils.sinDeg(rotation + shearX) * scaleX;
				let ld = MathUtils.sinDeg(rotationY) * scaleY;
				let skeleton = this.skeleton;
				if (skeleton.flipX) {
					x = -x;
					la = -la;
					lb = -lb;
				}
				if (skeleton.flipY) {
					y = -y;
					lc = -lc;
					ld = -ld;
				}
				this.a = la;
				this.b = lb;
				this.c = lc;
				this.d = ld;
				this.worldX = x + skeleton.x;
				this.worldY = y + skeleton.y;
				return;
			}

			let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			this.worldX = pa * x + pb * y + parent.worldX;
			this.worldY = pc * x + pd * y + parent.worldY;

			switch (this.data.transformMode) {
			case TransformMode.Normal: {
				let rotationY = rotation + 90 + shearY;
				let la = MathUtils.cosDeg(rotation + shearX) * scaleX;
				let lb = MathUtils.cosDeg(rotationY) * scaleY;
				let lc = MathUtils.sinDeg(rotation + shearX) * scaleX;
				let ld = MathUtils.sinDeg(rotationY) * scaleY;
				this.a = pa * la + pb * lc;
				this.b = pa * lb + pb * ld;
				this.c = pc * la + pd * lc;
				this.d = pc * lb + pd * ld;
				return;
			}
			case TransformMode.OnlyTranslation: {
				let rotationY = rotation + 90 + shearY;
				this.a = MathUtils.cosDeg(rotation + shearX) * scaleX;
				this.b = MathUtils.cosDeg(rotationY) * scaleY;
				this.c = MathUtils.sinDeg(rotation + shearX) * scaleX;
				this.d = MathUtils.sinDeg(rotationY) * scaleY;
				break;
			}
			case TransformMode.NoRotationOrReflection: {
				let s = pa * pa + pc * pc;
				let prx = 0;
				if (s > 0.0001) {
					s = Math.abs(pa * pd - pb * pc) / s;
					pb = pc * s;
					pd = pa * s;
					prx = Math.atan2(pc, pa) * MathUtils.radDeg;
				} else {
					pa = 0;
					pc = 0;
					prx = 90 - Math.atan2(pd, pb) * MathUtils.radDeg;
				}
				let rx = rotation + shearX - prx;
				let ry = rotation + shearY - prx + 90;
				let la = MathUtils.cosDeg(rx) * scaleX;
				let lb = MathUtils.cosDeg(ry) * scaleY;
				let lc = MathUtils.sinDeg(rx) * scaleX;
				let ld = MathUtils.sinDeg(ry) * scaleY;
				this.a = pa * la - pb * lc;
				this.b = pa * lb - pb * ld;
				this.c = pc * la + pd * lc;
				this.d = pc * lb + pd * ld;
				break;
			}
			case TransformMode.NoScale:
			case TransformMode.NoScaleOrReflection: {
				let cos = MathUtils.cosDeg(rotation);
				let sin = MathUtils.sinDeg(rotation);
				let za = pa * cos + pb * sin;
				let zc = pc * cos + pd * sin;
				let s = Math.sqrt(za * za + zc * zc);
				if (s > 0.00001) s = 1 / s;
				za *= s;
				zc *= s;
				s = Math.sqrt(za * za + zc * zc);
				let r = Math.PI / 2 + Math.atan2(zc, za);
				let zb = Math.cos(r) * s;
				let zd = Math.sin(r) * s;
				let la = MathUtils.cosDeg(shearX) * scaleX;
				let lb = MathUtils.cosDeg(90 + shearY) * scaleY;
				let lc = MathUtils.sinDeg(shearX) * scaleX;
				let ld = MathUtils.sinDeg(90 + shearY) * scaleY;
				if (this.data.transformMode != TransformMode.NoScaleOrReflection ? pa * pd - pb * pc < 0 : this.skeleton.flipX != this.skeleton.flipY) {
					zb = -zb;
					zd = -zd;
				}
				this.a = za * la + zb * lc;
				this.b = za * lb + zb * ld;
				this.c = zc * la + zd * lc;
				this.d = zc * lb + zd * ld;
				return;
			}
			}
			if (this.skeleton.flipX) {
				this.a = -this.a;
				this.b = -this.b;
			}
			if (this.skeleton.flipY) {
				this.c = -this.c;
				this.d = -this.d;
			}
		}

		setToSetupPose () {
			let data = this.data;
			this.x = data.x;
			this.y = data.y;
			this.rotation = data.rotation;
			this.scaleX = data.scaleX;
			this.scaleY = data.scaleY;
			this.shearX = data.shearX;
			this.shearY = data.shearY;
		}

		getWorldRotationX () {
			return Math.atan2(this.c, this.a) * MathUtils.radDeg;
		}

		getWorldRotationY () {
			return Math.atan2(this.d, this.b) * MathUtils.radDeg;
		}

		getWorldScaleX () {
			return Math.sqrt(this.a * this.a + this.c * this.c);
		}

		getWorldScaleY () {
			return Math.sqrt(this.b * this.b + this.d * this.d);
		}

		/** Computes the individual applied transform values from the world transform. This can be useful to perform processing using
	 	 * the applied transform after the world transform has been modified directly (eg, by a constraint).
	 	 * <p>
	 	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. */
		updateAppliedTransform () {
			this.appliedValid = true;
			let parent = this.parent;
			if (parent == null) {
				this.ax = this.worldX;
				this.ay = this.worldY;
				this.arotation = Math.atan2(this.c, this.a) * MathUtils.radDeg;
				this.ascaleX = Math.sqrt(this.a * this.a + this.c * this.c);
				this.ascaleY = Math.sqrt(this.b * this.b + this.d * this.d);
				this.ashearX = 0;
				this.ashearY = Math.atan2(this.a * this.b + this.c * this.d, this.a * this.d - this.b * this.c) * MathUtils.radDeg;
				return;
			}
			let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			let pid = 1 / (pa * pd - pb * pc);
			let dx = this.worldX - parent.worldX, dy = this.worldY - parent.worldY;
			this.ax = (dx * pd * pid - dy * pb * pid);
			this.ay = (dy * pa * pid - dx * pc * pid);
			let ia = pid * pd;
			let id = pid * pa;
			let ib = pid * pb;
			let ic = pid * pc;
			let ra = ia * this.a - ib * this.c;
			let rb = ia * this.b - ib * this.d;
			let rc = id * this.c - ic * this.a;
			let rd = id * this.d - ic * this.b;
			this.ashearX = 0;
			this.ascaleX = Math.sqrt(ra * ra + rc * rc);
			if (this.ascaleX > 0.0001) {
				let det = ra * rd - rb * rc;
				this.ascaleY = det / this.ascaleX;
				this.ashearY = Math.atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
				this.arotation = Math.atan2(rc, ra) * MathUtils.radDeg;
			} else {
				this.ascaleX = 0;
				this.ascaleY = Math.sqrt(rb * rb + rd * rd);
				this.ashearY = 0;
				this.arotation = 90 - Math.atan2(rd, rb) * MathUtils.radDeg;
			}
		}

		worldToLocal (world: Vector2) {
			let a = this.a, b = this.b, c = this.c, d = this.d;
			let invDet = 1 / (a * d - b * c);
			let x = world.x - this.worldX, y = world.y - this.worldY;
			world.x = (x * d * invDet - y * b * invDet);
			world.y = (y * a * invDet - x * c * invDet);
			return world;
		}

		localToWorld (local: Vector2) {
			let x = local.x, y = local.y;
			local.x = x * this.a + y * this.b + this.worldX;
			local.y = x * this.c + y * this.d + this.worldY;
			return local;
		}

		worldToLocalRotation (worldRotation: number) {
			let sin = MathUtils.sinDeg(worldRotation), cos = MathUtils.cosDeg(worldRotation);
			return Math.atan2(this.a * sin - this.c * cos, this.d * cos - this.b * sin) * MathUtils.radDeg;
		}

		localToWorldRotation (localRotation: number) {
			let sin = MathUtils.sinDeg(localRotation), cos = MathUtils.cosDeg(localRotation);
			return Math.atan2(cos * this.c + sin * this.d, cos * this.a + sin * this.b) * MathUtils.radDeg;
		}

		rotateWorld (degrees: number) {
			let a = this.a, b = this.b, c = this.c, d = this.d;
			let cos = MathUtils.cosDeg(degrees), sin = MathUtils.sinDeg(degrees);
			this.a = cos * a - sin * c;
			this.b = cos * b - sin * d;
			this.c = sin * a + cos * c;
			this.d = sin * b + cos * d;
			this.appliedValid = false;
		}
	}
}
