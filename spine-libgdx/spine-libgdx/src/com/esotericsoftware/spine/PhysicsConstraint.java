/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

import static com.esotericsoftware.spine.utils.SpineUtils.*;

/** Applies physics to a bone.
 * <p>
 * See <a href="https://esotericsoftware.com/spine-physics-constraints">Physics constraints</a> in the Spine User Guide. */
public class PhysicsConstraint extends Constraint<PhysicsConstraint, PhysicsConstraintData, PhysicsConstraintPose> {
	BonePose bone;

	boolean reset = true;
	float ux, uy, cx, cy, tx, ty;
	float xOffset, xLag, xVelocity;
	float yOffset, yLag, yVelocity;
	float rotateOffset, rotateLag, rotateVelocity;
	float scaleOffset, scaleLag, scaleVelocity;
	float remaining, lastTime;

	public PhysicsConstraint (PhysicsConstraintData data, Skeleton skeleton) {
		super(data, new PhysicsConstraintPose(), new PhysicsConstraintPose());
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");

		bone = skeleton.bones.items[data.bone.index].constrainedPose;
	}

	public PhysicsConstraint copy (Skeleton skeleton) {
		var copy = new PhysicsConstraint(data, skeleton);
		copy.pose.set(pose);
		return copy;
	}

	/** Resets all physics state that was the result of previous movement. Use this after moving a bone to prevent physics from
	 * reacting to the movement. */
	public void reset (Skeleton skeleton) {
		remaining = 0;
		lastTime = skeleton.time;
		reset = true;
		xOffset = 0;
		xLag = 0;
		xVelocity = 0;
		yOffset = 0;
		yLag = 0;
		yVelocity = 0;
		rotateOffset = 0;
		rotateLag = 0;
		rotateVelocity = 0;
		scaleOffset = 0;
		scaleLag = 0;
		scaleVelocity = 0;
	}

	/** Translates the physics constraint so the next {@link #update(Skeleton, Physics)} forces are applied as if the bone moved an
	 * additional amount in world space. */
	public void translate (float x, float y) {
		ux -= x;
		uy -= y;
		cx -= x;
		cy -= y;
	}

	/** Rotates the physics constraint so the next {@link #update(Skeleton, Physics)} forces are applied as if the bone rotated
	 * around the specified point in world space. */
	public void rotate (float x, float y, float degrees) {
		float r = degrees * degRad, cos = cos(r), sin = sin(r);
		float dx = cx - x, dy = cy - y;
		translate(dx * cos - dy * sin - dx, dx * sin + dy * cos - dy);
	}

