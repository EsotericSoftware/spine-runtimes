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

#import <spine/PolygonBatch.h>
#import <spine/spine-cocos2d-iphone.h>
#import <spine/extension.h>

@implementation spPolygonBatch

+ (id) createWithCapacity:(int)capacity {
	return [[(spPolygonBatch*)[self alloc] initWithCapacity:capacity] autorelease];
}

- (id) initWithCapacity:(int)capacity {
	// 32767 is max index, so 32767 / 3 - (32767 / 3 % 3) = 10920.
	NSAssert(capacity <= 10920, @"capacity cannot be > 10920");
	NSAssert(capacity >= 0, @"capacity cannot be < 0");

	self = [super init];
	if (!self) return nil;

	_capacity = capacity;
	_vertices = MALLOC(ccV2F_C4B_T2F, capacity);
	_triangles = MALLOC(GLushort, capacity * 3);

	return self;
}

- (void) dealloc {
	FREE(_vertices);
	FREE(_triangles);
	[super dealloc];
}

- (void) add:(CCTexture2D*)addTexture vertices:(const float*)addVertices uvs:(const float*)uvs
	verticesCount:(int)addVerticesCount triangles:(const int*)addTriangles trianglesCount:(int)addTrianglesCount
	color:(ccColor4B*)color {

	if (
		addTexture != _texture
		|| _verticesCount + (addVerticesCount >> 1) > _capacity
		|| _trianglesCount + addTrianglesCount > _capacity * 3) {
		[self flush];
		_texture = addTexture;
	}

	for (int i = 0; i < addTrianglesCount; ++i, ++_trianglesCount)
		_triangles[_trianglesCount] = addTriangles[i] + _verticesCount;

	for (int i = 0; i < addVerticesCount; i += 2, ++_verticesCount) {
		ccV2F_C4B_T2F* vertex = _vertices + _verticesCount;
		vertex->vertices.x = addVertices[i];
		vertex->vertices.y = addVertices[i + 1];
		vertex->colors = *color;
		vertex->texCoords.u = uvs[i];
		vertex->texCoords.v = uvs[i + 1];
	}
}

- (void) flush {
	if (!_verticesCount) return;

	ccGLBindTexture2D(_texture.name);
	glEnableVertexAttribArray(kCCVertexAttrib_Position);
	glEnableVertexAttribArray(kCCVertexAttrib_Color);
	glEnableVertexAttribArray(kCCVertexAttrib_TexCoords);
	glVertexAttribPointer(kCCVertexAttrib_Position, 2, GL_FLOAT, GL_FALSE, sizeof(ccV2F_C4B_T2F), &_vertices[0].vertices);
	glVertexAttribPointer(kCCVertexAttrib_Color, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(ccV2F_C4B_T2F), &_vertices[0].colors);
	glVertexAttribPointer(kCCVertexAttrib_TexCoords, 2, GL_FLOAT, GL_FALSE, sizeof(ccV2F_C4B_T2F), &_vertices[0].texCoords);

	glDrawElements(GL_TRIANGLES, _trianglesCount, GL_UNSIGNED_SHORT, _triangles);

	_verticesCount = 0;
	_trianglesCount = 0;

	CHECK_GL_ERROR_DEBUG();
}

@end
