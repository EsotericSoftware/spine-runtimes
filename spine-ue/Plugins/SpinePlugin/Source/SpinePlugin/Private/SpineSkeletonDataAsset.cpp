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

#include "SpineSkeletonDataAsset.h"
#include "EditorFramework/AssetImportData.h"
#include "Runtime/Core/Public/Misc/MessageDialog.h"
#include "SpinePlugin.h"
#include "spine/Version.h"
#include "spine/spine.h"
#include <cstring>
#include <string>

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

FName USpineSkeletonDataAsset::GetSkeletonDataFileName() const {
#if WITH_EDITORONLY_DATA
	TArray<FString> files;
	if (importData) importData->ExtractFilenames(files);
	if (files.Num() > 0)
		return FName(*files[0]);
	else
		return skeletonDataFileName;
#else
	return skeletonDataFileName;
#endif
}

#if WITH_EDITORONLY_DATA

void USpineSkeletonDataAsset::SetSkeletonDataFileName(const FName &SkeletonDataFileName) {
	skeletonDataFileName = SkeletonDataFileName;
	if (!importData) return;

	importData->UpdateFilenameOnly(SkeletonDataFileName.ToString());
	TArray<FString> files;
	importData->ExtractFilenames(files);
	if (files.Num() > 0) skeletonDataFileName = FName(*files[0]);
}

void USpineSkeletonDataAsset::UpdateSkeletonDataFileName(const FName &SkeletonDataFileName) {
	skeletonDataFileName = SkeletonDataFileName;
	if (!importData) return;

	importData->Update(SkeletonDataFileName.ToString());
	TArray<FString> files;
	importData->ExtractFilenames(files);
	if (files.Num() > 0) skeletonDataFileName = FName(*files[0]);
}

void USpineSkeletonDataAsset::PostInitProperties() {
	if (!HasAnyFlags(RF_ClassDefaultObject)) importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
	Super::PostInitProperties();
}

#if ((ENGINE_MAJOR_VERSION >= 5) && (ENGINE_MINOR_VERSION >= 4))
void USpineSkeletonDataAsset::GetAssetRegistryTags(FAssetRegistryTagsContext Context) const {
	if (importData) {
		Context.AddTag(FAssetRegistryTag(SourceFileTagName(), importData->GetSourceData().ToJson(), FAssetRegistryTag::TT_Hidden));
	}
	Super::GetAssetRegistryTags(Context);
}
#else
void USpineSkeletonDataAsset::GetAssetRegistryTags(TArray<FAssetRegistryTag> &OutTags) const {
	if (importData) {
		OutTags.Add(FAssetRegistryTag(SourceFileTagName(), importData->GetSourceData().ToJson(), FAssetRegistryTag::TT_Hidden));
	}

	Super::GetAssetRegistryTags(OutTags);
}
#endif

void USpineSkeletonDataAsset::Serialize(FArchive &Ar) {
	Super::Serialize(Ar);
#if ENGINE_MAJOR_VERSION == 4 && ENGINE_MINOR_VERSION <= 27
	if (Ar.IsLoading() && Ar.UE4Ver() < VER_UE4_ASSET_IMPORT_DATA_AS_JSON && !importData)
#else
	if (Ar.IsLoading() && Ar.UEVer() < VER_UE4_ASSET_IMPORT_DATA_AS_JSON && !importData)
#endif
		importData = NewObject<UAssetImportData>(this, TEXT("AssetImportData"));
}

#endif

void USpineSkeletonDataAsset::ClearNativeData() {
	for (auto &pair : atlasToNativeData) {
		if (pair.Value.skeletonData) delete pair.Value.skeletonData;
		if (pair.Value.animationStateData) delete pair.Value.animationStateData;
	}
	atlasToNativeData.Empty();
}

void USpineSkeletonDataAsset::BeginDestroy() {
	ClearNativeData();

	Super::BeginDestroy();
}

class SP_API NullAttachmentLoader : public AttachmentLoader {
public:
	virtual RegionAttachment *newRegionAttachment(Skin &skin, const String &placeholder, const String &name, const String &path, Sequence *sequence) {
		return new (__FILE__, __LINE__) RegionAttachment(name, sequence);
	}

	virtual MeshAttachment *newMeshAttachment(Skin &skin, const String &placeholder, const String &name, const String &path, Sequence *sequence) {
		return new (__FILE__, __LINE__) MeshAttachment(name, sequence);
	}

	virtual BoundingBoxAttachment *newBoundingBoxAttachment(Skin &skin, const String &placeholder, const String &name) {
		return new (__FILE__, __LINE__) BoundingBoxAttachment(name);
	}

	virtual PathAttachment *newPathAttachment(Skin &skin, const String &placeholder, const String &name) {
		return new (__FILE__, __LINE__) PathAttachment(name);
	}

	virtual PointAttachment *newPointAttachment(Skin &skin, const String &placeholder, const String &name) {
		return new (__FILE__, __LINE__) PointAttachment(name);
	}

