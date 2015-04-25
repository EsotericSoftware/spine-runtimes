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

#include <spine/PolygonBatch.h>
#include <spine/extension.h>

USING_NS_CC;

namespace spine {

PolygonBatch* PolygonBatch::createWithCapacity (int capacity) {
	PolygonBatch* batch = new PolygonBatch();
	batch->initWithCapacity(capacity);
	batch->autorelease();
	return batch;
}

PolygonBatch::PolygonBatch () :
	capacity(0), 
	vertices(nullptr), verticesCount(0),
	triangles(nullptr), trianglesCount(0),
	texture(nullptr)
{}

bool PolygonBatch::initWithCapacity (int capacity) {
	// 32767 is max index, so 32767 / 3 - (32767 / 3 % 3) = 10920.
	CCAssert(capacity <= 10920, "capacity cannot be > 10920");
	CCAssert(capacity >= 0, "capacity cannot be < 0");
	this->capacity = capacity;
	vertices = MALLOC(ccV2F_C4B_T2F, capacity);
	triangles = MALLOC(GLushort, capacity * 3);
	return true;
}

PolygonBatch::~PolygonBatch () {
	FREE(vertices);
	FREE(triangles);
}

void PolygonBatch::add (CCTexture2D* addTexture,
		const float* addVertices, const float* uvs, int addVerticesCount,
		const int* addTriangles, int addTrianglesCount,
		ccColor4B* color) {

	if (
		addTexture != texture
		|| verticesCount + (addVerticesCount >> 1) > capacity
		|| trianglesCount + addTrianglesCount > capacity * 3) {
		this->flush();
		texture = addTexture;
	}

	for (int i = 0; i < addTrianglesCount; ++i, ++trianglesCount)
		triangles[trianglesCount] = addTriangles[i] + verticesCount;

	for (int i = 0; i < addVerticesCount; i += 2, ++verticesCount) {
		ccV2F_C4B_T2F* vertex = vertices + verticesCount;
		vertex->vertices.x = addVertices[i];
		vertex->vertices.y = addVertices[i + 1];
		vertex->colors = *color;
		vertex->texCoords.u = uvs[i];
		vertex->texCoords.v = uvs[i + 1];
	}
}

void PolygonBatch::flush () {
	if (!verticesCount) return;

	ccGLBindTexture2D(texture->getName());
	glEnableVertexAttribArray(kCCVertexAttrib_Position);
	glEnableVertexAttribArray(kCCVertexAttrib_Color);
	glEnableVertexAttribArray(kCCVertexAttrib_TexCoords);
	glVertexAttribPointer(kCCVertexAttrib_Position, 2, GL_FLOAT, GL_FALSE, sizeof(ccV2F_C4B_T2F), &vertices[0].vertices);
	glVertexAttribPointer(kCCVertexAttrib_Color, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(ccV2F_C4B_T2F), &vertices[0].colors);
	glVertexAttribPointer(kCCVertexAttrib_TexCoords, 2, GL_FLOAT, GL_FALSE, sizeof(ccV2F_C4B_T2F), &vertices[0].texCoords);

	glDrawElements(GL_TRIANGLES, trianglesCount, GL_UNSIGNED_SHORT, triangles);

	verticesCount = 0;
	trianglesCount = 0;

	CHECK_GL_ERROR_DEBUG();
}

}
