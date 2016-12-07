#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

void callback(spAnimationState* state, spEventType type, spTrackEntry* entry, spEvent* event) {
	USpineSkeletonAnimationComponent* component = (USpineSkeletonAnimationComponent*)state->rendererObject;
	
	if (type == SP_ANIMATION_START) {		
		if (entry->rendererObject) {			
			UTrackEntry* uEntry = (UTrackEntry*)entry->rendererObject;
			component->AnimationStartEvent.Broadcast(uEntry);
			uEntry->AnimationStartEvent.Broadcast(uEntry);
		}
	}
}

USpineSkeletonAnimationComponent::USpineSkeletonAnimationComponent () {
	bWantsBeginPlay = true;
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineSkeletonAnimationComponent::BeginPlay() {
	Super::BeginPlay();
}

void USpineSkeletonAnimationComponent::TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	
	CheckState();

	if (state) {
		spAnimationState_update(state, DeltaTime);
		spAnimationState_apply(state, skeleton);
		spSkeleton_updateWorldTransform(skeleton);
	}
}

void USpineSkeletonAnimationComponent::CheckState () {
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();
		
		if (Atlas && SkeletonData) {
			spSkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			skeleton = spSkeleton_create(data);
			stateData = spAnimationStateData_create(data);
			state = spAnimationState_create(stateData);
			state->rendererObject = (void*)this;
			state->listener = callback;
		}
		
		lastAtlas = Atlas;
		lastData = SkeletonData;
	}
}

void USpineSkeletonAnimationComponent::DisposeState () {
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

void USpineSkeletonAnimationComponent::FinishDestroy () {
	DisposeState();
	Super::FinishDestroy();
}

UTrackEntry* USpineSkeletonAnimationComponent::SetAnimation (int trackIndex, FString animationName, bool loop) {
	CheckState();
	if (state && spSkeletonData_findAnimation(skeleton->data, TCHAR_TO_UTF8(*animationName))) {
		spTrackEntry* entry = spAnimationState_setAnimationByName(state, trackIndex, TCHAR_TO_UTF8(*animationName), loop ? 1 : 0);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
	
}

UTrackEntry* USpineSkeletonAnimationComponent::AddAnimation (int trackIndex, FString animationName, bool loop, float delay) {
	CheckState();
	if (state && spSkeletonData_findAnimation(skeleton->data, TCHAR_TO_UTF8(*animationName))) {
		spTrackEntry* entry = spAnimationState_addAnimationByName(state, trackIndex, TCHAR_TO_UTF8(*animationName), loop ? 1 : 0, delay);		
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::SetEmptyAnimation (int trackIndex, float mixDuration) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_setEmptyAnimation(state, trackIndex, mixDuration);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::AddEmptyAnimation (int trackIndex, float mixDuration, float delay) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_addEmptyAnimation(state, trackIndex, mixDuration, delay);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

void USpineSkeletonAnimationComponent::ClearTracks () {
	CheckState();
	if (state) {
		spAnimationState_clearTracks(state);
	}
}

void USpineSkeletonAnimationComponent::ClearTrack (int trackIndex) {
	CheckState();
	if (state) {
		spAnimationState_clearTrack(state, trackIndex);
	}
}

#undef LOCTEXT_NAMESPACE
