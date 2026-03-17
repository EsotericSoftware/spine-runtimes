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
	/// The applied pose for a bone. This is the <see cref="Bone"/> pose with constraints applied and the world transform computed by
	/// <see cref="Skeleton.UpdateWorldTransform(Physics)"/>.
	/// </summary>
	public class BonePose : BoneLocal, IUpdate {
		public Bone bone;
		internal float a, b, worldX;
		internal float c, d, worldY;
		internal int world, local;

		/// <summary>
		/// Called by <see cref="Skeleton.UpdateCache()"/> to compute the world transform, if needed.
		/// </summary>
		public void Update (Skeleton skeleton, Physics physics) {
			if (world != skeleton.update) UpdateWorldTransform(skeleton);
		}

		/// <summary>Computes the world transform using the parent bone's applied pose and this pose. Child bones are not updated.
		/// <para>
		/// See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
		/// Runtimes Guide.</para></summary>
		public void UpdateWorldTransform (Skeleton skeleton) {
			if (local == skeleton.update)
				UpdateLocalTransform(skeleton);
			else
				world = skeleton.update;

			if (bone.parent == null) { // Root bone.
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

			BonePose parent = bone.parent.applied;
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
				float r = rotation * MathUtils.DegRad, cos = (float)Math.Cos(r), sin = (float)Math.Sin(r);
				float za = (pa * cos + pb * sin) / skeleton.scaleX;
				float zc = (pc * cos + pd * sin) / skeleton.ScaleY;
				float s = (float)Math.Sqrt(za * za + zc * zc);
				if (s > 0.00001f) s = 1 / s;
				za *= s;
				zc *= s;
				s = (float)Math.Sqrt(za * za + zc * zc);
				if (inherit == Inherit.NoScale && (pa * pd - pb * pc < 0) != (skeleton.scaleX < 0 != skeleton.ScaleY < 0)) s = -s;
				r = MathUtils.PI / 2 + MathUtils.Atan2(zc, za);
				float zb = (float)Math.Cos(r) * s;
				float zd = (float)Math.Sin(r) * s;
				float rx = shearX * MathUtils.DegRad;
				float ry = (90 + shearY) * MathUtils.DegRad;
				float la = (float)Math.Cos(rx) * scaleX;
				float lb = (float)Math.Cos(ry) * scaleY;
				float lc = (float)Math.Sin(rx) * scaleX;
				float ld = (float)Math.Sin(ry) * scaleY;
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

		/// <summary>
		/// Computes the local transform values from the world transform.
		/// <para>
		/// If the world transform is modified (by a constraint, <see cref="RotateWorld(float)"/>, etc) then this method should be called so
		/// the local transform matches the world transform. The local transform may be needed by other code (eg to apply another
		/// constraint). </para>
		/// <para>
		/// Some information is ambiguous in the world transform, such as - 1,-1 scale versus 180 rotation.The local transform after
		/// calling this method is equivalent to the local transform used to compute the world transform, but may not be identical.
		/// </para></summary>
		public void UpdateLocalTransform (Skeleton skeleton) {
			local = 0;
			world = skeleton.update;

			if (bone.parent == null) {
				x = worldX - skeleton.x;
				y = worldY - skeleton.y;
				float a = this.a, b = this.b, c = this.c, d = this.d;
				rotation = MathUtils.Atan2Deg(c, a);
				scaleX = (float)Math.Sqrt(a * a + c * c);
				scaleY = (float)Math.Sqrt(b * b + d * d);
				shearX = 0;
				shearY = MathUtils.Atan2Deg(a * b + c * d, a * d - b * c);
				return;
			}

			BonePose parent = bone.parent.applied;
			float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
			float pid = 1 / (pa * pd - pb * pc);
			float ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
			float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
			x = (dx * ia - dy * ib);
			y = (dy * id - dx * ic);

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
					pb = -pc * skeleton.scaleX * s / skeleton.ScaleY;
					pd = pa * skeleton.ScaleY * s / skeleton.scaleX;
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

			shearX = 0;
			scaleX = (float)Math.Sqrt(ra * ra + rc * rc);
			if (scaleX > 0.0001f) {
				float det = ra * rd - rb * rc;
				scaleY = det / scaleX;
				shearY = -MathUtils.Atan2Deg(ra * rb + rc * rd, det);
				rotation = MathUtils.Atan2Deg(rc, ra);
			} else {
				scaleX = 0;
				scaleY = (float)Math.Sqrt(rb * rb + rd * rd);
				shearY = 0;
				rotation = 90 - MathUtils.Atan2Deg(rd, rb);
			}
		}

		/// <summary>
		/// If the world transform has been modified and the local transform no longer matches, <see cref="UpdateLocalTransform(Skeleton)"/>
		/// is called.
		/// </summary>
		public void ValidateLocalTransform (Skeleton skeleton) {
			if (local == skeleton.update) UpdateLocalTransform(skeleton);
		}

		internal void ModifyLocal (Skeleton skeleton) {
			if (local == skeleton.update) UpdateLocalTransform(skeleton);
			world = 0;
			ResetWorld(skeleton.update);
		}

		internal void ModifyWorld (int update) {
			local = update;
			world = update;
			ResetWorld(update);
		}

		internal void ResetWorld (int update) {
			Bone[] children = bone.children.Items;
			for (int i = 0, n = bone.children.Count; i < n; i++) {
				BonePose child = children[i].applied;
				if (child.world == update) {
					child.world = 0;
					child.local = 0;
					child.ResetWorld(update);
				}
			}
		}

		/// <summary>Part of the world transform matrix for the X axis. If changed, <see cref="UpdateLocalTransform(Skeleton)"/> should be called.</summary>
		public float A { get { return a; } set { a = value; } }
		/// <summary>Part of the world transform matrix for the Y axis. If changed, <see cref="UpdateLocalTransform(Skeleton)"/> should be called.</summary>
		public float B { get { return b; } set { b = value; } }
		/// <summary>Part of the world transform matrix for the X axis. If changed, <see cref="UpdateLocalTransform(Skeleton)"/> should be called.</summary>
		public float C { get { return c; } set { c = value; } }
		/// <summary>Part of the world transform matrix for the Y axis. If changed, <see cref="UpdateLocalTransform(Skeleton)"/> should be called.</summary>
		public float D { get { return d; } set { d = value; } }

		/// <summary>The world X position. If changed, <see cref="UpdateLocalTransform(Skeleton)"/> should be called.</summary>
		public float WorldX { get { return worldX; } set { worldX = value; } }
		/// <summary>The world Y position. If changed, <see cref="UpdateLocalTransform(Skeleton)"/> should be called.</summary>
		public float WorldY { get { return worldY; } set { worldY = value; } }
		/// <summary>The world rotation for the X axis, calculated using <see cref="a"/> and <see cref="c"/>.</summary>
		public float WorldRotationX { get { return MathUtils.Atan2Deg(c, a); } }
		/// <summary>The world rotation for the Y axis, calculated using <see cref="b"/> and <see cref="d"/>.</summary>
		public float WorldRotationY { get { return MathUtils.Atan2Deg(d, b); } }

		/// <summary>Returns the magnitude (always positive) of the world scale X, calculated using <see cref="a"/> and <see cref="c"/>.</summary>
		public float WorldScaleX { get { return (float)Math.Sqrt(a * a + c * c); } }
		/// <summary>Returns the magnitude (always positive) of the world scale Y, calculated using <see cref="b"/> and <see cref="d"/>.</summary>
		public float WorldScaleY { get { return (float)Math.Sqrt(b * b + d * d); } }

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
			if (bone.parent == null) {
				parentX = worldX;
				parentY = worldY;
			} else {
				bone.parent.applied.WorldToLocal(worldX, worldY, out parentX, out parentY);
			}
		}

		/// <summary>Transforms a point from the parent bone's coordinates to world coordinates.</summary>
		public void ParentToWorld (float parentX, float parentY, out float worldX, out float worldY) {
			if (bone.parent == null) {
				worldX = parentX;
				worldY = parentY;
			} else {
				bone.parent.applied.LocalToWorld(parentX, parentY, out worldX, out worldY);
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
		/// After changes are made to the world transform, <see cref="UpdateLocalTransform(Skeleton)"/> should be called on this bone and any
		/// child bones, recursively.
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
			return bone.data.name;
		}
	}
}
