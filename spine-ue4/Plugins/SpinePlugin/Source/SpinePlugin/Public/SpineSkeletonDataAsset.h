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

#include "Engine.h"
#include "spine/spine.h"
#include "SpineSkeletonDataAsset.generated.h"

USTRUCT(BlueprintType, Category = "Spine")
struct SPINEPLUGIN_API FSpineAnimationStateMixData {
	GENERATED_BODY();

public:	
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	FString From;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	FString To;
	
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	float Mix = 0;
};

UCLASS(BlueprintType, ClassGroup=(Spine))
class SPINEPLUGIN_API USpineSkeletonDataAsset: public UObject {
	GENERATED_BODY()
	
public:
	spine::SkeletonData* GetSkeletonData(spine::Atlas* Atlas);

	spine::AnimationStateData* GetAnimationStateData(spine::Atlas* atlas);
	void SetMix(const FString& from, const FString& to, float mix);
	float GetMix(const FString& from, const FString& to);
	
	FName GetSkeletonDataFileName () const;
	void SetRawData (TArray<uint8> &Data);
	
	virtual void BeginDestroy () override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	float DefaultMix = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	TArray<FSpineAnimationStateMixData> MixData;

	UPROPERTY(Transient, VisibleAnywhere)
	TArray<FString> Bones;

	UPROPERTY(Transient, VisibleAnywhere)
	TArray<FString> Slots;

	UPROPERTY(Transient, VisibleAnywhere)
	TArray<FString> Skins;

	UPROPERTY(Transient, VisibleAnywhere)
	TArray<FString> Animations;

	UPROPERTY(Transient, VisibleAnywhere)
	TArray<FString> Events;
	
protected:
	UPROPERTY()
	TArray<uint8> rawData;		
	
	UPROPERTY()
	FName skeletonDataFileName;

	spine::SkeletonData* skeletonData;
	spine::AnimationStateData* animationStateData;
	spine::Atlas* lastAtlas;
	
#if WITH_EDITORONLY_DATA
public:
	void SetSkeletonDataFileName (const FName &skeletonDataFileName);	 
	
protected:
	UPROPERTY(VisibleAnywhere, Instanced, Category=ImportSettings)
	class UAssetImportData* importData;
	
	virtual void PostInitProperties ( ) override;
	virtual void GetAssetRegistryTags(TArray<FAssetRegistryTag>& OutTags) const override;
	virtual void Serialize (FArchive& Ar) override;
#endif

	void LoadInfo();
};
