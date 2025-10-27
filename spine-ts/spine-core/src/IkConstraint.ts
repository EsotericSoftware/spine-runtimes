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
import type { BonePose } from "./BonePose.js";
import { Constraint } from "./Constraint.js";
import type { IkConstraintData } from "./IkConstraintData.js";
import { IkConstraintPose } from "./IkConstraintPose.js";
import type { Physics } from "./Physics.js";
import type { Skeleton } from "./Skeleton.js";
import { MathUtils } from "./Utils.js";

/** Stores the current pose for an IK constraint. An IK constraint adjusts the rotation of 1 or 2 constrained bones so the tip of
 * the last bone is as close to the target bone as possible.
 *
 * See [IK constraints](http://esotericsoftware.com/spine-ik-constraints) in the Spine User Guide. */
export class IkConstraint extends Constraint<IkConstraint, IkConstraintData, IkConstraintPose> {
	/** The 1 or 2 bones that will be modified by this IK constraint. */
	readonly bones: Array<BonePose>;

	/** The bone that is the IK target. */
	target: Bone;

	constructor (data: IkConstraintData, skeleton: Skeleton) {
		super(data, new IkConstraintPose(), new IkConstraintPose());
		if (!skeleton) throw new Error("skeleton cannot be null.");

		this.bones = [] as BonePose[];
		for (const boneData of data.bones)
			this.bones.push(skeleton.bones[boneData.index].constrained);

		this.target = skeleton.bones[data.target.index];
	}

	copy (skeleton: Skeleton): IkConstraint {
		var copy = new IkConstraint(this.data, skeleton);
		copy.pose.set(this.pose);
		return copy;
	}

	update (skeleton: Skeleton, physics: Physics) {
		const p = this.applied;
		if (p.mix === 0) return;
		const target = this.target.applied;
		const bones = this.bones;
		switch (bones.length) {
			case 1:
				IkConstraint.apply(skeleton, bones[0], target.worldX, target.worldY, p.compress, p.stretch, this.data.uniform, p.mix);
				break;
			case 2:
				IkConstraint.apply(skeleton, bones[0], bones[1], target.worldX, target.worldY, p.bendDirection, p.stretch, this.data.uniform,
					p.softness, p.mix);
				break;
		}
	}

	sort (skeleton: Skeleton) {
		skeleton.sortBone(this.target);
		const parent = this.bones[0].bone;
		skeleton.sortBone(parent);
		skeleton._updateCache.push(this);
		parent.sorted = false;
		skeleton.sortReset(parent.children);
		skeleton.constrained(parent);
		if (this.bones.length > 1) skeleton.constrained(this.bones[1].bone);
	}

	isSourceActive () {
		return this.target.active;
	}

	/** Applies 1 bone IK. The target is specified in the world coordinate system. */
	public static apply (skeleton: Skeleton, bone: BonePose, targetX: number, targetY: number, compress: boolean, stretch: boolean, uniform: boolean, mix: number): void;

	/** Applies 2 bone IK. The target is specified in the world coordinate system.
	 * @param child A direct descendant of the parent bone. */
	public static apply (skeleton: Skeleton, parent: BonePose, child: BonePose, targetX: number, targetY: number, bendDir: number, stretch: boolean, uniform: boolean, softness: number, mix: number): void;

	public static apply (skeleton: Skeleton, boneOrParent: BonePose, targetXorChild: number | BonePose, targetYOrTargetX: number, compressOrTargetY: boolean | number,
		stretchOrBendDir: boolean | number, uniformOrStretch: boolean, mixOrUniform: number | boolean, softness?: number, mix?: number) {

		if (typeof targetXorChild === "number")
			IkConstraint.apply1(skeleton, boneOrParent, targetXorChild, targetYOrTargetX, compressOrTargetY as boolean, stretchOrBendDir as boolean, uniformOrStretch, mixOrUniform as number);
		else
			IkConstraint.apply2(skeleton, boneOrParent, targetXorChild as BonePose, targetYOrTargetX, compressOrTargetY as number, stretchOrBendDir as number,
				uniformOrStretch, mixOrUniform as boolean, softness as number, mix as number);
	}

