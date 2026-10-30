/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/BonePose.h>
#include "spine/RTTI.h"
#include <spine/Bone.h>
#include <spine/BoneData.h>
#include <spine/Skeleton.h>
#include <spine/Physics.h>
#include <spine/MathUtil.h>

using namespace spine;

RTTI_IMPL(BonePose, Update);

BonePose::BonePose() : BoneLocal(), _bone(nullptr), _a(0), _b(0), _worldX(0), _c(0), _d(0), _worldY(0), _world(0), _local(0) {
}

BonePose::~BonePose() {
}

void BonePose::update(Skeleton &skeleton, Physics physics) {
	if (_world != skeleton._update) updateWorldTransform(skeleton);
}

void BonePose::updateWorldTransform(Skeleton &skeleton) {
	if (_local == skeleton._update)
		updateLocalTransform(skeleton);
	else
		_world = skeleton._update;

	if (_bone->getParent() == nullptr) {// Root bone.
		float sx = skeleton.getScaleX(), sy = skeleton.getScaleY();
		float rx = (_rotation + _shearX) * MathUtil::Deg_Rad;
		float ry = (_rotation + 90 + _shearY) * MathUtil::Deg_Rad;
		_a = MathUtil::cos(rx) * _scaleX * sx;
		_b = MathUtil::cos(ry) * _scaleY * sx;
		_c = MathUtil::sin(rx) * _scaleX * sy;
		_d = MathUtil::sin(ry) * _scaleY * sy;
		_worldX = _x * sx + skeleton.getX();
		_worldY = _y * sy + skeleton.getY();
		return;
	}

	BonePose &parent = _bone->getParent()->getAppliedPose();
	float pa = parent._a, pb = parent._b, pc = parent._c, pd = parent._d;
	_worldX = pa * _x + pb * _y + parent._worldX;
	_worldY = pc * _x + pd * _y + parent._worldY;

	switch (_inherit) {
		case Inherit_Normal: {
			float rx = (_rotation + _shearX) * MathUtil::Deg_Rad;
			float ry = (_rotation + 90 + _shearY) * MathUtil::Deg_Rad;
			float la = MathUtil::cos(rx) * _scaleX;
			float lb = MathUtil::cos(ry) * _scaleY;
			float lc = MathUtil::sin(rx) * _scaleX;
			float ld = MathUtil::sin(ry) * _scaleY;
			_a = pa * la + pb * lc;
			_b = pa * lb + pb * ld;
			_c = pc * la + pd * lc;
			_d = pc * lb + pd * ld;
			return;
		}
		case Inherit_OnlyTranslation: {
			float sx = skeleton.getScaleX(), sy = skeleton.getScaleY();
			float rx = (_rotation + _shearX) * MathUtil::Deg_Rad;
			float ry = (_rotation + 90 + _shearY) * MathUtil::Deg_Rad;
			_a = MathUtil::cos(rx) * _scaleX * sx;
			_b = MathUtil::cos(ry) * _scaleY * sx;
			_c = MathUtil::sin(rx) * _scaleX * sy;
			_d = MathUtil::sin(ry) * _scaleY * sy;
			break;
		}
		case Inherit_NoRotationOrReflection: {
			float sx = skeleton.getScaleX(), sy = skeleton.getScaleY(), sxi = 1 / sx, syi = 1 / sy;
			pa *= sxi;
			pc *= syi;
			float s = pa * pa + pc * pc, r;
			if (s > MathUtil::EpsilonSq) {
				s = MathUtil::abs(pa * pd * syi - pb * sxi * pc) / s;
				pb = pc * s;
				pd = pa * s;
				r = _rotation - MathUtil::atan2Deg(pc, pa);
			} else {
				pa = 0;
				pc = 0;
				r = _rotation - 90 + MathUtil::atan2Deg(pd, pb);
			}
			float rx = (r + _shearX) * MathUtil::Deg_Rad;
			float ry = (r + _shearY + 90) * MathUtil::Deg_Rad;
			float la = MathUtil::cos(rx) * _scaleX;
			float lb = MathUtil::cos(ry) * _scaleY;
			float lc = MathUtil::sin(rx) * _scaleX;
			float ld = MathUtil::sin(ry) * _scaleY;
			_a = (pa * la - pb * lc) * sx;
			_b = (pa * lb - pb * ld) * sx;
			_c = (pc * la + pd * lc) * sy;
			_d = (pc * lb + pd * ld) * sy;
			break;
		}
		case Inherit_NoScale:
		case Inherit_NoScaleOrReflection: {
			float sx = skeleton.getScaleX(), sy = skeleton.getScaleY(), sxi = 1 / sx, syi = 1 / sy;
			float r = _rotation * MathUtil::Deg_Rad, cosR = MathUtil::cos(r), sinR = MathUtil::sin(r);
			float za = (pa * cosR + pb * sinR) * sxi;
			float zc = (pc * cosR + pd * sinR) * syi;
			float s = 1 / MathUtil::sqrt(za * za + zc * zc);
			za *= s;
			zc *= s;
			float zb = -zc, zd = za;
			if (_inherit == Inherit_NoScale && (pa * pd - pb * pc < 0) != ((sx < 0) != (sy < 0))) {
				zb = -zb;
				zd = -zd;
			}
			float rx = _shearX * MathUtil::Deg_Rad;
			float ry = (90 + _shearY) * MathUtil::Deg_Rad;
			float la = MathUtil::cos(rx) * _scaleX;
			float lb = MathUtil::cos(ry) * _scaleY;
			float lc = MathUtil::sin(rx) * _scaleX;
			float ld = MathUtil::sin(ry) * _scaleY;
			_a = (za * la + zb * lc) * sx;
			_b = (za * lb + zb * ld) * sx;
			_c = (zc * la + zd * lc) * sy;
			_d = (zc * lb + zd * ld) * sy;
			break;
		}
	}
}

