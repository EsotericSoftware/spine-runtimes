
package com.esotericsoftware.spine;

public class BoneData {
	final BoneData parent;
	final String name;
	float length;
	float x, y;
	float rotation;
	float scaleX = 1, scaleY = 1;

	/** @param parent May be null. */
	public BoneData (String name, BoneData parent) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
		this.parent = parent;
	}

	/** Copy constructor.
	 * @param parent May be null. */
	public BoneData (BoneData bone, BoneData parent) {
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		this.parent = parent;
		name = bone.name;
		length = bone.length;
		x = bone.x;
		y = bone.y;
		rotation = bone.rotation;
		scaleX = bone.scaleX;
		scaleY = bone.scaleY;
	}

	/** @return May be null. */
	public BoneData getParent () {
		return parent;
	}

	public String getName () {
		return name;
	}

	public float getLength () {
		return length;
	}

	public void setLength (float length) {
		this.length = length;
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

	public float getRotation () {
		return rotation;
	}

	public void setRotation (float rotation) {
		this.rotation = rotation;
	}

	public float getScaleX () {
		return scaleX;
	}

	public void setScaleX (float scaleX) {
		this.scaleX = scaleX;
	}

	public float getScaleY () {
		return scaleY;
	}

	public void setScaleY (float scaleY) {
		this.scaleY = scaleY;
	}

	public String toString () {
		return name;
	}
}
