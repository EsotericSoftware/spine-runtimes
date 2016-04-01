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

#include <spine/SkeletonBatch.h>
#include <spine/extension.h>
#include <algorithm>

USING_NS_CC;
using namespace std;

namespace spine {

static SkeletonBatch* instance = nullptr;

void SkeletonBatch::setCommandSize (int maxVertices, int maxTriangles) {
	// 32767 is max index, so 32767 / 3 - (32767 / 3 % 3) = 10920.
	CCASSERT(maxTriangles <= 10920, "maxTriangles cannot be > 10920");
	CCASSERT(maxTriangles >= 0, "maxTriangles cannot be < 0");
	if (instance) delete instance;
	instance = new SkeletonBatch(maxVertices, maxTriangles);
}

SkeletonBatch* SkeletonBatch::getInstance () {
	if (!instance) instance = new SkeletonBatch(64, 64 * 3);
	return instance;
}

SkeletonBatch::SkeletonBatch (int maxVertices, int maxTriangles) :
	_maxVertices(maxVertices), _maxTriangles(maxTriangles),
	_renderer(nullptr), _transform(nullptr), _transformFlags(0), _globalZOrder(0), _glProgramState(nullptr),
	_texture(nullptr), _blendMode(SP_BLEND_MODE_NORMAL)
{
	_firstCommand = new Command(maxVertices, maxTriangles);
	_command = _firstCommand;

	Director::getInstance()->getScheduler()->scheduleUpdate(this, -1, false);
}

SkeletonBatch::~SkeletonBatch () {
	Director::getInstance()->getScheduler()->unscheduleUpdate(this);

	Command* command = _firstCommand;
	while (command) {
		Command* next = command->_next;
		delete command;
		command = next;
	}
}

void SkeletonBatch::setRendererState (Renderer* renderer, const Mat4* transform, uint32_t transformFlags,
		float globalZOrder, GLProgramState* glProgramState, bool premultipliedAlpha) {
	_renderer = renderer;
	_transform = transform;
	_transformFlags = transformFlags;
	_globalZOrder = globalZOrder;
	_glProgramState = glProgramState;
	_premultipliedAlpha = premultipliedAlpha;
}

void SkeletonBatch::update (float delta) {
	// Reuse commands at the beginning of each frame.
	_command = _firstCommand;
	_command->_triangles->vertCount = 0;
	_command->_triangles->indexCount = 0;
}

void SkeletonBatch::add (const Texture2D* addTexture,
	const float* addVertices, const float* uvs, int addVerticesCount,
	const int* addTriangles, int addTrianglesCount,
	const Color4B& color, spBlendMode blendMode
) {
	if (addTexture != _texture
		|| blendMode != _blendMode
		|| _command->_triangles->vertCount + (addVerticesCount >> 1) > _maxVertices
		|| _command->_triangles->indexCount + addTrianglesCount > _maxTriangles
	) {
		this->flush(max(addVerticesCount >> 1, _maxVertices), max(addTrianglesCount, _maxTriangles));
		_texture = addTexture;
		_blendMode = blendMode;
	}

	TrianglesCommand::Triangles* triangles = _command->_triangles;
	for (int i = 0; i < addTrianglesCount; ++i, ++triangles->indexCount)
		triangles->indices[triangles->indexCount] = addTriangles[i] + triangles->vertCount;

	for (int i = 0; i < addVerticesCount; i += 2, ++triangles->vertCount) {
		V3F_C4B_T2F* vertex = triangles->verts + triangles->vertCount;
		vertex->vertices.x = addVertices[i];
		vertex->vertices.y = addVertices[i + 1];
		vertex->colors = color;
		vertex->texCoords.u = uvs[i];
		vertex->texCoords.v = uvs[i + 1];
	}
}

void SkeletonBatch::flush (int maxVertices, int maxTriangles) {
	if (!_command->_triangles->vertCount) return;

	BlendFunc blendFunc;
	switch (_blendMode) {
	case SP_BLEND_MODE_ADDITIVE:
		blendFunc.src = _premultipliedAlpha ? GL_ONE : GL_SRC_ALPHA;
		blendFunc.dst = GL_ONE;
		break;
	case SP_BLEND_MODE_MULTIPLY:
		blendFunc.src = GL_DST_COLOR;
		blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
		break;
	case SP_BLEND_MODE_SCREEN:
		blendFunc.src = GL_ONE;
		blendFunc.dst = GL_ONE_MINUS_SRC_COLOR;
		break;
	default:
		blendFunc.src = _premultipliedAlpha ? GL_ONE : GL_SRC_ALPHA;
		blendFunc.dst = GL_ONE_MINUS_SRC_ALPHA;
	}

	_command->_trianglesCommand->init(_globalZOrder, _texture->getName(), _glProgramState, blendFunc, *_command->_triangles,
		*_transform, _transformFlags);
	_renderer->addCommand(_command->_trianglesCommand);

	if (!_command->_next) _command->_next = new Command(maxVertices, maxTriangles);
	_command = _command->_next;

	// If not as large as required, insert new command.
	if (_command->_maxVertices < maxVertices || _command->_maxTriangles < maxTriangles) {
		Command* next = _command->_next;
		_command = new Command(maxVertices, maxTriangles);
		_command->_next = next;
	}

	_command->_triangles->vertCount = 0;
	_command->_triangles->indexCount = 0;
}

SkeletonBatch::Command::Command (int maxVertices, int maxTriangles) :
	_maxVertices(maxVertices), _maxTriangles(maxTriangles), _next(nullptr)
{
	_trianglesCommand = new TrianglesCommand();

	_triangles = new TrianglesCommand::Triangles();
	_triangles->verts = new V3F_C4B_T2F[maxVertices];
	_triangles->indices = new GLushort[maxTriangles];
}

SkeletonBatch::Command::~Command () {
	delete [] _triangles->indices;
	delete [] _triangles->verts;
	delete _triangles;

	delete _trianglesCommand;
}

}
