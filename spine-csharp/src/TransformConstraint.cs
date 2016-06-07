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
	public class TransformConstraint : IUpdatable {
		internal TransformConstraintData data;
		internal Bone bone, target;
		internal float rotateMix, translateMix, scaleMix, shearMix;
		internal float offsetRotation, offsetX, offsetY, offsetScaleX, offsetScaleY, offsetShearY;

		public TransformConstraintData Data { get { return data; } }
		public Bone Bone { get { return bone; } set { bone = value; } }
		public Bone Target { get { return target; } set { target = value; } }
		public float RotateMix { get { return rotateMix; } set { rotateMix = value; } }
		public float TranslateMix { get { return translateMix; } set { translateMix = value; } }
		public float ScaleMix { get { return scaleMix; } set { scaleMix = value; } }
		public float ShearMix { get { return shearMix; } set { shearMix = value; } }

		public float OffsetRotation { get { return offsetRotation; } set { offsetRotation = value; } }
		public float OffsetX { get { return offsetX; } set { offsetX = value; } }
		public float OffsetY { get { return offsetY; } set { offsetY = value; } }
		public float OffsetScaleX { get { return offsetScaleX; } set { offsetScaleX = value; } }
		public float OffsetScaleY { get { return offsetScaleY; } set { offsetScaleY = value; } }
		public float OffsetShearY { get { return offsetShearY; } set { offsetShearY = value; } }

		public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
			if (data == null) throw new ArgumentNullException("data cannot be null.");
			if (skeleton == null) throw new ArgumentNullException("skeleton cannot be null.");
			this.data = data;
			translateMix = data.translateMix;
			rotateMix = data.rotateMix;
			scaleMix = data.scaleMix;
			shearMix = data.shearMix;
			offsetRotation = data.offsetRotation;
			offsetX = data.offsetX;
			offsetY = data.offsetY;
			offsetScaleX = data.offsetScaleX;
			offsetScaleY = data.offsetScaleY;
			offsetShearY = data.offsetShearY;

			bone = skeleton.FindBone(data.bone.name);
			target = skeleton.FindBone(data.target.name);
		}

		public void Apply () {
			Update();
		}

		public void Update () {
			Bone bone = this.bone;
			Bone target = this.target;

			if (rotateMix > 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = MathUtils.Atan2(target.c, target.a) - MathUtils.Atan2(c, a) + offsetRotation * MathUtils.degRad;
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) r += MathUtils.PI2;
				r *= rotateMix;
				float cos = MathUtils.Cos(r), sin = MathUtils.Sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (scaleMix > 0) {
				float bs = (float)Math.Sqrt(bone.a * bone.a + bone.c * bone.c);
				float ts = (float)Math.Sqrt(target.a * target.a + target.c * target.c);
				float s = bs > 0.00001f ? (bs + (ts - bs + offsetScaleX) * scaleMix) / bs : 0;
				bone.a *= s;
				bone.c *= s;
				bs = (float)Math.Sqrt(bone.b * bone.b + bone.d * bone.d);
				ts = (float)Math.Sqrt(target.b * target.b + target.d * target.d);
				s = bs > 0.00001f ? (bs + (ts - bs + offsetScaleY) * scaleMix) / bs : 0;
				bone.b *= s;
				bone.d *= s;
			}

			if (shearMix > 0) {
				float b = bone.b, d = bone.d;
				float by = MathUtils.Atan2(d, b);
				float r = MathUtils.Atan2(target.d, target.b) - MathUtils.Atan2(target.c, target.a) - (by - MathUtils.Atan2(bone.c, bone.a));
				if (r > MathUtils.PI)
					r -= MathUtils.PI2;
				else if (r < -MathUtils.PI) r += MathUtils.PI2;
				r = by + (r + offsetShearY * MathUtils.degRad) * shearMix;
				float s = (float)Math.Sqrt(b * b + d * d);
				bone.b = MathUtils.Cos(r) * s;
				bone.d = MathUtils.Sin(r) * s;
			}

			float translateMix = this.translateMix;
			if (translateMix > 0) {
				float tx, ty;
				target.LocalToWorld(offsetX, offsetY, out tx, out ty);
				bone.worldX += (tx - bone.worldX) * translateMix;
				bone.worldY += (ty - bone.worldY) * translateMix;
			}
		}

		override public String ToString () {
			return data.name;
		}
	}
}
