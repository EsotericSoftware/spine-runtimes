#pragma once

#include "Engine.h"
#include "spine/spine.h"
#include "SpineAtlasAsset.generated.h"

UCLASS( ClassGroup=(Spine) )
class SPINEPLUGIN_API USpineAtlasAsset : public UObject {
    GENERATED_BODY()
    
public:
    spAtlas* GetAtlas (bool forceReload = false);
    
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Spine)
    TArray<UTexture2D*> atlasPages;
    
    FString GetRawData () const;
    FName GetAtlasFileName () const;
    
    virtual void BeginDestroy () override;
    
protected:
    spAtlas* atlas = nullptr;
    
    UPROPERTY()
    FString rawData;
    
    UPROPERTY()
    FName atlasFileName;
    
#if WITH_EDITORONLY_DATA

public:
    void SetRawData (const FString &rawData);
    void SetAtlasFileName (const FName &atlasFileName);
    
protected:
    UPROPERTY(VisibleAnywhere, Instanced, Category=ImportSettings)
    class UAssetImportData* importData;
    
    virtual void PostInitProperties ( ) override;
    virtual void GetAssetRegistryTags(TArray<FAssetRegistryTag>& OutTags) const override;
    virtual void Serialize (FArchive& Ar) override;
#endif
};
