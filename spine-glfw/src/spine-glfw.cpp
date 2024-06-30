#include "spine-glfw.h"
#include <stdio.h>
#include <glbinding/gl/gl.h>
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

using namespace gl;

/// A blend mode, see https://en.esotericsoftware.com/spine-slots#Blending
/// Encodes the OpenGL source and destination blend function for both premultiplied and
/// non-premultiplied alpha blending.
typedef struct {
    unsigned int source_color;
    unsigned int source_color_pma;
    unsigned int dest_color;
    unsigned int source_alpha;
} blend_mode_t;

/// The 4 supported blend modes SPINE_BLEND_MODE_NORMAL, SPINE_BLEND_MODE_ADDITIVE, SPINE_BLEND_MODE_MULTIPLY,
/// and SPINE_BLEND_MODE_SCREEN, expressed as OpenGL blend functions.
blend_mode_t blend_modes[] = {
        {(unsigned int)GL_SRC_ALPHA, (unsigned int)GL_ONE, (unsigned int)GL_ONE_MINUS_SRC_ALPHA, (unsigned int)GL_ONE},
        {(unsigned int)GL_SRC_ALPHA, (unsigned int)GL_ONE, (unsigned int)GL_ONE, (unsigned int)GL_ONE},
        {(unsigned int)GL_DST_COLOR, (unsigned int)GL_DST_COLOR, (unsigned int)GL_ONE_MINUS_SRC_ALPHA, (unsigned int)GL_ONE_MINUS_SRC_ALPHA},
        {(unsigned int)GL_ONE, (unsigned int)GL_ONE, (unsigned int)GL_ONE_MINUS_SRC_COLOR, (unsigned int)GL_ONE_MINUS_SRC_COLOR}
};

mesh_t *mesh_create() {
    GLuint vao, vbo, ibo;
    glGenVertexArrays(1, &vao);
    glGenBuffers(1, &vbo);
    glGenBuffers(1, &ibo);

    glBindVertexArray(vao);

    glBindBuffer(GL_ARRAY_BUFFER, vbo);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ibo);

    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(vertex_t), (void*)offsetof(vertex_t, x));
    glEnableVertexAttribArray(0);

    glVertexAttribPointer(1, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(vertex_t), (void*)offsetof(vertex_t, color));
    glEnableVertexAttribArray(1);

    glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, sizeof(vertex_t), (void*)offsetof(vertex_t, u));
    glEnableVertexAttribArray(2);

    glVertexAttribPointer(3, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(vertex_t), (void*)offsetof(vertex_t, darkColor));
    glEnableVertexAttribArray(3);

    glBindVertexArray(0);

    mesh_t *mesh = (mesh_t*)malloc(sizeof(mesh_t));
    mesh->vao = vao;
    mesh->vbo = vbo;
    mesh->num_vertices = 0;
    mesh->ibo = ibo;
    mesh->num_indices = 0;
    return mesh;
}

void mesh_update(mesh_t *mesh, vertex_t *vertices, int num_vertices, uint16_t *indices, int num_indices) {
    glBindVertexArray(mesh->vao);

    glBindBuffer(GL_ARRAY_BUFFER, mesh->vbo);
    glBufferData(GL_ARRAY_BUFFER, num_vertices * sizeof(vertex_t), vertices, GL_STATIC_DRAW);
    mesh->num_vertices = num_vertices;
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, mesh->ibo);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, num_indices * sizeof(uint16_t), indices, GL_STATIC_DRAW);
    mesh->num_indices = num_indices;

    glBindVertexArray(0);
}

void mesh_draw(mesh_t *mesh) {
    glBindVertexArray(mesh->vao);
    glDrawElements(GL_TRIANGLES, mesh->num_indices, GL_UNSIGNED_SHORT, 0);
    glBindVertexArray(0);
}

void mesh_dispose(mesh_t *mesh) {
    glDeleteBuffers(1, &mesh->vbo);
    glDeleteBuffers(1, &mesh->ibo);
    glDeleteVertexArrays(1, &mesh->vao);
    free(mesh);
}

