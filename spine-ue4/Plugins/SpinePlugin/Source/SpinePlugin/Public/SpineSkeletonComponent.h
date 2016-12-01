// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "SpineSkeletonComponent.generated.h"

class USpineAtlasAsset;
UCLASS( ClassGroup=(Spine), meta=(BlueprintSpawnableComponent) )
class SPINEPLUGIN_API USpineSkeletonComponent : public UActorComponent
{
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
	void SetAnimation (int trackIndex, FString animationName, bool loop);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void AddAnimation (int trackIndex, FString animationName, bool loop, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void SetEmptyAnimation (int trackIndex, float mixDuration);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void AddEmptyAnimation (int trackIndex, float mixDuration, float delay);
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTracks ();
	
	UFUNCTION(BlueprintCallable, Category="Components|Spine")
	void ClearTrack (int trackIndex);
	
protected:
	void DisposeState();

	spAnimationStateData* stateData;
	spAnimationState* state;
	spSkeleton* skeleton;
	USpineAtlasAsset* lastAtlas = nullptr;
	USpineSkeletonDataAsset* lastData = nullptr;	
};
