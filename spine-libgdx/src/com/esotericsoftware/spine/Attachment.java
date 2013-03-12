
package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;

abstract public class Attachment {
	final String name;

	public Attachment (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	abstract public void draw (SpriteBatch batch, Slot slot);

	public String getName () {
		return name;
	}

	public String toString () {
		return name;
	}
}
