/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

#pragma once

// clang-format off
#include "Components/ActorComponent.h"
#include "SpineSkeletonComponent.h"
#include "spine/spine.h"
#include "SpineSkeletonAnimationComponent.generated.h"
// clang-format on

USTRUCT(BlueprintType, Category = "Spine")
struct SPINEPLUGIN_API FSpineEvent {
	GENERATED_BODY();

public:
	void SetEvent(spine::Event *event) {
		Name = FString(UTF8_TO_TCHAR(event->getData().getName().buffer()));
		if (!event->getStringValue().isEmpty()) {
			StringValue = FString(UTF8_TO_TCHAR(event->getStringValue().buffer()));
		}
		this->IntValue = event->getIntValue();
		this->FloatValue = event->getFloatValue();
		this->Time = event->getTime();
	}

	UPROPERTY(BlueprintReadonly)
	FString Name;

	UPROPERTY(BlueprintReadOnly)
	FString StringValue;

	UPROPERTY(BlueprintReadOnly)
	int IntValue;

	UPROPERTY(BlueprintReadOnly)
	float FloatValue;

	UPROPERTY(BlueprintReadOnly)
	float Time;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationStartDelegate, UTrackEntry *, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FSpineAnimationEventDelegate, UTrackEntry *, entry, FSpineEvent, evt);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationInterruptDelegate, UTrackEntry *, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationCompleteDelegate, UTrackEntry *, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationEndDelegate, UTrackEntry *, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationDisposeDelegate, UTrackEntry *, entry);

UCLASS(ClassGroup = (Spine), meta = (BlueprintSpawnableComponent), BlueprintType)
class SPINEPLUGIN_API UTrackEntry : public UObject {
	GENERATED_BODY()

public:
	UTrackEntry() {}

