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

#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

void UTrackEntry::SetTrackEntry(TrackEntry* entry) {
	this->entry = entry;
	if (entry) entry->setRendererObject((void*)this);
}

void callback(AnimationState* state, EventType type, TrackEntry* entry, Event* event) {
	USpineSkeletonAnimationComponent* component = (USpineSkeletonAnimationComponent*)state->getRendererObject();
		
	if (entry->getRendererObject()) {			
		UTrackEntry* uEntry = (UTrackEntry*)entry->getRendererObject();
		if (type == EventType_Start) {
			component->AnimationStart.Broadcast(uEntry);
			uEntry->AnimationStart.Broadcast(uEntry);
		}
		else if (type == EventType_Interrupt) {
			component->AnimationInterrupt.Broadcast(uEntry);
			uEntry->AnimationInterrupt.Broadcast(uEntry);
		} else if (type == EventType_Event) {
			FSpineEvent evt;
			evt.SetEvent(event);
			component->AnimationEvent.Broadcast(uEntry, evt);
			uEntry->AnimationEvent.Broadcast(uEntry, evt);
		}
		else if (type == EventType_Complete) {
			component->AnimationComplete.Broadcast(uEntry);
			uEntry->AnimationComplete.Broadcast(uEntry);
		}
		else if (type == EventType_End) {
			component->AnimationEnd.Broadcast(uEntry);
			uEntry->AnimationEnd.Broadcast(uEntry);
		}
		else if (type == EventType_Dispose) {
			component->AnimationDispose.Broadcast(uEntry);
			uEntry->AnimationDispose.Broadcast(uEntry);
			uEntry->SetTrackEntry(nullptr);
			component->GCTrackEntry(uEntry);
		}
	}	
}

USpineSkeletonAnimationComponent::USpineSkeletonAnimationComponent () {
	PrimaryComponentTick.bCanEverTick = true;
	bTickInEditor = true;
	bAutoActivate = true;
}

void USpineSkeletonAnimationComponent::BeginPlay() {
	Super::BeginPlay();
	trackEntries.Empty();
}

void USpineSkeletonAnimationComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	InternalTick(DeltaTime);
}

void USpineSkeletonAnimationComponent::InternalTick(float DeltaTime, bool CallDelegates) {
	CheckState();

	if (state) {
		state->update(DeltaTime);
		state->apply(*skeleton);
		if (CallDelegates) BeforeUpdateWorldTransform.Broadcast(this);
		skeleton->updateWorldTransform();
		if (CallDelegates) AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonAnimationComponent::CheckState () {
	if (lastAtlas != Atlas || lastData != SkeletonData) {
		DisposeState();
		
		if (Atlas && SkeletonData) {
			spine::SkeletonData *data = SkeletonData->GetSkeletonData(Atlas->GetAtlas(false), false);
			if (data) {
				skeleton = new (__FILE__, __LINE__) Skeleton(data);
				AnimationStateData* stateData = SkeletonData->GetAnimationStateData(Atlas->GetAtlas(false));
				state = new (__FILE__, __LINE__) AnimationState(stateData);
				state->setRendererObject((void*)this);
				state->setListener(callback);
				trackEntries.Empty();
			}
		}
		
		lastAtlas = Atlas;
		lastData = SkeletonData;
	}
}

void USpineSkeletonAnimationComponent::DisposeState () {	
	if (state) {
		delete state;
		state = nullptr;
	}

	if (skeleton) {
		delete skeleton;
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
	if (state) state->setTimeScale(timeScale);
}

float USpineSkeletonAnimationComponent::GetTimeScale() {
	CheckState();
	if (state) return state->getTimeScale();
	return 1;
}

UTrackEntry* USpineSkeletonAnimationComponent::SetAnimation (int trackIndex, FString animationName, bool loop) {
	CheckState();
	if (state && skeleton->getData()->findAnimation(TCHAR_TO_UTF8(*animationName))) {
		state->disableQueue();
		TrackEntry* entry = state->setAnimation(trackIndex, TCHAR_TO_UTF8(*animationName), loop);
		state->enableQueue();
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
	
}

UTrackEntry* USpineSkeletonAnimationComponent::AddAnimation (int trackIndex, FString animationName, bool loop, float delay) {
	CheckState();
	if (state && skeleton->getData()->findAnimation(TCHAR_TO_UTF8(*animationName))) {
		state->disableQueue();
		TrackEntry* entry = state->addAnimation(trackIndex, TCHAR_TO_UTF8(*animationName), loop, delay);		
		state->enableQueue();
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::SetEmptyAnimation (int trackIndex, float mixDuration) {
	CheckState();
	if (state) {
		TrackEntry* entry = state->setEmptyAnimation(trackIndex, mixDuration);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::AddEmptyAnimation (int trackIndex, float mixDuration, float delay) {
	CheckState();
	if (state) {
		TrackEntry* entry = state->addEmptyAnimation(trackIndex, mixDuration, delay);
		UTrackEntry* uEntry = NewObject<UTrackEntry>();
		uEntry->SetTrackEntry(entry);
		trackEntries.Add(uEntry);
		return uEntry;
	} else return NewObject<UTrackEntry>();
}

UTrackEntry* USpineSkeletonAnimationComponent::GetCurrent (int trackIndex) {
	CheckState();
	if (state) {
		TrackEntry* entry = state->getCurrent(trackIndex);
		if (entry->getRendererObject()) {
			return (UTrackEntry*)entry->getRendererObject();
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
		state->clearTracks();
	}
}

void USpineSkeletonAnimationComponent::ClearTrack (int trackIndex) {
	CheckState();
	if (state) {
		state->clearTrack(trackIndex);
	}
}

#undef LOCTEXT_NAMESPACE
