#include "C_InterfaceTestFixture.h" 
#include "SpineEventMonitor.h" 

#include "spine/spine.h"
#include <vector>

#include "KMemory.h" // last include

#define SPINEBOY_JSON "testdata/spineboy/spineboy-ess.json"
#define SPINEBOY_ATLAS "testdata/spineboy/spineboy.atlas"

#define RAPTOR_JSON "testdata/raptor/raptor-pro.json"
#define RAPTOR_ATLAS "testdata/raptor/raptor.atlas"

#define GOBLINS_JSON "testdata/goblins/goblins-pro.json"
#define GOBLINS_ATLAS "testdata/goblins/goblins.atlas"

#define MAX_RUN_TIME 6000 // equal to about 100 seconds of execution

void C_InterfaceTestFixture::setUp()
{
}

void C_InterfaceTestFixture::tearDown()
{
}

static spSkeletonData* readSkeletonJsonData(const char* filename, spAtlas* atlas) {
	spSkeletonJson* json = spSkeletonJson_create(atlas);
	ASSERT(json != 0);

	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, filename);
	ASSERT(skeletonData != 0);

	spSkeletonJson_dispose(json);
	return skeletonData;
}

typedef std::vector<std::string> AnimList;

static size_t enumerateAnimations(AnimList& outList, spSkeletonData* skeletonData)
{
	if (skeletonData){

		for (int n = 0; n < skeletonData->animationsCount; n++)
			outList.push_back(skeletonData->animations[n]->name);
	}

	return outList.size();
}

static void testRunner(const char* jsonName, const char* atlasName)
{
	///////////////////////////////////////////////////////////////////////////
	// Global Animation Information
	spAtlas* atlas = spAtlas_createFromFile(atlasName, 0);
	ASSERT(atlas != 0);

	spSkeletonData* skeletonData = readSkeletonJsonData(jsonName, atlas);
	ASSERT(skeletonData != 0);

	spAnimationStateData* stateData = spAnimationStateData_create(skeletonData);
	ASSERT(stateData != 0);
	stateData->defaultMix = 0.2f; // force mixing

	///////////////////////////////////////////////////////////////////////////
	// Animation Instance 
	spSkeleton* skeleton = spSkeleton_create(skeletonData);
	ASSERT(skeleton != 0);

	spAnimationState* state = spAnimationState_create(stateData);
	ASSERT(state != 0);


	///////////////////////////////////////////////////////////////////////////
	// Run animation
	spSkeleton_setToSetupPose(skeleton);
	SpineEventMonitor eventMonitor(state);
//	eventMonitor.SetDebugLogging(true);


	AnimList anims; // Let's chain all the animations together as a test
	size_t count = enumerateAnimations(anims, skeletonData);
	if (count > 0) spAnimationState_setAnimationByName(state, 0, anims[0].c_str(), false);
	for (size_t i = 1; i < count; ++i) {
		spAnimationState_addAnimationByName(state, 0, anims[i].c_str(), false, 0.0f);
	}

	// Run Loop
	for (int i = 0; i < MAX_RUN_TIME && eventMonitor.isAnimationPlaying(); ++i) {
		const float timeSlice = 1.0f / 60.0f;
		spSkeleton_update(skeleton, timeSlice);
		spAnimationState_update(state, timeSlice);
		spAnimationState_apply(state, skeleton);
	}

	
	///////////////////////////////////////////////////////////////////////////
	// Dispose Instance
	spSkeleton_dispose(skeleton);
	spAnimationState_dispose(state);

	///////////////////////////////////////////////////////////////////////////
	// Dispose Global
	spAnimationStateData_dispose(stateData);
	spSkeletonData_dispose(skeletonData);
	spAtlas_dispose(atlas);
}

void C_InterfaceTestFixture::spineboyTestCase()
{
	testRunner(SPINEBOY_JSON, SPINEBOY_ATLAS);
}

void C_InterfaceTestFixture::raptorTestCase()
{
	testRunner(RAPTOR_JSON, RAPTOR_ATLAS);
}

void C_InterfaceTestFixture::goblinsTestCase()
{
	testRunner(GOBLINS_JSON, GOBLINS_ATLAS);
}
