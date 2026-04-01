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

import type { BonePose } from "./BonePose.js";
import { Constraint } from "./Constraint.js";
import { Physics } from "./Physics.js";
import type { PhysicsConstraintData } from "./PhysicsConstraintData.js";
import { PhysicsConstraintPose } from "./PhysicsConstraintPose.js";
import { Skeleton } from "./Skeleton.js";
import { MathUtils } from "./Utils.js";


/** Applies physics to a bone.
 *
 * See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
export class PhysicsConstraint extends Constraint<PhysicsConstraint, PhysicsConstraintData, PhysicsConstraintPose> {
	bone: BonePose;

	_reset = true;
	ux = 0;
	uy = 0;
	cx = 0;
	cy = 0;
	tx = 0;
	ty = 0;
	xOffset = 0;
	xLag = 0;
	xVelocity = 0;
	yOffset = 0;
	yLag = 0;
	yVelocity = 0;
	rotateOffset = 0;
	rotateLag = 0;
	rotateVelocity = 0;
	scaleOffset = 0
	scaleLag = 0
	scaleVelocity = 0;
	remaining = 0;
	lastTime = 0;

	constructor (data: PhysicsConstraintData, skeleton: Skeleton) {
		super(data, new PhysicsConstraintPose(), new PhysicsConstraintPose());
		if (skeleton == null) throw new Error("skeleton cannot be null.");

		this.bone = skeleton.bones[data.bone.index].constrainedPose;
	}

	public copy (skeleton: Skeleton) {
		var copy = new PhysicsConstraint(this.data, skeleton);
		copy.pose.set(this.pose);
		return copy;
	}

	/** Resets all physics state that was the result of previous movement. Use this after moving a bone to prevent physics from
	 * reacting to the movement. */
	reset (skeleton: Skeleton) {
		this.remaining = 0;
		this.lastTime = skeleton.time;
		this._reset = true;
		this.xOffset = 0;
		this.xLag = 0;
		this.xVelocity = 0;
		this.yOffset = 0;
		this.yLag = 0;
		this.yVelocity = 0;
		this.rotateOffset = 0;
		this.rotateLag = 0;
		this.rotateVelocity = 0;
		this.scaleOffset = 0;
		this.scaleLag = 0;
		this.scaleVelocity = 0;
	}

	/** Translates the physics constraint so the next {@link update} forces are applied as if the bone moved an
	 * additional amount in world space. */
	translate (x: number, y: number) {
		this.ux -= x;
		this.uy -= y;
		this.cx -= x;
		this.cy -= y;
	}

	/** Rotates the physics constraint so the next {@link update} forces are applied as if the bone rotated
	 * around the specified point in world space. */
	rotate (x: number, y: number, degrees: number) {
		const r = degrees * MathUtils.degRad, cos = Math.cos(r), sin = Math.sin(r);
		const dx = this.cx - x, dy = this.cy - y;
		this.translate(dx * cos - dy * sin - dx, dx * sin + dy * cos - dy);
	}

	/** Applies the constraint to the constrained bones. */
	update (skeleton: Skeleton, physics: Physics) {
		const p = this.appliedPose;
		const mix = p.mix;
		if (mix === 0) return;

		const x = this.data.x > 0, y = this.data.y > 0, rotateOrShearX = this.data.rotate > 0 || this.data.shearX > 0, scaleX = this.data.scaleX > 0;
		const bone = this.bone;
		let l = bone.bone.data.length, t = this.data.step, z = 0;

		switch (physics) {
			case Physics.none:
				return;
			// biome-ignore lint/suspicious/noFallthroughSwitchClause: fall through expected
			case Physics.reset:
				this.reset(skeleton); // Fall through.
			case Physics.update: {
				const delta = Math.max(skeleton.time - this.lastTime, 0), aa = this.remaining;
				this.remaining += delta;
				this.lastTime = skeleton.time;

				const bx = bone.worldX, by = bone.worldY;
				if (this._reset) {
					this._reset = false;
					this.ux = bx;
					this.uy = by;
				} else {
					let a = this.remaining, i = p.inertia, f = skeleton.data.referenceScale, d = -1, m = 0, e = 0, qx = this.data.limit * delta,
						qy = qx * Math.abs(skeleton.scaleY);
					qx *= Math.abs(skeleton.scaleX);
					if (x || y) {
						if (x) {
							const u = (this.ux - bx) * i;
							this.xOffset += u > qx ? qx : u < -qx ? -qx : u;
							this.ux = bx;
						}
						if (y) {
							const u = (this.uy - by) * i;
							this.yOffset += u > qy ? qy : u < -qy ? -qy : u;
							this.uy = by;
						}
						if (a >= t) {
							const xs = this.xOffset, ys = this.yOffset;
							d = p.damping ** (60 * t);
							m = t * p.massInverse;
							e = p.strength;
							const w = f * p.wind, g = f * p.gravity;
							const ax = (w * skeleton.windX + g * skeleton.gravityX) * skeleton.scaleX;
							const ay = (w * skeleton.windY + g * skeleton.gravityY) * skeleton.scaleY;
							do {
								if (x) {
									this.xVelocity += (ax - this.xOffset * e) * m;
									this.xOffset += this.xVelocity * t;
									this.xVelocity *= d;
								}
								if (y) {
									this.yVelocity -= (ay + this.yOffset * e) * m;
									this.yOffset += this.yVelocity * t;
									this.yVelocity *= d;
								}
								a -= t;
							} while (a >= t);
							this.xLag = this.xOffset - xs;
							this.yLag = this.yOffset - ys;
						}
						z = Math.max(0, 1 - a / t);
						if (x) bone.worldX += (this.xOffset - this.xLag * z) * mix * this.data.x;
						if (y) bone.worldY += (this.yOffset - this.yLag * z) * mix * this.data.y;
					}
					if (rotateOrShearX || scaleX) {
						let ca = Math.atan2(bone.c, bone.a), c = 0, s = 0, mr = 0, dx = this.cx - bone.worldX, dy = this.cy - bone.worldY;
						if (dx > qx)
							dx = qx;
						else if (dx < -qx) //
							dx = -qx;
						if (dy > qy)
							dy = qy;
						else if (dy < -qy) //
							dy = -qy;
						a = this.remaining;
						if (rotateOrShearX) {
							mr = (this.data.rotate + this.data.shearX) * mix;
							z = this.rotateLag * Math.max(0, 1 - aa / t);
							let r = Math.atan2(dy + this.ty, dx + this.tx) - ca - (this.rotateOffset - z) * mr;
							this.rotateOffset += (r - Math.ceil(r * MathUtils.invPI2 - 0.5) * MathUtils.PI2) * i;
							r = (this.rotateOffset - z) * mr + ca;
							c = Math.cos(r);
							s = Math.sin(r);
							if (scaleX) {
								r = l * bone.getWorldScaleX();
								if (r > 0) this.scaleOffset += (dx * c + dy * s) * i / r;
							}
						} else {
							c = Math.cos(ca);
							s = Math.sin(ca);
							const r = l * bone.getWorldScaleX() - this.scaleLag * Math.max(0, 1 - aa / t);
							if (r > 0) this.scaleOffset += (dx * c + dy * s) * i / r;
						}
						if (a >= t) {
							if (d === -1) {
								d = p.damping ** (60 * t);
								m = t * p.massInverse;
								e = p.strength;
							}
							const ax = p.wind * skeleton.windX + p.gravity * skeleton.gravityX;
							const ay = (p.wind * skeleton.windY + p.gravity * skeleton.gravityY) * Skeleton.yDir;
							const rs = this.rotateOffset, ss = this.scaleOffset, h = l / f;
							while (true) {
								a -= t;
								if (scaleX) {
									this.scaleVelocity += (ax * c - ay * s - this.scaleOffset * e) * m;
									this.scaleOffset += this.scaleVelocity * t;
									this.scaleVelocity *= d;
								}
								if (rotateOrShearX) {
									this.rotateVelocity -= ((ax * s + ay * c) * h + this.rotateOffset * e) * m;
									this.rotateOffset += this.rotateVelocity * t;
									this.rotateVelocity *= d;
									if (a < t) break;
									const r = this.rotateOffset * mr + ca;
									c = Math.cos(r);
									s = Math.sin(r);
								} else if (a < t) //
									break;
							}
							this.rotateLag = this.rotateOffset - rs;
							this.scaleLag = this.scaleOffset - ss;
						}
						z = Math.max(0, 1 - a / t);
					}
					this.remaining = a;
				}
				this.cx = bone.worldX;
				this.cy = bone.worldY;
				break;
			}
			case Physics.pose:
				z = Math.max(0, 1 - this.remaining / t);
				if (x) bone.worldX += (this.xOffset - this.xLag * z) * mix * this.data.x;
				if (y) bone.worldY += (this.yOffset - this.yLag * z) * mix * this.data.y;
		}

		if (rotateOrShearX) {
			let o = (this.rotateOffset - this.rotateLag * z) * mix, s = 0, c = 0, a = 0;
			if (this.data.shearX > 0) {
				let r = 0;
				if (this.data.rotate > 0) {
					r = o * this.data.rotate;
					s = Math.sin(r);
					c = Math.cos(r);
					a = bone.b;
					bone.b = c * a - s * bone.d;
					bone.d = s * a + c * bone.d;
				}
				r += o * this.data.shearX;
				s = Math.sin(r);
				c = Math.cos(r);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
			} else {
				o *= this.data.rotate;
				s = Math.sin(o);
				c = Math.cos(o);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
				a = bone.b;
				bone.b = c * a - s * bone.d;
				bone.d = s * a + c * bone.d;
			}
		}
		if (scaleX) {
			const s = 1 + (this.scaleOffset - this.scaleLag * z) * mix * this.data.scaleX;
			bone.a *= s;
			bone.c *= s;
		}
		if (physics !== Physics.pose) {
			this.tx = l * bone.a;
			this.ty = l * bone.c;
		}
		bone.modifyWorld(skeleton._update);
	}

	sort (skeleton: Skeleton) {
		const bone = this.bone.bone;
		skeleton.sortBone(bone);
		skeleton._updateCache.push(this);
		skeleton.sortReset(bone.children);
		skeleton.constrained(bone);
	}

	isSourceActive () {
		return this.bone.bone.active;
	}

}
