/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#ifdef SPINE_UE4
#include "SpinePluginPrivatePCH.h"
#endif

#include <spine/PathConstraintPositionTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>

#include <spine/Animation.h>
#include <spine/PathConstraint.h>
#include <spine/PathConstraintData.h>
#include <spine/Property.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

using namespace spine;

RTTI_IMPL(PathConstraintPositionTimeline, CurveTimeline1)

PathConstraintPositionTimeline::PathConstraintPositionTimeline(size_t frameCount, size_t bezierCount,
															   int pathConstraintIndex) : CurveTimeline1(frameCount,
																										 bezierCount),
																						  _pathConstraintIndex(
																								  pathConstraintIndex) {
	PropertyId ids[] = {((PropertyId) Property_PathConstraintPosition << 32) | pathConstraintIndex};
	setPropertyIds(ids, 1);
}

PathConstraintPositionTimeline::~PathConstraintPositionTimeline() {
}

void PathConstraintPositionTimeline::apply(Skeleton &skeleton, float lastTime, float time, Vector<Event *> *pEvents,
										   float alpha, MixBlend blend, MixDirection direction) {
	SP_UNUSED(lastTime);
	SP_UNUSED(pEvents);
	SP_UNUSED(direction);

	PathConstraint *constraintP = skeleton._pathConstraints[_pathConstraintIndex];
	PathConstraint &constraint = *constraintP;
	if (!constraint.isActive()) return;

	if (time < _frames[0]) {
		switch (blend) {
			case MixBlend_Setup:
				constraint._position = constraint._data._position;
				return;
			case MixBlend_First:
				constraint._position += (constraint._data._position - constraint._position) * alpha;
				return;
			default:
				return;
		}
	}

	float position = getCurveValue(time);

	if (blend == MixBlend_Setup)
		constraint._position = constraint._data._position + (position - constraint._data._position) * alpha;
	else
		constraint._position += (position - constraint._position) * alpha;
}
