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

#include <spine/PathConstraintData.h>
#include <spine/PathConstraint.h>
#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/Skeleton.h>

using namespace spine;

RTTI_IMPL(PathConstraintData, ConstraintData)

PathConstraintData::PathConstraintData(const String &name)
	: ConstraintDataGeneric<PathConstraint, PathConstraintPose>(name), _slot(NULL), _positionMode(PositionMode_Fixed),
	  _spacingMode(SpacingMode_Length), _rotateMode(RotateMode_Tangent), _offsetRotation(0) {
}


Array<BoneData *> &PathConstraintData::getBones() {
	return _bones;
}

SlotData &PathConstraintData::getSlot() {
	return *_slot;
}

void PathConstraintData::setSlot(SlotData &slot) {
	_slot = &slot;
}

PositionMode PathConstraintData::getPositionMode() {
	return _positionMode;
}

void PathConstraintData::setPositionMode(PositionMode positionMode) {
	_positionMode = positionMode;
}

SpacingMode PathConstraintData::getSpacingMode() {
	return _spacingMode;
}

void PathConstraintData::setSpacingMode(SpacingMode spacingMode) {
	_spacingMode = spacingMode;
}

RotateMode PathConstraintData::getRotateMode() {
	return _rotateMode;
}

void PathConstraintData::setRotateMode(RotateMode rotateMode) {
	_rotateMode = rotateMode;
}

float PathConstraintData::getOffsetRotation() {
	return _offsetRotation;
}

void PathConstraintData::setOffsetRotation(float offsetRotation) {
	_offsetRotation = offsetRotation;
}

Constraint &PathConstraintData::create(Skeleton &skeleton) {
	return *(new (__FILE__, __LINE__) PathConstraint(*this, skeleton));
}