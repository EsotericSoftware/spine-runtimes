/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	/// <summary>
	/// <para>
	/// Stores the current pose for an IK constraint. An IK constraint adjusts the rotation of 1 or 2 constrained bones so the tip of
	/// the last bone is as close to the target bone as possible.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-ik-constraints">IK constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class IkConstraint : IConstraint {
		internal IkConstraintData data;
		internal ExposedList<Bone> bones = new ExposedList<Bone>();
		internal Bone target;
		internal int bendDirection;
		internal bool compress, stretch;
		internal float mix = 1;

		public IkConstraint (IkConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			mix = data.mix;
			bendDirection = data.bendDirection;
			compress = data.compress;
			stretch = data.stretch;

			bones = new ExposedList<Bone>(data.bones.Count);
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.FindBone(boneData.name));
			target = skeleton.FindBone(data.target.name);
		}

		/// <summary>Copy constructor.</summary>
		public IkConstraint (IkConstraint constraint, Skeleton skeleton) {
			if (constraint == null) throw new ArgumentNullException("constraint cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			data = constraint.data;
			bones = new ExposedList<Bone>(constraint.Bones.Count);
			foreach (Bone bone in constraint.Bones)
				bones.Add(skeleton.Bones.Items[bone.data.index]);
			target = skeleton.Bones.Items[constraint.target.data.index];
			mix = constraint.mix;
			bendDirection = constraint.bendDirection;
			compress = constraint.compress;
			stretch = constraint.stretch;
		}

		/// <summary>Applies the constraint to the constrained bones.</summary>
		public void Apply () {
			Update();
		}

		public void Update () {
			Bone target = this.target;
			ExposedList<Bone> bones = this.bones;
			switch (bones.Count) {
			case 1:
				Apply(bones.Items[0], target.worldX, target.worldY, compress, stretch, data.uniform, mix);
				break;
			case 2:
				Apply(bones.Items[0], bones.Items[1], target.worldX, target.worldY, bendDirection, stretch, mix);
				break;
			}
		}


		public int Order {
			get { return data.order; }
		}

		/// <summary>The bones that will be modified by this IK constraint.</summary>
		public ExposedList<Bone> Bones {
			get { return bones; }
		}

		/// <summary>The bone that is the IK target.</summary>
		public Bone Target {
			get { return target; }
			set { target = value; }
		}

		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained rotations.</summary>
		public float Mix {
			get { return mix; }
			set { mix = value; }
		}

		/// <summary>Controls the bend direction of the IK bones, either 1 or -1.</summary>
		public int BendDirection {
			get { return bendDirection; }
			set { bendDirection = value; }
		}

		/// <summary>
		/// When true and only a single bone is being constrained, if the target is too close, the bone is scaled to reach it.</summary>
		public bool Compress {
			get { return compress; }
			set { compress = value; }
		}

		/// <summary>
		///  When true, if the target is out of range, the parent bone is scaled to reach it. If more than one bone is being constrained
		///  and the parent bone has local nonuniform scale, stretch is not applied.</summary>
		public bool Stretch {
			get { return stretch; }
			set { stretch = value; }
		}

		/// <summary>The IK constraint's setup pose data.</summary>
		public IkConstraintData Data {
			get { return data; }
		}

		override public string ToString () {
			return data.name;
		}

		/// <summary>Applies 1 bone IK. The target is specified in the world coordinate system.</summary>
		static public void Apply (Bone bone, float targetX, float targetY, bool compress, bool stretch, bool uniform,
								float alpha) {
			if (!bone.appliedValid) bone.UpdateAppliedTransform();
			Bone p = bone.parent;
			float id = 1 / (p.a * p.d - p.b * p.c);
			float x = targetX - p.worldX, y = targetY - p.worldY;
			float tx = (x * p.d - y * p.b) * id - bone.ax, ty = (y * p.a - x * p.c) * id - bone.ay;
			float rotationIK = (float)Math.Atan2(ty, tx) * MathUtils.RadDeg - bone.ashearX - bone.arotation;
			if (bone.ascaleX < 0) rotationIK += 180;
			if (rotationIK > 180)
				rotationIK -= 360;
			else if (rotationIK < -180) //
				rotationIK += 360;
			float sx = bone.ascaleX, sy = bone.ascaleY;
			if (compress || stretch) {
				float b = bone.data.length * sx, dd = (float)Math.Sqrt(tx * tx + ty * ty);
				if ((compress && dd < b) || (stretch && dd > b) && b > 0.0001f) {
					float s = (dd / b - 1) * alpha + 1;
					sx *= s;
					if (uniform) sy *= s;
				}
			}
			bone.UpdateWorldTransform(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, sx, sy, bone.ashearX, bone.ashearY);
		}

		/// <summary>Applies 2 bone IK. The target is specified in the world coordinate system.</summary>
		/// <param name="child">A direct descendant of the parent bone.</param>
		static public void Apply (Bone parent, Bone child, float targetX, float targetY, int bendDir, bool stretch, float alpha) {
			if (alpha == 0) {
				child.UpdateWorldTransform();
				return;
			}
			if (!parent.appliedValid) parent.UpdateAppliedTransform();
			if (!child.appliedValid) child.UpdateAppliedTransform();
			float px = parent.ax, py = parent.ay, psx = parent.ascaleX, sx = psx, psy = parent.ascaleY, csx = child.ascaleX;
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
			float tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py, dd = tx * tx + ty * ty;
			x = cwx - pp.worldX;
			y = cwy - pp.worldY;
			float dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
			float l1 = (float)Math.Sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1, a2;
			if (u) {
				l2 *= psx;
				float cos = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
				if (cos < -1)
					cos = -1;
				else if (cos > 1) {
					cos = 1;
					if (stretch && l1 + l2 > 0.0001f) sx *= ((float)Math.Sqrt(dd) / (l1 + l2) - 1) * alpha + 1;
				}
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
					q = -(c1 + q) / 2;
					float r0 = q / c2, r1 = c / q;
					float r = Math.Abs(r0) < Math.Abs(r1) ? r0 : r1;
					if (r * r <= dd) {
						y = (float)Math.Sqrt(dd - r * r) * bendDir;
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
				if (dd <= (minDist + maxDist) / 2) {
					a1 = ta - (float)Math.Atan2(minY * bendDir, minX);
					a2 = minAngle * bendDir;
				} else {
					a1 = ta - (float)Math.Atan2(maxY * bendDir, maxX);
					a2 = maxAngle * bendDir;
				}
			}
			break_outer:
			float os = (float)Math.Atan2(cy, cx) * s2;
			float rotation = parent.arotation;
			a1 = (a1 - os) * MathUtils.RadDeg + os1 - rotation;
			if (a1 > 180)
				a1 -= 360;
			else if (a1 < -180) a1 += 360;
			parent.UpdateWorldTransform(px, py, rotation + a1 * alpha, sx, parent.ascaleY, 0, 0);
			rotation = child.arotation;
			a2 = ((a2 + os) * MathUtils.RadDeg - child.ashearX) * s2 + os2 - rotation;
			if (a2 > 180)
				a2 -= 360;
			else if (a2 < -180) a2 += 360;
			child.UpdateWorldTransform(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
		}
	}
}
