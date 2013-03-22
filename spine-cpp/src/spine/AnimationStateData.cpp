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
#include <spine/AnimationStateData.h>
#include <spine/Animation.h>

using std::invalid_argument;
using std::make_pair;
using std::pair;

namespace spine {

void AnimationStateData::setMixing (Animation *from, Animation *to, float duration) {
	if (!from) throw invalid_argument("from cannot be null.");
	if (!to) throw invalid_argument("to cannot be null.");
	animationToMixTime[make_pair(from, to)] = duration;
}

float AnimationStateData::getMixing (Animation *from, Animation *to) {
	if (!from) throw invalid_argument("from cannot be null.");
	if (!to) throw invalid_argument("to cannot be null.");
	pair<Animation*, Animation*> key = make_pair(from, to);
	if (animationToMixTime.find(key) != animationToMixTime.end()) return animationToMixTime[key];
	return 0;
}

} /* namespace spine */
