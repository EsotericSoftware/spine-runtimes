/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <stdexcept>
#include <spine/AnimationState.h>
#include <spine/AnimationStateData.h>
#include <spine/Animation.h>
#include <spine/BaseSkeleton.h>

namespace spine {

AnimationState::AnimationState (AnimationStateData *data) :
				previous(0),
				previousTime(0),
				previousLoop(false),
				mixTime(0),
				mixDuration(0),
				data(data),
				animation(0),
				time(0),
				loop(0) {
}

void AnimationState::update (float delta) {
	time += delta;
	previousTime += delta;
	mixTime += delta;
}

void AnimationState::apply (BaseSkeleton *skeleton) {
	if (!animation) return;
	if (previous) {
		previous->apply(skeleton, previousTime, previousLoop);
		float alpha = mixTime / mixDuration;
		if (alpha >= 1) {
			alpha = 1;
			previous = 0;
		}
		animation->mix(skeleton, time, loop, alpha);
	} else
		animation->apply(skeleton, time, loop);
}

void AnimationState::setAnimation (Animation *animation, bool loop) {
	setAnimation(animation, loop, 0);
}

void AnimationState::setAnimation (Animation *newAnimation, bool loop, float time) {
	previous = 0;
	if (newAnimation && animation && data) {
		mixDuration = data->getMixing(animation, newAnimation);
		if (mixDuration > 0) {
			mixTime = 0;
			previous = animation;
		}
	}
	animation = newAnimation;
	this->loop = loop;
	this->time = time;
}

} /* namespace spine */
