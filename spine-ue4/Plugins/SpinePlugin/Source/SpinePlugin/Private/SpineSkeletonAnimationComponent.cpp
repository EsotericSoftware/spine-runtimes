#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

void UTrackEntry::SetTrackEntry(spTrackEntry* entry) {
	this->entry = entry;
	if (entry) entry->rendererObject = (void*)this;
}

void callback(spAnimationState* state, spEventType type, spTrackEntry* entry, spEvent* event) {
	USpineSkeletonAnimationComponent* component = (USpineSkeletonAnimationComponent*)state->rendererObject;
		
	if (entry->rendererObject) {			
		UTrackEntry* uEntry = (UTrackEntry*)entry->rendererObject;
		if (type == SP_ANIMATION_START) {
			component->AnimationStart.Broadcast(uEntry);
			uEntry->AnimationStart.Broadcast(uEntry);
		}
		else if (type == SP_ANIMATION_INTERRUPT) {
			component->AnimationInterrupt.Broadcast(uEntry);
			uEntry->AnimationInterrupt.Broadcast(uEntry);
		} else if (type == SP_ANIMATION_EVENT) {
			FSpineEvent evt;
			evt.SetEvent(event);
			component->AnimationEvent.Broadcast(uEntry, evt);
			uEntry->AnimationEvent.Broadcast(uEntry, evt);
		}
		else if (type == SP_ANIMATION_COMPLETE) {
			component->AnimationComplete.Broadcast(uEntry);
			uEntry->AnimationComplete.Broadcast(uEntry);
		}
		else if (type == SP_ANIMATION_END) {
			component->AnimationEnd.Broadcast(uEntry);
			uEntry->AnimationEnd.Broadcast(uEntry);
		}
		else if (type == SP_ANIMATION_DISPOSE) {
			component->AnimationDispose.Broadcast(uEntry);
			uEntry->AnimationDispose.Broadcast(uEntry);
			uEntry->SetTrackEntry(nullptr);
			component->GCTrackEntry(uEntry);
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
	trackEntries.Empty();
}

void USpineSkeletonAnimationComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	CheckState();

	if (state) {
		spAnimationState_update(state, DeltaTime);
		spAnimationState_apply(state, skeleton);
		BeforeUpdateWorldTransform.Broadcast(this);
		spSkeleton_updateWorldTransform(skeleton);
		AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonAnimationComponent::InternalTick(float DeltaTime) {
	CheckState();

	if (state) {
		spAnimationState_update(state, DeltaTime);
		spAnimationState_apply(state, skeleton);
		BeforeUpdateWorldTransform.Broadcast(this);
		spSkeleton_updateWorldTransform(skeleton);
		AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonAnimationComponent::CheckState () {
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();
		
		if (Atlas && SkeletonData) {
			spSkeletonData* data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			skeleton = spSkeleton_create(data);
			spAnimationStateData* stateData = SkeletonData->GetAnimationStateData(Atlas->GetAtlas(false));
			state = spAnimationState_create(stateData);
			state->rendererObject = (void*)this;
			state->listener = callback;
			trackEntries.Empty();
		}
		
		lastAtlas = Atlas;
		lastData = SkeletonData;
	}
}

void USpineSkeletonAnimationComponent::DisposeState () {	
	if (state) {
		spAnimationState_dispose(state);
		state = nullptr;
	}

	if (skeleton) {
		spSkeleton_dispose(skeleton);
		skeleton = nullptr;
	}

	trackEntries.Empty();
}

void USpineSkeletonAnimationComponent::FinishDestroy () {
	DisposeState();
	Super::FinishDestroy();
}

void USpineSkeletonAnimationComponent::SetTimeScale(float timeScale) {
	CheckState();
	if (state) state->timeScale = timeScale;	
}

float USpineSkeletonAnimationComponent::GetTimeScale() {
	CheckState();
	if (state) return state->timeScale;
	return 1;
}

UTrackEntry* USpineSkeletonAnimationComponent::SetAnimation (int trackIndex, FString animationName, bool loop) {
	CheckState();
	if (state && spSkeletonData_findAnimation(skeleton->data, TCHAR_TO_UTF8(*animationName))) {
		spTrackEntry* entry = spAnimationState_setAnimationByName(state, trackIndex, TCHAR_TO_UTF8(*animationName), loop ? 1 : 0);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
	
}

UTrackEntry* USpineSkeletonAnimationComponent::AddAnimation (int trackIndex, FString animationName, bool loop, float delay) {
	CheckState();
	if (state && spSkeletonData_findAnimation(skeleton->data, TCHAR_TO_UTF8(*animationName))) {
		spTrackEntry* entry = spAnimationState_addAnimationByName(state, trackIndex, TCHAR_TO_UTF8(*animationName), loop ? 1 : 0, delay);		
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::SetEmptyAnimation (int trackIndex, float mixDuration) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_setEmptyAnimation(state, trackIndex, mixDuration);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::AddEmptyAnimation (int trackIndex, float mixDuration, float delay) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_addEmptyAnimation(state, trackIndex, mixDuration, delay);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::GetCurrent (int trackIndex) {
	CheckState();
	if (state) {
		spTrackEntry* entry = spAnimationState_getCurrent(state, trackIndex);
		if (entry->rendererObject) {
			return (UTrackEntry*)entry->rendererObject;
		} else {
			UTrackEntry* uEntry = NewObject<UTrackEntry>();
			uEntry->SetTrackEntry(entry);
			trackEntries.Add(uEntry);
			return uEntry;
		}
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