	private static apply1 (skeleton: Skeleton, bone: BonePose, targetX: number, targetY: number, compress: boolean, stretch: boolean, uniform: boolean, mix: number) {
		bone.modifyLocal(skeleton);

		// biome-ignore lint/style/noNonNullAssertion: reference runtime
		const p = bone.bone.parent!.applied;

		let pa = p.a, pb = p.b, pc = p.c, pd = p.d;
		let rotationIK = -bone.shearX - bone.rotation, tx = 0, ty = 0;

		switch (bone.inherit) {
			case Inherit.OnlyTranslation:
				tx = (targetX - bone.worldX) * MathUtils.signum(skeleton.scaleX);
				ty = (targetY - bone.worldY) * MathUtils.signum(skeleton.scaleY);
				break;
			// biome-ignore lint/suspicious/noFallthroughSwitchClause: reference runtime
			case Inherit.NoRotationOrReflection: {
				const s = Math.abs(pa * pd - pb * pc) / Math.max(0.0001, pa * pa + pc * pc);
				const sa = pa / skeleton.scaleX;
				const sc = pc / skeleton.scaleY;
				pb = -sc * s * skeleton.scaleX;
				pd = sa * s * skeleton.scaleY;
				rotationIK += Math.atan2(sc, sa) * MathUtils.radDeg;
			}
			// Fall through
			default: {
				const x = targetX - p.worldX, y = targetY - p.worldY;
				const d = pa * pd - pb * pc;
				if (Math.abs(d) <= 0.0001) {
					tx = 0;
					ty = 0;
				} else {
					tx = (x * pd - y * pb) / d - bone.x;
					ty = (y * pa - x * pc) / d - bone.y;
				}
			}
		}
		rotationIK += MathUtils.atan2Deg(ty, tx);
		if (bone.scaleX < 0) rotationIK += 180;
		if (rotationIK > 180)
			rotationIK -= 360;
		else if (rotationIK < -180)
			rotationIK += 360;
		bone.rotation += rotationIK * mix;
		if (compress || stretch) {
			switch (bone.inherit) {
				case Inherit.NoScale:
				case Inherit.NoScaleOrReflection:
					tx = targetX - bone.worldX;
					ty = targetY - bone.worldY;
			}
			const b = bone.bone.data.length * bone.scaleX;
			if (b > 0.0001) {
				const dd = tx * tx + ty * ty;
				if ((compress && dd < b * b) || (stretch && dd > b * b)) {
					const s = (Math.sqrt(dd) / b - 1) * mix + 1;
					bone.scaleX *= s;
					if (uniform) bone.scaleY *= s;
				}
			}
		}
	}

