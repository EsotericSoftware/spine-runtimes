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
	/// Stores a bone's current pose.
	/// <para>
	/// A bone has a local transform which is used to compute its world transform. A bone also has an applied transform, which is a
	/// local transform that can be applied to compute the world transform. The local transform and applied transform may differ if a
	/// constraint or application code modifies the world transform after it was computed from the local transform.
	/// </para>
	/// </summary>
	public class Bone : IUpdatable {
		static public bool yDown;

		internal BoneData data;
		internal Skeleton skeleton;
		internal Bone parent;
		internal ExposedList<Bone> children = new ExposedList<Bone>();
		internal float x, y, rotation, scaleX, scaleY, shearX, shearY;
		internal float ax, ay, arotation, ascaleX, ascaleY, ashearX, ashearY;

		internal float a, b, worldX;
		internal float c, d, worldY;
		internal Inherit inherit;

		internal bool sorted, active;

		public BoneData Data { get { return data; } }
		public Skeleton Skeleton { get { return skeleton; } }
		public Bone Parent { get { return parent; } }
		public ExposedList<Bone> Children { get { return children; } }
		public bool Active { get { return active; } }
		/// <summary>The local X translation.</summary>
		public float X { get { return x; } set { x = value; } }
		/// <summary>The local Y translation.</summary>
		public float Y { get { return y; } set { y = value; } }
		/// <summary>The local rotation.</summary>
		public float Rotation { get { return rotation; } set { rotation = value; } }

		/// <summary>The local scaleX.</summary>
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }

		/// <summary>The local scaleY.</summary>
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }

		/// <summary>The local shearX.</summary>
		public float ShearX { get { return shearX; } set { shearX = value; } }

		/// <summary>The local shearY.</summary>
		public float ShearY { get { return shearY; } set { shearY = value; } }

		/// <summary>Controls how parent world transforms affect this bone.</summary>
		public Inherit Inherit { get { return inherit; } set { inherit = value; } }

		/// <summary>The rotation, as calculated by any constraints.</summary>
		public float AppliedRotation { get { return arotation; } set { arotation = value; } }

		/// <summary>The applied local x translation.</summary>
		public float AX { get { return ax; } set { ax = value; } }

		/// <summary>The applied local y translation.</summary>
		public float AY { get { return ay; } set { ay = value; } }

		/// <summary>The applied local scaleX.</summary>
		public float AScaleX { get { return ascaleX; } set { ascaleX = value; } }

		/// <summary>The applied local scaleY.</summary>
		public float AScaleY { get { return ascaleY; } set { ascaleY = value; } }

		/// <summary>The applied local shearX.</summary>
		public float AShearX { get { return ashearX; } set { ashearX = value; } }

		/// <summary>The applied local shearY.</summary>
		public float AShearY { get { return ashearY; } set { ashearY = value; } }

		/// <summary>Part of the world transform matrix for the X axis. If changed, <see cref="UpdateAppliedTransform()"/> should be called.</summary>
		public float A { get { return a; } set { a = value; } }
		/// <summary>Part of the world transform matrix for the Y axis. If changed, <see cref="UpdateAppliedTransform()"/> should be called.</summary>
		public float B { get { return b; } set { b = value; } }
		/// <summary>Part of the world transform matrix for the X axis. If changed, <see cref="UpdateAppliedTransform()"/> should be called.</summary>
		public float C { get { return c; } set { c = value; } }
		/// <summary>Part of the world transform matrix for the Y axis. If changed, <see cref="UpdateAppliedTransform()"/> should be called.</summary>
		public float D { get { return d; } set { d = value; } }

		/// <summary>The world X position. If changed, <see cref="UpdateAppliedTransform()"/> should be called.</summary>
		public float WorldX { get { return worldX; } set { worldX = value; } }
		/// <summary>The world Y position. If changed, <see cref="UpdateAppliedTransform()"/> should be called.</summary>
		public float WorldY { get { return worldY; } set { worldY = value; } }
		/// <summary>The world rotation for the X axis, calculated using <see cref="a"/> and <see cref="c"/>.</summary>
		public float WorldRotationX { get { return MathUtils.Atan2Deg(c, a); } }
		/// <summary>The world rotation for the Y axis, calculated using <see cref="b"/> and <see cref="d"/>.</summary>
		public float WorldRotationY { get { return MathUtils.Atan2Deg(d, b); } }

		/// <summary>Returns the magnitide (always positive) of the world scale X.</summary>
		public float WorldScaleX { get { return (float)Math.Sqrt(a * a + c * c); } }
		/// <summary>Returns the magnitide (always positive) of the world scale Y.</summary>
		public float WorldScaleY { get { return (float)Math.Sqrt(b * b + d * d); } }

		public Bone (BoneData data, Skeleton skeleton, Bone parent) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			SetToSetupPose();
		}

		/// <summary>Copy constructor. Does not copy the <see cref="Children"/> bones.</summary>
		/// <param name="parent">May be null.</param>
		public Bone (Bone bone, Skeleton skeleton, Bone parent) {
			if (bone == null) throw new ArgumentNullException("bone", "bone cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.skeleton = skeleton;
			this.parent = parent;
			data = bone.data;
			x = bone.x;
			y = bone.y;
			rotation = bone.rotation;
			scaleX = bone.scaleX;
			scaleY = bone.scaleY;
			shearX = bone.shearX;
			shearY = bone.shearY;
			inherit = bone.inherit;
		}

		/// <summary>Computes the world transform using the parent bone and this bone's local applied transform.</summary>
		public void Update (Physics physics) {
			UpdateWorldTransform(ax, ay, arotation, ascaleX, ascaleY, ashearX, ashearY);
		}

		/// <summary>Computes the world transform using the parent bone and this bone's local transform.</summary>
		public void UpdateWorldTransform () {
			UpdateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
		}

		/// <summary>Computes the world transform using the parent bone and the specified local transform. The applied transform is set to the
		/// specified local transform. Child bones are not updated.
		/// <para>
		/// See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
		/// Runtimes Guide.</para></summary>
		public void UpdateWorldTransform (float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY) {
			ax = x;
			ay = y;
			arotation = rotation;
			ascaleX = scaleX;
			ascaleY = scaleY;
			ashearX = shearX;
			ashearY = shearY;

			Bone parent = this.parent;
			if (parent == null) { // Root bone.
				Skeleton skeleton = this.skeleton;
				float sx = skeleton.scaleX, sy = skeleton.ScaleY;
				float rx = (rotation + shearX) * MathUtils.DegRad;
				float ry = (rotation + 90 + shearY) * MathUtils.DegRad;
				a = (float)Math.Cos(rx) * scaleX * sx;
				b = (float)Math.Cos(ry) * scaleY * sx;
				c = (float)Math.Sin(rx) * scaleX * sy;
				d = (float)Math.Sin(ry) * scaleY * sy;
				worldX = x * sx + skeleton.x;
				worldY = y * sy + skeleton.y;
				return;
			}

			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			worldX = pa * x + pb * y + parent.worldX;
			worldY = pc * x + pd * y + parent.worldY;

			switch (inherit) {
			case Inherit.Normal: {
				float rx = (rotation + shearX) * MathUtils.DegRad;
				float ry = (rotation + 90 + shearY) * MathUtils.DegRad;
				float la = (float)Math.Cos(rx) * scaleX;
				float lb = (float)Math.Cos(ry) * scaleY;
				float lc = (float)Math.Sin(rx) * scaleX;
				float ld = (float)Math.Sin(ry) * scaleY;
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
				return;
			}
			case Inherit.OnlyTranslation: {
				float rx = (rotation + shearX) * MathUtils.DegRad;
				float ry = (rotation + 90 + shearY) * MathUtils.DegRad;
				a = (float)Math.Cos(rx) * scaleX;
				b = (float)Math.Cos(ry) * scaleY;
				c = (float)Math.Sin(rx) * scaleX;
				d = (float)Math.Sin(ry) * scaleY;
				break;
			}
			case Inherit.NoRotationOrReflection: {
				float sx = 1 / skeleton.scaleX, sy = 1 / skeleton.ScaleY;
				pa *= sx;
				pc *= sy;
				float s = pa * pa + pc * pc, prx;
				if (s > 0.0001f) {
					s = Math.Abs(pa * pd * sy - pb * sx * pc) / s;
					pb = pc * s;
					pd = pa * s;
					prx = MathUtils.Atan2Deg(pc, pa);
				} else {
					pa = 0;
					pc = 0;
					prx = 90 - MathUtils.Atan2Deg(pd, pb);
				}
				float rx = (rotation + shearX - prx) * MathUtils.DegRad;
				float ry = (rotation + shearY - prx + 90) * MathUtils.DegRad;
				float la = (float)Math.Cos(rx) * scaleX;
				float lb = (float)Math.Cos(ry) * scaleY;
				float lc = (float)Math.Sin(rx) * scaleX;
				float ld = (float)Math.Sin(ry) * scaleY;
				a = pa * la - pb * lc;
				b = pa * lb - pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
				break;
			}
			case Inherit.NoScale:
			case Inherit.NoScaleOrReflection: {
				rotation *= MathUtils.DegRad;
				float cos = (float)Math.Cos(rotation), sin = (float)Math.Sin(rotation);
				float za = (pa * cos + pb * sin) / skeleton.scaleX;
				float zc = (pc * cos + pd * sin) / skeleton.ScaleY;
				float s = (float)Math.Sqrt(za * za + zc * zc);
				if (s > 0.00001f) s = 1 / s;
				za *= s;
				zc *= s;
				s = (float)Math.Sqrt(za * za + zc * zc);
				if (inherit == Inherit.NoScale && (pa * pd - pb * pc < 0) != (skeleton.scaleX < 0 != skeleton.ScaleY < 0)) s = -s;
				rotation = MathUtils.PI / 2 + MathUtils.Atan2(zc, za);
				float zb = (float)Math.Cos(rotation) * s;
				float zd = (float)Math.Sin(rotation) * s;
				shearX *= MathUtils.DegRad;
				shearY = (90 + shearY) * MathUtils.DegRad;
				float la = (float)Math.Cos(shearX) * scaleX;
				float lb = (float)Math.Cos(shearY) * scaleY;
				float lc = (float)Math.Sin(shearX) * scaleX;
				float ld = (float)Math.Sin(shearY) * scaleY;
				a = za * la + zb * lc;
				b = za * lb + zb * ld;
				c = zc * la + zd * lc;
				d = zc * lb + zd * ld;
				break;
			}
			}
			a *= skeleton.scaleX;
			b *= skeleton.scaleX;
			c *= skeleton.ScaleY;
			d *= skeleton.ScaleY;
		}

		/// <summary>Sets this bone's local transform to the setup pose.</summary>
		public void SetToSetupPose () {
			BoneData data = this.data;
			x = data.x;
			y = data.y;
			rotation = data.rotation;
			scaleX = data.scaleX;
			scaleY = data.ScaleY;
			shearX = data.shearX;
			shearY = data.shearY;
			inherit = data.inherit;
		}

		/// <summary>
		/// Computes the applied transform values from the world transform.
		/// <para>
		/// If the world transform is modified (by a constraint, <see cref="RotateWorld(float)"/>, etc) then this method should be called so
		/// the applied transform matches the world transform. The applied transform may be needed by other code (eg to apply another
		/// constraint).
		/// </para><para>
		///  Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The applied transform after
		/// calling this method is equivalent to the local transform used to compute the world transform, but may not be identical.
		/// </para></summary>
		public void UpdateAppliedTransform () {
			Bone parent = this.parent;
			if (parent == null) {
				ax = worldX - skeleton.x;
				ay = worldY - skeleton.y;
				float a = this.a, b = this.b, c = this.c, d = this.d;
				arotation = MathUtils.Atan2Deg(c, a);
				ascaleX = (float)Math.Sqrt(a * a + c * c);
				ascaleY = (float)Math.Sqrt(b * b + d * d);
				ashearX = 0;
				ashearY = MathUtils.Atan2Deg(a * b + c * d, a * d - b * c);
				return;
			}

			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			float pid = 1 / (pa * pd - pb * pc);
			float ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
			float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
			ax = (dx * ia - dy * ib);
			ay = (dy * id - dx * ic);

			float ra, rb, rc, rd;
			if (inherit == Inherit.OnlyTranslation) {
				ra = a;
				rb = b;
				rc = c;
				rd = d;
			} else {
				switch (inherit) {
				case Inherit.NoRotationOrReflection: {
					float s = Math.Abs(pa * pd - pb * pc) / (pa * pa + pc * pc);
					float skeletonScaleY = skeleton.ScaleY;
					pb = -pc * skeleton.scaleX * s / skeletonScaleY;
					pd = pa * skeletonScaleY * s / skeleton.scaleX;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
					break;
				}
				case Inherit.NoScale:
				case Inherit.NoScaleOrReflection: {
					float r = rotation * MathUtils.DegRad, cos = (float)Math.Cos(r), sin = (float)Math.Sin(r);
					pa = (pa * cos + pb * sin) / skeleton.scaleX;
					pc = (pc * cos + pd * sin) / skeleton.ScaleY;
					float s = (float)Math.Sqrt(pa * pa + pc * pc);
					if (s > 0.00001f) s = 1 / s;
					pa *= s;
					pc *= s;
					s = (float)Math.Sqrt(pa * pa + pc * pc);
					if (inherit == Inherit.NoScale && pid < 0 != (skeleton.scaleX < 0 != skeleton.ScaleY < 0)) s = -s;
					r = MathUtils.PI / 2 + MathUtils.Atan2(pc, pa);
					pb = (float)Math.Cos(r) * s;
					pd = (float)Math.Sin(r) * s;
					pid = 1 / (pa * pd - pb * pc);
					ia = pd * pid;
					ib = pb * pid;
					ic = pc * pid;
					id = pa * pid;
					break;
				}
				}
				ra = ia * a - ib * c;
				rb = ia * b - ib * d;
				rc = id * c - ic * a;
				rd = id * d - ic * b;
			}

			ashearX = 0;
			ascaleX = (float)Math.Sqrt(ra * ra + rc * rc);
			if (ascaleX > 0.0001f) {
				float det = ra * rd - rb * rc;
				ascaleY = det / ascaleX;
				ashearY = -MathUtils.Atan2Deg(ra * rb + rc * rd, det);
				arotation = MathUtils.Atan2Deg(rc, ra);
			} else {
				ascaleX = 0;
				ascaleY = (float)Math.Sqrt(rb * rb + rd * rd);
				ashearY = 0;
				arotation = 90 - MathUtils.Atan2Deg(rd, rb);
			}
		}

		/// <summary>Transforms a point from world coordinates to the bone's local coordinates.</summary>
		public void WorldToLocal (float worldX, float worldY, out float localX, out float localY) {
			float a = this.a, b = this.b, c = this.c, d = this.d;
			float det = a * d - b * c;
			float x = worldX - this.worldX, y = worldY - this.worldY;
			localX = (x * d - y * b) / det;
			localY = (y * a - x * c) / det;
		}

		/// <summary>Transforms a point from the bone's local coordinates to world coordinates.</summary>
		public void LocalToWorld (float localX, float localY, out float worldX, out float worldY) {
			worldX = localX * a + localY * b + this.worldX;
			worldY = localX * c + localY * d + this.worldY;
		}

		/// <summary>Transforms a point from world coordinates to the parent bone's local coordinates.</summary>
		public void WorldToParent (float worldX, float worldY, out float parentX, out float parentY) {
			if (parent == null) {
				parentX = worldX;
				parentY = worldY;
			} else {
				parent.WorldToLocal(worldX, worldY, out parentX, out parentY);
			}
		}

		/// <summary>Transforms a point from the parent bone's coordinates to world coordinates.</summary>
		public void ParentToWorld (float parentX, float parentY, out float worldX, out float worldY) {
			if (parent == null) {
				worldX = parentX;
				worldY = parentY;
			} else {
				parent.LocalToWorld(parentX, parentY, out worldX, out worldY);
			}
		}

		/// <summary>Transforms a world rotation to a local rotation.</summary>
		public float WorldToLocalRotation (float worldRotation) {
			worldRotation *= MathUtils.DegRad;
			float sin = (float)Math.Sin(worldRotation), cos = (float)Math.Cos(worldRotation);
			return MathUtils.Atan2Deg(a * sin - c * cos, d * cos - b * sin) + rotation - shearX;
		}

		/// <summary>Transforms a local rotation to a world rotation.</summary>
		public float LocalToWorldRotation (float localRotation) {
			localRotation = (localRotation - rotation - shearX) * MathUtils.DegRad;
			float sin = (float)Math.Sin(localRotation), cos = (float)Math.Cos(localRotation);
			return MathUtils.Atan2Deg(cos * c + sin * d, cos * a + sin * b);
		}

		/// <summary>
		/// Rotates the world transform the specified amount.
		/// <para>
		/// After changes are made to the world transform, <see cref="UpdateAppliedTransform()"/> should be called and
		/// <see cref="Update(Skeleton.Physics)"/> will need to be called on any child bones, recursively.
		/// </para></summary>
		public void RotateWorld (float degrees) {
			degrees *= MathUtils.DegRad;
			float sin = (float)Math.Sin(degrees), cos = (float)Math.Cos(degrees);
			float ra = a, rb = b;
			a = cos * ra - sin * c;
			b = cos * rb - sin * d;
			c = sin * ra + cos * c;
			d = sin * rb + cos * d;
		}

		override public string ToString () {
			return data.name;
		}
	}
}
