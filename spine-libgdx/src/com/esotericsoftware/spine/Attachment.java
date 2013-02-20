
package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;

abstract public class Attachment {
	final String name;
	boolean resolved;
	private float x, y, scaleX, scaleY, rotation, width, height;

	public Attachment (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	abstract public void updateOffset ();

	abstract public void draw (SpriteBatch batch, Slot slot);

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

	public float getRotation () {
		return rotation;
	}

	public void setRotation (float rotation) {
		this.rotation = rotation;
	}

	public float getWidth () {
		return width;
	}

	public void setWidth (float width) {
		this.width = width;
	}

	public float getHeight () {
		return height;
	}

	public void setHeight (float height) {
		this.height = height;
	}

	public boolean isResolved () {
		return resolved;
	}

	public void setResolved (boolean resolved) {
		this.resolved = resolved;
	}

	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}
}