	virtual ClippingAttachment *newClippingAttachment(Skin &skin, const String &placeholder, const String &name) {
		return new (__FILE__, __LINE__) ClippingAttachment(name);
	}

	virtual void configureAttachment(Attachment *attachment) {
	}
};

void USpineSkeletonDataAsset::SetRawData(TArray<uint8> &Data) {
#if WITH_EDITORONLY_DATA
	FString error;
	if (!SetRawDataFromImport(Data, error)) UE_LOG(SpineLog, Error, TEXT("%s"), *error);
#else
	rawData = Data;
	ClearNativeData();
#endif
}

static bool checkVersion(const char *version) {
	if (!version) return false;
	return strncmp(version, SPINE_VERSION_STRING, strlen(SPINE_VERSION_STRING)) == 0;
}

static bool checkJson(const char *jsonData) {
	if (!jsonData) return false;
	Json json(jsonData);
	Json *skeleton = Json::getItem(&json, "skeleton");
	if (!skeleton) return false;
	const char *version = Json::getString(skeleton, "spine", 0);
	if (!version) return false;

	return checkVersion(version);
}

struct BinaryInput {
	const unsigned char *cursor;
	const unsigned char *end;
};

static bool readVarint(BinaryInput &input, int &value) {
	uint32 result = 0;
	for (int shift = 0; shift <= 28; shift += 7) {
		if (input.cursor >= input.end) return false;
		const unsigned char b = *input.cursor++;
		result |= (uint32) (b & 0x7F) << shift;
		if (!(b & 0x80)) {
			value = (int) result;
			return true;
		}
	}
	return false;
}

static bool readString(BinaryInput &input, std::string &value) {
	int length;
	if (!readVarint(input, length) || length <= 0) return false;

	const int byteCount = length - 1;
	if (byteCount > input.end - input.cursor) return false;
	value.assign((const char *) input.cursor, byteCount);
	input.cursor += byteCount;
	return true;
}

static bool checkBinary(const char *binaryData, int length) {
	if (!binaryData || length < 9) return false;

	BinaryInput input;
	input.cursor = (const unsigned char *) binaryData + 8;// Skip hash.
	input.end = (const unsigned char *) binaryData + length;
	std::string version;
	return readString(input, version) && checkVersion(version.c_str());
}

static bool isJsonFile(const FName &fileName) {
	return FPaths::GetExtension(fileName.ToString()).Equals(TEXT("json"), ESearchCase::IgnoreCase);
}

#if WITH_EDITORONLY_DATA
bool USpineSkeletonDataAsset::SetRawDataFromImport(const TArray<uint8> &Data, FString &Error) {
	TArray<FString> newBones;
	TArray<FString> newSlots;
	TArray<FString> newSkins;
	TArray<FString> newAnimations;
	TArray<FString> newEvents;
	if (!LoadInfo(Data, newBones, newSlots, newSkins, newAnimations, newEvents, Error)) return false;

	rawData = Data;
	ClearNativeData();
	Bones = MoveTemp(newBones);
	Slots = MoveTemp(newSlots);
	Skins = MoveTemp(newSkins);
	Animations = MoveTemp(newAnimations);
	Events = MoveTemp(newEvents);
	return true;
}

bool USpineSkeletonDataAsset::LoadInfo(const TArray<uint8> &Data, TArray<FString> &OutBones, TArray<FString> &OutSlots, TArray<FString> &OutSkins,
									   TArray<FString> &OutAnimations, TArray<FString> &OutEvents, FString &Error) const {
	if (Data.Num() == 0) {
		Error = FString::Printf(TEXT("Couldn't load empty skeleton data: %s"), *skeletonDataFileName.ToString());
		return false;
	}
	NullAttachmentLoader loader;
	SkeletonData *skeletonData = nullptr;
	if (isJsonFile(skeletonDataFileName)) {
		TArray<uint8> terminatedData = Data;
		terminatedData.Add(0);
		const char *jsonData = (const char *) terminatedData.GetData();
		SkeletonJson *json = new (__FILE__, __LINE__) SkeletonJson(loader);
		if (checkJson(jsonData)) skeletonData = json->readSkeletonData(jsonData);
		if (!skeletonData && !json->getError().isEmpty()) Error = UTF8_TO_TCHAR(json->getError().buffer());
		delete json;
	} else {
		SkeletonBinary *binary = new (__FILE__, __LINE__) SkeletonBinary(loader);
		if (checkBinary((const char *) Data.GetData(), Data.Num()))
			skeletonData = binary->readSkeletonData((const unsigned char *) Data.GetData(), Data.Num());
		if (!skeletonData && !binary->getError().isEmpty()) Error = UTF8_TO_TCHAR(binary->getError().buffer());
		delete binary;
	}
	if (!skeletonData) {
		if (Error.IsEmpty()) Error = TEXT("The skeleton version does not match the runtime or the data is malformed.");
		Error = FString::Printf(TEXT("Couldn't load skeleton data %s. %s"), *skeletonDataFileName.ToString(), *Error);
		return false;
	}

	for (int i = 0; i < skeletonData->getBones().size(); i++) OutBones.Add(UTF8_TO_TCHAR(skeletonData->getBones()[i]->getName().buffer()));
	for (int i = 0; i < skeletonData->getSlots().size(); i++) OutSlots.Add(UTF8_TO_TCHAR(skeletonData->getSlots()[i]->getName().buffer()));
	for (int i = 0; i < skeletonData->getSkins().size(); i++) OutSkins.Add(UTF8_TO_TCHAR(skeletonData->getSkins()[i]->getName().buffer()));
	for (int i = 0; i < skeletonData->getAnimations().size(); i++)
		OutAnimations.Add(UTF8_TO_TCHAR(skeletonData->getAnimations()[i]->getName().buffer()));
	for (int i = 0; i < skeletonData->getEvents().size(); i++) OutEvents.Add(UTF8_TO_TCHAR(skeletonData->getEvents()[i]->getName().buffer()));
	delete skeletonData;
	return true;
}
#endif

