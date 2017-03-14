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

#include "GLUtils.h"

#include <spine/extension.h>

#include <TargetConditionals.h>
#if TARGET_IPHONE_SIMULATOR || TARGET_OS_IPHONE
#include <OpenGLES/ES2/gl.h>
#include <OpenGLES/ES2/glext.h>
#else
#include <OpenGL/gl.h>
#include <OpenGL/glu.h>
#endif

#include <stdio.h>

#define STRINGIFY(A)  #A
#define MAX_VERTICES 64000
#define MAX_INDICES 64000

const char* TWO_COLOR_TINT_VERTEX_SHADER = STRINGIFY(
attribute vec4 a_position;
attribute vec4 a_color;
attribute vec4 a_color2;
attribute vec2 a_texCoords;
													 
uniform mat4 transform;

\n#ifdef GL_ES\n
varying lowp vec4 v_light;
varying lowp vec4 v_dark;
varying mediump vec2 v_texCoord;
\n#else\n
varying vec4 v_light;
varying vec4 v_dark;
varying vec2 v_texCoord;

\n#endif\n

void main() {
	v_light = a_color;
	v_dark = a_color2;
	v_texCoord = a_texCoords;
	gl_Position = transform * a_position;
}
);

const char* TWO_COLOR_TINT_FRAGMENT_SHADER = STRINGIFY(
\n#ifdef GL_ES\n
precision lowp float;
\n#endif\n
													   
uniform sampler2D texture;

varying vec4 v_light;
varying vec4 v_dark;
varying vec2 v_texCoord;

void main() {
   vec4 texColor = texture2D(texture, v_texCoord);
   float alpha = texColor.a * v_light.a;
   gl_FragColor.a = alpha;
   gl_FragColor.rgb = (1.0 - texColor.rgb) * v_dark.rgb * alpha + texColor.rgb * v_light.rgb;	
}
);

spMesh* spMesh_create(uint32_t numVertices, uint32_t numIndices) {
	spMesh* mesh = MALLOC(spMesh, 1);
	mesh->vertices = MALLOC(spVertex, numVertices);
	mesh->indices = MALLOC(unsigned short, numIndices);
	mesh->numVertices = numVertices;
	mesh->numIndices = numIndices;
	mesh->numAllocatedVertices = 0;
	mesh->numAllocatedIndices = 0;
	return mesh;
}

void spMesh_allocatePart(spMesh* mesh, spMeshPart* part, uint32_t numVertices, uint32_t numIndices, uint32_t textureHandle, uint32_t srcBlend, uint32_t dstBlend) {
	if (mesh->numVertices < mesh->numAllocatedVertices + numVertices) {
		mesh->numVertices = mesh->numAllocatedVertices + numVertices;
		mesh->vertices = REALLOC(mesh->vertices, spVertex, mesh->numVertices);
	}
	if (mesh->numIndices < mesh->numAllocatedIndices + numIndices) {
		mesh->numIndices = mesh->numAllocatedIndices + numIndices;
		mesh->indices = REALLOC(mesh->indices, unsigned short, mesh->numIndices);
	}
	
	part->mesh = mesh;
	part->startVertex = mesh->numAllocatedVertices;
	part->numIndices = numIndices;
	part->startIndex = mesh->numAllocatedIndices;
	part->numVertices = numVertices;
	mesh->numAllocatedVertices += numVertices;
	mesh->numAllocatedIndices += numIndices;
}

void spMesh_clearParts(spMesh* mesh) {
	mesh->numAllocatedIndices = 0;
	mesh->numAllocatedVertices = 0;
}

void spMesh_dispose(spMesh* mesh) {
	FREE(mesh->vertices);
	FREE(mesh->indices);
	FREE(mesh);
}

GLuint compileShader(GLenum shaderType, const char* shaderSource) {
	GLuint shader = glCreateShader(shaderType);
	glShaderSource(shader, 1, &shaderSource, 0);
	glCompileShader(shader);
	GLint status;
	glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
	if (!status) {
		GLsizei length;
		glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &length);
		if (length < 1) {
			printf("Unknown error while compiling shader\n");
			exit(-1);
		} else {
			char* log = MALLOC(char, length);
			glGetShaderInfoLog(shader, length, 0, log);
			printf("Error compiling shader: %s\n", log);
			exit(-1);
		}
	}
	return shader;
}

spShader* spShader_create(const char* vertexShaderSource, const char* fragmentShaderSource) {
	GLuint vertexShader = compileShader(GL_VERTEX_SHADER, vertexShaderSource);
	GLuint fragmentShader = compileShader(GL_FRAGMENT_SHADER, fragmentShaderSource);
	
	GLuint program = glCreateProgram();
	glAttachShader(program, vertexShader);
	glAttachShader(program, fragmentShader);
	glLinkProgram(program);
	
	GLint status;
	glGetProgramiv(program, GL_LINK_STATUS, &status);
	if (!status) {
		printf("Unknown error while linking program\n");
		exit(-1);
	}
	
	spShader* shader = MALLOC(spShader, 1);
	shader->program = program;
	shader->vertexShader = vertexShader;
	shader->fragmentShader = fragmentShader;
	return shader;
}

void spShader_dispose(spShader* shader) {
	glDeleteProgram(shader->program);
	glDeleteShader(shader->vertexShader);
	glDeleteShader(shader->fragmentShader);
	FREE(shader);
}

spTwoColorBatcher* spTwoColorBatcher_create() {
	spTwoColorBatcher* batcher = MALLOC(spTwoColorBatcher, 1);
	
	batcher->shader = spShader_create(TWO_COLOR_TINT_VERTEX_SHADER, TWO_COLOR_TINT_FRAGMENT_SHADER);
	batcher->positionAttributeLocation = glGetAttribLocation(batcher->shader->program, "a_position");
	batcher->colorAttributeLocation = glGetAttribLocation(batcher->shader->program, "a_color");
	batcher->color2AttributeLocation = glGetAttribLocation(batcher->shader->program, "a_color2");
	batcher->texCoordsAttributeLocation = glGetAttribLocation(batcher->shader->program, "a_texCoords");
	
	glGenBuffers(1, &batcher->vertexBufferHandle);
	glGenBuffers(1, &batcher->indexBufferHandle);
	batcher->verticesBuffer = MALLOC(spVertex, MAX_VERTICES);
	batcher->indicesBuffer = MALLOC(unsigned short, MAX_INDICES);
	batcher->numIndices = 0;
	batcher->numVertices = 0;
	return batcher;
}

void spTwoColorBatcher_add(spTwoColorBatcher* batcher, spMeshPart* mesh) {
	
}

void spTwoColorBatcher_flush(spTwoColorBatcher* batcher) {
	
}

void spDisposeTwoColorBatcher(spTwoColorBatcher* batcher) {
	spShader_dispose(batcher->shader);
	glDeleteBuffers(1, &batcher->vertexBufferHandle);
	FREE(batcher->verticesBuffer);
	glDeleteBuffers(1, &batcher->indexBufferHandle);
	FREE(batcher->indicesBuffer);
}