	/** Applies 2 bone IK. The target is specified in the world coordinate system.
	 * @param child A direct descendant of the parent bone. */
	private static apply2 (skeleton: Skeleton, parent: BonePose, child: BonePose, targetX: number, targetY: number, bendDir: number, stretch: boolean, uniform: boolean, softness: number, mix: number) {
		if (parent.inherit !== Inherit.Normal || child.inherit !== Inherit.Normal) return;
		parent.modifyLocal(skeleton);
		child.modifyLocal(skeleton);
		let px = parent.x, py = parent.y, psx = parent.scaleX, psy = parent.scaleY, csx = child.scaleX;
		let os1 = 0, os2 = 0, s2 = 0;
		if (psx < 0) {
			psx = -psx;
			os1 = 180;
			s2 = -1;
		} else {
			os1 = 0;
			s2 = 1;
		}
		if (psy < 0) {
			psy = -psy;
			s2 = -s2;
		}
		if (csx < 0) {
			csx = -csx;
			os2 = 180;
		} else
			os2 = 0;
		let cwx = 0, cwy = 0, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
		const u = Math.abs(psx - psy) <= 0.0001;
		if (!u || stretch) {
			child.y = 0;
			cwx = a * child.x + parent.worldX;
			cwy = c * child.x + parent.worldY;
		} else {
			cwx = a * child.x + b * child.y + parent.worldX;
			cwy = c * child.x + d * child.y + parent.worldY;
		}
		// biome-ignore lint/style/noNonNullAssertion: reference-runtime
		const pp = parent.bone.parent!.applied;
		a = pp.a;
		b = pp.b;
		c = pp.c;
		d = pp.d;
		let id = a * d - b * c, x = cwx - pp.worldX, y = cwy - pp.worldY;
		id = Math.abs(id) <= 0.0001 ? 0 : 1 / id;
		const dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
		let l1 = Math.sqrt(dx * dx + dy * dy), l2 = child.bone.data.length * csx, a1: number, a2: number;
		if (l1 < 0.0001) {
			IkConstraint.apply(skeleton, parent, targetX, targetY, false, stretch, false, mix);
			child.rotation = 0;
			return;
		}
		x = targetX - pp.worldX;
		y = targetY - pp.worldY;
		let tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
		let dd = tx * tx + ty * ty;
		if (softness !== 0) {
			softness *= psx * (csx + 1) * 0.5;
			const td = Math.sqrt(dd), sd = td - l1 - l2 * psx + softness;
			if (sd > 0) {
				let p = Math.min(1, sd / (softness * 2)) - 1;
				p = (sd - softness * (1 - p * p)) / td;
				tx -= p * tx;
				ty -= p * ty;
				dd = tx * tx + ty * ty;
			}
		}
		// biome-ignore lint/suspicious/noConfusingLabels: reference runtime
		outer:
		if (u) {
			l2 *= psx;
			let cos = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
			if (cos < -1) {
				cos = -1;
				a2 = Math.PI * bendDir;
			} else if (cos > 1) {
				cos = 1;
				a2 = 0;
				if (stretch) {
					a = (Math.sqrt(dd) / (l1 + l2) - 1) * mix + 1;
					parent.scaleX *= a;
					if (uniform) parent.scaleY *= a;
				}
			} else
				a2 = Math.acos(cos) * bendDir;
			a = l1 + l2 * cos;
			b = l2 * Math.sin(a2);
			a1 = Math.atan2(ty * a - tx * b, tx * a + ty * b);
		} else {
			a = psx * l2;
			b = psy * l2;
			const aa = a * a, bb = b * b, ta = Math.atan2(ty, tx);
			c = bb * l1 * l1 + aa * dd - aa * bb;
			const c1 = -2 * bb * l1, c2 = bb - aa;
			d = c1 * c1 - 4 * c2 * c;
			if (d >= 0) {
				let q = Math.sqrt(d);
				if (c1 < 0) q = -q;
				q = -(c1 + q) * 0.5;
				let r0 = q / c2, r1 = c / q;
				const r = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
				r0 = dd - r * r;
				if (r0 >= 0) {
					y = Math.sqrt(r0) * bendDir;
					a1 = ta - Math.atan2(y, r);
					a2 = Math.atan2(y / psy, (r - l1) / psx);
					break outer;
				}
			}
			let minAngle = MathUtils.PI, minX = l1 - a, minDist = minX * minX, minY = 0;
			let maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
			c = -a * l1 / (aa - bb);
			if (c >= -1 && c <= 1) {
				c = Math.acos(c);
				x = a * Math.cos(c) + l1;
				y = b * Math.sin(c);
				d = x * x + y * y;
				if (d < minDist) {
					minAngle = c;
					minDist = d;
					minX = x;
					minY = y;
				}
				if (d > maxDist) {
					maxAngle = c;
					maxDist = d;
					maxX = x;
					maxY = y;
				}
			}
			if (dd <= (minDist + maxDist) * 0.5) {
				a1 = ta - Math.atan2(minY * bendDir, minX);
				a2 = minAngle * bendDir;
			} else {
				a1 = ta - Math.atan2(maxY * bendDir, maxX);
				a2 = maxAngle * bendDir;
			}
		}
		const os = Math.atan2(child.y, child.x) * s2;
		a1 = (a1 - os) * MathUtils.radDeg + os1 - parent.rotation;
		if (a1 > 180)
			a1 -= 360;
		else if (a1 < -180) //
			a1 += 360;
		parent.rotation += a1 * mix;
		a2 = ((a2 + os) * MathUtils.radDeg - child.shearX) * s2 + os2 - child.rotation;
		if (a2 > 180)
			a2 -= 360;
		else if (a2 < -180) //
			a2 += 360;
		child.rotation += a2 * mix;
	}
}
