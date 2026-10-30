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

#include <spine/PhysicsConstraintTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>

#include <spine/Animation.h>
#include <spine/Property.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/PhysicsConstraint.h>
#include <cfloat>

using namespace spine;

RTTI_IMPL_MULTI(PhysicsConstraintTimeline, CurveTimeline, ConstraintTimeline)
RTTI_IMPL(PhysicsConstraintInertiaTimeline, PhysicsConstraintTimeline)
RTTI_IMPL(PhysicsConstraintStrengthTimeline, PhysicsConstraintTimeline)
RTTI_IMPL(PhysicsConstraintDampingTimeline, PhysicsConstraintTimeline)
RTTI_IMPL(PhysicsConstraintMassTimeline, PhysicsConstraintTimeline)
RTTI_IMPL(PhysicsConstraintWindTimeline, PhysicsConstraintTimeline)
RTTI_IMPL(PhysicsConstraintGravityTimeline, PhysicsConstraintTimeline)
RTTI_IMPL(PhysicsConstraintMixTimeline, PhysicsConstraintTimeline)
RTTI_IMPL_MULTI(PhysicsConstraintResetTimeline, Timeline, ConstraintTimeline)

PhysicsConstraintTimeline::PhysicsConstraintTimeline(size_t frameCount, size_t bezierCount, int constraintIndex, Property property)
	: CurveTimeline1(frameCount, bezierCount), ConstraintTimeline(), _constraintIndex(constraintIndex) {
	PropertyId ids[] = {((PropertyId) property << 32) | constraintIndex};
	setPropertyIds(ids, 1);
}

void PhysicsConstraintTimeline::apply(Skeleton &skeleton, float, float time, Array<Event *> *, float alpha, MixFrom from, bool add, bool out,
									  bool appliedPose) {
	SP_UNUSED(out);
	if (add && !_additive) add = false;
	if (_constraintIndex == -1) {
		float value = time >= _frames[0] ? getCurveValue(time) : 0;

		Array<PhysicsConstraint *> &physicsConstraints = skeleton.getPhysicsConstraints();
		for (size_t i = 0; i < physicsConstraints.size(); i++) {
			PhysicsConstraint *constraint = physicsConstraints[i];
			if (constraint->isActive() && global(constraint->_data)) {
				PhysicsConstraintPose &pose = appliedPose ? *constraint->_appliedPose : constraint->_pose;
				set(pose, getAbsoluteValue(time, alpha, from, add, get(pose), get(constraint->_data._setupPose), value));
			}
		}
	} else {
		PhysicsConstraint *constraint = static_cast<PhysicsConstraint *>(skeleton.getConstraints()[_constraintIndex]);
		if (constraint->isActive()) {
			PhysicsConstraintPose &pose = appliedPose ? *constraint->_appliedPose : constraint->_pose;
			set(pose, getAbsoluteValue(time, alpha, from, add, get(pose), get(constraint->_data._setupPose)));
		}
	}
}

void PhysicsConstraintResetTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *, float alpha, MixFrom from, bool add,
										   bool out, bool appliedPose) {
	PhysicsConstraint *constraint = nullptr;
	if (_constraintIndex != -1) {
		constraint = static_cast<PhysicsConstraint *>(skeleton.getConstraints()[_constraintIndex]);
		if (!constraint->isActive()) return;
	}

	if (lastTime > time) {// Apply after lastTime for looped animations.
		apply(skeleton, lastTime, FLT_MAX, nullptr, alpha, MixFrom_Current, false, false, false);
		lastTime = -1;
	} else if (lastTime >= _frames[_frames.size() - 1])// Last time is after last frame.
		return;
	if (time < _frames[0]) return;

	if (lastTime < _frames[0] || time >= _frames[Animation::search(_frames, lastTime) + 1]) {
		if (constraint != nullptr)
			constraint->reset(skeleton);
		else {
			Array<PhysicsConstraint *> &physicsConstraints = skeleton.getPhysicsConstraints();
			for (size_t i = 0; i < physicsConstraints.size(); i++) {
				constraint = physicsConstraints[i];
				if (constraint->isActive()) constraint->reset(skeleton);
			}
		}
	}
}