	/** Applies the constraint to the constrained bones. */
	public void update (Skeleton skeleton, Physics physics) {
		PhysicsConstraintPose p = appliedPose;
		float mix = p.mix;
		if (mix == 0) return;

		boolean x = data.x > 0, y = data.y > 0, rotateOrShearX = data.rotate > 0 || data.shearX > 0, scaleX = data.scaleX > 0;
		BonePose bone = this.bone;
		float l = bone.bone.data.length, t = data.step, z = 0;

		switch (physics) {
		case none:
			return;
		case reset:
			reset(skeleton);
			// Fall through.
		case update:
			float delta = Math.max(skeleton.time - lastTime, 0), aa = remaining;
			remaining += delta;
			lastTime = skeleton.time;

			float bx = bone.worldX, by = bone.worldY;
			if (reset) {
				reset = false;
				ux = bx;
				uy = by;
			} else {
				float a = remaining, i = p.inertia, f = skeleton.data.referenceScale, d = -1, m = 0, e = 0, qx = data.limit * delta,
					qy = qx * Math.abs(skeleton.scaleY);
				qx *= Math.abs(skeleton.scaleX);
				if (x || y) {
					if (x) {
						float u = (ux - bx) * i;
						xOffset += u > qx ? qx : u < -qx ? -qx : u;
						ux = bx;
					}
					if (y) {
						float u = (uy - by) * i;
						yOffset += u > qy ? qy : u < -qy ? -qy : u;
						uy = by;
					}
					if (a >= t) {
						float xs = xOffset, ys = yOffset;
						d = (float)Math.pow(p.damping, 60 * t);
						m = t * p.massInverse;
						e = p.strength;
						float w = f * p.wind, g = f * p.gravity;
						float ax = (w * skeleton.windX + g * skeleton.gravityX) * skeleton.scaleX;
						float ay = (w * skeleton.windY + g * skeleton.gravityY) * skeleton.scaleY;
						do {
							if (x) {
								xVelocity += (ax - xOffset * e) * m;
								xOffset += xVelocity * t;
								xVelocity *= d;
							}
							if (y) {
								yVelocity -= (ay + yOffset * e) * m;
								yOffset += yVelocity * t;
								yVelocity *= d;
							}
							a -= t;
						} while (a >= t);
						xLag = xOffset - xs;
						yLag = yOffset - ys;
					}
					z = Math.max(0, 1 - a / t);
					if (x) bone.worldX += (xOffset - xLag * z) * mix * data.x;
					if (y) bone.worldY += (yOffset - yLag * z) * mix * data.y;
				}
				if (rotateOrShearX || scaleX) {
					float ca = atan2(bone.c, bone.a), c, s, mr = 0, dx = cx - bone.worldX, dy = cy - bone.worldY;
					if (dx > qx)
						dx = qx;
					else if (dx < -qx) //
						dx = -qx;
					if (dy > qy)
						dy = qy;
					else if (dy < -qy) //
						dy = -qy;
					if (rotateOrShearX) {
						mr = (data.rotate + data.shearX) * mix;
						z = rotateLag * Math.max(0, 1 - aa / t);
						float r = atan2(dy + ty, dx + tx) - ca - (rotateOffset - z) * mr;
						rotateOffset += (r - (float)Math.ceil(r * invPI2 - 0.5f) * PI2) * i;
						r = (rotateOffset - z) * mr + ca;
						c = cos(r);
						s = sin(r);
						if (scaleX) {
							r = l * bone.getWorldScaleX();
							if (r > 0) scaleOffset += (dx * c + dy * s) * i / r;
						}
					} else {
						c = cos(ca);
						s = sin(ca);
						float r = l * bone.getWorldScaleX() - scaleLag * Math.max(0, 1 - aa / t);
						if (r > 0) scaleOffset += (dx * c + dy * s) * i / r;
					}
					a = remaining;
					if (a >= t) {
						if (d == -1) {
							d = (float)Math.pow(p.damping, 60 * t);
							m = t * p.massInverse;
							e = p.strength;
						}
						float ax = p.wind * skeleton.windX + p.gravity * skeleton.gravityX;
						float ay = p.wind * skeleton.windY + p.gravity * skeleton.gravityY;
						float rs = rotateOffset, ss = scaleOffset, h = l / f;
						while (true) {
							a -= t;
							if (scaleX) {
								scaleVelocity += (ax * c - ay * s - scaleOffset * e) * m;
								scaleOffset += scaleVelocity * t;
								scaleVelocity *= d;
							}
							if (rotateOrShearX) {
								rotateVelocity -= ((ax * s + ay * c) * h + rotateOffset * e) * m;
								rotateOffset += rotateVelocity * t;
								rotateVelocity *= d;
								if (a < t) break;
								float r = rotateOffset * mr + ca;
								c = cos(r);
								s = sin(r);
							} else if (a < t) //
								break;
						}
						rotateLag = rotateOffset - rs;
						scaleLag = scaleOffset - ss;
					}
					z = Math.max(0, 1 - a / t);
				}
				remaining = a;
			}
			cx = bone.worldX;
			cy = bone.worldY;
			break;
		case pose:
			z = Math.max(0, 1 - remaining / t);
			if (x) bone.worldX += (xOffset - xLag * z) * mix * data.x;
			if (y) bone.worldY += (yOffset - yLag * z) * mix * data.y;
		}

		if (rotateOrShearX) {
			float o = (rotateOffset - rotateLag * z) * mix, s, c, a;
			if (data.shearX > 0) {
				float r = 0;
				if (data.rotate > 0) {
					r = o * data.rotate;
					s = sin(r);
					c = cos(r);
					a = bone.b;
					bone.b = c * a - s * bone.d;
					bone.d = s * a + c * bone.d;
				}
				r += o * data.shearX;
				s = sin(r);
				c = cos(r);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
			} else {
				o *= data.rotate;
				s = sin(o);
				c = cos(o);
				a = bone.a;
				bone.a = c * a - s * bone.c;
				bone.c = s * a + c * bone.c;
				a = bone.b;
				bone.b = c * a - s * bone.d;
				bone.d = s * a + c * bone.d;
			}
		}
		if (scaleX) {
			float s = 1 + (scaleOffset - scaleLag * z) * mix * data.scaleX;
			bone.a *= s;
			bone.c *= s;
			switch (data.scaleYMode) {
			case uniform -> {
				bone.b *= s;
				bone.d *= s;
			}
			case volume -> {
				s = Math.abs(s);
				s = s >= 0.7f ? 1 / s : 4 - 3.67347f * s;
				bone.b *= s;
				bone.d *= s;
			}
			}
		}
		if (physics != Physics.pose) {
			tx = l * bone.a;
			ty = l * bone.c;
		}
		bone.modifyWorld(skeleton.update);
	}

	void sort (Skeleton skeleton) {
		Bone bone = this.bone.bone;
		skeleton.sortBone(bone);
		skeleton.updateCache.add(this);
		skeleton.sortReset(bone.children);
		skeleton.constrained(bone);
	}

	boolean isSourceActive () {
		return bone.bone.active;
	}

	/** The bone constrained by this physics constraint. */
	public BonePose getBone () {
		return bone;
	}

	public void setBone (BonePose bone) {
		this.bone = bone;
	}
}
