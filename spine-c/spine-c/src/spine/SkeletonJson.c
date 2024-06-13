/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include "Json.h"
#include <spine/Version.h>
#include <spine/Array.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/SkeletonJson.h>
#include <spine/extension.h>
#include <stdio.h>

typedef struct {
	const char *parent;
	const char *skin;
	int slotIndex;
	spMeshAttachment *mesh;
	int inheritTimeline;
} _spLinkedMesh;

typedef struct {
	spSkeletonJson super;
	int ownsLoader;

	int linkedMeshCount;
	int linkedMeshCapacity;
	_spLinkedMesh *linkedMeshes;
} _spSkeletonJson;

spSkeletonJson *spSkeletonJson_createWithLoader(spAttachmentLoader *attachmentLoader) {
	spSkeletonJson *self = SUPER(NEW(_spSkeletonJson));
	self->scale = 1;
	self->attachmentLoader = attachmentLoader;
	return self;
}

spSkeletonJson *spSkeletonJson_create(spAtlas *atlas) {
	spAtlasAttachmentLoader *attachmentLoader = spAtlasAttachmentLoader_create(atlas);
	spSkeletonJson *self = spSkeletonJson_createWithLoader(SUPER(attachmentLoader));
	SUB_CAST(_spSkeletonJson, self)->ownsLoader = 1;
	return self;
}

void spSkeletonJson_dispose(spSkeletonJson *self) {
	_spSkeletonJson *internal = SUB_CAST(_spSkeletonJson, self);
	if (internal->ownsLoader) spAttachmentLoader_dispose(self->attachmentLoader);
	FREE(internal->linkedMeshes);
	FREE(self->error);
	FREE(self);
}

void _spSkeletonJson_setError(spSkeletonJson *self, Json *root, const char *value1, const char *value2) {
	char message[256];
	int length;
	FREE(self->error);
	strcpy(message, value1);
	length = (int) strlen(value1);
	if (value2) strncat(message + length, value2, 255 - length);
	MALLOC_STR(self->error, message);
	if (root) Json_dispose(root);
}

static float toColor(const char *value, int index) {
	char digits[3];
	char *error;
	int color;

	if ((size_t) index >= strlen(value) / 2) return -1;
	value += index * 2;

	digits[0] = *value;
	digits[1] = *(value + 1);
	digits[2] = '\0';
	color = (int) strtoul(digits, &error, 16);
	if (*error != 0) return -1;
	return color / (float) 255;
}

static void toColor2(spColor *color, const char *value, int /*bool*/ hasAlpha) {
	color->r = toColor(value, 0);
	color->g = toColor(value, 1);
	color->b = toColor(value, 2);
	if (hasAlpha) color->a = toColor(value, 3);
	else
		color->a = 1.0f;
}

static void
setBezier(spCurveTimeline *timeline, int frame, int value, int bezier, float time1, float value1, float cx1, float cy1,
		  float cx2, float cy2, float time2, float value2) {
	spTimeline_setBezier(SUPER(timeline), bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
}

static int readCurve(Json *curve, spCurveTimeline *timeline, int bezier, int frame, int value, float time1, float time2,
					 float value1, float value2, float scale) {
	float cx1, cy1, cx2, cy2;
	if (curve->type == Json_String && strcmp(curve->valueString, "stepped") == 0) {
		spCurveTimeline_setStepped(timeline, frame);
		return bezier;
	}
	curve = Json_getItemAtIndex(curve, value << 2);
	cx1 = curve->valueFloat;
	curve = curve->next;
	cy1 = curve->valueFloat * scale;
	curve = curve->next;
	cx2 = curve->valueFloat;
	curve = curve->next;
	cy2 = curve->valueFloat * scale;
	setBezier(timeline, frame, value, bezier, time1, value1, cx1, cy1, cx2, cy2, time2, value2);
	return bezier + 1;
}

static spTimeline *readTimeline(Json *keyMap, spCurveTimeline1 *timeline, float defaultValue, float scale) {
	float time = Json_getFloat(keyMap, "time", 0);
	float value = Json_getFloat(keyMap, "value", defaultValue) * scale;
	int frame, bezier = 0;
	for (frame = 0;; ++frame) {
		Json *nextMap, *curve;
		float time2, value2;
		spCurveTimeline1_setFrame(timeline, frame, time, value);
		nextMap = keyMap->next;
		if (nextMap == NULL) break;
		time2 = Json_getFloat(nextMap, "time", 0);
		value2 = Json_getFloat(nextMap, "value", defaultValue) * scale;
		curve = Json_getItem(keyMap, "curve");
		if (curve != NULL) bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value, value2, scale);
		time = time2;
		value = value2;
		keyMap = nextMap;
	}
	/* timeline.shrink(); // BOZO */
	return SUPER(timeline);
}

static spTimeline *
readTimeline2(Json *keyMap, spCurveTimeline2 *timeline, const char *name1, const char *name2, float defaultValue,
			  float scale) {
	float time = Json_getFloat(keyMap, "time", 0);
	float value1 = Json_getFloat(keyMap, name1, defaultValue) * scale;
	float value2 = Json_getFloat(keyMap, name2, defaultValue) * scale;
	int frame, bezier = 0;
	for (frame = 0;; ++frame) {
		Json *nextMap, *curve;
		float time2, nvalue1, nvalue2;
		spCurveTimeline2_setFrame(timeline, frame, time, value1, value2);
		nextMap = keyMap->next;
		if (nextMap == NULL) break;
		time2 = Json_getFloat(nextMap, "time", 0);
		nvalue1 = Json_getFloat(nextMap, name1, defaultValue) * scale;
		nvalue2 = Json_getFloat(nextMap, name2, defaultValue) * scale;
		curve = Json_getItem(keyMap, "curve");
		if (curve != NULL) {
			bezier = readCurve(curve, timeline, bezier, frame, 0, time, time2, value1, nvalue1, scale);
			bezier = readCurve(curve, timeline, bezier, frame, 1, time, time2, value2, nvalue2, scale);
		}
		time = time2;
		value1 = nvalue1;
		value2 = nvalue2;
		keyMap = nextMap;
	}
	/* timeline.shrink(); // BOZO */
	return SUPER(timeline);
}

static spSequence *readSequence(Json *item) {
	spSequence *sequence;
	if (item == NULL) return NULL;
	sequence = spSequence_create(Json_getInt(item, "count", 0));
	sequence->start = Json_getInt(item, "start", 1);
	sequence->digits = Json_getInt(item, "digits", 0);
	sequence->setupIndex = Json_getInt(item, "setupIndex", 0);
	return sequence;
}

static void _spSkeletonJson_addLinkedMesh(spSkeletonJson *self, spMeshAttachment *mesh, const char *skin, int slotIndex,
										  const char *parent, int inheritDeform) {
	_spLinkedMesh *linkedMesh;
	_spSkeletonJson *internal = SUB_CAST(_spSkeletonJson, self);

	if (internal->linkedMeshCount == internal->linkedMeshCapacity) {
		_spLinkedMesh *linkedMeshes;
		internal->linkedMeshCapacity *= 2;
		if (internal->linkedMeshCapacity < 8) internal->linkedMeshCapacity = 8;
		linkedMeshes = MALLOC(_spLinkedMesh, internal->linkedMeshCapacity);
		memcpy(linkedMeshes, internal->linkedMeshes, sizeof(_spLinkedMesh) * internal->linkedMeshCount);
		FREE(internal->linkedMeshes);
		internal->linkedMeshes = linkedMeshes;
	}

	linkedMesh = internal->linkedMeshes + internal->linkedMeshCount++;
	linkedMesh->mesh = mesh;
	linkedMesh->skin = skin;
	linkedMesh->slotIndex = slotIndex;
	linkedMesh->parent = parent;
	linkedMesh->inheritTimeline = inheritDeform;
}

static void cleanUpTimelines(spTimelineArray *timelines) {
	int i, n;
	for (i = 0, n = timelines->size; i < n; ++i)
		spTimeline_dispose(timelines->items[i]);
	spTimelineArray_dispose(timelines);
}

static int findSlotIndex(spSkeletonJson *json, const spSkeletonData *skeletonData, const char *slotName, spTimelineArray *timelines) {
	spSlotData *slot = spSkeletonData_findSlot(skeletonData, slotName);
	if (slot) return slot->index;
	cleanUpTimelines(timelines);
	_spSkeletonJson_setError(json, NULL, "Slot not found: ", slotName);
	return -1;
}

int findIkConstraintIndex(spSkeletonJson *json, const spSkeletonData *skeletonData, const spIkConstraintData *constraint, spTimelineArray *timelines) {
	if (constraint) {
		int i;
		for (i = 0; i < skeletonData->ikConstraintsCount; ++i)
			if (skeletonData->ikConstraints[i] == constraint) return i;
	}
	cleanUpTimelines(timelines);
	_spSkeletonJson_setError(json, NULL, "IK constraint not found: ", constraint->name);
	return -1;
}

int findTransformConstraintIndex(spSkeletonJson *json, const spSkeletonData *skeletonData, const spTransformConstraintData *constraint, spTimelineArray *timelines) {
	if (constraint) {
		int i;
		for (i = 0; i < skeletonData->transformConstraintsCount; ++i)
			if (skeletonData->transformConstraints[i] == constraint) return i;
	}
	cleanUpTimelines(timelines);
	_spSkeletonJson_setError(json, NULL, "Transform constraint not found: ", constraint->name);
	return -1;
}

int findPathConstraintIndex(spSkeletonJson *json, const spSkeletonData *skeletonData, const spPathConstraintData *constraint, spTimelineArray *timelines) {
	if (constraint) {
		int i;
		for (i = 0; i < skeletonData->pathConstraintsCount; ++i)
			if (skeletonData->pathConstraints[i] == constraint) return i;
	}
	cleanUpTimelines(timelines);
	_spSkeletonJson_setError(json, NULL, "Path constraint not found: ", constraint->name);
	return -1;
}

