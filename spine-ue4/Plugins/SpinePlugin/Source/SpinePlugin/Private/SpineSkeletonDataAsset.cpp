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

#include "SpinePluginPrivatePCH.h"
#include "spine/spine.h"
#include <string.h>
#include <string>
#include <stdlib.h>
#include "Runtime/Core/Public/Misc/MessageDialog.h"

#define LOCTEXT_NAMESPACE "Spine"

using namespace spine;

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
	LoadInfo();
}

#endif

void USpineSkeletonDataAsset::BeginDestroy () {
	if (this->skeletonData) {
		delete this->skeletonData;
		this->skeletonData = nullptr;
	}
	if (this->animationStateData) {
		delete this->animationStateData;
		this->animationStateData = nullptr;
	}
	Super::BeginDestroy();
}

class SP_API NullAttachmentLoader : public AttachmentLoader {
public:

	virtual RegionAttachment* newRegionAttachment(Skin& skin, const String& name, const String& path) {
		return new(__FILE__, __LINE__) RegionAttachment(name);
	}

	virtual MeshAttachment* newMeshAttachment(Skin& skin, const String& name, const String& path) {
		return new(__FILE__, __LINE__) MeshAttachment(name);
	}

	virtual BoundingBoxAttachment* newBoundingBoxAttachment(Skin& skin, const String& name) {
		return new(__FILE__, __LINE__) BoundingBoxAttachment(name);
	}

	virtual PathAttachment* newPathAttachment(Skin& skin, const String& name) {
		return new(__FILE__, __LINE__) PathAttachment(name);
	}

	virtual PointAttachment* newPointAttachment(Skin& skin, const String& name) {
		return new(__FILE__, __LINE__) PointAttachment(name);
	}

	virtual ClippingAttachment* newClippingAttachment(Skin& skin, const String& name) {
		return new(__FILE__, __LINE__) ClippingAttachment(name);
	}

	virtual void configureAttachment(Attachment* attachment) {

	}
};

void USpineSkeletonDataAsset::SetRawData(TArray<uint8> &Data) {
	this->rawData.Empty();
	this->rawData.Append(Data);

	if (skeletonData) {
		delete skeletonData;
		skeletonData = nullptr;
	}

	LoadInfo();
}

static bool checkVersion(const char* version) {
	String tokens[3];
	int currToken = 0;

	while (*version && currToken < 3) {
		if (*version == '.') {
			version++;
			currToken++;
			continue;
		}

		char str[2];
		str[0] = *version;
		str[1] = 0;
		tokens[currToken].append(str);
		version++;
	}
	int versionNumber[3];
	for (int i = 0; i < 3; i++)
		versionNumber[i] = atoi(tokens[i].buffer());

	return versionNumber[0] >= 3 && versionNumber[1] >= 8 && versionNumber[2] >= 12;
}

static bool checkJson(const char* jsonData) {
	Json json(jsonData);
	Json* skeleton = Json::getItem(&json, "skeleton");
	if (!skeleton) return false;
	const char* version = Json::getString(skeleton, "spine", 0);
	if (!version) return false;

	return checkVersion(version);
}

struct BinaryInput {
	const unsigned char* cursor;
	const unsigned char* end;
};

static unsigned char readByte(BinaryInput *input) {
	return *input->cursor++;
}

static int readVarint(BinaryInput *input, bool optimizePositive) {
	unsigned char b = readByte(input);
	int value = b & 0x7F;
	if (b & 0x80) {
		b = readByte(input);
		value |= (b & 0x7F) << 7;
		if (b & 0x80) {
			b = readByte(input);
			value |= (b & 0x7F) << 14;
			if (b & 0x80) {
				b = readByte(input);
				value |= (b & 0x7F) << 21;
				if (b & 0x80) value |= (readByte(input) & 0x7F) << 28;
			}
		}
	}

	if (!optimizePositive) {
		value = (((unsigned int)value >> 1) ^ -(value & 1));
	}

	return value;
}

static char *readString(BinaryInput *input) {
	int length = readVarint(input, true);
	char *string;
	if (length == 0) {
		return NULL;
	}
	string = SpineExtension::alloc<char>(length, __FILE__, __LINE__);
	memcpy(string, input->cursor, length - 1);
	input->cursor += length - 1;
	string[length - 1] = '\0';
	return string;
}

static bool checkBinary(const char* binaryData, int length) {
	BinaryInput input;
	input.cursor = (const unsigned char*)binaryData;
	input.end = (const unsigned char*)binaryData + length;
	SpineExtension::free(readString(&input), __FILE__, __LINE__);
	char* version = readString(&input);
	bool result = checkVersion(version);
	SpineExtension::free(version, __FILE__, __LINE__);
	return result;
}

