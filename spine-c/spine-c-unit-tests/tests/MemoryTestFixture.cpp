#include "MemoryTestFixture.h" 
#include "SpineEventMonitor.h" 

#include "spine/spine.h"

#include "KMemory.h" // last include

#define SPINEBOY_JSON "testdata/spineboy/spineboy.json"
#define SPINEBOY_ATLAS "testdata/spineboy/spineboy.atlas"

#define MAX_RUN_TIME 6000 // equal to about 100 seconds of execution

MemoryTestFixture::~MemoryTestFixture()
{
	finalize();
}

void MemoryTestFixture::initialize()
{
	// on a Per- Fixture Basis, before Test execution
}

void MemoryTestFixture::finalize()
{
	// on a Per- Fixture Basis, after all tests pass/fail
}

void MemoryTestFixture::setUp()
{
	// Setup on Per-Test Basis
}

void MemoryTestFixture::tearDown()
{
	// Tear Down on Per-Test Basis
}


//////////////////////////////////////////////////////////////////////////
// Helper methods
static spSkeletonData* readSkeletonJsonData(const char* filename, spAtlas* atlas) {
	spSkeletonJson* json = spSkeletonJson_create(atlas);
	ASSERT(json != 0);

	spSkeletonData* skeletonData = spSkeletonJson_readSkeletonDataFile(json, filename);
	ASSERT(skeletonData != 0);

	spSkeletonJson_dispose(json);
	return skeletonData;
}

static void LoadSpineboyExample(spAtlas* &atlas, spSkeletonData* &skeletonData, spAnimationStateData* &stateData, spSkeleton* &skeleton, spAnimationState* &state)
{
	///////////////////////////////////////////////////////////////////////////
	// Global Animation Information
	atlas = spAtlas_createFromFile(SPINEBOY_ATLAS, 0);
	ASSERT(atlas != 0);

	skeletonData = readSkeletonJsonData(SPINEBOY_JSON, atlas);
	ASSERT(skeletonData != 0);

	stateData = spAnimationStateData_create(skeletonData);
	ASSERT(stateData != 0);
	stateData->defaultMix = 0.4f; // force mixing

	///////////////////////////////////////////////////////////////////////////
	// Animation Instance 
	skeleton = spSkeleton_create(skeletonData);
	ASSERT(skeleton != 0);

	state = spAnimationState_create(stateData);
	ASSERT(state != 0);
}

static void DisposeAll(spSkeleton* skeleton, spAnimationState* state, spAnimationStateData* stateData, spSkeletonData* skeletonData, spAtlas* atlas)
{
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


//////////////////////////////////////////////////////////////////////////
// Reproduce Memory leak as described in Issue #776
// https://github.com/EsotericSoftware/spine-runtimes/issues/776
void MemoryTestFixture::reproduceIssue_776()
{
	spAtlas* atlas = 0;
	spSkeletonData* skeletonData = 0;
	spAnimationStateData* stateData = 0;
	spSkeleton* skeleton = 0;
	spAnimationState* state = 0;

	//////////////////////////////////////////////////////////////////////////
	// Initialize Animations
	LoadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);

	///////////////////////////////////////////////////////////////////////////
	// Run animation
	spSkeleton_setToSetupPose(skeleton);
	InterruptMonitor eventMonitor(state);
	//eventMonitor.SetDebugLogging(true);

	// Interrupt the animation on this specific sequence of spEventType(s)
	eventMonitor
		.AddInterruptEvent(SP_ANIMATION_INTERRUPT, "jump")
		.AddInterruptEvent(SP_ANIMATION_START);

	spAnimationState_setAnimationByName(state, 0, "walk", true);
	spAnimationState_addAnimationByName(state, 0, "jump", false, 0.0f);
	spAnimationState_addAnimationByName(state, 0, "run",  true,  0.0f);
	spAnimationState_addAnimationByName(state, 0, "jump", false, 3.0f);
	spAnimationState_addAnimationByName(state, 0, "walk", true,  0.0f);
	spAnimationState_addAnimationByName(state, 0, "idle", false, 1.0f);

	for (int i = 0; i < MAX_RUN_TIME && eventMonitor.isAnimationPlaying(); ++i) {
		const float timeSlice = 1.0f / 60.0f;
		spSkeleton_update(skeleton, timeSlice);
		spAnimationState_update(state, timeSlice);
		spAnimationState_apply(state, skeleton);
	}

	//////////////////////////////////////////////////////////////////////////
	// Cleanup Animations
	DisposeAll(skeleton, state, stateData, skeletonData, atlas);
}

void MemoryTestFixture::reproduceIssue_777()
{
	spAtlas* atlas = 0;
	spSkeletonData* skeletonData = 0;
	spAnimationStateData* stateData = 0;
	spSkeleton* skeleton = 0;
	spAnimationState* state = 0;

	//////////////////////////////////////////////////////////////////////////
	// Initialize Animations
	LoadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);

	///////////////////////////////////////////////////////////////////////////
	// Run animation
	spSkeleton_setToSetupPose(skeleton);
	SpineEventMonitor eventMonitor(state);
	//eventMonitor.SetDebugLogging(true);

	// Set Animation and Play for 5 frames
	spAnimationState_setAnimationByName(state, 0, "walk", true);
	for (int i = 0; i < 5; ++i) {
		const float timeSlice = 1.0f / 60.0f;
		spSkeleton_update(skeleton, timeSlice);
		spAnimationState_update(state, timeSlice);
		spAnimationState_apply(state, skeleton);
	}

	// Change animation twice in a row
	spAnimationState_setAnimationByName(state, 0, "walk", false);
	spAnimationState_setAnimationByName(state, 0, "run", false);

	// run normal update
	for (int i = 0; i < 5; ++i) {
		const float timeSlice = 1.0f / 60.0f;
		spSkeleton_update(skeleton, timeSlice);
		spAnimationState_update(state, timeSlice);
		spAnimationState_apply(state, skeleton);
	}

	// Now we'd lose mixingFrom (the first "walk" entry we set above) and should leak
	spAnimationState_setAnimationByName(state, 0, "run", false);

	//////////////////////////////////////////////////////////////////////////
	// Cleanup Animations
	DisposeAll(skeleton, state, stateData, skeletonData, atlas);
}

spSkeleton* skeleton = 0;
static void  spineAnimStateHandler(spAnimationState* state, int type, spTrackEntry* entry, spEvent* event)
{
	if (type == SP_ANIMATION_COMPLETE)
	{
		spAnimationState_setAnimationByName(state, 0, "walk", false);
		spAnimationState_update(state, 0);
		spAnimationState_apply(state, skeleton);
	}
}

void MemoryTestFixture::reproduceIssue_Loop()
{
	spAtlas* atlas = 0;
	spSkeletonData* skeletonData = 0;
	spAnimationStateData* stateData = 0;
	spAnimationState* state = 0;

	//////////////////////////////////////////////////////////////////////////
	// Initialize Animations
	LoadSpineboyExample(atlas, skeletonData, stateData, skeleton, state);

	///////////////////////////////////////////////////////////////////////////

	if (state)
		state->listener = (spAnimationStateListener)&spineAnimStateHandler;

	spAnimationState_setAnimationByName(state, 0, "walk", false);

	// run normal update
	for (int i = 0; i < 50; ++i) {
		const float timeSlice = 1.0f / 60.0f;
		spSkeleton_update(skeleton, timeSlice);
		spAnimationState_update(state, timeSlice);
		spAnimationState_apply(state, skeleton);
	}

	DisposeAll(skeleton, state, stateData, skeletonData, atlas);
}


