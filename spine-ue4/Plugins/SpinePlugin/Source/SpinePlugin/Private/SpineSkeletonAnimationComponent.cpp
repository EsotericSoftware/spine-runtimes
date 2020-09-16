/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "SpinePluginPrivatePCH.h"

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

void UTrackEntry::SetTrackEntry(TrackEntry* trackEntry) {
	this->entry = trackEntry;
	if (entry) entry->setRendererObject((void*)this);
}

void callback(AnimationState* state, spine::EventType type, TrackEntry* entry, Event* event) {
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
	bAutoPlaying = true;
}

void USpineSkeletonAnimationComponent::BeginPlay() {
	Super::BeginPlay();
	trackEntries.Empty();
}

void USpineSkeletonAnimationComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) {
	Super::Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	InternalTick(DeltaTime, true, TickType == LEVELTICK_ViewportsOnly);
}

void USpineSkeletonAnimationComponent::InternalTick(float DeltaTime, bool CallDelegates, bool Preview) {
	CheckState();

	if (state && bAutoPlaying) {
		if (Preview) {
			if (lastPreviewAnimation != PreviewAnimation) {
				if (PreviewAnimation != "") SetAnimation(0, PreviewAnimation, true);
				else SetEmptyAnimation(0, 0);
				lastPreviewAnimation = PreviewAnimation;
			}

			if (lastPreviewSkin != PreviewSkin) {
				if (PreviewSkin != "") SetSkin(PreviewSkin);
				else SetSkin("default");
				lastPreviewSkin = PreviewSkin;
			}
		}
		state->update(DeltaTime);
		state->apply(*skeleton);
		if (CallDelegates) BeforeUpdateWorldTransform.Broadcast(this);
		skeleton->updateWorldTransform();
		if (CallDelegates) AfterUpdateWorldTransform.Broadcast(this);
	}
}

void USpineSkeletonAnimationComponent::CheckState () {
	bool needsUpdate = lastAtlas != Atlas || lastData != SkeletonData;

	if (!needsUpdate) {
		// Are we doing a re-import? Then check if the underlying spine-cpp data
		// has changed.
		if (lastAtlas && lastAtlas == Atlas && lastData && lastData == SkeletonData) {
			spine::Atlas* atlas = Atlas->GetAtlas();
			if (lastSpineAtlas != atlas) {
				needsUpdate = true;
			}
			if (skeleton && skeleton->getData() != SkeletonData->GetSkeletonData(atlas)) {
				needsUpdate = true;
			}
		}
	}

	if (needsUpdate) {
		DisposeState();

		if (Atlas && SkeletonData) {
			spine::SkeletonData *data = SkeletonData->GetSkeletonData(Atlas->GetAtlas());
			if (data) {
				skeleton = new (__FILE__, __LINE__) Skeleton(data);
				AnimationStateData* stateData = SkeletonData->GetAnimationStateData(Atlas->GetAtlas());
				state = new (__FILE__, __LINE__) AnimationState(stateData);
				state->setRendererObject((void*)this);
				state->setListener(callback);
				trackEntries.Empty();
			}
		}

		lastAtlas = Atlas;
		lastSpineAtlas = Atlas ? Atlas->GetAtlas() : nullptr;
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

void USpineSkeletonAnimationComponent::SetAutoPlay(bool bInAutoPlays)
{
	bAutoPlaying = bInAutoPlays;
}

void USpineSkeletonAnimationComponent::SetPlaybackTime(float InPlaybackTime, bool bCallDelegates)
{
	CheckState();

	if (state && state->getCurrent(0)) {
		spine::Animation* CurrentAnimation = state->getCurrent(0)->getAnimation();
		const float CurrentTime = state->getCurrent(0)->getTrackTime();
		InPlaybackTime = FMath::Clamp(InPlaybackTime, 0.0f, CurrentAnimation->getDuration());
		const float DeltaTime = InPlaybackTime - CurrentTime;
		state->update(DeltaTime);
		state->apply(*skeleton);

		//Call delegates and perform the world transform
		if (bCallDelegates)
		{
			BeforeUpdateWorldTransform.Broadcast(this);
		}
		skeleton->updateWorldTransform();
		if (bCallDelegates)
		{
			AfterUpdateWorldTransform.Broadcast(this);
		}
	}
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
	if (state && state->getCurrent(trackIndex)) {
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
