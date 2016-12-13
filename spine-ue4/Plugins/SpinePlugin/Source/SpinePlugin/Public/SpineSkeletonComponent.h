// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "Components/ActorComponent.h"
#include "spine/spine.h"
#include "SpineSkeletonComponent.generated.h"

class USpineSkeletonComponent;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineBeforeUpdateWorldTransformDelegate, USpineSkeletonComponent*, skeleton);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FSpineAfterUpdateWorldTransformDelegate, USpineSkeletonComponent*, skeleton);

class USpineAtlasAsset;
UCLASS(ClassGroup=(Spine), meta=(BlueprintSpawnableComponent))
class SPINEPLUGIN_API USpineSkeletonComponent: public UActorComponent {
	GENERATED_BODY()

public:
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Spine)
	USpineAtlasAsset* Atlas;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Spine)
	USpineSkeletonDataAsset* SkeletonData;
	
	spSkeleton* GetSkeleton () { return skeleton; };
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	bool SetSkin (const FString& SkinName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	bool setAttachment (const FString& slotName, const FString& attachmentName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	FTransform GetBoneWorldTransform (const FString& BoneName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	FTransform GetBoneLocalTransform (const FString& BoneName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetToSetupPose ();
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetBonesToSetupPose ();
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine")
	void SetSlotsToSetupPose ();
	
	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineBeforeUpdateWorldTransformDelegate BeforeUpdateWorldTransform;
	
	UPROPERTY(BlueprintAssignable, Category = "Components|Spine")
	FSpineAfterUpdateWorldTransformDelegate AfterUpdateWorldTransform;
		
	USpineSkeletonComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;	

	virtual void FinishDestroy () override;
	
protected:
	virtual void CheckState ();
	virtual void InternalTick(float DeltaTime);
	virtual void DisposeState ();

	spSkeleton* skeleton;
	USpineAtlasAsset* lastAtlas = nullptr;
	USpineSkeletonDataAsset* lastData = nullptr;	
};
