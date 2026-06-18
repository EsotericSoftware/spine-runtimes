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

import type { Bone } from "./Bone.js";
import { Inherit } from "./BoneData.js";
import type { Physics } from "./Physics.js";
import type { Pose } from "./Pose.js";
import type { Skeleton } from "./Skeleton.js";
import type { Update } from "./Update.js";
import { MathUtils, type Vector2 } from "./Utils.js";

/** The applied local pose and world transform for a bone. This is the {@link Bone.getPose} with constraints applied and the
 * world transform computed by {@link Skeleton.updateWorldTransform} and {@link updateWorldTransform}.
 *
 * If the world transform is changed, call {@link updateLocalTransform} before using the local transform. The local
 * transform may be needed by other code (eg to apply another constraint).
 *
 * After changing the world transform, call {@link updateWorldTransform} on every descendant bone. It may be more
 * convenient to modify the local transform instead, then call {@link Skeleton.updateWorldTransform} to update the world
 * transforms for all bones and apply constraints. */
export class BonePose implements Pose<BonePose>, Update {
	bone!: Bone;

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

	inherit = Inherit.Normal;

	/** The world transform `[a b][c d]` x-axis x component. */
	a = 0;

	/** The world transform `[a b][c d]` y-axis x component. */
	b = 0;

	/** The world transform `[a b][c d]` x-axis y component. */
	c = 0;

	/** The world transform `[a b][c d]` y-axis y component. */
	d = 0;

	/** The world X position. If changed, {@link updateLocalTransform} should be called. */
	worldY = 0;

	/** The world Y position. If changed, {@link updateLocalTransform} should be called. */
	worldX = 0;

	world = 0;
	local = 0;

	set (pose: BonePose): void {
		if (pose == null) throw new Error("pose cannot be null.");
		this.x = pose.x;
		this.y = pose.y;
		this.rotation = pose.rotation;
		this.scaleX = pose.scaleX;
		this.scaleY = pose.scaleY;
		this.shearX = pose.shearX;
		this.shearY = pose.shearY;
		this.inherit = pose.inherit;
	}

	setPosition (x: number, y: number): void {
		this.x = x;
		this.y = y;
	}

	setScale (scaleX: number, scaleY: number): void;
	setScale (scale: number): void;
	setScale (scaleOrX: number, scaleY?: number): void {
		this.scaleX = scaleOrX;
		this.scaleY = scaleY === undefined ? scaleOrX : scaleY;
	}

	/** Determines how parent world transforms affect this bone. */
	public getInherit (): Inherit {
		return this.inherit;
	}

	public setInherit (inherit: Inherit): void {
		if (inherit == null) throw new Error("inherit cannot be null.");
		this.inherit = inherit;
	}

	/** Called by {@link Skeleton.updateCache} to compute the world transform, if needed. */
	public update (skeleton: Skeleton, physics: Physics): void {
		if (this.world !== skeleton._update) this.updateWorldTransform(skeleton);
	}

