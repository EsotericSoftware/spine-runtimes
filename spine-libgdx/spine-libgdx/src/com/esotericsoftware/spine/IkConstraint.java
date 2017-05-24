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

import com.badlogic.gdx.utils.Array;

/** Stores the current pose for an IK constraint. An IK constraint adjusts the rotation of 1 or 2 constrained bones so the tip of
 * the last bone is as close to the target bone as possible.
 * <p>
 * See <a href="http://esotericsoftware.com/spine-ik-constraints">IK constraints</a> in the Spine User Guide. */
public class IkConstraint implements Constraint {
	final IkConstraintData data;
	final Array<Bone> bones;
	Bone target;
	float mix = 1;
	int bendDirection;

	public IkConstraint (IkConstraintData data, Skeleton skeleton) {
		if (data == null) throw new IllegalArgumentException("data cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		this.data = data;
		mix = data.mix;
		bendDirection = data.bendDirection;

		bones = new Array(data.bones.size);
		for (BoneData boneData : data.bones)
			bones.add(skeleton.findBone(boneData.name));
		target = skeleton.findBone(data.target.name);
	}

	/** Copy constructor. */
	public IkConstraint (IkConstraint constraint, Skeleton skeleton) {
		if (constraint == null) throw new IllegalArgumentException("constraint cannot be null.");
		if (skeleton == null) throw new IllegalArgumentException("skeleton cannot be null.");
		data = constraint.data;
		bones = new Array(constraint.bones.size);
		for (Bone bone : constraint.bones)
			bones.add(skeleton.bones.get(bone.data.index));
		target = skeleton.bones.get(constraint.target.data.index);
		mix = constraint.mix;
		bendDirection = constraint.bendDirection;
	}

	/** Applies the constraint to the constrained bones. */
	public void apply () {
		update();
	}

	public void update () {
		Bone target = this.target;
		Array<Bone> bones = this.bones;
		switch (bones.size) {
		case 1:
			apply(bones.first(), target.worldX, target.worldY, mix);
			break;
		case 2:
			apply(bones.first(), bones.get(1), target.worldX, target.worldY, bendDirection, mix);
			break;
		}
	}

	public int getOrder () {
		return data.order;
	}

	/** The bones that will be modified by this IK constraint. */
	public Array<Bone> getBones () {
		return bones;
	}

	/** The bone that is the IK target. */
	public Bone getTarget () {
		return target;
	}

	public void setTarget (Bone target) {
		this.target = target;
	}

	/** A percentage (0-1) that controls the mix between the constrained and unconstrained rotations. */
	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	/** Controls the bend direction of the IK bones, either 1 or -1. */
	public int getBendDirection () {
		return bendDirection;
	}

	public void setBendDirection (int bendDirection) {
		this.bendDirection = bendDirection;
	}

	/** The IK constraint's setup pose data. */
	public IkConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name;
	}

	/** Applies 1 bone IK. The target is specified in the world coordinate system. */
	static public void apply (Bone bone, float targetX, float targetY, float alpha) {
		if (!bone.appliedValid) bone.updateAppliedTransform();
		Bone p = bone.parent;
		float id = 1 / (p.a * p.d - p.b * p.c);
		float x = targetX - p.worldX, y = targetY - p.worldY;
		float tx = (x * p.d - y * p.b) * id - bone.ax, ty = (y * p.a - x * p.c) * id - bone.ay;
		float rotationIK = atan2(ty, tx) * radDeg - bone.ashearX - bone.arotation;
		if (bone.ascaleX < 0) rotationIK += 180;
		if (rotationIK > 180)
			rotationIK -= 360;
		else if (rotationIK < -180) rotationIK += 360;
		bone.updateWorldTransform(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, bone.ascaleX, bone.ascaleY, bone.ashearX,
			bone.ashearY);
	}

