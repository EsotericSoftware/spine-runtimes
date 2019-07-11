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
		internal bool appliedValid;

		internal float a, b, worldX;
		internal float c, d, worldY;

		internal bool sorted, active;

		public BoneData Data { get { return data; } }
		public Skeleton Skeleton { get { return skeleton; } }
		public Bone Parent { get { return parent; } }
		public ExposedList<Bone> Children { get { return children; } }
		/// <summary>Returns false when the bone has not been computed because <see cref="BoneData.SkinRequired"/> is true and the
		/// <see cref="Skeleton.Skin">active skin</see> does not <see cref="Skin.Bones">contain</see> this bone.</summary>
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

		public float A { get { return a; } }
		public float B { get { return b; } }
		public float C { get { return c; } }
		public float D { get { return d; } }

		public float WorldX { get { return worldX; } }
		public float WorldY { get { return worldY; } }
		public float WorldRotationX { get { return MathUtils.Atan2(c, a) * MathUtils.RadDeg; } }
		public float WorldRotationY { get { return MathUtils.Atan2(d, b) * MathUtils.RadDeg; } }

		/// <summary>Returns the magnitide (always positive) of the world scale X.</summary>
		public float WorldScaleX { get { return (float)Math.Sqrt(a * a + c * c); } }
		/// <summary>Returns the magnitide (always positive) of the world scale Y.</summary>
		public float WorldScaleY { get { return (float)Math.Sqrt(b * b + d * d); } }

		/// <param name="parent">May be null.</param>
		public Bone (BoneData data, Skeleton skeleton, Bone parent) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			SetToSetupPose();
		}

		/// <summary>Same as <see cref="UpdateWorldTransform"/>. This method exists for Bone to implement <see cref="Spine.IUpdatable"/>.</summary>
		public void Update () {
			UpdateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
		}

		/// <summary>Computes the world transform using the parent bone and this bone's local transform.</summary>
		public void UpdateWorldTransform () {
			UpdateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
		}

		/// <summary>Computes the world transform using the parent bone and the specified local transform.</summary>
		public void UpdateWorldTransform (float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY) {
			ax = x;
			ay = y;
			arotation = rotation;
			ascaleX = scaleX;
			ascaleY = scaleY;
			ashearX = shearX;
			ashearY = shearY;
			appliedValid = true;
			Skeleton skeleton = this.skeleton;

			Bone parent = this.parent;
			if (parent == null) { // Root bone.
				float rotationY = rotation + 90 + shearY, sx = skeleton.ScaleX, sy = skeleton.ScaleY;
				a = MathUtils.CosDeg(rotation + shearX) * scaleX * sx;
				b = MathUtils.CosDeg(rotationY) * scaleY * sx;
				c = MathUtils.SinDeg(rotation + shearX) * scaleX * sy;
				d = MathUtils.SinDeg(rotationY) * scaleY * sy;
				worldX = x * sx + skeleton.x;
				worldY = y * sy + skeleton.y;
				return;
			}

			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			worldX = pa * x + pb * y + parent.worldX;
			worldY = pc * x + pd * y + parent.worldY;

			switch (data.transformMode) {
			case TransformMode.Normal: {
					float rotationY = rotation + 90 + shearY;
					float la = MathUtils.CosDeg(rotation + shearX) * scaleX;
					float lb = MathUtils.CosDeg(rotationY) * scaleY;
					float lc = MathUtils.SinDeg(rotation + shearX) * scaleX;
					float ld = MathUtils.SinDeg(rotationY) * scaleY;
					a = pa * la + pb * lc;
					b = pa * lb + pb * ld;
					c = pc * la + pd * lc;
					d = pc * lb + pd * ld;
					return;
				}
			case TransformMode.OnlyTranslation: {
					float rotationY = rotation + 90 + shearY;
					a = MathUtils.CosDeg(rotation + shearX) * scaleX;
					b = MathUtils.CosDeg(rotationY) * scaleY;
					c = MathUtils.SinDeg(rotation + shearX) * scaleX;
					d = MathUtils.SinDeg(rotationY) * scaleY;
					break;
				}
			case TransformMode.NoRotationOrReflection: {
					float s = pa * pa + pc * pc, prx;
					if (s > 0.0001f) {
						s = Math.Abs(pa * pd - pb * pc) / s;
						pb = pc * s;
						pd = pa * s;
						prx = MathUtils.Atan2(pc, pa) * MathUtils.RadDeg;
					} else {
						pa = 0;
						pc = 0;
						prx = 90 - MathUtils.Atan2(pd, pb) * MathUtils.RadDeg;
					}
					float rx = rotation + shearX - prx;
					float ry = rotation + shearY - prx + 90;
					float la = MathUtils.CosDeg(rx) * scaleX;
					float lb = MathUtils.CosDeg(ry) * scaleY;
					float lc = MathUtils.SinDeg(rx) * scaleX;
					float ld = MathUtils.SinDeg(ry) * scaleY;
					a = pa * la - pb * lc;
					b = pa * lb - pb * ld;
					c = pc * la + pd * lc;
					d = pc * lb + pd * ld;
					break;
				}
			case TransformMode.NoScale:
			case TransformMode.NoScaleOrReflection: {
					float cos = MathUtils.CosDeg(rotation), sin = MathUtils.SinDeg(rotation);
					float za = (pa * cos + pb * sin) / skeleton.ScaleX;
					float zc = (pc * cos + pd * sin) / skeleton.ScaleY;
					float s = (float)Math.Sqrt(za * za + zc * zc);
					if (s > 0.00001f) s = 1 / s;
					za *= s;
					zc *= s;
					s = (float)Math.Sqrt(za * za + zc * zc);
					if (data.transformMode == TransformMode.NoScale
						&& (pa * pd - pb * pc < 0) != (skeleton.ScaleX < 0 != skeleton.ScaleY < 0)) s = -s;

					float r = MathUtils.PI / 2 + MathUtils.Atan2(zc, za);
					float zb = MathUtils.Cos(r) * s;
					float zd = MathUtils.Sin(r) * s;
					float la = MathUtils.CosDeg(shearX) * scaleX;
					float lb = MathUtils.CosDeg(90 + shearY) * scaleY;
					float lc = MathUtils.SinDeg(shearX) * scaleX;
					float ld = MathUtils.SinDeg(90 + shearY) * scaleY;
					a = za * la + zb * lc;
					b = za * lb + zb * ld;
					c = zc * la + zd * lc;
					d = zc * lb + zd * ld;
					break;
				}
			}

			a *= skeleton.ScaleX;
			b *= skeleton.ScaleX;
			c *= skeleton.ScaleY;
			d *= skeleton.ScaleY;
		}

		public void SetToSetupPose () {
			BoneData data = this.data;
			x = data.x;
			y = data.y;
			rotation = data.rotation;
			scaleX = data.scaleX;
			scaleY = data.scaleY;
			shearX = data.shearX;
			shearY = data.shearY;
		}

		/// <summary>
		/// Computes the individual applied transform values from the world transform. This can be useful to perform processing using
		/// the applied transform after the world transform has been modified directly (eg, by a constraint)..
		/// 
		/// Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation.
		/// </summary>
		internal void UpdateAppliedTransform () {
			appliedValid = true;
			Bone parent = this.parent;
			if (parent == null) {
				ax = worldX;
				ay = worldY;
				arotation = MathUtils.Atan2(c, a) * MathUtils.RadDeg;
				ascaleX = (float)Math.Sqrt(a * a + c * c);
				ascaleY = (float)Math.Sqrt(b * b + d * d);
				ashearX = 0;
				ashearY = MathUtils.Atan2(a * b + c * d, a * d - b * c) * MathUtils.RadDeg;
				return;
			}
			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			float pid = 1 / (pa * pd - pb * pc);
			float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
			ax = (dx * pd * pid - dy * pb * pid);
			ay = (dy * pa * pid - dx * pc * pid);
			float ia = pid * pd;
			float id = pid * pa;
			float ib = pid * pb;
			float ic = pid * pc;
			float ra = ia * a - ib * c;
			float rb = ia * b - ib * d;
			float rc = id * c - ic * a;
			float rd = id * d - ic * b;
			ashearX = 0;
			ascaleX = (float)Math.Sqrt(ra * ra + rc * rc);
			if (ascaleX > 0.0001f) {
				float det = ra * rd - rb * rc;
				ascaleY = det / ascaleX;
				ashearY = MathUtils.Atan2(ra * rb + rc * rd, det) * MathUtils.RadDeg;
				arotation = MathUtils.Atan2(rc, ra) * MathUtils.RadDeg;
			} else {
				ascaleX = 0;
				ascaleY = (float)Math.Sqrt(rb * rb + rd * rd);
				ashearY = 0;
				arotation = 90 - MathUtils.Atan2(rd, rb) * MathUtils.RadDeg;
			}
		}

		public void WorldToLocal (float worldX, float worldY, out float localX, out float localY) {			
			float a = this.a, b = this.b, c = this.c, d = this.d;
			float invDet = 1 / (a * d - b * c);
			float x = worldX - this.worldX, y = worldY - this.worldY;
			localX = (x * d * invDet - y * b * invDet);
			localY = (y * a * invDet - x * c * invDet);
		}

		public void LocalToWorld (float localX, float localY, out float worldX, out float worldY) {
			worldX = localX * a + localY * b + this.worldX;
			worldY = localX * c + localY * d + this.worldY;
		}

		public float WorldToLocalRotationX {
			get {
				Bone parent = this.parent;
				if (parent == null) return arotation;
				float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, a = this.a, c = this.c;
				return MathUtils.Atan2(pa * c - pc * a, pd * a - pb * c) * MathUtils.RadDeg;
			}
		}

		public float WorldToLocalRotationY {
			get {
				Bone parent = this.parent;
				if (parent == null) return arotation;
				float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, b = this.b, d = this.d;
				return MathUtils.Atan2(pa * d - pc * b, pd * b - pb * d) * MathUtils.RadDeg;
			}
		}

		public float WorldToLocalRotation (float worldRotation) {
			float sin = MathUtils.SinDeg(worldRotation), cos = MathUtils.CosDeg(worldRotation);
			return MathUtils.Atan2(a * sin - c * cos, d * cos - b * sin) * MathUtils.RadDeg + rotation - shearX;
		}

		public float LocalToWorldRotation (float localRotation) {
			localRotation -= rotation - shearX;
			float sin = MathUtils.SinDeg(localRotation), cos = MathUtils.CosDeg(localRotation);
			return MathUtils.Atan2(cos * c + sin * d, cos * a + sin * b) * MathUtils.RadDeg;
		}

		/// <summary>
		/// Rotates the world transform the specified amount and sets isAppliedValid to false.
		/// </summary>
		/// <param name="degrees">Degrees.</param>
		public void RotateWorld (float degrees) {
			float a = this.a, b = this.b, c = this.c, d = this.d;
			float cos = MathUtils.CosDeg(degrees), sin = MathUtils.SinDeg(degrees);
			this.a = cos * a - sin * c;
			this.b = cos * b - sin * d;
			this.c = sin * a + cos * c;
			this.d = sin * b + cos * d;
			appliedValid = false;
		}

		override public string ToString () {
			return data.name;
		}
	}
}
