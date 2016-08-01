
package com.esotericsoftware.spine;

import com.badlogic.gdx.utils.Array;

public class PathConstraintData {
	final String name;
	final Array<BoneData> bones = new Array();
	SlotData target;
	PositionMode positionMode;
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

	public PositionMode getPositionMode () {
		return positionMode;
	}

	public void setPositionMode (PositionMode positionMode) {
		this.positionMode = positionMode;
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

	static public enum PositionMode {
		fixed, percent;

		static public final PositionMode[] values = PositionMode.values();
	}

	static public enum SpacingMode {
		length, fixed, percent;

		static public final SpacingMode[] values = SpacingMode.values();
	}

	static public enum RotateMode {
		tangent, chain, chainScale;

		static public final RotateMode[] values = RotateMode.values();
	}
}
