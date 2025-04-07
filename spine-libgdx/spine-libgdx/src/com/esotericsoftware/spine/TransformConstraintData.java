/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.utils.Array;

/** Stores the setup pose for a {@link TransformConstraint}.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide. */
public class TransformConstraintData extends ConstraintData {
	final Array<BoneData> bones = new Array();
	BoneData source;
	float offsetX, offsetY, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;
	boolean localSource, localTarget, additive, clamp;
	final Array<FromProperty> properties = new Array();

	public TransformConstraintData (String name) {
		super(name);
	}

	/** The bones that will be modified by this transform constraint. */
	public Array<BoneData> getBones () {
		return bones;
	}

	/** The bone whose world transform will be copied to the constrained bones. */
	public BoneData getSource () {
		return source;
	}

	public void setSource (BoneData source) {
		if (source == null) throw new IllegalArgumentException("source cannot be null.");
		this.source = source;
	}

	/** The mapping of transform properties to other transform properties. */
	public Array<FromProperty> getProperties () {
		return properties;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotation. */
	public float getMixRotate () {
		return mixRotate;
	}

	public void setMixRotate (float mixRotate) {
		this.mixRotate = mixRotate;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation X. */
	public float getMixX () {
		return mixX;
	}

	public void setMixX (float mixX) {
		this.mixX = mixX;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translation Y. */
	public float getMixY () {
		return mixY;
	}

	public void setMixY (float mixY) {
		this.mixY = mixY;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale X. */
	public float getMixScaleX () {
		return mixScaleX;
	}

	public void setMixScaleX (float mixScaleX) {
		this.mixScaleX = mixScaleX;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale Y. */
	public float getMixScaleY () {
		return mixScaleY;
	}

	public void setMixScaleY (float mixScaleY) {
		this.mixScaleY = mixScaleY;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained shear Y. */
	public float getMixShearY () {
		return mixShearY;
	}

	public void setMixShearY (float mixShearY) {
		this.mixShearY = mixShearY;
	}

	/** An offset added to the constrained bone X translation. */
	public float getOffsetX () {
		return offsetX;
	}

	public void setOffsetX (float offsetX) {
		this.offsetX = offsetX;
	}

	/** An offset added to the constrained bone Y translation. */
	public float getOffsetY () {
		return offsetY;
	}

	public void setOffsetY (float offsetY) {
		this.offsetY = offsetY;
	}

	/** Reads the source bone's local transform instead of its world transform. */
	public boolean getLocalSource () {
		return localSource;
	}

	public void setLocalSource (boolean localSource) {
		this.localSource = localSource;
	}

	/** Sets the constrained bones' local transforms instead of their world transforms. */
	public boolean getLocalTarget () {
		return localTarget;
	}

	public void setLocalTarget (boolean localTarget) {
		this.localTarget = localTarget;
	}

	/** Adds the source bone transform to the constrained bones instead of setting it absolutely. */
	public boolean getAdditive () {
		return additive;
	}

	public void setAdditive (boolean additive) {
		this.additive = additive;
	}

	/** Prevents constrained bones from exceeding the ranged defined by {@link ToProperty#offset} and {@link ToProperty#max}. */
	public boolean getClamp () {
		return clamp;
	}

	public void setClamp (boolean clamp) {
		this.clamp = clamp;
	}

	/** Source property for a {@link TransformConstraint}. */
	static abstract public class FromProperty {
		/** The value of this property that corresponds to {@link ToProperty#offset}. */
		public float offset;

		/** Constrained properties. */
		public final Array<ToProperty> to = new Array();

		/** Reads this property from the specified bone. */
		abstract public float value (TransformConstraintData data, Bone source, boolean local);
	}

	/** Constrained property for a {@link TransformConstraint}. */
	static abstract public class ToProperty {
		/** The value of this property that corresponds to {@link FromProperty#offset}. */
		public float offset;

		/** The maximum value of this property when {@link TransformConstraintData#clamp clamped}. */
		public float max;

		/** The scale of the {@link FromProperty} value in relation to this property. */
		public float scale;

		/** Reads the mix for this property from the specified constraint. */
		abstract public float mix (TransformConstraint constraint);

		/** Applies the value to this property. */
		abstract public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive);
	}

	static public class FromRotate extends FromProperty {
		public float value (TransformConstraintData data, Bone source, boolean local) {
			return local ? source.arotation : atan2(source.c, source.a) * radDeg;
		}
	}

	static public class ToRotate extends ToProperty {
		public float mix (TransformConstraint constraint) {
			return constraint.mixRotate;
		}

		public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive) {
			if (local) {
				if (!additive) value -= bone.arotation;
				bone.arotation += value * constraint.mixRotate;
			} else {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				value *= degRad;
				if (!additive) value -= atan2(c, a);
				if (value > PI)
					value -= PI2;
				else if (value < -PI) //
					value += PI2;
				value *= constraint.mixRotate;
				float cos = cos(value), sin = sin(value);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}
		}
	}

	static public class FromX extends FromProperty {
		public float value (TransformConstraintData data, Bone source, boolean local) {
			return local ? source.ax + data.offsetX : data.offsetX * source.a + data.offsetY * source.b + source.worldX;
		}
	}

	static public class ToX extends ToProperty {
		public float mix (TransformConstraint constraint) {
			return constraint.mixX;
		}

		public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive) {
			if (local) {
				if (!additive) value -= bone.ax;
				bone.ax += value * constraint.mixX;
			} else {
				if (!additive) value -= bone.worldX;
				bone.worldX += value * constraint.mixX;
			}
		}
	}

	static public class FromY extends FromProperty {
		public float value (TransformConstraintData data, Bone source, boolean local) {
			return local ? source.ay + data.offsetY : data.offsetX * source.c + data.offsetY * source.d + source.worldY;
		}
	}

	static public class ToY extends ToProperty {
		public float mix (TransformConstraint constraint) {
			return constraint.mixY;
		}

		public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive) {
			if (local) {
				if (!additive) value -= bone.ay;
				bone.ay += value * constraint.mixY;
			} else {
				if (!additive) value -= bone.worldY;
				bone.worldY += value * constraint.mixY;
			}
		}
	}

	static public class FromScaleX extends FromProperty {
		public float value (TransformConstraintData data, Bone source, boolean local) {
			return local ? source.ascaleX : (float)Math.sqrt(source.a * source.a + source.c * source.c);
		}
	}

	static public class ToScaleX extends ToProperty {
		public float mix (TransformConstraint constraint) {
			return constraint.mixScaleX;
		}

		public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive) {
			if (local) {
				if (additive)
					bone.ascaleX *= 1 + ((value - 1) * constraint.mixScaleX);
				else if (bone.ascaleX != 0) //
					bone.ascaleX = 1 + (value / bone.ascaleX - 1) * constraint.mixScaleX;
			} else {
				float s;
				if (additive)
					s = 1 + (value - 1) * constraint.mixScaleX;
				else {
					s = (float)Math.sqrt(bone.a * bone.a + bone.c * bone.c);
					if (s != 0) s = 1 + (value / s - 1) * constraint.mixScaleX;
				}
				bone.a *= s;
				bone.c *= s;
			}
		}
	}

	static public class FromScaleY extends FromProperty {
		public float value (TransformConstraintData data, Bone source, boolean local) {
			return local ? source.ascaleY : (float)Math.sqrt(source.b * source.b + source.d * source.d);
		}
	}

	static public class ToScaleY extends ToProperty {
		public float mix (TransformConstraint constraint) {
			return constraint.mixScaleY;
		}

		public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive) {
			if (local) {
				if (additive)
					bone.ascaleY *= 1 + ((value - 1) * constraint.mixScaleY);
				else if (bone.ascaleY != 0) //
					bone.ascaleY = 1 + (value / bone.ascaleY - 1) * constraint.mixScaleY;
			} else {
				float s;
				if (additive)
					s = 1 + (value - 1) * constraint.mixScaleY;
				else {
					s = (float)Math.sqrt(bone.b * bone.b + bone.d * bone.d);
					if (s != 0) s = 1 + (value / s - 1) * constraint.mixScaleY;
				}
				bone.b *= s;
				bone.d *= s;
			}
		}
	}

	static public class FromShearY extends FromProperty {
		public float value (TransformConstraintData data, Bone source, boolean local) {
			return local ? source.ashearY : (atan2(source.d, source.b) - atan2(source.c, source.a)) * radDeg - 90;
		}
	}

	static public class ToShearY extends ToProperty {
		public float mix (TransformConstraint constraint) {
			return constraint.mixShearY;
		}

		public void apply (TransformConstraint constraint, Bone bone, float value, boolean local, boolean additive) {
			if (local) {
				if (!additive) value -= bone.ashearY;
				bone.ashearY += value * constraint.mixShearY;
			} else {
				float b = bone.b, d = bone.d, by = atan2(d, b);
				value = (value + 90) * degRad;
				if (additive)
					value -= PI / 2;
				else {
					value -= by - atan2(bone.c, bone.a);
					if (value > PI)
						value -= PI2;
					else if (value < -PI) //
						value += PI2;
				}
				value = by + value * constraint.mixShearY;
				float s = (float)Math.sqrt(b * b + d * d);
				bone.b = cos(value) * s;
				bone.d = sin(value) * s;
			}
		}
	}
}
