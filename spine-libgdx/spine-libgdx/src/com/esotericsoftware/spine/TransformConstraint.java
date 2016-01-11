
package com.esotericsoftware.spine;

import com.badlogic.gdx.math.Vector2;

public class TransformConstraint implements Updatable {
	final TransformConstraintData data;
	Bone bone, target;
	float translateMix, x, y;
	final Vector2 temp = new Vector2();

	public TransformConstraint (TransformConstraintData data, Skeleton skeleton) {
		this.data = data;
		translateMix = data.translateMix;

		if (skeleton != null) {
			bone = skeleton.findBone(data.bone.name);
			target = skeleton.findBone(data.target.name);
		}
	}

	/** Copy constructor. */
	public TransformConstraint (TransformConstraint constraint, Skeleton skeleton) {
		data = constraint.data;
		translateMix = data.translateMix;
		bone = skeleton.bones.get(constraint.bone.skeleton.bones.indexOf(constraint.target, true));
		target = skeleton.bones.get(constraint.target.skeleton.bones.indexOf(constraint.target, true));
	}

	public void apply () {
		update();
	}

	public void update () {
		float translateMix = this.translateMix;
		if (translateMix > 0) {
			Bone bone = this.bone;
			Vector2 temp = this.temp;
			target.localToWorld(temp.set(x, y));
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

	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public TransformConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}
}
