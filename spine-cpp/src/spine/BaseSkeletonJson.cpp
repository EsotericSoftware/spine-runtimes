#include <cstdlib>
#include <fstream>
#include <stdexcept>
#include <json/json.h>
#include <spine/BaseSkeletonJson.h>
#include <spine/BaseAttachmentLoader.h>
#include <spine/BaseRegionAttachment.h>
#include <spine/SkeletonData.h>
#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/Skin.h>
#include <spine/Animation.h>

using std::string;
using std::vector;
using std::runtime_error;
using std::invalid_argument;

namespace spine {

static float toColor (const string &value, int index) {
	if (value.size() != 8) throw runtime_error("Error parsing color, length must be 8: " + value);
	char *p;
	int color = strtoul(value.substr(index * 2, 2).c_str(), &p, 16);
	if (*p != 0) throw runtime_error("Error parsing color: " + value + ", invalid hex value: " + value.substr(index * 2, 2));
	return color / (float)255;
}

//

BaseSkeletonJson::BaseSkeletonJson (BaseAttachmentLoader *attachmentLoader) :
				attachmentLoader(attachmentLoader),
				scale(1),
				flipY(false) {
}

BaseSkeletonJson::~BaseSkeletonJson () {
  if (attachmentLoader)
    delete attachmentLoader;
}

SkeletonData* BaseSkeletonJson::readSkeletonData (std::ifstream &file) const {
	if (!file) throw invalid_argument("file cannot be null.");
	if (!file.is_open()) throw runtime_error("Skeleton file is not open.");

	return readSkeletonData((std::istream&)file);
}

SkeletonData* BaseSkeletonJson::readSkeletonData (std::istream &input) const {
	if (!input) throw invalid_argument("input cannot be null.");

	string json;
	std::getline(input, json, (char)EOF);
	return readSkeletonData(json);
}

SkeletonData* BaseSkeletonJson::readSkeletonData (const string &json) const {
	const char *begin = json.c_str();
	const char *end = begin + json.length();
	return readSkeletonData(begin, end);
}

SkeletonData* BaseSkeletonJson::readSkeletonData (const char *begin, const char *end) const {
	if (!begin) throw invalid_argument("begin cannot be null.");
	if (!end) throw invalid_argument("end cannot be null.");

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
		boneData->flipY = flipY;

		skeletonData->bones.push_back(boneData);
	}

	Json::Value slots = root["slots"];
	if (!slots.isNull()) {
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

					Attachment* attachment = attachmentLoader->newAttachment(type,
							attachmentMap.get("name", attachmentName).asString());

					if (type == region || type == regionSequence) {
						BaseRegionAttachment *regionAttachment = reinterpret_cast<BaseRegionAttachment*>(attachment);
						regionAttachment->x = attachmentMap.get("x", 0).asDouble() * scale;
						regionAttachment->y = attachmentMap.get("y", 0).asDouble() * scale;
						regionAttachment->scaleX = attachmentMap.get("scaleX", 1).asDouble();
						regionAttachment->scaleY = attachmentMap.get("scaleY", 1).asDouble();
						regionAttachment->rotation = attachmentMap.get("rotation", 0).asDouble();
						regionAttachment->width = attachmentMap.get("width", 32).asDouble() * scale;
						regionAttachment->height = attachmentMap.get("height", 32).asDouble() * scale;
					}

					skin->addAttachment(slotIndex, attachmentName, attachment);
				}
			}
		}
	}

	return skeletonData;
}

Animation* BaseSkeletonJson::readAnimation (std::ifstream &file, const SkeletonData *skeletonData) const {
	if (!file) throw invalid_argument("file cannot be null.");
	if (!file.is_open()) throw runtime_error("Animation file is not open.");

	return readAnimation((std::istream&)file, skeletonData);
}

Animation* BaseSkeletonJson::readAnimation (std::istream &input, const SkeletonData *skeletonData) const {
	if (!input) throw invalid_argument("input cannot be null.");

	string json;
	std::getline(input, json, (char)EOF);
	return readAnimation(json, skeletonData);
}

Animation* BaseSkeletonJson::readAnimation (const string &json, const SkeletonData *skeletonData) const {
	const char *begin = json.c_str();
	const char *end = begin + json.length();
	return readAnimation(begin, end, skeletonData);
}

static void readCurve (CurveTimeline *timeline, int keyframeIndex, const Json::Value &valueMap) {
	Json::Value curve = valueMap["curve"];
	if (curve.isNull()) return;
	if (curve.isString() && curve.asString() == "stepped")
		timeline->setStepped(keyframeIndex);
	else if (curve.isArray())
		timeline->setCurve(keyframeIndex, curve[0u].asDouble(), curve[1u].asDouble(), curve[2u].asDouble(), curve[3u].asDouble());
}

