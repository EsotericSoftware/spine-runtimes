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

#pragma once

#include "Components/ActorComponent.h"
#include "SpineSkeletonDataAsset.h"
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
	
	spine::Skeleton* GetSkeleton () { return skeleton; };

	UFUNCTION(BlueprintPure, Category = "Components|Spine|Skeleton")
	void GetSkins(TArray<FString> &Skins);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool SetSkins(UPARAM(ref) TArray<FString>& SkinNames);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool SetSkin (const FString SkinName);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool HasSkin(const FString SkinName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool SetAttachment (const FString slotName, const FString attachmentName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	FTransform GetBoneWorldTransform (const FString& BoneName);
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void SetBoneWorldPosition (const FString& BoneName, const FVector& position);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void UpdateWorldTransform();
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void SetToSetupPose ();
	
	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void SetBonesToSetupPose ();

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void SetSlotsToSetupPose();

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void SetScaleX(float scaleX);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	float GetScaleX();

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	void SetScaleY(float scaleY);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	float GetScaleY();

	UFUNCTION(BlueprintPure, Category = "Components|Spine|Skeleton")
	void GetBones(TArray<FString> &Bones);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool HasBone(const FString BoneName);

	UFUNCTION(BlueprintPure, Category = "Components|Spine|Skeleton")
	void GetSlots(TArray<FString> &Slots);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool HasSlot(const FString SlotName);

	UFUNCTION(BlueprintPure, Category = "Components|Spine|Skeleton")
	void GetAnimations(TArray<FString> &Animations);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	bool HasAnimation(FString AnimationName);

	UFUNCTION(BlueprintCallable, Category = "Components|Spine|Skeleton")
	float GetAnimationDuration(FString AnimationName);
	
	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Skeleton")
	FSpineBeforeUpdateWorldTransformDelegate BeforeUpdateWorldTransform;
	
	UPROPERTY(BlueprintAssignable, Category = "Components|Spine|Skeleton")
	FSpineAfterUpdateWorldTransformDelegate AfterUpdateWorldTransform;
		
	USpineSkeletonComponent ();
	
	virtual void BeginPlay () override;
		
	virtual void TickComponent (float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;	

	virtual void FinishDestroy () override;
	
protected:
	virtual void CheckState ();
	virtual void InternalTick(float DeltaTime, bool CallDelegates = true, bool Preview = false);
	virtual void DisposeState ();

	spine::Skeleton* skeleton;
	USpineAtlasAsset* lastAtlas = nullptr;
	spine::Atlas* lastSpineAtlas = nullptr;
	USpineSkeletonDataAsset* lastData = nullptr;
	spine::Skin* customSkin = nullptr;
};
