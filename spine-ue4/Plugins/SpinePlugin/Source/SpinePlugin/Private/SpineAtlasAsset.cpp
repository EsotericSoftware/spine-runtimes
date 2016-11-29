#include "SpinePluginPrivatePCH.h"
#include "spine/spine.h"
#include <string.h>
#include <string>
#include <stdlib.h>

FString USpineAtlasAsset::GetRawData () const {
    return rawData;
}

FName USpineAtlasAsset::GetAtlasFileName () const {
#if WITH_EDITORONLY_DATA
    TArray<FString> files;
    if (importData) importData->ExtractFilenames(files);
    if (files.Num() > 0) return FName(*files[0]);
    else return atlasFileName;
#else
    return atlasFileName;
#endif
}

#if WITH_EDITORONLY_DATA

void USpineAtlasAsset::SetRawData (const FString &_rawData) {
    this->rawData = _rawData;
}

void USpineAtlasAsset::SetAtlasFileName (const FName &_atlasFileName) {
    importData->UpdateFilenameOnly(_atlasFileName.ToString());
    TArray<FString> files;
    importData->ExtractFilenames(files);
    if (files.Num() > 0) atlasFileName = FName(*files[0]);
}

void USpineAtlasAsset::PostInitProperties () {
    if (!HasAnyFlags(RF_ClassDefaultObject)) importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
    Super::PostInitProperties();
}

void USpineAtlasAsset::GetAssetRegistryTags (TArray<FAssetRegistryTag>& OutTags) const {
    if (importData) {
        OutTags.Add(FAssetRegistryTag(SourceFileTagName(), importData->GetSourceData().ToJson(), FAssetRegistryTag::TT_Hidden) );
    }
    
    Super::GetAssetRegistryTags(OutTags);
}

void USpineAtlasAsset::Serialize (FArchive& Ar) {
    Super::Serialize(Ar);
    if (Ar.IsLoading() && Ar.UE4Ver() < VER_UE4_ASSET_IMPORT_DATA_AS_JSON && !importData)
        importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
}

void USpineAtlasAsset::BeginDestroy () {
    if (atlas) {
        spAtlas_dispose(atlas);
        atlas = nullptr;
    }
    Super::BeginDestroy();
}

spAtlas* USpineAtlasAsset::GetAtlas (bool forceReload) {
    if (!atlas || forceReload) {
        if (atlas) {
            spAtlas_dispose(atlas);
            atlas = nullptr;
        }
		std::string t = TCHAR_TO_UTF8(*rawData);
        atlas = spAtlas_create(t.c_str(), strlen(t.c_str()), "", nullptr);
        spAtlasPage* page = atlas->pages;
        int i = 0;
        while (page) {
            int num = atlasPages.Num();
            if (atlasPages.Num() > 0 && atlasPages.Num() > i)
                page->rendererObject = atlasPages[i++];
            page = page->next;
        }        
    }
    return this->atlas;
}

#endif