GLuint compile_shader(const char* source, GLenum type) {
    GLuint shader = glCreateShader(type);
    glShaderSource(shader, 1, &source, nullptr);
    glCompileShader(shader);

    GLint success;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetShaderInfoLog(shader, 512, nullptr, infoLog);
        printf("Error, shader compilation failed:\n%s\n", infoLog);
        glDeleteShader(shader);
        return 0;
    }

    return shader;
}

shader_t shader_create(const char* vertex_shader, const char* fragment_shader) {
    shader_t program;

    GLuint vertex_shader_id = compile_shader(vertex_shader, GL_VERTEX_SHADER);
    GLuint fragment_shader_id = compile_shader(fragment_shader, GL_FRAGMENT_SHADER);
    if (!vertex_shader_id || !fragment_shader_id) {
        glDeleteShader(vertex_shader_id);
        glDeleteShader(fragment_shader_id);
        return 0;
    }

    program = glCreateProgram();
    glAttachShader(program, vertex_shader_id);
    glAttachShader(program, fragment_shader_id);
    glLinkProgram(program);

    GLint success;
    glGetProgramiv(program, GL_LINK_STATUS, &success);
    if (!success) {
        char infoLog[512];
        glGetProgramInfoLog(program, 512, nullptr, infoLog);
        printf("Error, shader linking failed:\n%s\n", infoLog);
        glDeleteProgram(program);
        program = 0;
    }
    glDeleteShader(vertex_shader_id);
    glDeleteShader(fragment_shader_id);
    return program;
}

void shader_set_matrix4(shader_t shader, const char* name, const float *matrix) {
    shader_use(shader);
    GLint location = glGetUniformLocation(shader, name);
    glUniformMatrix4fv(location, 1, GL_FALSE, matrix);
}

void shader_set_float(shader_t shader, const char* name, float value) {
    shader_use(shader);
    GLint location = glGetUniformLocation(shader, name);
    glUniform1f(location, value);
}

void shader_set_int(shader_t shader, const char* name, int value) {
    shader_use(shader);
    GLint location = glGetUniformLocation(shader, name);
    glUniform1i(location, value);
}

void shader_use(shader_t program) {
    glUseProgram(program);
}

void shader_dispose(shader_t program) {
    glDeleteProgram(program);
}

texture_t texture_load(const char *file_path) {
    int width, height, nrChannels;
    unsigned char *data = stbi_load(file_path, &width, &height, &nrChannels, 0);
    if (!data) {
        printf("Failed to load texture\n");
        return 0;
    }

    GLenum format = GL_RGBA;
    if (nrChannels == 1)
        format = GL_RED;
    else if (nrChannels == 3)
        format = GL_RGB;
    else if (nrChannels == 4)
        format = GL_RGBA;

    texture_t texture;
    glGenTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, texture);
    glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data);
    glGenerateMipmap(GL_TEXTURE_2D);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    stbi_image_free(data);
    return texture;
}

void texture_use(texture_t texture) {
    glActiveTexture(GL_TEXTURE0); // Set active texture unit to 0
    glBindTexture(GL_TEXTURE_2D, texture);
}

void texture_dispose(texture_t texture) {
    glDeleteTextures(1, &texture);
}

void matrix_ortho_projection(float *matrix, float width, float height) {
    memset(matrix, 0, 16 * sizeof(float));

    float left = 0.0f;
    float right = width;
    float bottom = height;
    float top = 0.0f;
    float near = -1.0f;
    float far = 1.0f;

    matrix[0] = 2.0f / (right - left);
    matrix[5] = 2.0f / (top - bottom);
    matrix[10] = -2.0f / (far - near);
    matrix[12] = -(right + left) / (right - left);
    matrix[13] = -(top + bottom) / (top - bottom);
    matrix[14] = -(far + near) / (far - near);
    matrix[15] = 1.0f;
}

