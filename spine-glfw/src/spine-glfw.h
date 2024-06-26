#pragma once

#include <stdint.h>
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
void shader_set_matrix4(shader_t program, const char* name, const float *matrix);

/// Sets a uniform float by name
void shader_set_float(shader_t program, const char* name, float value);

/// Sets a uniform int by name
void shader_set_int(shader_t program, const char* name, int value);

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

/// Helper struct that contains a Spine atlas and the textures for each
/// atlas page
typedef struct {
    spine_atlas atlas;
    texture_t *textures;
} atlas_t;

/// Loads the .atlas file and its associated atlas pages as OpenGL textures
atlas_t *atlas_load(const char *file_path);

/// Disposes the atlas data and its associated OpenGL textures
void atlas_dispose(atlas_t *atlas);

/// Loads the skeleton data from the .skel or .json file using the given atlas
spine_skeleton_data skeleton_data_load(const char *file_path, atlas_t *atlas);

/// Renderer capable of rendering a spine_skeleton_drawable, using a shader, a mesh, and a
/// temporary CPU-side vertex buffer used to update the GPU-side mesh
typedef struct {
    shader_t shader;
    mesh_t *mesh;
    int vertex_buffer_size;
    vertex_t *vertex_buffer;
} renderer_t;

/// Creates a new renderer
renderer_t *renderer_create();

/// Sets the viewport size for the 2D orthographic projection
void renderer_set_viewport_size(renderer_t *renderer, int width, int height);

/// Draws the given skeleton drawbale. The atlas must be the atlas from which the drawable
/// was constructed.
void renderer_draw(renderer_t *renderer, spine_skeleton_drawable drawable, atlas_t *atlas);

/// Disposes the renderer
void renderer_dispose(renderer_t *renderer);