Animation* BaseSkeletonJson::readAnimation (const char *begin, const char *end, const SkeletonData *skeletonData) const {
	if (!begin) throw invalid_argument("begin cannot be null.");
	if (!end) throw invalid_argument("end cannot be null.");
	if (!skeletonData) throw invalid_argument("skeletonData cannot be null.");

	static string const TIMELINE_SCALE = "scale";
	static string const TIMELINE_ROTATE = "rotate";
	static string const TIMELINE_TRANSLATE = "translate";
	static string const TIMELINE_ATTACHMENT = "attachment";
	static string const TIMELINE_COLOR = "color";

	vector<Timeline*> timelines;
	float duration = 0;

	Json::Value root;
	Json::Reader reader;
	if (!reader.parse(begin, end, root))
		throw runtime_error("Error parsing animation JSON.\n" + reader.getFormatedErrorMessages());

	Json::Value bones = root["bones"];
	vector<string> boneNames = bones.getMemberNames();
	for (int i = 0; i < boneNames.size(); i++) {
		string boneName = boneNames[i];
		int boneIndex = skeletonData->findBoneIndex(boneName);
		if (boneIndex == -1) throw runtime_error("Bone not found: " + boneName);

		Json::Value timelineMap = bones[boneName];
		vector<string> timelineNames = timelineMap.getMemberNames();
		for (int i = 0; i < timelineNames.size(); i++) {
			string timelineName = timelineNames[i];
			Json::Value values = timelineMap[timelineName];

			if (timelineName == TIMELINE_ROTATE) {
				RotateTimeline *timeline = new RotateTimeline(values.size());
				timeline->boneIndex = boneIndex;

				int keyframeIndex = 0;
				for (int i = 0; i < values.size(); i++) {
					Json::Value valueMap = values[i];

					float time = valueMap["time"].asDouble();
					timeline->setKeyframe(keyframeIndex, time, valueMap["angle"].asDouble());
					readCurve(timeline, keyframeIndex, valueMap);
					keyframeIndex++;
				}
				timelines.push_back(timeline);
				if (timeline->getDuration() > duration) duration = timeline->getDuration();

			} else if (timelineName == TIMELINE_TRANSLATE || timelineName == TIMELINE_SCALE) {
				TranslateTimeline *timeline;
				float timelineScale = 1;
				if (timelineName == TIMELINE_SCALE)
					timeline = new ScaleTimeline(values.size());
				else {
					timeline = new TranslateTimeline(values.size());
					timelineScale = scale;
				}
				timeline->boneIndex = boneIndex;

				int keyframeIndex = 0;
				for (int i = 0; i < values.size(); i++) {
					Json::Value valueMap = values[i];

					float time = valueMap["time"].asDouble();
					timeline->setKeyframe(keyframeIndex, //
							valueMap["time"].asDouble(), //
							valueMap.get("x", 0).asDouble() * timelineScale, //
							valueMap.get("y", 0).asDouble() * timelineScale);
					readCurve(timeline, keyframeIndex, valueMap);
					keyframeIndex++;
				}
				timelines.push_back(timeline);
				if (timeline->getDuration() > duration) duration = timeline->getDuration();

			} else {
				throw runtime_error("Invalid timeline type for a bone: " + timelineName + " (" + boneName + ")");
			}
		}
	}

	Json::Value slots = root["slots"];
	if (!slots.isNull()) {
		vector<string> slotNames = slots.getMemberNames();
		for (int i = 0; i < slotNames.size(); i++) {
			string slotName = slotNames[i];
			int slotIndex = skeletonData->findSlotIndex(slotName);
			if (slotIndex == -1) throw runtime_error("Slot not found: " + slotName);

			Json::Value timelineMap = slots[slotName];
			vector<string> timelineNames = timelineMap.getMemberNames();
			for (int i = 0; i < timelineNames.size(); i++) {
				string timelineName = timelineNames[i];
				Json::Value values = timelineMap[timelineName];

				if (timelineName == TIMELINE_COLOR) {
					ColorTimeline *timeline = new ColorTimeline(values.size());
					timeline->slotIndex = slotIndex;

					int keyframeIndex = 0;
					for (int i = 0; i < values.size(); i++) {
						Json::Value valueMap = values[i];

						string s = valueMap["color"].asString();
						timeline->setKeyframe(keyframeIndex, valueMap["time"].asDouble(), //
								toColor(s, 0), toColor(s, 1), toColor(s, 2), toColor(s, 3));
						readCurve(timeline, keyframeIndex, valueMap);
						keyframeIndex++;
					}
					timelines.push_back(timeline);
					if (timeline->getDuration() > duration) duration = timeline->getDuration();

				} else if (timelineName == TIMELINE_ATTACHMENT) {
					AttachmentTimeline *timeline = new AttachmentTimeline(values.size());
					timeline->slotIndex = slotIndex;

					int keyframeIndex = 0;
					for (int i = 0; i < values.size(); i++) {
						Json::Value valueMap = values[i];

						Json::Value name = valueMap["name"];
						timeline->setKeyframe(keyframeIndex++, valueMap["time"].asDouble(), name.isNull() ? "" : name.asString());
					}
					timelines.push_back(timeline);
					if (timeline->getDuration() > duration) duration = timeline->getDuration();

				} else {
					throw runtime_error("Invalid timeline type for a slot: " + timelineName + " (" + slotName + ")");
				}
			}
		}
	}

	Animation *animation = new Animation(timelines, duration);
	return animation;
}

} /* namespace spine */
