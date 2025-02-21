
/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	using Physics = Skeleton.Physics;

	/// <summary>
	/// Stores the current pose for a physics constraint. A physics constraint applies physics to bones.
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class PhysicsConstraint : IUpdatable {
		internal readonly PhysicsConstraintData data;
		public Bone bone;
		internal float inertia, strength, damping, massInverse, wind, gravity, mix;

		bool reset = true;
		float ux, uy, cx, cy, tx, ty;
		float xOffset, xVelocity;
		float yOffset, yVelocity;
		float rotateOffset, rotateVelocity;
		float scaleOffset, scaleVelocity;

		internal bool active;

		readonly Skeleton skeleton;
		float remaining, lastTime;

		public PhysicsConstraint (PhysicsConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;

			bone = skeleton.bones.Items[data.bone.index];

			inertia = data.inertia;
			strength = data.strength;
			damping = data.damping;
			massInverse = data.massInverse;
			wind = data.wind;
			gravity = data.gravity;
			mix = data.mix;
		}

		/// <summary>Copy constructor.</summary>
		public PhysicsConstraint (PhysicsConstraint constraint, Skeleton skeleton)
			: this(constraint.data, skeleton) {

			inertia = constraint.inertia;
			strength = constraint.strength;
			damping = constraint.damping;
			massInverse = constraint.massInverse;
			wind = constraint.wind;
			gravity = constraint.gravity;
			mix = constraint.mix;
		}

		public void Reset () {
			remaining = 0;
			lastTime = skeleton.time;
			reset = true;
			xOffset = 0;
			xVelocity = 0;
			yOffset = 0;
			yVelocity = 0;
			rotateOffset = 0;
			rotateVelocity = 0;
			scaleOffset = 0;
			scaleVelocity = 0;
		}

		public void SetToSetupPose () {
			PhysicsConstraintData data = this.data;
			inertia = data.inertia;
			strength = data.strength;
			damping = data.damping;
			massInverse = data.massInverse;
			wind = data.wind;
			gravity = data.gravity;
			mix = data.mix;
		}

		/// <summary>
		/// Translates the physics constraint so next <see cref="Update(Physics)"/> forces are applied as if the bone moved an additional
		/// amount in world space.
		/// </summary>
		public void Translate (float x, float y) {
			ux -= x;
			uy -= y;
			cx -= x;
			cy -= y;
		}

		/// <summary>
		/// Rotates the physics constraint so next <see cref="Update(Physics)"/> forces are applied as if the bone rotated around the
		/// specified point in world space.
		/// </summary>
		public void Rotate (float x, float y, float degrees) {
			float r = degrees * MathUtils.DegRad, cos = (float)Math.Cos(r), sin = (float)Math.Sin(r);
			float dx = cx - x, dy = cy - y;
			Translate(dx * cos - dy * sin - dx, dx * sin + dy * cos - dy);
		}

		/// <summary>Applies the constraint to the constrained bones.</summary>
		public void Update (Physics physics) {
			float mix = this.mix;
			if (mix == 0) return;

			bool x = data.x > 0, y = data.y > 0, rotateOrShearX = data.rotate > 0 || data.shearX > 0, scaleX = data.scaleX > 0;
			Bone bone = this.bone;
			float l = bone.data.length;

			switch (physics) {
			case Physics.None:
				return;
			case Physics.Reset:
				Reset();
				goto case Physics.Update; // Fall through.
			case Physics.Update:
				Skeleton skeleton = this.skeleton;
				float delta = Math.Max(skeleton.time - lastTime, 0);
				remaining += delta;
				lastTime = skeleton.time;

				float bx = bone.worldX, by = bone.worldY;
				if (reset) {
					reset = false;
					ux = bx;
					uy = by;
				} else {
					float a = remaining, i = inertia, t = data.step, f = skeleton.data.referenceScale, d = -1;
					float qx = data.limit * delta, qy = qx * Math.Abs(skeleton.ScaleY);
					qx *= Math.Abs(skeleton.ScaleX);

					if (x || y) {
						if (x) {
							float u = (ux - bx) * i;
							xOffset += u > qx ? qx : u < -qx ? -qx : u;
							ux = bx;
						}
						if (y) {
							float u = (uy - by) * i;
							yOffset += u > qy ? qy : u < -qy ? -qy : u;
							uy = by;
						}
						if (a >= t) {
							d = (float)Math.Pow(damping, 60 * t);
							float m = massInverse * t, e = strength, w = wind * f * skeleton.ScaleX, g = gravity * f * skeleton.ScaleY;
							do {
								if (x) {
									xVelocity += (w - xOffset * e) * m;
									xOffset += xVelocity * t;
									xVelocity *= d;
								}
								if (y) {
									yVelocity -= (g + yOffset * e) * m;
									yOffset += yVelocity * t;
									yVelocity *= d;
								}
								a -= t;
							} while (a >= t);
						}
						if (x) bone.worldX += xOffset * mix * data.x;
						if (y) bone.worldY += yOffset * mix * data.y;
					}
					if (rotateOrShearX || scaleX) {
						float ca = (float)Math.Atan2(bone.c, bone.a), c, s, mr = 0;
						float dx = cx - bone.worldX, dy = cy - bone.worldY;
						if (dx > qx)
							dx = qx;
						else if (dx < -qx)
							dx = -qx;
						if (dy > qy)
							dy = qy;
						else if (dy < -qy)
							dy = -qy;
						if (rotateOrShearX) {
							mr = (data.rotate + data.shearX) * mix;
							float r = (float)Math.Atan2(dy + ty, dx + tx) - ca - rotateOffset * mr;
							rotateOffset += (r - (float)Math.Ceiling(r * MathUtils.InvPI2 - 0.5f) * MathUtils.PI2) * i;
							r = rotateOffset * mr + ca;
							c = (float)Math.Cos(r);
							s = (float)Math.Sin(r);
							if (scaleX) {
								r = l * bone.WorldScaleX;
								if (r > 0) scaleOffset += (dx * c + dy * s) * i / r;
							}
						} else {
							c = (float)Math.Cos(ca);
							s = (float)Math.Sin(ca);
							float r = l * bone.WorldScaleX;
							if (r > 0) scaleOffset += (dx * c + dy * s) * i / r;
						}
						a = remaining;
						if (a >= t) {
							if (d == -1) d = (float)Math.Pow(damping, 60 * t);
							float m = massInverse * t, e = strength, w = wind, g = (Bone.yDown ? -gravity : gravity), h = l / f;
							while (true) {
								a -= t;
								if (scaleX) {
									scaleVelocity += (w * c - g * s - scaleOffset * e) * m;
									scaleOffset += scaleVelocity * t;
									scaleVelocity *= d;
								}
								if (rotateOrShearX) {
									rotateVelocity -= ((w * s + g * c) * h + rotateOffset * e) * m;
									rotateOffset += rotateVelocity * t;
									rotateVelocity *= d;
									if (a < t) break;
									float r = rotateOffset * mr + ca;
									c = (float)Math.Cos(r);
									s = (float)Math.Sin(r);
								} else if (a < t) //
									break;
							}
						}
					}
					remaining = a;
				}
				cx = bone.worldX;
				cy = bone.worldY;
				break;
			case Physics.Pose:
				if (x) bone.worldX += xOffset * mix * data.x;
				if (y) bone.worldY += yOffset * mix * data.y;
				break;
			}

			if (rotateOrShearX) {
				float o = rotateOffset * mix, s, c, a;
				if (data.shearX > 0) {
					float r = 0;
					if (data.rotate > 0) {
						r = o * data.rotate;
						s = (float)Math.Sin(r);
						c = (float)Math.Cos(r);
						a = bone.b;
						bone.b = c * a - s * bone.d;
						bone.d = s * a + c * bone.d;
					}
					r += o * data.shearX;
					s = (float)Math.Sin(r);
					c = (float)Math.Cos(r);
					a = bone.a;
					bone.a = c * a - s * bone.c;
					bone.c = s * a + c * bone.c;
				} else {
					o *= data.rotate;
					s = (float)Math.Sin(o);
					c = (float)Math.Cos(o);
					a = bone.a;
					bone.a = c * a - s * bone.c;
					bone.c = s * a + c * bone.c;
					a = bone.b;
					bone.b = c * a - s * bone.d;
					bone.d = s * a + c * bone.d;
				}
			}
			if (scaleX) {
				float s = 1 + scaleOffset * mix * data.scaleX;
				bone.a *= s;
				bone.c *= s;
			}
			if (physics != Physics.Pose) {
				tx = l * bone.a;
				ty = l * bone.c;
			}
			bone.UpdateAppliedTransform();
		}

		/// <summary>The bone constrained by this physics constraint.</summary>
		public Bone Bone { get { return bone; } set { bone = value; } }
		public float Inertia { get { return inertia; } set { inertia = value; } }
		public float Strength { get { return strength; } set { strength = value; } }
		public float Damping { get { return damping; } set { damping = value; } }
		public float MassInverse { get { return massInverse; } set { massInverse = value; } }
		public float Wind { get { return wind; } set { wind = value; } }
		public float Gravity { get { return gravity; } set { gravity = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained poses.</summary>
		public float Mix { get { return mix; } set { mix = value; } }
		public bool Active { get { return active; } }


		/// <summary>The physics constraint's setup pose data.</summary>
		public PhysicsConstraintData getData () {
			return data;
		}

		/// <summary>The physics constraint's setup pose data.</summary>
		public PhysicsConstraintData Data { get { return data; } }

		override public string ToString () {
			return data.name;
		}
	}
}
