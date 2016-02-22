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
using System.Collections.Generic;

namespace Spine {
	public class Bone : IUpdatable {
		static public bool yDown;

		internal BoneData data;
		internal Skeleton skeleton;
		internal Bone parent;
		internal ExposedList<Bone> children = new ExposedList<Bone>();
		internal float x, y, rotation, scaleX, scaleY;
		internal float appliedRotation, appliedScaleX, appliedScaleY;

		internal float a, b, worldX;
		internal float c, d, worldY;
		internal float worldSignX, worldSignY;

		public BoneData Data { get { return data; } }
		public Skeleton Skeleton { get { return skeleton; } }
		public Bone Parent { get { return parent; } }
		public ExposedList<Bone> Children { get { return children; } }
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Rotation { get { return rotation; } set { rotation = value; } }
		/// <summary>The rotation, as calculated by any constraints.</summary>
		public float AppliedRotation { get { return appliedRotation; } set { appliedRotation = value; } }
		/// <summary>The scale X, as calculated by any constraints.</summary>
		public float AppliedScaleX { get { return appliedScaleX; } set { appliedScaleX = value; } }
		/// <summary>The scale Y, as calculated by any constraints.</summary>
		public float AppliedScaleY { get { return appliedScaleY; } set { appliedScaleY = value; } }
		public float ScaleX { get { return scaleX; } set { scaleX = value; } }
		public float ScaleY { get { return scaleY; } set { scaleY = value; } }

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
		public float WorldScaleX { get { return (float)Math.Sqrt(a * a + b * b) * worldSignX; } }
		public float WorldScaleY { get { return (float)Math.Sqrt(c * c + d * d) * worldSignY; } }

		/// <param name="parent">May be null.</param>
		public Bone (BoneData data, Skeleton skeleton, Bone parent) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			this.data = data;
			this.skeleton = skeleton;
			this.parent = parent;
			SetToSetupPose();
		}

		/// <summary>Same as {@link #updateWorldTransform()}. This method exists for Bone to implement {@link Updatable}.</summary>
		public void Update () {
			UpdateWorldTransform(x, y, rotation, scaleX, scaleY);
		}

		/// <summary>Computes the world SRT using the parent bone and this bone's local SRT.</summary>
		public void UpdateWorldTransform () {
			UpdateWorldTransform(x, y, rotation, scaleX, scaleY);
		}

		/// <summary>Computes the world SRT using the parent bone and the specified local SRT.</summary>
		public void UpdateWorldTransform (float x, float y, float rotation, float scaleX, float scaleY) {
			appliedRotation = rotation;
			appliedScaleX = scaleX;
			appliedScaleY = scaleY;

			float cos = MathUtils.CosDeg(rotation), sin = MathUtils.SinDeg(rotation);
			float la = cos * scaleX, lb = -sin * scaleY, lc = sin * scaleX, ld = cos * scaleY;
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
			} else if (data.inheritRotation) { // No scale inheritance.
				pa = 1;
				pb = 0;
				pc = 0;
				pd = 1;
				do {
					cos = MathUtils.CosDeg(parent.appliedRotation);
					sin = MathUtils.SinDeg(parent.appliedRotation);
					float temp = pa * cos + pb * sin;
					pb = pa * -sin + pb * cos;
					pa = temp;
					temp = pc * cos + pd * sin;
					pd = pc * -sin + pd * cos;
					pc = temp;

					if (!parent.data.inheritRotation) break;
					parent = parent.parent;
				} while (parent != null);
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
				if (skeleton.flipX) {
					a = -a;
					b = -b;
				}
				if (skeleton.flipY != yDown) {
					c = -c;
					d = -d;
				}
			} else if (data.inheritScale) { // No rotation inheritance.
				pa = 1;
				pb = 0;
				pc = 0;
				pd = 1;
				do {
					float r = parent.rotation;
					cos = MathUtils.CosDeg(r);
					sin = MathUtils.SinDeg(r);
					float psx = parent.appliedScaleX, psy = parent.appliedScaleY;
					float za = cos * psx, zb = -sin * psy, zc = sin * psx, zd = cos * psy;
					float temp = pa * za + pb * zc;
					pb = pa * zb + pb * zd;
					pa = temp;
					temp = pc * za + pd * zc;
					pd = pc * zb + pd * zd;
					pc = temp;

					if (psx < 0) r = -r;
					cos = MathUtils.CosDeg(-r);
					sin = MathUtils.SinDeg(-r);
					temp = pa * cos + pb * sin;
					pb = pa * -sin + pb * cos;
					pa = temp;
					temp = pc * cos + pd * sin;
					pd = pc * -sin + pd * cos;
					pc = temp;

					if (!parent.data.inheritScale) break;
					parent = parent.parent;
				} while (parent != null);
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
				if (skeleton.flipX) {
					a = -a;
					b = -b;
				}
				if (skeleton.flipY != yDown) {
					c = -c;
					d = -d;
				}
			} else {
				a = la;
				b = lb;
				c = lc;
				d = ld;
			}
		}

		public void SetToSetupPose () {
			BoneData data = this.data;
			x = data.x;
			y = data.y;
			rotation = data.rotation;
			scaleX = data.scaleX;
			scaleY = data.scaleY;
		}

		public void WorldToLocal (float worldX, float worldY, out float localX, out float localY) {
			float x = worldX - this.worldX, y = worldY - this.worldY;
			float a = this.a, b = this.b, c = this.c, d = this.d;
			float invDet = 1 / (a * d - b * c);
			localX = (x * a * invDet - y * b * invDet);
			localY = (y * d * invDet - x * c * invDet);
		}

		public void LocalToWorld (float localX, float localY, out float worldX, out float worldY) {
			float x = localX, y = localY;
			worldX = x * a + y * b + this.worldX;
			worldY = x * c + y * d + this.worldY;
		}

		override public String ToString () {
			return data.name;
		}
	}
}
