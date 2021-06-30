/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.utils.Null;

/** Stores the setup pose for a {@link Bone}. */
public class BoneData {
	final int index;
	final String name;
	@Null final BoneData parent;
	float length;
	float x, y, rotation, scaleX = 1, scaleY = 1, shearX, shearY;
	TransformMode transformMode = TransformMode.normal;
	boolean skinRequired;

	// Nonessential.
	final Color color = new Color(0.61f, 0.61f, 0.61f, 1); // 9b9b9bff

	public BoneData (int index, String name, @Null BoneData parent) {
		if (index < 0) throw new IllegalArgumentException("index must be >= 0.");
		if (name == null) throw new IllegalArgumentException("name cannot be null.");
		this.index = index;
		this.name = name;
		this.parent = parent;
	}

	/** Copy constructor. */
	public BoneData (BoneData bone, @Null BoneData parent) {
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

	/** The name of the bone, which is unique across all bones in the skeleton. */
	public String getName () {
		return name;
	}

	public @Null BoneData getParent () {
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
		if (transformMode == null) throw new IllegalArgumentException("transformMode cannot be null.");
		this.transformMode = transformMode;
	}

	/** When true, {@link Skeleton#updateWorldTransform()} only updates this bone if the {@link Skeleton#getSkin()} contains this
	 * bone.
	 * <p>
	 * See {@link Skin#getBones()}. */
	public boolean getSkinRequired () {
		return skinRequired;
	}

	public void setSkinRequired (boolean skinRequired) {
		this.skinRequired = skinRequired;
	}

	/** The color of the bone as it was in Spine, or a default color if nonessential data was not exported. Bones are not usually
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
