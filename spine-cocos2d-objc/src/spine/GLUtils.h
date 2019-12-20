/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#ifndef GLUtils_h
#define GLUtils_h

#include <stdint.h>

typedef struct spVertex {
	float x, y, z, w;
	uint32_t color;
	uint32_t color2;
	float u, v;
} spVertex;

typedef struct spMesh {
	spVertex* vertices;
	uint32_t numVertices;
	uint32_t numAllocatedVertices;
	unsigned short* indices;
	uint32_t numIndices;
	uint32_t numAllocatedIndices;
} spMesh;

typedef struct spMeshPart {
	spMesh* mesh;
	uint32_t startVertex;
	uint32_t numVertices;
	uint32_t startIndex;
	uint32_t numIndices;
	uint32_t textureHandle;
	uint32_t srcBlend;
	uint32_t dstBlend;
} spMeshPart;

spMesh* spMesh_create(uint32_t numVertices, uint32_t numIndices);
void spMesh_allocatePart(spMesh* mesh, spMeshPart* part, uint32_t numVertices, uint32_t numIndices, uint32_t textureHandle, uint32_t srcBlend, uint32_t dstBlend);
void spMesh_clearParts(spMesh* mesh);
void spMesh_dispose(spMesh* mesh);

typedef struct spShader {
	uint32_t program;
	uint32_t vertexShader;
	uint32_t fragmentShader;
} spShader;

spShader* spShader_create(const char* vertexShaderSource, const char* fragmentShaderSource);
void spShader_dispose(spShader* shader);

typedef struct spTwoColorBatcher {
	spShader* shader;

	uint32_t vertexBufferHandle;
	spVertex* verticesBuffer;
	uint32_t numVertices;

	uint32_t indexBufferHandle;
	unsigned short* indicesBuffer;
	uint32_t numIndices;

	int32_t positionAttributeLocation;
	int32_t colorAttributeLocation;
	int32_t color2AttributeLocation;
	int32_t texCoordsAttributeLocation;
	int32_t textureUniformLocation;

	uint32_t lastTextureHandle;
	uint32_t lastSrcBlend;
	uint32_t lastDstBlend;
} spTwoColorBatcher;

spTwoColorBatcher* spTwoColorBatcher_create();
void spTwoColorBatcher_add(spTwoColorBatcher* batcher, spMeshPart meshPart);
void spTwoColorBatcher_flush(spTwoColorBatcher* batcher);
void spDisposeTwoColorBatcher(spTwoColorBatcher* batcher);

#endif /* GLUtils_h */
