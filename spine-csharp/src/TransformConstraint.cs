/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
	/// Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
	/// bones to match that of the target bone.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class TransformConstraint : IUpdatable {
		internal readonly TransformConstraintData data;
		internal readonly ExposedList<Bone> bones;
		internal Bone target;
		internal float mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;

		internal bool active;

		public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			mixRotate = data.mixRotate;
			mixX = data.mixX;
			mixY = data.mixY;
			mixScaleX = data.mixScaleX;
			mixScaleY = data.mixScaleY;
			mixShearY = data.mixShearY;
			bones = new ExposedList<Bone>();
			foreach (BoneData boneData in data.bones)
				bones.Add(skeleton.bones.Items[boneData.index]);

			target = skeleton.bones.Items[data.target.index];
		}

		/// <summary>Copy constructor.</summary>
		public TransformConstraint (TransformConstraint constraint, Skeleton skeleton) {
			if (constraint == null) throw new ArgumentNullException("constraint cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			data = constraint.data;
			bones = new ExposedList<Bone>(constraint.Bones.Count);
			foreach (Bone bone in constraint.Bones)
				bones.Add(skeleton.Bones.Items[bone.data.index]);
			target = skeleton.Bones.Items[constraint.target.data.index];
			mixRotate = constraint.mixRotate;
			mixX = constraint.mixX;
			mixY = constraint.mixY;
			mixScaleX = constraint.mixScaleX;
			mixScaleY = constraint.mixScaleY;
			mixShearY = constraint.mixShearY;
		}

		public void Update () {
			if (mixRotate == 0 && mixX == 0 && mixY == 0 && mixScaleX == 0 && mixScaleY == 0 && mixShearY == 0) return;
			if (data.local) {
				if (data.relative)
					ApplyRelativeLocal();
				else
					ApplyAbsoluteLocal();
			} else {
				if (data.relative)
					ApplyRelativeWorld();
				else
					ApplyAbsoluteWorld();
			}
		}

		void ApplyAbsoluteWorld () {
			float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
			bool translate = mixX != 0 || mixY != 0;

			Bone target = this.target;
			float ta = target.a, tb = target.b, tc = target.c, td = target.d;
			float degRadReflect = ta * td - tb * tc > 0 ? MathUtils.DegRad : -MathUtils.DegRad;
			float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;

			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bones[i];

				if (mixRotate != 0) {
					float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					float r = MathUtils.Atan2(tc, ta) - MathUtils.Atan2(c, a) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					r *= mixRotate;
					float cos = MathUtils.Cos(r), sin = MathUtils.Sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}

				if (translate) {
					float tx, ty; //Vector2 temp = this.temp;
					target.LocalToWorld(data.offsetX, data.offsetY, out tx, out ty); //target.localToWorld(temp.set(data.offsetX, data.offsetY));
					bone.worldX += (tx - bone.worldX) * mixX;
					bone.worldY += (ty - bone.worldY) * mixY;
				}

				if (mixScaleX != 0) {
					float s = (float)Math.Sqrt(bone.a * bone.a + bone.c * bone.c);
					if (s != 0) s = (s + ((float)Math.Sqrt(ta * ta + tc * tc) - s + data.offsetScaleX) * mixScaleX) / s;
					bone.a *= s;
					bone.c *= s;
				}
				if (mixScaleY != 0) {
					float s = (float)Math.Sqrt(bone.b * bone.b + bone.d * bone.d);
					if (s != 0) s = (s + ((float)Math.Sqrt(tb * tb + td * td) - s + data.offsetScaleY) * mixScaleY) / s;
					bone.b *= s;
					bone.d *= s;
				}

				if (mixShearY > 0) {
					float b = bone.b, d = bone.d;
					float by = MathUtils.Atan2(d, b);
					float r = MathUtils.Atan2(td, tb) - MathUtils.Atan2(tc, ta) - (by - MathUtils.Atan2(bone.c, bone.a));
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					r = by + (r + offsetShearY) * mixShearY;
					float s = (float)Math.Sqrt(b * b + d * d);
					bone.b = MathUtils.Cos(r) * s;
					bone.d = MathUtils.Sin(r) * s;
				}

				bone.UpdateAppliedTransform();
			}
		}

		void ApplyRelativeWorld () {
			float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
			bool translate = mixX != 0 || mixY != 0;

			Bone target = this.target;
			float ta = target.a, tb = target.b, tc = target.c, td = target.d;
			float degRadReflect = ta * td - tb * tc > 0 ? MathUtils.DegRad : -MathUtils.DegRad;
			float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;

			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bones[i];

				if (mixRotate != 0) {
					float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					float r = MathUtils.Atan2(tc, ta) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					r *= mixRotate;
					float cos = MathUtils.Cos(r), sin = MathUtils.Sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
				}

				if (translate) {
					float tx, ty; //Vector2 temp = this.temp;
					target.LocalToWorld(data.offsetX, data.offsetY, out tx, out ty); //target.localToWorld(temp.set(data.offsetX, data.offsetY));
					bone.worldX += tx * mixX;
					bone.worldY += ty * mixY;
				}

				if (mixScaleX != 0) {
					float s = ((float)Math.Sqrt(ta * ta + tc * tc) - 1 + data.offsetScaleX) * mixScaleX + 1;
					bone.a *= s;
					bone.c *= s;
				}
				if (mixScaleY != 0) {
					float s = ((float)Math.Sqrt(tb * tb + td * td) - 1 + data.offsetScaleY) * mixScaleY + 1;
					bone.b *= s;
					bone.d *= s;
				}

				if (mixShearY > 0) {
					float r = MathUtils.Atan2(td, tb) - MathUtils.Atan2(tc, ta);
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) //
						r += MathUtils.PI2;
					float b = bone.b, d = bone.d;
					r = MathUtils.Atan2(d, b) + (r - MathUtils.PI / 2 + offsetShearY) * mixShearY;
					float s = (float)Math.Sqrt(b * b + d * d);
					bone.b = MathUtils.Cos(r) * s;
					bone.d = MathUtils.Sin(r) * s;
				}

				bone.UpdateAppliedTransform();
			}
		}

		void ApplyAbsoluteLocal () {
			float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;

			Bone target = this.target;

			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bones[i];

				float rotation = bone.arotation;
				if (mixRotate != 0) {
					float r = target.arotation - rotation + data.offsetRotation;
					r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
					rotation += r * mixRotate;
				}

				float x = bone.ax, y = bone.ay;
				x += (target.ax - x + data.offsetX) * mixX;
				y += (target.ay - y + data.offsetY) * mixY;

				float scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (mixScaleX != 0 && scaleX != 0)
					scaleX = (scaleX + (target.ascaleX - scaleX + data.offsetScaleX) * mixScaleX) / scaleX;
				if (mixScaleY != 0 && scaleY != 0)
					scaleY = (scaleY + (target.ascaleY - scaleY + data.offsetScaleY) * mixScaleY) / scaleY;

				float shearY = bone.ashearY;
				if (mixShearY != 0) {
					float r = target.ashearY - shearY + data.offsetShearY;
					r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
					shearY += r * mixShearY;
				}

				bone.UpdateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		void ApplyRelativeLocal () {
			float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;

			Bone target = this.target;

			Bone[] bones = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bones[i];

				float rotation = bone.arotation + (target.arotation + data.offsetRotation) * mixRotate;
				float x = bone.ax + (target.ax + data.offsetX) * mixX;
				float y = bone.ay + (target.ay + data.offsetY) * mixY;
				float scaleX = bone.ascaleX * (((target.ascaleX - 1 + data.offsetScaleX) * mixScaleX) + 1);
				float scaleY = bone.ascaleY * (((target.ascaleY - 1 + data.offsetScaleY) * mixScaleY) + 1);
				float shearY = bone.ashearY + (target.ashearY + data.offsetShearY) * mixShearY;

				bone.UpdateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		/// <summary>The bones that will be modified by this transform constraint.</summary>
		public ExposedList<Bone> Bones { get { return bones; } }
		/// <summary>The target bone whose world transform will be copied to the constrained bones.</summary>
		public Bone Target { get { return target; } set { target = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained rotation.</summary>
		public float MixRotate { get { return mixRotate; } set { mixRotate = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained translation X.</summary>
		public float MixX { get { return mixX; } set { mixX = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y.</summary>
		public float MixY { get { return mixY; } set { mixY = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained scale X.</summary>
		public float MixScaleX { get { return mixScaleX; } set { mixScaleX = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained scale Y.</summary>
		public float MixScaleY { get { return mixScaleY; } set { mixScaleY = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y.</summary>
		public float MixShearY { get { return mixShearY; } set { mixShearY = value; } }
		public bool Active { get { return active; } }
		/// <summary>The transform constraint's setup pose data.</summary>
		public TransformConstraintData Data { get { return data; } }

		override public string ToString () {
			return data.name;
		}
	}
}
