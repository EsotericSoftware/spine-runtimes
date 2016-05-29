
package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

public class PathConstraintData {
	final String name;
	final Array<BoneData> bones = new Array();
	SlotData target;
	float position, rotateMix, translateMix, scaleMix;
	float offsetRotation;

	public PathConstraintData (String name) {
		this.name = name;
	}

	public Array<BoneData> getBones () {
		return bones;
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

	public float getScaleMix () {
		return scaleMix;
	}

	public void setScaleMix (float scaleMix) {
		this.scaleMix = scaleMix;
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
