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

#include <spine/IkConstraint.h>

#include <spine/Bone.h>
#include <spine/BonePose.h>
#include <spine/ConstraintData.h>
#include <spine/IkConstraintData.h>
#include <spine/Skeleton.h>

#include <spine/BoneData.h>

using namespace spine;

RTTI_IMPL(IkConstraint, Constraint)

IkConstraint::IkConstraint(IkConstraintData &data, Skeleton &skeleton) : IkConstraintBase(data), _target(skeleton._bones[data._target->getIndex()]) {

	_bones.ensureCapacity(data._bones.size());
	for (size_t i = 0; i < data._bones.size(); i++) {
		BoneData *boneData = data._bones[i];
		_bones.add(&skeleton._bones[boneData->getIndex()]->_constrainedPose);
	}
}

IkConstraint &IkConstraint::copy(Skeleton &skeleton) {
	IkConstraint *copy = new (__FILE__, __LINE__) IkConstraint(_data, skeleton);
	copy->_pose.set(_pose);
	return *copy;
}

void IkConstraint::update(Skeleton &skeleton, Physics physics) {
	IkConstraintPose &p = *_appliedPose;
	if (p._mix == 0) return;
	BonePose &target = *_target->_appliedPose;
	switch (_bones.size()) {
		case 1: {
			apply(skeleton, *_bones[0], target._worldX, target._worldY, p._compress, p._stretch, _data._scaleYMode, p._mix);
		} break;
		case 2: {
			apply(skeleton, *_bones[0], *_bones[1], target._worldX, target._worldY, p._bendDirection, p._stretch, _data._scaleYMode, p._softness,
				  p._mix);
		} break;
	}
}

void IkConstraint::sort(Skeleton &skeleton) {
	skeleton.sortBone(_target);
	Bone *parent = _bones[0]->_bone;
	skeleton.sortBone(parent);
	skeleton._updateCache.add(this);
	parent->_sorted = false;
	skeleton.sortReset(parent->_children);
	skeleton.constrained(*parent);
	if (_bones.size() > 1) skeleton.constrained(*_bones[1]->_bone);
}

Array<BonePose *> &IkConstraint::getBones() {
	return _bones;
}

Bone &IkConstraint::getTarget() {
	return *_target;
}

void IkConstraint::setTarget(Bone &target) {
	_target = &target;
}

bool IkConstraint::isSourceActive() {
	return _target->_active;
}

void IkConstraint::apply(Skeleton &skeleton, BonePose &bone, float targetX, float targetY, bool compress, bool stretch, ScaleYMode scaleYMode,
						 float mix) {
	bone.modifyLocal(skeleton);
	BonePose &p = *bone._bone->_parent->_appliedPose;
	float pa = p._a, pb = p._b, pc = p._c, pd = p._d;
	float rotationIK = -bone._shearX - bone._rotation, tx, ty;
	switch (bone._inherit) {
		case Inherit_OnlyTranslation:
			tx = (targetX - bone._worldX) * MathUtil::sign(skeleton._scaleX);
			ty = (targetY - bone._worldY) * MathUtil::sign(skeleton._scaleY);
			break;
		case Inherit_NoRotationOrReflection: {
			float s = MathUtil::abs(pa * pd - pb * pc) / MathUtil::max(MathUtil::Epsilon, pa * pa + pc * pc);
			float sa = pa / skeleton._scaleX;
			float sc = pc / skeleton._scaleY;
			pb = -sc * s * skeleton._scaleX;
			pd = sa * s * skeleton._scaleY;
			rotationIK += MathUtil::atan2Deg(sc, sa);
			// Fall through.
		}
		default:
			float x = targetX - p._worldX, y = targetY - p._worldY;
			float d = pa * pd - pb * pc;
			if (MathUtil::abs(d) <= MathUtil::Epsilon) {
				tx = 0;
				ty = 0;
			} else {
				tx = (x * pd - y * pb) / d - bone._x;
				ty = (y * pa - x * pc) / d - bone._y;
			}
	}
	rotationIK += MathUtil::atan2Deg(ty, tx);
	if (bone._scaleX < 0) rotationIK += 180;
	if (rotationIK > 180)
		rotationIK -= 360;
	else if (rotationIK <= -180)//
		rotationIK += 360;
	bone._rotation += rotationIK * mix;
	if (compress || stretch) {
		switch (bone._inherit) {
			case Inherit_NoScale:
			case Inherit_NoScaleOrReflection:
				tx = targetX - bone._worldX;
				ty = targetY - bone._worldY;
				break;
			default:
				break;
		}
		float b = bone._bone->_data.getLength() * bone._scaleX;
		if (b > MathUtil::Epsilon) {
			float dd = tx * tx + ty * ty;
			if ((compress && dd < b * b) || (stretch && dd > b * b)) {
				float s = (MathUtil::sqrt(dd) / b - 1) * mix + 1;
				bone._scaleX *= s;
				switch (scaleYMode) {
					case ScaleYMode_Uniform:
						bone._scaleY *= s;
						break;
					case ScaleYMode_Volume:
						bone._scaleY /= s < 0.7f ? 0.25f + 0.642857f * s : s;
						break;
					default:
						break;
				}
			}
		}
	}
}

