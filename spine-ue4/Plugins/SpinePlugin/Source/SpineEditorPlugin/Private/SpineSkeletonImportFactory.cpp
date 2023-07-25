/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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
#include <string.h>

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
	if (Filename.Contains(TEXT(".skel"))) return true;

	if (Filename.Contains(TEXT(".json"))) {
		TArray<uint8> rawData;
		if (!FFileHelper::LoadFileToArray(rawData, *Filename, 0)) {
			return false;
		}
		if (rawData.Num() == 0) return false;
		return strcmp((const char *) rawData.GetData(), "skeleton") > 0 && strcmp((const char *) rawData.GetData(), "spine") > 0;
	}

	return false;
}

void LoadAtlas(const FString &Filename, const FString &TargetPath) {
	FAssetToolsModule &AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");

	FString skelFile = Filename.Replace(TEXT(".skel"), TEXT(".atlas")).Replace(TEXT(".json"), TEXT(".atlas"));
	if (!FPaths::FileExists(skelFile)) return;

	TArray<FString> fileNames;
	fileNames.Add(skelFile);
	AssetToolsModule.Get().ImportAssets(fileNames, TargetPath);
}

UObject *USpineSkeletonAssetFactory::FactoryCreateFile(UClass *InClass, UObject *InParent, FName InName, EObjectFlags Flags, const FString &Filename, const TCHAR *Parms, FFeedbackContext *Warn, bool &bOutOperationCanceled) {
	FString name(InName.ToString());
	name.Append("-data");

	USpineSkeletonDataAsset *asset = NewObject<USpineSkeletonDataAsset>(InParent, InClass, FName(*name), Flags);
	TArray<uint8> rawData;
	if (!FFileHelper::LoadFileToArray(rawData, *Filename, 0)) {
		return nullptr;
	}
	asset->SetSkeletonDataFileName(FName(*Filename));
	asset->SetRawData(rawData);

	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	LoadAtlas(Filename, longPackagePath);
	return asset;
}

bool USpineSkeletonAssetFactory::CanReimport(UObject *Obj, TArray<FString> &OutFilenames) {
	USpineSkeletonDataAsset *asset = Cast<USpineSkeletonDataAsset>(Obj);
	if (!asset) return false;

	FString filename = asset->GetSkeletonDataFileName().ToString();
	if (!filename.IsEmpty())
		OutFilenames.Add(filename);

	return true;
}

void USpineSkeletonAssetFactory::SetReimportPaths(UObject *Obj, const TArray<FString> &NewReimportPaths) {
	USpineSkeletonDataAsset *asset = Cast<USpineSkeletonDataAsset>(Obj);

	if (asset && ensure(NewReimportPaths.Num() == 1))
		asset->SetSkeletonDataFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineSkeletonAssetFactory::Reimport(UObject *Obj) {
	USpineSkeletonDataAsset *asset = Cast<USpineSkeletonDataAsset>(Obj);
	TArray<uint8> rawData;
	if (!FFileHelper::LoadFileToArray(rawData, *asset->GetSkeletonDataFileName().ToString(), 0)) return EReimportResult::Failed;
	asset->SetRawData(rawData);

	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	LoadAtlas(*asset->GetSkeletonDataFileName().ToString(), longPackagePath);

	if (Obj->GetOuter()) Obj->GetOuter()->MarkPackageDirty();
	else
		Obj->MarkPackageDirty();

	return EReimportResult::Succeeded;
}

#undef LOCTEXT_NAMESPACE
