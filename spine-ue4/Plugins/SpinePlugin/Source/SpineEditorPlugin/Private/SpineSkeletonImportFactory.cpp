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

#include "SpineSkeletonDataAsset.h"
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

bool USpineSkeletonAssetFactory::FactoryCanImport (const FString& Filename) {
	return true;
}

void LoadAtlas (const FString& Filename, const FString& TargetPath) {
	FAssetToolsModule& AssetToolsModule = FModuleManager::GetModuleChecked<FAssetToolsModule>("AssetTools");
	
	FString skelFile = Filename.Replace(TEXT(".skel"), TEXT(".atlas")).Replace(TEXT(".json"), TEXT(".atlas"));
	if (!FPaths::FileExists(skelFile)) return;
	
	TArray<FString> fileNames;
	fileNames.Add(skelFile);
	AssetToolsModule.Get().ImportAssets(fileNames, TargetPath);
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

bool USpineSkeletonAssetFactory::CanReimport (UObject* Obj, TArray<FString>& OutFilenames) {
	USpineSkeletonDataAsset* asset = Cast<USpineSkeletonDataAsset>(Obj);
	if (!asset) return false;
	
	FString filename = asset->GetSkeletonDataFileName().ToString();
	if (!filename.IsEmpty())
		OutFilenames.Add(filename);
	
	return true;
}

void USpineSkeletonAssetFactory::SetReimportPaths (UObject* Obj, const TArray<FString>& NewReimportPaths) {
	USpineSkeletonDataAsset* asset = Cast<USpineSkeletonDataAsset>(Obj);
	
	if (asset && ensure(NewReimportPaths.Num() == 1))
		asset->SetSkeletonDataFileName(FName(*NewReimportPaths[0]));
}

EReimportResult::Type USpineSkeletonAssetFactory::Reimport (UObject* Obj) {
	USpineSkeletonDataAsset* asset = Cast<USpineSkeletonDataAsset>(Obj);
	if (!FFileHelper::LoadFileToArray(asset->GetRawData(), *asset->GetSkeletonDataFileName().ToString(), 0)) return EReimportResult::Failed;
	
	const FString longPackagePath = FPackageName::GetLongPackagePath(asset->GetOutermost()->GetPathName());
	LoadAtlas(*asset->GetSkeletonDataFileName().ToString(), longPackagePath);
	
	if (Obj->GetOuter()) Obj->GetOuter()->MarkPackageDirty();
	else Obj->MarkPackageDirty();
	
	return EReimportResult::Succeeded;
}

#undef LOCTEXT_NAMESPACE
