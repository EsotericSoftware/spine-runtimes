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

UCLASS(ClassGroup=(Spine))
class SPINEPLUGIN_API USpineSkeletonDataAsset: public UObject {
	GENERATED_BODY()
	
public:
	spine::SkeletonData* GetSkeletonData(spine::Atlas* Atlas, bool ForceReload = false);

	spine::AnimationStateData* GetAnimationStateData(spine::Atlas* atlas);
	void SetMix(const FString& from, const FString& to, float mix);
	float GetMix(const FString& from, const FString& to);
	
	FName GetSkeletonDataFileName () const;
	TArray<uint8>& GetRawData ();
	
	virtual void BeginDestroy () override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	float DefaultMix = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
	TArray<FSpineAnimationStateMixData> MixData;
	
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
};
