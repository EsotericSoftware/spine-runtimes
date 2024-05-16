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

#include <spine/Animation.h>
#include <spine/Array.h>
#include <spine/AtlasAttachmentLoader.h>
#include <spine/SkeletonBinary.h>
#include <spine/extension.h>
#include <spine/Version.h>
#include <stdio.h>

typedef struct {
	const unsigned char *cursor;
	const unsigned char *end;
} _dataInput;

typedef struct {
	const char *parent;
	int skinIndex;
	int slotIndex;
	spMeshAttachment *mesh;
	int inheritTimeline;
} _spLinkedMesh;

typedef struct {
	spSkeletonBinary super;
	int ownsLoader;

	int linkedMeshCount;
	int linkedMeshCapacity;
	_spLinkedMesh *linkedMeshes;
} _spSkeletonBinary;

static int string_starts_with(const char *str, const char *needle) {
	int lenStr, lenNeedle, i;
	if (!str) return 0;
	lenStr = strlen(str);
	lenNeedle = strlen(needle);
	if (lenStr < lenNeedle) return 0;
	for (i = 0; i < lenNeedle; i++) {
		if (str[i] != needle[i]) return 0;
	}
	return -1;
}

static char *string_copy(const char *str) {
	if (str == NULL) return NULL;
	int len = strlen(str);
	char *tmp = (char *) malloc(len + 1);
	strncpy(tmp, str, len);
	tmp[len] = '\0';
	return tmp;
}

spSkeletonBinary *spSkeletonBinary_createWithLoader(spAttachmentLoader *attachmentLoader) {
	spSkeletonBinary *self = SUPER(NEW(_spSkeletonBinary));
	self->scale = 1;
	self->attachmentLoader = attachmentLoader;
	return self;
}

spSkeletonBinary *spSkeletonBinary_create(spAtlas *atlas) {
	spAtlasAttachmentLoader *attachmentLoader = spAtlasAttachmentLoader_create(atlas);
	spSkeletonBinary *self = spSkeletonBinary_createWithLoader(SUPER(attachmentLoader));
	SUB_CAST(_spSkeletonBinary, self)->ownsLoader = 1;
	return self;
}

void spSkeletonBinary_dispose(spSkeletonBinary *self) {
	_spSkeletonBinary *internal = SUB_CAST(_spSkeletonBinary, self);
	if (internal->ownsLoader) spAttachmentLoader_dispose(self->attachmentLoader);
	FREE(internal->linkedMeshes);
	FREE(self->error);
	FREE(self);
}

void _spSkeletonBinary_setError(spSkeletonBinary *self, const char *value1, const char *value2) {
	char message[256];
	int length;
	FREE(self->error);
	strcpy(message, value1);
	length = (int) strlen(value1);
	if (value2) strncat(message + length, value2, 255 - length);
	MALLOC_STR(self->error, message);
}

static unsigned char readByte(_dataInput *input) {
	return *input->cursor++;
}

static signed char readSByte(_dataInput *input) {
	return (signed char) readByte(input);
}

static int readBoolean(_dataInput *input) {
	return readByte(input) != 0;
}

static int readInt(_dataInput *input) {
	uint32_t result = readByte(input);
	result <<= 8;
	result |= readByte(input);
	result <<= 8;
	result |= readByte(input);
	result <<= 8;
	result |= readByte(input);
	return (int) result;
}

static int readVarint(_dataInput *input, int /*bool*/ optimizePositive) {
	unsigned char b = readByte(input);
	int32_t value = b & 0x7F;
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
	if (!optimizePositive) value = (((unsigned int) value >> 1) ^ -(value & 1));
	return value;
}

float readFloat(_dataInput *input) {
	union {
		int intValue;
		float floatValue;
	} intToFloat;
	intToFloat.intValue = readInt(input);
	return intToFloat.floatValue;
}

char *readString(_dataInput *input) {
	int length = readVarint(input, 1);
	char *string;
	if (length == 0) return NULL;
	string = MALLOC(char, length);
	memcpy(string, input->cursor, length - 1);
	input->cursor += length - 1;
	string[length - 1] = '\0';
	return string;
}

static char *readStringRef(_dataInput *input, spSkeletonData *skeletonData) {
	int index = readVarint(input, 1);
	return index == 0 ? 0 : skeletonData->strings[index - 1];
}

static void readColor(_dataInput *input, float *r, float *g, float *b, float *a) {
	*r = readByte(input) / 255.0f;
	*g = readByte(input) / 255.0f;
	*b = readByte(input) / 255.0f;
	*a = readByte(input) / 255.0f;
}

#define ATTACHMENT_REGION 0
#define ATTACHMENT_BOUNDING_BOX 1
#define ATTACHMENT_MESH 2
#define ATTACHMENT_LINKED_MESH 3
#define ATTACHMENT_PATH 4

#define BLEND_MODE_NORMAL 0
#define BLEND_MODE_ADDITIVE 1
#define BLEND_MODE_MULTIPLY 2
#define BLEND_MODE_SCREEN 3

#define BONE_ROTATE 0
#define BONE_TRANSLATE 1
#define BONE_TRANSLATEX 2
#define BONE_TRANSLATEY 3
#define BONE_SCALE 4
#define BONE_SCALEX 5
#define BONE_SCALEY 6
#define BONE_SHEAR 7
#define BONE_SHEARX 8
#define BONE_SHEARY 9
#define BONE_INHERIT 10

#define SLOT_ATTACHMENT 0
#define SLOT_RGBA 1
#define SLOT_RGB 2
#define SLOT_RGBA2 3
#define SLOT_RGB2 4
#define SLOT_ALPHA 5

#define ATTACHMENT_DEFORM 0
#define ATTACHMENT_SEQUENCE 1

#define PATH_POSITION 0
#define PATH_SPACING 1
#define PATH_MIX 2

#define CURVE_LINEAR 0
#define CURVE_STEPPED 1
#define CURVE_BEZIER 2

#define PATH_POSITION_FIXED 0
#define PATH_POSITION_PERCENT 1

#define PATH_SPACING_LENGTH 0
#define PATH_SPACING_FIXED 1
#define PATH_SPACING_PERCENT 2

#define PATH_ROTATE_TANGENT 0
#define PATH_ROTATE_CHAIN 1
#define PATH_ROTATE_CHAIN_SCALE 2

#define PHYSICS_INERTIA 0
#define PHYSICS_STRENGTH 1
#define PHYSICS_DAMPING 2
#define PHYSICS_MASS 4
#define PHYSICS_WIND 5
#define PHYSICS_GRAVITY 6
#define PHYSICS_MIX 7
#define PHYSICS_RESET 8

static spSequence *readSequence(_dataInput *input) {
	spSequence *sequence = spSequence_create(readVarint(input, -1));
	sequence->start = readVarint(input, -1);
	sequence->digits = readVarint(input, -1);
	sequence->setupIndex = readVarint(input, -1);
	return sequence;
}

static void
setBezier(_dataInput *input, spTimeline *timeline, int bezier, int frame, int value, float time1, float time2,
		  float value1, float value2, float scale) {
	float cx1 = readFloat(input);
	float cy1 = readFloat(input);
	float cx2 = readFloat(input);
	float cy2 = readFloat(input);
	spTimeline_setBezier(timeline, bezier, frame, value, time1, value1, cx1, cy1 * scale, cx2, cy2 * scale, time2,
						 value2);
}

static void readTimeline(_dataInput *input, spTimelineArray *timelines, spCurveTimeline1 *timeline, float scale) {
	int frame, bezier, frameLast;
	float time2, value2;
	float time = readFloat(input);
	float value = readFloat(input) * scale;
	for (frame = 0, bezier = 0, frameLast = timeline->super.frameCount - 1;; frame++) {
		spCurveTimeline1_setFrame(timeline, frame, time, value);
		if (frame == frameLast) break;
		time2 = readFloat(input);
		value2 = readFloat(input) * scale;
		switch (readSByte(input)) {
			case CURVE_STEPPED:
				spCurveTimeline_setStepped(timeline, frame);
				break;
			case CURVE_BEZIER:
				setBezier(input, SUPER(timeline), bezier++, frame, 0, time, time2, value, value2, scale);
		}
		time = time2;
		value = value2;
	}
	spTimelineArray_add(timelines, SUPER(timeline));
}