int findPhysicsConstraintIndex(spSkeletonJson *json, const spSkeletonData *skeletonData, const spPhysicsConstraintData *constraint, spTimelineArray *timelines) {
	if (constraint) {
		int i;
		for (i = 0; i < skeletonData->physicsConstraintsCount; ++i)
			if (skeletonData->physicsConstraints[i] == constraint) return i;
	}
	cleanUpTimelines(timelines);
	_spSkeletonJson_setError(json, NULL, "Physics constraint not found: ", constraint->name);
	return -1;
}

static spAnimation *_spSkeletonJson_readAnimation(spSkeletonJson *self, Json *root, spSkeletonData *skeletonData) {
	spTimelineArray *timelines = spTimelineArray_create(8);

	float scale = self->scale, duration;
	Json *bones = Json_getItem(root, "bones");
	Json *slots = Json_getItem(root, "slots");
	Json *ik = Json_getItem(root, "ik");
	Json *transform = Json_getItem(root, "transform");
	Json *paths = Json_getItem(root, "path");
	Json *physics = Json_getItem(root, "physics");
	Json *attachmentsJson = Json_getItem(root, "attachments");
	Json *drawOrderJson = Json_getItem(root, "drawOrder");
	Json *events = Json_getItem(root, "events");
	Json *boneMap, *slotMap, *keyMap, *nextMap, *curve, *timelineMap;
	Json *attachmentsMap, *constraintMap;
	int frame, bezier, i, n;
	spColor color, color2, newColor, newColor2;

	/* Slot timelines. */
	for (slotMap = slots ? slots->child : 0; slotMap; slotMap = slotMap->next) {
		int slotIndex = findSlotIndex(self, skeletonData, slotMap->name, timelines);
		if (slotIndex == -1) return NULL;

		for (timelineMap = slotMap->child; timelineMap; timelineMap = timelineMap->next) {
			int frames = timelineMap->size;
			if (strcmp(timelineMap->name, "attachment") == 0) {
				spAttachmentTimeline *timeline = spAttachmentTimeline_create(frames, slotIndex);
				for (keyMap = timelineMap->child, frame = 0; keyMap; keyMap = keyMap->next, ++frame) {
					spAttachmentTimeline_setFrame(timeline, frame, Json_getFloat(keyMap, "time", 0),
												  Json_getItem(keyMap, "name") ? Json_getItem(keyMap, "name")->valueString : NULL);
				}
				spTimelineArray_add(timelines, SUPER(timeline));

			} else if (strcmp(timelineMap->name, "rgba") == 0) {
				float time;
				spRGBATimeline *timeline = spRGBATimeline_create(frames, frames << 2, slotIndex);
				keyMap = timelineMap->child;
				time = Json_getFloat(keyMap, "time", 0);
				toColor2(&color, Json_getString(keyMap, "color", 0), 1);

				for (frame = 0, bezier = 0;; ++frame) {
					float time2;
					spRGBATimeline_setFrame(timeline, frame, time, color.r, color.g, color.b, color.a);
					nextMap = keyMap->next;
					if (!nextMap) {
						/* timeline.shrink(); // BOZO */
						break;
					}
					time2 = Json_getFloat(nextMap, "time", 0);
					toColor2(&newColor, Json_getString(nextMap, "color", 0), 1);
					curve = Json_getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, color.r, newColor.r,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, color.g, newColor.g,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 2, time, time2, color.b, newColor.b,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 3, time, time2, color.a, newColor.a,
										   1);
					}
					time = time2;
					color = newColor;
					keyMap = nextMap;
				}
				spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
			} else if (strcmp(timelineMap->name, "rgb") == 0) {
				float time;
				spRGBTimeline *timeline = spRGBTimeline_create(frames, frames * 3, slotIndex);
				keyMap = timelineMap->child;
				time = Json_getFloat(keyMap, "time", 0);
				toColor2(&color, Json_getString(keyMap, "color", 0), 1);

				for (frame = 0, bezier = 0;; ++frame) {
					float time2;
					spRGBTimeline_setFrame(timeline, frame, time, color.r, color.g, color.b);
					nextMap = keyMap->next;
					if (!nextMap) {
						/* timeline.shrink(); // BOZO */
						break;
					}
					time2 = Json_getFloat(nextMap, "time", 0);
					toColor2(&newColor, Json_getString(nextMap, "color", 0), 1);
					curve = Json_getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, color.r, newColor.r,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, color.g, newColor.g,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 2, time, time2, color.b, newColor.b,
										   1);
					}
					time = time2;
					color = newColor;
					keyMap = nextMap;
				}
				spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
			} else if (strcmp(timelineMap->name, "alpha") == 0) {
				spTimelineArray_add(timelines, readTimeline(timelineMap->child,
															SUPER(spAlphaTimeline_create(frames,
																						 frames, slotIndex)),
															0, 1));
			} else if (strcmp(timelineMap->name, "rgba2") == 0) {
				float time;
				spRGBA2Timeline *timeline = spRGBA2Timeline_create(frames, frames * 7, slotIndex);
				keyMap = timelineMap->child;
				time = Json_getFloat(keyMap, "time", 0);
				toColor2(&color, Json_getString(keyMap, "light", 0), 1);
				toColor2(&color2, Json_getString(keyMap, "dark", 0), 0);

				for (frame = 0, bezier = 0;; ++frame) {
					float time2;
					spRGBA2Timeline_setFrame(timeline, frame, time, color.r, color.g, color.b, color.a, color2.g,
											 color2.g, color2.b);
					nextMap = keyMap->next;
					if (!nextMap) {
						/* timeline.shrink(); // BOZO */
						break;
					}
					time2 = Json_getFloat(nextMap, "time", 0);
					toColor2(&newColor, Json_getString(nextMap, "light", 0), 1);
					toColor2(&newColor2, Json_getString(nextMap, "dark", 0), 0);
					curve = Json_getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, color.r, newColor.r,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, color.g, newColor.g,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 2, time, time2, color.b, newColor.b,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 3, time, time2, color.a, newColor.a,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 4, time, time2, color2.r, newColor2.r,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 5, time, time2, color2.g, newColor2.g,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 6, time, time2, color2.b, newColor2.b,
										   1);
					}
					time = time2;
					color = newColor;
					color2 = newColor2;
					keyMap = nextMap;
				}
				spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
			} else if (strcmp(timelineMap->name, "rgb2") == 0) {
				float time;
				spRGBA2Timeline *timeline = spRGBA2Timeline_create(frames, frames * 6, slotIndex);
				keyMap = timelineMap->child;
				time = Json_getFloat(keyMap, "time", 0);
				toColor2(&color, Json_getString(keyMap, "light", 0), 0);
				toColor2(&color2, Json_getString(keyMap, "dark", 0), 0);

				for (frame = 0, bezier = 0;; ++frame) {
					float time2;
					spRGBA2Timeline_setFrame(timeline, frame, time, color.r, color.g, color.b, color.a, color2.r,
											 color2.g, color2.b);
					nextMap = keyMap->next;
					if (!nextMap) {
						/* timeline.shrink(); // BOZO */
						break;
					}
					time2 = Json_getFloat(nextMap, "time", 0);
					toColor2(&newColor, Json_getString(nextMap, "light", 0), 0);
					toColor2(&newColor2, Json_getString(nextMap, "dark", 0), 0);
					curve = Json_getItem(keyMap, "curve");
					if (curve) {
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, color.r, newColor.r,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, color.g, newColor.g,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 2, time, time2, color.b, newColor.b,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 3, time, time2, color2.r, newColor2.r,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 4, time, time2, color2.g, newColor2.g,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 5, time, time2, color2.b, newColor2.b,
										   1);
					}
					time = time2;
					color = newColor;
					color2 = newColor2;
					keyMap = nextMap;
				}
				spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
			} else {
				cleanUpTimelines(timelines);
				_spSkeletonJson_setError(self, NULL, "Invalid timeline type for a slot: ", timelineMap->name);
				return NULL;
			}
		}
	}

	/* Bone timelines. */
	for (boneMap = bones ? bones->child : 0; boneMap; boneMap = boneMap->next) {
		int boneIndex = -1;
		for (i = 0; i < skeletonData->bonesCount; ++i) {
			if (strcmp(skeletonData->bones[i]->name, boneMap->name) == 0) {
				boneIndex = i;
				break;
			}
		}
		if (boneIndex == -1) {
			cleanUpTimelines(timelines);
			_spSkeletonJson_setError(self, NULL, "Bone not found: ", boneMap->name);
			return NULL;
		}

		for (timelineMap = boneMap->child; timelineMap; timelineMap = timelineMap->next) {
			int frames = timelineMap->size;
			if (frames == 0) continue;

			if (strcmp(timelineMap->name, "rotate") == 0) {
				spTimelineArray_add(timelines, readTimeline(timelineMap->child,
															SUPER(spRotateTimeline_create(frames,
																						  frames,
																						  boneIndex)),
															0, 1));
			} else if (strcmp(timelineMap->name, "translate") == 0) {
				spTranslateTimeline *timeline = spTranslateTimeline_create(frames, frames << 1,
																		   boneIndex);
				spTimelineArray_add(timelines, readTimeline2(timelineMap->child, SUPER(timeline), "x", "y", 0, scale));
			} else if (strcmp(timelineMap->name, "translatex") == 0) {
				spTranslateXTimeline *timeline = spTranslateXTimeline_create(frames, frames,
																			 boneIndex);
				spTimelineArray_add(timelines, readTimeline(timelineMap->child, SUPER(timeline), 0, scale));
			} else if (strcmp(timelineMap->name, "translatey") == 0) {
				spTranslateYTimeline *timeline = spTranslateYTimeline_create(frames, frames,
																			 boneIndex);
				spTimelineArray_add(timelines, readTimeline(timelineMap->child, SUPER(timeline), 0, scale));
			} else if (strcmp(timelineMap->name, "scale") == 0) {
				spScaleTimeline *timeline = spScaleTimeline_create(frames, frames << 1,
																   boneIndex);
				spTimelineArray_add(timelines, readTimeline2(timelineMap->child, SUPER(timeline), "x", "y", 1, 1));
			} else if (strcmp(timelineMap->name, "scalex") == 0) {
				spScaleXTimeline *timeline = spScaleXTimeline_create(frames, frames, boneIndex);
				spTimelineArray_add(timelines, readTimeline(timelineMap->child, SUPER(timeline), 1, 1));
			} else if (strcmp(timelineMap->name, "scaley") == 0) {
				spScaleYTimeline *timeline = spScaleYTimeline_create(frames, frames, boneIndex);
				spTimelineArray_add(timelines, readTimeline(timelineMap->child, SUPER(timeline), 1, 1));
			} else if (strcmp(timelineMap->name, "shear") == 0) {
				spShearTimeline *timeline = spShearTimeline_create(frames, frames << 1,
																   boneIndex);
				spTimelineArray_add(timelines, readTimeline2(timelineMap->child, SUPER(timeline), "x", "y", 0, 1));
			} else if (strcmp(timelineMap->name, "shearx") == 0) {
				spShearXTimeline *timeline = spShearXTimeline_create(frames, frames, boneIndex);
				spTimelineArray_add(timelines, readTimeline(timelineMap->child, SUPER(timeline), 0, 1));
			} else if (strcmp(timelineMap->name, "sheary") == 0) {
				spShearYTimeline *timeline = spShearYTimeline_create(frames, frames, boneIndex);
				spTimelineArray_add(timelines, readTimeline(timelineMap->child, SUPER(timeline), 0, 1));
			} else if (strcmp(timelineMap->name, "inherit") == 0) {
				spInheritTimeline *timeline = spInheritTimeline_create(frames, boneIndex);
				keyMap = timelineMap->child;
				for (frame = 0;; frame++) {
					float time = Json_getFloat(keyMap, "time", 0);
					const char *value = Json_getString(keyMap, "inherit", "normal");
					spInherit inherit = SP_INHERIT_NORMAL;
					if (strcmp(value, "normal") == 0) inherit = SP_INHERIT_NORMAL;
					else if (strcmp(value, "onlyTranslation") == 0)
						inherit = SP_INHERIT_ONLYTRANSLATION;
					else if (strcmp(value, "noRotationOrReflection") == 0)
						inherit = SP_INHERIT_NOROTATIONORREFLECTION;
					else if (strcmp(value, "noScale") == 0)
						inherit = SP_INHERIT_NOSCALE;
					else if (strcmp(value, "noScaleOrReflection") == 0)
						inherit = SP_INHERIT_NOSCALEORREFLECTION;
					spInheritTimeline_setFrame(timeline, frame, time, inherit);
					nextMap = keyMap->next;
					if (!nextMap) break;
				}
				spTimelineArray_add(timelines, SUPER(timeline));
			} else {
				cleanUpTimelines(timelines);
				_spSkeletonJson_setError(self, NULL, "Invalid timeline type for a bone: ", timelineMap->name);
				return NULL;
			}
		}
	}

	/* IK constraint timelines. */
	for (constraintMap = ik ? ik->child : 0; constraintMap; constraintMap = constraintMap->next) {
		spIkConstraintData *constraint;
		spIkConstraintTimeline *timeline;
		int constraintIndex;
		float time, mix, softness;
		keyMap = constraintMap->child;
		if (keyMap == NULL) continue;

		constraint = spSkeletonData_findIkConstraint(skeletonData, constraintMap->name);
		constraintIndex = findIkConstraintIndex(self, skeletonData, constraint, timelines);
		if (constraintIndex == -1) return NULL;
		timeline = spIkConstraintTimeline_create(constraintMap->size, constraintMap->size << 1, constraintIndex);

		time = Json_getFloat(keyMap, "time", 0);
		mix = Json_getFloat(keyMap, "mix", 1);
		softness = Json_getFloat(keyMap, "softness", 0) * scale;

		for (frame = 0, bezier = 0;; ++frame) {
			float time2, mix2, softness2;
			int bendDirection = Json_getInt(keyMap, "bendPositive", 1) ? 1 : -1;
			spIkConstraintTimeline_setFrame(timeline, frame, time, mix, softness, bendDirection,
											Json_getInt(keyMap, "compress", 0) ? 1 : 0,
											Json_getInt(keyMap, "stretch", 0) ? 1 : 0);
			nextMap = keyMap->next;
			if (!nextMap) {
				/* timeline.shrink(); // BOZO */
				break;
			}

			time2 = Json_getFloat(nextMap, "time", 0);
			mix2 = Json_getFloat(nextMap, "mix", 1);
			softness2 = Json_getFloat(nextMap, "softness", 0) * scale;
			curve = Json_getItem(keyMap, "curve");
			if (curve) {
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, mix, mix2, 1);
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, softness, softness2, scale);
			}

			time = time2;
			mix = mix2;
			softness = softness2;
			keyMap = nextMap;
		}

		spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
	}

	/* Transform constraint timelines. */
	for (constraintMap = transform ? transform->child : 0; constraintMap; constraintMap = constraintMap->next) {
		spTransformConstraintData *constraint;
		spTransformConstraintTimeline *timeline;
		int constraintIndex;
		float time, mixRotate, mixShearY, mixX, mixY, mixScaleX, mixScaleY;
		keyMap = constraintMap->child;
		if (keyMap == NULL) continue;

		constraint = spSkeletonData_findTransformConstraint(skeletonData, constraintMap->name);
		constraintIndex = findTransformConstraintIndex(self, skeletonData, constraint, timelines);
		if (constraintIndex == -1) return NULL;
		timeline = spTransformConstraintTimeline_create(constraintMap->size, constraintMap->size * 6, constraintIndex);

		time = Json_getFloat(keyMap, "time", 0);
		mixRotate = Json_getFloat(keyMap, "mixRotate", 1);
		mixShearY = Json_getFloat(keyMap, "mixShearY", 1);
		mixX = Json_getFloat(keyMap, "mixX", 1);
		mixY = Json_getFloat(keyMap, "mixY", mixX);
		mixScaleX = Json_getFloat(keyMap, "mixScaleX", 1);
		mixScaleY = Json_getFloat(keyMap, "mixScaleY", mixScaleX);

		for (frame = 0, bezier = 0;; ++frame) {
			float time2, mixRotate2, mixShearY2, mixX2, mixY2, mixScaleX2, mixScaleY2;
			spTransformConstraintTimeline_setFrame(timeline, frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY,
												   mixShearY);
			nextMap = keyMap->next;
			if (!nextMap) {
				/* timeline.shrink(); // BOZO */
				break;
			}

			time2 = Json_getFloat(nextMap, "time", 0);
			mixRotate2 = Json_getFloat(nextMap, "mixRotate", 1);
			mixShearY2 = Json_getFloat(nextMap, "mixShearY", 1);
			mixX2 = Json_getFloat(nextMap, "mixX", 1);
			mixY2 = Json_getFloat(nextMap, "mixY", mixX2);
			mixScaleX2 = Json_getFloat(nextMap, "mixScaleX", 1);
			mixScaleY2 = Json_getFloat(nextMap, "mixScaleY", mixScaleX2);
			curve = Json_getItem(keyMap, "curve");
			if (curve) {
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, mixRotate, mixRotate2, 1);
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, mixX, mixX2, 1);
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 2, time, time2, mixY, mixY2, 1);
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
				bezier = readCurve(curve, SUPER(timeline), bezier, frame, 5, time, time2, mixShearY, mixShearY2, 1);
			}

			time = time2;
			mixRotate = mixRotate2;
			mixX = mixX2;
			mixY = mixY2;
			mixScaleX = mixScaleX2;
			mixScaleY = mixScaleY2;
			mixScaleX = mixScaleX2;
			keyMap = nextMap;
		}

		spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
	}

	/** Path constraint timelines. */
	for (constraintMap = paths ? paths->child : 0; constraintMap; constraintMap = constraintMap->next) {
		spPathConstraintData *constraint = spSkeletonData_findPathConstraint(skeletonData, constraintMap->name);
		int constraintIndex = findPathConstraintIndex(self, skeletonData, constraint, timelines);
		if (constraintIndex == -1) return NULL;
		for (timelineMap = constraintMap->child; timelineMap; timelineMap = timelineMap->next) {
			const char *timelineName;
			int frames;
			keyMap = timelineMap->child;
			if (keyMap == NULL) continue;
			frames = timelineMap->size;
			timelineName = timelineMap->name;
			if (strcmp(timelineName, "position") == 0) {
				spPathConstraintPositionTimeline *timeline = spPathConstraintPositionTimeline_create(frames,
																									 frames,
																									 constraintIndex);
				spTimelineArray_add(timelines, readTimeline(keyMap, SUPER(timeline), 0,
															constraint->positionMode == SP_POSITION_MODE_FIXED ? scale : 1));
			} else if (strcmp(timelineName, "spacing") == 0) {
				spCurveTimeline1 *timeline = SUPER(
						spPathConstraintSpacingTimeline_create(frames, frames, constraintIndex));
				spTimelineArray_add(timelines, readTimeline(keyMap, timeline, 0,
															constraint->spacingMode == SP_SPACING_MODE_LENGTH ||
																			constraint->spacingMode == SP_SPACING_MODE_FIXED
																	? scale
																	: 1));
			} else if (strcmp(timelineName, "mix") == 0) {
				spPathConstraintMixTimeline *timeline = spPathConstraintMixTimeline_create(frames,
																						   frames * 3,
																						   constraintIndex);
				float time = Json_getFloat(keyMap, "time", 0);
				float mixRotate = Json_getFloat(keyMap, "mixRotate", 1);
				float mixX = Json_getFloat(keyMap, "mixX", 1);
				float mixY = Json_getFloat(keyMap, "mixY", mixX);
				for (frame = 0, bezier = 0;; ++frame) {
					float time2, mixRotate2, mixX2, mixY2;
					spPathConstraintMixTimeline_setFrame(timeline, frame, time, mixRotate, mixX, mixY);
					nextMap = keyMap->next;
					if (!nextMap) {
						/* timeline.shrink(); // BOZO */
						break;
					}

					time2 = Json_getFloat(nextMap, "time", 0);
					mixRotate2 = Json_getFloat(nextMap, "mixRotate", 1);
					mixX2 = Json_getFloat(nextMap, "mixX", 1);
					mixY2 = Json_getFloat(nextMap, "mixY", mixX2);
					curve = Json_getItem(keyMap, "curve");
					if (curve != NULL) {
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, mixRotate, mixRotate2,
										   1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 1, time, time2, mixX, mixX2, 1);
						bezier = readCurve(curve, SUPER(timeline), bezier, frame, 2, time, time2, mixY, mixY2, 1);
					}
					time = time2;
					mixRotate = mixRotate2;
					mixX = mixX2;
					mixY = mixY2;
					keyMap = nextMap;
				}
				spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
			}
		}
	}

	/** Physics constraint timelines. */
	for (constraintMap = physics ? physics->child : 0; constraintMap; constraintMap = constraintMap->next) {
		int index = -1;
		if (constraintMap->name && strlen(constraintMap->name) > 0) {
			spPhysicsConstraintData *constraint = spSkeletonData_findPhysicsConstraint(skeletonData, constraintMap->name);
			index = findPhysicsConstraintIndex(self, skeletonData, constraint, timelines);
			if (index == -1) return NULL;
		}
		for (timelineMap = constraintMap->child; timelineMap; timelineMap = timelineMap->next) {
			keyMap = timelineMap->child;
			if (keyMap == NULL) continue;
			const char *timelineName = timelineMap->name;
			int frames = timelineMap->size;
			if (strcmp(timelineName, "reset") == 0) {
				spPhysicsConstraintResetTimeline *timeline = spPhysicsConstraintResetTimeline_create(frames, index);
				for (frame = 0; keyMap != NULL; keyMap = keyMap->next, frame++) {
					spPhysicsConstraintResetTimeline_setFrame(timeline, frame, Json_getFloat(keyMap, "time", 0));
				}
				spTimelineArray_add(timelines, SUPER(timeline));
				continue;
			}

			spPhysicsConstraintTimeline *timeline = NULL;
			if (strcmp(timelineName, "inertia") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA);
			} else if (strcmp(timelineName, "strength") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH);
			} else if (strcmp(timelineName, "damping") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING);
			} else if (strcmp(timelineName, "mass") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_MASS);
			} else if (strcmp(timelineName, "wind") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_WIND);
			} else if (strcmp(timelineName, "gravity") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY);
			} else if (strcmp(timelineName, "mix") == 0) {
				timeline = spPhysicsConstraintTimeline_create(frames, frames, index, SP_TIMELINE_PHYSICSCONSTRAINT_MIX);
			} else {
				continue;
			}
			spTimelineArray_add(timelines, readTimeline(keyMap, SUPER(timeline), 0, 1));
		}
	}

	/* Attachment timelines. */
	for (attachmentsMap = attachmentsJson ? attachmentsJson->child : 0; attachmentsMap; attachmentsMap = attachmentsMap->next) {
		spSkin *skin = spSkeletonData_findSkin(skeletonData, attachmentsMap->name);
		for (slotMap = attachmentsMap->child; slotMap; slotMap = slotMap->next) {
			Json *attachmentMap;
			int slotIndex = findSlotIndex(self, skeletonData, slotMap->name, timelines);
			if (slotIndex == -1) return NULL;

			for (attachmentMap = slotMap->child; attachmentMap; attachmentMap = attachmentMap->next) {
				spAttachment *baseAttachment = spSkin_getAttachment(skin, slotIndex, attachmentMap->name);
				if (!baseAttachment) {
					cleanUpTimelines(timelines);
					_spSkeletonJson_setError(self, 0, "Attachment not found: ", attachmentMap->name);
					return NULL;
				}

				for (timelineMap = attachmentMap->child; timelineMap; timelineMap = timelineMap->next) {
					int frames;
					const char *timelineName;
					keyMap = timelineMap->child;
					if (keyMap == NULL) continue;
					frames = timelineMap->size;
					timelineName = timelineMap->name;
					if (!strcmp("deform", timelineName)) {
						float *tempDeform;
						spVertexAttachment *vertexAttachment;
						int weighted, deformLength;
						spDeformTimeline *timeline;
						float time;

						vertexAttachment = SUB_CAST(spVertexAttachment, baseAttachment);
						weighted = vertexAttachment->bones != 0;
						deformLength = weighted ? vertexAttachment->verticesCount / 3 * 2 : vertexAttachment->verticesCount;
						tempDeform = MALLOC(float, deformLength);

						timeline = spDeformTimeline_create(timelineMap->size, deformLength, timelineMap->size,
														   slotIndex,
														   vertexAttachment);

						time = Json_getFloat(keyMap, "time", 0);
						for (frame = 0, bezier = 0;; ++frame) {
							Json *vertices = Json_getItem(keyMap, "vertices");
							float *deform;
							float time2;

							if (!vertices) {
								if (weighted) {
									deform = tempDeform;
									memset(deform, 0, sizeof(float) * deformLength);
								} else
									deform = vertexAttachment->vertices;
							} else {
								int v, start = Json_getInt(keyMap, "offset", 0);
								Json *vertex;
								deform = tempDeform;
								memset(deform, 0, sizeof(float) * start);
								if (self->scale == 1) {
									for (vertex = vertices->child, v = start; vertex; vertex = vertex->next, ++v)
										deform[v] = vertex->valueFloat;
								} else {
									for (vertex = vertices->child, v = start; vertex; vertex = vertex->next, ++v)
										deform[v] = vertex->valueFloat * self->scale;
								}
								memset(deform + v, 0, sizeof(float) * (deformLength - v));
								if (!weighted) {
									float *verticesValues = vertexAttachment->vertices;
									for (v = 0; v < deformLength; ++v)
										deform[v] += verticesValues[v];
								}
							}
							spDeformTimeline_setFrame(timeline, frame, time, deform);
							nextMap = keyMap->next;
							if (!nextMap) {
								/* timeline.shrink(); // BOZO */
								break;
							}
							time2 = Json_getFloat(nextMap, "time", 0);
							curve = Json_getItem(keyMap, "curve");
							if (curve) {
								bezier = readCurve(curve, SUPER(timeline), bezier, frame, 0, time, time2, 0, 1, 1);
							}
							time = time2;
							keyMap = nextMap;
						}
						FREE(tempDeform);

						spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
					} else if (!strcmp(timelineName, "sequence")) {
						spSequenceTimeline *timeline = spSequenceTimeline_create(frames, slotIndex, baseAttachment);
						float lastDelay = 0;
						for (frame = 0; keyMap != NULL; keyMap = keyMap->next, frame++) {
							float delay = Json_getFloat(keyMap, "delay", lastDelay);
							float time = Json_getFloat(keyMap, "time", 0);
							const char *modeString = Json_getString(keyMap, "mode", "hold");
							int index = Json_getInt(keyMap, "index", 0);
							int mode = SP_SEQUENCE_MODE_HOLD;
							if (!strcmp(modeString, "once")) mode = SP_SEQUENCE_MODE_ONCE;
							if (!strcmp(modeString, "loop")) mode = SP_SEQUENCE_MODE_LOOP;
							if (!strcmp(modeString, "pingpong")) mode = SP_SEQUENCE_MODE_PINGPONG;
							if (!strcmp(modeString, "onceReverse")) mode = SP_SEQUENCE_MODE_ONCEREVERSE;
							if (!strcmp(modeString, "loopReverse")) mode = SP_SEQUENCE_MODE_LOOPREVERSE;
							if (!strcmp(modeString, "pingpongReverse")) mode = SP_SEQUENCE_MODE_PINGPONGREVERSE;
							spSequenceTimeline_setFrame(timeline, frame, time, mode, index, delay);
							lastDelay = delay;
						}
						spTimelineArray_add(timelines, SUPER(timeline));
					}
				}
			}
		}
	}

	/* Draw order timeline. */
	if (drawOrderJson) {
		spDrawOrderTimeline *timeline = spDrawOrderTimeline_create(drawOrderJson->size, skeletonData->slotsCount);
		for (keyMap = drawOrderJson->child, frame = 0; keyMap; keyMap = keyMap->next, ++frame) {
			int ii;
			int *drawOrder = 0;
			Json *offsets = Json_getItem(keyMap, "offsets");
			if (offsets) {
				Json *offsetMap;
				int *unchanged = MALLOC(int, skeletonData->slotsCount - offsets->size);
				int originalIndex = 0, unchangedIndex = 0;

				drawOrder = MALLOC(int, skeletonData->slotsCount);
				for (ii = skeletonData->slotsCount - 1; ii >= 0; --ii)
					drawOrder[ii] = -1;

				for (offsetMap = offsets->child; offsetMap; offsetMap = offsetMap->next) {
					int slotIndex = findSlotIndex(self, skeletonData, Json_getString(offsetMap, "slot", 0), timelines);
					if (slotIndex == -1) return NULL;

					/* Collect unchanged items. */
					while (originalIndex != slotIndex)
						unchanged[unchangedIndex++] = originalIndex++;
					/* Set changed items. */
					drawOrder[originalIndex + Json_getInt(offsetMap, "offset", 0)] = originalIndex;
					originalIndex++;
				}
				/* Collect remaining unchanged items. */
				while (originalIndex < skeletonData->slotsCount)
					unchanged[unchangedIndex++] = originalIndex++;
				/* Fill in unchanged items. */
				for (ii = skeletonData->slotsCount - 1; ii >= 0; ii--)
					if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
				FREE(unchanged);
			}
			spDrawOrderTimeline_setFrame(timeline, frame, Json_getFloat(keyMap, "time", 0), drawOrder);
			FREE(drawOrder);
		}

		spTimelineArray_add(timelines, SUPER(timeline));
	}

	/* Event timeline. */
	if (events) {
		spEventTimeline *timeline = spEventTimeline_create(events->size);
		for (keyMap = events->child, frame = 0; keyMap; keyMap = keyMap->next, ++frame) {
			spEvent *event;
			const char *stringValue;
			spEventData *eventData = spSkeletonData_findEvent(skeletonData, Json_getString(keyMap, "name", 0));
			if (!eventData) {
				cleanUpTimelines(timelines);
				_spSkeletonJson_setError(self, 0, "Event not found: ", Json_getString(keyMap, "name", 0));
				return NULL;
			}
			event = spEvent_create(Json_getFloat(keyMap, "time", 0), eventData);
			event->intValue = Json_getInt(keyMap, "int", eventData->intValue);
			event->floatValue = Json_getFloat(keyMap, "float", eventData->floatValue);
			stringValue = Json_getString(keyMap, "string", eventData->stringValue);
			if (stringValue) MALLOC_STR(event->stringValue, stringValue);
			if (eventData->audioPath) {
				event->volume = Json_getFloat(keyMap, "volume", 1);
				event->balance = Json_getFloat(keyMap, "volume", 0);
			}
			spEventTimeline_setFrame(timeline, frame, event);
		}
		spTimelineArray_add(timelines, SUPER(timeline));
	}

	duration = 0;
	for (i = 0, n = timelines->size; i < n; ++i)
		duration = MAX(duration, spTimeline_getDuration(timelines->items[i]));
	return spAnimation_create(root->name, timelines, duration);
}

