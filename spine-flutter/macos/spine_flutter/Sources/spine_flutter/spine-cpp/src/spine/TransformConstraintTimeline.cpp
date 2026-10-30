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

#include <spine/TransformConstraintTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>

#include <spine/Animation.h>
#include <spine/Property.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/TransformConstraint.h>
#include <spine/TransformConstraintData.h>
#include <spine/TransformConstraintPose.h>

using namespace spine;

RTTI_IMPL_MULTI(TransformConstraintTimeline, CurveTimeline, ConstraintTimeline)

TransformConstraintTimeline::TransformConstraintTimeline(size_t frameCount, size_t bezierCount, int transformConstraintIndex)
	: CurveTimeline(frameCount, TransformConstraintTimeline::ENTRIES, bezierCount), ConstraintTimeline(), _constraintIndex(transformConstraintIndex) {
	PropertyId ids[] = {((PropertyId) Property_TransformConstraint << 32) | transformConstraintIndex};
	setPropertyIds(ids, 1);
	_additive = true;
}

TransformConstraintTimeline::~TransformConstraintTimeline() {
}

void TransformConstraintTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add,
										bool out, bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(out);

	TransformConstraint *constraint = (TransformConstraint *) skeleton._constraints[_constraintIndex];
	if (!constraint->isActive()) return;
	TransformConstraintPose &pose = appliedPose ? *constraint->_appliedPose : constraint->_pose;

	if (time < _frames[0]) {
		TransformConstraintPose &setup = constraint->_data._setupPose;
		switch (from) {
			case MixFrom_Setup:
				pose._mixRotate = setup._mixRotate;
				pose._mixX = setup._mixX;
				pose._mixY = setup._mixY;
				pose._mixScaleX = setup._mixScaleX;
				pose._mixScaleY = setup._mixScaleY;
				pose._mixShearY = setup._mixShearY;
				break;
			case MixFrom_First:
				pose._mixRotate += (setup._mixRotate - pose._mixRotate) * alpha;
				pose._mixX += (setup._mixX - pose._mixX) * alpha;
				pose._mixY += (setup._mixY - pose._mixY) * alpha;
				pose._mixScaleX += (setup._mixScaleX - pose._mixScaleX) * alpha;
				pose._mixScaleY += (setup._mixScaleY - pose._mixScaleY) * alpha;
				pose._mixShearY += (setup._mixShearY - pose._mixShearY) * alpha;
				break;
			case MixFrom_Current:
				break;
		}
		return;
	}

	float rotate, x, y, scaleX, scaleY, shearY;
	int i = Animation::search(_frames, time, ENTRIES);
	int curveType = (int) _curves[i / ENTRIES];
	switch (curveType) {
		case LINEAR: {
			float before = _frames[i];
			rotate = _frames[i + ROTATE];
			x = _frames[i + X];
			y = _frames[i + Y];
			scaleX = _frames[i + SCALEX];
			scaleY = _frames[i + SCALEY];
			shearY = _frames[i + SHEARY];
			float t = (time - before) / (_frames[i + ENTRIES] - before);
			rotate += (_frames[i + ENTRIES + ROTATE] - rotate) * t;
			x += (_frames[i + ENTRIES + X] - x) * t;
			y += (_frames[i + ENTRIES + Y] - y) * t;
			scaleX += (_frames[i + ENTRIES + SCALEX] - scaleX) * t;
			scaleY += (_frames[i + ENTRIES + SCALEY] - scaleY) * t;
			shearY += (_frames[i + ENTRIES + SHEARY] - shearY) * t;
			break;
		}
		case STEPPED: {
			rotate = _frames[i + ROTATE];
			x = _frames[i + X];
			y = _frames[i + Y];
			scaleX = _frames[i + SCALEX];
			scaleY = _frames[i + SCALEY];
			shearY = _frames[i + SHEARY];
			break;
		}
		default: {
			rotate = getBezierValue(time, i, ROTATE, curveType - BEZIER);
			x = getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER);
			y = getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER);
			scaleX = getBezierValue(time, i, SCALEX, curveType + BEZIER_SIZE * 3 - BEZIER);
			scaleY = getBezierValue(time, i, SCALEY, curveType + BEZIER_SIZE * 4 - BEZIER);
			shearY = getBezierValue(time, i, SHEARY, curveType + BEZIER_SIZE * 5 - BEZIER);
		}
	}

	TransformConstraintPose &base = from == MixFrom_Setup ? constraint->_data._setupPose : pose;
	if (add) {
		pose._mixRotate = base._mixRotate + rotate * alpha;
		pose._mixX = base._mixX + x * alpha;
		pose._mixY = base._mixY + y * alpha;
		pose._mixScaleX = base._mixScaleX + scaleX * alpha;
		pose._mixScaleY = base._mixScaleY + scaleY * alpha;
		pose._mixShearY = base._mixShearY + shearY * alpha;
	} else {
		pose._mixRotate = base._mixRotate + (rotate - base._mixRotate) * alpha;
		pose._mixX = base._mixX + (x - base._mixX) * alpha;
		pose._mixY = base._mixY + (y - base._mixY) * alpha;
		pose._mixScaleX = base._mixScaleX + (scaleX - base._mixScaleX) * alpha;
		pose._mixScaleY = base._mixScaleY + (scaleY - base._mixScaleY) * alpha;
		pose._mixShearY = base._mixShearY + (shearY - base._mixShearY) * alpha;
	}
}

void TransformConstraintTimeline::setFrame(int frame, float time, float mixRotate, float mixX, float mixY, float mixScaleX, float mixScaleY,
										   float mixShearY) {
	frame *= ENTRIES;
	_frames[frame] = time;
	_frames[frame + ROTATE] = mixRotate;
	_frames[frame + X] = mixX;
	_frames[frame + Y] = mixY;
	_frames[frame + SCALEX] = mixScaleX;
	_frames[frame + SCALEY] = mixScaleY;
	_frames[frame + SHEARY] = mixShearY;
}