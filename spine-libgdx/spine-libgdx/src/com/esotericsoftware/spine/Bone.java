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

import static com.esotericsoftware.spine.utils.SpineUtils.*;
import static com.badlogic.gdx.math.Matrix3.*;

import com.badlogic.gdx.math.Matrix3;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.BoneData.TransformMode;

/** Stores a bone's current pose.
 * <p>
 * A bone has a local transform which is used to compute its world transform. A bone also has an applied transform, which is a
 * local transform that can be applied to compute the world transform. The local transform and applied transform may differ if a
 * constraint or application code modifies the world transform after it was computed from the local transform. */
public class Bone implements Updatable {
	final BoneData data;
	final Skeleton skeleton;
	final Bone parent;
	final Array<Bone> children = new Array();
	float x, y, rotation, scaleX, scaleY, shearX, shearY;
	float ax, ay, arotation, ascaleX, ascaleY, ashearX, ashearY;
	boolean appliedValid;
	float a, b, worldX;
	float c, d, worldY;

	boolean sorted;

	/** @param parent May be null. */
	public Bone (BoneData data, Skeleton skeleton, Bone parent) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		this.skeleton = skeleton;
		this.parent = parent;
		setToSetupPose();
	}

	/** Copy constructor. Does not copy the children bones.
	 * @param parent May be null. */
	public Bone (Bone bone, Skeleton skeleton, Bone parent) {
		if (bone == null) throw new IllegalArgumentException("bone cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.skeleton = skeleton;
		this.parent = parent;
		data = bone.data;
		x = bone.x;
		y = bone.y;
		rotation = bone.rotation;
		scaleX = bone.scaleX;
		scaleY = bone.scaleY;
		shearX = bone.shearX;
		shearY = bone.shearY;
	}

	/** Same as {@link #updateWorldTransform()}. This method exists for Bone to implement {@link Updatable}. */
	public void update () {
		updateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
	}

	/** Computes the world transform using the parent bone and this bone's local transform.
	 * <p>
	 * See {@link #updateWorldTransform(float, float, float, float, float, float, float)}. */
	public void updateWorldTransform () {
		updateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
	}

	/** Computes the world transform using the parent bone and the specified local transform. Child bones are not updated.
	 * <p>
	 * See <a href="http://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide. */
	public void updateWorldTransform (float x, float y, float rotation, float scaleX, float scaleY, float shearX, float shearY) {
		ax = x;
		ay = y;
		arotation = rotation;
		ascaleX = scaleX;
		ascaleY = scaleY;
		ashearX = shearX;
		ashearY = shearY;
		appliedValid = true;

		Bone parent = this.parent;
		if (parent == null) { // Root bone.
			float rotationY = rotation + 90 + shearY;
			float la = cosDeg(rotation + shearX) * scaleX;
			float lb = cosDeg(rotationY) * scaleY;
			float lc = sinDeg(rotation + shearX) * scaleX;
			float ld = sinDeg(rotationY) * scaleY;
			Skeleton skeleton = this.skeleton;
			if (skeleton.flipX) {
				x = -x;
				la = -la;
				lb = -lb;
			}
			if (skeleton.flipY) {
				y = -y;
				lc = -lc;
				ld = -ld;
			}
			a = la;
			b = lb;
			c = lc;
			d = ld;
			worldX = x + skeleton.x;
			worldY = y + skeleton.y;
			return;
		}

		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		worldX = pa * x + pb * y + parent.worldX;
		worldY = pc * x + pd * y + parent.worldY;

		switch (data.transformMode) {
		case normal: {
			float rotationY = rotation + 90 + shearY;
			float la = cosDeg(rotation + shearX) * scaleX;
			float lb = cosDeg(rotationY) * scaleY;
			float lc = sinDeg(rotation + shearX) * scaleX;
			float ld = sinDeg(rotationY) * scaleY;
			a = pa * la + pb * lc;
			b = pa * lb + pb * ld;
			c = pc * la + pd * lc;
			d = pc * lb + pd * ld;
			return;
		}
		case onlyTranslation: {
			float rotationY = rotation + 90 + shearY;
			a = cosDeg(rotation + shearX) * scaleX;
			b = cosDeg(rotationY) * scaleY;
			c = sinDeg(rotation + shearX) * scaleX;
			d = sinDeg(rotationY) * scaleY;
			break;
		}
		case noRotationOrReflection: {
			float s = pa * pa + pc * pc, prx;
			if (s > 0.0001f) {
				s = Math.abs(pa * pd - pb * pc) / s;
				pb = pc * s;
				pd = pa * s;
				prx = atan2(pc, pa) * radDeg;
			} else {
				pa = 0;
				pc = 0;
				prx = 90 - atan2(pd, pb) * radDeg;
			}
			float rx = rotation + shearX - prx;
			float ry = rotation + shearY - prx + 90;
			float la = cosDeg(rx) * scaleX;
			float lb = cosDeg(ry) * scaleY;
			float lc = sinDeg(rx) * scaleX;
			float ld = sinDeg(ry) * scaleY;
			a = pa * la - pb * lc;
			b = pa * lb - pb * ld;
			c = pc * la + pd * lc;
			d = pc * lb + pd * ld;
			break;
		}
		case noScale:
		case noScaleOrReflection: {
			float cos = cosDeg(rotation), sin = sinDeg(rotation);
			float za = pa * cos + pb * sin;
			float zc = pc * cos + pd * sin;
			float s = (float)Math.sqrt(za * za + zc * zc);
			if (s > 0.00001f) s = 1 / s;
			za *= s;
			zc *= s;
			s = (float)Math.sqrt(za * za + zc * zc);
			float r = PI / 2 + atan2(zc, za);
			float zb = cos(r) * s;
			float zd = sin(r) * s;
			float la = cosDeg(shearX) * scaleX;
			float lb = cosDeg(90 + shearY) * scaleY;
			float lc = sinDeg(shearX) * scaleX;
			float ld = sinDeg(90 + shearY) * scaleY;			
			if (data.transformMode != TransformMode.noScaleOrReflection ? pa * pd - pb * pc < 0 : skeleton.flipX != skeleton.flipY) {
			    zb = -zb;
			    zd = -zd;
			}			
			a = za * la + zb * lc;
			b = za * lb + zb * ld;
			c = zc * la + zd * lc;
			d = zc * lb + zd * ld;
			return;
		}
		}
		if (skeleton.flipX) {
			a = -a;
			b = -b;
		}
		if (skeleton.flipY) {
			c = -c;
			d = -d;
		}
	}

	/** Sets this bone's local transform to the setup pose. */
	public void setToSetupPose () {
		BoneData data = this.data;
		x = data.x;
		y = data.y;
		rotation = data.rotation;
		scaleX = data.scaleX;
		scaleY = data.scaleY;
		shearX = data.shearX;
		shearY = data.shearY;
	}

	/** The bone's setup pose data. */
	public BoneData getData () {
		return data;
	}

	/** The skeleton this bone belongs to. */
	public Skeleton getSkeleton () {
		return skeleton;
	}

	/** The parent bone, or null if this is the root bone. */
	public Bone getParent () {
		return parent;
	}

	/** The immediate children of this bone. */
	public Array<Bone> getChildren () {
		return children;
	}

	// -- Local transform

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

	public void setScale (float scale) {
		scaleX = scale;
		scaleY = scale;
	}

	/** The local shearX. */
	public float getShearX () {
		return shearX;
	}

	public void setShearX (float shearX) {
		this.shearX = shearX;
	}

	/** The local shearY. */
	public float getShearY () {
		return shearY;
	}

	public void setShearY (float shearY) {
		this.shearY = shearY;
	}

	// -- Applied transform

	/** The applied local x translation. */
	public float getAX () {
		return ax;
	}

	public void setAX (float ax) {
		this.ax = ax;
	}

	/** The applied local y translation. */
	public float getAY () {
		return ay;
	}

	public void setAY (float ay) {
		this.ay = ay;
	}

	/** The applied local rotation. */
	public float getARotation () {
		return arotation;
	}

	public void setARotation (float arotation) {
		this.arotation = arotation;
	}

	/** The applied local scaleX. */
	public float getAScaleX () {
		return ascaleX;
	}

	public void setAScaleX (float ascaleX) {
		this.ascaleX = ascaleX;
	}

	/** The applied local scaleY. */
	public float getAScaleY () {
		return ascaleY;
	}

	public void setAScaleY (float ascaleY) {
		this.ascaleY = ascaleY;
	}

	/** The applied local shearX. */
	public float getAShearX () {
		return ashearX;
	}

	public void setAShearX (float ashearX) {
		this.ashearX = ashearX;
	}

	/** The applied local shearY. */
	public float getAShearY () {
		return ashearY;
	}

	public void setAShearY (float ashearY) {
		this.ashearY = ashearY;
	}

	/** If true, the applied transform matches the world transform. If false, the world transform has been modified since it was
	 * computed and {@link #updateAppliedTransform()} must be called before accessing the applied transform. */
	public boolean isAppliedValid () {
		return appliedValid;
	}

	public void setAppliedValid (boolean appliedValid) {
		this.appliedValid = appliedValid;
	}

	/** Computes the applied transform values from the world transform. This allows the applied transform to be accessed after the
	 * world transform has been modified (by a constraint, {@link #rotateWorld(float)}, etc).
	 * <p>
	 * If {@link #updateWorldTransform()} has been called for a bone and {@link #isAppliedValid()} is false, then
	 * {@link #updateAppliedTransform()} must be called before accessing the applied transform.
	 * <p>
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The applied transform after
	 * calling this method is equivalent to the local tranform used to compute the world transform, but may not be identical. */
	public void updateAppliedTransform () {
		appliedValid = true;
		Bone parent = this.parent;
		if (parent == null) {
			ax = worldX;
			ay = worldY;
			arotation = atan2(c, a) * radDeg;
			ascaleX = (float)Math.sqrt(a * a + c * c);
			ascaleY = (float)Math.sqrt(b * b + d * d);
			ashearX = 0;
			ashearY = atan2(a * b + c * d, a * d - b * c) * radDeg;
			return;
		}
		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		float pid = 1 / (pa * pd - pb * pc);
		float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
		ax = (dx * pd * pid - dy * pb * pid);
		ay = (dy * pa * pid - dx * pc * pid);
		float ia = pid * pd;
		float id = pid * pa;
		float ib = pid * pb;
		float ic = pid * pc;
		float ra = ia * a - ib * c;
		float rb = ia * b - ib * d;
		float rc = id * c - ic * a;
		float rd = id * d - ic * b;
		ashearX = 0;
		ascaleX = (float)Math.sqrt(ra * ra + rc * rc);
		if (ascaleX > 0.0001f) {
			float det = ra * rd - rb * rc;
			ascaleY = det / ascaleX;
			ashearY = atan2(ra * rb + rc * rd, det) * radDeg;
			arotation = atan2(rc, ra) * radDeg;
		} else {
			ascaleX = 0;
			ascaleY = (float)Math.sqrt(rb * rb + rd * rd);
			ashearY = 0;
			arotation = 90 - atan2(rd, rb) * radDeg;
		}
	}

	// -- World transform

	/** Part of the world transform matrix for the X axis. If changed, {@link #setAppliedValid(boolean)} should be set to false. */
	public float getA () {
		return a;
	}

	public void setA (float a) {
		this.a = a;
	}

	/** Part of the world transform matrix for the Y axis. If changed, {@link #setAppliedValid(boolean)} should be set to false. */
	public float getB () {
		return b;
	}

	public void setB (float b) {
		this.b = b;
	}

	/** Part of the world transform matrix for the X axis. If changed, {@link #setAppliedValid(boolean)} should be set to false. */
	public float getC () {
		return c;
	}

	public void setC (float c) {
		this.c = c;
	}

	/** Part of the world transform matrix for the Y axis. If changed, {@link #setAppliedValid(boolean)} should be set to false. */
	public float getD () {
		return d;
	}

	public void setD (float d) {
		this.d = d;
	}

	/** The world X position. If changed, {@link #setAppliedValid(boolean)} should be set to false. */
	public float getWorldX () {
		return worldX;
	}

	public void setWorldX (float worldX) {
		this.worldX = worldX;
	}

	/** The world Y position. If changed, {@link #setAppliedValid(boolean)} should be set to false. */
	public float getWorldY () {
		return worldY;
	}

	public void setWorldY (float worldY) {
		this.worldY = worldY;
	}

	/** The world rotation for the X axis, calculated using {@link #a} and {@link #c}. */
	public float getWorldRotationX () {
		return atan2(c, a) * radDeg;
	}

	/** The world rotation for the Y axis, calculated using {@link #b} and {@link #d}. */
	public float getWorldRotationY () {
		return atan2(d, b) * radDeg;
	}

	/** The magnitude (always positive) of the world scale X, calculated using {@link #a} and {@link #c}. */
	public float getWorldScaleX () {
		return (float)Math.sqrt(a * a + c * c);
	}

	/** The magnitude (always positive) of the world scale Y, calculated using {@link #b} and {@link #d}. */
	public float getWorldScaleY () {
		return (float)Math.sqrt(b * b + d * d);
	}

	public Matrix3 getWorldTransform (Matrix3 worldTransform) {
		if (worldTransform == null) throw new IllegalArgumentException("worldTransform cannot be null.");
		float[] val = worldTransform.val;
		val[M00] = a;
		val[M01] = b;
		val[M10] = c;
		val[M11] = d;
		val[M02] = worldX;
		val[M12] = worldY;
		val[M20] = 0;
		val[M21] = 0;
		val[M22] = 1;
		return worldTransform;
	}

	/** Transforms a point from world coordinates to the bone's local coordinates. */
	public Vector2 worldToLocal (Vector2 world) {
		float invDet = 1 / (a * d - b * c);
		float x = world.x - worldX, y = world.y - worldY;
		world.x = x * d * invDet - y * b * invDet;
		world.y = y * a * invDet - x * c * invDet;
		return world;
	}

	/** Transforms a point from the bone's local coordinates to world coordinates. */
	public Vector2 localToWorld (Vector2 local) {
		float x = local.x, y = local.y;
		local.x = x * a + y * b + worldX;
		local.y = x * c + y * d + worldY;
		return local;
	}

	/** Transforms a world rotation to a local rotation. */
	public float worldToLocalRotation (float worldRotation) {
		float sin = sinDeg(worldRotation), cos = cosDeg(worldRotation);
		return atan2(a * sin - c * cos, d * cos - b * sin) * radDeg;
	}

	/** Transforms a local rotation to a world rotation. */
	public float localToWorldRotation (float localRotation) {
		float sin = sinDeg(localRotation), cos = cosDeg(localRotation);
		return atan2(cos * c + sin * d, cos * a + sin * b) * radDeg;
	}

	/** Rotates the world transform the specified amount and sets {@link #isAppliedValid()} to false.
	 * {@link #updateWorldTransform()} will need to be called on any child bones, recursively, and any constraints reapplied. */
	public void rotateWorld (float degrees) {
		float cos = cosDeg(degrees), sin = sinDeg(degrees);
		a = cos * a - sin * c;
		b = cos * b - sin * d;
		c = sin * a + cos * c;
		d = sin * b + cos * d;
		appliedValid = false;
	}

	// ---

	public String toString () {
		return data.name;
	}
}