static void readTimeline2(_dataInput *input, spTimelineArray *timelines, spCurveTimeline2 *timeline, float scale) {
	int frame, bezier, frameLast;
	float time2, nvalue1, nvalue2;
	float time = readFloat(input);
	float value1 = readFloat(input) * scale;
	float value2 = readFloat(input) * scale;
	for (frame = 0, bezier = 0, frameLast = timeline->super.frameCount - 1;; frame++) {
		spCurveTimeline2_setFrame(timeline, frame, time, value1, value2);
		if (frame == frameLast) break;
		time2 = readFloat(input);
		nvalue1 = readFloat(input) * scale;
		nvalue2 = readFloat(input) * scale;
		switch (readSByte(input)) {
			case CURVE_STEPPED:
				spCurveTimeline_setStepped(timeline, frame);
				break;
			case CURVE_BEZIER:
				setBezier(input, SUPER(timeline), bezier++, frame, 0, time, time2, value1, nvalue1, scale);
				setBezier(input, SUPER(timeline), bezier++, frame, 1, time, time2, value2, nvalue2, scale);
		}
		time = time2;
		value1 = nvalue1;
		value2 = nvalue2;
	}
	spTimelineArray_add(timelines, SUPER(timeline));
}

static void _spSkeletonBinary_addLinkedMesh(spSkeletonBinary *self, spMeshAttachment *mesh,
											int skinIndex, int slotIndex, const char *parent, int inheritDeform) {
	_spLinkedMesh *linkedMesh;
	_spSkeletonBinary *internal = SUB_CAST(_spSkeletonBinary, self);

	if (internal->linkedMeshCount == internal->linkedMeshCapacity) {
		_spLinkedMesh *linkedMeshes;
		internal->linkedMeshCapacity *= 2;
		if (internal->linkedMeshCapacity < 8) internal->linkedMeshCapacity = 8;
		/* TODO Why not realloc? */
		linkedMeshes = MALLOC(_spLinkedMesh, internal->linkedMeshCapacity);
		memcpy(linkedMeshes, internal->linkedMeshes, sizeof(_spLinkedMesh) * internal->linkedMeshCount);
		FREE(internal->linkedMeshes);
		internal->linkedMeshes = linkedMeshes;
	}

	linkedMesh = internal->linkedMeshes + internal->linkedMeshCount++;
	linkedMesh->mesh = mesh;
	linkedMesh->skinIndex = skinIndex;
	linkedMesh->slotIndex = slotIndex;
	linkedMesh->parent = parent;
	linkedMesh->inheritTimeline = inheritDeform;
}

