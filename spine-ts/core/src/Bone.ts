/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.5
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
		appliedRotation = 0;

		a = 0; b = 0; worldX = 0;
		c = 0; d = 0; worldY = 0;
		worldSignX = 0; worldSignY = 0;

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
			this.appliedRotation = rotation;

			let rotationY = rotation + 90 + shearY;
			let la = MathUtils.cosDeg(rotation + shearX) * scaleX, lb = MathUtils.cosDeg(rotationY) * scaleY;
			let lc = MathUtils.sinDeg(rotation + shearX) * scaleX, ld = MathUtils.sinDeg(rotationY) * scaleY;

			let parent = this.parent;
			if (parent == null) { // Root bone.
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
				this.worldX = x;
				this.worldY = y;
				this.worldSignX = MathUtils.signum(scaleX);
				this.worldSignY = MathUtils.signum(scaleY);
				return;
			}

			let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			this.worldX = pa * x + pb * y + parent.worldX;
			this.worldY = pc * x + pd * y + parent.worldY;
			this.worldSignX = parent.worldSignX * MathUtils.signum(scaleX);
			this.worldSignY = parent.worldSignY * MathUtils.signum(scaleY);

			if (this.data.inheritRotation && this.data.inheritScale) {
				this.a = pa * la + pb * lc;
				this.b = pa * lb + pb * ld;
				this.c = pc * la + pd * lc;
				this.d = pc * lb + pd * ld;
			} else {
				if (this.data.inheritRotation) { // No scale inheritance.
					pa = 1;
					pb = 0;
					pc = 0;
					pd = 1;
					do {
						let cos = MathUtils.cosDeg(parent.appliedRotation), sin = MathUtils.sinDeg(parent.appliedRotation);
						let temp = pa * cos + pb * sin;
						pb = pb * cos - pa * sin;
						pa = temp;
						temp = pc * cos + pd * sin;
						pd = pd * cos - pc * sin;
						pc = temp;

						if (!parent.data.inheritRotation) break;
						parent = parent.parent;
					} while (parent != null);
					this.a = pa * la + pb * lc;
					this.b = pa * lb + pb * ld;
					this.c = pc * la + pd * lc;
					this.d = pc * lb + pd * ld;
				} else if (this.data.inheritScale) { // No rotation inheritance.
					pa = 1;
					pb = 0;
					pc = 0;
					pd = 1;
					do {
						let cos = MathUtils.cosDeg(parent.appliedRotation), sin = MathUtils.sinDeg(parent.appliedRotation);
						let psx = parent.scaleX, psy = parent.scaleY;
						let za = cos * psx, zb = sin * psy, zc = sin * psx, zd = cos * psy;
						let temp = pa * za + pb * zc;
						pb = pb * zd - pa * zb;
						pa = temp;
						temp = pc * za + pd * zc;
						pd = pd * zd - pc * zb;
						pc = temp;

						if (psx >= 0) sin = -sin;
						temp = pa * cos + pb * sin;
						pb = pb * cos - pa * sin;
						pa = temp;
						temp = pc * cos + pd * sin;
						pd = pd * cos - pc * sin;
						pc = temp;

						if (!parent.data.inheritScale) break;
						parent = parent.parent;
					} while (parent != null);
					this.a = pa * la + pb * lc;
					this.b = pa * lb + pb * ld;
					this.c = pc * la + pd * lc;
					this.d = pc * lb + pd * ld;
				} else {
					this.a = la;
					this.b = lb;
					this.c = lc;
					this.d = ld;
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
			return Math.sqrt(this.a * this.a + this.b * this.b) * this.worldSignX;
		}

		getWorldScaleY () {
			return Math.sqrt(this.c * this.c + this.d * this.d) * this.worldSignY;
		}

		worldToLocalRotationX () {
			let parent = this.parent;
			if (parent == null) return this.rotation;
			let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, a = this.a, c = this.c;
			return Math.atan2(pa * c - pc * a, pd * a - pb * c) * MathUtils.radDeg;
		}

		worldToLocalRotationY () {
			let parent = this.parent;
			if (parent == null) return this.rotation;
			let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, b = this.b, d = this.d;
			return Math.atan2(pa * d - pc * b, pd * b - pb * d) * MathUtils.radDeg;
		}

		rotateWorld (degrees: number) {
			let a = this.a, b = this.b, c = this.c, d = this.d;
			let cos = MathUtils.cosDeg(degrees), sin = MathUtils.sinDeg(degrees);
			this.a = cos * a - sin * c;
			this.b = cos * b - sin * d;
			this.c = sin * a + cos * c;
			this.d = sin * b + cos * d;
		}

		/** Computes the local transform from the world transform. This can be useful to perform processing on the local transform
		 * after the world transform has been modified directly (eg, by a constraint).
		 * <p>
		 * Some redundant information is lost by the world transform, such as -1,-1 scale versus 180 rotation. The computed local
		 * transform values may differ from the original values but are functionally the same. */
		updateLocalTransform () {
			let parent = this.parent;
			if (parent == null) {
				this.x = this.worldX;
				this.y = this.worldY;
				this.rotation = Math.atan2(this.c, this.a) * MathUtils.radDeg;
				this.scaleX = Math.sqrt(this.a * this.a + this.c * this.c);
				this.scaleY = Math.sqrt(this.b * this.b + this.d * this.d);
				let det = this.a * this.d - this.b * this.c;
				this.shearX = 0;
				this.shearY = Math.atan2(this.a * this.b + this.c * this.d, det) * MathUtils.radDeg;
				return;
			}
			let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			let pid = 1 / (pa * pd - pb * pc);
			let dx = this.worldX - parent.worldX, dy = this.worldY - parent.worldY;
			this.x = (dx * pd * pid - dy * pb * pid);
			this.y = (dy * pa * pid - dx * pc * pid);
			let ia = pid * pd;
			let id = pid * pa;
			let ib = pid * pb;
			let ic = pid * pc;
			let ra = ia * this.a - ib * this.c;
			let rb = ia * this.b - ib * this.d;
			let rc = id * this.c - ic * this.a;
			let rd = id * this.d - ic * this.b;
			this.shearX = 0;
			this.scaleX = Math.sqrt(ra * ra + rc * rc);
			if (this.scaleX > 0.0001) {
				let det = ra * rd - rb * rc;
				this.scaleY = det / this.scaleX;
				this.shearY = Math.atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
				this.rotation = Math.atan2(rc, ra) * MathUtils.radDeg;
			} else {
				this.scaleX = 0;
				this.scaleY = Math.sqrt(rb * rb + rd * rd);
				this.shearY = 0;
				this.rotation = 90 - Math.atan2(rd, rb) * MathUtils.radDeg;
			}
			this.appliedRotation = this.rotation;
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
	}
}
