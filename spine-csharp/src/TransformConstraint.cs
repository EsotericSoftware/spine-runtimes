/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;

namespace Spine {
	public class TransformConstraint : IConstraint {
		internal TransformConstraintData data;
		internal ExposedList<Bone> bones;
		internal Bone target;
		internal float rotateMix, translateMix, scaleMix, shearMix;

		public TransformConstraintData Data { get { return data; } }
		public int Order { get { return data.order; } }
		public ExposedList<Bone> Bones { get { return bones; } }
		public Bone Target { get { return target; } set { target = value; } }
		public float RotateMix { get { return rotateMix; } set { rotateMix = value; } }
		public float TranslateMix { get { return translateMix; } set { translateMix = value; } }
		public float ScaleMix { get { return scaleMix; } set { scaleMix = value; } }
		public float ShearMix { get { return shearMix; } set { shearMix = value; } }

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
				bones.Add (skeleton.FindBone (boneData.name));
			
			target = skeleton.FindBone(data.target.name);
		}

		public void Apply () {
			Update();
		}

		public void Update () {
			float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
			Bone target = this.target;
			float ta = target.a, tb = target.b, tc = target.c, td = target.d;
			float degRadReflect = (ta * td - tb * tc > 0) ? MathUtils.DegRad : -MathUtils.DegRad;
			float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;
			var bones = this.bones;
			var bonesItems = bones.Items;
			for (int i = 0, n = bones.Count; i < n; i++) {
				Bone bone = bonesItems[i];
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
					float tempx, tempy;
					target.LocalToWorld(data.offsetX, data.offsetY, out tempx, out tempy);
					bone.worldX += (tempx - bone.worldX) * translateMix;
					bone.worldY += (tempy - bone.worldY) * translateMix;
					modified = true;
				}

				if (scaleMix > 0) {
					float s = (float)Math.Sqrt(bone.a * bone.a + bone.c * bone.c);
					float ts = (float)Math.Sqrt(ta * ta + tc * tc);
					if (s > 0.00001f) s = (s + (ts - s + data.offsetScaleX) * scaleMix) / s;
					bone.a *= s;
					bone.c *= s;
					s = (float)Math.Sqrt(bone.b * bone.b + bone.d * bone.d);
					ts = (float)Math.Sqrt(tb * tb + td * td);
					if (s > 0.00001f) s = (s + (ts - s + data.offsetScaleY) * scaleMix) / s;
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

		override public String ToString () {
			return data.name;
		}
	}
}