static spAnimation *_spSkeletonBinary_readAnimation(spSkeletonBinary *self, const char *name,
													_dataInput *input, spSkeletonData *skeletonData) {
	spTimelineArray *timelines = spTimelineArray_create(18);
	float duration = 0;
	int i, n, ii, nn, iii, nnn;
	int frame, bezier;
	int drawOrderCount, eventCount;
	spAnimation *animation;
	float scale = self->scale;

	int numTimelines = readVarint(input, 1);
	UNUSED(numTimelines);

	/* Slot timelines. */
	for (i = 0, n = readVarint(input, 1); i < n; ++i) {
		int slotIndex = readVarint(input, 1);
		for (ii = 0, nn = readVarint(input, 1); ii < nn; ++ii) {
			unsigned char timelineType = readByte(input);
			int frameCount = readVarint(input, 1);
			int frameLast = frameCount - 1;
			switch (timelineType) {
				case SLOT_ATTACHMENT: {
					spAttachmentTimeline *timeline = spAttachmentTimeline_create(frameCount, slotIndex);
					for (frame = 0; frame < frameCount; ++frame) {
						float time = readFloat(input);
						const char *attachmentName = readStringRef(input, skeletonData);
						spAttachmentTimeline_setFrame(timeline, frame, time, attachmentName);
					}
					spTimelineArray_add(timelines, SUPER(timeline));
					break;
				}
				case SLOT_RGBA: {
					int bezierCount = readVarint(input, 1);
					spRGBATimeline *timeline = spRGBATimeline_create(frameCount, bezierCount, slotIndex);

					float time = readFloat(input);
					float r = readByte(input) / 255.0;
					float g = readByte(input) / 255.0;
					float b = readByte(input) / 255.0;
					float a = readByte(input) / 255.0;

					for (frame = 0, bezier = 0;; frame++) {
						float time2, r2, g2, b2, a2;
						spRGBATimeline_setFrame(timeline, frame, time, r, g, b, a);
						if (frame == frameLast) break;

						time2 = readFloat(input);
						r2 = readByte(input) / 255.0;
						g2 = readByte(input) / 255.0;
						b2 = readByte(input) / 255.0;
						a2 = readByte(input) / 255.0;

						switch (readSByte(input)) {
							case CURVE_STEPPED:
								spCurveTimeline_setStepped(SUPER(timeline), frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, r, r2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, g, g2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 2, time, time2, b, b2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 3, time, time2, a, a2, 1);
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
						a = a2;
					}
					spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
					break;
				}
				case SLOT_RGB: {
					int bezierCount = readVarint(input, 1);
					spRGBTimeline *timeline = spRGBTimeline_create(frameCount, bezierCount, slotIndex);

					float time = readFloat(input);
					float r = readByte(input) / 255.0;
					float g = readByte(input) / 255.0;
					float b = readByte(input) / 255.0;

					for (frame = 0, bezier = 0;; frame++) {
						float time2, r2, g2, b2;
						spRGBTimeline_setFrame(timeline, frame, time, r, g, b);
						if (frame == frameLast) break;

						time2 = readFloat(input);
						r2 = readByte(input) / 255.0;
						g2 = readByte(input) / 255.0;
						b2 = readByte(input) / 255.0;

						switch (readSByte(input)) {
							case CURVE_STEPPED:
								spCurveTimeline_setStepped(SUPER(timeline), frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, r, r2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, g, g2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 2, time, time2, b, b2, 1);
						}
						time = time2;
						r = r2;
						g = g2;
						b = b2;
					}
					spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
					break;
				}
				case SLOT_RGBA2: {
					int bezierCount = readVarint(input, 1);
					spRGBA2Timeline *timeline = spRGBA2Timeline_create(frameCount, bezierCount, slotIndex);

					float time = readFloat(input);
					float r = readByte(input) / 255.0;
					float g = readByte(input) / 255.0;
					float b = readByte(input) / 255.0;
					float a = readByte(input) / 255.0;
					float r2 = readByte(input) / 255.0;
					float g2 = readByte(input) / 255.0;
					float b2 = readByte(input) / 255.0;

					for (frame = 0, bezier = 0;; frame++) {
						float time2, nr, ng, nb, na, nr2, ng2, nb2;
						spRGBA2Timeline_setFrame(timeline, frame, time, r, g, b, a, r2, g2, b2);
						if (frame == frameLast) break;
						time2 = readFloat(input);
						nr = readByte(input) / 255.0;
						ng = readByte(input) / 255.0;
						nb = readByte(input) / 255.0;
						na = readByte(input) / 255.0;
						nr2 = readByte(input) / 255.0;
						ng2 = readByte(input) / 255.0;
						nb2 = readByte(input) / 255.0;

						switch (readSByte(input)) {
							case CURVE_STEPPED:
								spCurveTimeline_setStepped(SUPER(timeline), frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, r, nr, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, g, ng, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 2, time, time2, b, nb, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 3, time, time2, a, na, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 4, time, time2, r2, nr2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 5, time, time2, g2, ng2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 6, time, time2, b2, nb2, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						a = na;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
					}
					spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
					break;
				}
				case SLOT_RGB2: {
					int bezierCount = readVarint(input, 1);
					spRGB2Timeline *timeline = spRGB2Timeline_create(frameCount, bezierCount, slotIndex);

					float time = readFloat(input);
					float r = readByte(input) / 255.0;
					float g = readByte(input) / 255.0;
					float b = readByte(input) / 255.0;
					float r2 = readByte(input) / 255.0;
					float g2 = readByte(input) / 255.0;
					float b2 = readByte(input) / 255.0;

					for (frame = 0, bezier = 0;; frame++) {
						float time2, nr, ng, nb, nr2, ng2, nb2;
						spRGB2Timeline_setFrame(timeline, frame, time, r, g, b, r2, g2, b2);
						if (frame == frameLast) break;
						time2 = readFloat(input);
						nr = readByte(input) / 255.0;
						ng = readByte(input) / 255.0;
						nb = readByte(input) / 255.0;
						nr2 = readByte(input) / 255.0;
						ng2 = readByte(input) / 255.0;
						nb2 = readByte(input) / 255.0;

						switch (readSByte(input)) {
							case CURVE_STEPPED:
								spCurveTimeline_setStepped(SUPER(timeline), frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, r, nr, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, g, ng, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 2, time, time2, b, nb, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 3, time, time2, r2, nr2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 4, time, time2, g2, ng2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 5, time, time2, b2, nb2, 1);
						}
						time = time2;
						r = nr;
						g = ng;
						b = nb;
						r2 = nr2;
						g2 = ng2;
						b2 = nb2;
					}
					spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
					break;
				}
				case SLOT_ALPHA: {
					int bezierCount = readVarint(input, 1);
					spAlphaTimeline *timeline = spAlphaTimeline_create(frameCount, bezierCount, slotIndex);
					float time = readFloat(input);
					float a = readByte(input) / 255.0;
					for (frame = 0, bezier = 0;; frame++) {
						float time2, a2;
						spAlphaTimeline_setFrame(timeline, frame, time, a);
						if (frame == frameLast) break;
						time2 = readFloat(input);
						a2 = readByte(input) / 255;
						switch (readSByte(input)) {
							case CURVE_STEPPED:
								spCurveTimeline_setStepped(SUPER(timeline), frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, a, a2, 1);
						}
						time = time2;
						a = a2;
					}
					spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
					break;
				}
				default: {
					return NULL;
				}
			}
		}
	}

	/* Bone timelines. */
	for (i = 0, n = readVarint(input, 1); i < n; ++i) {
		int boneIndex = readVarint(input, 1);
		for (ii = 0, nn = readVarint(input, 1); ii < nn; ++ii) {
			unsigned char timelineType = readByte(input);
			int frameCount = readVarint(input, 1);
			if (timelineType == BONE_INHERIT) {
				spInheritTimeline *timeline = spInheritTimeline_create(frameCount, boneIndex);
				for (frame = 0; frame < frameCount; frame++) {
					float time = readFloat(input);
					spInherit inherit = (spInherit) readByte(input);
					spInheritTimeline_setFrame(timeline, frame, time, inherit);
				}
				spTimelineArray_add(timelines, SUPER(timeline));
				continue;
			}
			int bezierCount = readVarint(input, 1);
			switch (timelineType) {
				case BONE_ROTATE:
					readTimeline(input, timelines, SUPER(spRotateTimeline_create(frameCount, bezierCount, boneIndex)),
								 1);
					break;
				case BONE_TRANSLATE:
					readTimeline2(input, timelines,
								  SUPER(spTranslateTimeline_create(frameCount, bezierCount, boneIndex)),
								  scale);
					break;
				case BONE_TRANSLATEX:
					readTimeline(input, timelines,
								 SUPER(spTranslateXTimeline_create(frameCount, bezierCount, boneIndex)),
								 scale);
					break;
				case BONE_TRANSLATEY:
					readTimeline(input, timelines,
								 SUPER(spTranslateYTimeline_create(frameCount, bezierCount, boneIndex)),
								 scale);
					break;
				case BONE_SCALE:
					readTimeline2(input, timelines, SUPER(spScaleTimeline_create(frameCount, bezierCount, boneIndex)),
								  1);
					break;
				case BONE_SCALEX:
					readTimeline(input, timelines, SUPER(spScaleXTimeline_create(frameCount, bezierCount, boneIndex)),
								 1);
					break;
				case BONE_SCALEY:
					readTimeline(input, timelines, SUPER(spScaleYTimeline_create(frameCount, bezierCount, boneIndex)),
								 1);
					break;
				case BONE_SHEAR:
					readTimeline2(input, timelines, SUPER(spShearTimeline_create(frameCount, bezierCount, boneIndex)),
								  1);
					break;
				case BONE_SHEARX:
					readTimeline(input, timelines, SUPER(spShearXTimeline_create(frameCount, bezierCount, boneIndex)),
								 1);
					break;
				case BONE_SHEARY:
					readTimeline(input, timelines, SUPER(spShearYTimeline_create(frameCount, bezierCount, boneIndex)),
								 1);
					break;
				default: {
					for (iii = 0; iii < timelines->size; ++iii)
						spTimeline_dispose(timelines->items[iii]);
					spTimelineArray_dispose(timelines);
					_spSkeletonBinary_setError(self, "Invalid timeline type for a bone: ",
											   skeletonData->bones[boneIndex]->name);
					return NULL;
				}
			}
		}
	}

	/* IK constraint timelines. */
	for (i = 0, n = readVarint(input, 1); i < n; ++i) {
		int index = readVarint(input, 1);
		int frameCount = readVarint(input, 1);
		int frameLast = frameCount - 1;
		int bezierCount = readVarint(input, 1);
		spIkConstraintTimeline *timeline = spIkConstraintTimeline_create(frameCount, bezierCount, index);
		int flags = readByte(input);
		float time = readFloat(input), mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? readFloat(input) : 1) : 0;
		float softness = (flags & 4) != 0 ? readFloat(input) * scale : 0;
		for (frame = 0, bezier = 0;; frame++) {
			spIkConstraintTimeline_setFrame(timeline, frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
			if (frame == frameLast) break;
			flags = readByte(input);
			float time2 = readFloat(input), mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? readFloat(input) : 1) : 0;
			float softness2 = (flags & 4) != 0 ? readFloat(input) * scale : 0;
			if ((flags & 64) != 0)
				spCurveTimeline_setStepped(SUPER(timeline), frame);
			else if ((flags & 128) != 0) {
				setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, mix, mix2, 1);
				setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, softness, softness2, scale);
			}
			time = time2;
			mix = mix2;
			softness = softness2;
		}
		spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
	}

	/* Transform constraint timelines. */
	for (i = 0, n = readVarint(input, 1); i < n; ++i) {
		int index = readVarint(input, 1);
		int frameCount = readVarint(input, 1);
		int frameLast = frameCount - 1;
		int bezierCount = readVarint(input, 1);
		spTransformConstraintTimeline *timeline = spTransformConstraintTimeline_create(frameCount, bezierCount, index);
		float time = readFloat(input);
		float mixRotate = readFloat(input);
		float mixX = readFloat(input);
		float mixY = readFloat(input);
		float mixScaleX = readFloat(input);
		float mixScaleY = readFloat(input);
		float mixShearY = readFloat(input);
		for (frame = 0, bezier = 0;; frame++) {
			float time2, mixRotate2, mixX2, mixY2, mixScaleX2, mixScaleY2, mixShearY2;
			spTransformConstraintTimeline_setFrame(timeline, frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY,
												   mixShearY);
			if (frame == frameLast) break;
			time2 = readFloat(input);
			mixRotate2 = readFloat(input);
			mixX2 = readFloat(input);
			mixY2 = readFloat(input);
			mixScaleX2 = readFloat(input);
			mixScaleY2 = readFloat(input);
			mixShearY2 = readFloat(input);
			switch (readSByte(input)) {
				case CURVE_STEPPED:
					spCurveTimeline_setStepped(SUPER(timeline), frame);
					break;
				case CURVE_BEZIER:
					setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
					setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, mixX, mixX2, 1);
					setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 2, time, time2, mixY, mixY2, 1);
					setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
					setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
					setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
			}
			time = time2;
			mixRotate = mixRotate2;
			mixX = mixX2;
			mixY = mixY2;
			mixScaleX = mixScaleX2;
			mixScaleY = mixScaleY2;
			mixShearY = mixShearY2;
		}
		spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
	}

	/* Path constraint timelines. */
	for (i = 0, n = readVarint(input, 1); i < n; ++i) {
		int index = readVarint(input, 1);
		spPathConstraintData *data = skeletonData->pathConstraints[index];
		for (ii = 0, nn = readVarint(input, 1); ii < nn; ++ii) {
			int type = readByte(input);
			int frameCount = readVarint(input, 1);
			int bezierCount = readVarint(input, 1);
			switch (type) {
				case PATH_POSITION: {
					readTimeline(input, timelines, SUPER(spPathConstraintPositionTimeline_create(frameCount, bezierCount, index)),
								 data->positionMode == SP_POSITION_MODE_FIXED ? scale
																			  : 1);
					break;
				}
				case PATH_SPACING: {
					readTimeline(input, timelines,
								 SUPER(spPathConstraintSpacingTimeline_create(frameCount,
																			  bezierCount,
																			  index)),
								 data->spacingMode == SP_SPACING_MODE_LENGTH ||
												 data->spacingMode == SP_SPACING_MODE_FIXED
										 ? scale
										 : 1);
					break;
				}
				case PATH_MIX: {
					float time, mixRotate, mixX, mixY;
					int frameLast;
					spPathConstraintMixTimeline *timeline = spPathConstraintMixTimeline_create(frameCount, bezierCount,
																							   index);
					time = readFloat(input);
					mixRotate = readFloat(input);
					mixX = readFloat(input);
					mixY = readFloat(input);
					for (frame = 0, bezier = 0, frameLast = timeline->super.super.frameCount - 1;; frame++) {
						float time2, mixRotate2, mixX2, mixY2;
						spPathConstraintMixTimeline_setFrame(timeline, frame, time, mixRotate, mixX, mixY);
						if (frame == frameLast) break;
						time2 = readFloat(input);
						mixRotate2 = readFloat(input);
						mixX2 = readFloat(input);
						mixY2 = readFloat(input);
						switch (readSByte(input)) {
							case CURVE_STEPPED:
								spCurveTimeline_setStepped(SUPER(timeline), frame);
								break;
							case CURVE_BEZIER:
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, mixRotate,
										  mixRotate2, 1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 1, time, time2, mixX, mixX2,
										  1);
								setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 2, time, time2, mixY, mixY2,
										  1);
						}
						time = time2;
						mixRotate = mixRotate2;
						mixX = mixX2;
						mixY = mixY2;
					}
					spTimelineArray_add(timelines, SUPER(SUPER(timeline)));
				}
			}
		}
	}

	for (i = 0, n = readVarint(input, 1); i < n; i++) {
		int index = readVarint(input, 1) - 1;
		for (ii = 0, nn = readVarint(input, 1); ii < nn; ii++) {
			int type = readByte(input);
			int frameCount = readVarint(input, 1);
			if (type == PHYSICS_RESET) {
				spPhysicsConstraintResetTimeline *timeline = spPhysicsConstraintResetTimeline_create(frameCount, index);
				for (frame = 0; frame < frameCount; frame++)
					spPhysicsConstraintResetTimeline_setFrame(timeline, frame, readFloat(input));
				spTimelineArray_add(timelines, SUPER(timeline));
				continue;
			}
			int bezierCount = readVarint(input, 1);
			switch (type) {
				case PHYSICS_INERTIA:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_INERTIA)), 1);
					break;
				case PHYSICS_STRENGTH:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_STRENGTH)), 1);
					break;
				case PHYSICS_DAMPING:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_DAMPING)), 1);
					break;
				case PHYSICS_MASS:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_MASS)), 1);
					break;
				case PHYSICS_WIND:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_WIND)), 1);
					break;
				case PHYSICS_GRAVITY:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_GRAVITY)), 1);
					break;
				case PHYSICS_MIX:
					readTimeline(input, timelines, SUPER(spPhysicsConstraintTimeline_create(frameCount, bezierCount, index, SP_TIMELINE_PHYSICSCONSTRAINT_MIX)), 1);
			}
		}
	}

	/* Attachment timelines. */
	for (i = 0, n = readVarint(input, 1); i < n; ++i) {
		spSkin *skin = skeletonData->skins[readVarint(input, 1)];
		for (ii = 0, nn = readVarint(input, 1); ii < nn; ++ii) {
			int slotIndex = readVarint(input, 1);
			for (iii = 0, nnn = readVarint(input, 1); iii < nnn; ++iii) {
				int frameCount, frameLast, bezierCount;
				float time, time2;
				unsigned int timelineType;

				const char *attachmentName = readStringRef(input, skeletonData);
				spVertexAttachment *attachment = SUB_CAST(spVertexAttachment,
														  spSkin_getAttachment(skin, slotIndex, attachmentName));
				if (!attachment) {
					for (i = 0; i < timelines->size; ++i)
						spTimeline_dispose(timelines->items[i]);
					spTimelineArray_dispose(timelines);
					_spSkeletonBinary_setError(self, "Attachment not found: ", attachmentName);
					return NULL;
				}

				timelineType = readByte(input);
				frameCount = readVarint(input, 1);
				frameLast = frameCount - 1;

				switch (timelineType) {
					case ATTACHMENT_DEFORM: {
						float *tempDeform;
						int weighted, deformLength;
						spDeformTimeline *timeline;
						weighted = attachment->bones != 0;
						deformLength = weighted ? attachment->verticesCount / 3 * 2 : attachment->verticesCount;
						tempDeform = MALLOC(float, deformLength);

						bezierCount = readVarint(input, 1);
						timeline = spDeformTimeline_create(frameCount, deformLength, bezierCount, slotIndex,
														   attachment);

						time = readFloat(input);
						for (frame = 0, bezier = 0;; ++frame) {
							float *deform;
							int end = readVarint(input, 1);
							if (!end) {
								if (weighted) {
									deform = tempDeform;
									memset(deform, 0, sizeof(float) * deformLength);
								} else
									deform = attachment->vertices;
							} else {
								int v, start = readVarint(input, 1);
								deform = tempDeform;
								memset(deform, 0, sizeof(float) * start);
								end += start;
								if (self->scale == 1) {
									for (v = start; v < end; ++v)
										deform[v] = readFloat(input);
								} else {
									for (v = start; v < end; ++v)
										deform[v] = readFloat(input) * self->scale;
								}
								memset(deform + v, 0, sizeof(float) * (deformLength - v));
								if (!weighted) {
									float *vertices = attachment->vertices;
									for (v = 0; v < deformLength; ++v)
										deform[v] += vertices[v];
								}
							}
							spDeformTimeline_setFrame(timeline, frame, time, deform);
							if (frame == frameLast) break;
							time2 = readFloat(input);
							switch (readSByte(input)) {
								case CURVE_STEPPED:
									spCurveTimeline_setStepped(SUPER(timeline), frame);
									break;
								case CURVE_BEZIER:
									setBezier(input, SUPER(SUPER(timeline)), bezier++, frame, 0, time, time2, 0, 1, 1);
							}
							time = time2;
						}
						FREE(tempDeform);

						spTimelineArray_add(timelines, (spTimeline *) timeline);
						break;
					}
					case ATTACHMENT_SEQUENCE: {
						int modeAndIndex;
						float delay;
						spSequenceTimeline *timeline = spSequenceTimeline_create(frameCount, slotIndex, (spAttachment *) attachment);
						for (frame = 0; frame < frameCount; frame++) {
							time = readFloat(input);
							modeAndIndex = readInt(input);
							delay = readFloat(input);
							spSequenceTimeline_setFrame(timeline, frame, time, modeAndIndex & 0xf, modeAndIndex >> 4, delay);
						}
						spTimelineArray_add(timelines, (spTimeline *) timeline);
						break;
					}
				}
			}
		}
	}

	/* Draw order timeline. */
	drawOrderCount = readVarint(input, 1);
	if (drawOrderCount) {
		spDrawOrderTimeline *timeline = spDrawOrderTimeline_create(drawOrderCount, skeletonData->slotsCount);
		for (i = 0; i < drawOrderCount; ++i) {
			float time = readFloat(input);
			int offsetCount = readVarint(input, 1);
			int *drawOrder = MALLOC(int, skeletonData->slotsCount);
			int *unchanged = MALLOC(int, skeletonData->slotsCount - offsetCount);
			int originalIndex = 0, unchangedIndex = 0;
			memset(drawOrder, -1, sizeof(int) * skeletonData->slotsCount);
			for (ii = 0; ii < offsetCount; ++ii) {
				int slotIndex = readVarint(input, 1);
				while (originalIndex != slotIndex)
					unchanged[unchangedIndex++] = originalIndex++;
				drawOrder[originalIndex + readVarint(input, 1)] = originalIndex;
				++originalIndex;
			}
			while (originalIndex < skeletonData->slotsCount)
				unchanged[unchangedIndex++] = originalIndex++;
			for (ii = skeletonData->slotsCount - 1; ii >= 0; ii--)
				if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
			FREE(unchanged);
			spDrawOrderTimeline_setFrame(timeline, i, time, drawOrder);
			FREE(drawOrder);
		}
		spTimelineArray_add(timelines, (spTimeline *) timeline);
	}

	/* Event timeline. */
	eventCount = readVarint(input, 1);
	if (eventCount) {
		spEventTimeline *timeline = spEventTimeline_create(eventCount);
		for (i = 0; i < eventCount; ++i) {
			float time = readFloat(input);
			spEventData *eventData = skeletonData->events[readVarint(input, 1)];
			spEvent *event = spEvent_create(time, eventData);
			event->intValue = readVarint(input, 0);
			event->floatValue = readFloat(input);
			const char *event_stringValue = readString(input);
			if (event_stringValue == NULL) {
				event->stringValue = string_copy(eventData->stringValue);
			} else {
				event->stringValue = string_copy(event_stringValue);
				FREE(event_stringValue);
			}

			if (eventData->audioPath) {
				event->volume = readFloat(input);
				event->balance = readFloat(input);
			}
			spEventTimeline_setFrame(timeline, i, event);
		}
		spTimelineArray_add(timelines, (spTimeline *) timeline);
	}

	duration = 0;
	for (i = 0, n = timelines->size; i < n; i++) {
		duration = MAX(duration, spTimeline_getDuration(timelines->items[i]));
	}
	animation = spAnimation_create(name, timelines, duration);
	return animation;
}

