
package com.esotericsoftware.spine;

public class TransformConstraintData {
	final String name;
	BoneData bone, target;
	float translateMix;
	float x, y;

	public TransformConstraintData (String name) {
		this.name = name;
	}

	public String getName () {
		return name;
	}

	public BoneData getBone () {
		return bone;
	}

	public void setBone (BoneData bone) {
		this.bone = bone;
	}

	public BoneData getTarget () {
		return target;
	}

	public void setTarget (BoneData target) {
		this.target = target;
	}

	public float getTranslateMix () {
		return translateMix;
	}

	public void setTranslateMix (float translateMix) {
		this.translateMix = translateMix;
	}

	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	public String toString () {
		return name;
	}
}