const uint8_t *file_read(const char *path, int *length) {
    uint8_t *data;
    FILE *file = fopen(path, "rb");
    if (!file) return 0;
    fseek(file, 0, SEEK_END);
    *length = (int) ftell(file);
    fseek(file, 0, SEEK_SET);
    data = (uint8_t*)(malloc(*length + 1));
    fread(data, 1, *length, file);
    fclose(file);
    data[*length] = 0;
    return data;
}

atlas_t *atlas_load(const char *file_path) {
    int length = 0;
    utf8 *atlas_data = (utf8*)file_read(file_path, &length);
    if (!atlas_data) {
        printf("Could not load atlas %s\n", file_path);
        return nullptr;
    }

    spine_atlas spine_atlas = spine_atlas_load(atlas_data);
    free(atlas_data);
    if (!spine_atlas) {
        printf("Could not load atlas %s\n", file_path);
        return nullptr;
    }
    atlas_t *atlas = (atlas_t*)malloc(sizeof(atlas_t));
    atlas->atlas = spine_atlas;
    int num_textures = spine_atlas_get_num_image_paths(spine_atlas);
    atlas->textures = (texture_t*)malloc(sizeof(texture_t) * num_textures);
    memset(atlas->textures, 0, sizeof(texture_t) * num_textures);

    char parent_dir[1024];
    strncpy(parent_dir, file_path, sizeof(parent_dir));
    char *last_slash = strrchr(parent_dir, '/');
    if (last_slash) {
        *(last_slash + 1) = '\0';
    } else {
        parent_dir[0] = '\0';
    }

    for (int i = 0; i < num_textures; i++) {
        char *relative_path = spine_atlas_get_image_path(spine_atlas, i);
        char full_path[1024];
        snprintf(full_path, sizeof(full_path), "%s%s", parent_dir, relative_path);
        texture_t texture = texture_load(full_path);
        if (!texture) {
            printf("Could not load atlas texture %s\n", full_path);
            atlas_dispose(atlas);
            return nullptr;
        }
        atlas->textures[i] = texture;
    }

    return atlas;
}

void atlas_dispose(atlas_t *atlas) {
    for (int i = 0; i < spine_atlas_get_num_image_paths(atlas->atlas); i++) {
        texture_dispose(atlas->textures[i]);
    }
    spine_atlas_dispose(atlas->atlas);
    free(atlas->textures);
    free(atlas);
}

spine_skeleton_data skeleton_data_load(const char *file_path, atlas_t *atlas) {
    int length = 0;
    uint8_t *data = (uint8_t*)file_read(file_path, &length);
    if (!data) {
        printf("Could not load skeleton data file %s\n", file_path);
        return nullptr;
    }

    spine_skeleton_data_result result;
    const char *ext = strrchr(file_path, '.');
    if (ext && strcmp(ext, ".skel") == 0) {
        result = spine_skeleton_data_load_binary(atlas->atlas, data, length);
    } else {
        result = spine_skeleton_data_load_json(atlas->atlas, (utf8*)data);
    }
    free(data);

    if (spine_skeleton_data_result_get_error(result)) {
        printf("Could not load skeleton data file %s:\n%s\n", file_path, spine_skeleton_data_result_get_error(result));
        spine_skeleton_data_result_dispose(result);
        return nullptr;
    }
    spine_skeleton_data skeleton_data = spine_skeleton_data_result_get_data(result);
    spine_skeleton_data_result_dispose(result);
    return skeleton_data;
}