static void
_readVertices(spSkeletonJson *self, Json *attachmentMap, spVertexAttachment *attachment, int verticesLength) {
	Json *entry;
	float *vertices;
	int i, n, nn, entrySize;
	spFloatArray *weights;
	spIntArray *bones;

	attachment->worldVerticesLength = verticesLength;

	entry = Json_getItem(attachmentMap, "vertices");
	entrySize = entry->size;
	vertices = MALLOC(float, entrySize);
	for (entry = entry->child, i = 0; entry; entry = entry->next, ++i)
		vertices[i] = entry->valueFloat;

	if (verticesLength == entrySize) {
		if (self->scale != 1)
			for (i = 0; i < entrySize; ++i)
				vertices[i] *= self->scale;
		attachment->verticesCount = verticesLength;
		attachment->vertices = vertices;

		attachment->bonesCount = 0;
		attachment->bones = 0;
		return;
	}

	weights = spFloatArray_create(verticesLength * 3 * 3);
	bones = spIntArray_create(verticesLength * 3);

	for (i = 0, n = entrySize; i < n;) {
		int boneCount = (int) vertices[i++];
		spIntArray_add(bones, boneCount);
		for (nn = i + boneCount * 4; i < nn; i += 4) {
			spIntArray_add(bones, (int) vertices[i]);
			spFloatArray_add(weights, vertices[i + 1] * self->scale);
			spFloatArray_add(weights, vertices[i + 2] * self->scale);
			spFloatArray_add(weights, vertices[i + 3]);
		}
	}

	attachment->verticesCount = weights->size;
	attachment->vertices = weights->items;
	FREE(weights);
	attachment->bonesCount = bones->size;
	attachment->bones = bones->items;
	FREE(bones);

	FREE(vertices);
}

