
package com.esotericsoftware.spine;

public class PathConstraintData {
	final String name;
	BoneData bone;
	SlotData target;
	float position, rotateMix, translateMix;
	float offsetRotation;

	public PathConstraintData (String name) {
		this.name = name;
	}

	public BoneData getBone () {
		return bone;
	}

	public void setBone (BoneData bone) {
		this.bone = bone;
	}

	public SlotData getTarget () {
		return target;
	}

	public void setTarget (SlotData target) {
		this.target = target;
	}

	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
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

	public float getOffsetRotation () {
		return offsetRotation;
	}

	public void setOffsetRotation (float offsetRotation) {
		this.offsetRotation = offsetRotation;
	}

	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}
}
