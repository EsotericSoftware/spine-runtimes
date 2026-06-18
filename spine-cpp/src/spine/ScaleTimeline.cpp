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

#include <spine/ScaleTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>
#include <spine/Animation.h>

#include <spine/Bone.h>
#include <spine/BoneData.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

using namespace spine;

RTTI_IMPL(ScaleTimeline, BoneTimeline2)

ScaleTimeline::ScaleTimeline(size_t frameCount, size_t bezierCount, int boneIndex)
	: BoneTimeline2(frameCount, bezierCount, boneIndex, Property_ScaleX, Property_ScaleY) {
}

void ScaleTimeline::_apply(BonePose &pose, BonePose &setup, float time, float alpha, MixFrom from, bool add, bool out) {
	if (time < _frames[0]) {
		switch (from) {
			case MixFrom_Setup:
				pose._scaleX = setup._scaleX;
				pose._scaleY = setup._scaleY;
				break;
			case MixFrom_First:
				pose._scaleX += (setup._scaleX - pose._scaleX) * alpha;
				pose._scaleY += (setup._scaleY - pose._scaleY) * alpha;
				break;
			case MixFrom_Current:
				break;
		}
		return;
	}

	float x, y;
	int i = Animation::search(_frames, time, BoneTimeline2::ENTRIES);
	int curveType = (int) _curves[i / BoneTimeline2::ENTRIES];
	switch (curveType) {
		case CurveTimeline::LINEAR: {
			float before = _frames[i];
			x = _frames[i + BoneTimeline2::VALUE1];
			y = _frames[i + BoneTimeline2::VALUE2];
			float t = (time - before) / (_frames[i + BoneTimeline2::ENTRIES] - before);
			x += (_frames[i + BoneTimeline2::ENTRIES + BoneTimeline2::VALUE1] - x) * t;
			y += (_frames[i + BoneTimeline2::ENTRIES + BoneTimeline2::VALUE2] - y) * t;
			break;
		}
		case CurveTimeline::STEPPED: {
			x = _frames[i + BoneTimeline2::VALUE1];
			y = _frames[i + BoneTimeline2::VALUE2];
			break;
		}
		default: {
			x = getBezierValue(time, i, BoneTimeline2::VALUE1, curveType - BoneTimeline2::BEZIER);
			y = getBezierValue(time, i, BoneTimeline2::VALUE2, curveType + BoneTimeline2::BEZIER_SIZE - BoneTimeline2::BEZIER);
		}
	}
	x *= setup._scaleX;
	y *= setup._scaleY;

	if (alpha == 1 && !add) {
		pose._scaleX = x;
		pose._scaleY = y;
	} else {
		float bx, by;
		if (from == MixFrom_Setup) {
			bx = setup._scaleX;
			by = setup._scaleY;
		} else {
			bx = pose._scaleX;
			by = pose._scaleY;
		}
		if (add) {
			pose._scaleX = bx + (x - setup._scaleX) * alpha;
			pose._scaleY = by + (y - setup._scaleY) * alpha;
		} else if (out) {
			pose._scaleX = bx + (MathUtil::abs(x) * MathUtil::sign(bx) - bx) * alpha;
			pose._scaleY = by + (MathUtil::abs(y) * MathUtil::sign(by) - by) * alpha;
		} else {
			bx = MathUtil::abs(bx) * MathUtil::sign(x);
			by = MathUtil::abs(by) * MathUtil::sign(y);
			pose._scaleX = bx + (x - bx) * alpha;
			pose._scaleY = by + (y - by) * alpha;
		}
	}
}

RTTI_IMPL(ScaleXTimeline, BoneTimeline1)

ScaleXTimeline::ScaleXTimeline(size_t frameCount, size_t bezierCount, int boneIndex)
	: BoneTimeline1(frameCount, bezierCount, boneIndex, Property_ScaleX) {
}

void ScaleXTimeline::_apply(BonePose &pose, BonePose &setup, float time, float alpha, MixFrom from, bool add, bool out) {
	pose._scaleX = getScaleValue(time, alpha, from, add, out, pose._scaleX, setup._scaleX);
}

RTTI_IMPL(ScaleYTimeline, BoneTimeline1)

ScaleYTimeline::ScaleYTimeline(size_t frameCount, size_t bezierCount, int boneIndex)
	: BoneTimeline1(frameCount, bezierCount, boneIndex, Property_ScaleY) {
}

void ScaleYTimeline::_apply(BonePose &pose, BonePose &setup, float time, float alpha, MixFrom from, bool add, bool out) {
	pose._scaleY = getScaleValue(time, alpha, from, add, out, pose._scaleY, setup._scaleY);
}