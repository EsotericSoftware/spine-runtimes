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

#include <spine/Slider.h>
#include <spine/Skeleton.h>
#include <spine/Bone.h>
#include <spine/BoneData.h>
#include <spine/BonePose.h>
#include <spine/Animation.h>
#include <spine/Timeline.h>
#include <spine/SlotTimeline.h>
#include <spine/SlotCurveTimeline.h>
#include <spine/AttachmentTimeline.h>
#include <spine/SequenceTimeline.h>
#include <spine/ColorTimeline.h>
#include <spine/ConstraintTimeline.h>
#include <spine/ConstraintTimeline1.h>
#include <spine/PhysicsConstraintTimeline.h>
#include <spine/IkConstraintTimeline.h>
#include <spine/PathConstraintMixTimeline.h>
#include <spine/TransformConstraintTimeline.h>
#include <spine/DrawOrderTimeline.h>
#include <spine/DrawOrderFolderTimeline.h>
#include <spine/SliderData.h>
#include <spine/SliderPose.h>
#include <spine/Slot.h>
#include <spine/IkConstraint.h>
#include <spine/PathConstraint.h>
#include <spine/PhysicsConstraint.h>
#include <spine/TransformConstraint.h>
#include <spine/MathUtil.h>

using namespace spine;

RTTI_IMPL(Slider, Constraint)

namespace {
	SlotTimeline *asSlotTimeline(Timeline *timeline) {
		if (timeline->getRTTI().isExactly(AlphaTimeline::rtti)) return static_cast<AlphaTimeline *>(timeline);
		if (timeline->getRTTI().instanceOf(SlotCurveTimeline::rtti)) return static_cast<SlotCurveTimeline *>(timeline);
		if (timeline->getRTTI().isExactly(AttachmentTimeline::rtti)) return static_cast<AttachmentTimeline *>(timeline);
		if (timeline->getRTTI().isExactly(SequenceTimeline::rtti)) return static_cast<SequenceTimeline *>(timeline);
		return NULL;
	}

	ConstraintTimeline *asConstraintTimeline(Timeline *timeline) {
		if (timeline->getRTTI().instanceOf(ConstraintTimeline1::rtti)) return static_cast<ConstraintTimeline1 *>(timeline);
		if (timeline->getRTTI().instanceOf(PhysicsConstraintTimeline::rtti)) return static_cast<PhysicsConstraintTimeline *>(timeline);
		if (timeline->getRTTI().isExactly(PhysicsConstraintResetTimeline::rtti)) return static_cast<PhysicsConstraintResetTimeline *>(timeline);
		if (timeline->getRTTI().isExactly(IkConstraintTimeline::rtti)) return static_cast<IkConstraintTimeline *>(timeline);
		if (timeline->getRTTI().isExactly(PathConstraintMixTimeline::rtti)) return static_cast<PathConstraintMixTimeline *>(timeline);
		if (timeline->getRTTI().isExactly(TransformConstraintTimeline::rtti)) return static_cast<TransformConstraintTimeline *>(timeline);
		return NULL;
	}

	void constrain(Skeleton &skeleton, Constraint *constraint) {
		if (constraint->getRTTI().instanceOf(IkConstraint::rtti))
			skeleton.constrained(*static_cast<IkConstraint *>(constraint));
		else if (constraint->getRTTI().instanceOf(PathConstraint::rtti))
			skeleton.constrained(*static_cast<PathConstraint *>(constraint));
		else if (constraint->getRTTI().instanceOf(PhysicsConstraint::rtti))
			skeleton.constrained(*static_cast<PhysicsConstraint *>(constraint));
		else if (constraint->getRTTI().instanceOf(Slider::rtti))
			skeleton.constrained(*static_cast<Slider *>(constraint));
		else if (constraint->getRTTI().instanceOf(TransformConstraint::rtti))
			skeleton.constrained(*static_cast<TransformConstraint *>(constraint));
	}
}

float Slider::_offsets[6];

Slider::Slider(SliderData &data, Skeleton &skeleton) : SliderBase(data), _bone(NULL) {
	if (data._bone != NULL) {
		_bone = skeleton._bones[data._bone->getIndex()];
	}
}

Slider &Slider::copy(Skeleton &skeleton) {
	Slider *copy = new (__FILE__, __LINE__) Slider(_data, skeleton);
	copy->_pose.set(_pose);
	return *copy;
}

void Slider::update(Skeleton &skeleton, Physics physics) {
	SliderPose &p = *_appliedPose;
	if (p._mix == 0) return;

	Animation *animation = _data._animation;
	if (_bone != NULL) {
		if (!_bone->isActive()) return;
		if (_data._local) _bone->_appliedPose->validateLocalTransform(skeleton);
		p._time = _data._offset +
			(_data._property->value(skeleton, *_bone->_appliedPose, _data._local, _offsets) - _data._property->_offset) * _data._scale;
		if (_data._loop)
			p._time = animation->getDuration() + MathUtil::fmod(p._time, animation->getDuration());
		else
			p._time = MathUtil::max(0.0f, p._time);
	}

	Array<Bone *> &bones = skeleton._bones;
	const Array<int> &indices = animation->getBones();
	for (size_t i = 0, n = indices.size(); i < n; i++) bones[indices[i]]->_appliedPose->modifyLocal(skeleton);

	animation->apply(skeleton, p._time, p._time, _data._loop, NULL, p._mix, MixFrom_Current, _data._additive, false, true);
}

void Slider::sort(Skeleton &skeleton) {
	if (_bone != NULL && !_data._local) skeleton.sortBone(_bone);
	skeleton._updateCache.add(this);

	Array<Bone *> &bones = skeleton._bones;
	const Array<int> &indices = _data._animation->getBones();
	for (size_t i = 0, n = indices.size(); i < n; i++) {
		Bone *bone = bones[indices[i]];
		bone->_sorted = false;
		skeleton.sortReset(bone->getChildren());
		skeleton.constrained(*bone);
	}

	Array<Timeline *> &timelines = _data._animation->getTimelines();
	Array<Slot *> &slots = skeleton._slots;
	Array<Constraint *> &constraints = skeleton._constraints;
	Array<PhysicsConstraint *> &physics = skeleton._physics;
	size_t physicsCount = physics.size();
	for (size_t i = 0, n = timelines.size(); i < n; i++) {
		Timeline *t = timelines[i];

		SlotTimeline *slotTimeline = asSlotTimeline(t);
		if (slotTimeline) {
			skeleton.constrained(*slots[slotTimeline->getSlotIndex()]);
		} else if (t->getRTTI().instanceOf(DrawOrderTimeline::rtti) || t->getRTTI().instanceOf(DrawOrderFolderTimeline::rtti)) {
			skeleton.getDrawOrder().constrained();
		} else {
			ConstraintTimeline *constraintTimeline = asConstraintTimeline(t);
			if (!constraintTimeline) continue;
			if (t->getRTTI().instanceOf(PhysicsConstraintTimeline::rtti) && constraintTimeline->getConstraintIndex() == -1) {
				for (size_t ii = 0; ii < physicsCount; ii++) skeleton.constrained(*physics[ii]);
			} else {
				int index = constraintTimeline->getConstraintIndex();
				if (index != -1) constrain(skeleton, constraints[index]);
			}
		}
	}
}

bool Slider::isSourceActive() {
	return _bone == NULL || _bone->isActive();
}

Bone &Slider::getBone() {
	return *_bone;
}

void Slider::setBone(Bone &bone) {
	_bone = &bone;
}