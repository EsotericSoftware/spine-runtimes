/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
	float width, height;
	int verticesLength = SUPER(self)->worldVerticesLength;
	FREE(self->uvs);
	self->uvs = MALLOC(float, verticesLength);

	if (self->regionRotate) {
		float textureWidth = self->regionHeight / (self->regionU2 - self->regionU);
		float textureHeight = self->regionWidth / (self->regionV2 - self->regionV);
		float u = self->regionU - (self->regionOriginalHeight - self->regionOffsetY - self->regionHeight) / textureWidth;
		float v = self->regionV - (self->regionOriginalWidth - self->regionOffsetX - self->regionWidth) / textureHeight;
		width = self->regionOriginalHeight / textureWidth;
		height = self->regionOriginalWidth / textureHeight;
		for (i = 0, n = verticesLength; i < n; i += 2) {
			self->uvs[i] = u + self->regionUVs[i + 1] * width;
			self->uvs[i + 1] = v + height - self->regionUVs[i] * height;
		}
		return;
	} else {
		float textureWidth = self->regionWidth / (self->regionU2 - self->regionU);
		float textureHeight = self->regionHeight / (self->regionV2 - self->regionV);
		float u = self->regionU - self->regionOffsetX / textureWidth;
		float v = self->regionV - (self->regionOriginalHeight - self->regionOffsetY - self->regionHeight) / textureHeight;
		width = self->regionOriginalWidth / textureWidth;
		height = self->regionOriginalHeight / textureHeight;
		for (i = 0, n = verticesLength; i < n; i += 2) {
			self->uvs[i] = u + self->regionUVs[i] * width;
			self->uvs[i + 1] = v + self->regionUVs[i + 1] * height;
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
