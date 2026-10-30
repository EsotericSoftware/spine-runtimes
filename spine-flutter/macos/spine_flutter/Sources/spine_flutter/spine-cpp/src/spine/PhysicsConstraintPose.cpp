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

#include <spine/PhysicsConstraintPose.h>

using namespace spine;

PhysicsConstraintPose::PhysicsConstraintPose()
	: Pose<PhysicsConstraintPose>(), _inertia(0), _strength(0), _damping(0), _massInverse(0), _wind(0), _gravity(0), _mix(0) {
}

PhysicsConstraintPose::~PhysicsConstraintPose() {
}

void PhysicsConstraintPose::set(PhysicsConstraintPose &pose) {
	_inertia = pose._inertia;
	_strength = pose._strength;
	_damping = pose._damping;
	_massInverse = pose._massInverse;
	_wind = pose._wind;
	_gravity = pose._gravity;
	_mix = pose._mix;
}

float PhysicsConstraintPose::getInertia() {
	return _inertia;
}

void PhysicsConstraintPose::setInertia(float inertia) {
	_inertia = inertia;
}

float PhysicsConstraintPose::getStrength() {
	return _strength;
}

void PhysicsConstraintPose::setStrength(float strength) {
	_strength = strength;
}

float PhysicsConstraintPose::getDamping() {
	return _damping;
}

void PhysicsConstraintPose::setDamping(float damping) {
	_damping = damping;
}

float PhysicsConstraintPose::getMassInverse() {
	return _massInverse;
}

void PhysicsConstraintPose::setMassInverse(float massInverse) {
	_massInverse = massInverse;
}

float PhysicsConstraintPose::getWind() {
	return _wind;
}

void PhysicsConstraintPose::setWind(float wind) {
	_wind = wind;
}

float PhysicsConstraintPose::getGravity() {
	return _gravity;
}

void PhysicsConstraintPose::setGravity(float gravity) {
	_gravity = gravity;
}

float PhysicsConstraintPose::getMix() {
	return _mix;
}

void PhysicsConstraintPose::setMix(float mix) {
	_mix = mix;
}