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

using namespace Spine;

void loadBinary(const String& binaryFile, const String& atlasFile, Atlas* &atlas, SkeletonData* &skeletonData, AnimationStateData* &stateData, Skeleton* &skeleton, AnimationState* &state) {
	atlas = new (__FILE__, __LINE__) Atlas(atlasFile, NULL);
	assert(atlas != NULL);

	SkeletonBinary binary(atlas);
	skeletonData = binary.readSkeletonDataFile(binaryFile);
	assert(skeletonData);

	skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);
	assert(skeleton != NULL);

	stateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
	assert(stateData != NULL);
	stateData->setDefaultMix(0.4f);

	state = new (__FILE__, __LINE__) AnimationState(stateData);
}

void loadJson(const String& jsonFile, const String& atlasFile, Atlas* &atlas, SkeletonData* &skeletonData, AnimationStateData* &stateData, Skeleton* &skeleton, AnimationState* &state) {
	atlas = new (__FILE__, __LINE__) Atlas(atlasFile, NULL);
	assert(atlas != NULL);

	SkeletonJson json(atlas);
	skeletonData = json.readSkeletonDataFile(jsonFile);
	assert(skeletonData);

	skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);
	assert(skeleton != NULL);

	stateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
	assert(stateData != NULL);
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

struct TestData {
	TestData(const String& jsonSkeleton, const String& binarySkeleton, const String& atlas) : _jsonSkeleton(jsonSkeleton), _binarySkeleton(binarySkeleton), _atlas(atlas) { }

	String _jsonSkeleton;
	String _binarySkeleton;
	String _atlas;
};

void testLoading() {
	Vector<TestData> testData;
	testData.add(TestData("testdata/coin/coin-pro.json", "testdata/coin/coin-pro.skel", "testdata/coin/coin.atlas"));
	testData.add(TestData("testdata/goblins/goblins-pro.json", "testdata/goblins/goblins-pro.skel", "testdata/goblins/goblins.atlas"));
	testData.add(TestData("testdata/raptor/raptor-pro.json", "testdata/raptor/raptor-pro.skel", "testdata/raptor/raptor.atlas"));
	testData.add(TestData("testdata/spineboy/spineboy-pro.json", "testdata/spineboy/spineboy-pro.skel", "testdata/spineboy/spineboy.atlas"));
	testData.add(TestData("testdata/stretchyman/stretchyman-pro.json", "testdata/stretchyman/stretchyman-pro.skel", "testdata/stretchyman/stretchyman.atlas"));
	testData.add(TestData("testdata/tank/tank-pro.json", "testdata/tank/tank-pro.skel", "testdata/tank/tank.atlas"));

	for (size_t i = 0; i < testData.size(); i++) {
		TestData& data = testData[i];
		Atlas* atlas = NULL;
		SkeletonData* skeletonData = NULL;
		AnimationStateData* stateData = NULL;
		Skeleton* skeleton = NULL;
		AnimationState* state = NULL;

		printf("Loading %s\n", data._jsonSkeleton.buffer());
		loadJson(data._jsonSkeleton, data._atlas, atlas, skeletonData, stateData, skeleton, state);
		dispose(atlas, skeletonData, stateData, skeleton, state);

		printf("Loading %s\n", data._binarySkeleton.buffer());
		loadBinary(data._binarySkeleton, data._atlas, atlas, skeletonData, stateData, skeleton, state);
		dispose(atlas, skeletonData, stateData, skeleton, state);
	}
}

int main (int argc, char** argv) {
	TestSpineExtension* ext = new TestSpineExtension();
	SpineExtension::setInstance(ext);

 	testLoading();

	ext->reportLeaks();
}
