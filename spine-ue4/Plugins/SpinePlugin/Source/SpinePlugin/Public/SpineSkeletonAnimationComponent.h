// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "SpineSkeletonComponent.h"
#include "spine/spine.h"
#include "SpineSkeletonAnimationComponent.generated.h"

USTRUCT(BlueprintType, Category="Spine")
struct SPINEPLUGIN_API FSpineEvent {
	GENERATED_BODY();

public:
	void SetEvent(spEvent* event) {
		Name = FString(UTF8_TO_TCHAR(event->data->name));
		if (event->stringValue) {			
			StringValue = FString(UTF8_TO_TCHAR(event->stringValue));
		}
		this->IntValue = event->intValue;
		this->FloatValue = event->floatValue;
		this->Time = event->time;
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

	void SetTrackEntry (spTrackEntry* entry);
	spTrackEntry* GetTrackEntry();
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	int GetTrackIndex () { return entry ? entry->trackIndex : 0; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	bool GetLoop () { return entry ? entry->loop != 0 : false; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetLoop (bool loop) { if (entry) entry->loop = loop ? 1 : 0; }
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetEventThreshold () { return entry ? entry->eventThreshold : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetEventThreshold(float eventThreshold) { if (entry) entry->eventThreshold = eventThreshold; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetAttachmentThreshold() { return entry ? entry->attachmentThreshold : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetAttachmentThreshold(float attachmentThreshold) { if (entry) entry->attachmentThreshold = attachmentThreshold; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetDrawOrderThreshold() { return entry ? entry->drawOrderThreshold : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetDrawOrderThreshold(float drawOrderThreshold) { if (entry) entry->drawOrderThreshold = drawOrderThreshold; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetAnimationStart() { return entry ? entry->animationStart : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetAnimationStart(float animationStart) { if (entry) entry->animationStart = animationStart; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetAnimationEnd() { return entry ? entry->animationEnd : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetAnimationEnd(float animationEnd) { if (entry) entry->animationEnd = animationEnd; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetAnimationLast() { return entry ? entry->animationLast : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetAnimationLast(float animationLast) { if (entry) entry->animationLast = animationLast; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetNextAnimationLast() { return entry ? entry->nextAnimationLast : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetNextAnimationLast(float nextAnimationLast) { if (entry) entry->nextAnimationLast = nextAnimationLast; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetDelay() { return entry ? entry->delay : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetDelay(float delay) { if (entry) entry->delay = delay; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetTrackTime() { return entry ? entry->trackTime : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetTrackTime(float trackTime) { if (entry) entry->trackTime = trackTime; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetTrackLast() { return entry ? entry->trackLast : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetTrackLast(float trackLast) { if (entry) entry->trackLast = trackLast; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetNextTrackLast() { return entry ? entry->nextTrackLast : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetNextTrackLast(float nextTrackLast) { if (entry) entry->nextTrackLast = nextTrackLast; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetTrackEnd() { return entry ? entry->trackEnd : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetTrackEnd(float trackEnd) { if (entry) entry->trackEnd = trackEnd; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetTimeScale() { return entry ? entry->timeScale : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetTimeScale(float timeScale) { if (entry) entry->timeScale = timeScale; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetAlpha() { return entry ? entry->alpha : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetAlpha(float alpha) { if (entry) entry->alpha = alpha; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetMixTime() { return entry ? entry->mixTime : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetMixTime(float mixTime) { if (entry) entry->mixTime = mixTime; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetMixDuration() { return entry ? entry->mixDuration : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetMixDuration(float mixDuration) { if (entry) entry->mixDuration = mixDuration; }

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetMixAlpha() { return entry ? entry->mixAlpha : 0; }
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetMixAlpha(float mixAlpha) { if (entry) entry->mixAlpha = mixAlpha; }	

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationStartDelegate AnimationStart;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationInterruptDelegate AnimationInterrupt;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationEventDelegate AnimationEvent;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationCompleteDelegate AnimationComplete;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationEndDelegate AnimationEnd;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationDisposeDelegate AnimationDispose;

protected:
	spTrackEntry* entry = nullptr;
};

class USpineAtlasAsset;
UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonAnimationComponent: public USpineSkeletonComponent {
	GENERATED_BODY()

public:
	spAnimationState* GetAnimationState () { return state; };
		
	USpineSkeletonAnimationComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	virtual void FinishDestroy () override;
	
	// Blueprint functions
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetTimeScale(float timeScale);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	float GetTimeScale();

	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* SetAnimation (int trackIndex, FString animationName, bool loop);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* AddAnimation (int trackIndex, FString animationName, bool loop, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* SetEmptyAnimation (int trackIndex, float mixDuration);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* AddEmptyAnimation (int trackIndex, float mixDuration, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* GetCurrent (int trackIndex);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTracks ();
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTrack (int trackIndex);
	
	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationStartDelegate AnimationStart;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationInterruptDelegate AnimationInterrupt;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationEventDelegate AnimationEvent;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationCompleteDelegate AnimationComplete;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationEndDelegate AnimationEnd;

	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationDisposeDelegate AnimationDispose;
	
	// used in C event callback. Needs to be public as we can't call
	// protected methods from plain old C function.
	void GCTrackEntry(UTrackEntry* entry) { trackEntries.Remove(entry); }
protected:
	virtual void CheckState () override;
	virtual void InternalTick(float DeltaTime) override;
	virtual void DisposeState () override;
	
	spAnimationState* state;

	// keep track of track entries so they won't get GCed while
	// in transit within a blueprint
	UPROPERTY()
	TSet<UTrackEntry*> trackEntries;
};
