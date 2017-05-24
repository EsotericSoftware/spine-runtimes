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
	export class IkConstraint implements Constraint {
		data: IkConstraintData;
		bones: Array<Bone>;
		target: Bone;
		mix = 1;
		bendDirection = 0;

		constructor (data: IkConstraintData, skeleton: Skeleton) {
			if (data == null) throw new Error("data cannot be null.");
			if (skeleton == null) throw new Error("skeleton cannot be null.");
			this.data = data;
			this.mix = data.mix;
			this.bendDirection = data.bendDirection;

			this.bones = new Array<Bone>();
			for (let i = 0; i < data.bones.length; i++)
				this.bones.push(skeleton.findBone(data.bones[i].name));
			this.target = skeleton.findBone(data.target.name);
		}

		getOrder () {
			return this.data.order;
		}

		apply () {
			this.update();
		}

		update () {
			let target = this.target;
			let bones = this.bones;
			switch (bones.length) {
			case 1:
				this.apply1(bones[0], target.worldX, target.worldY, this.mix);
				break;
			case 2:
				this.apply2(bones[0], bones[1], target.worldX, target.worldY, this.bendDirection, this.mix);
				break;
			}
		}

		/** Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified in the world
		 * coordinate system. */
		apply1 (bone: Bone, targetX: number, targetY: number, alpha: number) {
			if (!bone.appliedValid) bone.updateAppliedTransform();
			let p = bone.parent;
			let id = 1 / (p.a * p.d - p.b * p.c);
			let x = targetX - p.worldX, y = targetY - p.worldY;
			let tx = (x * p.d - y * p.b) * id - bone.ax, ty = (y * p.a - x * p.c) * id - bone.ay;
			let rotationIK = Math.atan2(ty, tx) * MathUtils.radDeg - bone.ashearX - bone.arotation;
			if (bone.ascaleX < 0) rotationIK += 180;
			if (rotationIK > 180)
				rotationIK -= 360;
			else if (rotationIK < -180) rotationIK += 360;
			bone.updateWorldTransformWith(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, bone.ascaleX, bone.ascaleY, bone.ashearX,
				bone.ashearY);
		}

		/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
		 * target is specified in the world coordinate system.
		 * @param child A direct descendant of the parent bone. */
		apply2 (parent: Bone, child: Bone, targetX: number, targetY: number, bendDir: number, alpha: number) {
			if (alpha == 0) {
				child.updateWorldTransform();
				return;
			}
			if (!parent.appliedValid) parent.updateAppliedTransform();
			if (!child.appliedValid) child.updateAppliedTransform();
			let px = parent.ax, py = parent.ay, psx = parent.ascaleX, psy = parent.ascaleY, csx = child.ascaleX;
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
			let cx = child.ax, cy = 0, cwx = 0, cwy = 0, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
			let u = Math.abs(psx - psy) <= 0.0001;
			if (!u) {
				cy = 0;
				cwx = a * cx + parent.worldX;
				cwy = c * cx + parent.worldY;
			} else {
				cy = child.ay;
				cwx = a * cx + b * cy + parent.worldX;
				cwy = c * cx + d * cy + parent.worldY;
			}
			let pp = parent.parent;
			a = pp.a;
			b = pp.b;
			c = pp.c;
			d = pp.d;
			let id = 1 / (a * d - b * c), x = targetX - pp.worldX, y = targetY - pp.worldY;
			let tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
			x = cwx - pp.worldX;
			y = cwy - pp.worldY;
			let dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
			let l1 = Math.sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1 = 0, a2 = 0;
			outer:
			if (u) {
				l2 *= psx;
				let cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
				if (cos < -1)
					cos = -1;
				else if (cos > 1) cos = 1;
				a2 = Math.acos(cos) * bendDir;
				a = l1 + l2 * cos;
				b = l2 * Math.sin(a2);
				a1 = Math.atan2(ty * a - tx * b, tx * a + ty * b);
			} else {
				a = psx * l2;
				b = psy * l2;
				let aa = a * a, bb = b * b, dd = tx * tx + ty * ty, ta = Math.atan2(ty, tx);
				c = bb * l1 * l1 + aa * dd - aa * bb;
				let c1 = -2 * bb * l1, c2 = bb - aa;
				d = c1 * c1 - 4 * c2 * c;
				if (d >= 0) {
					let q = Math.sqrt(d);
					if (c1 < 0) q = -q;
					q = -(c1 + q) / 2;
					let r0 = q / c2, r1 = c / q;
					let r = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
					if (r * r <= dd) {
						y = Math.sqrt(dd - r * r) * bendDir;
						a1 = ta - Math.atan2(y, r);
						a2 = Math.atan2(y / psy, (r - l1) / psx);
						break outer;
					}
				}
				let minAngle = spine.MathUtils.PI, minX = l1 - a, minDist = minX * minX, minY = 0;
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
				if (dd <= (minDist + maxDist) / 2) {
					a1 = ta - Math.atan2(minY * bendDir, minX);
					a2 = minAngle * bendDir;
				} else {
					a1 = ta - Math.atan2(maxY * bendDir, maxX);
					a2 = maxAngle * bendDir;
				}
			}
			let os = Math.atan2(cy, cx) * s2;
			let rotation = parent.arotation;
			a1 = (a1 - os) * MathUtils.radDeg + os1 - rotation;
			if (a1 > 180)
				a1 -= 360;
			else if (a1 < -180) a1 += 360;
			parent.updateWorldTransformWith(px, py, rotation + a1 * alpha, parent.ascaleX, parent.ascaleY, 0, 0);
			rotation = child.arotation;
			a2 = ((a2 + os) * MathUtils.radDeg - child.ashearX) * s2 + os2 - rotation;
			if (a2 > 180)
				a2 -= 360;
			else if (a2 < -180) a2 += 360;
			child.updateWorldTransformWith(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
		}
	}
}