	void SetTrackEntry(spine::TrackEntry *trackEntry);
	spine::TrackEntry *GetTrackEntry() { return entry; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	int GetTrackIndex() { return entry ? entry->getTrackIndex() : 0; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	bool GetLoop() { return entry ? entry->getLoop() : false; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetLoop(bool loop) {
		if (entry) entry->setLoop(loop);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetEventThreshold() { return entry ? entry->getEventThreshold() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetEventThreshold(float eventThreshold) {
		if (entry) entry->setEventThreshold(eventThreshold);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetAttachmentThreshold() { return entry ? entry->getAttachmentThreshold() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetAttachmentThreshold(float attachmentThreshold) {
		if (entry) entry->setAttachmentThreshold(attachmentThreshold);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetDrawOrderThreshold() { return entry ? entry->getDrawOrderThreshold() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetDrawOrderThreshold(float drawOrderThreshold) {
		if (entry) entry->setDrawOrderThreshold(drawOrderThreshold);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetAnimationStart() { return entry ? entry->getAnimationStart() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetAnimationStart(float animationStart) {
		if (entry) entry->setAnimationStart(animationStart);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetAnimationEnd() { return entry ? entry->getAnimationEnd() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetAnimationEnd(float animationEnd) {
		if (entry) entry->setAnimationEnd(animationEnd);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetAnimationLast() { return entry ? entry->getAnimationLast() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetAnimationLast(float animationLast) {
		if (entry) entry->setAnimationLast(animationLast);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetDelay() { return entry ? entry->getDelay() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetDelay(float delay) {
		if (entry) entry->setDelay(delay);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetTrackTime() { return entry ? entry->getTrackTime() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetTrackTime(float trackTime) {
		if (entry) entry->setTrackTime(trackTime);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetTrackEnd() { return entry ? entry->getTrackEnd() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetTrackEnd(float trackEnd) {
		if (entry) entry->setTrackEnd(trackEnd);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetTimeScale() { return entry ? entry->getTimeScale() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetTimeScale(float timeScale) {
		if (entry) entry->setTimeScale(timeScale);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetAlpha() { return entry ? entry->getAlpha() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetAlpha(float alpha) {
		if (entry) entry->setAlpha(alpha);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetMixTime() { return entry ? entry->getMixTime() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetMixTime(float mixTime) {
		if (entry) entry->setMixTime(mixTime);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float GetMixDuration() { return entry ? entry->getMixDuration() : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	void SetMixDuration(float mixDuration) {
		if (entry) entry->setMixDuration(mixDuration);
	}

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	FString getAnimationName() { return entry ? entry->getAnimation()->getName().buffer() : ""; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	float getAnimationDuration() { return entry ? entry->getAnimation()->getDuration() : 0; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|TrackEntry")
	bool isValidAnimation() { return entry != nullptr; }

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|TrackEntry")
	FSpineAnimationStartDelegate AnimationStart;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|TrackEntry")
	FSpineAnimationInterruptDelegate AnimationInterrupt;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|TrackEntry")
	FSpineAnimationEventDelegate AnimationEvent;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|TrackEntry")
	FSpineAnimationCompleteDelegate AnimationComplete;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|TrackEntry")
	FSpineAnimationEndDelegate AnimationEnd;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|TrackEntry")
	FSpineAnimationDisposeDelegate AnimationDispose;

protected:
	spine::TrackEntry *entry = nullptr;
};

class USpineAtlasAsset;
UCLASS(ClassGroup = (Spine), meta = (BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonAnimationComponent : public USpineSkeletonComponent {
	GENERATED_BODY()

public:
	spine::AnimationState *GetAnimationState() { return state; };

	USpineSkeletonAnimationComponent();

	virtual void BeginPlay() override;

	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction *ThisTickFunction) override;

	virtual void FinishDestroy() override;

	//Added functions for manual configuration

	/* Manages if this skeleton should update automatically or is paused. */
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	void SetAutoPlay(bool bInAutoPlays);

	/* Directly set the time of the current animation, will clamp to animation range. */
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	void SetPlaybackTime(float InPlaybackTime, bool bCallDelegates = true);

	// Blueprint functions
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	void SetTimeScale(float timeScale);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	float GetTimeScale();

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	UTrackEntry *SetAnimation(int trackIndex, FString animationName, bool loop);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	UTrackEntry *AddAnimation(int trackIndex, FString animationName, bool loop, float delay);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	UTrackEntry *SetEmptyAnimation(int trackIndex, float mixDuration);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	UTrackEntry *AddEmptyAnimation(int trackIndex, float mixDuration, float delay);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	UTrackEntry *GetCurrent(int trackIndex);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	void ClearTracks();

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Animation")
	void ClearTrack(int trackIndex);

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Animation")
	FSpineAnimationStartDelegate AnimationStart;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Animation")
	FSpineAnimationInterruptDelegate AnimationInterrupt;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Animation")
	FSpineAnimationEventDelegate AnimationEvent;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Animation")
	FSpineAnimationCompleteDelegate AnimationComplete;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Animation")
	FSpineAnimationEndDelegate AnimationEnd;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Animation")
	FSpineAnimationDisposeDelegate AnimationDispose;

	UPROPERTY(EditAnywhere, Category = Spine)
	FString PreviewAnimation;

	UPROPERTY(EditAnywhere, Category = Spine)
	FString PreviewSkin;

	// used in C event callback. Needs to be public as we can't call
	// protected methods from plain old C function.
	void GCTrackEntry(UTrackEntry *entry) { trackEntries.Remove(entry); }

protected:
	virtual void CheckState() override;
	virtual void InternalTick(float DeltaTime, bool CallDelegates = true, bool Preview = false) override;
	virtual void DisposeState() override;

	spine::AnimationState *state;

	// keep track of track entries so they won't get GCed while
	// in transit within a blueprint
	UPROPERTY()
	TSet<UTrackEntry *> trackEntries;

private:
	/* If the animation should update automatically. */
	UPROPERTY()
	bool bAutoPlaying;

	FString lastPreviewAnimation;
	FString lastPreviewSkin;
};
