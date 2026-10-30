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

#include <spine/TransformConstraint.h>

#include <spine/Bone.h>
#include <spine/BonePose.h>
#include <spine/Skeleton.h>
#include <spine/TransformConstraintData.h>
#include <spine/MathUtil.h>

#include <spine/BoneData.h>

using namespace spine;

RTTI_IMPL(TransformConstraint, Constraint)

TransformConstraint::TransformConstraint(TransformConstraintData &data, Skeleton &skeleton) : TransformConstraintBase(data) {

	_bones.ensureCapacity(data.getBones().size());
	for (size_t i = 0; i < data.getBones().size(); i++) {
		BoneData *boneData = data.getBones()[i];
		_bones.add(&skeleton._bones[boneData->getIndex()]->_constrainedPose);
	}

	_source = skeleton._bones[data._source->getIndex()];
}

TransformConstraint &TransformConstraint::copy(Skeleton &skeleton) {
	TransformConstraint *copy = new (__FILE__, __LINE__) TransformConstraint(_data, skeleton);
	copy->_pose.set(_pose);
	return *copy;
}

/// Applies the constraint to the constrained bones.
void TransformConstraint::update(Skeleton &skeleton, Physics physics) {
	TransformConstraintPose &p = *_appliedPose;
	if (p._mixRotate == 0 && p._mixX == 0 && p._mixY == 0 && p._mixScaleX == 0 && p._mixScaleY == 0 && p._mixShearY == 0) return;

	TransformConstraintData &data = _data;
	bool localSource = data._localSource, localTarget = data._localTarget, additive = data._additive, clamp = data._clamp;
	float *offsets = data._offsets;
	BonePose &source = *_source->_appliedPose;
	if (localSource) {
		source.validateLocalTransform(skeleton);
	}
	FromProperty **fromItems = data._properties.buffer();
	size_t fn = data._properties.size();
	BonePose **bones = _bones.buffer();
	for (size_t i = 0, n = _bones.size(); i < n; i++) {
		BonePose *bone = bones[i];
		if (localTarget) {
			bone->modifyLocal(skeleton);
		} else {
			bone->modifyWorld(skeleton);
		}
		for (size_t f = 0; f < fn; f++) {
			FromProperty *from = fromItems[f];
			float value = from->value(skeleton, source, localSource, offsets) - from->_offset;
			Array<ToProperty *> &toProps = from->_to;
			ToProperty **toItems = toProps.buffer();
			for (size_t t = 0, tn = toProps.size(); t < tn; t++) {
				ToProperty *to = toItems[t];
				if (to->mix(p) != 0) {
					float clamped = to->_offset + value * to->_scale;
					if (clamp) {
						if (to->_offset < to->_max)
							clamped = MathUtil::clamp(clamped, to->_offset, to->_max);
						else
							clamped = MathUtil::clamp(clamped, to->_max, to->_offset);
					}
					to->apply(skeleton, p, *bone, clamped, localTarget, additive);
				}
			}
		}
	}
}

void TransformConstraint::sort(Skeleton &skeleton) {
	if (!_data._localSource) skeleton.sortBone(_source);
	BonePose **bones = _bones.buffer();
	size_t boneCount = _bones.size();
	bool worldTarget = !_data._localTarget;
	if (worldTarget) {
		for (size_t i = 0; i < boneCount; i++) skeleton.sortBone(bones[i]->_bone);
	}
	skeleton._updateCache.add(this);
	for (size_t i = 0; i < boneCount; i++) {
		Bone *bone = bones[i]->_bone;
		skeleton.sortReset(bone->_children);
		skeleton.constrained(*bone);
	}
	for (size_t i = 0; i < boneCount; i++) bones[i]->_bone->_sorted = worldTarget;
}

bool TransformConstraint::isSourceActive() {
	return _source->_active;
}

/// The bones that will be modified by this transform constraint.
Array<BonePose *> &TransformConstraint::getBones() {
	return _bones;
}

/// The bone whose world transform will be copied to the constrained bones.
Bone &TransformConstraint::getSource() {
	return *_source;
}

void TransformConstraint::setSource(Bone &source) {
	_source = &source;
}