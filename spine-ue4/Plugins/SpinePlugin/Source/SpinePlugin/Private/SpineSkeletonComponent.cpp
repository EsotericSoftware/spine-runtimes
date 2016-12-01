#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

void callback(spAnimationState* state, spEventType type, spTrackEntry* entry, spEvent* event) {
	USpineSkeletonComponent* component = (USpineSkeletonComponent*)entry->rendererObject;
}

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
	
	CheckState();

	if (state) {
		spAnimationState_update(state, DeltaTime);
		spAnimationState_apply(state, skeleton);
		spSkeleton_updateWorldTransform(skeleton);
	}
}

void USpineSkeletonComponent::CheckState () {
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

FTrackEntry USpineSkeletonComponent::SetAnimation (int trackIndex, FString animationName, bool loop) {
	CheckState();
	if (state && spSkeletonData_findAnimation(skeleton->data, TCHAR_TO_UTF8(*animationName))) {
		spTrackEntry* entry = spAnimationState_setAnimationByName(state, trackIndex, TCHAR_TO_UTF8(*animationName), loop ? 1 : 0);
		return FTrackEntry(entry);
	} else return FTrackEntry();
	
}

FTrackEntry USpineSkeletonComponent::AddAnimation (int trackIndex, FString animationName, bool loop, float delay) {
	CheckState();
	if (state && spSkeletonData_findAnimation(skeleton->data, TCHAR_TO_UTF8(*animationName))) {
		spTrackEntry* entry = spAnimationState_addAnimationByName(state, trackIndex, TCHAR_TO_UTF8(*animationName), loop ? 1 : 0, delay);		
		return FTrackEntry(entry);
	} else return FTrackEntry();
}

FTrackEntry USpineSkeletonComponent::SetEmptyAnimation (int trackIndex, float mixDuration) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_setEmptyAnimation(state, trackIndex, mixDuration);
		return FTrackEntry(entry);
	} else return FTrackEntry();
}

FTrackEntry USpineSkeletonComponent::AddEmptyAnimation (int trackIndex, float mixDuration, float delay) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_addEmptyAnimation(state, trackIndex, mixDuration, delay);
		return FTrackEntry(entry);
	} else return FTrackEntry();
}

void USpineSkeletonComponent::ClearTracks () {
	CheckState();
	if (state) {
		spAnimationState_clearTracks(state);
	}
}

void USpineSkeletonComponent::ClearTrack (int trackIndex) {
	CheckState();
	if (state) {
		spAnimationState_clearTrack(state, trackIndex);
	}
}

#undef LOCTEXT_NAMESPACE