spSkeletonData *spSkeletonJson_readSkeletonDataFile(spSkeletonJson *self, const char *path) {
	int length;
	spSkeletonData *skeletonData;
	const char *json = _spUtil_readFile(path, &length);
	if (length == 0 || !json) {
		_spSkeletonJson_setError(self, 0, "Unable to read skeleton file: ", path);
		return NULL;
	}
	skeletonData = spSkeletonJson_readSkeletonData(self, json);
	FREE(json);
	return skeletonData;
}

static int string_starts_with(const char *str, const char *needle) {
	int lenStr, lenNeedle, i;
	if (!str) return 0;
	lenStr = (int) strlen(str);
	lenNeedle = (int) strlen(needle);
	if (lenStr < lenNeedle) return 0;
	for (i = 0; i < lenNeedle; i++) {
		if (str[i] != needle[i]) return 0;
	}
	return -1;
}

spSkeletonData *spSkeletonJson_readSkeletonData(spSkeletonJson *self, const char *json) {
	int i, ii;
	spSkeletonData *skeletonData;
	Json *root, *skeleton, *bones, *boneMap, *ik, *transform, *pathJson, *physics, *slots, *skins, *animations, *events;
	_spSkeletonJson *internal = SUB_CAST(_spSkeletonJson, self);

	FREE(self->error);
	self->error = 0;
	internal->linkedMeshCount = 0;

	root = Json_create(json);
	if (!root) {
		_spSkeletonJson_setError(self, 0, "Invalid skeleton JSON: ", Json_getError());
		return NULL;
	}

	skeletonData = spSkeletonData_create();

	skeleton = Json_getItem(root, "skeleton");
	if (skeleton) {
		MALLOC_STR(skeletonData->hash, Json_getString(skeleton, "hash", "0"));
		MALLOC_STR(skeletonData->version, Json_getString(skeleton, "spine", "0"));
		if (!string_starts_with(skeletonData->version, SPINE_VERSION_STRING)) {
			char errorMsg[255];
			snprintf(errorMsg, 255, "Skeleton version %s does not match runtime version %s", skeletonData->version, SPINE_VERSION_STRING);
			_spSkeletonJson_setError(self, 0, errorMsg, NULL);
			return NULL;
		}
		skeletonData->x = Json_getFloat(skeleton, "x", 0);
		skeletonData->y = Json_getFloat(skeleton, "y", 0);
		skeletonData->width = Json_getFloat(skeleton, "width", 0);
		skeletonData->height = Json_getFloat(skeleton, "height", 0);
		skeletonData->referenceScale = Json_getFloat(skeleton, "referenceScale", 100) * self->scale;
		skeletonData->fps = Json_getFloat(skeleton, "fps", 30);
		skeletonData->imagesPath = Json_getString(skeleton, "images", 0);
		if (skeletonData->imagesPath) {
			char *tmp = NULL;
			MALLOC_STR(tmp, skeletonData->imagesPath);
			skeletonData->imagesPath = tmp;
		}
		skeletonData->audioPath = Json_getString(skeleton, "audio", 0);
		if (skeletonData->audioPath) {
			char *tmp = NULL;
			MALLOC_STR(tmp, skeletonData->audioPath);
			skeletonData->audioPath = tmp;
		}
	}

	/* Bones. */
	bones = Json_getItem(root, "bones");
	skeletonData->bones = MALLOC(spBoneData *, bones->size);
	for (boneMap = bones->child, i = 0; boneMap; boneMap = boneMap->next, ++i) {
		spBoneData *data;
		const char *inherit;
		const char *color;

		spBoneData *parent = 0;
		const char *parentName = Json_getString(boneMap, "parent", 0);
		if (parentName) {
			parent = spSkeletonData_findBone(skeletonData, parentName);
			if (!parent) {
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Parent bone not found: ", parentName);
				return NULL;
			}
		}

		data = spBoneData_create(skeletonData->bonesCount, Json_getString(boneMap, "name", 0), parent);
		data->length = Json_getFloat(boneMap, "length", 0) * self->scale;
		data->x = Json_getFloat(boneMap, "x", 0) * self->scale;
		data->y = Json_getFloat(boneMap, "y", 0) * self->scale;
		data->rotation = Json_getFloat(boneMap, "rotation", 0);
		data->scaleX = Json_getFloat(boneMap, "scaleX", 1);
		data->scaleY = Json_getFloat(boneMap, "scaleY", 1);
		data->shearX = Json_getFloat(boneMap, "shearX", 0);
		data->shearY = Json_getFloat(boneMap, "shearY", 0);
		inherit = Json_getString(boneMap, "inherit", "normal");
		data->inherit = SP_INHERIT_NORMAL;
		if (strcmp(inherit, "normal") == 0) data->inherit = SP_INHERIT_NORMAL;
		else if (strcmp(inherit, "onlyTranslation") == 0)
			data->inherit = SP_INHERIT_ONLYTRANSLATION;
		else if (strcmp(inherit, "noRotationOrReflection") == 0)
			data->inherit = SP_INHERIT_NOROTATIONORREFLECTION;
		else if (strcmp(inherit, "noScale") == 0)
			data->inherit = SP_INHERIT_NOSCALE;
		else if (strcmp(inherit, "noScaleOrReflection") == 0)
			data->inherit = SP_INHERIT_NOSCALEORREFLECTION;
		data->skinRequired = Json_getInt(boneMap, "skin", 0) ? 1 : 0;

		color = Json_getString(boneMap, "color", 0);
		if (color) toColor2(&data->color, color, -1);

		data->icon = Json_getString(boneMap, "icon", "");
		if (data->icon) {
			char *tmp = NULL;
			MALLOC_STR(tmp, data->icon);
			data->icon = tmp;
		}
		data->visible = Json_getInt(boneMap, "visible", -1) ? -1 : 0;

		skeletonData->bones[i] = data;
		skeletonData->bonesCount++;
	}

	/* Slots. */
	slots = Json_getItem(root, "slots");
	if (slots) {
		Json *slotMap;
		skeletonData->slots = MALLOC(spSlotData *, slots->size);
		for (slotMap = slots->child, i = 0; slotMap; slotMap = slotMap->next, ++i) {
			spSlotData *data;
			const char *color;
			const char *dark;
			Json *item;

			const char *boneName = Json_getString(slotMap, "bone", 0);
			spBoneData *boneData = spSkeletonData_findBone(skeletonData, boneName);
			if (!boneData) {
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Slot bone not found: ", boneName);
				return NULL;
			}

			char *slotName = (char *) Json_getString(slotMap, "name", NULL);
			data = spSlotData_create(i, slotName, boneData);

			color = Json_getString(slotMap, "color", 0);
			if (color) {
				spColor_setFromFloats(&data->color,
									  toColor(color, 0),
									  toColor(color, 1),
									  toColor(color, 2),
									  toColor(color, 3));
			}

			dark = Json_getString(slotMap, "dark", 0);
			if (dark) {
				data->darkColor = spColor_create();
				spColor_setFromFloats(data->darkColor,
									  toColor(dark, 0),
									  toColor(dark, 1),
									  toColor(dark, 2),
									  1.0f);
			}

			item = Json_getItem(slotMap, "attachment");
			if (item) spSlotData_setAttachmentName(data, item->valueString);

			item = Json_getItem(slotMap, "blend");
			if (item) {
				if (strcmp(item->valueString, "additive") == 0)
					data->blendMode = SP_BLEND_MODE_ADDITIVE;
				else if (strcmp(item->valueString, "multiply") == 0)
					data->blendMode = SP_BLEND_MODE_MULTIPLY;
				else if (strcmp(item->valueString, "screen") == 0)
					data->blendMode = SP_BLEND_MODE_SCREEN;
			}

			data->visible = Json_getInt(slotMap, "visible", -1);
			skeletonData->slots[i] = data;
			skeletonData->slotsCount++;
		}
	}

	/* IK constraints. */
	ik = Json_getItem(root, "ik");
	if (ik) {
		Json *constraintMap;
		skeletonData->ikConstraints = MALLOC(spIkConstraintData *, ik->size);
		for (constraintMap = ik->child, i = 0; constraintMap; constraintMap = constraintMap->next, ++i) {
			const char *targetName;

			spIkConstraintData *data = spIkConstraintData_create(Json_getString(constraintMap, "name", 0));
			data->order = Json_getInt(constraintMap, "order", 0);
			data->skinRequired = Json_getInt(constraintMap, "skin", 0) ? 1 : 0;

			boneMap = Json_getItem(constraintMap, "bones");
			data->bonesCount = boneMap->size;
			data->bones = MALLOC(spBoneData *, boneMap->size);
			for (boneMap = boneMap->child, ii = 0; boneMap; boneMap = boneMap->next, ++ii) {
				data->bones[ii] = spSkeletonData_findBone(skeletonData, boneMap->valueString);
				if (!data->bones[ii]) {
					spIkConstraintData_dispose(data);
					spSkeletonData_dispose(skeletonData);
					_spSkeletonJson_setError(self, root, "IK bone not found: ", boneMap->valueString);
					return NULL;
				}
			}

			targetName = Json_getString(constraintMap, "target", 0);
			data->target = spSkeletonData_findBone(skeletonData, targetName);
			if (!data->target) {
				spIkConstraintData_dispose(data);
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Target bone not found: ", targetName);
				return NULL;
			}

			data->bendDirection = Json_getInt(constraintMap, "bendPositive", 1) ? 1 : -1;
			data->compress = Json_getInt(constraintMap, "compress", 0) ? 1 : 0;
			data->stretch = Json_getInt(constraintMap, "stretch", 0) ? 1 : 0;
			data->uniform = Json_getInt(constraintMap, "uniform", 0) ? 1 : 0;
			data->mix = Json_getFloat(constraintMap, "mix", 1);
			data->softness = Json_getFloat(constraintMap, "softness", 0) * self->scale;

			skeletonData->ikConstraints[i] = data;
			skeletonData->ikConstraintsCount++;
		}
	}

	/* Transform constraints. */
	transform = Json_getItem(root, "transform");
	if (transform) {
		Json *constraintMap;
		skeletonData->transformConstraints = MALLOC(spTransformConstraintData *, transform->size);
		for (constraintMap = transform->child, i = 0; constraintMap; constraintMap = constraintMap->next, ++i) {
			const char *name;

			spTransformConstraintData *data = spTransformConstraintData_create(
					Json_getString(constraintMap, "name", 0));
			data->order = Json_getInt(constraintMap, "order", 0);
			data->skinRequired = Json_getInt(constraintMap, "skin", 0) ? 1 : 0;

			boneMap = Json_getItem(constraintMap, "bones");
			data->bonesCount = boneMap->size;
			data->bones = MALLOC(spBoneData *, boneMap->size);
			for (boneMap = boneMap->child, ii = 0; boneMap; boneMap = boneMap->next, ++ii) {
				data->bones[ii] = spSkeletonData_findBone(skeletonData, boneMap->valueString);
				if (!data->bones[ii]) {
					spTransformConstraintData_dispose(data);
					spSkeletonData_dispose(skeletonData);
					_spSkeletonJson_setError(self, root, "Transform bone not found: ", boneMap->valueString);
					return NULL;
				}
			}

			name = Json_getString(constraintMap, "target", 0);
			data->target = spSkeletonData_findBone(skeletonData, name);
			if (!data->target) {
				spTransformConstraintData_dispose(data);
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Target bone not found: ", name);
				return NULL;
			}

			data->local = Json_getInt(constraintMap, "local", 0);
			data->relative = Json_getInt(constraintMap, "relative", 0);
			data->offsetRotation = Json_getFloat(constraintMap, "rotation", 0);
			data->offsetX = Json_getFloat(constraintMap, "x", 0) * self->scale;
			data->offsetY = Json_getFloat(constraintMap, "y", 0) * self->scale;
			data->offsetScaleX = Json_getFloat(constraintMap, "scaleX", 0);
			data->offsetScaleY = Json_getFloat(constraintMap, "scaleY", 0);
			data->offsetShearY = Json_getFloat(constraintMap, "shearY", 0);

			data->mixRotate = Json_getFloat(constraintMap, "mixRotate", 1);
			data->mixX = Json_getFloat(constraintMap, "mixX", 1);
			data->mixY = Json_getFloat(constraintMap, "mixY", data->mixX);
			data->mixScaleX = Json_getFloat(constraintMap, "mixScaleX", 1);
			data->mixScaleY = Json_getFloat(constraintMap, "mixScaleY", data->mixScaleX);
			data->mixShearY = Json_getFloat(constraintMap, "mixShearY", 1);

			skeletonData->transformConstraints[i] = data;
			skeletonData->transformConstraintsCount++;
		}
	}

	/* Path constraints */
	pathJson = Json_getItem(root, "path");
	if (pathJson) {
		Json *constraintMap;
		skeletonData->pathConstraints = MALLOC(spPathConstraintData *, pathJson->size);
		for (constraintMap = pathJson->child, i = 0; constraintMap; constraintMap = constraintMap->next, ++i) {
			const char *name;
			const char *item;

			spPathConstraintData *data = spPathConstraintData_create(Json_getString(constraintMap, "name", 0));
			data->order = Json_getInt(constraintMap, "order", 0);
			data->skinRequired = Json_getInt(constraintMap, "skin", 0) ? 1 : 0;

			boneMap = Json_getItem(constraintMap, "bones");
			data->bonesCount = boneMap->size;
			data->bones = MALLOC(spBoneData *, boneMap->size);
			for (boneMap = boneMap->child, ii = 0; boneMap; boneMap = boneMap->next, ++ii) {
				data->bones[ii] = spSkeletonData_findBone(skeletonData, boneMap->valueString);
				if (!data->bones[ii]) {
					spPathConstraintData_dispose(data);
					spSkeletonData_dispose(skeletonData);
					_spSkeletonJson_setError(self, root, "Path bone not found: ", boneMap->valueString);
					return NULL;
				}
			}

			name = Json_getString(constraintMap, "target", 0);
			data->target = spSkeletonData_findSlot(skeletonData, name);
			if (!data->target) {
				spPathConstraintData_dispose(data);
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Target slot not found: ", name);
				return NULL;
			}

			item = Json_getString(constraintMap, "positionMode", "percent");
			if (strcmp(item, "fixed") == 0) data->positionMode = SP_POSITION_MODE_FIXED;
			else if (strcmp(item, "percent") == 0)
				data->positionMode = SP_POSITION_MODE_PERCENT;

			item = Json_getString(constraintMap, "spacingMode", "length");
			if (strcmp(item, "length") == 0) data->spacingMode = SP_SPACING_MODE_LENGTH;
			else if (strcmp(item, "fixed") == 0)
				data->spacingMode = SP_SPACING_MODE_FIXED;
			else if (strcmp(item, "percent") == 0)
				data->spacingMode = SP_SPACING_MODE_PERCENT;
			else
				data->spacingMode = SP_SPACING_MODE_PROPORTIONAL;

			item = Json_getString(constraintMap, "rotateMode", "tangent");
			if (strcmp(item, "tangent") == 0) data->rotateMode = SP_ROTATE_MODE_TANGENT;
			else if (strcmp(item, "chain") == 0)
				data->rotateMode = SP_ROTATE_MODE_CHAIN;
			else if (strcmp(item, "chainScale") == 0)
				data->rotateMode = SP_ROTATE_MODE_CHAIN_SCALE;

			data->offsetRotation = Json_getFloat(constraintMap, "rotation", 0);
			data->position = Json_getFloat(constraintMap, "position", 0);
			if (data->positionMode == SP_POSITION_MODE_FIXED) data->position *= self->scale;
			data->spacing = Json_getFloat(constraintMap, "spacing", 0);
			if (data->spacingMode == SP_SPACING_MODE_LENGTH || data->spacingMode == SP_SPACING_MODE_FIXED)
				data->spacing *= self->scale;
			data->mixRotate = Json_getFloat(constraintMap, "mixRotate", 1);
			data->mixX = Json_getFloat(constraintMap, "mixX", 1);
			data->mixY = Json_getFloat(constraintMap, "mixY", data->mixX);

			skeletonData->pathConstraints[i] = data;
			skeletonData->pathConstraintsCount++;
		}
	}

	/* Physics constraints */
	physics = Json_getItem(root, "physics");
	if (physics) {
		Json *constraintMap;
		skeletonData->physicsConstraintsCount = physics->size;
		skeletonData->physicsConstraints = MALLOC(spPhysicsConstraintData *, physics->size);
		for (constraintMap = physics->child, i = 0; constraintMap; constraintMap = constraintMap->next, ++i) {
			const char *name;

			spPhysicsConstraintData *data = spPhysicsConstraintData_create(
					Json_getString(constraintMap, "name", 0));
			data->order = Json_getInt(constraintMap, "order", 0);
			data->skinRequired = Json_getInt(constraintMap, "skin", 0);

			name = Json_getString(constraintMap, "bone", 0);
			data->bone = spSkeletonData_findBone(skeletonData, name);
			if (!data->bone) {
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Physics bone not found: ", name);
				return NULL;
			}

			data->x = Json_getFloat(constraintMap, "x", 0);
			data->y = Json_getFloat(constraintMap, "y", 0);
			data->rotate = Json_getFloat(constraintMap, "rotate", 0);
			data->scaleX = Json_getFloat(constraintMap, "scaleX", 0);
			data->shearX = Json_getFloat(constraintMap, "shearX", 0);
			data->limit = Json_getFloat(constraintMap, "limit", 5000) * self->scale;
			data->step = 1.0f / Json_getInt(constraintMap, "fps", 60);
			data->inertia = Json_getFloat(constraintMap, "inertia", 1);
			data->strength = Json_getFloat(constraintMap, "strength", 100);
			data->damping = Json_getFloat(constraintMap, "damping", 1);
			data->massInverse = 1.0f / Json_getFloat(constraintMap, "mass", 1);
			data->wind = Json_getFloat(constraintMap, "wind", 0);
			data->gravity = Json_getFloat(constraintMap, "gravity", 0);
			data->mix = Json_getFloat(constraintMap, "mix", 1);
			data->inertiaGlobal = Json_getInt(constraintMap, "inertiaGlobal", 0);
			data->strengthGlobal = Json_getInt(constraintMap, "strengthGlobal", 0);
			data->dampingGlobal = Json_getInt(constraintMap, "dampingGlobal", 0);
			data->massGlobal = Json_getInt(constraintMap, "massGlobal", 0);
			data->windGlobal = Json_getInt(constraintMap, "windGlobal", 0);
			data->gravityGlobal = Json_getInt(constraintMap, "gravityGlobal", 0);
			data->mixGlobal = Json_getInt(constraintMap, "mixGlobal", 0);

			skeletonData->physicsConstraints[i] = data;
		}
	}

	/* Skins. */
	skins = Json_getItem(root, "skins");
	if (skins) {
		Json *skinMap;
		skeletonData->skins = MALLOC(spSkin *, skins->size);
		for (skinMap = skins->child, i = 0; skinMap; skinMap = skinMap->next, ++i) {
			Json *attachmentsMap;
			Json *curves;
			Json *skinPart;
			spSkin *skin = spSkin_create(Json_getString(skinMap, "name", ""));

			skinPart = Json_getItem(skinMap, "bones");
			if (skinPart) {
				for (skinPart = skinPart->child; skinPart; skinPart = skinPart->next) {
					spBoneData *bone = spSkeletonData_findBone(skeletonData, skinPart->valueString);
					if (!bone) {
						spSkin_dispose(skin);
						spSkeletonData_dispose(skeletonData);
						_spSkeletonJson_setError(self, root, "Skin bone constraint not found: ", skinPart->valueString);
						return NULL;
					}
					spBoneDataArray_add(skin->bones, bone);
				}
			}

			skinPart = Json_getItem(skinMap, "ik");
			if (skinPart) {
				for (skinPart = skinPart->child; skinPart; skinPart = skinPart->next) {
					spIkConstraintData *constraint = spSkeletonData_findIkConstraint(skeletonData,
																					 skinPart->valueString);
					if (!constraint) {
						spSkin_dispose(skin);
						spSkeletonData_dispose(skeletonData);
						_spSkeletonJson_setError(self, root, "Skin IK constraint not found: ", skinPart->valueString);
						return NULL;
					}
					spIkConstraintDataArray_add(skin->ikConstraints, constraint);
				}
			}

			skinPart = Json_getItem(skinMap, "path");
			if (skinPart) {
				for (skinPart = skinPart->child; skinPart; skinPart = skinPart->next) {
					spPathConstraintData *constraint = spSkeletonData_findPathConstraint(skeletonData,
																						 skinPart->valueString);
					if (!constraint) {
						spSkin_dispose(skin);
						spSkeletonData_dispose(skeletonData);
						_spSkeletonJson_setError(self, root, "Skin path constraint not found: ", skinPart->valueString);
						return NULL;
					}
					spPathConstraintDataArray_add(skin->pathConstraints, constraint);
				}
			}

			skinPart = Json_getItem(skinMap, "transform");
			if (skinPart) {
				for (skinPart = skinPart->child; skinPart; skinPart = skinPart->next) {
					spTransformConstraintData *constraint = spSkeletonData_findTransformConstraint(skeletonData,
																								   skinPart->valueString);
					if (!constraint) {
						spSkin_dispose(skin);
						spSkeletonData_dispose(skeletonData);
						_spSkeletonJson_setError(self, root, "Skin transform constraint not found: ",
												 skinPart->valueString);
						return NULL;
					}
					spTransformConstraintDataArray_add(skin->transformConstraints, constraint);
				}
			}

			skinPart = Json_getItem(skinMap, "physics");
			if (skinPart) {
				for (skinPart = skinPart->child; skinPart; skinPart = skinPart->next) {
					spPhysicsConstraintData *constraint = spSkeletonData_findPhysicsConstraint(skeletonData,
																							   skinPart->valueString);
					if (!constraint) {
						spSkeletonData_dispose(skeletonData);
						_spSkeletonJson_setError(self, root, "Skin physics constraint not found: ", skinPart->valueString);
						return NULL;
					}
					spPhysicsConstraintDataArray_add(skin->physicsConstraints, constraint);
				}
			}

			skeletonData->skins[skeletonData->skinsCount++] = skin;
			if (strcmp(skin->name, "default") == 0) skeletonData->defaultSkin = skin;

			skinPart = Json_getItem(skinMap, "attachments");
			if (skinPart) {
				for (attachmentsMap = skinPart->child; attachmentsMap; attachmentsMap = attachmentsMap->next) {
					spSlotData *slot = spSkeletonData_findSlot(skeletonData, attachmentsMap->name);
					Json *attachmentMap;

					for (attachmentMap = attachmentsMap->child; attachmentMap; attachmentMap = attachmentMap->next) {
						spAttachment *attachment;
						const char *skinAttachmentName = attachmentMap->name;
						const char *attachmentName = Json_getString(attachmentMap, "name", skinAttachmentName);
						const char *path = Json_getString(attachmentMap, "path", attachmentName);
						const char *color;
						Json *entry;
						spSequence *sequence;

						const char *typeString = Json_getString(attachmentMap, "type", "region");
						spAttachmentType type;
						if (strcmp(typeString, "region") == 0) type = SP_ATTACHMENT_REGION;
						else if (strcmp(typeString, "mesh") == 0)
							type = SP_ATTACHMENT_MESH;
						else if (strcmp(typeString, "linkedmesh") == 0)
							type = SP_ATTACHMENT_LINKED_MESH;
						else if (strcmp(typeString, "boundingbox") == 0)
							type = SP_ATTACHMENT_BOUNDING_BOX;
						else if (strcmp(typeString, "path") == 0)
							type = SP_ATTACHMENT_PATH;
						else if (strcmp(typeString, "clipping") == 0)
							type = SP_ATTACHMENT_CLIPPING;
						else if (strcmp(typeString, "point") == 0)
							type = SP_ATTACHMENT_POINT;
						else {
							spSkeletonData_dispose(skeletonData);
							_spSkeletonJson_setError(self, root, "Unknown attachment type: ", typeString);
							return NULL;
						}

						sequence = readSequence(Json_getItem(attachmentMap, "sequence"));
						attachment = spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type,
																		 attachmentName,
																		 path, sequence);
						if (!attachment) {
							if (self->attachmentLoader->error1) {
								spSkeletonData_dispose(skeletonData);
								_spSkeletonJson_setError(self, root, self->attachmentLoader->error1,
														 self->attachmentLoader->error2);
								return NULL;
							}
							continue;
						}

						switch (attachment->type) {
							case SP_ATTACHMENT_REGION: {
								spRegionAttachment *region = SUB_CAST(spRegionAttachment, attachment);
								if (path) MALLOC_STR(region->path, path);
								region->x = Json_getFloat(attachmentMap, "x", 0) * self->scale;
								region->y = Json_getFloat(attachmentMap, "y", 0) * self->scale;
								region->scaleX = Json_getFloat(attachmentMap, "scaleX", 1);
								region->scaleY = Json_getFloat(attachmentMap, "scaleY", 1);
								region->rotation = Json_getFloat(attachmentMap, "rotation", 0);
								region->width = Json_getFloat(attachmentMap, "width", 32) * self->scale;
								region->height = Json_getFloat(attachmentMap, "height", 32) * self->scale;
								region->sequence = sequence;

								color = Json_getString(attachmentMap, "color", 0);
								if (color) {
									spColor_setFromFloats(&region->color,
														  toColor(color, 0),
														  toColor(color, 1),
														  toColor(color, 2),
														  toColor(color, 3));
								}

								if (region->region != NULL) spRegionAttachment_updateRegion(region);

								spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
								break;
							}
							case SP_ATTACHMENT_MESH:
							case SP_ATTACHMENT_LINKED_MESH: {
								spMeshAttachment *mesh = SUB_CAST(spMeshAttachment, attachment);

								MALLOC_STR(mesh->path, path);

								color = Json_getString(attachmentMap, "color", 0);
								if (color) {
									spColor_setFromFloats(&mesh->color,
														  toColor(color, 0),
														  toColor(color, 1),
														  toColor(color, 2),
														  toColor(color, 3));
								}

								mesh->width = Json_getFloat(attachmentMap, "width", 32) * self->scale;
								mesh->height = Json_getFloat(attachmentMap, "height", 32) * self->scale;
								mesh->sequence = sequence;

								entry = Json_getItem(attachmentMap, "parent");
								if (!entry) {
									int verticesLength;
									entry = Json_getItem(attachmentMap, "triangles");
									mesh->trianglesCount = entry->size;
									mesh->triangles = MALLOC(unsigned short, entry->size);
									for (entry = entry->child, ii = 0; entry; entry = entry->next, ++ii)
										mesh->triangles[ii] = (unsigned short) entry->valueInt;

									entry = Json_getItem(attachmentMap, "uvs");
									verticesLength = entry->size;
									mesh->regionUVs = MALLOC(float, verticesLength);
									for (entry = entry->child, ii = 0; entry; entry = entry->next, ++ii)
										mesh->regionUVs[ii] = entry->valueFloat;

									_readVertices(self, attachmentMap, SUPER(mesh), verticesLength);

									if (mesh->region != NULL) spMeshAttachment_updateRegion(mesh);

									mesh->hullLength = Json_getInt(attachmentMap, "hull", 0);

									entry = Json_getItem(attachmentMap, "edges");
									if (entry) {
										mesh->edgesCount = entry->size;
										mesh->edges = MALLOC(unsigned short, entry->size);
										for (entry = entry->child, ii = 0; entry; entry = entry->next, ++ii)
											mesh->edges[ii] = (unsigned short) entry->valueInt;
									}

									spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
								} else {
									int inheritTimelines = Json_getInt(attachmentMap, "timelines", 1);
									_spSkeletonJson_addLinkedMesh(self, SUB_CAST(spMeshAttachment, attachment),
																  Json_getString(attachmentMap, "skin", 0), slot->index,
																  entry->valueString, inheritTimelines);
								}
								break;
							}
							case SP_ATTACHMENT_BOUNDING_BOX: {
								spBoundingBoxAttachment *box = SUB_CAST(spBoundingBoxAttachment, attachment);
								int vertexCount = Json_getInt(attachmentMap, "vertexCount", 0) << 1;
								_readVertices(self, attachmentMap, SUPER(box), vertexCount);
								box->super.verticesCount = vertexCount;
								color = Json_getString(attachmentMap, "color", 0);
								if (color) {
									spColor_setFromFloats(&box->color,
														  toColor(color, 0),
														  toColor(color, 1),
														  toColor(color, 2),
														  toColor(color, 3));
								}
								spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
								break;
							}
							case SP_ATTACHMENT_PATH: {
								spPathAttachment *pathAttachment = SUB_CAST(spPathAttachment, attachment);
								int vertexCount = 0;
								pathAttachment->closed = Json_getInt(attachmentMap, "closed", 0);
								pathAttachment->constantSpeed = Json_getInt(attachmentMap, "constantSpeed", 1);
								vertexCount = Json_getInt(attachmentMap, "vertexCount", 0);
								_readVertices(self, attachmentMap, SUPER(pathAttachment), vertexCount << 1);

								pathAttachment->lengthsLength = vertexCount / 3;
								pathAttachment->lengths = MALLOC(float, pathAttachment->lengthsLength);

								curves = Json_getItem(attachmentMap, "lengths");
								for (curves = curves->child, ii = 0; curves; curves = curves->next, ++ii)
									pathAttachment->lengths[ii] = curves->valueFloat * self->scale;
								color = Json_getString(attachmentMap, "color", 0);
								if (color) {
									spColor_setFromFloats(&pathAttachment->color,
														  toColor(color, 0),
														  toColor(color, 1),
														  toColor(color, 2),
														  toColor(color, 3));
								}
								break;
							}
							case SP_ATTACHMENT_POINT: {
								spPointAttachment *point = SUB_CAST(spPointAttachment, attachment);
								point->x = Json_getFloat(attachmentMap, "x", 0) * self->scale;
								point->y = Json_getFloat(attachmentMap, "y", 0) * self->scale;
								point->rotation = Json_getFloat(attachmentMap, "rotation", 0);

								color = Json_getString(attachmentMap, "color", 0);
								if (color) {
									spColor_setFromFloats(&point->color,
														  toColor(color, 0),
														  toColor(color, 1),
														  toColor(color, 2),
														  toColor(color, 3));
								}
								break;
							}
							case SP_ATTACHMENT_CLIPPING: {
								spClippingAttachment *clip = SUB_CAST(spClippingAttachment, attachment);
								int vertexCount = 0;
								const char *end = Json_getString(attachmentMap, "end", 0);
								if (end) {
									spSlotData *endSlot = spSkeletonData_findSlot(skeletonData, end);
									clip->endSlot = endSlot;
								}
								vertexCount = Json_getInt(attachmentMap, "vertexCount", 0) << 1;
								_readVertices(self, attachmentMap, SUPER(clip), vertexCount);
								color = Json_getString(attachmentMap, "color", 0);
								if (color) {
									spColor_setFromFloats(&clip->color,
														  toColor(color, 0),
														  toColor(color, 1),
														  toColor(color, 2),
														  toColor(color, 3));
								}
								spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
								break;
							}
						}

						spSkin_setAttachment(skin, slot->index, skinAttachmentName, attachment);
					}
				}
			}
		}
	}

	/* Linked meshes. */
	for (i = 0; i < internal->linkedMeshCount; ++i) {
		spAttachment *parent;
		_spLinkedMesh *linkedMesh = internal->linkedMeshes + i;
		spSkin *skin = !linkedMesh->skin ? skeletonData->defaultSkin : spSkeletonData_findSkin(skeletonData, linkedMesh->skin);
		if (!skin) {
			spSkeletonData_dispose(skeletonData);
			_spSkeletonJson_setError(self, root, "Skin not found: ", linkedMesh->skin);
			return NULL;
		}
		parent = spSkin_getAttachment(skin, linkedMesh->slotIndex, linkedMesh->parent);
		if (!parent) {
			spSkeletonData_dispose(skeletonData);
			_spSkeletonJson_setError(self, root, "Parent mesh not found: ", linkedMesh->parent);
			return NULL;
		}
		linkedMesh->mesh->super.timelineAttachment = linkedMesh->inheritTimeline ? parent
																				 : SUPER(SUPER(linkedMesh->mesh));
		spMeshAttachment_setParentMesh(linkedMesh->mesh, SUB_CAST(spMeshAttachment, parent));
		if (linkedMesh->mesh->region != NULL) spMeshAttachment_updateRegion(linkedMesh->mesh);
		spAttachmentLoader_configureAttachment(self->attachmentLoader, SUPER(SUPER(linkedMesh->mesh)));
	}

	/* Events. */
	events = Json_getItem(root, "events");
	if (events) {
		Json *eventMap;
		const char *stringValue;
		const char *audioPath;
		skeletonData->eventsCount = events->size;
		skeletonData->events = MALLOC(spEventData *, events->size);
		for (eventMap = events->child, i = 0; eventMap; eventMap = eventMap->next, ++i) {
			spEventData *eventData = spEventData_create(eventMap->name);
			eventData->intValue = Json_getInt(eventMap, "int", 0);
			eventData->floatValue = Json_getFloat(eventMap, "float", 0);
			stringValue = Json_getString(eventMap, "string", 0);
			if (stringValue) MALLOC_STR(eventData->stringValue, stringValue);
			audioPath = Json_getString(eventMap, "audio", 0);
			if (audioPath) {
				MALLOC_STR(eventData->audioPath, audioPath);
				eventData->volume = Json_getFloat(eventMap, "volume", 1);
				eventData->balance = Json_getFloat(eventMap, "balance", 0);
			}
			skeletonData->events[i] = eventData;
		}
	}

	/* Animations. */
	animations = Json_getItem(root, "animations");
	if (animations) {
		Json *animationMap;
		skeletonData->animations = MALLOC(spAnimation *, animations->size);
		for (animationMap = animations->child; animationMap; animationMap = animationMap->next) {
			spAnimation *animation = _spSkeletonJson_readAnimation(self, animationMap, skeletonData);
			if (!animation) {
				spSkeletonData_dispose(skeletonData);
				_spSkeletonJson_setError(self, root, "Animation broken: ", animationMap->name);
				return NULL;
			}
			skeletonData->animations[skeletonData->animationsCount++] = animation;
		}
	}

	Json_dispose(root);
	return skeletonData;
}
