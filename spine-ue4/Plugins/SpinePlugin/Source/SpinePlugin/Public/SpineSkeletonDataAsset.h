#pragma once

#include "Engine.h"
#include "spine/spine.h"
#include "SpineSkeletonDataAsset.generated.h"

UCLASS( ClassGroup=(Spine) )
class SPINEPLUGIN_API USpineSkeletonDataAsset : public UObject {
    GENERATED_BODY()
    
public:
    spSkeletonData* GetSkeletonData(spAtlas* atlas, bool forceReload = false);
    
    FName GetSkeletonDataFileName () const;
    TArray<uint8>& GetRawData ();
    
    virtual void BeginDestroy () override;
    
protected:
    UPROPERTY()
    TArray<uint8> rawData;
    
    spAtlas* lastAtlas = nullptr;
    spSkeletonData* skeletonData = nullptr;
    
    UPROPERTY()
    FName skeletonDataFileName;
    
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