void BonePose::updateLocalTransform(Skeleton &skeleton) {
	_local = 0;
	_world = skeleton._update;

	float sx = skeleton.getScaleX(), sy = skeleton.getScaleY();
	if (_bone->getParent() == nullptr) {
		float sxi = 1 / sx, syi = 1 / sy;
		_x = (_worldX - skeleton.getX()) * sxi;
		_y = (_worldY - skeleton.getY()) * syi;
		setLocal(_a * sxi, _b * sxi, _c * syi, _d * syi, 0);
		return;
	}

	BonePose &parent = _bone->getParent()->getAppliedPose();
	float pa = parent._a, pb = parent._b, pc = parent._c, pd = parent._d;
	float pad = pa * pd - pb * pc, pid = 1 / pad;
	float ia = pd * pid, ib = pb * pid, ic = pc * pid, id = pa * pid;
	float dx = _worldX - parent._worldX, dy = _worldY - parent._worldY;
	_x = dx * ia - dy * ib;
	_y = dy * id - dx * ic;

	switch (_inherit) {
		case Inherit_Normal:
			setLocal(ia * _a - ib * _c, ia * _b - ib * _d, id * _c - ic * _a, id * _d - ic * _b, 0);
			break;
		case Inherit_OnlyTranslation: {
			float sxi = 1 / sx, syi = 1 / sy;
			setLocal(_a * sxi, _b * sxi, _c * syi, _d * syi, 0);
			break;
		}
		case Inherit_NoRotationOrReflection: {
			float sxi = 1 / sx, syi = 1 / sy;
			pa *= sxi;
			pc *= syi;
			float wa = _a * sxi, wb = _b * sxi, wc = _c * syi, wd = _d * syi;
			float s = 1 / (pa * pa + pc * pc), det = 1 / MathUtil::abs(pad * sxi * syi);
			setLocal((pa * wa + pc * wc) * s, (pa * wb + pc * wd) * s, (pa * wc - pc * wa) * det, (pa * wd - pc * wb) * det,
					 MathUtil::atan2Deg(pc, pa));
			break;
		}
		case Inherit_NoScale:
		case Inherit_NoScaleOrReflection: {
			float sxi = 1 / sx, syi = 1 / sy;
			float wa = _a * sxi, wb = _b * sxi, wc = _c * syi, wd = _d * syi;
			float tx = pd * _a - pb * _c, ty = pa * _c - pc * _a;
			if (pad < 0) {
				tx = -tx;
				ty = -ty;
			}
			float r = MathUtil::atan2Deg(ty, tx);
			_rotation = r;
			r *= MathUtil::Deg_Rad;
			float cosR = MathUtil::cos(r), sinR = MathUtil::sin(r);
			float za = (pa * cosR + pb * sinR) * sxi;
			float zc = (pc * cosR + pd * sinR) * syi;
			float s = 1 / MathUtil::sqrt(za * za + zc * zc);
			za *= s;
			zc *= s;
			float si = _inherit == Inherit_NoScale && (pad < 0) != ((sx < 0) != (sy < 0)) ? -1.0f : 1.0f;
			setLocal(za * wa + zc * wc, za * wb + zc * wd, (za * wc - zc * wa) * si, (za * wd - zc * wb) * si);
			break;
		}
	}
}

