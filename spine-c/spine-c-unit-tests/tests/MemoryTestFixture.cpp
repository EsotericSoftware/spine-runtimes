#include <spine/extension.h>
#include "MemoryTestFixture.h"
#include "SpineEventMonitor.h" 

#include "spine/spine.h"

#include "KMemory.h" // last include

#define SPINEBOY_JSON "testdata/spineboy/spineboy-ess.json"
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

void MemoryTestFixture::triangulator() {
	spTriangulator* triangulator = spTriangulator_create();
	spFloatArray* polygon = spFloatArray_create(16);
	spFloatArray_add(polygon, 0);
	spFloatArray_add(polygon, 0);
	spFloatArray_add(polygon, 100);
	spFloatArray_add(polygon, 0);
	spFloatArray_add(polygon, 100);
	spFloatArray_add(polygon, 100);
	spFloatArray_add(polygon, 0);
	spFloatArray_add(polygon, 100);

	spShortArray* triangles = spTriangulator_triangulate(triangulator, polygon);
	ASSERT(triangles->size == 6);
	ASSERT(triangles->items[0] == 3);
	ASSERT(triangles->items[1] == 0);
	ASSERT(triangles->items[2] == 1);
	ASSERT(triangles->items[3] == 3);
	ASSERT(triangles->items[4] == 1);
	ASSERT(triangles->items[5] == 2);

	spArrayFloatArray* polys = spTriangulator_decompose(triangulator, polygon, triangles);
	ASSERT(polys->size == 1);
	ASSERT(polys->items[0]->size == 8);
	ASSERT(polys->items[0]->items[0] == 0);
	ASSERT(polys->items[0]->items[1] == 100);
	ASSERT(polys->items[0]->items[2] == 0);
	ASSERT(polys->items[0]->items[3] == 0);
	ASSERT(polys->items[0]->items[4] == 100);
	ASSERT(polys->items[0]->items[5] == 0);
	ASSERT(polys->items[0]->items[6] == 100);
	ASSERT(polys->items[0]->items[7] == 100);

	spFloatArray_dispose(polygon);
	spTriangulator_dispose(triangulator);
}

void MemoryTestFixture::skeletonClipper() {
	spSkeletonClipping* clipping = spSkeletonClipping_create();

	spBoneData* boneData = spBoneData_create(0, "bone", 0);
	spBone* bone = spBone_create(boneData, 0, 0);
	CONST_CAST(float, bone->a) = 1;
	CONST_CAST(float, bone->b) = 0;
	CONST_CAST(float, bone->c) = 0;
	CONST_CAST(float, bone->d) = 1;
	CONST_CAST(float, bone->worldX) = 0;
	CONST_CAST(float, bone->worldY) = 0;
	spSlotData* slotData = spSlotData_create(0, "slot", 0);
	spSlot* slot = spSlot_create(slotData, bone);
	spClippingAttachment* clip = spClippingAttachment_create("clipping");
	clip->endSlot = slotData;
	clip->super.worldVerticesLength = 4 * 2;
	clip->super.verticesCount = 4;
	clip->super.vertices = MALLOC(float, 4 * 8);
	clip->super.vertices[0] = 0;
	clip->super.vertices[1] = 50;
	clip->super.vertices[2] = 100;
	clip->super.vertices[3] = 50;
	clip->super.vertices[4] = 100;
	clip->super.vertices[5] = 70;
	clip->super.vertices[6] = 0;
	clip->super.vertices[7] = 70;

	spSkeletonClipping_clipStart(clipping, slot, clip);

	spFloatArray* vertices = spFloatArray_create(16);
	spFloatArray_add(vertices, 0);
	spFloatArray_add(vertices, 0);
	spFloatArray_add(vertices, 100);
	spFloatArray_add(vertices, 0);
	spFloatArray_add(vertices, 50);
	spFloatArray_add(vertices, 150);
	spFloatArray* uvs = spFloatArray_create(16);
	spFloatArray_add(uvs, 0);
	spFloatArray_add(uvs, 0);
	spFloatArray_add(uvs, 1);
	spFloatArray_add(uvs, 0);
	spFloatArray_add(uvs, 0.5f);
	spFloatArray_add(uvs, 1);
	spUnsignedShortArray* indices = spUnsignedShortArray_create(16);
	spUnsignedShortArray_add(indices, 0);
	spUnsignedShortArray_add(indices, 1);
	spUnsignedShortArray_add(indices, 2);

	spSkeletonClipping_clipTriangles(clipping, vertices->items, vertices->size, indices->items, indices->size, uvs->items, 2);

	float expectedVertices[8] = { 83.333328, 50.000000, 76.666664, 70.000000, 23.333334, 70.000000, 16.666672, 50.000000 };
	ASSERT(clipping->clippedVertices->size == 8);
	for (int i = 0; i < clipping->clippedVertices->size; i++) {
		ASSERT(ABS(clipping->clippedVertices->items[i] - expectedVertices[i]) < 0.001);
	}

	float expectedUVs[8] = { 0.833333f, 0.333333, 0.766667, 0.466667, 0.233333, 0.466667, 0.166667, 0.333333 };
	ASSERT(clipping->clippedUVs->size == 8);
	for (int i = 0; i < clipping->clippedUVs->size; i++) {
		ASSERT(ABS(clipping->clippedUVs->items[i] - expectedUVs[i]) < 0.001);
	}

	short expectedIndices[6] = { 0, 1, 2, 0, 2, 3 };
	ASSERT(clipping->clippedTriangles->size == 6);
	for (int i = 0; i < clipping->clippedTriangles->size; i++) {
		ASSERT(clipping->clippedTriangles->items[i] == expectedIndices[i]);
	}

	spFloatArray_dispose(vertices);
	spFloatArray_dispose(uvs);
	spUnsignedShortArray_dispose(indices);

	spSlotData_dispose(slotData);
	spSlot_dispose(slot);
	spBoneData_dispose(boneData);
	spBone_dispose(bone);
	_spClippingAttachment_dispose(SUPER(SUPER(clip)));
	spSkeletonClipping_dispose(clipping);
}