static float *_readFloatArray(_dataInput *input, int n, float scale) {
	float *array = MALLOC(float, n);
	int i;
	if (scale == 1)
		for (i = 0; i < n; ++i)
			array[i] = readFloat(input);
	else
		for (i = 0; i < n; ++i)
			array[i] = readFloat(input) * scale;
	return array;
}

static unsigned short *_readShortArray(_dataInput *input, int n) {
	unsigned short *array = MALLOC(unsigned short, n);
	int i;
	for (i = 0; i < n; ++i) {
		array[i] = (unsigned short) readVarint(input, 1);
	}
	return array;
}

static int _readVertices(_dataInput *input, float **vertices, int *verticesLength, int **bones, int *bonesCount, int /*bool*/ weighted, float scale) {
	int vertexCount = readVarint(input, 1);
	*verticesLength = vertexCount << 1;
	if (!weighted) {
		*vertices = _readFloatArray(input, *verticesLength, scale);
		*bones = NULL;
		*bonesCount = 0;
		return *verticesLength;
	}

	float *v = MALLOC(float, (*verticesLength) * 3 * 3);
	int *b = MALLOC(int, (*verticesLength) * 3);
	int boneIdx = 0;
	int vertexIdx = 0;
	for (int i = 0; i < vertexCount; ++i) {
		int boneCount = readVarint(input, 1);
		b[boneIdx++] = boneCount;
		for (int ii = 0; ii < boneCount; ++ii) {
			b[boneIdx++] = readVarint(input, 1);
			v[vertexIdx++] = readFloat(input) * scale;
			v[vertexIdx++] = readFloat(input) * scale;
			v[vertexIdx++] = readFloat(input);
		}
	}
	*vertices = v;
	*bones = b;
	*bonesCount = boneIdx;
	*verticesLength = vertexIdx;
	return vertexCount << 1;
}

