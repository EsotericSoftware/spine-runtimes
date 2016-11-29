#include "SpineEditorPluginPrivatePCH.h"

#include "SpineAtlasAsset.h"
#include "AssetRegistryModule.h"
#include "AssetToolsModule.h"
#include "PackageTools.h"
#include "Developer/DesktopPlatform/Public/IDesktopPlatform.h"
#include "Developer/DesktopPlatform/Public/DesktopPlatformModule.h"
#include "spine/spine.h"
#include <string>
#include <string.h>
#include <stdlib.h>

#define LOCTEXT_NAMESPACE "Spine"

USpineAtlasAssetFactory::USpineAtlasAssetFactory (const FObjectInitializer& objectInitializer): Super(objectInitializer) {
    bCreateNew = false;
    bEditAfterNew = true;
    bEditorImport = true;
    SupportedClass = USpineAtlasAsset::StaticClass();
    
    Formats.Add(TEXT("atlas;Spine atlas file"));
}

FText USpineAtlasAssetFactory::GetToolTip () const {
    return LOCTEXT("SpineAtlasAssetFactory", "Animations exported from Spine");
}

bool USpineAtlasAssetFactory::FactoryCanImport (const FString& filename) {
    return true;
}

UObject* USpineAtlasAssetFactory::FactoryCreateFile (UClass * InClass, UObject * InParent, FName InName, EObjectFlags Flags, const FString & Filename, const TCHAR* Parms, FFeedbackContext * Warn, bool& bOutOperationCanceled) {
    FString rawString;
    if (!FFileHelper::LoadFileToString(rawString, *Filename)) {
        return nullptr;
    }
    const FString longPackagePath = FPackageName::GetLongPackagePath(InParent->GetOutermost()->GetPathName());
    FString CurrentSourcePath;
    FString FilenameNoExtension;
    FString UnusedExtension;
    FPaths::Split(UFactory::GetCurrentFilename(), CurrentSourcePath, FilenameNoExtension, UnusedExtension);
    FString name(InName.ToString());
    name.Append("-atlas");
    USpineAtlasAsset* asset = NewObject<USpineAtlasAsset>(InParent, InClass, FName(*name), Flags);
    asset->SetRawData(rawString);
    asset->SetAtlasFileName(FName(*Filename));
    LoadAtlas(asset, CurrentSourcePath, longPackagePath);
    return asset;
}

bool USpineAtlasAssetFactory::CanReimport(UObject* Obj, TArray<FString>& OutFilenames) {
    USpineAtlasAsset* asset = Cast<USpineAtlasAsset>(Obj);
    if (!asset) return false;
    FString filename = asset->GetAtlasFileName().ToString();
    if (!filename.IsEmpty())
        OutFilenames.Add(filename);
    return true;
}

void USpineAtlasAssetFactory::SetReimportPaths(UObject* Obj, const TArray<FString>& NewReimportPaths) {
    USpineAtlasAsset* asset = Cast<USpineAtlasAsset>(Obj);
    if (asset && ensure(NewReimportPaths.Num() == 1))
        asset->SetAtlasFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineAtlasAssetFactory::Reimport(UObject* Obj) {
    USpineAtlasAsset* asset = Cast<USpineAtlasAsset>(Obj);
    FString rawString;
    if (!FFileHelper::LoadFileToString(rawString, *asset->GetAtlasFileName().ToString())) return EReimportResult::Failed;
    asset->SetRawData(rawString);
    const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
    FString CurrentSourcePath;
    FString FilenameNoExtension;
    FString UnusedExtension;
    FPaths::Split(UFactory::GetCurrentFilename(), CurrentSourcePath, FilenameNoExtension, UnusedExtension);
    LoadAtlas(asset, CurrentSourcePath, longPackagePath);
    if (Obj->GetOuter()) Obj->GetOuter()->MarkPackageDirty();
    else Obj->MarkPackageDirty();
    return EReimportResult::Succeeded;
}

UTexture2D* resolveTexture (USpineAtlasAsset* asset, const FString& pageFileName, const FString& targetSubPath) {
    FAssetToolsModule& AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
    
    TArray<FString> fileNames;
    fileNames.Add(pageFileName);
    
    //@TODO: Avoid the first compression, since we're going to recompress
    TArray<UObject*> importedAsset = AssetToolsModule.Get().ImportAssets(fileNames, targetSubPath);
    UTexture2D* texture = (importedAsset.Num() > 0) ? Cast<UTexture2D>(importedAsset[0]) : nullptr;
    
    return texture;
}

void USpineAtlasAssetFactory::LoadAtlas(USpineAtlasAsset* asset, const FString& currentSourcePath, const FString& longPackagePath) {
    spAtlas* atlas = asset->GetAtlas(true);
    asset->atlasPages.Empty();
    
    const FString targetTexturePath = longPackagePath / TEXT("Textures");
    
    spAtlasPage* page = atlas->pages;
    while (page) {
        const FString sourceTextureFilename = FPaths::Combine(*currentSourcePath, UTF8_TO_TCHAR(page->name));
        UTexture2D* texture = resolveTexture(asset, sourceTextureFilename, targetTexturePath);
        page = page->next;
        asset->atlasPages.Add(texture);
    }
}

#undef LOCTEXT_NAMESPACE
