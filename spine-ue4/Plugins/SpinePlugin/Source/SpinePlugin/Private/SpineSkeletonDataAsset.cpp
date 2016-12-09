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

#endif

#undef LOCTEXT_NAMESPACE