void BonePose::setLocal(float ra, float rb, float rc, float rd) {
	float x = ra * ra + rc * rc, y = rb * rb + rd * rd;
	if (x > MathUtil::EpsilonSq) {
		_shearX = MathUtil::atan2Deg(rc, ra);
		_scaleX = MathUtil::sqrt(x);
	} else {
		_shearX = 0;
		_scaleX = 0;
	}
	_scaleY = MathUtil::sqrt(y);
	if (y > MathUtil::EpsilonSq) {
		_shearY = MathUtil::atan2Deg(rd, rb);
		if (ra * rd - rb * rc < 0) {
			_scaleY = -_scaleY;
			_shearY += 90;
		} else
			_shearY -= 90;
		if (_shearY > 180)
			_shearY -= 360;
		else if (_shearY <= -180)//
			_shearY += 360;
	} else
		_shearY = 0;
}

void BonePose::setLocal(float ra, float rb, float rc, float rd, float ro) {
	_shearX = 0;
	float x = ra * ra + rc * rc, y = rb * rb + rd * rd;
	if (x > MathUtil::EpsilonSq) {
		float r = MathUtil::atan2Deg(rc, ra);
		_rotation = r + ro;
		_scaleX = MathUtil::sqrt(x);
		_scaleY = MathUtil::sqrt(y);
		if (y > MathUtil::EpsilonSq) {
			_shearY = MathUtil::atan2Deg(rd, rb);
			if (ra * rd - rb * rc < 0) {
				_scaleY = -_scaleY;
				_shearY += 90 - r;
			} else
				_shearY -= 90 + r;
			if (_shearY > 180)
				_shearY -= 360;
			else if (_shearY <= -180)//
				_shearY += 360;
		} else
			_shearY = 0;
	} else {
		_scaleX = 0;
		_scaleY = MathUtil::sqrt(y);
		_shearY = 0;
		_rotation = y > MathUtil::EpsilonSq ? MathUtil::atan2Deg(rd, rb) - 90 + ro : ro;
	}
}

void BonePose::validateLocalTransform(Skeleton &skeleton) {
	if (_local == skeleton._update) updateLocalTransform(skeleton);
}

void BonePose::modifyLocal(Skeleton &skeleton) {
	if (_local == skeleton._update) updateLocalTransform(skeleton);
	_world = 0;
	resetWorld(skeleton, skeleton._update);
}

void BonePose::modifyWorld(Skeleton &skeleton) {
	int update = skeleton._update;
	_local = update;
	_world = update;
	resetWorld(skeleton, update);
}