spAttachment *spSkeletonBinary_readAttachment(spSkeletonBinary *self, _dataInput *input,
											  spSkin *skin, int slotIndex, const char *attachmentName,
											  spSkeletonData *skeletonData, int /*bool*/ nonessential) {
	int flags = readByte(input);
	const char *name = (flags & 8) != 0 ? readStringRef(input, skeletonData) : attachmentName;
	spAttachmentType type = (spAttachmentType) (flags & 0x7);

	switch (type) {
		case SP_ATTACHMENT_REGION: {
			char *path = (flags & 16) != 0 ? readStringRef(input, skeletonData) : (char *) name;
			path = string_copy(path);
			spColor color;
			spColor_setFromFloats(&color, 1, 1, 1, 1);
			if ((flags & 32) != 0) readColor(input, &color.r, &color.g, &color.b, &color.a);
			spSequence *sequence = (flags & 64) != 0 ? readSequence(input) : NULL;
			float rotation = (flags & 128) != 0 ? readFloat(input) : 0;
			float x = readFloat(input) * self->scale;
			float y = readFloat(input) * self->scale;
			float scaleX = readFloat(input);
			float scaleY = readFloat(input);
			float width = readFloat(input) * self->scale;
			float height = readFloat(input) * self->scale;
			spRegionAttachment *region = SUB_CAST(spRegionAttachment, spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name,
																										  path, sequence));
			region->path = path;
			region->rotation = rotation;
			region->x = x;
			region->y = y;
			region->scaleX = scaleX;
			region->scaleY = scaleY;
			region->width = width;
			region->height = height;
			spColor_setFromColor(&region->color, &color);
			region->sequence = sequence;
			if (sequence == NULL) spRegionAttachment_updateRegion(region);
			spAttachmentLoader_configureAttachment(self->attachmentLoader, SUPER(region));
			return SUPER(region);
		}
		case SP_ATTACHMENT_BOUNDING_BOX: {
			spBoundingBoxAttachment *box = SUB_CAST(spBoundingBoxAttachment, spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name, 0,
																												 NULL));
			if (!box) return NULL;
			_readVertices(input, &box->super.vertices, &box->super.verticesCount, &box->super.bones, &box->super.bonesCount, (flags & 16) != 0, self->scale);
			box->super.worldVerticesLength = box->super.verticesCount;
			if (nonessential) {
				readColor(input, &box->color.r, &box->color.g, &box->color.b, &box->color.a);
			}
			spAttachmentLoader_configureAttachment(self->attachmentLoader, SUPER(SUPER(box)));
			return SUPER(SUPER(box));
		}
		case SP_ATTACHMENT_MESH: {
			float *uvs = NULL;
			int uvsCount = 0;
			unsigned short *triangles = NULL;
			int trianglesCount = 0;
			float *vertices = NULL;
			int verticesCount = 0;
			int *bones = NULL;
			int bonesCount = 0;
			int hullLength = 0;
			float width = 0;
			float height = 0;
			unsigned short *edges = NULL;
			int edgesCount = 0;

			char *path = (flags & 16) != 0 ? readStringRef(input, skeletonData) : (char *) name;
			path = string_copy(path);
			spColor color;
			spColor_setFromFloats(&color, 1, 1, 1, 1);
			if ((flags & 32) != 0) readColor(input, &color.r, &color.g, &color.b, &color.a);
			spSequence *sequence = (flags & 64) != 0 ? readSequence(input) : NULL;
			hullLength = readVarint(input, 1);
			int verticesLength = _readVertices(input, &vertices, &verticesCount, &bones, &bonesCount, (flags & 128) != 0, self->scale);
			uvsCount = verticesLength;
			uvs = _readFloatArray(input, uvsCount, 1);
			trianglesCount = (verticesLength - hullLength - 2) * 3;
			triangles = _readShortArray(input, trianglesCount);

			if (nonessential) {
				edgesCount = readVarint(input, 1);
				edges = _readShortArray(input, edgesCount);
				width = readFloat(input);
				height = readFloat(input);
			}


			spAttachment *attachment = spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name, path, sequence);
			if (!attachment) return NULL;
			spMeshAttachment *mesh = SUB_CAST(spMeshAttachment, attachment);
			mesh->path = path;
			spColor_setFromColor(&mesh->color, &color);
			mesh->regionUVs = uvs;
			mesh->triangles = triangles;
			mesh->trianglesCount = trianglesCount;
			mesh->super.vertices = vertices;
			mesh->super.verticesCount = verticesCount;
			mesh->super.bones = bones;
			mesh->super.bonesCount = bonesCount;
			mesh->super.worldVerticesLength = verticesLength;
			mesh->hullLength = hullLength;
			mesh->edges = edges;
			mesh->edgesCount = edgesCount;
			mesh->width = width;
			mesh->height = height;
			mesh->sequence = sequence;
			if (sequence == NULL) spMeshAttachment_updateRegion(mesh);
			spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
			return attachment;
		}
		case SP_ATTACHMENT_LINKED_MESH: {
			char *path = (flags & 16) != 0 ? readStringRef(input, skeletonData) : (char *) name;
			path = string_copy(path);
			spColor color;
			spColor_setFromFloats(&color, 1, 1, 1, 1);
			if ((flags & 32) != 0) readColor(input, &color.r, &color.g, &color.b, &color.a);
			spSequence *sequence = (flags & 64) != 0 ? readSequence(input) : NULL;
			int /*bool*/ inheritTimelines = (flags & 128) != 0;
			int skinIndex = readVarint(input, 1);
			char *parent = readStringRef(input, skeletonData);
			float width = 0, height = 0;
			if (nonessential) {
				width = readFloat(input) * self->scale;
				height = readFloat(input) * self->scale;
			}
			spAttachment *attachment = spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name, path, sequence);
			spMeshAttachment *mesh = NULL;
			if (!attachment)
				return NULL;
			mesh = SUB_CAST(spMeshAttachment, attachment);
			mesh->path = (char *) path;
			if (mesh->path) {
				char *tmp = NULL;
				MALLOC_STR(tmp, mesh->path);
				mesh->path = tmp;
			}
			spColor_setFromColor(&mesh->color, &color);
			mesh->sequence = sequence;
			mesh->width = width;
			mesh->height = height;
			_spSkeletonBinary_addLinkedMesh(self, mesh, skinIndex, slotIndex, parent, inheritTimelines);
			return attachment;
		}
		case SP_ATTACHMENT_PATH: {
			spAttachment *attachment = spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name, 0,
																		   NULL);
			spPathAttachment *path = NULL;
			if (!attachment)
				return NULL;
			path = SUB_CAST(spPathAttachment, attachment);
			path->closed = (flags & 16) != 0;
			path->constantSpeed = (flags & 32) != 0;
			int verticesLength = _readVertices(input, &path->super.vertices, &path->super.verticesCount, &path->super.bones, &path->super.bonesCount, (flags & 64) != 0, self->scale);
			path->super.worldVerticesLength = verticesLength;
			path->lengthsLength = verticesLength / 6;
			path->lengths = MALLOC(float, path->lengthsLength);
			for (int i = 0; i < path->lengthsLength; ++i) {
				path->lengths[i] = readFloat(input) * self->scale;
			}
			if (nonessential) {
				readColor(input, &path->color.r, &path->color.g, &path->color.b, &path->color.a);
			}
			spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
			return attachment;
		}
		case SP_ATTACHMENT_POINT: {
			spAttachment *attachment = spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name, 0,
																		   NULL);
			spPointAttachment *point = NULL;
			if (!attachment)
				return NULL;
			point = SUB_CAST(spPointAttachment, attachment);
			point->rotation = readFloat(input);
			point->x = readFloat(input) * self->scale;
			point->y = readFloat(input) * self->scale;

			if (nonessential) {
				readColor(input, &point->color.r, &point->color.g, &point->color.b, &point->color.a);
			}
			spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
			return attachment;
		}
		case SP_ATTACHMENT_CLIPPING: {
			int endSlotIndex = readVarint(input, 1);
			spAttachment *attachment = spAttachmentLoader_createAttachment(self->attachmentLoader, skin, type, name, 0,
																		   NULL);
			spClippingAttachment *clip = NULL;
			if (!attachment)
				return NULL;
			clip = SUB_CAST(spClippingAttachment, attachment);
			int verticesLength = _readVertices(input, &clip->super.vertices, &clip->super.verticesCount, &clip->super.bones, &clip->super.bonesCount, (flags & 16) != 0, self->scale);
			clip->super.worldVerticesLength = verticesLength;
			if (nonessential) {
				readColor(input, &clip->color.r, &clip->color.g, &clip->color.b, &clip->color.a);
			}
			clip->endSlot = skeletonData->slots[endSlotIndex];
			spAttachmentLoader_configureAttachment(self->attachmentLoader, attachment);
			return attachment;
		}
	}

	return NULL;
}