renderer_t *renderer_create() {
    shader_t shader = shader_create(R"(
        #version 330 core
        layout (location = 0) in vec2 aPos;
        layout (location = 1) in vec4 aLightColor;
        layout (location = 2) in vec2 aTexCoord;
        layout (location = 3) in vec4 aDarkColor;

        uniform mat4 uMatrix;

        out vec4 lightColor;
        out vec4 darkColor;
        out vec2 texCoord;

        void main() {
            lightColor = aLightColor;
            darkColor = aDarkColor;
            texCoord = aTexCoord;
            gl_Position = uMatrix * vec4(aPos, 0.0, 1.0);
        }
    )", R"(
        #version 330 core
        in vec4 lightColor;
        in vec4 darkColor;
        in vec2 texCoord;
        out vec4 fragColor;

        uniform sampler2D uTexture;
        void main() {
            vec4 texColor = texture(uTexture, texCoord);
            float alpha = texColor.a * lightColor.a;
            fragColor.a = alpha;
            fragColor.rgb = ((texColor.a - 1.0) * darkColor.a + 1.0 - texColor.rgb) * darkColor.rgb + texColor.rgb * lightColor.rgb;
        }
    )");
    if (!shader) return nullptr;
    mesh_t *mesh = mesh_create();
    renderer_t *renderer = (renderer_t*)malloc(sizeof(renderer_t));
    renderer->shader = shader;
    renderer->mesh = mesh;
    renderer->vertex_buffer_size = 0;
    renderer->vertex_buffer = nullptr;
    return renderer;
}

void renderer_set_viewport_size(renderer_t *renderer, int width, int height) {
    float matrix[16];
    matrix_ortho_projection(matrix, width, height);
    shader_use(renderer->shader);
    shader_set_matrix4(renderer->shader, "uMatrix", matrix);
}

void renderer_draw(renderer_t *renderer, spine_skeleton_drawable drawable, atlas_t *atlas) {
    shader_use(renderer->shader);
    shader_set_int(renderer->shader, "uTexture", 0);
    gl::glEnable(gl::GLenum::GL_BLEND);

    spine_render_command command = spine_skeleton_drawable_render(drawable);
    while (command) {
        int num_command_vertices = spine_render_command_get_num_vertices(command);
        if (renderer->vertex_buffer_size < num_command_vertices) {
            renderer->vertex_buffer_size = num_command_vertices;
            free(renderer->vertex_buffer);
            renderer->vertex_buffer = (vertex_t *)malloc(sizeof(vertex_t) * renderer->vertex_buffer_size);
        }
        float *positions = spine_render_command_get_positions(command);
        float *uvs = spine_render_command_get_uvs(command);
        int32_t *colors = spine_render_command_get_colors(command);
        int32_t *darkColors = spine_render_command_get_dark_colors(command);
        for (int i = 0, j = 0; i < num_command_vertices; i++, j += 2) {
            vertex_t *vertex = &renderer->vertex_buffer[i];
            vertex->x = positions[j];
            vertex->y = positions[j + 1];
            vertex->u = uvs[j];
            vertex->v = uvs[j+1];
            uint32_t color = colors[i];
            vertex->color = (color & 0xFF00FF00) | ((color & 0x00FF0000) >> 16) | ((color & 0x000000FF) << 16);
            uint32_t darkColor = darkColors[i];
            vertex->darkColor = (darkColor & 0xFF00FF00) | ((darkColor & 0x00FF0000) >> 16) | ((darkColor & 0x000000FF) << 16);
        }
        int num_command_indices = spine_render_command_get_num_indices(command);
        uint16_t *indices = spine_render_command_get_indices(command);
        mesh_update(renderer->mesh, renderer->vertex_buffer, num_command_vertices, indices, num_command_indices);

        blend_mode_t blend_mode = blend_modes[spine_render_command_get_blend_mode(command)];
        gl::glBlendFuncSeparate(spine_atlas_is_pma(atlas->atlas) ? (gl::GLenum)blend_mode.source_color_pma : (gl::GLenum)blend_mode.source_color, (gl::GLenum)blend_mode.dest_color, (gl::GLenum)blend_mode.source_alpha, (gl::GLenum)blend_mode.dest_color);

        texture_t texture = atlas->textures[spine_render_command_get_atlas_page(command)];
        texture_use(texture);

        mesh_draw(renderer->mesh);
        command = spine_render_command_get_next(command);
    }
}

void renderer_dispose(renderer_t *renderer) {
    shader_dispose(renderer->shader);
    mesh_dispose(renderer->mesh);
    free(renderer->vertex_buffer);
    free(renderer);
}