void BonePose::resetWorld(Skeleton &skeleton, int update) {
	Array<Bone *> &children = _bone->getChildren();
	for (size_t i = 0, n = children.size(); i < n; i++) {
		BonePose &child = children[i]->getAppliedPose();
		if (child._world == update) {
			if (child._local == update) child.updateLocalTransform(skeleton);
			child._world = 0;
			child.resetWorld(skeleton, update);
		}
	}
}

float BonePose::getA() {
	return _a;
}

void BonePose::setA(float a) {
	this->_a = a;
}

float BonePose::getB() {
	return _b;
}

void BonePose::setB(float b) {
	this->_b = b;
}

float BonePose::getC() {
	return _c;
}

void BonePose::setC(float c) {
	this->_c = c;
}

float BonePose::getD() {
	return _d;
}

void BonePose::setD(float d) {
	this->_d = d;
}

float BonePose::getWorldX() {
	return _worldX;
}

void BonePose::setWorldX(float worldX) {
	this->_worldX = worldX;
}

float BonePose::getWorldY() {
	return _worldY;
}

void BonePose::setWorldY(float worldY) {
	this->_worldY = worldY;
}

float BonePose::getWorldRotationX() {
	return MathUtil::atan2(_c, _a) * MathUtil::Rad_Deg;
}

float BonePose::getWorldRotationY() {
	return MathUtil::atan2(_d, _b) * MathUtil::Rad_Deg;
}

float BonePose::getWorldScaleX() {
	return MathUtil::sqrt(_a * _a + _c * _c);
}

float BonePose::getWorldScaleY() {
	return MathUtil::sqrt(_b * _b + _d * _d);
}


void BonePose::worldToLocal(float worldX, float worldY, float &outLocalX, float &outLocalY) {
	float det = _a * _d - _b * _c;
	float x = worldX - _worldX, y = worldY - _worldY;
	outLocalX = (x * _d - y * _b) / det;
	outLocalY = (y * _a - x * _c) / det;
}

void BonePose::localToWorld(float localX, float localY, float &outWorldX, float &outWorldY) {
	outWorldX = localX * _a + localY * _b + _worldX;
	outWorldY = localX * _c + localY * _d + _worldY;
}

void BonePose::worldToParent(float worldX, float worldY, float &outParentX, float &outParentY) {
	if (_bone->getParent() == nullptr) {
		outParentX = worldX;
		outParentY = worldY;
	} else {
		_bone->getParent()->getAppliedPose().worldToLocal(worldX, worldY, outParentX, outParentY);
	}
}

void BonePose::parentToWorld(float parentX, float parentY, float &outWorldX, float &outWorldY) {
	if (_bone->getParent() == nullptr) {
		outWorldX = parentX;
		outWorldY = parentY;
	} else {
		_bone->getParent()->getAppliedPose().localToWorld(parentX, parentY, outWorldX, outWorldY);
	}
}

float BonePose::worldToLocalRotation(float worldRotation) {
	worldRotation *= MathUtil::Deg_Rad;
	float sinRot = MathUtil::sin(worldRotation), cosRot = MathUtil::cos(worldRotation);
	return MathUtil::atan2(_a * sinRot - _c * cosRot, _d * cosRot - _b * sinRot) * MathUtil::Rad_Deg + _rotation - _shearX;
}

float BonePose::localToWorldRotation(float localRotation) {
	localRotation = (localRotation - _rotation - _shearX) * MathUtil::Deg_Rad;
	float sinRot = MathUtil::sin(localRotation), cosRot = MathUtil::cos(localRotation);
	return MathUtil::atan2(cosRot * _c + sinRot * _d, cosRot * _a + sinRot * _b) * MathUtil::Rad_Deg;
}

void BonePose::rotateWorld(float degrees) {
	degrees *= MathUtil::Deg_Rad;
	float sinRot = MathUtil::sin(degrees), cosRot = MathUtil::cos(degrees);
	float ra = _a, rb = _b;
	_a = cosRot * ra - sinRot * _c;
	_b = cosRot * rb - sinRot * _d;
	_c = sinRot * ra + cosRot * _c;
	_d = sinRot * rb + cosRot * _d;
}