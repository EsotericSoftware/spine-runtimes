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

#pragma once

#include "Components/ActorComponent.h"
#include "SpineSkeletonComponent.h"
#include "spine/spine.h"
#include "SpineSkeletonAnimationComponent.generated.h"

USTRUCT(BlueprintType, Category="Spine")
struct SPINEPLUGIN_API FSpineEvent {
	GENERATED_BODY();

public:
	void SetEvent(spine::Event* event) {
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

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationStartDelegate, UTrackEntry*, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FSpineAnimationEventDelegate, UTrackEntry*, entry, FSpineEvent, evt);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationInterruptDelegate, UTrackEntry*, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationCompleteDelegate, UTrackEntry*, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationEndDelegate, UTrackEntry*, entry);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationDisposeDelegate, UTrackEntry*, entry);

UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent), BlueprintType)
class SPINEPLUGIN_API UTrackEntry: public UObject {
	GENERATED_BODY ()

public:
	
	UTrackEntry () { }		

	void SetTrackEntry (spine::TrackEntry* entry);
	spine::TrackEntry* GetTrackEntry() { return entry; }
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	int GetTrackIndex () { return entry ? entry->getTrackIndex() : 0; }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	bool GetLoop () { return entry ? entry->getLoop() : false; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
		void SetLoop(bool loop) { if (entry) entry->setLoop(loop); }
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetEventThreshold () { return entry ? entry->getEventThreshold() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetEventThreshold(float eventThreshold) { if (entry) entry->setEventThreshold(eventThreshold); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetAttachmentThreshold() { return entry ? entry->getAttachmentThreshold() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetAttachmentThreshold(float attachmentThreshold) { if (entry) entry->setAttachmentThreshold(attachmentThreshold); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetDrawOrderThreshold() { return entry ? entry->getDrawOrderThreshold() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetDrawOrderThreshold(float drawOrderThreshold) { if (entry) entry->setDrawOrderThreshold(drawOrderThreshold); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetAnimationStart() { return entry ? entry->getAnimationStart() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetAnimationStart(float animationStart) { if (entry) entry->setAnimationStart(animationStart); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetAnimationEnd() { return entry ? entry->getAnimationEnd() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetAnimationEnd(float animationEnd) { if (entry) entry->setAnimationEnd(animationEnd); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetAnimationLast() { return entry ? entry->getAnimationLast() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetAnimationLast(float animationLast) { if (entry) entry->setAnimationLast(animationLast); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetDelay() { return entry ? entry->getDelay() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetDelay(float delay) { if (entry) entry->setDelay(delay); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetTrackTime() { return entry ? entry->getTrackTime() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetTrackTime(float trackTime) { if (entry) entry->setTrackTime(trackTime); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetTrackEnd() { return entry ? entry->getTrackEnd() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetTrackEnd(float trackEnd) { if (entry) entry->setTrackEnd(trackEnd); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetTimeScale() { return entry ? entry->getTimeScale() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetTimeScale(float timeScale) { if (entry) entry->setTimeScale(timeScale); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetAlpha() { return entry ? entry->getAlpha() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetAlpha(float alpha) { if (entry) entry->setAlpha(alpha); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetMixTime() { return entry ? entry->getMixTime() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetMixTime(float mixTime) { if (entry) entry->setMixTime(mixTime); }

	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	float GetMixDuration() { return entry ? entry->getMixDuration() : 0; }
	UFUNCTION(BlueprintCallable, Category="Components|Spine|TrackEntry")
	void SetMixDuration(float mixDuration) { if (entry) entry->setMixDuration(mixDuration); }

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
	spine::TrackEntry* entry = nullptr;
};

class USpineAtlasAsset;
UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonAnimationComponent: public USpineSkeletonComponent {
	GENERATED_BODY()

public:
	spine::AnimationState* GetAnimationState () { return state; };
		
	USpineSkeletonAnimationComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	virtual void FinishDestroy () override;
	
	// Blueprint functions
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	void SetTimeScale(float timeScale);

	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	float GetTimeScale();

	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	UTrackEntry* SetAnimation (int trackIndex, FString animationName, bool loop);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	UTrackEntry* AddAnimation (int trackIndex, FString animationName, bool loop, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	UTrackEntry* SetEmptyAnimation (int trackIndex, float mixDuration);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	UTrackEntry* AddEmptyAnimation (int trackIndex, float mixDuration, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	UTrackEntry* GetCurrent (int trackIndex);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	void ClearTracks ();
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine|Animation")
	void ClearTrack (int trackIndex);
	
	UPROPERTY(BlueprintAssignable, Category="Components|Spine|Animation")
	FSpineAnimationStartDelegate AnimationStart;

	UPROPERTY(BlueprintAssignable, Category="Components|Spine|Animation")
	FSpineAnimationInterruptDelegate AnimationInterrupt;

	UPROPERTY(BlueprintAssignable, Category="Components|Spine|Animation")
	FSpineAnimationEventDelegate AnimationEvent;

	UPROPERTY(BlueprintAssignable, Category="Components|Spine|Animation")
	FSpineAnimationCompleteDelegate AnimationComplete;

	UPROPERTY(BlueprintAssignable, Category="Components|Spine|Animation")
	FSpineAnimationEndDelegate AnimationEnd;

	UPROPERTY(BlueprintAssignable, Category="Components|Spine|Animation")
	FSpineAnimationDisposeDelegate AnimationDispose;
	
	// used in C event callback. Needs to be public as we can't call
	// protected methods from plain old C function.
	void GCTrackEntry(UTrackEntry* entry) { trackEntries.Remove(entry); }
protected:
	virtual void CheckState () override;
	virtual void InternalTick(float DeltaTime, bool CallDelegates = true) override;
	virtual void DisposeState () override;
	
	spine::AnimationState* state;

	// keep track of track entries so they won't get GCed while
	// in transit within a blueprint
	UPROPERTY()
	TSet<UTrackEntry*> trackEntries;
};