	/** Applies 2 bone IK. The target is specified in the world coordinate system.
	 * @param child A direct descendant of the parent bone. */
	static public void apply (Bone parent, Bone child, float targetX, float targetY, int bendDir, float alpha) {
		if (alpha == 0) {
			child.updateWorldTransform();
			return;
		}
		if (!parent.appliedValid) parent.updateAppliedTransform();
		if (!child.appliedValid) child.updateAppliedTransform();
		float px = parent.ax, py = parent.ay, psx = parent.ascaleX, psy = parent.ascaleY, csx = child.ascaleX;
		int os1, os2, s2;
		if (psx < 0) {
			psx = -psx;
			os1 = 180;
			s2 = -1;
		} else {
			os1 = 0;
			s2 = 1;
		}
		if (psy < 0) {
			psy = -psy;
			s2 = -s2;
		}
		if (csx < 0) {
			csx = -csx;
			os2 = 180;
		} else
			os2 = 0;
		float cx = child.ax, cy, cwx, cwy, a = parent.a, b = parent.b, c = parent.c, d = parent.d;
		boolean u = Math.abs(psx - psy) <= 0.0001f;
		if (!u) {
			cy = 0;
			cwx = a * cx + parent.worldX;
			cwy = c * cx + parent.worldY;
		} else {
			cy = child.ay;
			cwx = a * cx + b * cy + parent.worldX;
			cwy = c * cx + d * cy + parent.worldY;
		}
		Bone pp = parent.parent;
		a = pp.a;
		b = pp.b;
		c = pp.c;
		d = pp.d;
		float id = 1 / (a * d - b * c), x = targetX - pp.worldX, y = targetY - pp.worldY;
		float tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
		x = cwx - pp.worldX;
		y = cwy - pp.worldY;
		float dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
		float l1 = (float)Math.sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1, a2;
		outer:
		if (u) {
			l2 *= psx;
			float cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
			if (cos < -1)
				cos = -1;
			else if (cos > 1) cos = 1;
			a2 = (float)Math.acos(cos) * bendDir;
			a = l1 + l2 * cos;
			b = l2 * sin(a2);
			a1 = atan2(ty * a - tx * b, tx * a + ty * b);
		} else {
			a = psx * l2;
			b = psy * l2;
			float aa = a * a, bb = b * b, dd = tx * tx + ty * ty, ta = atan2(ty, tx);
			c = bb * l1 * l1 + aa * dd - aa * bb;
			float c1 = -2 * bb * l1, c2 = bb - aa;
			d = c1 * c1 - 4 * c2 * c;
			if (d >= 0) {
				float q = (float)Math.sqrt(d);
				if (c1 < 0) q = -q;
				q = -(c1 + q) / 2;
				float r0 = q / c2, r1 = c / q;
				float r = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
				if (r * r <= dd) {
					y = (float)Math.sqrt(dd - r * r) * bendDir;
					a1 = ta - atan2(y, r);
					a2 = atan2(y / psy, (r - l1) / psx);
					break outer;
				}
			}
			float minAngle = PI, minX = l1 - a, minDist = minX * minX, minY = 0;
			float maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
			c = -a * l1 / (aa - bb);
			if (c >= -1 && c <= 1) {
				c = (float)Math.acos(c);
				x = a * cos(c) + l1;
				y = b * sin(c);
				d = x * x + y * y;
				if (d < minDist) {
					minAngle = c;
					minDist = d;
					minX = x;
					minY = y;
				}
				if (d > maxDist) {
					maxAngle = c;
					maxDist = d;
					maxX = x;
					maxY = y;
				}
			}
			if (dd <= (minDist + maxDist) / 2) {
				a1 = ta - atan2(minY * bendDir, minX);
				a2 = minAngle * bendDir;
			} else {
				a1 = ta - atan2(maxY * bendDir, maxX);
				a2 = maxAngle * bendDir;
			}
		}
		float os = atan2(cy, cx) * s2;
		float rotation = parent.arotation;
		a1 = (a1 - os) * radDeg + os1 - rotation;
		if (a1 > 180)
			a1 -= 360;
		else if (a1 < -180) a1 += 360;
		parent.updateWorldTransform(px, py, rotation + a1 * alpha, parent.ascaleX, parent.ascaleY, 0, 0);
		rotation = child.arotation;
		a2 = ((a2 + os) * radDeg - child.ashearX) * s2 + os2 - rotation;
		if (a2 > 180)
			a2 -= 360;
		else if (a2 < -180) a2 += 360;
		child.updateWorldTransform(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
	}
}
