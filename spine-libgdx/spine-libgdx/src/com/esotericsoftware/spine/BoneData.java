/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;

/** Stores the setup pose for a {@link Bone}. */
public class BoneData {
	final int index;
	final String name;
	final BoneData parent;
	float length;
	float x, y, rotation, scaleX = 1, scaleY = 1, shearX, shearY;
	TransformMode transformMode = TransformMode.normal;

	// Nonessential.
	final Color color = new Color(0.61f, 0.61f, 0.61f, 1); // 9b9b9bff

	/** @param parent May be null. */
	public BoneData (int index, String name, BoneData parent) {
		if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.index = index;
		this.name = name;
		this.parent = parent;
	}

	/** Copy constructor.
	 * @param parent May be null. */
	public BoneData (BoneData bone, BoneData parent) {
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		index = bone.index;
		name = bone.name;
		this.parent = parent;
		length = bone.length;
		x = bone.x;
		y = bone.y;
		rotation = bone.rotation;
		scaleX = bone.scaleX;
		scaleY = bone.scaleY;
		shearX = bone.shearX;
		shearY = bone.shearY;
	}

	/** The index of the bone in {@link Skeleton#getBones()}. */
	public int getIndex () {
		return index;
	}

	/** The name of the bone, which is unique within the skeleton. */
	public String getName () {
		return name;
	}

	/** @return May be null. */
	public BoneData getParent () {
		return parent;
	}

	/** The bone's length. */
	public float getLength () {
		return length;
	}

	public void setLength (float length) {
		this.length = length;
	}

	/** The local x translation. */
	public float getX () {
		return x;
	}

	public void setX (float x) {
		this.x = x;
	}

	/** The local y translation. */
	public float getY () {
		return y;
	}

	public void setY (float y) {
		this.y = y;
	}

	public void setPosition (float x, float y) {
		this.x = x;
		this.y = y;
	}

	/** The local rotation. */
	public float getRotation () {
		return rotation;
	}

	public void setRotation (float rotation) {
		this.rotation = rotation;
	}

	/** The local scaleX. */
	public float getScaleX () {
		return scaleX;
	}

	public void setScaleX (float scaleX) {
		this.scaleX = scaleX;
	}

	/** The local scaleY. */
	public float getScaleY () {
		return scaleY;
	}

	public void setScaleY (float scaleY) {
		this.scaleY = scaleY;
	}

	public void setScale (float scaleX, float scaleY) {
		this.scaleX = scaleX;
		this.scaleY = scaleY;
	}

	/** The local shearX. */
	public float getShearX () {
		return shearX;
	}

	public void setShearX (float shearX) {
		this.shearX = shearX;
	}

	/** The local shearX. */
	public float getShearY () {
		return shearY;
	}

	public void setShearY (float shearY) {
		this.shearY = shearY;
	}

	/** The transform mode for how parent world transforms affect this bone. */
	public TransformMode getTransformMode () {
		return transformMode;
	}

	public void setTransformMode (TransformMode transformMode) {
		this.transformMode = transformMode;
	}

	/** The color of the bone as it was in Spine. Available only when nonessential data was exported. Bones are not usually
	 * rendered at runtime. */
	public Color getColor () {
		return color;
	}

	public String toString () {
		return name;
	}

	/** Determines how a bone inherits world transforms from parent bones. */
	static public enum TransformMode {
		normal, onlyTranslation, noRotationOrReflection, noScale, noScaleOrReflection;

		static public final TransformMode[] values = TransformMode.values();
	}
}
