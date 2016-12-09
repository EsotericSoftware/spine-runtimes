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
	spSkeletonData* GetSkeletonData(spAtlas* Atlas, bool ForceReload = false);

	spAnimationStateData* GetAnimationStateData(spAtlas* atlas);
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

	spSkeletonData* skeletonData;
	spAnimationStateData* animationStateData;
	spAtlas* lastAtlas;
	
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
