/******************************************************************************
 * Spine Runtimes Software License
 * Version 2
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software, you may not (a) modify, translate, adapt or
 * otherwise create derivative works, improvements of the Software or develop
 * new applications using the Software or (b) remove, delete, alter or obscure
 * any trademarks or any copyright, trademark, patent or other intellectual
 * property or proprietary rights notices on or in the Software, including
 * any copy thereof. Redistributions in binary or source form must include
 * this license and terms. THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.Matrix3.*;

import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Matrix3;

public class Bone {
	final BoneData data;
	final Bone parent;
	float x, y;
	float rotation;
	float scaleX, scaleY;

	float m00, m01, worldX; // a b x
	float m10, m11, worldY; // c d y
	float worldRotation;
	float worldScaleX, worldScaleY;

	/** @param parent May be null. */
	public Bone (BoneData data, Bone parent) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		this.data = data;
		this.parent = parent;
		setToSetupPose();
	}

	/** Copy constructor.
	 * @param parent May be null. */
	public Bone (Bone bone, Bone parent) {
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		this.parent = parent;
		data = bone.data;
		x = bone.x;
		y = bone.y;
		rotation = bone.rotation;
		scaleX = bone.scaleX;
		scaleY = bone.scaleY;
	}

	/** Computes the world SRT using the parent bone and the local SRT. */
	public void updateWorldTransform (boolean flipX, boolean flipY) {
		Bone parent = this.parent;
		if (parent != null) {
			worldX = x * parent.m00 + y * parent.m01 + parent.worldX;
			worldY = x * parent.m10 + y * parent.m11 + parent.worldY;
			if (data.inheritScale) {
				worldScaleX = parent.worldScaleX * scaleX;
				worldScaleY = parent.worldScaleY * scaleY;
			} else {
				worldScaleX = scaleX;
				worldScaleY = scaleY;
			}
			worldRotation = data.inheritRotation ? parent.worldRotation + rotation : rotation;
		} else {
			worldX = flipX ? -x : x;
			worldY = flipY ? -y : y;
			worldScaleX = scaleX;
			worldScaleY = scaleY;
			worldRotation = rotation;
		}
		float cos = MathUtils.cosDeg(worldRotation);
		float sin = MathUtils.sinDeg(worldRotation);
		m00 = cos * worldScaleX;
		m10 = sin * worldScaleX;
		m01 = -sin * worldScaleY;
		m11 = cos * worldScaleY;
		if (flipX) {
			m00 = -m00;
			m01 = -m01;
		}
		if (flipY) {
			m10 = -m10;
			m11 = -m11;
		}
	}

	public void setToSetupPose () {
		BoneData data = this.data;
		x = data.x;
		y = data.y;
		rotation = data.rotation;
		scaleX = data.scaleX;
		scaleY = data.scaleY;
	}

	public BoneData getData () {
		return data;
	}

	public Bone getParent () {
		return parent;
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

	public float getM00 () {
		return m00;
	}

	public float getM01 () {
		return m01;
	}

	public float getM10 () {
		return m10;
	}

	public float getM11 () {
		return m11;
	}

	public float getWorldX () {
		return worldX;
	}

	public float getWorldY () {
		return worldY;
	}

	public float getWorldRotation () {
		return worldRotation;
	}

	public float getWorldScaleX () {
		return worldScaleX;
	}

	public float getWorldScaleY () {
		return worldScaleY;
	}

	public Matrix3 getWorldTransform (Matrix3 worldTransform) {
		if (worldTransform == null) throw new IllegalArgumentException("worldTransform cannot be null.");
		float[] val = worldTransform.val;
		val[M00] = m00;
		val[M01] = m01;
		val[M10] = m10;
		val[M11] = m11;
		val[M02] = worldX;
		val[M12] = worldY;
		val[M20] = 0;
		val[M21] = 0;
		val[M22] = 1;
		return worldTransform;
	}

	public String toString () {
		return data.name;
	}
}
