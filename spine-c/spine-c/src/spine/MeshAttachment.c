/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/MeshAttachment.h>
#include <spine/extension.h>

void _spMeshAttachment_dispose (spAttachment* attachment) {
	spMeshAttachment* self = SUB_CAST(spMeshAttachment, attachment);
	FREE(self->path);
	FREE(self->uvs);
	if (!self->parentMesh) {
		_spVertexAttachment_deinit(SUPER(self));
		FREE(self->regionUVs);
		FREE(self->triangles);
		FREE(self->edges);
	} else
		_spAttachment_deinit(attachment);
	FREE(self);
}

spMeshAttachment* spMeshAttachment_create (const char* name) {
	spMeshAttachment* self = NEW(spMeshAttachment);
	_spVertexAttachment_init(SUPER(self));
	spColor_setFromFloats(&self->color, 1, 1, 1, 1);
	_spAttachment_init(SUPER(SUPER(self)), name, SP_ATTACHMENT_MESH, _spMeshAttachment_dispose);
	return self;
}

void spMeshAttachment_updateUVs (spMeshAttachment* self) {
	int i, n;
	int verticesLength = SUPER(self)->worldVerticesLength;
	FREE(self->uvs);
	float* uvs = self->uvs = MALLOC(float, verticesLength);
	n = verticesLength;
	float u = self->regionU, v = self->regionV;
	float width = 0, height = 0;

	switch (self->regionDegrees) {
	case 90: {
		float textureWidth = self->regionHeight / (self->regionU2 - self->regionU);
		float textureHeight = self->regionWidth / (self->regionV2 - self->regionV);
		u -= (self->regionOriginalHeight - self->regionOffsetY - self->regionHeight) / textureWidth;
		v -= (self->regionOriginalWidth - self->regionOffsetX - self->regionWidth) / textureHeight;
		width = self->regionOriginalHeight / textureWidth;
		height = self->regionOriginalWidth / textureHeight;
		for (i = 0; i < n; i += 2) {
			uvs[i] = u + self->regionUVs[i + 1] * width;
			uvs[i + 1] = v + (1 - self->regionUVs[i]) * height;
		}
		return;
	}
	case 180: {
		float textureWidth = self->regionWidth / (self->regionU2 - self->regionU);
		float textureHeight = self->regionHeight / (self->regionV2 - self->regionV);
		u -= (self->regionOriginalWidth - self->regionOffsetX - self->regionWidth) / textureWidth;
		v -= self->regionOffsetY / textureHeight;
		width = self->regionOriginalWidth / textureWidth;
		height = self->regionOriginalHeight / textureHeight;
		for (i = 0; i < n; i += 2) {
			uvs[i] = u + (1 - self->regionUVs[i]) * width;
			uvs[i + 1] = v + (1 - self->regionUVs[i + 1]) * height;
		}
		return;
	}
	case 270: {
		float textureHeight = self->regionHeight / (self->regionV2 - self->regionV);
		float textureWidth = self->regionWidth / (self->regionU2 - self->regionU);
		u -= self->regionOffsetY / textureWidth;
		v -= self->regionOffsetX / textureHeight;
		width = self->regionOriginalHeight / textureWidth;
		height = self->regionOriginalWidth / textureHeight;
		for (i = 0; i < n; i += 2) {
			uvs[i] = u + (1 - self->regionUVs[i + 1]) * width;
			uvs[i + 1] = v + self->regionUVs[i] * height;
		}
		return;
	}
	default: {
		float textureWidth = self->regionWidth / (self->regionU2 - self->regionU);
		float textureHeight = self->regionHeight / (self->regionV2 - self->regionV);
		u -= self->regionOffsetX / textureWidth;
		v -= (self->regionOriginalHeight - self->regionOffsetY - self->regionHeight) / textureHeight;
		width = self->regionOriginalWidth / textureWidth;
		height = self->regionOriginalHeight / textureHeight;
		for (i = 0; i < n; i += 2) {
			uvs[i] = u + self->regionUVs[i] * width;
			uvs[i + 1] = v + self->regionUVs[i + 1] * height;
		}
	}
	}
}

void spMeshAttachment_setParentMesh (spMeshAttachment* self, spMeshAttachment* parentMesh) {
	CONST_CAST(spMeshAttachment*, self->parentMesh) = parentMesh;
	if (parentMesh) {

		self->super.bones = parentMesh->super.bones;
		self->super.bonesCount = parentMesh->super.bonesCount;

		self->super.vertices = parentMesh->super.vertices;
		self->super.verticesCount = parentMesh->super.verticesCount;

		self->regionUVs = parentMesh->regionUVs;

		self->triangles = parentMesh->triangles;
		self->trianglesCount = parentMesh->trianglesCount;

		self->hullLength = parentMesh->hullLength;
		
		self->super.worldVerticesLength = parentMesh->super.worldVerticesLength;

		self->edges = parentMesh->edges;
		self->edgesCount = parentMesh->edgesCount;

		self->width = parentMesh->width;
		self->height = parentMesh->height;
	}
}
