/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#include "SpineAtlasImportFactory.h"
#include "AssetToolsModule.h"
#include "SpineAtlasAsset.h"
#include "Editor.h"
#include "EditorFramework/AssetImportData.h"

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

USpineAtlasAssetFactory::USpineAtlasAssetFactory(const FObjectInitializer &objectInitializer) : Super(objectInitializer) {
	bCreateNew = false;
	bEditAfterNew = true;
	bEditorImport = true;
	SupportedClass = USpineAtlasAsset::StaticClass();

	Formats.Add(TEXT("atlas;Spine Atlas file"));
}

FText USpineAtlasAssetFactory::GetToolTip() const {
	return LOCTEXT("SpineAtlasAssetFactory", "Animations exported from Spine");
}

bool USpineAtlasAssetFactory::FactoryCanImport(const FString &Filename) {
	return FPaths::GetExtension(Filename).Equals(TEXT("atlas"), ESearchCase::IgnoreCase);
}

UObject *USpineAtlasAssetFactory::FactoryCreateFile(UClass *InClass, UObject *InParent, FName InName, EObjectFlags Flags, const FString &Filename,
													const TCHAR *Parms, FFeedbackContext *Warn, bool &bOutOperationCanceled) {
	FString FileExtension = FPaths::GetExtension(Filename);
	GEditor->GetEditorSubsystem<UImportSubsystem>()->BroadcastAssetPreImport(this, InClass, InParent, InName, *FileExtension);

	FString rawString;
	if (!FFileHelper::LoadFileToString(rawString, *Filename)) {
		return nullptr;
	}

	FString currentSourcePath, filenameNoExtension, unusedExtension;
	const FString longPackagePath = FPackageName::GetLongPackagePath(InParent->GetOutermost()->GetPathName());
	FPaths::Split(UFactory::GetCurrentFilename(), currentSourcePath, filenameNoExtension, unusedExtension);

	USpineAtlasAsset *asset = NewObject<USpineAtlasAsset>(InParent, InClass, InName, Flags);
	asset->SetRawData(rawString);
	asset->SetAtlasFileName(FName(*Filename));
	LoadAtlas(asset, currentSourcePath, longPackagePath);
	asset->UpdateAtlasFileName(FName(*Filename));
	GEditor->GetEditorSubsystem<UImportSubsystem>()->BroadcastAssetPostImport(this, asset);
	return asset;
}

bool USpineAtlasAssetFactory::CanReimport(UObject *Obj, TArray<FString> &OutFilenames) {
	USpineAtlasAsset *asset = Cast<USpineAtlasAsset>(Obj);
	if (!asset) return false;

	FString filename = asset->GetAtlasFileName().ToString();
	if (!filename.IsEmpty()) OutFilenames.Add(filename);

	return true;
}

void USpineAtlasAssetFactory::SetReimportPaths(UObject *Obj, const TArray<FString> &NewReimportPaths) {
	USpineAtlasAsset *asset = Cast<USpineAtlasAsset>(Obj);

	if (asset && ensure(NewReimportPaths.Num() == 1)) asset->SetAtlasFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineAtlasAssetFactory::Reimport(UObject *Obj) {
	USpineAtlasAsset *asset = Cast<USpineAtlasAsset>(Obj);
	if (!asset) return EReimportResult::Failed;

	const FString sourceFilename = asset->GetAtlasFileName().ToString();
	FString rawString;
	if (!FFileHelper::LoadFileToString(rawString, *sourceFilename)) return EReimportResult::Failed;
	asset->SetRawData(rawString);

	FString currentSourcePath, filenameNoExtension, unusedExtension;
	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	FPaths::Split(sourceFilename, currentSourcePath, filenameNoExtension, unusedExtension);

	LoadAtlas(asset, currentSourcePath, longPackagePath);
	asset->UpdateAtlasFileName(FName(*sourceFilename));

	if (Obj->GetOuter())
		Obj->GetOuter()->MarkPackageDirty();
	else
		Obj->MarkPackageDirty();

	GEditor->GetEditorSubsystem<UImportSubsystem>()->BroadcastAssetReimport(asset);
	return EReimportResult::Succeeded;
}

static bool isTextureUpToDate(UTexture2D *Texture, const FString &SourceFilename) {
	const UAssetImportData *importData = Texture->AssetImportData;
	if (!importData || importData->GetSourceFileCount() != 1) return false;
	if (!FPaths::IsSamePath(importData->GetFirstFilename(), SourceFilename)) return false;
	const FMD5Hash &importedHash = importData->GetSourceData().SourceFiles[0].FileHash;
	return importedHash.IsValid() && importedHash == FMD5Hash::HashFile(*SourceFilename);
}

static UTexture2D *resolveTexture(const FString &PageFileName, const FString &TargetSubPath) {
	const FString assetName = FPaths::GetBaseFilename(PageFileName);
	const FString objectPath = TargetSubPath / assetName + TEXT(".") + assetName;
	if (UTexture2D *texture = LoadObject<UTexture2D>(nullptr, *objectPath, nullptr, LOAD_NoWarn | LOAD_Quiet)) {
		// Reimport the existing texture in place if its source image changed, so references to it stay valid.
		if (FPaths::FileExists(PageFileName) && !isTextureUpToDate(texture, PageFileName))
			FReimportManager::Instance()->Reimport(texture, false, true, PageFileName, nullptr, INDEX_NONE, false, true);
		return texture;
	}

	FAssetToolsModule &AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
	TArray<FString> fileNames;
	fileNames.Add(PageFileName);

	TArray<UObject *> importedAsset = AssetToolsModule.Get().ImportAssets(fileNames, TargetSubPath, nullptr, false);
	return (importedAsset.Num() > 0) ? Cast<UTexture2D>(importedAsset[0]) : nullptr;
}

void USpineAtlasAssetFactory::LoadAtlas(USpineAtlasAsset *Asset, const FString &CurrentSourcePath, const FString &LongPackagePath) {
	Atlas *atlas = Asset->GetAtlas();
	Asset->atlasPages.Empty();

	const FString targetTexturePath = LongPackagePath / TEXT("Textures");

	Array<AtlasPage *> &pages = atlas->getPages();
	for (size_t i = 0, n = pages.size(); i < n; i++) {
		AtlasPage *page = pages[i];
		const FString sourceTextureFilename = FPaths::Combine(*CurrentSourcePath, UTF8_TO_TCHAR(page->name.buffer()));
		UTexture2D *texture = resolveTexture(sourceTextureFilename, targetTexturePath);
		Asset->atlasPages.Add(texture);
	}
}

#undef LOCTEXT_NAMESPACE