	/** Computes the world transform using the parent bone's world transform and this applied local pose. Child bones are not
	 * updated.
	 *
	 * See <a href="https://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide. */
	updateWorldTransform (skeleton: Skeleton): void {
		if (this.local === skeleton._update)
			this.updateLocalTransform(skeleton);
		else
			this.world = skeleton._update;

		const rotation = this.rotation;
		const scaleX = this.scaleX;
		const scaleY = this.scaleY;
		const shearX = this.shearX;
		const shearY = this.shearY;
		if (!this.bone.parent) { // Root bone.
			const sx = skeleton.scaleX, sy = skeleton.scaleY;
			const rx = (rotation + shearX) * MathUtils.degRad;
			const ry = (rotation + 90 + shearY) * MathUtils.degRad;
			this.a = Math.cos(rx) * scaleX * sx;
			this.b = Math.cos(ry) * scaleY * sx;
			this.c = Math.sin(rx) * scaleX * sy;
			this.d = Math.sin(ry) * scaleY * sy;
			this.worldX = this.x * sx + skeleton.x;
			this.worldY = this.y * sy + skeleton.y;
			return;
		}

		const parent = this.bone.parent.appliedPose;
		let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		this.worldX = pa * this.x + pb * this.y + parent.worldX;
		this.worldY = pc * this.x + pd * this.y + parent.worldY;

		switch (this.inherit) {
			case Inherit.Normal: {
				const rx = (rotation + shearX) * MathUtils.degRad;
				const ry = (rotation + 90 + shearY) * MathUtils.degRad;
				const la = Math.cos(rx) * scaleX;
				const lb = Math.cos(ry) * scaleY;
				const lc = Math.sin(rx) * scaleX;
				const ld = Math.sin(ry) * scaleY;
				this.a = pa * la + pb * lc;
				this.b = pa * lb + pb * ld;
				this.c = pc * la + pd * lc;
				this.d = pc * lb + pd * ld;
				return;
			}
			case Inherit.OnlyTranslation: {
				const sx = skeleton.scaleX, sy = skeleton.scaleY;
				const rx = (rotation + shearX) * MathUtils.degRad;
				const ry = (rotation + 90 + shearY) * MathUtils.degRad;
				this.a = Math.cos(rx) * scaleX * sx;
				this.b = Math.cos(ry) * scaleY * sx;
				this.c = Math.sin(rx) * scaleX * sy;
				this.d = Math.sin(ry) * scaleY * sy;
				break;
			}
			case Inherit.NoRotationOrReflection: {
				const sx = skeleton.scaleX, sy = skeleton.scaleY, sxi = 1 / sx, syi = 1 / sy;
				pa *= sxi;
				pc *= syi;
				let s = pa * pa + pc * pc;
				let r = 0;
				if (s > MathUtils.epsilon2) {
					s = Math.abs(pa * pd * syi - pb * sxi * pc) / s;
					pb = pc * s;
					pd = pa * s;
					r = rotation - MathUtils.atan2Deg(pc, pa);
				} else {
					pa = 0;
					pc = 0;
					r = rotation - 90 + MathUtils.atan2Deg(pd, pb);
				}
				const rx = (r + shearX) * MathUtils.degRad;
				const ry = (r + shearY + 90) * MathUtils.degRad;
				const la = Math.cos(rx) * scaleX;
				const lb = Math.cos(ry) * scaleY;
				const lc = Math.sin(rx) * scaleX;
				const ld = Math.sin(ry) * scaleY;
				this.a = (pa * la - pb * lc) * sx;
				this.b = (pa * lb - pb * ld) * sx;
				this.c = (pc * la + pd * lc) * sy;
				this.d = (pc * lb + pd * ld) * sy;
				break;
			}
			case Inherit.NoScale:
			case Inherit.NoScaleOrReflection: {
				const sx = skeleton.scaleX, sy = skeleton.scaleY, sxi = 1 / sx, syi = 1 / sy;
				const r = rotation * MathUtils.degRad, cos = Math.cos(r), sin = Math.sin(r);
				let za = (pa * cos + pb * sin) * sxi;
				let zc = (pc * cos + pd * sin) * syi;
				const s = 1 / Math.sqrt(za * za + zc * zc);
				za *= s;
				zc *= s;
				let zb = -zc, zd = za;
				if (this.inherit === Inherit.NoScale && pa * pd - pb * pc < 0 !== (sx < 0 !== sy < 0)) {
					zb = -zb;
					zd = -zd;
				}
				const rx = shearX * MathUtils.degRad;
				const ry = (90 + shearY) * MathUtils.degRad;
				const la = Math.cos(rx) * scaleX;
				const lb = Math.cos(ry) * scaleY;
				const lc = Math.sin(rx) * scaleX;
				const ld = Math.sin(ry) * scaleY;
				this.a = (za * la + zb * lc) * sx;
				this.b = (za * lb + zb * ld) * sx;
				this.c = (zc * la + zd * lc) * sy;
				this.d = (zc * lb + zd * ld) * sy;
				break;
			}
		}
	}

