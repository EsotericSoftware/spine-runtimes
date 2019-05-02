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
	/// Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
	/// bones to match that of the target bone.</para>
	/// <para>
	/// See <a href="http://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide.</para>
	/// </summary>
	public class TransformConstraint : IConstraint {
		internal TransformConstraintData data;
		internal ExposedList<Bone> bones;
		internal Bone target;
		internal float rotateMix, translateMix, scaleMix, shearMix;

		public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data", "data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
			this.data = data;
			rotateMix = data.rotateMix;
			translateMix = data.translateMix;
			scaleMix = data.scaleMix;
			shearMix = data.shearMix;

			bones = new ExposedList<Bone>();
			foreach (BoneData boneData in data.bones)
				bones.Add (skeleton.FindBone(boneData.name));

			target = skeleton.FindBone(data.target.name);
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
			rotateMix = constraint.rotateMix;
			translateMix = constraint.translateMix;
			scaleMix = constraint.scaleMix;
			shearMix = constraint.shearMix;
		}

		/// <summary>Applies the constraint to the constrained bones.</summary>
		public void Apply () {
			Update();
		}

		public void Update () {
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
			float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			Bone target = this.target;
			float ta = target.a, tb = target.b, tc = target.c, td = target.d;
			float degRadReflect = ta * td - tb * tc > 0 ? MathUtils.DegRad : -MathUtils.DegRad;
			float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;
			var bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bones.Items[i];
				bool modified = false;

				if (rotateMix != 0) {
					float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					float r = MathUtils.Atan2(tc, ta) - MathUtils.Atan2(c, a) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) r += MathUtils.PI2;
					r *= rotateMix;
					float cos = MathUtils.Cos(r), sin = MathUtils.Sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
					modified = true;
				}

				if (translateMix != 0) {
					float tx, ty; //Vector2 temp = this.temp;
					target.LocalToWorld(data.offsetX, data.offsetY, out tx, out ty); //target.localToWorld(temp.set(data.offsetX, data.offsetY));
					bone.worldX += (tx - bone.worldX) * translateMix;
					bone.worldY += (ty - bone.worldY) * translateMix;
					modified = true;
				}

				if (scaleMix > 0) {
					float s = (float)Math.Sqrt(bone.a * bone.a + bone.c * bone.c);
					if (s != 0) s = (s + ((float)Math.Sqrt(ta * ta + tc * tc) - s + data.offsetScaleX) * scaleMix) / s;
					bone.a *= s;
					bone.c *= s;
					s = (float)Math.Sqrt(bone.b * bone.b + bone.d * bone.d);
					if (s != 0) s = (s + ((float)Math.Sqrt(tb * tb + td * td) - s + data.offsetScaleY) * scaleMix) / s;
					bone.b *= s;
					bone.d *= s;
					modified = true;
				}

				if (shearMix > 0) {
					float b = bone.b, d = bone.d;
					float by = MathUtils.Atan2(d, b);
					float r = MathUtils.Atan2(td, tb) - MathUtils.Atan2(tc, ta) - (by - MathUtils.Atan2(bone.c, bone.a));
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) r += MathUtils.PI2;
					r = by + (r + offsetShearY) * shearMix;
					float s = (float)Math.Sqrt(b * b + d * d);
					bone.b = MathUtils.Cos(r) * s;
					bone.d = MathUtils.Sin(r) * s;
					modified = true;
				}

				if (modified) bone.appliedValid = false;
			}
		}

		void ApplyRelativeWorld () {
			float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			Bone target = this.target;
			float ta = target.a, tb = target.b, tc = target.c, td = target.d;
			float degRadReflect = ta * td - tb * tc > 0 ? MathUtils.DegRad : -MathUtils.DegRad;
			float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;
			var bones = this.bones;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bones.Items[i];
				bool modified = false;

				if (rotateMix != 0) {
					float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
					float r = MathUtils.Atan2(tc, ta) + offsetRotation;
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) r += MathUtils.PI2;
					r *= rotateMix;
					float cos = MathUtils.Cos(r), sin = MathUtils.Sin(r);
					bone.a = cos * a - sin * c;
					bone.b = cos * b - sin * d;
					bone.c = sin * a + cos * c;
					bone.d = sin * b + cos * d;
					modified = true;
				}

				if (translateMix != 0) {
					float tx, ty; //Vector2 temp = this.temp;
					target.LocalToWorld(data.offsetX, data.offsetY, out tx, out ty); //target.localToWorld(temp.set(data.offsetX, data.offsetY));
					bone.worldX += tx * translateMix;
					bone.worldY += ty * translateMix;
					modified = true;
				}

				if (scaleMix > 0) {
					float s = ((float)Math.Sqrt(ta * ta + tc * tc) - 1 + data.offsetScaleX) * scaleMix + 1;
					bone.a *= s;
					bone.c *= s;
					s = ((float)Math.Sqrt(tb * tb + td * td) - 1 + data.offsetScaleY) * scaleMix + 1;
					bone.b *= s;
					bone.d *= s;
					modified = true;
				}

				if (shearMix > 0) {
					float r = MathUtils.Atan2(td, tb) - MathUtils.Atan2(tc, ta);
					if (r > MathUtils.PI)
						r -= MathUtils.PI2;
					else if (r < -MathUtils.PI) r += MathUtils.PI2;
					float b = bone.b, d = bone.d;
					r = MathUtils.Atan2(d, b) + (r - MathUtils.PI / 2 + offsetShearY) * shearMix;
					float s = (float)Math.Sqrt(b * b + d * d);
					bone.b = MathUtils.Cos(r) * s;
					bone.d = MathUtils.Sin(r) * s;
					modified = true;
				}

				if (modified) bone.appliedValid = false;
			}
		}

		void ApplyAbsoluteLocal () {
			float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			Bone target = this.target;
			if (!target.appliedValid) target.UpdateAppliedTransform();
			var bonesItems = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bonesItems[i];
				if (!bone.appliedValid) bone.UpdateAppliedTransform();

				float rotation = bone.arotation;
				if (rotateMix != 0) {
					float r = target.arotation - rotation + data.offsetRotation;
					r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
					rotation += r * rotateMix;
				}

				float x = bone.ax, y = bone.ay;
				if (translateMix != 0) {
					x += (target.ax - x + data.offsetX) * translateMix;
					y += (target.ay - y + data.offsetY) * translateMix;
				}

				float scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (scaleMix != 0) {
					if (scaleX != 0) scaleX = (scaleX + (target.ascaleX - scaleX + data.offsetScaleX) * scaleMix) / scaleX;
					if (scaleY != 0) scaleY = (scaleY + (target.ascaleY - scaleY + data.offsetScaleY) * scaleMix) / scaleY;
				}

				float shearY = bone.ashearY;
				if (shearMix != 0) {
					float r = target.ashearY - shearY + data.offsetShearY;
					r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
					shearY += r * shearMix;
				}

				bone.UpdateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		void ApplyRelativeLocal () {
			float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			Bone target = this.target;
			if (!target.appliedValid) target.UpdateAppliedTransform();
			var bonesItems = this.bones.Items;
			for (int i = 0, n = this.bones.Count; i < n; i++) {
				Bone bone = bonesItems[i];
				if (!bone.appliedValid) bone.UpdateAppliedTransform();

				float rotation = bone.arotation;
				if (rotateMix != 0) rotation += (target.arotation + data.offsetRotation) * rotateMix;

				float x = bone.ax, y = bone.ay;
				if (translateMix != 0) {
					x += (target.ax + data.offsetX) * translateMix;
					y += (target.ay + data.offsetY) * translateMix;
				}

				float scaleX = bone.ascaleX, scaleY = bone.ascaleY;
				if (scaleMix != 0) {
					scaleX *= ((target.ascaleX - 1 + data.offsetScaleX) * scaleMix) + 1;
					scaleY *= ((target.ascaleY - 1 + data.offsetScaleY) * scaleMix) + 1;
				}

				float shearY = bone.ashearY;
				if (shearMix != 0) shearY += (target.ashearY + data.offsetShearY) * shearMix;

				bone.UpdateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
			}
		}

		public int Order { get { return data.order; } }
		/// <summary>The bones that will be modified by this transform constraint.</summary>
		public ExposedList<Bone> Bones { get { return bones; } }
		/// <summary>The target bone whose world transform will be copied to the constrained bones.</summary>
		public Bone Target { get { return target; } set { target = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained rotations.</summary>
		public float RotateMix { get { return rotateMix; } set { rotateMix = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained translations.</summary>
		public float TranslateMix { get { return translateMix; } set { translateMix = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained scales.</summary>
		public float ScaleMix { get { return scaleMix; } set { scaleMix = value; } }
		/// <summary>A percentage (0-1) that controls the mix between the constrained and unconstrained scales.</summary>
		public float ShearMix { get { return shearMix; } set { shearMix = value; } }
		/// <summary>The transform constraint's setup pose data.</summary>
		public TransformConstraintData Data { get { return data; } }

		override public string ToString () {
			return data.name;
		}
	}
}
