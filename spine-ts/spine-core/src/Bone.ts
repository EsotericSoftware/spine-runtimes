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

import { BoneData, TransformMode } from "./BoneData";
import { Skeleton } from "./Skeleton";
import { Updatable } from "./Updatable";
import { MathUtils, Vector2 } from "./Utils";

/** Stores a bone's current pose.
 *
 * A bone has a local transform which is used to compute its world transform. A bone also has an applied transform, which is a
 * local transform that can be applied to compute the world transform. The local transform and applied transform may differ if a
 * constraint or application code modifies the world transform after it was computed from the local transform. */
export class Bone implements Updatable {
	/** The bone's setup pose data. */
	data: BoneData;

	/** The skeleton this bone belongs to. */
	skeleton: Skeleton;

	/** The parent bone, or null if this is the root bone. */
	parent: Bone | null = null;

	/** The immediate children of this bone. */
	children = new Array<Bone>();

	/** The local x translation. */
	x = 0;

	/** The local y translation. */
	y = 0;

	/** The local rotation in degrees, counter clockwise. */
	rotation = 0;

	/** The local scaleX. */
	scaleX = 0;

	/** The local scaleY. */
	scaleY = 0;

	/** The local shearX. */
	shearX = 0;

	/** The local shearY. */
	shearY = 0;

	/** The applied local x translation. */
	ax = 0;

	/** The applied local y translation. */
	ay = 0;

	/** The applied local rotation in degrees, counter clockwise. */
	arotation = 0;

	/** The applied local scaleX. */
	ascaleX = 0;

	/** The applied local scaleY. */
	ascaleY = 0;

	/** The applied local shearX. */
	ashearX = 0;

	/** The applied local shearY. */
	ashearY = 0;

	/** Part of the world transform matrix for the X axis. If changed, {@link #updateAppliedTransform()} should be called. */
	a = 0;

	/** Part of the world transform matrix for the Y axis. If changed, {@link #updateAppliedTransform()} should be called. */
	b = 0;

	/** Part of the world transform matrix for the X axis. If changed, {@link #updateAppliedTransform()} should be called. */
	c = 0;

	/** Part of the world transform matrix for the Y axis. If changed, {@link #updateAppliedTransform()} should be called. */
	d = 0;

	/** The world X position. If changed, {@link #updateAppliedTransform()} should be called. */
	worldY = 0;

	/** The world Y position. If changed, {@link #updateAppliedTransform()} should be called. */
	worldX = 0;

	sorted = false;
	active = false;

	/** @param parent May be null. */
	constructor (data: BoneData, skeleton: Skeleton, parent: Bone | null) {
		if (!data) throw new Error("data cannot be null.");
		if (!skeleton) throw new Error("skeleton cannot be null.");
		this.data = data;
		this.skeleton = skeleton;
		this.parent = parent;
		this.setToSetupPose();
	}

	/** Returns false when the bone has not been computed because {@link BoneData#skinRequired} is true and the
	  * {@link Skeleton#skin active skin} does not {@link Skin#bones contain} this bone. */
	isActive () {
		return this.active;
	}

	/** Computes the world transform using the parent bone and this bone's local applied transform. */
	update () {
		this.updateWorldTransformWith(this.ax, this.ay, this.arotation, this.ascaleX, this.ascaleY, this.ashearX, this.ashearY);
	}

	/** Computes the world transform using the parent bone and this bone's local transform.
	 *
	 * See {@link #updateWorldTransformWith()}. */
	updateWorldTransform () {
		this.updateWorldTransformWith(this.x, this.y, this.rotation, this.scaleX, this.scaleY, this.shearX, this.shearY);
	}

