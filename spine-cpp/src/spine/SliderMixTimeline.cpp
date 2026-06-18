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

#include <spine/SliderMixTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>

#include <spine/Animation.h>
#include <spine/Slider.h>
#include <spine/SliderData.h>
#include <spine/SliderPose.h>
#include <spine/Property.h>

using namespace spine;

RTTI_IMPL(SliderMixTimeline, ConstraintTimeline1)

SliderMixTimeline::SliderMixTimeline(size_t frameCount, size_t bezierCount, int sliderIndex)
	: ConstraintTimeline1(frameCount, bezierCount, sliderIndex, Property_SliderMix) {
	PropertyId ids[] = {((PropertyId) Property_SliderMix << 32) | sliderIndex};
	setPropertyIds(ids, 1);
	_additive = true;
}

SliderMixTimeline::~SliderMixTimeline() {
}

void SliderMixTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
							  bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(out);

	Slider *constraint = (Slider *) skeleton._constraints[_constraintIndex];
	if (constraint->isActive()) {
		SliderPose &pose = appliedPose ? *constraint->_appliedPose : constraint->_pose;
		SliderData &data = constraint->_data;
		pose._mix = getAbsoluteValue(time, alpha, from, add, pose._mix, data._setupPose._mix);
	}
}