spSkin *spSkeletonBinary_readSkin(spSkeletonBinary *self, _dataInput *input, int /*bool*/ defaultSkin,
								  spSkeletonData *skeletonData, int /*bool*/ nonessential) {
	spSkin *skin;
	int i, n, ii, nn, slotCount;

	if (defaultSkin) {
		slotCount = readVarint(input, 1);
		if (slotCount == 0) return NULL;
		skin = spSkin_create("default");
	} else {
		char *name = readString(input);
		skin = spSkin_create(name);
		FREE(name);
		if (nonessential) readColor(input, &skin->color.r, &skin->color.g, &skin->color.b, &skin->color.a);
		for (i = 0, n = readVarint(input, 1); i < n; i++)
			spBoneDataArray_add(skin->bones, skeletonData->bones[readVarint(input, 1)]);

		for (i = 0, n = readVarint(input, 1); i < n; i++)
			spIkConstraintDataArray_add(skin->ikConstraints, skeletonData->ikConstraints[readVarint(input, 1)]);

		for (i = 0, n = readVarint(input, 1); i < n; i++)
			spTransformConstraintDataArray_add(skin->transformConstraints,
											   skeletonData->transformConstraints[readVarint(input, 1)]);

		for (i = 0, n = readVarint(input, 1); i < n; i++)
			spPathConstraintDataArray_add(skin->pathConstraints, skeletonData->pathConstraints[readVarint(input, 1)]);

		for (i = 0, n = readVarint(input, 1); i < n; i++)
			spPhysicsConstraintDataArray_add(skin->physicsConstraints, skeletonData->physicsConstraints[readVarint(input, 1)]);

		slotCount = readVarint(input, 1);
	}

	for (i = 0; i < slotCount; ++i) {
		int slotIndex = readVarint(input, 1);
		for (ii = 0, nn = readVarint(input, 1); ii < nn; ++ii) {
			const char *name = readStringRef(input, skeletonData);
			spAttachment *attachment = spSkeletonBinary_readAttachment(self, input, skin, slotIndex, name, skeletonData,
																	   nonessential);
			if (!attachment)
				return NULL;
			spSkin_setAttachment(skin, slotIndex, name, attachment);
		}
	}
	return skin;
}

spSkeletonData *spSkeletonBinary_readSkeletonDataFile(spSkeletonBinary *self, const char *path) {
	int length;
	spSkeletonData *skeletonData;
	const char *binary = _spUtil_readFile(path, &length);
	if (length == 0 || !binary) {
		_spSkeletonBinary_setError(self, "Unable to read skeleton file: ", path);
		return NULL;
	}
	skeletonData = spSkeletonBinary_readSkeletonData(self, (unsigned char *) binary, length);
	FREE(binary);
	return skeletonData;
}