void USpineSkeletonDataAsset::LoadInfo() {
#if WITH_EDITORONLY_DATA
	int dataLen = rawData.Num();
	if (dataLen == 0) return;
	NullAttachmentLoader loader;
	SkeletonData* skeletonData = nullptr;
	if (skeletonDataFileName.GetPlainNameString().Contains(TEXT(".json"))) {
		SkeletonJson* json = new (__FILE__, __LINE__) SkeletonJson(&loader);
		if(checkJson((const char*)rawData.GetData())) skeletonData = json->readSkeletonData((const char*)rawData.GetData());
		if (!skeletonData) {
			FMessageDialog::Debugf(FText::FromString(FString("Couldn't load skeleton data and/or atlas. Please ensure the version of your exported data matches your runtime version.\n\n") + skeletonDataFileName.GetPlainNameString() + FString("\n\n") + UTF8_TO_TCHAR(json->getError().buffer())));
			UE_LOG(SpineLog, Error, TEXT("Couldn't load skeleton data and atlas: %s"), UTF8_TO_TCHAR(json->getError().buffer()));
		}
		delete json;
	}
	else {
		SkeletonBinary* binary = new (__FILE__, __LINE__) SkeletonBinary(&loader);
		if (checkBinary((const char*)rawData.GetData(), (int)rawData.Num())) skeletonData = binary->readSkeletonData((const unsigned char*)rawData.GetData(), (int)rawData.Num());
		if (!skeletonData) {
			FMessageDialog::Debugf(FText::FromString(FString("Couldn't load skeleton data and/or atlas. Please ensure the version of your exported data matches your runtime version.\n\n") + skeletonDataFileName.GetPlainNameString() + FString("\n\n") + UTF8_TO_TCHAR(binary->getError().buffer())));
			UE_LOG(SpineLog, Error, TEXT("Couldn't load skeleton data and atlas: %s"), UTF8_TO_TCHAR(binary->getError().buffer()));
		}
		delete binary;
	}
	if (skeletonData) {
		Bones.Empty();
		for (int i = 0; i < skeletonData->getBones().size(); i++)
			Bones.Add(UTF8_TO_TCHAR(skeletonData->getBones()[i]->getName().buffer()));
		Skins.Empty();
		for (int i = 0; i < skeletonData->getSkins().size(); i++)
			Skins.Add(UTF8_TO_TCHAR(skeletonData->getSkins()[i]->getName().buffer()));
		Slots.Empty();
		for (int i = 0; i < skeletonData->getSlots().size(); i++)
			Slots.Add(UTF8_TO_TCHAR(skeletonData->getSlots()[i]->getName().buffer()));
		Animations.Empty();
		for (int i = 0; i < skeletonData->getAnimations().size(); i++)
			Animations.Add(UTF8_TO_TCHAR(skeletonData->getAnimations()[i]->getName().buffer()));
		Events.Empty();
		for (int i = 0; i < skeletonData->getEvents().size(); i++)
			Events.Add(UTF8_TO_TCHAR(skeletonData->getEvents()[i]->getName().buffer()));
		delete skeletonData;
	}
#endif
}

SkeletonData* USpineSkeletonDataAsset::GetSkeletonData (Atlas* Atlas) {
	if (!skeletonData || lastAtlas != Atlas) {
		if (skeletonData) {
			delete skeletonData;
			skeletonData = nullptr;
		}		
		int dataLen = rawData.Num();
		if (skeletonDataFileName.GetPlainNameString().Contains(TEXT(".json"))) {
			SkeletonJson* json = new (__FILE__, __LINE__) SkeletonJson(Atlas);
			if (checkJson((const char*)rawData.GetData())) this->skeletonData = json->readSkeletonData((const char*)rawData.GetData());
			if (!skeletonData) {
#if WITH_EDITORONLY_DATA
				FMessageDialog::Debugf(FText::FromString(FString("Couldn't load skeleton data and/or atlas. Please ensure the version of your exported data matches your runtime version.\n\n") + skeletonDataFileName.GetPlainNameString() + FString("\n\n") + UTF8_TO_TCHAR(json->getError().buffer())));
#endif
				UE_LOG(SpineLog, Error, TEXT("Couldn't load skeleton data and atlas: %s"), UTF8_TO_TCHAR(json->getError().buffer()));
			}
			delete json;
		} else {
			SkeletonBinary* binary = new (__FILE__, __LINE__) SkeletonBinary(Atlas);
			if (checkBinary((const char*)rawData.GetData(), (int)rawData.Num())) this->skeletonData = binary->readSkeletonData((const unsigned char*)rawData.GetData(), (int)rawData.Num());
			if (!skeletonData) {
#if WITH_EDITORONLY_DATA
				FMessageDialog::Debugf(FText::FromString(FString("Couldn't load skeleton data and/or atlas. Please ensure the version of your exported data matches your runtime version.\n\n") + skeletonDataFileName.GetPlainNameString() + FString("\n\n") + UTF8_TO_TCHAR(binary->getError().buffer())));
#endif
				UE_LOG(SpineLog, Error, TEXT("Couldn't load skeleton data and atlas: %s"), UTF8_TO_TCHAR(binary->getError().buffer()));
			}
			delete binary;
		}
		if (animationStateData) {
			delete animationStateData;
			animationStateData = nullptr;
			GetAnimationStateData(Atlas);
		}
		lastAtlas = Atlas;
	}
	return this->skeletonData;
}

AnimationStateData* USpineSkeletonDataAsset::GetAnimationStateData(Atlas* atlas) {
	if (!animationStateData) {
		SkeletonData* data = GetSkeletonData(atlas);
		animationStateData = new (__FILE__, __LINE__) AnimationStateData(data);
	}
	for (auto& data : MixData) {
		if (!data.From.IsEmpty() && !data.To.IsEmpty()) {
			const char* fromChar = TCHAR_TO_UTF8(*data.From);
			const char* toChar = TCHAR_TO_UTF8(*data.To);
			animationStateData->setMix(fromChar, toChar, data.Mix);
		}
	}
	animationStateData->setDefaultMix(DefaultMix);
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
