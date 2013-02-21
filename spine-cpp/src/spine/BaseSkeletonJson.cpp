#include <cstdlib>
#include <cstdio>
#include <stdexcept>
#include <json/json.h>
#include <spine/BaseSkeletonJson.h>
#include <spine/BaseAttachmentLoader.h>
#include <spine/SkeletonData.h>
#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/Skin.h>

using std::string;
using std::vector;
using std::runtime_error;

namespace spine {

BaseSkeletonJson::BaseSkeletonJson (BaseAttachmentLoader *attachmentLoader) :
				attachmentLoader(attachmentLoader),
				scale(1) {
}

BaseSkeletonJson::~BaseSkeletonJson () {
}

float toColor (const string &value, int index) {
	if (value.size() != 8) throw runtime_error("Error parsing color, length must be 8: " + value);
	char *p;
	int color = strtoul(value.substr(index * 2, 2).c_str(), &p, 16);
	if (*p != 0) throw runtime_error("Error parsing color: " + value + ", invalid hex value: " + value.substr(index * 2, 2));
	return color / (float)255;
}

SkeletonData* BaseSkeletonJson::readSkeletonData (std::istream &file) const {
	string json;
	std::getline(file, json, (char)EOF);
	return readSkeletonData(json);
}

SkeletonData* BaseSkeletonJson::readSkeletonData (const string &json) const {
	const char *begin = json.c_str();
	const char *end = begin + json.length();
	return readSkeletonData(begin, end);
}

SkeletonData* BaseSkeletonJson::readSkeletonData (const char *begin, const char *end) const {
	static string const ATTACHMENT_REGION = "region";
	static string const ATTACHMENT_REGION_SEQUENCE = "regionSequence";

	Json::Value root;
	Json::Reader reader;
	if (!reader.parse(begin, end, root)) throw runtime_error("Error parsing skeleton JSON.\n" + reader.getFormatedErrorMessages());

	SkeletonData *skeletonData = new SkeletonData();

	Json::Value bones = root["bones"];
	skeletonData->bones.reserve(bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Json::Value boneMap = bones[i];
		string boneName = boneMap["name"].asString();

		BoneData *boneData = new BoneData(boneName);
		if (boneMap.isMember("parent")) {
			boneData->parent = skeletonData->findBone(boneMap["parent"].asString());
			if (!boneData->parent) throw runtime_error("Parent bone not found: " + boneName);
		}

		boneData->length = boneMap.get("length", 0).asDouble() * scale;
		boneData->x = boneMap.get("x", 0).asDouble() * scale;
		boneData->y = boneMap.get("y", 0).asDouble() * scale;
		boneData->rotation = boneMap.get("rotation", 0).asDouble();
		boneData->scaleX = boneMap.get("scaleX", 1).asDouble();
		boneData->scaleY = boneMap.get("scaleY", 1).asDouble();

		skeletonData->bones.push_back(boneData);
	}

	if (root.isMember("slots")) {
		Json::Value slots = root["slots"];
		skeletonData->slots.reserve(slots.size());
		for (int i = 0; i < slots.size(); ++i) {
			Json::Value slotMap = slots[i];
			string slotName = slotMap["name"].asString();

			string boneName = slotMap["bone"].asString();
			BoneData* boneData = skeletonData->findBone(boneName);
			if (!boneData) throw runtime_error("Slot bone not found: " + boneName);

			SlotData *slotData = new SlotData(slotName, boneData);

			if (slotMap.isMember("color")) {
				string s = slotMap["color"].asString();
				slotData->r = toColor(s, 0);
				slotData->g = toColor(s, 1);
				slotData->b = toColor(s, 2);
				slotData->a = toColor(s, 3);
			}

			if (slotMap.isMember("attachment")) slotData->attachmentName = new string(slotMap["attachment"].asString());

			skeletonData->slots.push_back(slotData);
		}
	}

	if (root.isMember("skins")) {
		Json::Value skinsMap = root["skins"];
		vector<string> skinNames = skinsMap.getMemberNames();
		skeletonData->skins.reserve(skinNames.size());
		for (int i = 0; i < skinNames.size(); i++) {
			string skinName = skinNames[i];
			Skin *skin = new Skin(skinName);
			skeletonData->skins.push_back(skin);
			if (skinName == "default") skeletonData->defaultSkin = skin;

			Json::Value slotMap = skinsMap[skinName];
			vector<string> slotNames = slotMap.getMemberNames();
			for (int i = 0; i < slotNames.size(); i++) {
				string slotName = slotNames[i];
				int slotIndex = skeletonData->findSlotIndex(slotName);

				Json::Value attachmentsMap = slotMap[slotName];
				vector<string> attachmentNames = attachmentsMap.getMemberNames();
				for (int i = 0; i < attachmentNames.size(); i++) {
					string attachmentName = attachmentNames[i];
					Json::Value attachmentMap = attachmentsMap[attachmentName];

					AttachmentType type;
					string typeString = attachmentMap.get("type", ATTACHMENT_REGION).asString();
					if (typeString == ATTACHMENT_REGION)
						type = region;
					else if (typeString == ATTACHMENT_REGION_SEQUENCE)
						type = regionSequence;
					else
						throw runtime_error("Unknown attachment type: " + typeString + " (" + attachmentName + ")");

					Attachment* attachment = attachmentLoader->newAttachment(type);
					attachment->name = attachmentMap.get("name", attachmentName).asString();
					attachment->x = attachmentMap.get("x", 0).asDouble() * scale;
					attachment->y = attachmentMap.get("y", 0).asDouble() * scale;
					attachment->scaleX = attachmentMap.get("scaleX", 1).asDouble();
					attachment->scaleY = attachmentMap.get("scaleY", 1).asDouble();
					attachment->rotation = attachmentMap.get("rotation", 0).asDouble();
					attachment->width = attachmentMap.get("width", 32).asDouble() * scale;
					attachment->height = attachmentMap.get("height", 32).asDouble() * scale;

					skin->addAttachment(slotIndex, attachmentName, attachment);
				}
			}
		}
	}

	return skeletonData;
}

} /* namespace spine */
