// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "spine/spine.h"
#include "SpineSkeletonComponent.generated.h"

USTRUCT(BlueprintType)
struct SPINEPLUGIN_API FTrackEntry {
	GENERATED_BODY ();
	
	FTrackEntry (): entry(0) { }
	
	FTrackEntry (spTrackEntry* entry) { this->entry = entry; }
	
	spTrackEntry* entry;
};

class USpineAtlasAsset;
UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonComponent: public UActorComponent {
	GENERATED_BODY()

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Spine)
	USpineAtlasAsset* Atlas;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Spine)
	USpineSkeletonDataAsset* SkeletonData;
	
	spAnimationStateData* GetAnimationStateData () { return stateData; };
	
	spAnimationState* GetAnimationState () { return state; };
	
	spSkeleton* GetSkeleton () { return skeleton; };
		
	USpineSkeletonComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	virtual void FinishDestroy () override;
	
	// Blueprint functions
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	FTrackEntry SetAnimation (int trackIndex, FString animationName, bool loop);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	FTrackEntry AddAnimation (int trackIndex, FString animationName, bool loop, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	FTrackEntry SetEmptyAnimation (int trackIndex, float mixDuration);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	FTrackEntry AddEmptyAnimation (int trackIndex, float mixDuration, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTracks ();
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTrack (int trackIndex);
	
	UFUNCTION(BlueprintImplentableEvent, category = "Components|Spine")
	void AnimationEvent(int trackIndex, );
	
protected:
	void CheckState ();
	void DisposeState ();

	spAnimationStateData* stateData;
	spAnimationState* state;
	spSkeleton* skeleton;
	USpineAtlasAsset* lastAtlas = nullptr;
	USpineSkeletonDataAsset* lastData = nullptr;	
};
