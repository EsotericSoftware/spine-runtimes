/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

package com.esotericsoftware.spine;

public class BoneData {
	final BoneData parent;
	final String name;
	float length;
	float x, y;
	float rotation;
	float scaleX = 1, scaleY = 1;
	boolean inheritScale = true, inheritRotation = true;

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

	public boolean getInheritScale () {
		return inheritScale;
	}

	public void setInheritScale (boolean inheritScale) {
		this.inheritScale = inheritScale;
	}

	public boolean getInheritRotation () {
		return inheritRotation;
	}

	public void setInheritRotation (boolean inheritRotation) {
		this.inheritRotation = inheritRotation;
	}

	public String toString () {
		return name;
	}
}
