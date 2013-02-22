
package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;

abstract public class Attachment {
	final String name;
	boolean resolved;

	public Attachment (String name) {
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.name = name;
	}

	abstract public void updateOffset ();

	abstract public void draw (SpriteBatch batch, Slot slot);

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