	/** Computes the local transform values from the world transform.
	 *
	 * If the world transform is modified (by a constraint, {@link rotateWorld}, etc) then this method should be called so
	 * the local transform matches the world transform. The local transform may be needed by other code (eg to apply another
	 * constraint).
	 *
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The local transform after
	 * calling this method is equivalent to the local transform used to compute the world transform, but may not be identical. */
	public updateLocalTransform (skeleton: Skeleton): void {
		this.local = 0;
		this.world = skeleton._update;

		const sx = skeleton.scaleX, sy = skeleton.scaleY;
		if (!this.bone.parent) {
			const sxi = 1 / sx, syi = 1 / sy;
			this.x = (this.worldX - skeleton.x) * sxi;
			this.y = (this.worldY - skeleton.y) * syi;
			this.set5(this.a * sxi, this.b * sxi, this.c * syi, this.d * syi, 0);
			return;
		}

		const parent = this.bone.parent.appliedPose;
		let pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		const pad = pa * pd - pb * pc, pid = 1 / (pa * pd - pb * pc);
		const ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
		const dx = this.worldX - parent.worldX, dy = this.worldY - parent.worldY;
		this.x = dx * ia - dy * ib;
		this.y = dy * id - dx * ic;

		switch (this.inherit) {
			case Inherit.Normal: this.set5(ia * this.a - ib * this.c, ia * this.b - ib * this.d, id * this.c - ic * this.a, id * this.d - ic * this.b, 0); break;
			case Inherit.OnlyTranslation: {
				const sxi = 1 / sx, syi = 1 / sy;
				this.set5(this.a * sxi, this.b * sxi, this.c * syi, this.d * syi, 0);
				break;
			}
			case Inherit.NoRotationOrReflection: {
				const sxi = 1 / sx, syi = 1 / sy;
				pa *= sxi;
				pc *= syi;
				const wa = this.a * sxi, wb = this.b * sxi, wc = this.c * syi, wd = this.d * syi;
				const s = 1 / (pa * pa + pc * pc), det = 1 / Math.abs(pad * sxi * syi);
				this.set5((pa * wa + pc * wc) * s, (pa * wb + pc * wd) * s, (pa * wc - pc * wa) * det, (pa * wd - pc * wb) * det,
					MathUtils.atan2Deg(pc, pa));
				break;
			}
			case Inherit.NoScale:
			case Inherit.NoScaleOrReflection: {
				const sxi = 1 / sx, syi = 1 / sy;
				const wa = this.a * sxi, wb = this.b * sxi, wc = this.c * syi, wd = this.d * syi;
				let tx = pd * this.a - pb * this.c, ty = pa * this.c - pc * this.a;
				if (pad < 0) {
					tx = -tx;
					ty = -ty;
				}
				let r = MathUtils.atan2Deg(ty, tx);
				this.rotation = r;
				r *= MathUtils.degRad;
				const cos = Math.cos(r), sin = Math.sin(r);
				let za = (pa * cos + pb * sin) * sxi;
				let zc = (pc * cos + pd * sin) * syi;
				const s = 1 / Math.sqrt(za * za + zc * zc);
				za *= s;
				zc *= s;
				const si = this.inherit === Inherit.NoScale && pad < 0 !== (sx < 0 !== sy < 0) ? -1 : 1;
				this.set4(za * wa + zc * wc, za * wb + zc * wd, (za * wc - zc * wa) * si, (za * wd - zc * wb) * si);
			}
		}
	}

	private set4 (ra: number, rb: number, rc: number, rd: number): void {
		const x = ra * ra + rc * rc, y = rb * rb + rd * rd;
		if (x > MathUtils.epsilon2) {
			this.shearX = MathUtils.atan2Deg(rc, ra);
			this.scaleX = Math.sqrt(x);
		} else {
			this.shearX = 0;
			this.scaleX = 0;
		}
		this.scaleY = Math.sqrt(y);
		if (y > MathUtils.epsilon2) {
			this.shearY = MathUtils.atan2Deg(rd, rb);
			if (ra * rd - rb * rc < 0) {
				this.scaleY = -this.scaleY;
				this.shearY += 90;
			} else
				this.shearY -= 90;
			if (this.shearY > 180)
				this.shearY -= 360;
			else if (this.shearY <= -180) //
				this.shearY += 360;
		} else
			this.shearY = 0;
	}

	private set5 (ra: number, rb: number, rc: number, rd: number, ro: number): void {
		this.shearX = 0;
		const x = ra * ra + rc * rc, y = rb * rb + rd * rd;
		if (x > MathUtils.epsilon2) {
			const r = MathUtils.atan2Deg(rc, ra);
			this.rotation = r + ro;
			this.scaleX = Math.sqrt(x);
			this.scaleY = Math.sqrt(y);
			if (y > MathUtils.epsilon2) {
				this.shearY = MathUtils.atan2Deg(rd, rb);
				if (ra * rd - rb * rc < 0) {
					this.scaleY = -this.scaleY;
					this.shearY += 90 - r;
				} else
					this.shearY -= 90 + r;
				if (this.shearY > 180)
					this.shearY -= 360;
				else if (this.shearY <= -180) //
					this.shearY += 360;
			} else
				this.shearY = 0;
		} else {
			this.scaleX = 0;
			this.scaleY = Math.sqrt(y);
			this.shearY = 0;
			this.rotation = y > MathUtils.epsilon2 ? MathUtils.atan2Deg(rd, rb) - 90 + ro : ro;
		}
	}

	/** If the world transform has been modified by constraints and the local transform no longer matches,
	 * {@link updateLocalTransform} is called. Call this after {@link Skeleton.updateWorldTransform} before
	 * using the applied local transform. */
	public validateLocalTransform (skeleton: Skeleton): void {
		if (this.local === skeleton._update) this.updateLocalTransform(skeleton);
	}

