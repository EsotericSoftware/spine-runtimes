
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;

public class TransformConstraint implements Updatable {
	final TransformConstraintData data;
	Bone bone, target;
	float rotateMix, translateMix, scaleMix, shearMix;
	final Vector2 temp = new Vector2();

	public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
		this.data = data;
		rotateMix = data.rotateMix;
		translateMix = data.translateMix;
		scaleMix = data.scaleMix;
		shearMix = data.shearMix;

		if (skeleton != null) {
			bone = skeleton.findBone(data.bone.name);
			target = skeleton.findBone(data.target.name);
		}
	}

	/** Copy constructor. */
	public TransformConstraint (TransformConstraint constraint, Skeleton skeleton) {
		data = constraint.data;
		bone = skeleton.bones.get(constraint.bone.data.index);
		target = skeleton.bones.get(constraint.target.data.index);
		rotateMix = constraint.rotateMix;
		translateMix = constraint.translateMix;
		scaleMix = constraint.scaleMix;
		shearMix = constraint.shearMix;
	}

	public void apply () {
		update();
	}

	public void update () {
		Bone bone = this.bone;
		Bone target = this.target;

		if (rotateMix > 0) {
			float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
			float r = atan2(target.c, target.a) - atan2(c, a) + data.offsetRotation * degRad;
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
			float s = bs > 0.00001f ? (bs + (ts - bs + data.offsetScaleX) * scaleMix) / bs : 0;
			bone.a *= s;
			bone.c *= s;
			bs = (float)Math.sqrt(bone.b * bone.b + bone.d * bone.d);
			ts = (float)Math.sqrt(target.b * target.b + target.d * target.d);
			s = bs > 0.00001f ? (bs + (ts - bs + data.offsetScaleY) * scaleMix) / bs : 0;
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
			r = by + (r + data.offsetShearY * degRad) * shearMix;
			float s = (float)Math.sqrt(b * b + d * d);
			bone.b = cos(r) * s;
			bone.d = sin(r) * s;
		}

		float translateMix = this.translateMix;
		if (translateMix > 0) {
			Vector2 temp = this.temp;
			target.localToWorld(temp.set(data.offsetX, data.offsetY));
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

	public TransformConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
