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

#include <spine/PhysicsConstraint.h>
#include <spine/PhysicsConstraintData.h>
#include <spine/PhysicsConstraintPose.h>
#include <spine/BonePose.h>
#include <spine/Skeleton.h>
#include <spine/SkeletonData.h>
#include <spine/BoneData.h>
#include <spine/Bone.h>
#include <spine/MathUtil.h>

using namespace spine;

RTTI_IMPL(PhysicsConstraint, Constraint)

PhysicsConstraint::PhysicsConstraint(PhysicsConstraintData &data, Skeleton &skeleton)
	: PhysicsConstraintBase(data), _reset(true), _ux(0), _uy(0), _cx(0), _cy(0), _tx(0), _ty(0), _xOffset(0), _xLag(0), _xVelocity(0), _yOffset(0),
	  _yLag(0), _yVelocity(0), _rotateOffset(0), _rotateLag(0), _rotateVelocity(0), _scaleOffset(0), _scaleLag(0), _scaleVelocity(0), _remaining(0),
	  _lastTime(0) {

	_bone = &skeleton._bones[(size_t) data._bone->getIndex()]->_constrainedPose;
}

PhysicsConstraint &PhysicsConstraint::copy(Skeleton &skeleton) {
	PhysicsConstraint *copy = new (__FILE__, __LINE__) PhysicsConstraint(_data, skeleton);
	copy->_pose.set(_pose);
	return *copy;
}

void PhysicsConstraint::reset(Skeleton &skeleton) {
	_remaining = 0;
	_lastTime = skeleton.getTime();
	_reset = true;
	_xOffset = 0;
	_xLag = 0;
	_xVelocity = 0;
	_yOffset = 0;
	_yLag = 0;
	_yVelocity = 0;
	_rotateOffset = 0;
	_rotateLag = 0;
	_rotateVelocity = 0;
	_scaleOffset = 0;
	_scaleLag = 0;
	_scaleVelocity = 0;
}

void PhysicsConstraint::translate(float x, float y) {
	_ux -= x;
	_uy -= y;
	_cx -= x;
	_cy -= y;
}

void PhysicsConstraint::rotate(float x, float y, float degrees) {
	float r = degrees * MathUtil::Deg_Rad, cosVal = MathUtil::cos(r), sinVal = MathUtil::sin(r);
	float dx = _cx - x, dy = _cy - y;
	translate(dx * cosVal - dy * sinVal - dx, dx * sinVal + dy * cosVal - dy);
}

