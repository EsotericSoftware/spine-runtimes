/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class Bone : IUpdatable {
		static public bool yDown;

		internal BoneData data;
		internal Skeleton skeleton;
		internal Bone parent;
		internal ExposedList<Bone> children = new ExposedList<Bone>();
		internal float x, y, rotation, scaleX, scaleY, shearX, shearY;
		internal float appliedRotation;

		internal float a, b, worldX;
		internal float c, d, worldY;
		internal float worldSignX, worldSignY;

		internal bool sorted;

		public BoneData Data { get { return data; } }
		public Skeleton Skeleton { get { return skeleton; } }
		public Bone Parent { get { return parent; } }
		public ExposedList<Bone> Children { get { return children; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Rotation { get { return rotation; } set { rotation = value; } }
		/// <summary>The rotation, as calculated by any constraints.</summary>
		public float AppliedRotation { get { return appliedRotation; } set { appliedRotation = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }
		public float ShearX { get { return shearX; } set { shearX = value; } }
		public float ShearY { get { return shearY; } set { shearY = value; } }

		public float A { get { return a; } }
		public float B { get { return b; } }
		public float C { get { return c; } }
		public float D { get { return d; } }
		public float WorldX { get { return worldX; } }
		public float WorldY { get { return worldY; } }
		public float WorldSignX { get { return worldSignX; } }
		public float WorldSignY { get { return worldSignY; } }
		public float WorldRotationX { get { return MathUtils.Atan2(c, a) * MathUtils.radDeg; } }
		public float WorldRotationY { get { return MathUtils.Atan2(d, b) * MathUtils.radDeg; } }
		public float WorldScaleX { get { return (float)Math.Sqrt(a * a + c * c) * worldSignX; } }
		public float WorldScaleY { get { return (float)Math.Sqrt(b * b + d * d) * worldSignY; } }

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
			appliedRotation = rotation;

			float rotationY = rotation + 90 + shearY;
			float la = MathUtils.CosDeg(rotation + shearX) * scaleX, lb = MathUtils.CosDeg(rotationY) * scaleY;
			float lc = MathUtils.SinDeg(rotation + shearX) * scaleX, ld = MathUtils.SinDeg(rotationY) * scaleY;

			Bone parent = this.parent;
			if (parent == null) { // Root bone.
				Skeleton skeleton = this.skeleton;
				if (skeleton.flipX) {
					x = -x;
					la = -la;
					lb = -lb;
				}
				if (skeleton.flipY != yDown) {
					y = -y;
					lc = -lc;
					ld = -ld;
				}
				a = la;
				b = lb;
				c = lc;
				d = ld;
				worldX = x;
				worldY = y;
				worldSignX = Math.Sign(scaleX);
				worldSignY = Math.Sign(scaleY);
				return;
			}

			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			worldX = pa * x + pb * y + parent.worldX;
			worldY = pc * x + pd * y + parent.worldY;
			worldSignX = parent.worldSignX * Math.Sign(scaleX);
			worldSignY = parent.worldSignY * Math.Sign(scaleY);

			if (data.inheritRotation && data.inheritScale) {
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			} else {
				if (data.inheritRotation) { // No scale inheritance.
					pa = 1;
					pb = 0;
					pc = 0;
					pd = 1;
					do {
						float cos = MathUtils.CosDeg(parent.appliedRotation), sin = MathUtils.SinDeg(parent.appliedRotation);
						float temp = pa * cos + pb * sin;
						pb = pb * cos - pa * sin;
						pa = temp;
						temp = pc * cos + pd * sin;
						pd = pd * cos - pc * sin;
						pc = temp;

						if (!parent.data.inheritRotation) break;
						parent = parent.parent;
					} while (parent != null);
					a = pa * la + pb * lc;
					b = pa * lb + pb * ld;
					c = pc * la + pd * lc;
					d = pc * lb + pd * ld;
				} else if (data.inheritScale) { // No rotation inheritance.
					pa = 1;
					pb = 0;
					pc = 0;
					pd = 1;
					do {
						float cos = MathUtils.CosDeg(parent.appliedRotation), sin = MathUtils.SinDeg(parent.appliedRotation);
						float psx = parent.scaleX, psy = parent.scaleY;
						float za = cos * psx, zb = sin * psy, zc = sin * psx, zd = cos * psy;
						float temp = pa * za + pb * zc;
						pb = pb * zd - pa * zb;
						pa = temp;
						temp = pc * za + pd * zc;
						pd = pd * zd - pc * zb;
						pc = temp;

						if (psx >= 0) sin = -sin;
						temp = pa * cos + pb * sin;
						pb = pb * cos - pa * sin;
						pa = temp;
						temp = pc * cos + pd * sin;
						pd = pd * cos - pc * sin;
						pc = temp;

						if (!parent.data.inheritScale) break;
						parent = parent.parent;
					} while (parent != null);
					a = pa * la + pb * lc;
					b = pa * lb + pb * ld;
					c = pc * la + pd * lc;
					d = pc * lb + pd * ld;
				} else {
					a = la;
					b = lb;
					c = lc;
					d = ld;
				}
				if (skeleton.flipX) {
					a = -a;
					b = -b;
				}
				if (skeleton.flipY != yDown) {
					c = -c;
					d = -d;
				}
			}
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

		public float WorldToLocalRotationX {
			get {
				Bone parent = this.parent;
				if (parent == null) return rotation;
				float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, a = this.a, c = this.c;
				return MathUtils.Atan2(pa * c - pc * a, pd * a - pb * c) * MathUtils.radDeg;
			}
		}

		public float WorldToLocalRotationY {
			get {
				Bone parent = this.parent;
				if (parent == null) return rotation;
				float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, b = this.b, d = this.d;
				return MathUtils.Atan2(pa * d - pc * b, pd * b - pb * d) * MathUtils.radDeg;
			}
		}

		public void RotateWorld (float degrees) {
			float a = this.a, b = this.b, c = this.c, d = this.d;
			float cos = MathUtils.CosDeg(degrees), sin = MathUtils.SinDeg(degrees);
			this.a = cos * a - sin * c;
			this.b = cos * b - sin * d;
			this.c = sin * a + cos * c;
			this.d = sin * b + cos * d;
		}

		/// <summary>
		/// Computes the local transform from the world transform. This can be useful to perform processing on the local transform
		/// after the world transform has been modified directly (eg, by a constraint).
		/// 
		/// Some redundant information is lost by the world transform, such as -1,-1 scale versus 180 rotation. The computed local
		/// transform values may differ from the original values but are functionally the same.
		/// </summary>
		public void UpdateLocalTransform () {
			Bone parent = this.parent;
			if (parent == null) {
				x = worldX;
				y = worldY;
				rotation = MathUtils.Atan2(c, a) * MathUtils.radDeg;
				scaleX = (float)Math.Sqrt(a * a + c * c);
				scaleY = (float)Math.Sqrt(b * b + d * d);
				float det = a * d - b * c;
				shearX = 0;
				shearY = MathUtils.Atan2(a * b + c * d, det) * MathUtils.radDeg;
				return;
			}
			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			float pid = 1 / (pa * pd - pb * pc);
			float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
			x = (dx * pd * pid - dy * pb * pid);
			y = (dy * pa * pid - dx * pc * pid);
			float ia = pid * pd;
			float id = pid * pa;
			float ib = pid * pb;
			float ic = pid * pc;
			float ra = ia * a - ib * c;
			float rb = ia * b - ib * d;
			float rc = id * c - ic * a;
			float rd = id * d - ic * b;
			shearX = 0;
			scaleX = (float)Math.Sqrt(ra * ra + rc * rc);
			if (scaleX > 0.0001f) {
				float det = ra * rd - rb * rc;
				scaleY = det / scaleX;
				shearY = MathUtils.Atan2(ra * rb + rc * rd, det) * MathUtils.radDeg;
				rotation = MathUtils.Atan2(rc, ra) * MathUtils.radDeg;
			} else {
				scaleX = 0;
				scaleY = (float)Math.Sqrt(rb * rb + rd * rd);
				shearY = 0;
				rotation = 90 - MathUtils.Atan2(rd, rb) * MathUtils.radDeg;
			}
			appliedRotation = rotation;
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

		override public String ToString () {
			return data.name;
		}
	}
}
