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

#include <spine/PhysicsConstraintData.h>
#include <spine/PhysicsConstraint.h>
#include <spine/BoneData.h>
#include <spine/Skeleton.h>

using namespace spine;

RTTI_IMPL(PhysicsConstraintData, ConstraintData)

PhysicsConstraintData::PhysicsConstraintData(const String &name)
	: ConstraintDataGeneric<PhysicsConstraint, PhysicsConstraintPose>(name), _bone(NULL), _x(0), _y(0), _rotate(0), _scaleX(0), _shearX(0), _limit(0),
	  _step(0), _scaleYMode(ScaleYMode_None), _inertiaGlobal(false), _strengthGlobal(false), _dampingGlobal(false), _massGlobal(false),
	  _windGlobal(false), _gravityGlobal(false), _mixGlobal(false) {
}

BoneData &PhysicsConstraintData::getBone() {
	return *_bone;
}

void PhysicsConstraintData::setBone(BoneData &bone) {
	_bone = &bone;
}

float PhysicsConstraintData::getStep() {
	return _step;
}

void PhysicsConstraintData::setStep(float step) {
	_step = step;
}

float PhysicsConstraintData::getX() {
	return _x;
}

void PhysicsConstraintData::setX(float x) {
	_x = x;
}

float PhysicsConstraintData::getY() {
	return _y;
}

void PhysicsConstraintData::setY(float y) {
	_y = y;
}

float PhysicsConstraintData::getRotate() {
	return _rotate;
}

void PhysicsConstraintData::setRotate(float rotate) {
	_rotate = rotate;
}

float PhysicsConstraintData::getScaleX() {
	return _scaleX;
}

void PhysicsConstraintData::setScaleX(float scaleX) {
	_scaleX = scaleX;
}

float PhysicsConstraintData::getShearX() {
	return _shearX;
}

void PhysicsConstraintData::setShearX(float shearX) {
	_shearX = shearX;
}

float PhysicsConstraintData::getLimit() {
	return _limit;
}

void PhysicsConstraintData::setLimit(float limit) {
	_limit = limit;
}

ScaleYMode PhysicsConstraintData::getScaleYMode() {
	return _scaleYMode;
}

void PhysicsConstraintData::setScaleYMode(ScaleYMode scaleYMode) {
	_scaleYMode = scaleYMode;
}

bool PhysicsConstraintData::getInertiaGlobal() {
	return _inertiaGlobal;
}

void PhysicsConstraintData::setInertiaGlobal(bool inertiaGlobal) {
	_inertiaGlobal = inertiaGlobal;
}

bool PhysicsConstraintData::getStrengthGlobal() {
	return _strengthGlobal;
}

void PhysicsConstraintData::setStrengthGlobal(bool strengthGlobal) {
	_strengthGlobal = strengthGlobal;
}

bool PhysicsConstraintData::getDampingGlobal() {
	return _dampingGlobal;
}

void PhysicsConstraintData::setDampingGlobal(bool dampingGlobal) {
	_dampingGlobal = dampingGlobal;
}

bool PhysicsConstraintData::getMassGlobal() {
	return _massGlobal;
}

void PhysicsConstraintData::setMassGlobal(bool massGlobal) {
	_massGlobal = massGlobal;
}

bool PhysicsConstraintData::getWindGlobal() {
	return _windGlobal;
}

void PhysicsConstraintData::setWindGlobal(bool windGlobal) {
	_windGlobal = windGlobal;
}

bool PhysicsConstraintData::getGravityGlobal() {
	return _gravityGlobal;
}

void PhysicsConstraintData::setGravityGlobal(bool gravityGlobal) {
	_gravityGlobal = gravityGlobal;
}

bool PhysicsConstraintData::getMixGlobal() {
	return _mixGlobal;
}

void PhysicsConstraintData::setMixGlobal(bool mixGlobal) {
	_mixGlobal = mixGlobal;
}

Constraint &PhysicsConstraintData::create(Skeleton &skeleton) {
	return *(new (__FILE__, __LINE__) PhysicsConstraint(*this, skeleton));
}