void PhysicsConstraint::update(Skeleton &skeleton, Physics physics) {
	PhysicsConstraintPose &p = *_appliedPose;
	float mix = p._mix;
	if (mix == 0) return;

	bool x = _data._x > 0, y = _data._y > 0, rotateOrShearX = _data._rotate > 0 || _data._shearX > 0, scaleX = _data._scaleX > 0;
	BonePose *bone = _bone;
	float l = bone->_bone->_data.getLength(), t = _data._step, z = 0;

	switch (physics) {
		case Physics_None:
			return;
		case Physics_Reset:
			reset(skeleton);
			// Fall through.
		case Physics_Update: {
			float delta = MathUtil::max(skeleton._time - _lastTime, 0.0f), aa = _remaining;
			_remaining += delta;
			_lastTime = skeleton._time;

			float bx = bone->_worldX, by = bone->_worldY;
			if (_reset) {
				_reset = false;
				_ux = bx;
				_uy = by;
			} else {
				float a = _remaining, i = p._inertia, f = skeleton._data.getReferenceScale(), d = -1, m = 0, e = 0, qx = _data._limit * delta,
					  qy = qx * MathUtil::abs(skeleton.getScaleY());
				qx *= MathUtil::abs(skeleton._scaleX);
				if (x || y) {
					if (x) {
						float u = (_ux - bx) * i;
						_xOffset += u > qx ? qx : u < -qx ? -qx : u;
						_ux = bx;
					}
					if (y) {
						float u = (_uy - by) * i;
						_yOffset += u > qy ? qy : u < -qy ? -qy : u;
						_uy = by;
					}
					if (a >= t) {
						float xs = _xOffset, ys = _yOffset;
						d = MathUtil::pow(p._damping, 60 * t);
						m = t * p._massInverse;
						e = p._strength;
						float w = f * p._wind, g = f * p._gravity;
						float ax = (w * skeleton._windX + g * skeleton._gravityX) * skeleton._scaleX;
						float ay = (w * skeleton._windY + g * skeleton._gravityY) * skeleton.getScaleY();
						do {
							if (x) {
								_xVelocity += (ax - _xOffset * e) * m;
								_xOffset += _xVelocity * t;
								_xVelocity *= d;
							}
							if (y) {
								_yVelocity -= (ay + _yOffset * e) * m;
								_yOffset += _yVelocity * t;
								_yVelocity *= d;
							}
							a -= t;
						} while (a >= t);
						_xLag = _xOffset - xs;
						_yLag = _yOffset - ys;
					}
					z = MathUtil::max(0.0f, 1 - a / t);
					if (x) bone->_worldX += (_xOffset - _xLag * z) * mix * _data._x;
					if (y) bone->_worldY += (_yOffset - _yLag * z) * mix * _data._y;
				}
				if (rotateOrShearX || scaleX) {
					float ca = MathUtil::atan2(bone->_c, bone->_a), c, s, mr = 0, dx = _cx - bone->_worldX, dy = _cy - bone->_worldY;
					if (dx > qx)
						dx = qx;
					else if (dx < -qx)
						dx = -qx;
					if (dy > qy)
						dy = qy;
					else if (dy < -qy)
						dy = -qy;
					if (rotateOrShearX) {
						mr = (_data._rotate + _data._shearX) * mix;
						z = _rotateLag * MathUtil::max(0.0f, 1 - aa / t);
						float r = MathUtil::atan2(dy + _ty, dx + _tx) - ca - (_rotateOffset - z) * mr;
						_rotateOffset += (r - MathUtil::ceil(r * MathUtil::InvPi_2 - 0.5f) * MathUtil::Pi_2) * i;
						r = (_rotateOffset - z) * mr + ca;
						c = MathUtil::cos(r);
						s = MathUtil::sin(r);
						if (scaleX) {
							r = l * bone->getWorldScaleX();
							if (r > 0) _scaleOffset += (dx * c + dy * s) * i / r;
						}
					} else {
						c = MathUtil::cos(ca);
						s = MathUtil::sin(ca);
						float r = l * bone->getWorldScaleX() - _scaleLag * MathUtil::max(0.0f, 1 - aa / t);
						if (r > 0) _scaleOffset += (dx * c + dy * s) * i / r;
					}
					a = _remaining;
					if (a >= t) {
						if (d == -1) {
							d = MathUtil::pow(p._damping, 60 * t);
							m = t * p._massInverse;
							e = p._strength;
						}
						float ax = p._wind * skeleton._windX + p._gravity * skeleton._gravityX;
						float ay = p._wind * skeleton._windY + p._gravity * skeleton._gravityY;
						float rs = _rotateOffset, ss = _scaleOffset, h = l / f;
						while (true) {
							a -= t;
							if (scaleX) {
								_scaleVelocity += (ax * c - ay * s - _scaleOffset * e) * m;
								_scaleOffset += _scaleVelocity * t;
								_scaleVelocity *= d;
							}
							if (rotateOrShearX) {
								_rotateVelocity -= ((ax * s + ay * c) * h + _rotateOffset * e) * m;
								_rotateOffset += _rotateVelocity * t;
								_rotateVelocity *= d;
								if (a < t) break;
								float r = _rotateOffset * mr + ca;
								c = MathUtil::cos(r);
								s = MathUtil::sin(r);
							} else if (a < t)
								break;
						}
						_rotateLag = _rotateOffset - rs;
						_scaleLag = _scaleOffset - ss;
					}
					z = MathUtil::max(0.0f, 1 - a / t);
				}
				_remaining = a;
			}
			_cx = bone->_worldX;
			_cy = bone->_worldY;
			break;
		}
		case Physics_Pose: {
			z = MathUtil::max(0.0f, 1 - _remaining / t);
			if (x) bone->_worldX += (_xOffset - _xLag * z) * mix * _data._x;
			if (y) bone->_worldY += (_yOffset - _yLag * z) * mix * _data._y;
			break;
		}
	}

	if (rotateOrShearX) {
		float o = (_rotateOffset - _rotateLag * z) * mix, s, c, a;
		if (_data._shearX > 0) {
			float r = 0;
			if (_data._rotate > 0) {
				r = o * _data._rotate;
				s = MathUtil::sin(r);
				c = MathUtil::cos(r);
				a = bone->_b;
				bone->_b = c * a - s * bone->_d;
				bone->_d = s * a + c * bone->_d;
			}
			r += o * _data._shearX;
			s = MathUtil::sin(r);
			c = MathUtil::cos(r);
			a = bone->_a;
			bone->_a = c * a - s * bone->_c;
			bone->_c = s * a + c * bone->_c;
		} else {
			o *= _data._rotate;
			s = MathUtil::sin(o);
			c = MathUtil::cos(o);
			a = bone->_a;
			bone->_a = c * a - s * bone->_c;
			bone->_c = s * a + c * bone->_c;
			a = bone->_b;
			bone->_b = c * a - s * bone->_d;
			bone->_d = s * a + c * bone->_d;
		}
	}
	if (scaleX) {
		float s = 1 + (_scaleOffset - _scaleLag * z) * mix * _data._scaleX;
		bone->_a *= s;
		bone->_c *= s;
		switch (_data._scaleYMode) {
			case ScaleYMode_Uniform:
				bone->_b *= s;
				bone->_d *= s;
				break;
			case ScaleYMode_Volume:
				s = MathUtil::abs(s);
				s = s >= 0.7f ? 1 / s : 4 - 3.67347f * s;
				bone->_b *= s;
				bone->_d *= s;
				break;
			default:
				break;
		}
	}
	if (physics != Physics_Pose) {
		_tx = l * bone->_a;
		_ty = l * bone->_c;
	}
	bone->modifyWorld(skeleton._update);
}

void PhysicsConstraint::sort(Skeleton &skeleton) {
	Bone *bone = _bone->_bone;
	skeleton.sortBone(bone);
	skeleton._updateCache.add(this);
	skeleton.sortReset(bone->_children);
	skeleton.constrained(*bone);
}

bool PhysicsConstraint::isSourceActive() {
	return _bone->_bone->isActive();
}

BonePose &PhysicsConstraint::getBone() {
	return *_bone;
}

void PhysicsConstraint::setBone(BonePose &bone) {
	_bone = &bone;
}
