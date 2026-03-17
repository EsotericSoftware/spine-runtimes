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
	public class TransformConstraintData : ConstraintData<TransformConstraint, TransformConstraintPose> {
		public const int ROTATION = 0, X = 1, Y = 2, SCALEX = 3, SCALEY = 4, SHEARY = 5;

		internal readonly ExposedList<BoneData> bones = new ExposedList<BoneData>();
		internal BoneData source;
		internal float[] offsets = new float[6];
		internal bool localSource, localTarget, additive, clamp;
		internal readonly ExposedList<FromProperty> properties = new ExposedList<FromProperty>();

		public TransformConstraintData (string name)
			: base(name, new TransformConstraintPose()) {
		}

		override public IConstraint Create (Skeleton skeleton) {
			return new TransformConstraint(this, skeleton);
		}

		/// <summary>The bones that will be modified by this transform constraint.</summary>
		public ExposedList<BoneData> Bones { get { return bones; } }

		/// <summary>The bone whose world transform will be copied to the constrained bones.</summary>
		public BoneData Source {
			get { return source; }
			set {
				if (source == null) throw new ArgumentNullException("Source", "source cannot be null.");
				source = value;
			}
		}

		/// <summary>An offset added to the constrained bone rotation.</summary>
		public float OffsetRotation { get { return offsets[ROTATION]; } set { offsets[ROTATION] = value; } }
		/// <summary>An offset added to the constrained bone X translation.</summary>
		public float OffsetX { get { return offsets[X]; } set { offsets[X] = value; } }
		/// <summary>An offset added to the constrained bone Y translation.</summary>
		public float OffsetY { get { return offsets[Y]; } set { offsets[Y] = value; } }
		/// <summary>An offset added to the constrained bone scaleX.</summary>
		public float OffsetScaleX { get { return offsets[SCALEX]; } set { offsets[SCALEX] = value; } }
		/// <summary>An offset added to the constrained bone scaleY.</summary>
		public float OffsetScaleY { get { return offsets[SCALEY]; } set { offsets[SCALEY] = value; } }
		/// <summary>An offset added to the constrained bone shearY.</summary>
		public float OffsetShearY { get { return offsets[SHEARY]; } set { offsets[SHEARY] = value; } }

		/// <summary>Reads the source bone's local transform instead of its world transform.</summary>
		public bool LocalSource { get { return localSource; } set { localSource = value; } }
		/// <summary>Sets the constrained bones' local transforms instead of their world transforms.</summary>
		public bool LocalTarget { get { return localTarget; } set { localTarget = value; } }
		/// <summary>Adds the source bone transform to the constrained bones instead of setting it absolutely.</summary>
		public bool Additive { get { return additive; } set { additive = value; } }
		/// <summary>Prevents constrained bones from exceeding the ranged defined by <see cref="ToProperty.offset"/> and
		/// <see cref="ToProperty.max"/>.</summary>
		public bool Clamp { get { return clamp; } set { clamp = value; } }

		/// <summary>The mapping of transform properties to other transform properties.</summary>
		public ExposedList<FromProperty> Properties {
			get { return properties; }
		}

		/// <summary>Source property for a <see cref="TransformConstraint"/>.</summary>
		abstract public class FromProperty {
			/// <summary>The value of this property that corresponds to <see cref="ToProperty.offset"/>.</summary>
			public float offset;

			/// <summary>Constrained properties.</summary>
			public readonly ExposedList<ToProperty> to = new ExposedList<ToProperty>(1);

			/// <summary>Reads this property from the specified bone.</summary>
			abstract public float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets);
		}

		///<summary>Constrained property for a <see cref="TransformConstraint"/>.</summary>
		abstract public class ToProperty {
			/// <summary>The value of this property that corresponds to <see cref="FromProperty.offset"/>.</summary>
			public float offset;

			/// <summary>The maximum value of this property when <see cref="TransformConstraintData.clamp"/> clamped.</summary>
			public float max;

			/// <summary>The scale of the <see cref="FromProperty"/> value in relation to this property.</summary>
			public float scale;

			/// <summary>Reads the mix for this property from the specified pose.</summary>
			public abstract float Mix (TransformConstraintPose pose);

			/// <summary>Applies the value to this property.</summary>
			public abstract void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local, bool additive);
		}

		public class FromRotate : FromProperty {
			public override float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets) {
				if (local) return source.rotation + offsets[ROTATION];
				float sx = skeleton.scaleX, sy = skeleton.ScaleY;
				float value = MathUtils.Atan2(source.c / sy, source.a / sx) * MathUtils.RadDeg
					+ ((source.a * source.d - source.b * source.c) * sx * sy > 0 ? offsets[ROTATION] : -offsets[ROTATION]);
				if (value < 0) value += 360;
				return value;
			}
		}

		public class ToRotate : ToProperty {
			public override float Mix (TransformConstraintPose pose) {
				return pose.mixRotate;
			}

			public override void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local,
				bool additive) {
				if (local)
					bone.rotation += (additive ? value : value - bone.rotation) * pose.mixRotate;
				else {
					float sx = skeleton.scaleX, sy = skeleton.ScaleY, ix = 1 / sx, iy = 1 / sy;
					float a = bone.a * ix, b = bone.b * ix, c = bone.c * iy, d = bone.d * iy;
					value *= MathUtils.DegRad;
					if (!additive) value -= MathUtils.Atan2(c, a);
					if (value > MathUtils.PI)
						value -= MathUtils.PI2;
					else if (value < -MathUtils.PI) //
						value += MathUtils.PI2;
					value *= pose.mixRotate;
					float cos = MathUtils.Cos(value), sin = MathUtils.Sin(value);
					bone.a = (cos * a - sin * c) * sx;
					bone.b = (cos * b - sin * d) * sx;
					bone.c = (sin * a + cos * c) * sy;
					bone.d = (sin * b + cos * d) * sy;
				}
			}
		}

		public class FromX : FromProperty {
			public override float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets) {
				return local ? source.x + offsets[X] : (offsets[X] * source.a + offsets[Y] * source.b + source.worldX) / skeleton.scaleX;
			}
		}

		public class ToX : ToProperty {
			public override float Mix (TransformConstraintPose pose) {
				return pose.mixX;
			}

			public override void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local,
				bool additive) {
				if (local)
					bone.x += (additive ? value : value - bone.x) * pose.mixX;
				else {
					if (!additive) value -= bone.worldX / skeleton.scaleX;
					bone.worldX += value * pose.mixX * skeleton.scaleX;
				}
			}
		}

		public class FromY : FromProperty {
			public override float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets) {
				return local ? source.y + offsets[Y] : (offsets[X] * source.c + offsets[Y] * source.d + source.worldY) / skeleton.ScaleY;
			}
		}

		public class ToY : ToProperty {
			public override float Mix (TransformConstraintPose pose) {
				return pose.mixY;
			}

			public override void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local,
				bool additive) {
				if (local)
					bone.y += (additive ? value : value - bone.y) * pose.mixY;
				else {
					float skeletonScaleY = skeleton.ScaleY;
					if (!additive) value -= bone.worldY / skeletonScaleY;
					bone.worldY += value * pose.mixY * skeletonScaleY;
				}
			}
		}

		public class FromScaleX : FromProperty {
			public override float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets) {
				if (local) return source.scaleX + offsets[SCALEX];
				float a = source.a / skeleton.scaleX, c = source.c / skeleton.ScaleY;
				return (float)Math.Sqrt(a * a + c * c) + offsets[SCALEX];
			}
		}

		public class ToScaleX : ToProperty {
			public override float Mix (TransformConstraintPose pose) {
				return pose.mixScaleX;
			}

			public override void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local,
				bool additive) {
				if (local) {
					if (additive)
						bone.scaleX *= 1 + (value - 1) * pose.mixScaleX;
					else if (bone.scaleX != 0) //
						bone.scaleX += (value - bone.scaleX) * pose.mixScaleX;
				} else if (additive) {
					float s = 1 + (value - 1) * pose.mixScaleX;
					bone.a *= s;
					bone.c *= s;
				} else {
					float a = bone.a / skeleton.scaleX, c = bone.c / skeleton.ScaleY, s = (float)Math.Sqrt(a * a + c * c);
					if (s != 0) {
						s = 1 + (value - s) * pose.mixScaleX / s;
						bone.a *= s;
						bone.c *= s;
					}
				}
			}
		}

		public class FromScaleY : FromProperty {
			public override float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets) {
				if (local) return source.scaleY + offsets[SCALEY];
				float b = source.b / skeleton.scaleX, d = source.d / skeleton.ScaleY;
				return (float)Math.Sqrt(b * b + d * d) + offsets[SCALEY];
			}
		}

		public class ToScaleY : ToProperty {
			public override float Mix (TransformConstraintPose pose) {
				return pose.mixScaleY;
			}

			public override void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local,
				bool additive) {
				if (local) {
					if (additive)
						bone.scaleY *= 1 + (value - 1) * pose.mixScaleY;
					else if (bone.scaleY != 0) //
						bone.scaleY += (value - bone.scaleY) * pose.mixScaleY;
				} else if (additive) {
					float s = 1 + (value - 1) * pose.mixScaleY;
					bone.b *= s;
					bone.d *= s;
				} else {
					float b = bone.b / skeleton.scaleX, d = bone.d / skeleton.ScaleY, s = (float)Math.Sqrt(b * b + d * d);
					if (s != 0) {
						s = 1 + (value - s) * pose.mixScaleY / s;
						bone.b *= s;
						bone.d *= s;
					}
				}
			}
		}

		public class FromShearY : FromProperty {
			public override float Value (Skeleton skeleton, BonePose source, bool local, float[] offsets) {
				if (local) return source.shearY + offsets[SHEARY];
				float ix = 1 / skeleton.scaleX, iy = 1 / skeleton.ScaleY;
				return (MathUtils.Atan2(source.d * iy, source.b * ix) - MathUtils.Atan2(source.c * iy, source.a * ix)) * MathUtils.RadDeg - 90 + offsets[SHEARY];
			}
		}

		public class ToShearY : ToProperty {
			public override float Mix (TransformConstraintPose pose) {
				return pose.mixShearY;
			}

			public override void Apply (Skeleton skeleton, TransformConstraintPose pose, BonePose bone, float value, bool local,
				bool additive) {
				if (local) {
					if (!additive) value -= bone.shearY;
					bone.shearY += value * pose.mixShearY;
				} else {
					float sx = skeleton.scaleX, sy = skeleton.ScaleY, b = bone.b / sx, d = bone.d / sy, by = MathUtils.Atan2(d, b);
					value = (value + 90) * MathUtils.DegRad;
					if (additive)
						value -= MathUtils.PI / 2;
					else {
						value -= by - MathUtils.Atan2(bone.c / sy, bone.a / sx);
						if (value > MathUtils.PI)
							value -= MathUtils.PI2;
						else if (value < -MathUtils.PI) //
							value += MathUtils.PI2;
					}
					value = by + value * pose.mixShearY;
					float s = (float)Math.Sqrt(b * b + d * d);
					bone.b = MathUtils.Cos(value) * s * sx;
					bone.d = MathUtils.Sin(value) * s * sy;
				}
			}
		}
	}
}