SkeletonData *USpineSkeletonDataAsset::GetSkeletonData(Atlas *Atlas) {
	SkeletonData *skeletonData = nullptr;
	AnimationStateData *animationStateData = nullptr;
	if (atlasToNativeData.Contains(Atlas)) {
		skeletonData = atlasToNativeData[Atlas].skeletonData;
		animationStateData = atlasToNativeData[Atlas].animationStateData;
	}

	if (!skeletonData) {
		if (isJsonFile(skeletonDataFileName)) {
			TArray<uint8> terminatedData = rawData;
			terminatedData.Add(0);
			const char *jsonData = (const char *) terminatedData.GetData();
			SkeletonJson *json = new (__FILE__, __LINE__) SkeletonJson(*Atlas);
			if (checkJson(jsonData)) skeletonData = json->readSkeletonData(jsonData);
			if (!skeletonData) {
#if WITH_EDITORONLY_DATA
				FMessageDialog::Debugf(FText::FromString(FString("Couldn't load skeleton data and/or atlas. Please ensure "
																 "the version of your exported data matches your runtime "
																 "version.\n\n") +
														 skeletonDataFileName.GetPlainNameString() + FString("\n\n") +
														 UTF8_TO_TCHAR(json->getError().buffer())));
#endif
				UE_LOG(SpineLog, Error, TEXT("Couldn't load skeleton data and atlas: %s"), UTF8_TO_TCHAR(json->getError().buffer()));
			}
			delete json;
		} else {
			SkeletonBinary *binary = new (__FILE__, __LINE__) SkeletonBinary(*Atlas);
			if (checkBinary((const char *) rawData.GetData(), (int) rawData.Num()))
				skeletonData = binary->readSkeletonData((const unsigned char *) rawData.GetData(), (int) rawData.Num());
			if (!skeletonData) {
#if WITH_EDITORONLY_DATA
				FMessageDialog::Debugf(FText::FromString(FString("Couldn't load skeleton data and/or atlas. Please ensure "
																 "the version of your exported data matches your runtime "
																 "version.\n\n") +
														 skeletonDataFileName.GetPlainNameString() + FString("\n\n") +
														 UTF8_TO_TCHAR(binary->getError().buffer())));
#endif
				UE_LOG(SpineLog, Error, TEXT("Couldn't load skeleton data and atlas: %s"), UTF8_TO_TCHAR(binary->getError().buffer()));
			}
			delete binary;
		}

		if (skeletonData) {
			animationStateData = new (__FILE__, __LINE__) AnimationStateData(*skeletonData);
			SetMixes(animationStateData);
			atlasToNativeData.Add(Atlas, {skeletonData, animationStateData});
		}
	}

	return skeletonData;
}

void USpineSkeletonDataAsset::SetMixes(AnimationStateData *animationStateData) {
	for (auto &data : MixData) {
		if (!data.From.IsEmpty() && !data.To.IsEmpty()) {
			std::string fromChar = TCHAR_TO_UTF8(*data.From);
			std::string toChar = TCHAR_TO_UTF8(*data.To);
			animationStateData->setMix(fromChar.c_str(), toChar.c_str(), data.Mix);
		}
	}
	animationStateData->setDefaultMix(DefaultMix);
}

AnimationStateData *USpineSkeletonDataAsset::GetAnimationStateData(Atlas *atlas) {
	if (!atlasToNativeData.Contains(atlas)) return nullptr;
	AnimationStateData *data = atlasToNativeData[atlas].animationStateData;
	SetMixes(data);
	return data;
}

void USpineSkeletonDataAsset::SetMix(const FString &from, const FString &to, float mix) {
	FSpineAnimationStateMixData data;
	data.From = from;
	data.To = to;
	data.Mix = mix;
	this->MixData.Add(data);
	for (auto &pair : atlasToNativeData) {
		SetMixes(pair.Value.animationStateData);
	}
}

float USpineSkeletonDataAsset::GetMix(const FString &from, const FString &to) {
	for (auto &data : MixData) {
		if (data.From.Equals(from) && data.To.Equals(to)) return data.Mix;
	}
	return 0;
}

#undef LOCTEXT_NAMESPACE
