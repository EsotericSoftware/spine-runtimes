#include "SpinePluginPrivatePCH.h"
#include "spine/spine.h"
#include <string.h>
#include <string>
#include <stdlib.h>

FName USpineSkeletonDataAsset::GetSkeletonDataFileName () const {
#if WITH_EDITORONLY_DATA
    TArray<FString> files;
    if (importData) importData->ExtractFilenames(files);
    if (files.Num() > 0) return FName(*files[0]);
    else return skeletonDataFileName;
#else
    return skeletonDataFileName;
#endif
}

TArray<uint8>& USpineSkeletonDataAsset::GetRawData () {
    return this->rawData;
}

#if WITH_EDITORONLY_DATA

void USpineSkeletonDataAsset::SetSkeletonDataFileName (const FName &_skeletonDataFileName) {
    importData->UpdateFilenameOnly(_skeletonDataFileName.ToString());
    TArray<FString> files;
    importData->ExtractFilenames(files);
    if (files.Num() > 0) this->skeletonDataFileName = FName(*files[0]);
}

void USpineSkeletonDataAsset::PostInitProperties () {
    if (!HasAnyFlags(RF_ClassDefaultObject)) importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
    Super::PostInitProperties();
}

void USpineSkeletonDataAsset::GetAssetRegistryTags (TArray<FAssetRegistryTag>& OutTags) const {
    if (importData) {
        OutTags.Add(FAssetRegistryTag(SourceFileTagName(), importData->GetSourceData().ToJson(), FAssetRegistryTag::TT_Hidden) );
    }
    
    Super::GetAssetRegistryTags(OutTags);
}

void USpineSkeletonDataAsset::Serialize (FArchive& Ar) {
    Super::Serialize(Ar);
    if (Ar.IsLoading() && Ar.UE4Ver() < VER_UE4_ASSET_IMPORT_DATA_AS_JSON && !importData)
        importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
}

void USpineSkeletonDataAsset::BeginDestroy () {
    if (this->skeletonData) {
        spSkeletonData_dispose(this->skeletonData);
        this->skeletonData = nullptr;
    }
    Super::BeginDestroy();
}

spSkeletonData* USpineSkeletonDataAsset::GetSkeletonData (spAtlas* atlas, bool forceReload) {
    if (!skeletonData || forceReload) {
        if (skeletonData) {
            spSkeletonData_dispose(skeletonData);
            skeletonData = nullptr;
        }
        int dataLen = rawData.Num();
        if (skeletonDataFileName.GetPlainNameString().Contains(TEXT(".json"))) {
            spSkeletonJson* json = spSkeletonJson_create(atlas);
            this->skeletonData = spSkeletonJson_readSkeletonData(json, (const char*)rawData.GetData());
            spSkeletonJson_dispose(json);
        } else {
            spSkeletonBinary* binary = spSkeletonBinary_create(atlas);
            this->skeletonData = spSkeletonBinary_readSkeletonData(binary, (const unsigned char*)rawData.GetData(), (int)rawData.Num());
            spSkeletonBinary_dispose(binary);
        }
        lastAtlas = atlas;
    }
    return this->skeletonData;
}

#endif
