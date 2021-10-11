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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the target bone.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide. */
public class TransformConstraint implements Updatable {
	final TransformConstraintData data;
	final Array<Bone> bones;
	Bone target;
	float mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY;

	boolean active;
	final Vector2 temp = new Vector2();

	public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		mixRotate = data.mixRotate;
		mixX = data.mixX;
		mixY = data.mixY;
		mixScaleX = data.mixScaleX;
		mixScaleY = data.mixScaleY;
		mixShearY = data.mixShearY;
		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.bones.get(boneData.index));
		target = skeleton.bones.get(data.target.index);
	}

	/** Copy constructor. */
	public TransformConstraint (TransformConstraint constraint, Skeleton skeleton) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = constraint.data;
		bones = new Array(constraint.bones.size);
		for (Bone bone : constraint.bones)
			bones.add(skeleton.bones.get(bone.data.index));
		target = skeleton.bones.get(constraint.target.data.index);
		mixRotate = constraint.mixRotate;
		mixX = constraint.mixX;
		mixY = constraint.mixY;
		mixScaleX = constraint.mixScaleX;
		mixScaleY = constraint.mixScaleY;
		mixShearY = constraint.mixShearY;
	}

	/** Applies the constraint to the constrained bones. */
	public void update () {
		if (mixRotate == 0 && mixX == 0 && mixY == 0 && mixScaleX == 0 && mixScaleX == 0 && mixShearY == 0) return;
		if (data.local) {
			if (data.relative)
				applyRelativeLocal();
			else
				applyAbsoluteLocal();
		} else {
			if (data.relative)
				applyRelativeWorld();
			else
				applyAbsoluteWorld();
		}
	}

	private void applyAbsoluteWorld () {
		float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
		boolean translate = mixX != 0 || mixY != 0;

		Bone target = this.target;
		float ta = target.a, tb = target.b, tc = target.c, td = target.d;
		float degRadReflect = ta * td - tb * tc > 0 ? degRad : -degRad;
		float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;

		Object[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++) {
			Bone bone = (Bone)bones[i];

			if (mixRotate != 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(tc, ta) - atan2(c, a) + offsetRotation;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) //
					r += PI2;
				r *= mixRotate;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (translate) {
				Vector2 temp = this.temp;
				target.localToWorld(temp.set(data.offsetX, data.offsetY));
				bone.worldX += (temp.x - bone.worldX) * mixX;
				bone.worldY += (temp.y - bone.worldY) * mixY;
			}

			if (mixScaleX != 0) {
				float s = (float)Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				if (s != 0) s = (s + ((float)Math.sqrt(ta * ta + tc * tc) - s + data.offsetScaleX) * mixScaleX) / s;
				bone.a *= s;
				bone.c *= s;
			}
			if (mixScaleY != 0) {
				float s = (float)Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				if (s != 0) s = (s + ((float)Math.sqrt(tb * tb + td * td) - s + data.offsetScaleY) * mixScaleY) / s;
				bone.b *= s;
				bone.d *= s;
			}

			if (mixShearY > 0) {
				float b = bone.b, d = bone.d;
				float by = atan2(d, b);
				float r = atan2(td, tb) - atan2(tc, ta) - (by - atan2(bone.c, bone.a));
				if (r > PI)
					r -= PI2;
				else if (r < -PI) //
					r += PI2;
				r = by + (r + offsetShearY) * mixShearY;
				float s = (float)Math.sqrt(b * b + d * d);
				bone.b = cos(r) * s;
				bone.d = sin(r) * s;
			}

			bone.updateAppliedTransform();
		}
	}

	private void applyRelativeWorld () {
		float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;
		boolean translate = mixX != 0 || mixY != 0;

		Bone target = this.target;
		float ta = target.a, tb = target.b, tc = target.c, td = target.d;
		float degRadReflect = ta * td - tb * tc > 0 ? degRad : -degRad;
		float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;

		Object[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++) {
			Bone bone = (Bone)bones[i];

			if (mixRotate != 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(tc, ta) + offsetRotation;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) //
					r += PI2;
				r *= mixRotate;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
			}

			if (translate) {
				Vector2 temp = this.temp;
				target.localToWorld(temp.set(data.offsetX, data.offsetY));
				bone.worldX += temp.x * mixX;
				bone.worldY += temp.y * mixY;
			}

			if (mixScaleX != 0) {
				float s = ((float)Math.sqrt(ta * ta + tc * tc) - 1 + data.offsetScaleX) * mixScaleX + 1;
				bone.a *= s;
				bone.c *= s;
			}
			if (mixScaleY != 0) {
				float s = ((float)Math.sqrt(tb * tb + td * td) - 1 + data.offsetScaleY) * mixScaleY + 1;
				bone.b *= s;
				bone.d *= s;
			}

			if (mixShearY > 0) {
				float r = atan2(td, tb) - atan2(tc, ta);
				if (r > PI)
					r -= PI2;
				else if (r < -PI) //
					r += PI2;
				float b = bone.b, d = bone.d;
				r = atan2(d, b) + (r - PI / 2 + offsetShearY) * mixShearY;
				float s = (float)Math.sqrt(b * b + d * d);
				bone.b = cos(r) * s;
				bone.d = sin(r) * s;
			}

			bone.updateAppliedTransform();
		}
	}

	private void applyAbsoluteLocal () {
		float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;

		Bone target = this.target;

		Object[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++) {
			Bone bone = (Bone)bones[i];

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

			bone.updateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	private void applyRelativeLocal () {
		float mixRotate = this.mixRotate, mixX = this.mixX, mixY = this.mixY, mixScaleX = this.mixScaleX,
			mixScaleY = this.mixScaleY, mixShearY = this.mixShearY;

		Bone target = this.target;

		Object[] bones = this.bones.items;
		for (int i = 0, n = this.bones.size; i < n; i++) {
			Bone bone = (Bone)bones[i];

			float rotation = bone.arotation + (target.arotation + data.offsetRotation) * mixRotate;
			float x = bone.ax + (target.ax + data.offsetX) * mixX;
			float y = bone.ay + (target.ay + data.offsetY) * mixY;
			float scaleX = bone.ascaleX * (((target.ascaleX - 1 + data.offsetScaleX) * mixScaleX) + 1);
			float scaleY = bone.ascaleY * (((target.ascaleY - 1 + data.offsetScaleY) * mixScaleY) + 1);
			float shearY = bone.ashearY + (target.ashearY + data.offsetShearY) * mixShearY;

			bone.updateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	/** The bones that will be modified by this transform constraint. */
	public Array<Bone> getBones () {
		return bones;
	}

	/** The target bone whose world transform will be copied to the constrained bones. */
	public Bone getTarget () {
		return target;
	}

	public void setTarget (Bone target) {
		if (target == null) throw new IllegalArgumentException("target cannot be null.");
		this.target = target;
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

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scale X. */
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

	public boolean isActive () {
		return active;
	}

	/** The transform constraint's setup pose data. */
	public TransformConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
