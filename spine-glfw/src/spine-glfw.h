/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#pragma once

#include <stdint.h>
#include <spine/spine.h>
#include <spine-cpp-lite.h>

/// A vertex of a mesh generated from a Spine skeleton
struct vertex_t {
	float x, y;
	uint32_t color;
	float u, v;
	uint32_t darkColor;
};

/// A GPU-side mesh using OpenGL vertex arrays, vertex buffer, and
/// indices buffer.
typedef struct {
	unsigned int vao;
	unsigned int vbo;
	int num_vertices;
	unsigned int ibo;
	int num_indices;
} mesh_t;

mesh_t *mesh_create();
void mesh_update(mesh_t *mesh, vertex_t *vertices, int num_vertices, uint16_t *indices, int num_indices);
void mesh_draw(mesh_t *mesh);
void mesh_dispose(mesh_t *mesh);

/// A shader (the OpenGL shader program id)
typedef unsigned int shader_t;

/// Creates a shader program from the vertex and fragment shader
shader_t shader_create(const char *vertex_shader, const char *fragment_shader);

/// Sets a uniform matrix by name
void shader_set_matrix4(shader_t program, const char *name, const float *matrix);

/// Sets a uniform float by name
void shader_set_float(shader_t program, const char *name, float value);

/// Sets a uniform int by name
void shader_set_int(shader_t program, const char *name, int value);

/// Binds the shader
void shader_use(shader_t shader);

/// Disposes the shader
void shader_dispose(shader_t shader);

/// A texture (the OpenGL texture object id)
typedef unsigned int texture_t;

/// Loads the given image and creates an OpenGL texture with default settings and auto-generated mipmap levels
texture_t texture_load(const char *file_path);

/// Binds the texture to texture unit 0
void texture_use(texture_t texture);

/// Disposes the texture
void texture_dispose(texture_t texture);

/// A TextureLoader implementation for OpenGL. Use this with spine::Atlas.
class GlTextureLoader : public spine::TextureLoader {
public:
	void load(spine::AtlasPage &page, const spine::String &path);
	void unload(void *texture);
};

/// Renderer capable of rendering a spine_skeleton_drawable, using a shader, a mesh, and a
/// temporary CPU-side vertex buffer used to update the GPU-side mesh
typedef struct {
	shader_t shader;
	mesh_t *mesh;
	int vertex_buffer_size;
	vertex_t *vertex_buffer;
	spine::SkeletonRenderer *renderer;
} renderer_t;

/// Creates a new renderer
renderer_t *renderer_create();

/// Sets the viewport size for the 2D orthographic projection
void renderer_set_viewport_size(renderer_t *renderer, int width, int height);

/// Draws the given skeleton. The atlas must be the atlas from which the drawable
/// was constructed.
void renderer_draw(renderer_t *renderer, spine::Skeleton *skeleton, bool premultipliedAlpha);

/// Draws the given skeleton. The atlas must be the atlas from which the drawable
/// was constructed.
void renderer_draw_lite(renderer_t *renderer, spine_skeleton skeleton, bool premultipliedAlpha);

/// Disposes the renderer
void renderer_dispose(renderer_t *renderer);
