
package com.esotericsoftware.spine;

import static com.badlogic.gdx.math.Matrix3.*;
import static com.esotericsoftware.spine.utils.SpineUtils.*;

import com.badlogic.gdx.math.Matrix3;
import com.badlogic.gdx.math.Vector2;

import com.esotericsoftware.spine.BoneData.Inherit;

/** The applied local pose and world transform for a bone. This is the {@link Bone#pose} with constraints applied and the world
 * transform computed by {@link Skeleton#updateWorldTransform(Physics)} and {@link #updateWorldTransform(Skeleton)}.
 * <p>
 * If the world transform is changed, call {@link #updateLocalTransform(Skeleton)} before using the local transform. The local
 * transform may be needed by other code (eg to apply another constraint).
 * <p>
 * After changing the world transform, call {@link #updateWorldTransform(Skeleton)} on every descendant bone. It may be more
 * convenient to modify the local transform instead, then call {@link Skeleton#updateWorldTransform(Physics)} to update the world
 * transforms for all bones and apply constraints. */
public class BonePose implements Pose<BonePose>, Update {
	Bone bone;

	float x, y, rotation, scaleX, scaleY, shearX, shearY;
	Inherit inherit;

	float a, b, worldX;
	float c, d, worldY;
	int world, local;

	public void set (BonePose pose) {
		if (pose == null) throw new IllegalArgumentException("pose cannot be null.");
		x = pose.x;
		y = pose.y;
		rotation = pose.rotation;
		scaleX = pose.scaleX;
		scaleY = pose.scaleY;
		shearX = pose.shearX;
		shearY = pose.shearY;
		inherit = pose.inherit;
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

	/** Sets local x and y translation. */
	public void setPosition (float x, float y) {
		this.x = x;
		this.y = y;
	}

	/** The local rotation in degrees, counter clockwise. */
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

	/** Sets local scaleX and scaleY. */
	public void setScale (float scaleX, float scaleY) {
		this.scaleX = scaleX;
		this.scaleY = scaleY;
	}

	/** Sets local scaleX and scaleY to the same value. */
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

	/** Determines how parent world transforms affect this bone. */
	public Inherit getInherit () {
		return inherit;
	}

	public void setInherit (Inherit inherit) {
		if (inherit == null) throw new IllegalArgumentException("inherit cannot be null.");
		this.inherit = inherit;
	}

	/** Called by {@link Skeleton#updateCache()} to compute the world transform, if needed. */
	public void update (Skeleton skeleton, Physics physics) {
		if (world != skeleton.update) updateWorldTransform(skeleton);
	}

	/** Computes the world transform using the parent bone's world transform and this applied local pose. Child bones are not
	 * updated.
	 * <p>
	 * See <a href="https://esotericsoftware.com/spine-runtime-skeletons#World-transforms">World transforms</a> in the Spine
	 * Runtimes Guide. */
	public void updateWorldTransform (Skeleton skeleton) {
		if (local == skeleton.update)
			updateLocalTransform(skeleton);
		else
			world = skeleton.update;

		if (bone.parent == null) { // Root bone.
			float sx = skeleton.scaleX, sy = skeleton.scaleY;
			float rx = (rotation + shearX) * degRad;
			float ry = (rotation + 90 + shearY) * degRad;
			a = cos(rx) * scaleX * sx;
			b = cos(ry) * scaleY * sx;
			c = sin(rx) * scaleX * sy;
			d = sin(ry) * scaleY * sy;
			worldX = x * sx + skeleton.x;
			worldY = y * sy + skeleton.y;
			return;
		}

		BonePose parent = bone.parent.appliedPose;
		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		worldX = pa * x + pb * y + parent.worldX;
		worldY = pc * x + pd * y + parent.worldY;

		switch (inherit) {
		case normal -> {
			float rx = (rotation + shearX) * degRad;
			float ry = (rotation + 90 + shearY) * degRad;
			float la = cos(rx) * scaleX;
			float lb = cos(ry) * scaleY;
			float lc = sin(rx) * scaleX;
			float ld = sin(ry) * scaleY;
			a = pa * la + pb * lc;
			b = pa * lb + pb * ld;
			c = pc * la + pd * lc;
			d = pc * lb + pd * ld;
		}
		case onlyTranslation -> {
			float sx = skeleton.scaleX, sy = skeleton.scaleY;
			float rx = (rotation + shearX) * degRad;
			float ry = (rotation + 90 + shearY) * degRad;
			a = cos(rx) * scaleX * sx;
			b = cos(ry) * scaleY * sx;
			c = sin(rx) * scaleX * sy;
			d = sin(ry) * scaleY * sy;
		}
		case noRotationOrReflection -> {
			float sx = skeleton.scaleX, sy = skeleton.scaleY, sxi = 1 / sx, syi = 1 / sy;
			pa *= sxi;
			pc *= syi;
			float s = pa * pa + pc * pc, r;
			if (s > epsilonSq) {
				s = Math.abs(pa * pd * syi - pb * sxi * pc) / s;
				pb = pc * s;
				pd = pa * s;
				r = rotation - atan2Deg(pc, pa);
			} else {
				pa = 0;
				pc = 0;
				r = rotation - 90 + atan2Deg(pd, pb);
			}
			float rx = (r + shearX) * degRad;
			float ry = (r + shearY + 90) * degRad;
			float la = cos(rx) * scaleX;
			float lb = cos(ry) * scaleY;
			float lc = sin(rx) * scaleX;
			float ld = sin(ry) * scaleY;
			a = (pa * la - pb * lc) * sx;
			b = (pa * lb - pb * ld) * sx;
			c = (pc * la + pd * lc) * sy;
			d = (pc * lb + pd * ld) * sy;
		}
		case noScale, noScaleOrReflection -> {
			float sx = skeleton.scaleX, sy = skeleton.scaleY, sxi = 1 / sx, syi = 1 / sy;
			float r = rotation * degRad, cos = cos(r), sin = sin(r);
			float za = (pa * cos + pb * sin) * sxi;
			float zc = (pc * cos + pd * sin) * syi;
			float s = 1 / (float)Math.sqrt(za * za + zc * zc);
			za *= s;
			zc *= s;
			float zb = -zc, zd = za;
			if (inherit == Inherit.noScale && pa * pd - pb * pc < 0 != (sx < 0 != sy < 0)) {
				zb = -zb;
				zd = -zd;
			}
			float rx = shearX * degRad;
			float ry = (90 + shearY) * degRad;
			float la = cos(rx) * scaleX;
			float lb = cos(ry) * scaleY;
			float lc = sin(rx) * scaleX;
			float ld = sin(ry) * scaleY;
			a = (za * la + zb * lc) * sx;
			b = (za * lb + zb * ld) * sx;
			c = (zc * la + zd * lc) * sy;
			d = (zc * lb + zd * ld) * sy;
		}
		}
	}

	/** Computes the local transform values from the world transform.
	 * <p>
	 * Some information is ambiguous in the world transform, such as -1,-1 scale versus 180 rotation. The local transform after
	 * calling this method is equivalent to the local transform used to compute the world transform, but may not be identical. */
	public void updateLocalTransform (Skeleton skeleton) {
		local = 0;
		world = skeleton.update;

		float sx = skeleton.scaleX, sy = skeleton.scaleY;
		if (bone.parent == null) {
			float sxi = 1 / sx, syi = 1 / sy;
			x = (worldX - skeleton.x) * sxi;
			y = (worldY - skeleton.y) * syi;
			set(a * sxi, b * sxi, c * syi, d * syi, 0);
			return;
		}

		BonePose parent = bone.parent.appliedPose;
		float pa = parent.a, pb = parent.b, pc = parent.c, pd = parent.d;
		float pad = pa * pd - pb * pc, pid = 1 / pad;
		float ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
		float dx = worldX - parent.worldX, dy = worldY - parent.worldY;
		x = dx * ia - dy * ib;
		y = dy * id - dx * ic;

		switch (inherit) {
		case normal -> set(ia * a - ib * c, ia * b - ib * d, id * c - ic * a, id * d - ic * b, 0);
		case onlyTranslation -> {
			float sxi = 1 / sx, syi = 1 / sy;
			set(a * sxi, b * sxi, c * syi, d * syi, 0);
		}
		case noRotationOrReflection -> {
			float sxi = 1 / sx, syi = 1 / sy;
			pa *= sxi;
			pc *= syi;
			float wa = a * sxi, wb = b * sxi, wc = c * syi, wd = d * syi;
			float s = 1 / (pa * pa + pc * pc), det = 1 / Math.abs(pad * sxi * syi);
			set((pa * wa + pc * wc) * s, (pa * wb + pc * wd) * s, (pa * wc - pc * wa) * det, (pa * wd - pc * wb) * det,
				atan2Deg(pc, pa));
		}
		case noScale, noScaleOrReflection -> {
			float sxi = 1 / sx, syi = 1 / sy;
			float wa = a * sxi, wb = b * sxi, wc = c * syi, wd = d * syi;
			float tx = pd * a - pb * c, ty = pa * c - pc * a;
			if (pad < 0) {
				tx = -tx;
				ty = -ty;
			}
			float r = atan2Deg(ty, tx);
			rotation = r;
			r *= degRad;
			float cos = cos(r), sin = sin(r);
			float za = (pa * cos + pb * sin) * sxi;
			float zc = (pc * cos + pd * sin) * syi;
			float s = 1 / (float)Math.sqrt(za * za + zc * zc);
			za *= s;
			zc *= s;
			float si = inherit == Inherit.noScale && pad < 0 != (sx < 0 != sy < 0) ? -1 : 1;
			set(za * wa + zc * wc, za * wb + zc * wd, (za * wc - zc * wa) * si, (za * wd - zc * wb) * si);
		}
		}
	}

	private void set (float ra, float rb, float rc, float rd) {
		float x = ra * ra + rc * rc, y = rb * rb + rd * rd;
		if (x > epsilonSq) {
			shearX = atan2Deg(rc, ra);
			scaleX = (float)Math.sqrt(x);
		} else {
			shearX = 0;
			scaleX = 0;
		}
		scaleY = (float)Math.sqrt(y);
		if (y > epsilonSq) {
			shearY = atan2Deg(rd, rb);
			if (ra * rd - rb * rc < 0) {
				scaleY = -scaleY;
				shearY += 90;
			} else
				shearY -= 90;
			if (shearY > 180)
				shearY -= 360;
			else if (shearY <= -180) //
				shearY += 360;
		} else
			shearY = 0;
	}

	private void set (float ra, float rb, float rc, float rd, float ro) {
		shearX = 0;
		float x = ra * ra + rc * rc, y = rb * rb + rd * rd;
		if (x > epsilonSq) {
			float r = atan2Deg(rc, ra);
			rotation = r + ro;
			scaleX = (float)Math.sqrt(x);
			scaleY = (float)Math.sqrt(y);
			if (y > epsilonSq) {
				shearY = atan2Deg(rd, rb);
				if (ra * rd - rb * rc < 0) {
					scaleY = -scaleY;
					shearY += 90 - r;
				} else
					shearY -= 90 + r;
				if (shearY > 180)
					shearY -= 360;
				else if (shearY <= -180) //
					shearY += 360;
			} else
				shearY = 0;
		} else {
			scaleX = 0;
			scaleY = (float)Math.sqrt(y);
			shearY = 0;
			rotation = y > epsilonSq ? atan2Deg(rd, rb) - 90 + ro : ro;
		}
	}

	/** If the world transform has been modified by constraints and the local transform no longer matches,
	 * {@link #updateLocalTransform(Skeleton)} is called. Call this after {@link Skeleton#updateWorldTransform(Physics)} before
	 * using the applied local transform. */
	public void validateLocalTransform (Skeleton skeleton) {
		if (local == skeleton.update) updateLocalTransform(skeleton);
	}

	void modifyLocal (Skeleton skeleton) {
		if (local == skeleton.update) updateLocalTransform(skeleton);
		world = 0;
		resetWorld(skeleton.update);
	}

	void modifyWorld (int update) {
		local = update;
		world = update;
		resetWorld(update);
	}

	private void resetWorld (int update) {
		Bone[] children = bone.children.items;
		for (int i = 0, n = bone.children.size; i < n; i++) {
			BonePose child = children[i].appliedPose;
			if (child.world == update) {
				child.world = 0;
				child.local = 0;
				child.resetWorld(update);
			}
		}
	}

	/** The world transform <code>[a b][c d]</code> x-axis x component. */
	public float getA () {
		return a;
	}

	public void setA (float a) {
		this.a = a;
	}

	/** The world transform <code>[a b][c d]</code> y-axis x component. */
	public float getB () {
		return b;
	}

	public void setB (float b) {
		this.b = b;
	}

	/** The world transform <code>[a b][c d]</code> x-axis y component. */
	public float getC () {
		return c;
	}

	public void setC (float c) {
		this.c = c;
	}

	/** The world transform <code>[a b][c d]</code> y-axis y component. */
	public float getD () {
		return d;
	}

	public void setD (float d) {
		this.d = d;
	}

	/** The world X position. */
	public float getWorldX () {
		return worldX;
	}

	public void setWorldX (float worldX) {
		this.worldX = worldX;
	}

	/** The world Y position. */
	public float getWorldY () {
		return worldY;
	}

	public void setWorldY (float worldY) {
		this.worldY = worldY;
	}

	/** The world rotation for the X axis, calculated using {@link #a} and {@link #c}. This is the direction the bone is
	 * pointing. */
	public float getWorldRotationX () {
		return atan2Deg(c, a);
	}

	/** The world rotation for the Y axis, calculated using {@link #b} and {@link #d}. */
	public float getWorldRotationY () {
		return atan2Deg(d, b);
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
		if (world == null) throw new IllegalArgumentException("world cannot be null.");
		float det = a * d - b * c;
		float x = world.x - worldX, y = world.y - worldY;
		world.x = (x * d - y * b) / det;
		world.y = (y * a - x * c) / det;
		return world;
	}

	/** Transforms a point from the bone's local coordinates to world coordinates. */
	public Vector2 localToWorld (Vector2 local) {
		if (local == null) throw new IllegalArgumentException("local cannot be null.");
		float x = local.x, y = local.y;
		local.x = x * a + y * b + worldX;
		local.y = x * c + y * d + worldY;
		return local;
	}

	/** Transforms a point from world coordinates to the parent bone's local coordinates. */
	public Vector2 worldToParent (Vector2 world) {
		if (world == null) throw new IllegalArgumentException("world cannot be null.");
		return bone.parent == null ? world : bone.parent.appliedPose.worldToLocal(world);
	}

	/** Transforms a point from the parent bone's coordinates to world coordinates. */
	public Vector2 parentToWorld (Vector2 world) {
		if (world == null) throw new IllegalArgumentException("world cannot be null.");
		return bone.parent == null ? world : bone.parent.appliedPose.localToWorld(world);
	}

	/** Transforms a world rotation to a local rotation. */
	public float worldToLocalRotation (float worldRotation) {
		worldRotation *= degRad;
		float sin = sin(worldRotation), cos = cos(worldRotation);
		return atan2Deg(a * sin - c * cos, d * cos - b * sin) + rotation - shearX;
	}

	/** Transforms a local rotation to a world rotation. */
	public float localToWorldRotation (float localRotation) {
		localRotation = (localRotation - rotation - shearX) * degRad;
		float sin = sin(localRotation), cos = cos(localRotation);
		return atan2Deg(cos * c + sin * d, cos * a + sin * b);
	}

	/** Rotates the world transform the specified amount. */
	public void rotateWorld (float degrees) {
		degrees *= degRad;
		float sin = sin(degrees), cos = cos(degrees);
		float ra = a, rb = b;
		a = cos * ra - sin * c;
		b = cos * rb - sin * d;
		c = sin * ra + cos * c;
		d = sin * rb + cos * d;
	}

	public String toString () {
		return bone.data.name;
	}
}
