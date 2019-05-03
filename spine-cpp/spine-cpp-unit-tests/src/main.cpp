#include <stdio.h>
#include <spine/spine.h>
#include <spine/Debug.h>

#pragma warning ( disable : 4710 )

using namespace spine;

void loadBinary(const String &binaryFile, const String &atlasFile, Atlas *&atlas, SkeletonData *&skeletonData,
				AnimationStateData *&stateData, Skeleton *&skeleton, AnimationState *&state) {
	atlas = new(__FILE__, __LINE__) Atlas(atlasFile, NULL);
	assert(atlas != NULL);

	SkeletonBinary binary(atlas);
	skeletonData = binary.readSkeletonDataFile(binaryFile);
	assert(skeletonData);

	skeleton = new(__FILE__, __LINE__) Skeleton(skeletonData);
	assert(skeleton != NULL);

	stateData = new(__FILE__, __LINE__) AnimationStateData(skeletonData);
	assert(stateData != NULL);
	stateData->setDefaultMix(0.4f);

	state = new(__FILE__, __LINE__) AnimationState(stateData);
}

void loadJson(const String &jsonFile, const String &atlasFile, Atlas *&atlas, SkeletonData *&skeletonData,
			  AnimationStateData *&stateData, Skeleton *&skeleton, AnimationState *&state) {
	atlas = new(__FILE__, __LINE__) Atlas(atlasFile, NULL);
	assert(atlas != NULL);

	SkeletonJson json(atlas);
	skeletonData = json.readSkeletonDataFile(jsonFile);
	assert(skeletonData);

	skeleton = new(__FILE__, __LINE__) Skeleton(skeletonData);
	assert(skeleton != NULL);

	stateData = new(__FILE__, __LINE__) AnimationStateData(skeletonData);
	assert(stateData != NULL);
	stateData->setDefaultMix(0.4f);

	state = new(__FILE__, __LINE__) AnimationState(stateData);
}

void dispose(Atlas *atlas, SkeletonData *skeletonData, AnimationStateData *stateData, Skeleton *skeleton,
			 AnimationState *state) {
	delete skeleton;
	delete state;
	delete stateData;
	delete skeletonData;
	delete atlas;
}

struct TestData {
	TestData(const String &jsonSkeleton, const String &binarySkeleton, const String &atlas) : _jsonSkeleton(
			jsonSkeleton), _binarySkeleton(binarySkeleton), _atlas(atlas) {}

	String _jsonSkeleton;
	String _binarySkeleton;
	String _atlas;
};

void testLoading() {
	Vector<TestData> testData;
	testData.add(TestData("testdata/coin/coin-pro.json", "testdata/coin/coin-pro.skel", "testdata/coin/coin.atlas"));
/*testData.add(TestData("testdata/goblins/goblins-pro.json", "testdata/goblins/goblins-pro.skel",
						  "testdata/goblins/goblins.atlas"));
	testData.add(TestData("testdata/raptor/raptor-pro.json", "testdata/raptor/raptor-pro.skel",
						  "testdata/raptor/raptor.atlas"));
	testData.add(TestData("testdata/spineboy/spineboy-pro.json", "testdata/spineboy/spineboy-pro.skel",
						  "testdata/spineboy/spineboy.atlas"));
	testData.add(TestData("testdata/stretchyman/stretchyman-pro.json", "testdata/stretchyman/stretchyman-pro.skel",
						  "testdata/stretchyman/stretchyman.atlas"));
	testData.add(TestData("testdata/tank/tank-pro.json", "testdata/tank/tank-pro.skel", "testdata/tank/tank.atlas"));*/

	for (size_t i = 0; i < testData.size(); i++) {
		TestData &data = testData[i];
		Atlas *atlas = NULL;
		SkeletonData *skeletonData = NULL;
		AnimationStateData *stateData = NULL;
		Skeleton *skeleton = NULL;
		AnimationState *state = NULL;

		printf("Loading %s\n", data._jsonSkeleton.buffer());
		loadJson(data._jsonSkeleton, data._atlas, atlas, skeletonData, stateData, skeleton, state);
		dispose(atlas, skeletonData, stateData, skeleton, state);

		printf("Loading %s\n", data._binarySkeleton.buffer());
		loadBinary(data._binarySkeleton, data._atlas, atlas, skeletonData, stateData, skeleton, state);
		dispose(atlas, skeletonData, stateData, skeleton, state);
	}
}

namespace spine {
	SpineExtension* getDefaultExtension() {
		return new DefaultSpineExtension();
	}
}

int main(int argc, char **argv) {
	DebugExtension debug(SpineExtension::getInstance());
	SpineExtension::setInstance(&debug);

	testLoading();

	debug.reportLeaks();
}
