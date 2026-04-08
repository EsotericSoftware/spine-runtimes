/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

using System;

namespace Spine {

	/// <summary>
	/// <para>
	/// Adjusts the local rotation of 1 or 2 constrained bones so the world position of the tip of the last bone is as close to the
	/// target bone as possible.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-ik-constraints">IK constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class IkConstraint : Constraint<IkConstraint, IkConstraintData, IkConstraintPose> {
		internal readonly ExposedList<BonePose> bones;
		internal Bone target;

		public IkConstraint (IkConstraintData data, Skeleton skeleton)
			: base(data, new IkConstraintPose(), new IkConstraintPose()) {
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");

			bones = new ExposedList<BonePose>(data.bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.bones.Items[boneData.index].constrainedPose);

			target = skeleton.bones.Items[data.target.index];
		}

		override public IConstraint Copy (Skeleton skeleton) {
			var copy = new IkConstraint(data, skeleton);
			copy.pose.Set(pose);
			return copy;
		}

		/// <summary>Applies the constraint to the constrained bones.</summary>
		override public void Update (Skeleton skeleton, Physics physics) {
			IkConstraintPose p = appliedPose;
			if (p.mix == 0) return;
			BonePose target = this.target.appliedPose;
			BonePose[] bones = this.bones.Items;
			switch (this.bones.Count) {
			case 1:
				Apply(skeleton, bones[0], target.worldX, target.worldY, p.compress, p.stretch, data.uniform, p.mix);
				break;
			case 2:
				Apply(skeleton, bones[0], bones[1], target.worldX, target.worldY, p.bendDirection, p.stretch, data.uniform,
					p.softness, p.mix);
				break;
			}
		}

		override public void Sort (Skeleton skeleton) {
			skeleton.SortBone(target);
			Bone parent = bones.Items[0].bone;
			skeleton.SortBone(parent);
			skeleton.updateCache.Add(this);
			parent.sorted = false;
			skeleton.SortReset(parent.children);
			skeleton.Constrained(parent);
			if (bones.Count > 1) skeleton.Constrained(bones.Items[1].bone);
		}

		override public bool IsSourceActive { get { return target.active; } }

		/// <summary>The bones that will be modified by this IK constraint.</summary>
		public ExposedList<BonePose> Bones {
			get { return bones; }
		}

		/// <summary>The bone that is the IK target.</summary>
		public Bone Target {
			get { return target; }
			set { target = value; }
		}

		/// <summary>Applies 1 bone IK. The target is specified in the world coordinate system.</summary>
		static public void Apply (Skeleton skeleton, BonePose bone, float targetX, float targetY, bool compress, bool stretch,
			bool uniform, float mix) {
			if (bone == null) throw new ArgumentNullException("bone", "bone cannot be null.");
			bone.ModifyLocal(skeleton);
			BonePose p = bone.bone.parent.appliedPose;

			float pa = p.a, pb = p.b, pc = p.c, pd = p.d;
			float rotationIK = -bone.shearX - bone.rotation, tx, ty;
			switch (bone.inherit) {
			case Inherit.OnlyTranslation:
				tx = (targetX - bone.worldX) * Math.Sign(skeleton.ScaleX);
				ty = (targetY - bone.worldY) * Math.Sign(skeleton.ScaleY);
				break;
			case Inherit.NoRotationOrReflection: {
				float s = Math.Abs(pa * pd - pb * pc) / Math.Max(0.0001f, pa * pa + pc * pc);
				float sa = pa / skeleton.scaleX;
				float sc = pc / skeleton.ScaleY;
				pb = -sc * s * skeleton.scaleX;
				pd = sa * s * skeleton.ScaleY;
				rotationIK += MathUtils.Atan2Deg(sc, sa);
				goto default; // Fall through.
			}
			default: {
				float x = targetX - p.worldX, y = targetY - p.worldY;
				float d = pa * pd - pb * pc;
				if (Math.Abs(d) <= 0.0001f) {
					tx = 0;
					ty = 0;
				} else {
					tx = (x * pd - y * pb) / d - bone.x;
					ty = (y * pa - x * pc) / d - bone.y;
				}
				break;
			}
			}

			rotationIK += MathUtils.Atan2Deg(ty, tx);
			if (bone.scaleX < 0) rotationIK += 180;
			if (rotationIK > 180)
				rotationIK -= 360;
			else if (rotationIK < -180) //
				rotationIK += 360;
			bone.rotation += rotationIK * mix;
			if (compress || stretch) {
				switch (bone.inherit) {
				case Inherit.NoScale:
				case Inherit.NoScaleOrReflection:
					tx = targetX - bone.worldX;
					ty = targetY - bone.worldY;
					break;
				}
				float b = bone.bone.data.length * bone.scaleX;
				if (b > 0.0001f) {
					float dd = tx * tx + ty * ty;
					if ((compress && dd < b * b) || (stretch && dd > b * b)) {
						float s = ((float)Math.Sqrt(dd) / b - 1) * mix + 1;
						bone.scaleX *= s;
						if (uniform) bone.scaleY *= s;
					}
				}
			}
		}

