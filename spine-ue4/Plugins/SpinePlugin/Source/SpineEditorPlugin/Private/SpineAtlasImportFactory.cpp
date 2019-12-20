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

#include "SpineEditorPluginPrivatePCH.h"

#include "SpineAtlasAsset.h"
#include "AssetRegistryModule.h"
#include "AssetToolsModule.h"
#include "PackageTools.h"
#include "Developer/AssetTools/Public/IAssetTools.h"
#include "Developer/DesktopPlatform/Public/IDesktopPlatform.h"
#include "Developer/DesktopPlatform/Public/DesktopPlatformModule.h"
#include <string>
#include <string.h>
#include <stdlib.h>

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

USpineAtlasAssetFactory::USpineAtlasAssetFactory (const FObjectInitializer& objectInitializer): Super(objectInitializer) {
	bCreateNew = false;
	bEditAfterNew = true;
	bEditorImport = true;
	SupportedClass = USpineAtlasAsset::StaticClass();
	
	Formats.Add(TEXT("atlas;Spine Atlas file"));
}

FText USpineAtlasAssetFactory::GetToolTip () const {
	return LOCTEXT("SpineAtlasAssetFactory", "Animations exported from Spine");
}

bool USpineAtlasAssetFactory::FactoryCanImport (const FString& Filename) {
	return true;
}

UObject* USpineAtlasAssetFactory::FactoryCreateFile (UClass * InClass, UObject * InParent, FName InName, EObjectFlags Flags, const FString & Filename, const TCHAR* Parms, FFeedbackContext * Warn, bool& bOutOperationCanceled) {
	FString rawString;
	if (!FFileHelper::LoadFileToString(rawString, *Filename)) {
		return nullptr;
	}
	
	FString currentSourcePath, filenameNoExtension, unusedExtension;
	const FString longPackagePath = FPackageName::GetLongPackagePath(InParent->GetOutermost()->GetPathName());
	FPaths::Split(UFactory::GetCurrentFilename(), currentSourcePath, filenameNoExtension, unusedExtension);
	FString name(InName.ToString());
	name.Append("-atlas");
	
	USpineAtlasAsset* asset = NewObject<USpineAtlasAsset>(InParent, InClass, FName(*name), Flags);
	asset->SetRawData(rawString);
	asset->SetAtlasFileName(FName(*Filename));
	LoadAtlas(asset, currentSourcePath, longPackagePath);
	return asset;
}

bool USpineAtlasAssetFactory::CanReimport (UObject* Obj, TArray<FString>& OutFilenames) {
	USpineAtlasAsset* asset = Cast<USpineAtlasAsset>(Obj);
	if (!asset) return false;
	
	FString filename = asset->GetAtlasFileName().ToString();
	if (!filename.IsEmpty())
		OutFilenames.Add(filename);
	
	return true;
}

void USpineAtlasAssetFactory::SetReimportPaths (UObject* Obj, const TArray<FString>& NewReimportPaths) {
	USpineAtlasAsset* asset = Cast<USpineAtlasAsset>(Obj);
	
	if (asset && ensure(NewReimportPaths.Num() == 1))
		asset->SetAtlasFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineAtlasAssetFactory::Reimport (UObject* Obj) {
	USpineAtlasAsset* asset = Cast<USpineAtlasAsset>(Obj);
	FString rawString;
	if (!FFileHelper::LoadFileToString(rawString, *asset->GetAtlasFileName().ToString())) return EReimportResult::Failed;
	asset->SetRawData(rawString);
	
	FString currentSourcePath, filenameNoExtension, unusedExtension;
	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	FString currentFileName = asset->GetAtlasFileName().ToString();
	FPaths::Split(currentFileName, currentSourcePath, filenameNoExtension, unusedExtension);
	
	LoadAtlas(asset, currentSourcePath, longPackagePath);
	
	if (Obj->GetOuter()) Obj->GetOuter()->MarkPackageDirty();
	else Obj->MarkPackageDirty();
	
	return EReimportResult::Succeeded;
}

UTexture2D* resolveTexture (USpineAtlasAsset* Asset, const FString& PageFileName, const FString& TargetSubPath) {
	FAssetToolsModule& AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
	
	TArray<FString> fileNames;
	fileNames.Add(PageFileName);
		
	TArray<UObject*> importedAsset = AssetToolsModule.Get().ImportAssets(fileNames, TargetSubPath);
	UTexture2D* texture = (importedAsset.Num() > 0) ? Cast<UTexture2D>(importedAsset[0]) : nullptr;
	
	return texture;
}

void USpineAtlasAssetFactory::LoadAtlas (USpineAtlasAsset* Asset, const FString& CurrentSourcePath, const FString& LongPackagePath) {
	Atlas* atlas = Asset->GetAtlas();
	Asset->atlasPages.Empty();
	
	const FString targetTexturePath = LongPackagePath / TEXT("Textures");
	
	Vector<AtlasPage*> &pages = atlas->getPages();
	for (size_t i = 0, n = pages.size(); i < n; i++) {
		AtlasPage* page = pages[i];
		const FString sourceTextureFilename = FPaths::Combine(*CurrentSourcePath, UTF8_TO_TCHAR(page->name.buffer()));
		UTexture2D* texture = resolveTexture(Asset, sourceTextureFilename, targetTexturePath);
		Asset->atlasPages.Add(texture);
	}
}

#undef LOCTEXT_NAMESPACE
