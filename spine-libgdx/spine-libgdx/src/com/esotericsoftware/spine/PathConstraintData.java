
package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.PathConstraint.RotateMode;
import com.esotericsoftware.spine.PathConstraint.SpacingMode;

public class PathConstraintData {
	final String name;
	final Array<BoneData> bones = new Array();
	SlotData target;
	SpacingMode spacingMode;
	RotateMode rotateMode;
	float offsetRotation;
	float position, spacing, rotateMix, translateMix;

	public PathConstraintData (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
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

	public SpacingMode getSpacingMode () {
		return spacingMode;
	}

	public void setSpacingMode (SpacingMode spacingMode) {
		this.spacingMode = spacingMode;
	}

	public RotateMode getRotateMode () {
		return rotateMode;
	}

	public void setRotateMode (RotateMode rotateMode) {
		this.rotateMode = rotateMode;
	}

	public float getOffsetRotation () {
		return offsetRotation;
	}

	public void setOffsetRotation (float offsetRotation) {
		this.offsetRotation = offsetRotation;
	}

	public float getPosition () {
		return position;
	}

	public void setPosition (float position) {
		this.position = position;
	}

	public float getSpacing () {
		return spacing;
	}

	public void setSpacing (float spacing) {
		this.spacing = spacing;
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

	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}
}