		/// <summary>Applies 2 bone IK. The target is specified in the world coordinate system.</summary>
		/// <param name="child">A direct descendant of the parent bone.</param>
		static public void Apply (Skeleton skeleton, BonePose parent, BonePose child, float targetX, float targetY, int bendDir,
			bool stretch, bool uniform, float softness, float mix) {
			if (parent == null) throw new ArgumentNullException("parent", "parent cannot be null.");
			if (child == null) throw new ArgumentNullException("child", "child cannot be null.");
			if (parent.inherit != Inherit.Normal || child.inherit != Inherit.Normal) return;
			parent.ModifyLocal(skeleton);
			child.ModifyLocal(skeleton);
			float px = parent.x, py = parent.y, psx = parent.scaleX, psy = parent.scaleY, csx = child.scaleX;
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
			float cwx, cwy, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
			bool u = Math.Abs(psx - psy) <= 0.0001f;
			if (!u || stretch) {
				child.y = 0;
				cwx = a * child.x + parent.worldX;
				cwy = c * child.x + parent.worldY;
			} else {
				cwx = a * child.x + b * child.y + parent.worldX;
				cwy = c * child.x + d * child.y + parent.worldY;
			}
			BonePose pp = parent.bone.parent.appliedPose;
			a = pp.a;
			b = pp.b;
			c = pp.c;
			d = pp.d;
			float id = a * d - b * c, x = cwx - pp.worldX, y = cwy - pp.worldY;
			id = Math.Abs(id) <= 0.0001f ? 0 : 1 / id;
			float dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
			float l1 = (float)Math.Sqrt(dx * dx + dy * dy), l2 = child.bone.data.length * csx, a1, a2;
			if (l1 < 0.0001f) {
				Apply(skeleton, parent, targetX, targetY, false, stretch, false, mix);
				child.rotation = 0;
				return;
			}
			x = targetX - pp.worldX;
			y = targetY - pp.worldY;
			float tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
			float dd = tx * tx + ty * ty;
			if (softness != 0) {
				softness *= psx * (csx + 1) * 0.5f;
				float td = (float)Math.Sqrt(dd), sd = td - l1 - l2 * psx + softness;
				if (sd > 0) {
					float p = Math.Min(1, sd / (softness * 2)) - 1;
					p = (sd - softness * (1 - p * p)) / td;
					tx -= p * tx;
					ty -= p * ty;
					dd = tx * tx + ty * ty;
				}
			}
			if (u) {
				l2 *= psx;
				float cos = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
				if (cos < -1) {
					cos = -1;
					a2 = MathUtils.PI * bendDir;
				} else if (cos > 1) {
					cos = 1;
					a2 = 0;
					if (stretch) {
						a = ((float)Math.Sqrt(dd) / (l1 + l2) - 1) * mix + 1;
						parent.scaleX *= a;
						if (uniform) parent.scaleY *= a;
					}
				} else
					a2 = (float)Math.Acos(cos) * bendDir;
				a = l1 + l2 * cos;
				b = l2 * (float)Math.Sin(a2);
				a1 = (float)Math.Atan2(ty * a - tx * b, tx * a + ty * b);
			} else {
				a = psx * l2;
				b = psy * l2;
				float aa = a * a, bb = b * b, ta = (float)Math.Atan2(ty, tx);
				c = bb * l1 * l1 + aa * dd - aa * bb;
				float c1 = -2 * bb * l1, c2 = bb - aa;
				d = c1 * c1 - 4 * c2 * c;
				if (d >= 0) {
					float q = (float)Math.Sqrt(d);
					if (c1 < 0) q = -q;
					q = -(c1 + q) * 0.5f;
					float r0 = q / c2, r1 = c / q;
					float r = Math.Abs(r0) < Math.Abs(r1) ? r0 : r1;
					r0 = dd - r * r;
					if (r0 >= 0) {
						y = (float)Math.Sqrt(r0) * bendDir;
						a1 = ta - (float)Math.Atan2(y, r);
						a2 = (float)Math.Atan2(y / psy, (r - l1) / psx);
						goto break_outer; // break outer;
					}
				}
				float minAngle = MathUtils.PI, minX = l1 - a, minDist = minX * minX, minY = 0;
				float maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
				c = -a * l1 / (aa - bb);
				if (c >= -1 && c <= 1) {
					c = (float)Math.Acos(c);
					x = a * (float)Math.Cos(c) + l1;
					y = b * (float)Math.Sin(c);
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
				if (dd <= (minDist + maxDist) * 0.5f) {
					a1 = ta - (float)Math.Atan2(minY * bendDir, minX);
					a2 = minAngle * bendDir;
				} else {
					a1 = ta - (float)Math.Atan2(maxY * bendDir, maxX);
					a2 = maxAngle * bendDir;
				}
			}
			break_outer:
			float os = (float)Math.Atan2(child.y, child.x) * s2;
			a1 = (a1 - os) * MathUtils.RadDeg + os1 - parent.rotation;
			if (a1 > 180)
				a1 -= 360;
			else if (a1 < -180)
				a1 += 360;
			parent.rotation += a1 * mix;
			a2 = ((a2 + os) * MathUtils.RadDeg - child.shearX) * s2 + os2 - child.rotation;
			if (a2 > 180)
				a2 -= 360;
			else if (a2 < -180)
				a2 += 360;
			child.rotation += a2 * mix;
		}
	}
}
