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

import com.badlogic.gdx.utils.Array;

public class IkConstraint implements Updatable {
	final IkConstraintData data;
	final Array<Bone> bones;
	Bone target;
	float mix = 1;
	int bendDirection;

	public IkConstraint (IkConstraintData data, Skeleton skeleton) {
		this.data = data;
		mix = data.mix;
		bendDirection = data.bendDirection;

		bones = new Array(data.bones.size);
		if (skeleton != null) {
			for (BoneData boneData : data.bones)
				bones.add(skeleton.findBone(boneData.name));
			target = skeleton.findBone(data.target.name);
		}
	}

	/** Copy constructor. */
	public IkConstraint (IkConstraint ikConstraint, Skeleton skeleton) {
		data = ikConstraint.data;
		bones = new Array(ikConstraint.bones.size);
		for (Bone bone : ikConstraint.bones)
			bones.add(skeleton.bones.get(bone.skeleton.bones.indexOf(bone, true)));
		target = skeleton.bones.get(ikConstraint.target.skeleton.bones.indexOf(ikConstraint.target, true));
		mix = ikConstraint.mix;
		bendDirection = ikConstraint.bendDirection;
	}

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

	public Array<Bone> getBones () {
		return bones;
	}

	public Bone getTarget () {
		return target;
	}

	public void setTarget (Bone target) {
		this.target = target;
	}

	public float getMix () {
		return mix;
	}

	public void setMix (float mix) {
		this.mix = mix;
	}

	public int getBendDirection () {
		return bendDirection;
	}

	public void setBendDirection (int bendDirection) {
		this.bendDirection = bendDirection;
	}

	public IkConstraintData getData () {
		return data;
	}

	public String toString () {
		return data.name + " CONSTRAINT";
	}

	/** Adjusts the bone rotation so the tip is as close to the target position as possible. The target is specified in the world
	 * coordinate system. */
	static public void apply (Bone bone, float targetX, float targetY, float alpha) {
		float parentRotation = bone.parent == null ? 0 : bone.parent.getWorldRotationX();
		float rotation = bone.rotation;
		float rotationIK = atan2(targetY - bone.worldY, targetX - bone.worldX) * radDeg - parentRotation;
		if (bone.worldSignX != bone.worldSignY) rotationIK = 360 - rotationIK;
		if (rotationIK > 180)
			rotationIK -= 360;
		else if (rotationIK < -180) rotationIK += 360;
		bone.updateWorldTransform(bone.x, bone.y, rotation + (rotationIK - rotation) * alpha, bone.scaleX, bone.scaleY);
	}

	/** Adjusts the parent and child bone rotations so the tip of the child is as close to the target position as possible. The
	 * target is specified in the world coordinate system.
	 * @param child Any descendant bone of the parent. */
	static public void apply (Bone parent, Bone child, float targetX, float targetY, int bendDir, float alpha) {
		if (alpha == 0) return;
		float px = parent.x, py = parent.y, psx = parent.scaleX, psy = parent.scaleY, csx = child.scaleX, cy = child.y;
		int offset1, offset2, sign2;
		if (psx < 0) {
			psx = -psx;
			offset1 = 180;
			sign2 = -1;
		} else {
			offset1 = 0;
			sign2 = 1;
		}
		if (psy < 0) {
			psy = -psy;
			sign2 = -sign2;
		}
		if (csx < 0) {
			csx = -csx;
			offset2 = 180;
		} else
			offset2 = 0;
		Bone pp = parent.parent;
		float tx, ty, dx, dy;
		if (pp == null) {
			tx = targetX - px;
			ty = targetY - py;
			dx = child.worldX - px;
			dy = child.worldY - py;
		} else {
			float a = pp.a, b = pp.b, c = pp.c, d = pp.d, invDet = 1 / (a * d - b * c);
			float wx = pp.worldX, wy = pp.worldY, x = targetX - wx, y = targetY - wy;
			tx = (x * d - y * b) * invDet - px;
			ty = (y * a - x * c) * invDet - py;
			x = child.worldX - wx;
			y = child.worldY - wy;
			dx = (x * d - y * b) * invDet - px;
			dy = (y * a - x * c) * invDet - py;
		}
		float l1 = (float)Math.sqrt(dx * dx + dy * dy), l2 = child.data.length * csx, a1, a2;
		outer:
		if (Math.abs(psx - psy) <= 0.0001f) {
			l2 *= psx;
			float cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
			if (cos < -1)
				cos = -1;
			else if (cos > 1) cos = 1;
			a2 = (float)Math.acos(cos) * bendDir;
			float a = l1 + l2 * cos, o = l2 * sin(a2);
			a1 = atan2(ty * a - tx * o, tx * a + ty * o);
		} else {
			cy = 0;
			float a = psx * l2, b = psy * l2, ta = atan2(ty, tx);
			float aa = a * a, bb = b * b, ll = l1 * l1, dd = tx * tx + ty * ty;
			float c0 = bb * ll + aa * dd - aa * bb, c1 = -2 * bb * l1, c2 = bb - aa;
			float d = c1 * c1 - 4 * c2 * c0;
			if (d >= 0) {
				float q = (float)Math.sqrt(d);
				if (c1 < 0) q = -q;
				q = -(c1 + q) / 2;
				float r0 = q / c2, r1 = c0 / q;
				float r = Math.abs(r0) < Math.abs(r1) ? r0 : r1;
				if (r * r <= dd) {
					float y = (float)Math.sqrt(dd - r * r) * bendDir;
					a1 = ta - atan2(y, r);
					a2 = atan2(y / psy, (r - l1) / psx);
					break outer;
				}
			}
			float minAngle = 0, minDist = Float.MAX_VALUE, minX = 0, minY = 0;
			float maxAngle = 0, maxDist = 0, maxX = 0, maxY = 0;
			float x = l1 + a, dist = x * x;
			if (dist > maxDist) {
				maxAngle = 0;
				maxDist = dist;
				maxX = x;
			}
			x = l1 - a;
			dist = x * x;
			if (dist < minDist) {
				minAngle = PI;
				minDist = dist;
				minX = x;
			}
			float angle = (float)Math.acos(-a * l1 / (aa - bb));
			x = a * cos(angle) + l1;
			float y = b * sin(angle);
			dist = x * x + y * y;
			if (dist < minDist) {
				minAngle = angle;
				minDist = dist;
				minX = x;
				minY = y;
			}
			if (dist > maxDist) {
				maxAngle = angle;
				maxDist = dist;
				maxX = x;
				maxY = y;
			}
			if (dd <= (minDist + maxDist) / 2) {
				a1 = ta - atan2(minY * bendDir, minX);
				a2 = minAngle * bendDir;
			} else {
				a1 = ta - atan2(maxY * bendDir, maxX);
				a2 = maxAngle * bendDir;
			}
		}
		float offset = atan2(cy, child.x) * sign2;
		a1 = (a1 - offset) * radDeg + offset1;
		a2 = (a2 + offset) * radDeg * sign2 + offset2;
		if (a1 > 180)
			a1 -= 360;
		else if (a1 < -180) a1 += 360;
		if (a2 > 180)
			a2 -= 360;
		else if (a2 < -180) a2 += 360;
		float rotation = parent.rotation;
		parent.updateWorldTransform(parent.x, parent.y, rotation + (a1 - rotation) * alpha, parent.scaleX, parent.scaleY);
		rotation = child.rotation;
		child.updateWorldTransform(child.x, cy, rotation + (a2 - rotation) * alpha, child.scaleX, child.scaleY);
	}
}
