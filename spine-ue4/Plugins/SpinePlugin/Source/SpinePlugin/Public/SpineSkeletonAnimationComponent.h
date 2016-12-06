// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "SpineSkeletonComponent.h"
#include "spine/spine.h"
#include "SpineSkeletonAnimationComponent.generated.h"

UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent), BlueprintType)
class SPINEPLUGIN_API UTrackEntry: public UObject {
	GENERATED_BODY ()

public:
	
	UTrackEntry () { }
	
	spTrackEntry* entry = nullptr;
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	int GetTrackIndex() { return entry ? entry->trackIndex : 0; }
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAnimationStartEvent, UTrackEntry*, entry);

class USpineAtlasAsset;
UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonAnimationComponent: public USpineSkeletonComponent {
	GENERATED_BODY()

public:
	spAnimationStateData* GetAnimationStateData () { return stateData; };
	
	spAnimationState* GetAnimationState () { return state; };
		
	USpineSkeletonAnimationComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	virtual void FinishDestroy () override;
	
	// Blueprint functions
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* SetAnimation (int trackIndex, FString animationName, bool loop);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* AddAnimation (int trackIndex, FString animationName, bool loop, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* SetEmptyAnimation (int trackIndex, float mixDuration);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	UTrackEntry* AddEmptyAnimation (int trackIndex, float mixDuration, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTracks ();
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTrack (int trackIndex);
	
	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAnimationStartEvent AnimationStartEvent;
	
protected:
	virtual void CheckState () override;
	virtual void DisposeState () override;

	spAnimationStateData* stateData;
	spAnimationState* state;		
};