	/** Computes the world transform using the parent bone and the specified local transform. The applied transform is set to the
	 * specified local transform. Child bones are not updated.
	 *
	 * See [World transforms](http://esotericsoftware.com/spine-runtime-skeletons#World-transforms) in the Spine
	 * Runtimes Guide. */
	updateWorldTransformWith (x: number, y: number, rotation: number, scaleX: number, scaleY: number, shearX: number, shearY: number) {
		this.ax = x;
		this.ay = y;
		this.arotation = rotation;
		this.ascaleX = scaleX;
		this.ascaleY = scaleY;
		this.ashearX = shearX;
		this.ashearY = shearY;

		let parent = this.parent;
		if (!parent) { // Root bone.
			let skeleton = this.skeleton;
			let rotationY = rotation + 90 + shearY;
			let sx = skeleton.scaleX;
			let sy = skeleton.scaleY;
			this.a = MathUtils.cosDeg(rotation + shearX) * scaleX * sx;
			this.b = MathUtils.cosDeg(rotationY) * scaleY * sx;
			this.c = MathUtils.sinDeg(rotation + shearX) * scaleX * sy;
			this.d = MathUtils.sinDeg(rotationY) * scaleY * sy;
			this.worldX = x * sx + skeleton.x;
			this.worldY = y * sy + skeleton.y;
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
					pa /= this.skeleton.scaleX;
					pc /= this.skeleton.scaleY;
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
				let za = (pa * cos + pb * sin) / this.skeleton.scaleX;
				let zc = (pc * cos + pd * sin) / this.skeleton.scaleY;
				let s = Math.sqrt(za * za + zc * zc);
				if (s > 0.00001) s = 1 / s;
				za *= s;
				zc *= s;
				s = Math.sqrt(za * za + zc * zc);
				if (this.data.transformMode == TransformMode.NoScale
					&& (pa * pd - pb * pc < 0) != (this.skeleton.scaleX < 0 != this.skeleton.scaleY < 0)) s = -s;
				let r = Math.PI / 2 + Math.atan2(zc, za);
				let zb = Math.cos(r) * s;
				let zd = Math.sin(r) * s;
				let la = MathUtils.cosDeg(shearX) * scaleX;
				let lb = MathUtils.cosDeg(90 + shearY) * scaleY;
				let lc = MathUtils.sinDeg(shearX) * scaleX;
				let ld = MathUtils.sinDeg(90 + shearY) * scaleY;
				this.a = za * la + zb * lc;
				this.b = za * lb + zb * ld;
				this.c = zc * la + zd * lc;
				this.d = zc * lb + zd * ld;
				break;
			}
		}
		this.a *= this.skeleton.scaleX;
		this.b *= this.skeleton.scaleX;
		this.c *= this.skeleton.scaleY;
		this.d *= this.skeleton.scaleY;
	}

	/** Sets this bone's local transform to the setup pose. */
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

	/** The world rotation for the X axis, calculated using {@link #a} and {@link #c}. */
	getWorldRotationX () {
		return Math.atan2(this.c, this.a) * MathUtils.radDeg;
	}

	/** The world rotation for the Y axis, calculated using {@link #b} and {@link #d}. */
	getWorldRotationY () {
		return Math.atan2(this.d, this.b) * MathUtils.radDeg;
	}

	/** The magnitude (always positive) of the world scale X, calculated using {@link #a} and {@link #c}. */
	getWorldScaleX () {
		return Math.sqrt(this.a * this.a + this.c * this.c);
	}

	/** The magnitude (always positive) of the world scale Y, calculated using {@link #b} and {@link #d}. */
	getWorldScaleY () {
		return Math.sqrt(this.b * this.b + this.d * this.d);
	}

	/** Computes the applied transform values from the world transform.
	 *
	 * If the world transform is modified (by a constraint, {@link #rotateWorld(float)}, etc) then this method should be called so
	 * the applied transform matches the world transform. The applied transform may be needed by other code (eg to apply other
	 * constraints).
	 *
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The applied transform after
	 * calling this method is equivalent to the local transform used to compute the world transform, but may not be identical. */
	updateAppliedTransform () {
		let parent = this.parent;
		if (!parent) {
			this.ax = this.worldX - this.skeleton.x;
			this.ay = this.worldY - this.skeleton.y;
			this.arotation = Math.atan2(this.c, this.a) * MathUtils.radDeg;
			this.ascaleX = Math.sqrt(this.a * this.a + this.c * this.c);
			this.ascaleY = Math.sqrt(this.b * this.b + this.d * this.d);
			this.ashearX = 0;
			this.ashearY = Math.atan2(this.a * this.b + this.c * this.d, this.a * this.d - this.b * this.c) * MathUtils.radDeg;
			return;
		}
		let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		let pid = 1 / (pa * pd - pb * pc);
		let ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
		let dx = this.worldX - parent.worldX, dy = this.worldY - parent.worldY;
		this.ax = (dx * ia - dy * ib);
		this.ay = (dy * id - dx * ic);

		let ra, rb, rc, rd;
		if (this.data.transformMode == TransformMode.OnlyTranslation) {
			ra = this.a;
			rb = this.b;
			rc = this.c;
			rd = this.d;
		} else {
			switch (this.data.transformMode) {
				case TransformMode.NoRotationOrReflection: {
					let s = Math.abs(pa * pd - pb * pc) / (pa * pa + pc * pc);
					let sa = pa / this.skeleton.scaleX;
					let sc = pc / this.skeleton.scaleY;
					pb = -sc * s * this.skeleton.scaleX;
					pd = sa * s * this.skeleton.scaleY;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
					break;
				}
				case TransformMode.NoScale:
				case TransformMode.NoScaleOrReflection:
					let cos = MathUtils.cosDeg(this.rotation), sin = MathUtils.sinDeg(this.rotation);
					pa = (pa * cos + pb * sin) / this.skeleton.scaleX;
					pc = (pc * cos + pd * sin) / this.skeleton.scaleY;
					let s = Math.sqrt(pa * pa + pc * pc);
					if (s > 0.00001) s = 1 / s;
					pa *= s;
					pc *= s;
					s = Math.sqrt(pa * pa + pc * pc);
					if (this.data.transformMode == TransformMode.NoScale && pid < 0 != (this.skeleton.scaleX < 0 != this.skeleton.scaleY < 0)) s = -s;
					let r = MathUtils.PI / 2 + Math.atan2(pc, pa);
					pb = Math.cos(r) * s;
					pd = Math.sin(r) * s;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
					ic = pc * pid;
					id = pa * pid;
			}
			ra = ia * this.a - ib * this.c;
			rb = ia * this.b - ib * this.d;
			rc = id * this.c - ic * this.a;
			rd = id * this.d - ic * this.b;
		}

		this.ashearX = 0;
		this.ascaleX = Math.sqrt(ra * ra + rc * rc);
		if (this.ascaleX > 0.0001) {
			let det = ra * rd - rb * rc;
			this.ascaleY = det / this.ascaleX;
			this.ashearY = -Math.atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
			this.arotation = Math.atan2(rc, ra) * MathUtils.radDeg;
		} else {
			this.ascaleX = 0;
			this.ascaleY = Math.sqrt(rb * rb + rd * rd);
			this.ashearY = 0;
			this.arotation = 90 - Math.atan2(rd, rb) * MathUtils.radDeg;
		}
	}

	/** Transforms a point from world coordinates to the bone's local coordinates. */
	worldToLocal (world: Vector2) {
		let invDet = 1 / (this.a * this.d - this.b * this.c);
		let x = world.x - this.worldX, y = world.y - this.worldY;
		world.x = x * this.d * invDet - y * this.b * invDet;
		world.y = y * this.a * invDet - x * this.c * invDet;
		return world;
	}

	/** Transforms a point from the bone's local coordinates to world coordinates. */
	localToWorld (local: Vector2) {
		let x = local.x, y = local.y;
		local.x = x * this.a + y * this.b + this.worldX;
		local.y = x * this.c + y * this.d + this.worldY;
		return local;
	}

	/** Transforms a world rotation to a local rotation. */
	worldToLocalRotation (worldRotation: number) {
		let sin = MathUtils.sinDeg(worldRotation), cos = MathUtils.cosDeg(worldRotation);
		return Math.atan2(this.a * sin - this.c * cos, this.d * cos - this.b * sin) * MathUtils.radDeg + this.rotation - this.shearX;
	}

	/** Transforms a local rotation to a world rotation. */
	localToWorldRotation (localRotation: number) {
		localRotation -= this.rotation - this.shearX;
		let sin = MathUtils.sinDeg(localRotation), cos = MathUtils.cosDeg(localRotation);
		return Math.atan2(cos * this.c + sin * this.d, cos * this.a + sin * this.b) * MathUtils.radDeg;
	}

	/** Rotates the world transform the specified amount.
	 * <p>
	 * After changes are made to the world transform, {@link #updateAppliedTransform()} should be called and {@link #update()} will
	 * need to be called on any child bones, recursively. */
	rotateWorld (degrees: number) {
		let a = this.a, b = this.b, c = this.c, d = this.d;
		let cos = MathUtils.cosDeg(degrees), sin = MathUtils.sinDeg(degrees);
		this.a = cos * a - sin * c;
		this.b = cos * b - sin * d;
		this.c = sin * a + cos * c;
		this.d = sin * b + cos * d;
	}
}