void IkConstraint::apply(Skeleton &skeleton, BonePose &parent, BonePose &child, float targetX, float targetY, int bendDir, bool stretch,
						 ScaleYMode scaleYMode, float softness, float mix) {
	if (parent._inherit != Inherit_Normal || child._inherit != Inherit_Normal) return;
	parent.modifyLocal(skeleton);
	child.modifyLocal(skeleton);
	float px = parent._x, py = parent._y, psx = parent._scaleX, psy = parent._scaleY, csx = child._scaleX;
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
	float cwx, cwy, a = parent._a, b = parent._b, c = parent._c, d = parent._d;
	bool u = MathUtil::abs(psx - psy) <= MathUtil::Epsilon;
	if (!u || stretch) {
		child._y = 0;
		cwx = a * child._x + parent._worldX;
		cwy = c * child._x + parent._worldY;
	} else {
		cwx = a * child._x + b * child._y + parent._worldX;
		cwy = c * child._x + d * child._y + parent._worldY;
	}
	BonePose &pp = *parent._bone->_parent->_appliedPose;
	a = pp._a;
	b = pp._b;
	c = pp._c;
	d = pp._d;
	float id = a * d - b * c, x = cwx - pp._worldX, y = cwy - pp._worldY;
	id = MathUtil::abs(id) <= MathUtil::Epsilon ? 0 : 1 / id;
	float dx = (x * d - y * b) * id - px, dy = (y * a - x * c) * id - py;
	float l1 = MathUtil::sqrt(dx * dx + dy * dy), l2 = child._bone->_data.getLength() * csx, a1, a2;
	if (l1 < MathUtil::Epsilon) {
		apply(skeleton, parent, targetX, targetY, false, stretch, ScaleYMode_None, mix);
		child._rotation = 0;
		return;
	}
	x = targetX - pp._worldX;
	y = targetY - pp._worldY;
	float tx = (x * d - y * b) * id - px, ty = (y * a - x * c) * id - py;
	float dd = tx * tx + ty * ty;
	if (softness != 0) {
		softness *= psx * (csx + 1) * 0.5f;
		float td = MathUtil::sqrt(dd), sd = td - l1 - l2 * psx + softness;
		if (sd > 0) {
			float p = MathUtil::min(1.0f, sd / (softness * 2)) - 1;
			p = (sd - softness * (1 - p * p)) / td;
			tx -= p * tx;
			ty -= p * ty;
			dd = tx * tx + ty * ty;
		}
	}

	if (u) {
		l2 *= psx;
		float cos = (dd - l1 * l1 - l2 * l2) / (2 * l1 * l2);
		if (cos < -1) {
			cos = -1;
			a2 = MathUtil::Pi * bendDir;
		} else if (cos > 1) {
			cos = 1;
			a2 = 0;
			if (stretch) {
				a = (MathUtil::sqrt(dd) / (l1 + l2) - 1) * mix + 1;
				parent._scaleX *= a;
				switch (scaleYMode) {
					case ScaleYMode_Uniform:
						parent._scaleY *= a;
						break;
					case ScaleYMode_Volume:
						parent._scaleY /= a < 0.7f ? 0.25f + 0.642857f * a : a;
						break;
					default:
						break;
				}
			}
		} else
			a2 = MathUtil::acos(cos) * bendDir;
		a = l1 + l2 * cos;
		b = l2 * MathUtil::sin(a2);
		a1 = MathUtil::atan2(ty * a - tx * b, tx * a + ty * b);
	} else {
		a = psx * l2;
		b = psy * l2;
		float aa = a * a, bb = b * b, ta = MathUtil::atan2(ty, tx);
		c = bb * l1 * l1 + aa * dd - aa * bb;
		float c1 = -2 * bb * l1, c2 = bb - aa;
		d = c1 * c1 - 4 * c2 * c;
		if (d >= 0) {
			float q = MathUtil::sqrt(d);
			if (c1 < 0) q = -q;
			q = -(c1 + q) * 0.5f;
			float r0 = q / c2, r1 = c / q;
			float r = MathUtil::abs(r0) < MathUtil::abs(r1) ? r0 : r1;
			r0 = dd - r * r;
			if (r0 >= 0) {
				y = MathUtil::sqrt(r0) * bendDir;
				a1 = ta - MathUtil::atan2(y, r);
				a2 = MathUtil::atan2(y / psy, (r - l1) / psx);
				goto outer_break;
			}
		}
		float minAngle = MathUtil::Pi, minX = l1 - a, minDist = minX * minX, minY = 0;
		float maxAngle = 0, maxX = l1 + a, maxDist = maxX * maxX, maxY = 0;
		c = -a * l1 / (aa - bb);
		if (c >= -1 && c <= 1) {
			c = MathUtil::acos(c);
			x = a * MathUtil::cos(c) + l1;
			y = b * MathUtil::sin(c);
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
		if (dd <= (minDist + maxDist) * 0.5f) {
			a1 = ta - MathUtil::atan2(minY * bendDir, minX);
			a2 = minAngle * bendDir;
		} else {
			a1 = ta - MathUtil::atan2(maxY * bendDir, maxX);
			a2 = maxAngle * bendDir;
		}
	}
outer_break:
	float os = MathUtil::atan2(child._y, child._x) * s2;
	a1 = (a1 - os) * MathUtil::Rad_Deg + os1 - parent._rotation;
	if (a1 > 180)
		a1 -= 360;
	else if (a1 <= -180)//
		a1 += 360;
	parent._rotation += a1 * mix;
	a2 = ((a2 + os) * MathUtil::Rad_Deg - child._shearX) * s2 + os2 - child._rotation;
	if (a2 > 180)
		a2 -= 360;
	else if (a2 <= -180)//
		a2 += 360;
	child._rotation += a2 * mix;
}
