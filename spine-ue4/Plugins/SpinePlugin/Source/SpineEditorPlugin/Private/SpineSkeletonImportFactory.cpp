#include "SpineEditorPluginPrivatePCH.h"

#include "SpineSkeletonDataAsset.h"
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

USpineSkeletonAssetFactory::USpineSkeletonAssetFactory (const FObjectInitializer& objectInitializer): Super(objectInitializer) {
    bCreateNew = false;
    bEditAfterNew = true;
    bEditorImport = true;
    SupportedClass = USpineSkeletonDataAsset::StaticClass();
    
    Formats.Add(TEXT("json;Spine skeleton file"));
    Formats.Add(TEXT("skel;Spine skeleton file"));
}

FText USpineSkeletonAssetFactory::GetToolTip () const {
    return LOCTEXT("USpineSkeletonAssetFactory", "Animations exported from Spine");
}

bool USpineSkeletonAssetFactory::FactoryCanImport (const FString& filename) {
    return true;
}

void LoadAtlas (const FString& filename, const FString& targetPath) {
    FAssetToolsModule& AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
    
    FString skelFile = filename.Replace(TEXT(".skel"), TEXT(".atlas")).Replace(TEXT(".json"), TEXT(".atlas"));
    if (!FPaths::FileExists(skelFile)) return;
    
    TArray<FString> fileNames;
    fileNames.Add(skelFile);
    TArray<UObject*> importedAssets = AssetToolsModule.Get().ImportAssets(fileNames, targetPath);
}

UObject* USpineSkeletonAssetFactory::FactoryCreateFile (UClass * InClass, UObject * InParent, FName InName, EObjectFlags Flags, const FString & Filename, const TCHAR* Parms, FFeedbackContext * Warn, bool& bOutOperationCanceled) {
    FString name(InName.ToString());
    name.Append("-data");
    USpineSkeletonDataAsset* asset = NewObject<USpineSkeletonDataAsset>(InParent, InClass, FName(*name), Flags);
    if (!FFileHelper::LoadFileToArray(asset->GetRawData(), *Filename, 0)) {
        return nullptr;
    }
    asset->SetSkeletonDataFileName(FName(*Filename));
    const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
    LoadAtlas(Filename, longPackagePath);
    return asset;
}

bool USpineSkeletonAssetFactory::CanReimport(UObject* Obj, TArray<FString>& OutFilenames) {
    USpineSkeletonDataAsset* asset = Cast<USpineSkeletonDataAsset>(Obj);
    if (!asset) return false;
    FString filename = asset->GetSkeletonDataFileName().ToString();
    if (!filename.IsEmpty())
        OutFilenames.Add(filename);
    return true;
}

void USpineSkeletonAssetFactory::SetReimportPaths(UObject* Obj, const TArray<FString>& NewReimportPaths) {
    USpineSkeletonDataAsset* asset = Cast<USpineSkeletonDataAsset>(Obj);
    if (asset && ensure(NewReimportPaths.Num() == 1))
        asset->SetSkeletonDataFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineSkeletonAssetFactory::Reimport(UObject* Obj) {
    USpineSkeletonDataAsset* asset = Cast<USpineSkeletonDataAsset>(Obj);
    FString rawString;
    if (!FFileHelper::LoadFileToArray(asset->GetRawData(), *asset->GetSkeletonDataFileName().ToString(), 0)) return EReimportResult::Failed;
    const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
    LoadAtlas(*asset->GetSkeletonDataFileName().ToString(), longPackagePath);
    if (Obj->GetOuter()) Obj->GetOuter()->MarkPackageDirty();
    else Obj->MarkPackageDirty();
    return EReimportResult::Succeeded;
}
#undef LOCTEXT_NAMESPACE
