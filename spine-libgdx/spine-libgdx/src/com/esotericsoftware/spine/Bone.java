/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.MathUtils.*;
import static com.badlogic.gdx.math.Matrix3.*;

import com.badlogic.gdx.math.Matrix3;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.utils.Array;
import com.esotericsoftware.spine.BoneData.TransformMode;

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

	/** Computes the world transform using the parent bone and this bone's local transform. */
	public void updateWorldTransform () {
		updateWorldTransform(x, y, rotation, scaleX, scaleY, shearX, shearY);
	}

	/** Computes the world transform using the parent bone and the specified local transform. */
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
		case noRotation: {
			if (false) {
				// Summing parent rotations.
				// 1) Negative parent scale causes bone to rotate.
				float sum = 0;
				Bone current = parent;
				while (current != null) {
					sum += current.arotation;
					current = current.parent;
				}
				rotation -= sum;
				float rotationY = rotation + 90 + shearY;
				float la = cosDeg(rotation + shearX) * scaleX;
				float lb = cosDeg(rotationY) * scaleY;
				float lc = sinDeg(rotation + shearX) * scaleX;
				float ld = sinDeg(rotationY) * scaleY;
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			} else if (true) {
				// Old way.
				// 1) Immediate parent scale is applied in wrong direction.
				// 2) Negative parent scale causes bone to rotate.
				pa = 1;
				pb = 0;
				pc = 0;
				pd = 1;
				float rotationY, la, lb, lc, ld;
				outer:
				do {
					if (!parent.appliedValid) parent.updateAppliedTransform();
					float pr = parent.arotation, psx = parent.ascaleX;
					rotationY = pr + 90 + parent.ashearY;
					la = cosDeg(pr + parent.shearX);
					lb = cosDeg(rotationY);
					lc = sinDeg(pr + parent.shearX);
					ld = sinDeg(rotationY);
					float temp = (pa * la + pb * lc) * psx;
					pb = (pb * ld + pa * lb) * parent.ascaleY;
					pa = temp;
					temp = (pc * la + pd * lc) * psx;
					pd = (pd * ld + pc * lb) * parent.ascaleY;
					pc = temp;

					if (psx < 0) lc = -lc;
					temp = pa * la - pb * lc;
					pb = pb * ld - pa * lb;
					pa = temp;
					temp = pc * la - pd * lc;
					pd = pd * ld - pc * lb;
					pc = temp;

					switch (parent.data.transformMode) {
					case noScale:
					case noScaleOrReflection:
						break outer;
					}
					parent = parent.parent;
				} while (parent != null);
				rotationY = rotation + 90 + shearY;
				la = cosDeg(rotation + shearX) * scaleX;
				lb = cosDeg(rotationY) * scaleY;
				lc = sinDeg(rotation + shearX) * scaleX;
				ld = sinDeg(rotationY) * scaleY;
				a = pa * la + pb * lc;
				b = pa * lb + pb * ld;
				c = pc * la + pd * lc;
				d = pc * lb + pd * ld;
			} else {
				// New way.
				// 1) Negative scale can cause bone to flip.
				float psx = (float)Math.sqrt(pa * pa + pc * pc), psy, pr;
				if (psx > 0.0001f) {
					float det = pa * pd - pb * pc;
					psy = det / psx;
					pr = atan2(pc, pa) * radDeg;
				} else {
					psx = 0;
					psy = (float)Math.sqrt(pb * pb + pd * pd);
					pr = 90 - atan2(pd, pb) * radDeg;
				}
				float blend;
				if (pr < -90)
					blend = 1 + (pr + 90) / 90;
				else if (pr < 0)
					blend = -pr / 90;
				else if (pr < 90)
					blend = pr / 90;
				else
					blend = 1 - (pr - 90) / 90;
				pa = psx + (Math.abs(psy) * Math.signum(psx) - psx) * blend;
				pd = psy + (Math.abs(psx) * Math.signum(psy) - psy) * blend;
				float rotationY = rotation + 90 + shearY;
				a = pa * cosDeg(rotation + shearX) * scaleX;
				b = pa * cosDeg(rotationY) * scaleY;
				c = pd * sinDeg(rotation + shearX) * scaleX;
				d = pd * sinDeg(rotationY) * scaleY;
			}
			break;
		}
		case noScale:
		case noScaleOrReflection: {
			float cos = cosDeg(rotation), sin = sinDeg(rotation);
			float za = pa * cos + pb * sin, zb = za;
			float zc = pc * cos + pd * sin, zd = zc;
			float s = (float)Math.sqrt(za * za + zc * zc);
			if (s > 0.00001f) s = 1 / s;
			za *= s;
			zc *= s;
			s = (float)Math.sqrt(zb * zb + zd * zd);
			if (s > 0.00001f) s = 1 / s;
			zb *= s;
			zd *= s;
			float by = atan2(zd, zb), r = PI / 2 - (by - atan2(zc, za));
			if (r > PI)
				r -= PI2;
			else if (r < -PI) r += PI2;
			r += by;
			s = (float)Math.sqrt(zb * zb + zd * zd);
			zb = cos(r) * s;
			zd = sin(r) * s;
			float la = cosDeg(shearX) * scaleX;
			float lb = cosDeg(90 + shearY) * scaleY;
			float lc = sinDeg(shearX) * scaleX;
			float ld = sinDeg(90 + shearY) * scaleY;
			a = za * la + zb * lc;
			b = za * lb + zb * ld;
			c = zc * la + zd * lc;
			d = zc * lb + zd * ld;
			if (data.transformMode != TransformMode.noScaleOrReflection ? pa * pd - pb * pc < 0 : skeleton.flipX != skeleton.flipY) {
				b = -b;
				d = -d;
			}
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

	public BoneData getData () {
		return data;
	}

	public Skeleton getSkeleton () {
		return skeleton;
	}

	public Bone getParent () {
		return parent;
	}

	public Array<Bone> getChildren () {
		return children;
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

	public void setPosition (float x, float y) {
		this.x = x;
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

	public void setScale (float scaleX, float scaleY) {
		this.scaleX = scaleX;
		this.scaleY = scaleY;
	}

	public void setScale (float scale) {
		scaleX = scale;
		scaleY = scale;
	}

	public float getShearX () {
		return shearX;
	}

	public void setShearX (float shearX) {
		this.shearX = shearX;
	}

	public float getShearY () {
		return shearY;
	}

	public void setShearY (float shearY) {
		this.shearY = shearY;
	}

	public float getA () {
		return a;
	}

	public float getB () {
		return b;
	}

	public float getC () {
		return c;
	}

	public float getD () {
		return d;
	}

	public float getWorldX () {
		return worldX;
	}

	public float getWorldY () {
		return worldY;
	}

	public float getWorldRotationX () {
		return atan2(c, a) * radDeg;
	}

	public float getWorldRotationY () {
		return atan2(d, b) * radDeg;
	}

	/** Returns the magnitude (always positive) of the world scale X. */
	public float getWorldScaleX () {
		return (float)Math.sqrt(a * a + c * c);
	}

	/** Returns the magnitude (always positive) of the world scale Y. */
	public float getWorldScaleY () {
		return (float)Math.sqrt(b * b + d * d);
	}

	public float worldToLocalRotationX () {
		Bone parent = this.parent;
		if (parent == null) return arotation;
		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, a = this.a, c = this.c;
		return atan2(pa * c - pc * a, pd * a - pb * c) * radDeg;
	}

	public float worldToLocalRotationY () {
		Bone parent = this.parent;
		if (parent == null) return arotation;
		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d, b = this.b, d = this.d;
		return atan2(pa * d - pc * b, pd * b - pb * d) * radDeg;
	}

	public void rotateWorld (float degrees) {
		float a = this.a, b = this.b, c = this.c, d = this.d;
		float cos = cosDeg(degrees), sin = sinDeg(degrees);
		this.a = cos * a - sin * c;
		this.b = cos * b - sin * d;
		this.c = sin * a + cos * c;
		this.d = sin * b + cos * d;
		appliedValid = false;
	}

	/** Computes the individual applied transform values from the world transform. This can be useful to perform processing using
	 * the applied transform after the world transform has been modified directly (eg, by a constraint).
	 * <p>
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. */
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

	public Vector2 worldToLocal (Vector2 world) {
		float a = this.a, b = this.b, c = this.c, d = this.d;
		float invDet = 1 / (a * d - b * c);
		float x = world.x - worldX, y = world.y - worldY;
		world.x = (x * d * invDet - y * b * invDet);
		world.y = (y * a * invDet - x * c * invDet);
		return world;
	}

	public Vector2 localToWorld (Vector2 local) {
		float x = local.x, y = local.y;
		local.x = x * a + y * b + worldX;
		local.y = x * c + y * d + worldY;
		return local;
	}

	public String toString () {
		return data.name;
	}
}