spSkeletonData *spSkeletonBinary_readSkeletonData(spSkeletonBinary *self, const unsigned char *binary,
												  const int length) {
	int i, n, ii, nonessential;
	char buffer[32];
	int lowHash, highHash;
	spSkeletonData *skeletonData;
	_spSkeletonBinary *internal = SUB_CAST(_spSkeletonBinary, self);

	_dataInput *input = NEW(_dataInput);
	input->cursor = binary;
	input->end = binary + length;

	FREE(self->error);
	self->error = 0;
	internal->linkedMeshCount = 0;

	skeletonData = spSkeletonData_create();
	lowHash = readInt(input);
	highHash = readInt(input);
	snprintf(buffer, 32, "%x%x", highHash, lowHash);
	buffer[31] = 0;
	MALLOC_STR(skeletonData->hash, buffer);

	skeletonData->version = readString(input);
	if (!strlen(skeletonData->version)) {
		FREE(skeletonData->version);
		skeletonData->version = 0;
	} else {
		if (!string_starts_with(skeletonData->version, SPINE_VERSION_STRING)) {
			FREE(input);
			spSkeletonData_dispose(skeletonData);
			char errorMsg[255];
			snprintf(errorMsg, 255, "Skeleton version %s does not match runtime version %s", skeletonData->version, SPINE_VERSION_STRING);
			_spSkeletonBinary_setError(self, errorMsg, NULL);
			return NULL;
		}
	}

	skeletonData->x = readFloat(input);
	skeletonData->y = readFloat(input);
	skeletonData->width = readFloat(input);
	skeletonData->height = readFloat(input);
	skeletonData->referenceScale = readFloat(input);

	nonessential = readBoolean(input);

	if (nonessential) {
		skeletonData->fps = readFloat(input);
		skeletonData->imagesPath = readString(input);
		if (!strlen(skeletonData->imagesPath)) {
			FREE(skeletonData->imagesPath);
			skeletonData->imagesPath = 0;
		}
		skeletonData->audioPath = readString(input);
		if (!strlen(skeletonData->audioPath)) {
			FREE(skeletonData->audioPath);
			skeletonData->audioPath = 0;
		}
	}

	skeletonData->stringsCount = n = readVarint(input, 1);
	skeletonData->strings = MALLOC(char *, skeletonData->stringsCount);
	for (i = 0; i < n; i++) {
		skeletonData->strings[i] = readString(input);
	}

	/* Bones. */
	skeletonData->bonesCount = readVarint(input, 1);
	skeletonData->bones = MALLOC(spBoneData *, skeletonData->bonesCount);
	for (i = 0; i < skeletonData->bonesCount; ++i) {
		const char *name = readString(input);
		spBoneData *parent = i == 0 ? 0 : skeletonData->bones[readVarint(input, 1)];
		spBoneData *data = spBoneData_create(i, name, parent);
		FREE(name);
		data->rotation = readFloat(input);
		data->x = readFloat(input) * self->scale;
		data->y = readFloat(input) * self->scale;
		data->scaleX = readFloat(input);
		data->scaleY = readFloat(input);
		data->shearX = readFloat(input);
		data->shearY = readFloat(input);
		data->length = readFloat(input) * self->scale;
		data->inherit = (spInherit) readVarint(input, 1);
		data->skinRequired = readBoolean(input);
		if (nonessential) {
			readColor(input, &data->color.r, &data->color.g, &data->color.b, &data->color.a);
			data->icon = readString(input);
			data->visible = readBoolean(input);
		}
		skeletonData->bones[i] = data;
	}

	/* Slots. */
	skeletonData->slotsCount = readVarint(input, 1);
	skeletonData->slots = MALLOC(spSlotData *, skeletonData->slotsCount);
	for (i = 0; i < skeletonData->slotsCount; ++i) {
		char *slotName = readString(input);
		spBoneData *boneData = skeletonData->bones[readVarint(input, 1)];
		spSlotData *slotData = spSlotData_create(i, slotName, boneData);
		FREE(slotName);
		readColor(input, &slotData->color.r, &slotData->color.g, &slotData->color.b, &slotData->color.a);
		int a = readByte(input);
		int r = readByte(input);
		int g = readByte(input);
		int b = readByte(input);
		if (!(r == 0xff && g == 0xff && b == 0xff && a == 0xff)) {
			slotData->darkColor = spColor_create();
			spColor_setFromFloats(slotData->darkColor, r / 255.0f, g / 255.0f, b / 255.0f, 1);
		}
		char *attachmentName = readStringRef(input, skeletonData);
		if (attachmentName) MALLOC_STR(slotData->attachmentName, attachmentName);
		else
			slotData->attachmentName = 0;
		slotData->blendMode = (spBlendMode) readVarint(input, 1);
		if (nonessential) {
			slotData->visible = readBoolean(input);
		}
		skeletonData->slots[i] = slotData;
	}

	/* IK constraints. */
	skeletonData->ikConstraintsCount = readVarint(input, 1);
	skeletonData->ikConstraints = MALLOC(spIkConstraintData *, skeletonData->ikConstraintsCount);
	for (i = 0; i < skeletonData->ikConstraintsCount; ++i) {
		const char *name = readString(input);
		spIkConstraintData *data = spIkConstraintData_create(name);
		FREE(name);
		data->order = readVarint(input, 1);
		data->bonesCount = readVarint(input, 1);
		data->bones = MALLOC(spBoneData *, data->bonesCount);
		for (ii = 0; ii < data->bonesCount; ++ii)
			data->bones[ii] = skeletonData->bones[readVarint(input, 1)];
		data->target = skeletonData->bones[readVarint(input, 1)];
		int flags = readByte(input);
		data->skinRequired = (flags & 1) != 0;
		data->bendDirection = (flags & 2) != 0 ? 1 : -1;
		data->compress = (flags & 4) != 0;
		data->stretch = (flags & 8) != 0;
		data->uniform = (flags & 16) != 0;
		if ((flags & 32) != 0) data->mix = (flags & 64) != 0 ? readFloat(input) : 1;
		if ((flags & 128) != 0) data->softness = readFloat(input) * self->scale;

		skeletonData->ikConstraints[i] = data;
	}

	/* Transform constraints. */
	skeletonData->transformConstraintsCount = readVarint(input, 1);
	skeletonData->transformConstraints = MALLOC(
			spTransformConstraintData *, skeletonData->transformConstraintsCount);
	for (i = 0; i < skeletonData->transformConstraintsCount; ++i) {
		const char *name = readString(input);
		spTransformConstraintData *data = spTransformConstraintData_create(name);
		FREE(name);
		data->order = readVarint(input, 1);
		data->bonesCount = readVarint(input, 1);
		data->bones = MALLOC(spBoneData *, data->bonesCount);
		for (ii = 0; ii < data->bonesCount; ++ii)
			data->bones[ii] = skeletonData->bones[readVarint(input, 1)];
		data->target = skeletonData->bones[readVarint(input, 1)];
		int flags = readByte(input);
		data->skinRequired = (flags & 1) != 0;
		data->local = (flags & 2) != 0;
		data->relative = (flags & 4) != 0;
		if ((flags & 8) != 0) data->offsetRotation = readFloat(input);
		if ((flags & 16) != 0) data->offsetX = readFloat(input) * self->scale;
		if ((flags & 32) != 0) data->offsetY = readFloat(input) * self->scale;
		if ((flags & 64) != 0) data->offsetScaleX = readFloat(input);
		if ((flags & 128) != 0) data->offsetScaleY = readFloat(input);
		flags = readByte(input);
		if ((flags & 1) != 0) data->offsetShearY = readFloat(input);
		if ((flags & 2) != 0) data->mixRotate = readFloat(input);
		if ((flags & 4) != 0) data->mixX = readFloat(input);
		if ((flags & 8) != 0) data->mixY = readFloat(input);
		if ((flags & 16) != 0) data->mixScaleX = readFloat(input);
		if ((flags & 32) != 0) data->mixScaleY = readFloat(input);
		if ((flags & 64) != 0) data->mixShearY = readFloat(input);

		skeletonData->transformConstraints[i] = data;
	}

	/* Path constraints */
	skeletonData->pathConstraintsCount = readVarint(input, 1);
	skeletonData->pathConstraints = MALLOC(spPathConstraintData *, skeletonData->pathConstraintsCount);
	for (i = 0; i < skeletonData->pathConstraintsCount; ++i) {
		const char *name = readString(input);
		spPathConstraintData *data = spPathConstraintData_create(name);
		FREE(name);
		data->order = readVarint(input, 1);
		data->skinRequired = readBoolean(input);
		data->bonesCount = readVarint(input, 1);
		data->bones = MALLOC(spBoneData *, data->bonesCount);
		for (ii = 0; ii < data->bonesCount; ++ii)
			data->bones[ii] = skeletonData->bones[readVarint(input, 1)];
		data->target = skeletonData->slots[readVarint(input, 1)];
		int flags = readByte(input);
		data->positionMode = (spPositionMode) (flags & 1);
		data->spacingMode = (spSpacingMode) ((flags >> 1) & 3);
		data->rotateMode = (spRotateMode) ((flags >> 3) & 3);
		if ((flags & 128) != 0) data->offsetRotation = readFloat(input);
		data->position = readFloat(input);
		if (data->positionMode == SP_POSITION_MODE_FIXED) data->position *= self->scale;
		data->spacing = readFloat(input);
		if (data->spacingMode == SP_SPACING_MODE_LENGTH || data->spacingMode == SP_SPACING_MODE_FIXED)
			data->spacing *= self->scale;
		data->mixRotate = readFloat(input);
		data->mixX = readFloat(input);
		data->mixY = readFloat(input);
		skeletonData->pathConstraints[i] = data;
	}

	// Physics constraints.
	skeletonData->physicsConstraintsCount = readVarint(input, 1);
	skeletonData->physicsConstraints = MALLOC(spPhysicsConstraintData *, skeletonData->physicsConstraintsCount);
	for (i = 0; i < skeletonData->physicsConstraintsCount; i++) {
		const char *name = readString(input);
		spPhysicsConstraintData *data = spPhysicsConstraintData_create(name);
		FREE(name);
		data->order = readVarint(input, 1);
		data->bone = skeletonData->bones[readVarint(input, 1)];
		int flags = readByte(input);
		data->skinRequired = (flags & 1) != 0;
		if ((flags & 2) != 0) data->x = readFloat(input);
		if ((flags & 4) != 0) data->y = readFloat(input);
		if ((flags & 8) != 0) data->rotate = readFloat(input);
		if ((flags & 16) != 0) data->scaleX = readFloat(input);
		if ((flags & 32) != 0) data->shearX = readFloat(input);
		data->limit = ((flags & 64) != 0 ? readFloat(input) : 5000) * self->scale;
		data->step = 1.f / readByte(input);
		data->inertia = readFloat(input);
		data->strength = readFloat(input);
		data->damping = readFloat(input);
		data->massInverse = (flags & 128) != 0 ? readFloat(input) : 1;
		data->wind = readFloat(input);
		data->gravity = readFloat(input);
		flags = readByte(input);
		if ((flags & 1) != 0) data->inertiaGlobal = -1;
		if ((flags & 2) != 0) data->strengthGlobal = -1;
		if ((flags & 4) != 0) data->dampingGlobal = -1;
		if ((flags & 8) != 0) data->massGlobal = -1;
		if ((flags & 16) != 0) data->windGlobal = -1;
		if ((flags & 32) != 0) data->gravityGlobal = -1;
		if ((flags & 64) != 0) data->mixGlobal = -1;
		data->mix = (flags & 128) != 0 ? readFloat(input) : 1;
		skeletonData->physicsConstraints[i] = data;
	}

	/* Default skin. */
	skeletonData->defaultSkin = spSkeletonBinary_readSkin(self, input, -1, skeletonData, nonessential);
	if (self->attachmentLoader->error1) {
		FREE(input);
		spSkin_dispose(skeletonData->defaultSkin);
		spSkeletonData_dispose(skeletonData);
		_spSkeletonBinary_setError(self, self->attachmentLoader->error1, self->attachmentLoader->error2);
		return NULL;
	}
	skeletonData->skinsCount = readVarint(input, 1);

	if (skeletonData->defaultSkin)
		++skeletonData->skinsCount;

	skeletonData->skins = MALLOC(spSkin *, skeletonData->skinsCount);

	if (skeletonData->defaultSkin)
		skeletonData->skins[0] = skeletonData->defaultSkin;

	/* Skins. */
	for (i = skeletonData->defaultSkin ? 1 : 0; i < skeletonData->skinsCount; ++i) {
		spSkin *skin = spSkeletonBinary_readSkin(self, input, 0, skeletonData, nonessential);
		if (self->attachmentLoader->error1) {
			FREE(input);
			skeletonData->skinsCount = i + 1;
			spSkeletonData_dispose(skeletonData);
			_spSkeletonBinary_setError(self, self->attachmentLoader->error1, self->attachmentLoader->error2);
			return NULL;
		}
		skeletonData->skins[i] = skin;
	}

	/* Linked meshes. */
	for (i = 0; i < internal->linkedMeshCount; ++i) {
		_spLinkedMesh *linkedMesh = internal->linkedMeshes + i;
		spSkin *skin = skeletonData->skins[linkedMesh->skinIndex];
		if (!skin) {
			FREE(input);
			spSkeletonData_dispose(skeletonData);
			_spSkeletonBinary_setError(self, "Skin not found", "");
			return NULL;
		}
		spAttachment *parent = spSkin_getAttachment(skin, linkedMesh->slotIndex, linkedMesh->parent);
		if (!parent) {
			FREE(input);
			spSkeletonData_dispose(skeletonData);
			_spSkeletonBinary_setError(self, "Parent mesh not found: ", linkedMesh->parent);
			return NULL;
		}
		linkedMesh->mesh->super.timelineAttachment = linkedMesh->inheritTimeline ? parent
																				 : SUPER(SUPER(linkedMesh->mesh));
		spMeshAttachment_setParentMesh(linkedMesh->mesh, SUB_CAST(spMeshAttachment, parent));
		if (linkedMesh->mesh->region) spMeshAttachment_updateRegion(linkedMesh->mesh);
		spAttachmentLoader_configureAttachment(self->attachmentLoader, SUPER(SUPER(linkedMesh->mesh)));
	}

	/* Events. */
	skeletonData->eventsCount = readVarint(input, 1);
	skeletonData->events = MALLOC(spEventData *, skeletonData->eventsCount);
	for (i = 0; i < skeletonData->eventsCount; ++i) {
		const char *name = readString(input);
		spEventData *eventData = spEventData_create(name);
		FREE(name);
		eventData->intValue = readVarint(input, 0);
		eventData->floatValue = readFloat(input);
		eventData->stringValue = readString(input);
		eventData->audioPath = readString(input);
		if (eventData->audioPath) {
			eventData->volume = readFloat(input);
			eventData->balance = readFloat(input);
		}
		skeletonData->events[i] = eventData;
	}

	/* Animations. */
	skeletonData->animationsCount = readVarint(input, 1);
	skeletonData->animations = MALLOC(spAnimation *, skeletonData->animationsCount);
	for (i = 0; i < skeletonData->animationsCount; ++i) {
		const char *name = readString(input);
		spAnimation *animation = _spSkeletonBinary_readAnimation(self, name, input, skeletonData);
		FREE(name);
		if (!animation) {
			FREE(input);
			skeletonData->animationsCount = i + 1;
			spSkeletonData_dispose(skeletonData);
			_spSkeletonBinary_setError(self, "Animation corrupted: ", name);
			return NULL;
		}
		skeletonData->animations[i] = animation;
	}

	FREE(input);
	return skeletonData;
}
