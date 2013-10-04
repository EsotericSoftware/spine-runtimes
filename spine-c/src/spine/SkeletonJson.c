/******************************************************************************
 * Spine Runtime Software License - Version 1.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms in whole or in part, with
 * or without modification, are permitted provided that the following conditions
 * are met:
 * 
 * 1. A Spine Essential, Professional, Enterprise, or Education License must
 *    be purchased from Esoteric Software and the license must remain valid:
 *    http://esotericsoftware.com/
 * 2. Redistributions of source code must retain this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer.
 * 3. Redistributions in binary form must reproduce this license, which is the
 *    above copyright notice, this declaration of conditions and the following
 *    disclaimer, in the documentation and/or other materials provided with the
 *    distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/SkeletonJson.h>
#include <stdio.h>
#include "Json.h"
#include <spine/extension.h>
#include <spine/RegionAttachment.h>
#include <spine/AtlasAttachmentLoader.h>

typedef struct {
	SkeletonJson super;
	int ownsLoader;
} _SkeletonJson;

SkeletonJson* SkeletonJson_createWithLoader (AttachmentLoader* attachmentLoader) {
	SkeletonJson* self = SUPER(NEW(_SkeletonJson));
	self->scale = 1;
	self->attachmentLoader = attachmentLoader;
	return self;
}

SkeletonJson* SkeletonJson_create (Atlas* atlas) {
	AtlasAttachmentLoader* attachmentLoader = AtlasAttachmentLoader_create(atlas);
	SkeletonJson* self = SkeletonJson_createWithLoader(SUPER(attachmentLoader));
	SUB_CAST(_SkeletonJson, self)->ownsLoader = 1;
	return self;
}

void SkeletonJson_dispose (SkeletonJson* self) {
	if (SUB_CAST(_SkeletonJson, self)->ownsLoader) AttachmentLoader_dispose(self->attachmentLoader);
	FREE(self->error);
	FREE(self);
}

void _SkeletonJson_setError (SkeletonJson* self, Json* root, const char* value1, const char* value2) {
	char message[256];
	int length;
	FREE(self->error);
	strcpy(message, value1);
	length = strlen(value1);
	if (value2) strncat(message + length, value2, 256 - length);
	MALLOC_STR(self->error, message);
	if (root) Json_dispose(root);
}

static float toColor (const char* value, int index) {
	char digits[3];
	char *error;
	int color;

	if (strlen(value) != 8) return -1;
	value += index * 2;

	digits[0] = *value;
	digits[1] = *(value + 1);
	digits[2] = '\0';
	color = strtoul(digits, &error, 16);
	if (*error != 0) return -1;
	return color / (float)255;
}

static void readCurve (CurveTimeline* timeline, int frameIndex, Json* frame) {
	Json* curve = Json_getItem(frame, "curve");
	if (!curve) return;
	if (curve->type == Json_String && strcmp(curve->valueString, "stepped") == 0)
		CurveTimeline_setStepped(timeline, frameIndex);
	else if (curve->type == Json_Array) {
		Json* child0 = curve->child;
		Json* child1 = child0->next;
		Json* child2 = child1->next;
		Json* child3 = child2->next;
		CurveTimeline_setCurve(timeline, frameIndex, child0->valueFloat, child1->valueFloat, child2->valueFloat,
				child3->valueFloat);
	}
}

static Animation* _SkeletonJson_readAnimation (SkeletonJson* self, Json* root, SkeletonData *skeletonData) {
	int i;
	Animation* animation;

	Json* bones = Json_getItem(root, "bones");
	Json* slots = Json_getItem(root, "slots");
	Json* drawOrder = Json_getItem(root, "draworder");
	Json* events = Json_getItem(root, "events");
	Json *boneMap, *slotMap;

	int timelineCount = 0;
	for (boneMap = bones ? bones->child : 0; boneMap; boneMap = boneMap->next)
		timelineCount += boneMap->size;
	for (slotMap = slots ? slots->child : 0; slotMap; slotMap = slotMap->next)
		timelineCount += slotMap->size;
	if (events) ++timelineCount;
	if (drawOrder) ++timelineCount;

	animation = Animation_create(root->name, timelineCount);
	animation->timelineCount = 0;
	skeletonData->animations[skeletonData->animationCount] = animation;
	++skeletonData->animationCount;

	for (boneMap = bones ? bones->child : 0; boneMap; boneMap = boneMap->next) {
		Json *timelineArray;

		int boneIndex = SkeletonData_findBoneIndex(skeletonData, boneMap->name);
		if (boneIndex == -1) {
			Animation_dispose(animation);
			_SkeletonJson_setError(self, root, "Bone not found: ", boneMap->name);
			return 0;
		}

		for (timelineArray = boneMap->child; timelineArray; timelineArray = timelineArray->next) {
			Json* frame;
			float duration;

			if (strcmp(timelineArray->name, "rotate") == 0) {
				RotateTimeline *timeline = RotateTimeline_create(timelineArray->size);
				timeline->boneIndex = boneIndex;
				for (frame = timelineArray->child, i = 0; frame; frame = frame->next, ++i) {
					RotateTimeline_setFrame(timeline, i, Json_getFloat(frame, "time", 0), Json_getFloat(frame, "angle", 0));
					readCurve(SUPER(timeline), i, frame);
				}
				animation->timelines[animation->timelineCount++] = (Timeline*)timeline;
				duration = timeline->frames[timelineArray->size * 2 - 2];
				if (duration > animation->duration) animation->duration = duration;

			} else {
				int isScale = strcmp(timelineArray->name, "scale") == 0;
				if (isScale || strcmp(timelineArray->name, "translate") == 0) {
					float scale = isScale ? 1 : self->scale;
					TranslateTimeline *timeline =
							isScale ? ScaleTimeline_create(timelineArray->size) : TranslateTimeline_create(timelineArray->size);
					timeline->boneIndex = boneIndex;
					for (frame = timelineArray->child, i = 0; frame; frame = frame->next, ++i) {
						TranslateTimeline_setFrame(timeline, i, Json_getFloat(frame, "time", 0), Json_getFloat(frame, "x", 0) * scale,
								Json_getFloat(frame, "y", 0) * scale);
						readCurve(SUPER(timeline), i, frame);
					}
					animation->timelines[animation->timelineCount++] = (Timeline*)timeline;
					duration = timeline->frames[timelineArray->size * 3 - 3];
					if (duration > animation->duration) animation->duration = duration;
				} else {
					Animation_dispose(animation);
					_SkeletonJson_setError(self, 0, "Invalid timeline type for a bone: ", timelineArray->name);
					return 0;
				}
			}
		}
	}

	for (slotMap = slots ? slots->child : 0; slotMap; slotMap = slotMap->next) {
		Json *timelineArray;

		int slotIndex = SkeletonData_findSlotIndex(skeletonData, slotMap->name);
		if (slotIndex == -1) {
			Animation_dispose(animation);
			_SkeletonJson_setError(self, root, "Slot not found: ", slotMap->name);
			return 0;
		}

		for (timelineArray = slotMap->child; timelineArray; timelineArray = timelineArray->next) {
			Json* frame;
			float duration;

			if (strcmp(timelineArray->name, "color") == 0) {
				ColorTimeline *timeline = ColorTimeline_create(timelineArray->size);
				timeline->slotIndex = slotIndex;
				for (frame = timelineArray->child, i = 0; frame; frame = frame->next, ++i) {
					const char* s = Json_getString(frame, "color", 0);
					ColorTimeline_setFrame(timeline, i, Json_getFloat(frame, "time", 0), toColor(s, 0), toColor(s, 1), toColor(s, 2),
							toColor(s, 3));
					readCurve(SUPER(timeline), i, frame);
				}
				animation->timelines[animation->timelineCount++] = (Timeline*)timeline;
				duration = timeline->frames[timelineArray->size * 5 - 5];
				if (duration > animation->duration) animation->duration = duration;

			} else if (strcmp(timelineArray->name, "attachment") == 0) {
				AttachmentTimeline *timeline = AttachmentTimeline_create(timelineArray->size);
				timeline->slotIndex = slotIndex;
				for (frame = timelineArray->child, i = 0; frame; frame = frame->next, ++i) {
					Json* name = Json_getItem(frame, "name");
					AttachmentTimeline_setFrame(timeline, i, Json_getFloat(frame, "time", 0),
							name->type == Json_NULL ? 0 : name->valueString);
				}
				animation->timelines[animation->timelineCount++] = (Timeline*)timeline;
				duration = timeline->frames[timelineArray->size - 1];
				if (duration > animation->duration) animation->duration = duration;

			} else {
				Animation_dispose(animation);
				_SkeletonJson_setError(self, 0, "Invalid timeline type for a slot: ", timelineArray->name);
				return 0;
			}
		}
	}

	if (events) {
		Json* frame;
		float duration;

		EventTimeline* timeline = EventTimeline_create(events->size);
		for (frame = events->child, i = 0; frame; frame = frame->next, ++i) {
			Event* event;
			const char* stringValue;
			EventData* eventData = SkeletonData_findEvent(skeletonData, Json_getString(frame, "name", 0));
			if (!eventData) {
				Animation_dispose(animation);
				_SkeletonJson_setError(self, 0, "Event not found: ", Json_getString(frame, "name", 0));
				return 0;
			}
			event = Event_create(eventData);
			event->intValue = Json_getInt(frame, "int", eventData->intValue);
			event->floatValue = Json_getFloat(frame, "float", eventData->floatValue);
			stringValue = Json_getString(frame, "string", eventData->stringValue);
			if (stringValue) MALLOC_STR(event->stringValue, stringValue);
			EventTimeline_setFrame(timeline, i, Json_getFloat(frame, "time", 0), event);
		}
		animation->timelines[animation->timelineCount++] = (Timeline*)timeline;
		duration = timeline->frames[events->size - 1];
		if (duration > animation->duration) animation->duration = duration;
	}

	if (drawOrder) {
		Json* frame;
		float duration;

		DrawOrderTimeline* timeline = DrawOrderTimeline_create(drawOrder->size, skeletonData->slotCount);
		for (frame = drawOrder->child, i = 0; frame; frame = frame->next, ++i) {
			int ii;
			int* drawOrder = 0;
			Json* offsets = Json_getItem(frame, "offsets");
			if (offsets) {
				Json* offsetMap;
				int* unchanged = MALLOC(int, skeletonData->slotCount - offsets->size);
				int originalIndex = 0, unchangedIndex = 0;

				drawOrder = MALLOC(int, skeletonData->slotCount);
				for (ii = skeletonData->slotCount - 1; ii >= 0; --ii)
					drawOrder[ii] = -1;

				for (offsetMap = offsets->child; offsetMap; offsetMap = offsetMap->next) {
					int slotIndex = SkeletonData_findSlotIndex(skeletonData, Json_getString(offsetMap, "slot", 0));
					if (slotIndex == -1) {
						Animation_dispose(animation);
						_SkeletonJson_setError(self, 0, "Slot not found: ", Json_getString(offsetMap, "slot", 0));
						return 0;
					}
					/* Collect unchanged items. */
					while (originalIndex != slotIndex)
						unchanged[unchangedIndex++] = originalIndex++;
					/* Set changed items. */
					drawOrder[originalIndex + Json_getInt(offsetMap, "offset", 0)] = originalIndex;
					++originalIndex;
				}
				/* Collect remaining unchanged items. */
				while (originalIndex < skeletonData->slotCount)
					unchanged[unchangedIndex++] = originalIndex++;
				/* Fill in unchanged items. */
				for (ii = skeletonData->slotCount - 1; ii >= 0; ii--)
					if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
				FREE(unchanged);
			}
			DrawOrderTimeline_setFrame(timeline, i, Json_getFloat(frame, "time", 0), drawOrder);
			FREE(drawOrder);
		}
		animation->timelines[animation->timelineCount++] = (Timeline*)timeline;
		duration = timeline->frames[drawOrder->size - 1];
		if (duration > animation->duration) animation->duration = duration;
	}

	return animation;
}

