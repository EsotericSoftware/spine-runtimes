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

package com.esotericsoftware.spine;

import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

/** Stores the current pose for a transform constraint. A transform constraint adjusts the world transform of the constrained
 * bones to match that of the target bone.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-transform-constraints">Transform constraints</a> in the Spine User Guide. */
public class TransformConstraint implements Constraint {
	final TransformConstraintData data;
	final Array<Bone> bones;
	Bone target;
	float rotateMix, translateMix, scaleMix, shearMix;
	final Vector2 temp = new Vector2();

	public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;
		scaleMix = data.scaleMix;
		shearMix = data.shearMix;
		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.findBone(boneData.name));
		target = skeleton.findBone(data.target.name);
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
		rotateMix = constraint.rotateMix;
		translateMix = constraint.translateMix;
		scaleMix = constraint.scaleMix;
		shearMix = constraint.shearMix;
	}

	/** Applies the constraint to the constrained bones. */
	public void apply () {
		update();
	}

	public void update () {
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
		float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
		Bone target = this.target;
		float ta = target.a, tb = target.b, tc = target.c, td = target.d;
		float degRadReflect = ta * td - tb * tc > 0 ? degRad : -degRad;
		float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			boolean modified = false;

			if (rotateMix != 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(tc, ta) - atan2(c, a) + offsetRotation;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r *= rotateMix;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
				modified = true;
			}

			if (translateMix != 0) {
				Vector2 temp = this.temp;
				target.localToWorld(temp.set(data.offsetX, data.offsetY));
				bone.worldX += (temp.x - bone.worldX) * translateMix;
				bone.worldY += (temp.y - bone.worldY) * translateMix;
				modified = true;
			}

			if (scaleMix > 0) {
				float s = (float)Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				if (s != 0) s = (s + ((float)Math.sqrt(ta * ta + tc * tc) - s + data.offsetScaleX) * scaleMix) / s;
				bone.a *= s;
				bone.c *= s;
				s = (float)Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				if (s != 0) s = (s + ((float)Math.sqrt(tb * tb + td * td) - s + data.offsetScaleY) * scaleMix) / s;
				bone.b *= s;
				bone.d *= s;
				modified = true;
			}

			if (shearMix > 0) {
				float b = bone.b, d = bone.d;
				float by = atan2(d, b);
				float r = atan2(td, tb) - atan2(tc, ta) - (by - atan2(bone.c, bone.a));
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r = by + (r + offsetShearY) * shearMix;
				float s = (float)Math.sqrt(b * b + d * d);
				bone.b = cos(r) * s;
				bone.d = sin(r) * s;
				modified = true;
			}

			if (modified) bone.appliedValid = false;
		}
	}

	private void applyRelativeWorld () {
		float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
		Bone target = this.target;
		float ta = target.a, tb = target.b, tc = target.c, td = target.d;
		float degRadReflect = ta * td - tb * tc > 0 ? degRad : -degRad;
		float offsetRotation = data.offsetRotation * degRadReflect, offsetShearY = data.offsetShearY * degRadReflect;
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			boolean modified = false;

			if (rotateMix != 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(tc, ta) + offsetRotation;
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r *= rotateMix;
				float cos = cos(r), sin = sin(r);
				bone.a = cos * a - sin * c;
				bone.b = cos * b - sin * d;
				bone.c = sin * a + cos * c;
				bone.d = sin * b + cos * d;
				modified = true;
			}

			if (translateMix != 0) {
				Vector2 temp = this.temp;
				target.localToWorld(temp.set(data.offsetX, data.offsetY));
				bone.worldX += temp.x * translateMix;
				bone.worldY += temp.y * translateMix;
				modified = true;
			}

			if (scaleMix > 0) {
				float s = ((float)Math.sqrt(ta * ta + tc * tc) - 1 + data.offsetScaleX) * scaleMix + 1;
				bone.a *= s;
				bone.c *= s;
				s = ((float)Math.sqrt(tb * tb + td * td) - 1 + data.offsetScaleY) * scaleMix + 1;
				bone.b *= s;
				bone.d *= s;
				modified = true;
			}

			if (shearMix > 0) {
				float r = atan2(td, tb) - atan2(tc, ta);
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				float b = bone.b, d = bone.d;
				r = atan2(d, b) + (r - PI / 2 + offsetShearY) * shearMix;
				float s = (float)Math.sqrt(b * b + d * d);
				bone.b = cos(r) * s;
				bone.d = sin(r) * s;
				modified = true;
			}

			if (modified) bone.appliedValid = false;
		}
	}

	private void applyAbsoluteLocal () {
		float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
		Bone target = this.target;
		if (!target.appliedValid) target.updateAppliedTransform();
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			if (!bone.appliedValid) bone.updateAppliedTransform();

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
			if (scaleMix > 0) {
				if (scaleX != 0) scaleX = (scaleX + (target.ascaleX - scaleX + data.offsetScaleX) * scaleMix) / scaleX;
				if (scaleY != 0) scaleY = (scaleY + (target.ascaleY - scaleY + data.offsetScaleY) * scaleMix) / scaleY;
			}

			float shearY = bone.ashearY;
			if (shearMix > 0) {
				float r = target.ashearY - shearY + data.offsetShearY;
				r -= (16384 - (int)(16384.499999999996 - r / 360)) * 360;
				bone.shearY += r * shearMix;
			}

			bone.updateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	private void applyRelativeLocal () {
		float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
		Bone target = this.target;
		if (!target.appliedValid) target.updateAppliedTransform();
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);
			if (!bone.appliedValid) bone.updateAppliedTransform();

			float rotation = bone.arotation;
			if (rotateMix != 0) rotation += (target.arotation + data.offsetRotation) * rotateMix;

			float x = bone.ax, y = bone.ay;
			if (translateMix != 0) {
				x += (target.ax + data.offsetX) * translateMix;
				y += (target.ay + data.offsetY) * translateMix;
			}

			float scaleX = bone.ascaleX, scaleY = bone.ascaleY;
			if (scaleMix > 0) {
				scaleX *= ((target.ascaleX - 1 + data.offsetScaleX) * scaleMix) + 1;
				scaleY *= ((target.ascaleY - 1 + data.offsetScaleY) * scaleMix) + 1;
			}

			float shearY = bone.ashearY;
			if (shearMix > 0) shearY += (target.ashearY + data.offsetShearY) * shearMix;

			bone.updateWorldTransform(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
		}
	}

	public int getOrder () {
		return data.order;
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
		this.target = target;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
	public float getRotateMix () {
		return rotateMix;
	}

	public void setRotateMix (float rotateMix) {
		this.rotateMix = rotateMix;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained translations. */
	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scales. */
	public float getScaleMix () {
		return scaleMix;
	}

	public void setScaleMix (float scaleMix) {
		this.scaleMix = scaleMix;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained scales. */
	public float getShearMix () {
		return shearMix;
	}

	public void setShearMix (float shearMix) {
		this.shearMix = shearMix;
	}

	/** The transform constraint's setup pose data. */
	public TransformConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