	modifyLocal (skeleton: Skeleton): void {
		if (this.local === skeleton._update) this.updateLocalTransform(skeleton);
		this.world = 0;
		this.resetWorld(skeleton._update);
	}

	modifyWorld (update: number): void {
		this.local = update;
		this.world = update;
		this.resetWorld(update);
	}

	private resetWorld (update: number): void {
		const children = this.bone.children;
		for (let i = 0, n = children.length; i < n; i++) {
			const child = children[i].appliedPose;
			if (child.world === update) {
				child.world = 0;
				child.local = 0;
				child.resetWorld(update);
			}
		}
	}

	/** The world rotation for the X axis, calculated using {@link a} and {@link c}. This is the direction the bone is
	 * pointing. */
	public getWorldRotationX (): number {
		return MathUtils.atan2Deg(this.c, this.a);
	}

	/** The world rotation for the Y axis, calculated using {@link b} and {@link d}. */
	public getWorldRotationY (): number {
		return MathUtils.atan2Deg(this.d, this.b);
	}

	/** The magnitude (always positive) of the world scale X, calculated using {@link a} and {@link c}. */
	public getWorldScaleX (): number {
		return Math.sqrt(this.a * this.a + this.c * this.c);
	}

	/** The magnitude (always positive) of the world scale Y, calculated using {@link b} and {@link d}. */
	public getWorldScaleY (): number {
		return Math.sqrt(this.b * this.b + this.d * this.d);
	}

	// public Matrix3 getWorldTransform (Matrix3 worldTransform) {
	// 	if (worldTransform == null) throw new IllegalArgumentException("worldTransform cannot be null.");
	// 	float[] val = worldTransform.val;
	// 	val[M00] = a;
	// 	val[M01] = b;
	// 	val[M10] = c;
	// 	val[M11] = d;
	// 	val[M02] = worldX;
	// 	val[M12] = worldY;
	// 	val[M20] = 0;
	// 	val[M21] = 0;
	// 	val[M22] = 1;
	// 	return worldTransform;
	// }

	/** Transforms a point from world coordinates to the bone's local coordinates. */
	public worldToLocal (world: Vector2): Vector2 {
		if (world == null) throw new Error("world cannot be null.");
		const det = this.a * this.d - this.b * this.c;
		const x = world.x - this.worldX, y = world.y - this.worldY;
		world.x = (x * this.d - y * this.b) / det;
		world.y = (y * this.a - x * this.c) / det;
		return world;
	}

	/** Transforms a point from the bone's local coordinates to world coordinates. */
	public localToWorld (local: Vector2): Vector2 {
		if (local == null) throw new Error("local cannot be null.");
		const x = local.x, y = local.y;
		local.x = x * this.a + y * this.b + this.worldX;
		local.y = x * this.c + y * this.d + this.worldY;
		return local;
	}

	/** Transforms a point from world coordinates to the parent bone's local coordinates. */
	public worldToParent (world: Vector2): Vector2 {
		if (world == null) throw new Error("world cannot be null.");
		return this.bone.parent == null ? world : this.bone.parent.appliedPose.worldToLocal(world);
	}

	/** Transforms a point from the parent bone's coordinates to world coordinates. */
	public parentToWorld (world: Vector2): Vector2 {
		if (world == null) throw new Error("world cannot be null.");
		return this.bone.parent == null ? world : this.bone.parent.appliedPose.localToWorld(world);
	}

	/** Transforms a world rotation to a local rotation. */
	public worldToLocalRotation (worldRotation: number): number {
		worldRotation *= MathUtils.degRad;
		const sin = Math.sin(worldRotation), cos = Math.cos(worldRotation);
		return MathUtils.atan2Deg(this.a * sin - this.c * cos, this.d * cos - this.b * sin) + this.rotation - this.shearX;
	}

	/** Transforms a local rotation to a world rotation. */
	localToWorldRotation (localRotation: number): number {
		localRotation = (localRotation - this.rotation - this.shearX) * MathUtils.degRad;
		const sin = Math.sin(localRotation), cos = Math.cos(localRotation);
		return MathUtils.atan2Deg(cos * this.c + sin * this.d, cos * this.a + sin * this.b);
	}

	/** Rotates the world transform the specified amount. */
	rotateWorld (degrees: number) {
		degrees *= MathUtils.degRad;
		const sin = Math.sin(degrees), cos = Math.cos(degrees);
		const ra = this.a, rb = this.b;
		this.a = cos * ra - sin * this.c;
		this.b = cos * rb - sin * this.d;
		this.c = sin * ra + cos * this.c;
		this.d = sin * rb + cos * this.d;
	}
}