SkeletonData* SkeletonJson_readSkeletonDataFile (SkeletonJson* self, const char* path) {
	int length;
	SkeletonData* skeletonData;
	const char* json = _Util_readFile(path, &length);
	if (!json) {
		_SkeletonJson_setError(self, 0, "Unable to read skeleton file: ", path);
		return 0;
	}
	skeletonData = SkeletonJson_readSkeletonData(self, json);
	FREE(json);
	return skeletonData;
}

SkeletonData* SkeletonJson_readSkeletonData (SkeletonJson* self, const char* json) {
	int i;
	SkeletonData* skeletonData;
	Json *root, *bones, *boneMap, *slots, *skins, *animations, *events;

	FREE(self->error);
	CONST_CAST(char*, self->error) = 0;

	root = Json_create(json);
	if (!root) {
		_SkeletonJson_setError(self, 0, "Invalid skeleton JSON: ", Json_getError());
		return 0;
	}

	skeletonData = SkeletonData_create();

	bones = Json_getItem(root, "bones");
	skeletonData->bones = MALLOC(BoneData*, bones->size);
	for (boneMap = bones->child, i = 0; boneMap; boneMap = boneMap->next, ++i) {
		BoneData* boneData;

		BoneData* parent = 0;
		const char* parentName = Json_getString(boneMap, "parent", 0);
		if (parentName) {
			parent = SkeletonData_findBone(skeletonData, parentName);
			if (!parent) {
				SkeletonData_dispose(skeletonData);
				_SkeletonJson_setError(self, root, "Parent bone not found: ", parentName);
				return 0;
			}
		}

		boneData = BoneData_create(Json_getString(boneMap, "name", 0), parent);
		boneData->length = Json_getFloat(boneMap, "length", 0) * self->scale;
		boneData->x = Json_getFloat(boneMap, "x", 0) * self->scale;
		boneData->y = Json_getFloat(boneMap, "y", 0) * self->scale;
		boneData->rotation = Json_getFloat(boneMap, "rotation", 0);
		boneData->scaleX = Json_getFloat(boneMap, "scaleX", 1);
		boneData->scaleY = Json_getFloat(boneMap, "scaleY", 1);
		boneData->inheritScale = Json_getInt(boneMap, "inheritScale", 1);
		boneData->inheritRotation = Json_getInt(boneMap, "inheritRotation", 1);

		skeletonData->bones[i] = boneData;
		++skeletonData->boneCount;
	}

	slots = Json_getItem(root, "slots");
	if (slots) {
		Json *slotMap;
		skeletonData->slots = MALLOC(SlotData*, slots->size);
		for (slotMap = slots->child, i = 0; slotMap; slotMap = slotMap->next, ++i) {
			SlotData* slotData;
			const char* color;
			Json *attachmentItem;

			const char* boneName = Json_getString(slotMap, "bone", 0);
			BoneData* boneData = SkeletonData_findBone(skeletonData, boneName);
			if (!boneData) {
				SkeletonData_dispose(skeletonData);
				_SkeletonJson_setError(self, root, "Slot bone not found: ", boneName);
				return 0;
			}

			slotData = SlotData_create(Json_getString(slotMap, "name", 0), boneData);

			color = Json_getString(slotMap, "color", 0);
			if (color) {
				slotData->r = toColor(color, 0);
				slotData->g = toColor(color, 1);
				slotData->b = toColor(color, 2);
				slotData->a = toColor(color, 3);
			}

			attachmentItem = Json_getItem(slotMap, "attachment");
			if (attachmentItem) SlotData_setAttachmentName(slotData, attachmentItem->valueString);

			slotData->additiveBlending = Json_getInt(slotMap, "additive", 0);

			skeletonData->slots[i] = slotData;
			++skeletonData->slotCount;
		}
	}

	skins = Json_getItem(root, "skins");
	if (skins) {
		Json *slotMap;
		skeletonData->skins = MALLOC(Skin*, skins->size);
		for (slotMap = skins->child, i = 0; slotMap; slotMap = slotMap->next, ++i) {
			Json *attachmentsMap;
			Skin *skin = Skin_create(slotMap->name);

			skeletonData->skins[i] = skin;
			++skeletonData->skinCount;
			if (strcmp(slotMap->name, "default") == 0) skeletonData->defaultSkin = skin;

			for (attachmentsMap = slotMap->child; attachmentsMap; attachmentsMap = attachmentsMap->next) {
				int slotIndex = SkeletonData_findSlotIndex(skeletonData, attachmentsMap->name);
				Json *attachmentMap;

				for (attachmentMap = attachmentsMap->child; attachmentMap; attachmentMap = attachmentMap->next) {
					Attachment* attachment;
					const char* skinAttachmentName = attachmentMap->name;
					const char* attachmentName = Json_getString(attachmentMap, "name", skinAttachmentName);

					const char* typeString = Json_getString(attachmentMap, "type", "region");
					AttachmentType type;
					if (strcmp(typeString, "region") == 0)
						type = ATTACHMENT_REGION;
					else if (strcmp(typeString, "boundingbox") == 0)
						type = ATTACHMENT_BOUNDING_BOX;
					else if (strcmp(typeString, "regionsequence") == 0)
						type = ATTACHMENT_REGION_SEQUENCE;
					else {
						SkeletonData_dispose(skeletonData);
						_SkeletonJson_setError(self, root, "Unknown attachment type: ", typeString);
						return 0;
					}

					attachment = AttachmentLoader_newAttachment(self->attachmentLoader, skin, type, attachmentName);
					if (!attachment) {
						if (self->attachmentLoader->error1) {
							SkeletonData_dispose(skeletonData);
							_SkeletonJson_setError(self, root, self->attachmentLoader->error1, self->attachmentLoader->error2);
							return 0;
						}
						continue;
					}

					switch (attachment->type) {
					case ATTACHMENT_REGION:
					case ATTACHMENT_REGION_SEQUENCE: {
						RegionAttachment* regionAttachment = (RegionAttachment*)attachment;
						regionAttachment->x = Json_getFloat(attachmentMap, "x", 0) * self->scale;
						regionAttachment->y = Json_getFloat(attachmentMap, "y", 0) * self->scale;
						regionAttachment->scaleX = Json_getFloat(attachmentMap, "scaleX", 1);
						regionAttachment->scaleY = Json_getFloat(attachmentMap, "scaleY", 1);
						regionAttachment->rotation = Json_getFloat(attachmentMap, "rotation", 0);
						regionAttachment->width = Json_getFloat(attachmentMap, "width", 32) * self->scale;
						regionAttachment->height = Json_getFloat(attachmentMap, "height", 32) * self->scale;
						RegionAttachment_updateOffset(regionAttachment);
						break;
					}
					case ATTACHMENT_BOUNDING_BOX: {
						BoundingBoxAttachment* box = (BoundingBoxAttachment*)attachment;
						Json* verticesArray = Json_getItem(attachmentMap, "vertices");
						Json* vertex;
						int i;
						box->verticesCount = verticesArray->size;
						box->vertices = MALLOC(float, verticesArray->size);
						for (vertex = verticesArray->child, i = 0; vertex; vertex = vertex->next, ++i)
							box->vertices[i] = vertex->valueFloat * self->scale;
						break;
					}
					}

					Skin_addAttachment(skin, slotIndex, skinAttachmentName, attachment);
				}
			}
		}
	}

	/* Events. */
	events = Json_getItem(root, "events");
	if (events) {
		Json *eventMap;
		const char* stringValue;
		skeletonData->events = MALLOC(EventData*, events->size);
		for (eventMap = events->child; eventMap; eventMap = eventMap->next) {
			EventData* eventData = EventData_create(eventMap->name);
			eventData->intValue = Json_getInt(eventMap, "int", 0);
			eventData->floatValue = Json_getFloat(eventMap, "float", 0);
			stringValue = Json_getString(eventMap, "string", 0);
			if (stringValue) MALLOC_STR(eventData->stringValue, stringValue);
			skeletonData->events[skeletonData->eventCount++] = eventData;
		}
	}

	/* Animations. */
	animations = Json_getItem(root, "animations");
	if (animations) {
		Json *animationMap;
		skeletonData->animations = MALLOC(Animation*, animations->size);
		for (animationMap = animations->child; animationMap; animationMap = animationMap->next)
			_SkeletonJson_readAnimation(self, animationMap, skeletonData);
	}

	Json_dispose(root);
	return skeletonData;
}
