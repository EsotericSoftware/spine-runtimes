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

#include <spine/SkeletonBounds.h>
#include <limits.h>
#include <spine/extension.h>

BoundingPolygon* BoundingPolygon_create (int capacity) {
	BoundingPolygon* self = NEW(BoundingPolygon);
	self->capacity = capacity;
	CONST_CAST(float*, self->vertices) = MALLOC(float, capacity);
	return self;
}

void BoundingPolygon_dispose (BoundingPolygon* self) {
	FREE(self->vertices);
	FREE(self);
}

int/*bool*/BoundingPolygon_containsPoint (BoundingPolygon* self, float x, float y) {
	int prevIndex = self->count - 2;
	int inside = 0;
	int i;
	for (i = 0; i < self->count; i += 2) {
		float vertexY = self->vertices[i + 1];
		float prevY = self->vertices[prevIndex + 1];
		if ((vertexY < y && prevY >= y) || (prevY < y && vertexY >= y)) {
			float vertexX = self->vertices[i];
			if (vertexX + (y - vertexY) / (prevY - vertexY) * (self->vertices[prevIndex] - vertexX) < x) inside = !inside;
		}
		prevIndex = i;
	}
	return inside;
}

int/*bool*/BoundingPolygon_intersectsSegment (BoundingPolygon* self, float x1, float y1, float x2, float y2) {
	float width12 = x1 - x2, height12 = y1 - y2;
	float det1 = x1 * y2 - y1 * x2;
	float x3 = self->vertices[self->count - 2], y3 = self->vertices[self->count - 1];
	int i;
	for (i = 0; i < self->count; i += 2) {
		float x4 = self->vertices[i], y4 = self->vertices[i + 1];
		float det2 = x3 * y4 - y3 * x4;
		float width34 = x3 - x4, height34 = y3 - y4;
		float det3 = width12 * height34 - height12 * width34;
		float x = (det1 * width34 - width12 * det2) / det3;
		if (((x >= x3 && x <= x4) || (x >= x4 && x <= x3)) && ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))) {
			float y = (det1 * height34 - height12 * det2) / det3;
			if (((y >= y3 && y <= y4) || (y >= y4 && y <= y3)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))) return 1;
		}
		x3 = x4;
		y3 = y4;
	}
	return 0;
}

/**/

typedef struct {
	SkeletonBounds super;
	int capacity;
} _SkeletonBounds;

SkeletonBounds* SkeletonBounds_create () {
	return SUPER(NEW(_SkeletonBounds));
}

void SkeletonBounds_dispose (SkeletonBounds* self) {
	int i;
	for (i = 0; i < SUB_CAST(_SkeletonBounds, self)->capacity; ++i)
		if (self->polygons[i]) BoundingPolygon_dispose(self->polygons[i]);
	FREE(self->polygons);
	FREE(self->boundingBoxes);
	FREE(self);
}

void SkeletonBounds_update (SkeletonBounds* self, Skeleton* skeleton, int/*bool*/updateAabb) {
	int i;

	_SkeletonBounds* internal = SUB_CAST(_SkeletonBounds, self);
	if (internal->capacity < skeleton->slotCount) {
		BoundingPolygon** newPolygons;

		FREE(self->boundingBoxes);
		self->boundingBoxes = MALLOC(BoundingBoxAttachment*, skeleton->slotCount);

		newPolygons = CALLOC(BoundingPolygon*, skeleton->slotCount);
		memcpy(newPolygons, self->polygons, internal->capacity);
		FREE(self->polygons);
		self->polygons = newPolygons;

		internal->capacity = skeleton->slotCount;
	}

	self->minX = (float)INT_MAX;
	self->minY = (float)INT_MIN;
	self->maxX = (float)INT_MAX;
	self->maxY = (float)INT_MIN;

	self->count = 0;
	for (i = 0; i < skeleton->slotCount; ++i) {
		BoundingPolygon* polygon;
		BoundingBoxAttachment* boundingBox;

		Slot* slot = skeleton->slots[i];
		Attachment* attachment = slot->attachment;
		if (!attachment || attachment->type != ATTACHMENT_BOUNDING_BOX) continue;
		boundingBox = (BoundingBoxAttachment*)attachment;
		self->boundingBoxes[self->count] = boundingBox;

		polygon = self->polygons[self->count];
		if (!polygon || polygon->capacity < boundingBox->verticesCount) {
			if (polygon) BoundingPolygon_dispose(polygon);
			self->polygons[self->count] = polygon = BoundingPolygon_create(boundingBox->verticesCount);
		}
		polygon->count = boundingBox->verticesCount;
		BoundingBoxAttachment_computeWorldVertices(boundingBox, skeleton->x, skeleton->y, slot->bone, polygon->vertices);

		if (updateAabb) {
			int ii = 0;
			for (; ii < polygon->count; ii += 2) {
				float x = polygon->vertices[ii];
				float y = polygon->vertices[ii + 1];
				if (x < self->minX) self->minX = x;
				if (y < self->minY) self->minY = y;
				if (x > self->maxX) self->maxX = x;
				if (y > self->maxY) self->maxY = y;
			}
		}

		++self->count;
	}
}

int/*bool*/SkeletonBounds_aabbContainsPoint (SkeletonBounds* self, float x, float y) {
	return x >= self->minX && x <= self->maxX && y >= self->minY && y <= self->maxY;
}

int/*bool*/SkeletonBounds_aabbIntersectsSegment (SkeletonBounds* self, float x1, float y1, float x2, float y2) {
	float m, x, y;
	if ((x1 <= self->minX && x2 <= self->minX) || (y1 <= self->minY && y2 <= self->minY) || (x1 >= self->maxX && x2 >= self->maxX)
			|| (y1 >= self->maxY && y2 >= self->maxY)) return 0;
	m = (y2 - y1) / (x2 - x1);
	y = m * (self->minX - x1) + y1;
	if (y > self->minY && y < self->maxY) return 1;
	y = m * (self->maxX - x1) + y1;
	if (y > self->minY && y < self->maxY) return 1;
	x = (self->minY - y1) / m + x1;
	if (x > self->minX && x < self->maxX) return 1;
	x = (self->maxY - y1) / m + x1;
	if (x > self->minX && x < self->maxX) return 1;
	return 0;
}

int/*bool*/SkeletonBounds_aabbIntersectsSkeleton (SkeletonBounds* self, SkeletonBounds* bounds) {
	return self->minX < bounds->maxX && self->maxX > bounds->minX && self->minY < bounds->maxY && self->maxY > bounds->minY;
}

BoundingBoxAttachment* SkeletonBounds_containsPoint (SkeletonBounds* self, float x, float y) {
	int i;
	for (i = 0; i < self->count; ++i)
		if (BoundingPolygon_containsPoint(self->polygons[i], x, y)) return self->boundingBoxes[i];
	return 0;
}

BoundingBoxAttachment* SkeletonBounds_intersectsSegment (SkeletonBounds* self, float x1, float y1, float x2, float y2) {
	int i;
	for (i = 0; i < self->count; ++i)
		if (BoundingPolygon_intersectsSegment(self->polygons[i], x1, y1, x2, y2)) return self->boundingBoxes[i];
	return 0;
}

BoundingPolygon* SkeletonBounds_getPolygon (SkeletonBounds* self, BoundingBoxAttachment* boundingBox) {
	int i;
	for (i = 0; i < self->count; ++i)
		if (self->boundingBoxes[i] == boundingBox) return self->polygons[i];
	return 0;
}
