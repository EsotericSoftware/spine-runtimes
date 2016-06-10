
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;

import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;

public class TransformConstraint implements Updatable {
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

	public void apply () {
		update();
	}

	public void update () {
		float rotateMix = this.rotateMix, translateMix = this.translateMix, scaleMix = this.scaleMix, shearMix = this.shearMix;
		Bone target = this.target;
		float ta = target.a, tb = target.b, tc = target.c, td = target.d;
		Array<Bone> bones = this.bones;
		for (int i = 0, n = bones.size; i < n; i++) {
			Bone bone = bones.get(i);

			if (rotateMix > 0) {
				float a = bone.a, b = bone.b, c = bone.c, d = bone.d;
				float r = atan2(tc, ta) - atan2(c, a) + data.offsetRotation * degRad;
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

			if (translateMix > 0) {
				Vector2 temp = this.temp;
				target.localToWorld(temp.set(data.offsetX, data.offsetY));
				bone.worldX += (temp.x - bone.worldX) * translateMix;
				bone.worldY += (temp.y - bone.worldY) * translateMix;
			}

			if (scaleMix > 0) {
				float bs = (float)Math.sqrt(bone.a * bone.a + bone.c * bone.c);
				float ts = (float)Math.sqrt(ta * ta + tc * tc);
				float s = bs > 0.00001f ? (bs + (ts - bs + data.offsetScaleX) * scaleMix) / bs : 0;
				bone.a *= s;
				bone.c *= s;
				bs = (float)Math.sqrt(bone.b * bone.b + bone.d * bone.d);
				ts = (float)Math.sqrt(tb * tb + td * td);
				s = bs > 0.00001f ? (bs + (ts - bs + data.offsetScaleY) * scaleMix) / bs : 0;
				bone.b *= s;
				bone.d *= s;
			}

			if (shearMix > 0) {
				float b = bone.b, d = bone.d;
				float by = atan2(d, b);
				float r = atan2(td, tb) - atan2(tc, ta) - (by - atan2(bone.c, bone.a));
				if (r > PI)
					r -= PI2;
				else if (r < -PI) r += PI2;
				r = by + (r + data.offsetShearY * degRad) * shearMix;
				float s = (float)Math.sqrt(b * b + d * d);
				bone.b = cos(r) * s;
				bone.d = sin(r) * s;
			}
		}
	}

	public Array<Bone> getBones () {
		return bones;
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
