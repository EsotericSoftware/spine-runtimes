/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <stdio.h>
#include <spine/spine.h>

#include "TestHarness.h"

#define SPINEBOY_JSON "testdata/raptor/raptor-pro.json"
#define SPINEBOY_ATLAS "testdata/raptor/raptor.atlas"

using namespace Spine;

void loadSpineboy(Atlas* &atlas, SkeletonData* &skeletonData, AnimationStateData* &stateData, Skeleton* &skeleton, AnimationState* &state) {
	atlas = new (__FILE__, __LINE__) Atlas(SPINEBOY_ATLAS, 0);
	assert(atlas != 0);

	SkeletonJson json(atlas);
	skeletonData = json.readSkeletonDataFile(SPINEBOY_JSON);
	assert(skeletonData);

	skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);
	assert(skeleton != 0);

	stateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
	assert(stateData != 0);
	stateData->setDefaultMix(0.4f);

	state = new (__FILE__, __LINE__) AnimationState(stateData);
}

void dispose(Atlas* atlas, SkeletonData* skeletonData, AnimationStateData* stateData, Skeleton* skeleton, AnimationState* state) {
	delete skeleton;
	delete state;
	delete stateData;
	delete skeletonData;
	delete atlas;
}

void reproduceIssue_776() {
	Atlas* atlas = 0;
	SkeletonData* skeletonData = 0;
	AnimationStateData* stateData = 0;
	Skeleton* skeleton = 0;
	AnimationState* state = 0;

	loadSpineboy(atlas, skeletonData, stateData, skeleton, state);
	dispose(atlas, skeletonData, stateData, skeleton, state);
}

int main (int argc, char** argv) {
	TestSpineExtension* ext = new TestSpineExtension();
	SpineExtension::setInstance(ext);

	reproduceIssue_776();

	ext->reportLeaks();
}