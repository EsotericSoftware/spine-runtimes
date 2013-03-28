#include <spine/SkeletonJson.h>
#include <math.h>
#include <spine/util.h>
#include <spine/cJSON.h>
#include <spine/RegionAttachment.h>

#include <stdio.h>

static float scale;
static const char* error;

void SkeletonJson_setScale (float value) {
	scale = value;
}

void* SkeletonJson_setError (cJSON* root, const char* value1, const char* value2) {
	FREE(error)
	char message[256];
	strcpy(message, value1);
	int length = strlen(value1);
	if (value2) strncat(message + length, value2, 256 - length);
	MALLOC_STR(error, message)
	if (root) cJSON_dispose(root);
	return 0;
}

const char* SkeletonJson_getError () {
	return error;
}

static float toColor (const char* value, int index) {
	if (strlen(value) != 8) return -1;
	value += index * 2;
	char digits[3];
	digits[0] = *value;
	digits[1] = *(value + 1);
	digits[2] = '\0';
	char* error;
	int color = strtoul(digits, &error, 16);
	if (*error != 0) return -1;
	return color / (float)255;
}

SkeletonData* SkeletonJson_readSkeletonDataFile (const char* path) {
	const char* data = readFile(path);
	if (!data) return SkeletonJson_setError(0, "Unable to read file: ", path);
	SkeletonData* skeletonData = SkeletonJson_readSkeletonData(data);
	FREE(data)
	return skeletonData;
}

SkeletonData* SkeletonJson_readSkeletonData (const char* json) {
	FREE(error)
	error = 0;

	cJSON* root = cJSON_Parse(json);
	if (!root) return SkeletonJson_setError(0, "Invalid JSON: ", cJSON_GetErrorPtr());

	SkeletonData* skeletonData = SkeletonData_create();
	int i, ii, iii;

	cJSON* bones = cJSON_GetObjectItem(root, "bones");
	int boneCount = cJSON_GetArraySize(bones);
	skeletonData->bones = malloc(sizeof(BoneData*) * boneCount);
	for (i = 0; i < boneCount; ++i) {
		cJSON* boneMap = cJSON_GetArrayItem(bones, i);

		const char* boneName = cJSON_GetObjectString(boneMap, "name", 0);

		BoneData* parent = 0;
		const char* parentName = cJSON_GetObjectString(boneMap, "parent", 0);
		if (parentName) {
			parent = SkeletonData_findBone(skeletonData, parentName);
			if (!parent) return SkeletonJson_setError(root, "Parent bone not found: ", parentName);
		}

		BoneData* boneData = BoneData_create(boneName, parent);
		boneData->length = cJSON_GetObjectFloat(boneMap, "parent", 0) * scale;
		boneData->x = cJSON_GetObjectFloat(boneMap, "x", 0) * scale;
		boneData->y = cJSON_GetObjectFloat(boneMap, "y", 0) * scale;
		boneData->rotation = cJSON_GetObjectFloat(boneMap, "rotation", 0);
		boneData->scaleX = cJSON_GetObjectFloat(boneMap, "scaleX", 1);
		boneData->scaleY = cJSON_GetObjectFloat(boneMap, "scaleY", 1);

		skeletonData->bones[i] = boneData;
		skeletonData->boneCount++;
	}

	cJSON* slots = cJSON_GetObjectItem(root, "slots");
	if (slots) {
		int slotCount = cJSON_GetArraySize(slots);
		skeletonData->slots = malloc(sizeof(SlotData*) * slotCount);
		for (i = 0; i < slotCount; ++i) {
			cJSON* slotMap = cJSON_GetArrayItem(slots, i);

			const char* slotName = cJSON_GetObjectString(slotMap, "name", 0);

			const char* boneName = cJSON_GetObjectString(slotMap, "bone", 0);
			BoneData* boneData = SkeletonData_findBone(skeletonData, boneName);
			if (!boneData) return SkeletonJson_setError(root, "Slot bone not found: ", boneName);

			SlotData* slotData = SlotData_create(slotName, boneData);

			const char* color = cJSON_GetObjectString(slotMap, "color", 0);
			if (color) {
				slotData->r = toColor(color, 0);
				slotData->g = toColor(color, 1);
				slotData->b = toColor(color, 2);
				slotData->a = toColor(color, 3);
			}

			cJSON *attachmentItem = cJSON_GetObjectItem(slotMap, "attachment");
			if (attachmentItem) SlotData_setAttachmentName(slotData, attachmentItem->valuestring);

			skeletonData->slots[i] = slotData;
			skeletonData->slotCount++;
		}
	}

	cJSON* skinsMap = cJSON_GetObjectItem(root, "skins");
	if (skinsMap) {
		int skinCount = cJSON_GetArraySize(skinsMap);
		skeletonData->skins = malloc(sizeof(Skin*) * skinCount);
		for (i = 0; i < skinCount; ++i) {
			cJSON* slotMap = cJSON_GetArrayItem(skinsMap, i);
			const char* skinName = slotMap->name;
			Skin *skin = Skin_create(skinName);
			skeletonData->skins[i] = skin;
			skeletonData->skinCount++;
			if (strcmp(skinName, "default") == 0) skeletonData->defaultSkin = skin;

			int slotNameCount = cJSON_GetArraySize(slotMap);
			for (ii = 0; ii < slotNameCount; ++ii) {
				cJSON* attachmentsMap = cJSON_GetArrayItem(slotMap, ii);
				const char* slotName = attachmentsMap->name;
				int slotIndex = SkeletonData_findSlotIndex(skeletonData, slotName);

				int attachmentCount = cJSON_GetArraySize(attachmentsMap);
				for (iii = 0; iii < attachmentCount; ++iii) {
					cJSON* attachmentMap = cJSON_GetArrayItem(attachmentsMap, iii);
					const char* attachmentName = attachmentMap->name;

					const char* typeString = cJSON_GetObjectString(attachmentMap, "type", "region");
					AttachmentType type;
					if (strcmp(typeString, "region") == 0)
						type = ATTACHMENT_REGION;
					else if (strcmp(typeString, "regionSequence") == 0)
						type = ATTACHMENT_REGION_SEQUENCE;
					else
						return SkeletonJson_setError(root, "Unknown attachment type: ", typeString);

					Attachment* attachment = Attachment_getAttachmentLoader()(type,
							cJSON_GetObjectString(attachmentMap, "name", attachmentName));

					if (type == ATTACHMENT_REGION || type == ATTACHMENT_REGION_SEQUENCE) {
						RegionAttachment* regionAttachment = (RegionAttachment*)attachment;
						regionAttachment->x = cJSON_GetObjectFloat(attachmentMap, "x", 0) * scale;
						regionAttachment->y = cJSON_GetObjectFloat(attachmentMap, "y", 0) * scale;
						regionAttachment->scaleX = cJSON_GetObjectFloat(attachmentMap, "scaleX", 1);
						regionAttachment->scaleY = cJSON_GetObjectFloat(attachmentMap, "scaleY", 1);
						regionAttachment->rotation = cJSON_GetObjectFloat(attachmentMap, "rotation", 0);
						regionAttachment->width = cJSON_GetObjectFloat(attachmentMap, "width", 32) * scale;
						regionAttachment->height = cJSON_GetObjectFloat(attachmentMap, "height", 32) * scale;
						RegionAttachment_updateOffset(regionAttachment);
					}

					Skin_addAttachment(skin, slotIndex, attachmentName, attachment);
				}
			}
		}
	}

	cJSON_dispose(root);
	return skeletonData;
}
