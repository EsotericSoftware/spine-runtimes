#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

USpineSkeletonComponent::USpineSkeletonComponent () {
	bWantsBeginPlay = true;
    PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineSkeletonComponent::BeginPlay() {
	Super::BeginPlay();
}

void USpineSkeletonComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();

		if (Atlas && SkeletonData) {
			spSkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			skeleton = spSkeleton_create(data);
			stateData = spAnimationStateData_create(data);
			state = spAnimationState_create(stateData);
		}

		lastAtlas = Atlas;
		lastData = SkeletonData;
	}

	if (state) {
		spAnimationState_update(state, DeltaTime);
		spAnimationState_apply(state, skeleton);
        spSkeleton_updateWorldTransform(skeleton);
	}
}

void USpineSkeletonComponent::DisposeState () {
	if (stateData) {
		spAnimationStateData_dispose(stateData);
		stateData = nullptr;
	}

	if (state) {
		spAnimationState_dispose(state);
		state = nullptr;
	}

	if (skeleton) {
		spSkeleton_dispose(skeleton);
		skeleton = nullptr;
	}
}

void USpineSkeletonComponent::FinishDestroy () {
	DisposeState();
	Super::FinishDestroy();
}

#undef LOCTEXT_NAMESPACE
