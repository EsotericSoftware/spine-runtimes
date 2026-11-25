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

#include "SpineSkeletonImportFactory.h"
#include "AssetToolsModule.h"
#include "Developer/AssetTools/Public/IAssetTools.h"
#include "SpineSkeletonDataAsset.h"
#include "Editor.h"

#define LOCTEXT_NAMESPACE "Spine"

USpineSkeletonAssetFactory::USpineSkeletonAssetFactory(const FObjectInitializer &objectInitializer) : Super(objectInitializer) {
	bCreateNew = false;
	bEditAfterNew = true;
	bEditorImport = true;
	SupportedClass = USpineSkeletonDataAsset::StaticClass();

	Formats.Add(TEXT("json;Spine skeleton file"));
	Formats.Add(TEXT("skel;Spine skeleton file"));
}

FText USpineSkeletonAssetFactory::GetToolTip() const {
	return LOCTEXT("USpineSkeletonAssetFactory", "Animations exported from Spine");
}

bool USpineSkeletonAssetFactory::FactoryCanImport(const FString &Filename) {
	const FString extension = FPaths::GetExtension(Filename);
	if (extension.Equals(TEXT("skel"), ESearchCase::IgnoreCase)) return true;
	if (!extension.Equals(TEXT("json"), ESearchCase::IgnoreCase)) return false;

	FString rawData;
	if (!FFileHelper::LoadFileToString(rawData, *Filename)) return false;
	return rawData.Contains(TEXT("\"skeleton\"")) && rawData.Contains(TEXT("\"spine\""));
}

static void LoadAtlas(const FString &Filename, const FString &TargetPath) {
	FAssetToolsModule &AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");

	const FString atlasFile = FPaths::ChangeExtension(Filename, TEXT("atlas"));
	if (!FPaths::FileExists(atlasFile)) return;

	// Reimport an existing atlas in place instead of prompting to overwrite it, which also updates its page textures.
	const FString atlasName = FPaths::GetBaseFilename(atlasFile);
	const FString atlasObjectPath = TargetPath / atlasName + TEXT(".") + atlasName;
	if (USpineAtlasAsset *atlas = LoadObject<USpineAtlasAsset>(nullptr, *atlasObjectPath, nullptr, LOAD_NoWarn | LOAD_Quiet)) {
		FReimportManager::Instance()->Reimport(atlas, false, true, atlasFile, nullptr, INDEX_NONE, false, true);
		return;
	}

	TArray<FString> fileNames;
	fileNames.Add(atlasFile);
	AssetToolsModule.Get().ImportAssets(fileNames, TargetPath, nullptr, false);
}

UObject *USpineSkeletonAssetFactory::FactoryCreateFile(UClass *InClass, UObject *InParent, FName InName, EObjectFlags Flags, const FString &Filename,
													   const TCHAR *Parms, FFeedbackContext *Warn, bool &bOutOperationCanceled) {
	const FString fileExtension = FPaths::GetExtension(Filename);
	GEditor->GetEditorSubsystem<UImportSubsystem>()->BroadcastAssetPreImport(this, InClass, InParent, InName, *fileExtension);

	TArray<uint8> rawData;
	if (!FFileHelper::LoadFileToArray(rawData, *Filename, 0)) return nullptr;

	USpineSkeletonDataAsset *asset = NewObject<USpineSkeletonDataAsset>(InParent, InClass, InName, Flags);
	asset->SetSkeletonDataFileName(FName(*Filename));
	FString error;
	if (!asset->SetRawDataFromImport(rawData, error)) {
		if (Warn) Warn->Logf(ELogVerbosity::Error, TEXT("%s"), *error);
		return nullptr;
	}
	asset->UpdateSkeletonDataFileName(FName(*Filename));

	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	LoadAtlas(Filename, longPackagePath);
	GEditor->GetEditorSubsystem<UImportSubsystem>()->BroadcastAssetPostImport(this, asset);
	return asset;
}

bool USpineSkeletonAssetFactory::CanReimport(UObject *Obj, TArray<FString> &OutFilenames) {
	USpineSkeletonDataAsset *asset = Cast<USpineSkeletonDataAsset>(Obj);
	if (!asset) return false;

	FString filename = asset->GetSkeletonDataFileName().ToString();
	if (!filename.IsEmpty()) OutFilenames.Add(filename);

	return true;
}

void USpineSkeletonAssetFactory::SetReimportPaths(UObject *Obj, const TArray<FString> &NewReimportPaths) {
	USpineSkeletonDataAsset *asset = Cast<USpineSkeletonDataAsset>(Obj);

	if (asset && ensure(NewReimportPaths.Num() == 1)) asset->SetSkeletonDataFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineSkeletonAssetFactory::Reimport(UObject *Obj) {
	USpineSkeletonDataAsset *asset = Cast<USpineSkeletonDataAsset>(Obj);
	if (!asset) return EReimportResult::Failed;

	const FString sourceFilename = asset->GetSkeletonDataFileName().ToString();
	TArray<uint8> rawData;
	if (!FFileHelper::LoadFileToArray(rawData, *sourceFilename, 0)) return EReimportResult::Failed;

	FString error;
	if (!asset->SetRawDataFromImport(rawData, error)) {
		UE_LOG(LogTemp, Error, TEXT("%s"), *error);
		return EReimportResult::Failed;
	}
	asset->UpdateSkeletonDataFileName(FName(*sourceFilename));

	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	LoadAtlas(sourceFilename, longPackagePath);

	if (Obj->GetOuter())
		Obj->GetOuter()->MarkPackageDirty();
	else
		Obj->MarkPackageDirty();

	GEditor->GetEditorSubsystem<UImportSubsystem>()->BroadcastAssetReimport(asset);
	return EReimportResult::Succeeded;
}

#undef LOCTEXT_NAMESPACE
