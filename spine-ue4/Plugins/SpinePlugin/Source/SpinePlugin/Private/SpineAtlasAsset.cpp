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

#include "SpinePluginPrivatePCH.h"
#include "spine/spine.h"
#include <string.h>
#include <string>
#include <stdlib.h>

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

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

void USpineAtlasAsset::SetRawData (const FString &RawData) {
	this->rawData = RawData;
}

void USpineAtlasAsset::SetAtlasFileName (const FName &AtlasFileName) {
	importData->UpdateFilenameOnly(AtlasFileName.ToString());
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

#endif

void USpineAtlasAsset::BeginDestroy () {
	if (atlas) {
		delete atlas;
		atlas = nullptr;
	}
	Super::BeginDestroy();
}

Atlas* USpineAtlasAsset::GetAtlas (bool ForceReload) {
	if (!atlas || ForceReload) {
		if (atlas) {
			delete atlas;
			atlas = nullptr;
		}
		std::string t = TCHAR_TO_UTF8(*rawData);

		atlas = new (__FILE__, __LINE__) Atlas(t.c_str(), strlen(t.c_str()), "", nullptr);
		Vector<AtlasPage*> &pages = atlas->getPages();
		for (size_t i = 0, n = pages.size(), j = 0; i < n; i++) {
			AtlasPage* page = pages[i];
			if (atlasPages.Num() > 0 && atlasPages.Num() > i)
				page->setRendererObject(atlasPages[j++]);
		}
	}
	return this->atlas;
}

#undef LOCTEXT_NAMESPACE
