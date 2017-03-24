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

using System;

namespace Spine {
	public class IkConstraint : IConstraint {
		internal IkConstraintData data;
		internal ExposedList<Bone> bones = new ExposedList<Bone>();
		internal Bone target;
		internal float mix;
		internal int bendDirection;

		public IkConstraintData Data { get { return data; } }
		public int Order { get { return data.order; } }
		public ExposedList<Bone> Bones { get { return bones; } }
		public Bone Target { get { return target; } set { target = value; } }
		public int BendDirection { get { return bendDirection; } set { bendDirection = value; } }
		public float Mix { get { return mix; } set { mix = value; } }

		public IkConstraint (IkConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			mix = data.mix;
			bendDirection = data.bendDirection;

			bones = new ExposedList<Bone>(data.bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.FindBone(boneData.name));
			target = skeleton.FindBone(data.target.name);
		}

		public void Update () {
			Apply();
		}

		public void Apply () {
			Bone target = this.target;
			ExposedList<Bone> bones = this.bones;
			switch (bones.Count) {
			case 1:
				Apply(bones.Items[0], target.worldX, target.worldY, mix);
				break;
			case 2:
				Apply(bones.Items[0], bones.Items[1], target.worldX, target.worldY, bendDirection, mix);
				break;
			}
		}

		override public String ToString () {
			return data.name;
		}

		/// <summary>Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified
		/// in the world coordinate system.</summary>
		static public void Apply (Bone bone, float targetX, float targetY, float alpha) {
			if (!bone.appliedValid) bone.UpdateAppliedTransform();
			Bone p = bone.parent;
			float id = 1 / (p.a * p.d - p.b * p.c);
			float x = targetX - p.worldX, y = targetY - p.worldY;
			float tx = (x * p.d - y * p.b) * id - bone.ax, ty = (y * p.a - x * p.c) * id - bone.ay;
			float rotationIK = (float)Math.Atan2(ty, tx) * MathUtils.RadDeg - bone.ashearX - bone.arotation;
			if (bone.ascaleX < 0) rotationIK += 180;
			if (rotationIK > 180)
				rotationIK -= 360;
			else if (rotationIK < -180) rotationIK += 360;
			bone.UpdateWorldTransform(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, bone.ascaleX, bone.ascaleY, bone.ashearX, 
				bone.ashearY);
		}

		/// <summary>Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as
		/// possible. The target is specified in the world coordinate system.</summary>
		/// <param name="child">A direct descendant of the parent bone.</param>
		static public void Apply (Bone parent, Bone child, float targetX, float targetY, int bendDir, float alpha) {
			if (alpha == 0) {
				child.UpdateWorldTransform ();
				return;
			}
			//float px = parent.x, py = parent.y, psx = parent.scaleX, psy = parent.scaleY, csx = child.scaleX;
			if (!parent.appliedValid) parent.UpdateAppliedTransform();
			if (!child.appliedValid) child.UpdateAppliedTransform();
			float px = parent.ax, py = parent.ay, psx = parent.ascaleX, psy = parent.ascaleY, csx = child.ascaleX;
			int os1, os2, s2;
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
			float cx = child.ax, cy, cwx, cwy, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
			bool u = Math.Abs(psx - psy) <= 0.0001f;
			if (!u) {
				cy = 0;
				cwx = a * cx + parent.worldX;
				cwy = c * cx + parent.worldY;
			} else {
				cy = child.ay;
				cwx = a * cx + b * cy + parent.worldX;
				cwy = c * cx + d * cy + parent.worldY;
			}
			Bone pp = parent.parent;
			a = pp.a;
			b = pp.b;
			c = pp.c;
			d = pp.d;
			float id = 1 / (a * d - b * c), x = targetX - pp.worldX, y = targetY - pp.worldY;
			float tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
			x = cwx - pp.worldX;
			y = cwy - pp.worldY;
			float dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
			float l1 = (float)Math.Sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1, a2;
			if (u) {
				l2 *= psx;
				float cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
				if (cos < -1)
					cos = -1;
				else if (cos > 1) cos = 1;
				a2 = (float)Math.Acos(cos) * bendDir;
				a = l1 + l2 * cos;
				b = l2 * (float)Math.Sin(a2);
				a1 = (float)Math.Atan2(ty * a - tx * b, tx * a + ty * b);
			} else {
				a = psx * l2;
				b = psy * l2;
				float aa = a * a, bb = b * b, dd = tx * tx + ty * ty, ta = (float)Math.Atan2(ty, tx);
				c = bb * l1 * l1 + aa * dd - aa * bb;
				float c1 = -2 * bb * l1, c2 = bb - aa;
				d = c1 * c1 - 4 * c2 * c;
				if (d >= 0) {
					float q = (float)Math.Sqrt(d);
					if (c1 < 0) q = -q;
					q = -(c1 + q) / 2;
					float r0 = q / c2, r1 = c / q;
					float r = Math.Abs(r0) < Math.Abs(r1) ? r0 : r1;
					if (r * r <= dd) {
						y = (float)Math.Sqrt(dd - r * r) * bendDir;
						a1 = ta - (float)Math.Atan2(y, r);
						a2 = (float)Math.Atan2(y / psy, (r - l1) / psx);
						goto outer;
					}
				}
				float minAngle = 0, minDist = float.MaxValue, minX = 0, minY = 0;
				float maxAngle = 0, maxDist = 0, maxX = 0, maxY = 0;
				x = l1 + a;
				d = x * x;
				if (d > maxDist) {
					maxAngle = 0;
					maxDist = d;
					maxX = x;
				}
				x = l1 - a;
				d = x * x;
				if (d < minDist) {
					minAngle = (float)Math.PI;
					minDist = d;
					minX = x;
				}
				float angle = (float)Math.Acos(-a * l1 / (aa - bb));
				x = a * (float)Math.Cos(angle) + l1;
				y = b * (float)Math.Sin(angle);
				d = x * x + y * y;
				if (d < minDist) {
					minAngle = angle;
					minDist = d;
					minX = x;
					minY = y;
				}
				if (d > maxDist) {
					maxAngle = angle;
					maxDist = d;
					maxX = x;
					maxY = y;
				}
				if (dd <= (minDist + maxDist) / 2) {
					a1 = ta - (float)Math.Atan2(minY * bendDir, minX);
					a2 = minAngle * bendDir;
				} else {
					a1 = ta - (float)Math.Atan2(maxY * bendDir, maxX);
					a2 = maxAngle * bendDir;
				}
			}
			outer:
			float os = (float)Math.Atan2(cy, cx) * s2;
			float rotation = parent.arotation;
			a1 = (a1 - os) * MathUtils.RadDeg + os1 - rotation;
			if (a1 > 180)
				a1 -= 360;
			else if (a1 < -180) a1 += 360;
			parent.UpdateWorldTransform(px, py, rotation + a1 * alpha, parent.scaleX, parent.ascaleY, 0, 0);
			rotation = child.arotation;
			a2 = ((a2 + os) * MathUtils.RadDeg - child.ashearX) * s2 + os2 - rotation;
			if (a2 > 180)
				a2 -= 360;
			else if (a2 < -180) a2 += 360;
			child.UpdateWorldTransform(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
		}
	}
}
