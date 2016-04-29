
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;

public class TransformConstraint implements Updatable {
	final TransformConstraintData data;
	Bone bone, target;
	float rotateMix, translateMix, scaleMix, shearMix;
	float offsetRotation, offsetX, offsetY, offsetScaleX, offsetScaleY, offsetShearY;
	final Vector2 temp = new Vector2();

	public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
		this.data = data;
		translateMix = data.translateMix;
		rotateMix = data.rotateMix;
		scaleMix = data.scaleMix;
		shearMix = data.shearMix;
		offsetX = data.offsetX;
		offsetY = data.offsetY;
		offsetScaleX = data.offsetScaleX;
		offsetScaleY = data.offsetScaleY;
		offsetShearY = data.offsetShearY;

		if (skeleton != null) {
			bone = skeleton.findBone(data.bone.name);
			target = skeleton.findBone(data.target.name);
		}
	}

	/** Copy constructor. */
	public TransformConstraint (TransformConstraint constraint, Skeleton skeleton) {
		data = constraint.data;
		bone = skeleton.bones.get(constraint.bone.skeleton.bones.indexOf(constraint.bone, true));
		target = skeleton.bones.get(constraint.target.skeleton.bones.indexOf(constraint.target, true));
		translateMix = constraint.translateMix;
		rotateMix = constraint.rotateMix;
		scaleMix = constraint.scaleMix;
		shearMix = constraint.shearMix;
		offsetX = constraint.offsetX;
		offsetY = constraint.offsetY;
		offsetScaleX = constraint.offsetScaleX;
		offsetScaleY = constraint.offsetScaleY;
		offsetShearY = constraint.offsetShearY;
	}

	public void apply () {
		update();
	}

	public void update () {
		Bone bone = this.bone;
		Bone target = this.target;

		if (rotateMix > 0) {
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			float r = atan2(target.c, target.a) - atan2(c, a) + offsetRotation * degRad;
			if (r > PI)
				r -= PI2;
			else if (r < -PI) r += PI2;
			r *= rotateMix;
			float cos = cos(r), sin = sin(r);
			bone.a = cos * a - sin * c;
			bone.b = cos * b - sin * d;
			bone.c = sin * a + cos * c;
			bone.d = sin * b + cos * d;
		}

		if (scaleMix > 0) {
			float bs = (float)Math.sqrt(bone.a * bone.a + bone.c * bone.c);
			float ts = (float)Math.sqrt(target.a * target.a + target.c * target.c);
			float s = (bs > 0.00001f ? (bs + (ts - bs + offsetScaleX) * scaleMix) / bs : 0);
			bone.a *= s;
			bone.c *= s;
			bs = (float)Math.sqrt(bone.b * bone.b + bone.d * bone.d);
			ts = (float)Math.sqrt(target.b * target.b + target.d * target.d);
			s = (bs > 0.00001f ? (bs + (ts - bs + offsetScaleY) * scaleMix) / bs : 0);
			bone.b *= s;
			bone.d *= s;
		}

		if (shearMix > 0) {
			float b = bone.b, d = bone.d;
			float by = atan2(d, b);
			float r = atan2(target.d, target.b) - atan2(target.c, target.a) - (by - atan2(bone.c, bone.a));
			if (r > PI)
				r -= PI2;
			else if (r < -PI) r += PI2;
			r = by + (r + offsetShearY * degRad) * shearMix;
			float s = (float)Math.sqrt(b * b + d * d);
			bone.b = cos(r) * s;
			bone.d = sin(r) * s;
		}

		float translateMix = this.translateMix;
		if (translateMix > 0) {
			Vector2 temp = this.temp;
			target.localToWorld(temp.set(offsetX, offsetY));
			bone.worldX += (temp.x - bone.worldX) * translateMix;
			bone.worldY += (temp.y - bone.worldY) * translateMix;
		}
	}

	public Bone getBone () {
		return bone;
	}

	public void setBone (Bone bone) {
		this.bone = bone;
	}

	public Bone getTarget () {
		return target;
	}

	public void setTarget (Bone target) {
		this.target = target;
	}

	public float getRotateMix () {
		return rotateMix;
	}

	public void setRotateMix (float rotateMix) {
		this.rotateMix = rotateMix;
	}

	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public float getScaleMix () {
		return scaleMix;
	}

	public void setScaleMix (float scaleMix) {
		this.scaleMix = scaleMix;
	}

	public float getShearMix () {
		return shearMix;
	}

	public void setShearMix (float shearMix) {
		this.shearMix = shearMix;
	}

	public float getOffsetRotation () {
		return offsetRotation;
	}

	public void setOffsetRotation (float offsetRotation) {
		this.offsetRotation = offsetRotation;
	}

	public float getOffsetX () {
		return offsetX;
	}

	public void setOffsetX (float offsetX) {
		this.offsetX = offsetX;
	}

	public float getOffsetY () {
		return offsetY;
	}

	public void setOffsetY (float offsetY) {
		this.offsetY = offsetY;
	}

	public float getOffsetScaleX () {
		return offsetScaleX;
	}

	public void setOffsetScaleX (float offsetScaleX) {
		this.offsetScaleX = offsetScaleX;
	}

	public float getOffsetScaleY () {
		return offsetScaleY;
	}

	public void setOffsetScaleY (float offsetScaleY) {
		this.offsetScaleY = offsetScaleY;
	}

	public float getOffsetShearY () {
		return offsetShearY;
	}

	public void setOffsetShearY (float offsetShearY) {
		this.offsetShearY = offsetShearY;
	}

	public TransformConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
