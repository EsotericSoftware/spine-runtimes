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

#include "SpineEditorPluginPrivatePCH.h"

#include "SpineAtlasAsset.h"
#include "AssetRegistryModule.h"
#include "AssetToolsModule.h"
#include "PackageTools.h"
#include "Developer/AssetTools/Public/IAssetTools.h"
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
	FPaths::Split(UFactory::GetCurrentFilename(), currentSourcePath, filenameNoExtension, unusedExtension);
	
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
	spAtlas* atlas = Asset->GetAtlas(true);
	Asset->atlasPages.Empty();
	
	const FString targetTexturePath = LongPackagePath / TEXT("Textures");
	
	spAtlasPage* page = atlas->pages;
	while (page) {
		const FString sourceTextureFilename = FPaths::Combine(*CurrentSourcePath, UTF8_TO_TCHAR(page->name));
		UTexture2D* texture = resolveTexture(Asset, sourceTextureFilename, targetTexturePath);
		page = page->next;
		Asset->atlasPages.Add(texture);
	}
}

#undef LOCTEXT_NAMESPACE
