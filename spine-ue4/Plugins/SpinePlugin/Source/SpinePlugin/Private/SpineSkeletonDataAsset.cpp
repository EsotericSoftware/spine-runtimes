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

FName USpineSkeletonDataAsset::GetSkeletonDataFileName () const {
#if WITH_EDITORONLY_DATA
	TArray<FString> files;
	if (importData) importData->ExtractFilenames(files);
	if (files.Num() > 0) return FName(*files[0]);
	else return skeletonDataFileName;
#else
	return skeletonDataFileName;
#endif
}

TArray<uint8>& USpineSkeletonDataAsset::GetRawData () {
	return this->rawData;
}

#if WITH_EDITORONLY_DATA

void USpineSkeletonDataAsset::SetSkeletonDataFileName (const FName &SkeletonDataFileName) {
	importData->UpdateFilenameOnly(SkeletonDataFileName.ToString());
	TArray<FString> files;
	importData->ExtractFilenames(files);
	if (files.Num() > 0) this->skeletonDataFileName = FName(*files[0]);
}

void USpineSkeletonDataAsset::PostInitProperties () {
	if (!HasAnyFlags(RF_ClassDefaultObject)) importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
	Super::PostInitProperties();
}

void USpineSkeletonDataAsset::GetAssetRegistryTags (TArray<FAssetRegistryTag>& OutTags) const {
	if (importData) {
		OutTags.Add(FAssetRegistryTag(SourceFileTagName(), importData->GetSourceData().ToJson(), FAssetRegistryTag::TT_Hidden) );
	}
	
	Super::GetAssetRegistryTags(OutTags);
}

void USpineSkeletonDataAsset::Serialize (FArchive& Ar) {
	Super::Serialize(Ar);
	if (Ar.IsLoading() && Ar.UE4Ver() < VER_UE4_ASSET_IMPORT_DATA_AS_JSON && !importData)
		importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
}

#endif

void USpineSkeletonDataAsset::BeginDestroy () {
	if (this->skeletonData) {
		spSkeletonData_dispose(this->skeletonData);
		this->skeletonData = nullptr;
	}
	if (this->animationStateData) {
		spAnimationStateData_dispose(this->animationStateData);
		this->animationStateData = nullptr;
	}
	Super::BeginDestroy();
}

spSkeletonData* USpineSkeletonDataAsset::GetSkeletonData (spAtlas* Atlas, bool ForceReload) {
	if (!skeletonData || ForceReload) {
		if (skeletonData) {
			spSkeletonData_dispose(skeletonData);
			skeletonData = nullptr;
		}		
		int dataLen = rawData.Num();
		if (skeletonDataFileName.GetPlainNameString().Contains(TEXT(".json"))) {
			spSkeletonJson* json = spSkeletonJson_create(Atlas);
			this->skeletonData = spSkeletonJson_readSkeletonData(json, (const char*)rawData.GetData());
			spSkeletonJson_dispose(json);
		} else {
			spSkeletonBinary* binary = spSkeletonBinary_create(Atlas);
			this->skeletonData = spSkeletonBinary_readSkeletonData(binary, (const unsigned char*)rawData.GetData(), (int)rawData.Num());
			spSkeletonBinary_dispose(binary);
		}
		if (animationStateData) {
			spAnimationStateData_dispose(animationStateData);
			GetAnimationStateData(Atlas);
		}
		lastAtlas = Atlas;
	}
	return this->skeletonData;
}

spAnimationStateData* USpineSkeletonDataAsset::GetAnimationStateData(spAtlas* atlas) {
	if (!animationStateData) {
		spSkeletonData* skeletonData = GetSkeletonData(atlas, false);
		animationStateData = spAnimationStateData_create(skeletonData);
	}
	for (auto& data : MixData) {
		if (!data.From.IsEmpty() && !data.To.IsEmpty()) {
			const char* fromChar = TCHAR_TO_UTF8(*data.From);
			const char* toChar = TCHAR_TO_UTF8(*data.To);
			spAnimationStateData_setMixByName(animationStateData, fromChar, toChar, data.Mix);
		}
	}
	animationStateData->defaultMix = DefaultMix;
	return this->animationStateData;
}

void USpineSkeletonDataAsset::SetMix(const FString& from, const FString& to, float mix) {
	FSpineAnimationStateMixData data;
	data.From = from;
	data.To = to;
	data.Mix = mix;	
	this->MixData.Add(data);
	if (lastAtlas) {
		GetAnimationStateData(lastAtlas);
	}
}

float USpineSkeletonDataAsset::GetMix(const FString& from, const FString& to) {
	for (auto& data : MixData) {
		if (data.From.Equals(from) && data.To.Equals(to)) return data.Mix;
	}
	return 0;
}

#undef LOCTEXT_NAMESPACE
