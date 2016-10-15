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

#include "SimpleCommand.h"

USING_NS_CC;
using namespace std;

Scene* SimpleCommand::scene () {
	Scene *scene = Scene::create();
	scene->addChild(SimpleCommand::create());
	return scene;
}

bool SimpleCommand::init () {
	if (!Node::init()) return false;

	setGLProgramState(GLProgramState::getOrCreateWithGLProgramName(GLProgram::SHADER_NAME_POSITION_TEXTURE_COLOR_NO_MVP));

	_texture = _director->getTextureCache()->addImage("sprite.png");
	
	setPosition(100, 100);

	return true;
}

void SimpleCommand::draw (Renderer* renderer, const Mat4& transform, uint32_t transformFlags) {
	TrianglesCommand::Triangles* triangles = new TrianglesCommand::Triangles();
	
	float x = 0, y = 0;
	float w = 80, h = 80;

	triangles->vertCount = 4;
	triangles->verts = new V3F_C4B_T2F[4];
	triangles->verts[0].colors = Color4B::WHITE;
	triangles->verts[0].texCoords.u = 0;
	triangles->verts[0].texCoords.v = 1;
	triangles->verts[0].vertices.x = 0;
	triangles->verts[0].vertices.y = 0;
	triangles->verts[0].vertices.z = 0;

	triangles->verts[1].colors = Color4B::WHITE;
	triangles->verts[1].texCoords.u = 0;
	triangles->verts[1].texCoords.v = 0;
	triangles->verts[1].vertices.x = 0;
	triangles->verts[1].vertices.y = h;
	triangles->verts[1].vertices.z = 0;

	triangles->verts[2].colors = Color4B::WHITE;
	triangles->verts[2].texCoords.u = 1;
	triangles->verts[2].texCoords.v = 1;
	triangles->verts[2].vertices.x = w;
	triangles->verts[2].vertices.y = 0;
	triangles->verts[2].vertices.z = 0;

	triangles->verts[3].colors = Color4B::WHITE;
	triangles->verts[3].texCoords.u = 1;
	triangles->verts[3].texCoords.v = 0;
	triangles->verts[3].vertices.x = w;
	triangles->verts[3].vertices.y = h;
	triangles->verts[3].vertices.z = 0;

	triangles->indexCount = 6;
	triangles->indices = new GLushort[6];
	triangles->indices[0] = 0;
	triangles->indices[1] = 1;
	triangles->indices[2] = 2;
	triangles->indices[3] = 3;
	triangles->indices[4] = 2;
	triangles->indices[5] = 1;

	TrianglesCommand* trianglesCommand = new TrianglesCommand();
	trianglesCommand->init(_globalZOrder, _texture->getName(), getGLProgramState(), BlendFunc::ALPHA_PREMULTIPLIED, *triangles, transform, transformFlags);
   renderer->addCommand(trianglesCommand);
}
