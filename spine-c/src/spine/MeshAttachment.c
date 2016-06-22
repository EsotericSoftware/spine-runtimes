/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/MeshAttachment.h>
#include <spine/extension.h>

void _spMeshAttachment_dispose (spAttachment* attachment) {
	spMeshAttachment* self = SUB_CAST(spMeshAttachment, attachment);
	_spVertexAttachment_deinit(SUPER(self));
	FREE(self->path);
	FREE(self->uvs);
	if (!self->parentMesh) {
		FREE(self->regionUVs);
		FREE(self->triangles);
		FREE(self->edges);
	}
	FREE(self);
}

spMeshAttachment* spMeshAttachment_create (const char* name) {
	spMeshAttachment* self = NEW(spMeshAttachment);
	self->r = 1;
	self->g = 1;
	self->b = 1;
	self->a = 1;
	_spAttachment_init(SUPER(self), name, SP_ATTACHMENT_MESH, _spMeshAttachment_dispose);
	return self;
}

void spMeshAttachment_updateUVs (spMeshAttachment* self) {
	int i;
	float width = self->regionU2 - self->regionU, height = self->regionV2 - self->regionV;
	FREE(self->uvs);
	self->uvs = MALLOC(float, self->super.verticesCount);
	if (self->regionRotate) {
		for (i = 0; i < self->super.verticesCount; i += 2) {
			self->uvs[i] = self->regionU + self->regionUVs[i + 1] * width;
			self->uvs[i + 1] = self->regionV + height - self->regionUVs[i] * height;
		}
	} else {
		for (i = 0; i < self->super.verticesCount; i += 2) {
			self->uvs[i] = self->regionU + self->regionUVs[i] * width;
			self->uvs[i + 1] = self->regionV + self->regionUVs[i + 1] * height;
		}
	}
}

void spMeshAttachment_computeWorldVertices (spMeshAttachment* self, spSlot* slot, float* worldVertices) {
	int i;
	spSkeleton* skeleton = slot->bone->skeleton;
	float x = skeleton->x, y = skeleton->y;
	float* deform = slot->attachmentVertices;
	spVertexAttachment* vertexAttachment = SUPER(self);
	float* vertices = vertexAttachment->vertices;
	int* bones = vertexAttachment->bones;
	if (!bones) {
		int verticesLength = vertexAttachment->verticesCount;
		spBone* bone;
		int v, w;
		if (slot->attachmentVerticesCount > 0) vertices = deform;
		bone = slot->bone;
		x += bone->worldX;
		y += bone->worldY;
		for (v = 0, w = 0; v < verticesLength; v += 2, w += 2) {
			float vx = vertices[v], vy = vertices[v + 1];
			worldVertices[w] = vx * bone->a + vy * bone->b + x;
			worldVertices[w + 1] = vx * bone->c + vy * bone->d + y;
		}
	} else {
		spBone** skeletonBones = skeleton->bones;
		if (slot->attachmentVerticesCount == 0) {
			int w, v, b, n;
			for (w = 0, v = 0, b = 0, n = skeleton->bonesCount; v < n; w += 2) {
				float wx = x, wy = y;
				int nn = bones[v++] + v;
				for (; v < nn; v++, b += 3) {
					spBone* bone = skeletonBones[bones[v]];
					float vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
					wx += (vx * bone->a + vy * bone->b + bone->worldX) * weight;
					wy += (vx * bone->c + vy * bone->d + bone->worldY) * weight;
				}
				worldVertices[w] = wx;
				worldVertices[w + 1] = wy;
			}
		} else {
			int w, v, b, f, n;
			for (w = 0, v = 0, b = 0, f = 0, n = skeleton->bonesCount; v < n; w += 2) {
				float wx = x, wy = y;
				int nn = bones[v++] + v;
				for (; v < nn; v++, b += 3, f += 2) {
					spBone* bone = skeletonBones[bones[v]];
					float vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
					wx += (vx * bone->a + vy * bone->b + bone->worldX) * weight;
					wy += (vx * bone->c + vy * bone->d + bone->worldY) * weight;
				}
				worldVertices[w] = wx;
				worldVertices[w + 1] = wy;
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

		self->edges = parentMesh->edges;
		self->edgesCount = parentMesh->edgesCount;

		self->width = parentMesh->width;
		self->height = parentMesh->height;
	}
}
