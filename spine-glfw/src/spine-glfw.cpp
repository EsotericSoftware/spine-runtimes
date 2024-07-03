#include "spine-glfw.h"
#include <cstdio>
#include <glbinding/gl/gl.h>
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

using namespace gl;
using namespace spine;

/// Set the default extension used for memory allocations and file I/O
SpineExtension *spine::getDefaultExtension() {
	return new spine::DefaultSpineExtension();
}

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
		{(unsigned int) GL_SRC_ALPHA, (unsigned int) GL_ONE, (unsigned int) GL_ONE_MINUS_SRC_ALPHA, (unsigned int) GL_ONE},
		{(unsigned int) GL_SRC_ALPHA, (unsigned int) GL_ONE, (unsigned int) GL_ONE, (unsigned int) GL_ONE},
		{(unsigned int) GL_DST_COLOR, (unsigned int) GL_DST_COLOR, (unsigned int) GL_ONE_MINUS_SRC_ALPHA, (unsigned int) GL_ONE_MINUS_SRC_ALPHA},
		{(unsigned int) GL_ONE, (unsigned int) GL_ONE, (unsigned int) GL_ONE_MINUS_SRC_COLOR, (unsigned int) GL_ONE_MINUS_SRC_COLOR}};

mesh_t *mesh_create() {
	GLuint vao, vbo, ibo;
	glGenVertexArrays(1, &vao);
	glGenBuffers(1, &vbo);
	glGenBuffers(1, &ibo);

	glBindVertexArray(vao);

	glBindBuffer(GL_ARRAY_BUFFER, vbo);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ibo);

	glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(vertex_t), (void *) offsetof(vertex_t, x));
	glEnableVertexAttribArray(0);

	glVertexAttribPointer(1, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(vertex_t), (void *) offsetof(vertex_t, color));
	glEnableVertexAttribArray(1);

	glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, sizeof(vertex_t), (void *) offsetof(vertex_t, u));
	glEnableVertexAttribArray(2);

	glVertexAttribPointer(3, 4, GL_UNSIGNED_BYTE, GL_TRUE, sizeof(vertex_t), (void *) offsetof(vertex_t, darkColor));
	glEnableVertexAttribArray(3);

	glBindVertexArray(0);

	auto *mesh = (mesh_t *) malloc(sizeof(mesh_t));
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
	glBufferData(GL_ARRAY_BUFFER, (GLsizeiptr) (num_vertices * sizeof(vertex_t)), vertices, GL_STATIC_DRAW);
	mesh->num_vertices = num_vertices;
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, mesh->ibo);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, (GLsizeiptr) (num_indices * sizeof(uint16_t)), indices, GL_STATIC_DRAW);
	mesh->num_indices = num_indices;

	glBindVertexArray(0);
}

void mesh_draw(mesh_t *mesh) {
	glBindVertexArray(mesh->vao);
	glDrawElements(GL_TRIANGLES, mesh->num_indices, GL_UNSIGNED_SHORT, nullptr);
	glBindVertexArray(0);
}

void mesh_dispose(mesh_t *mesh) {
	glDeleteBuffers(1, &mesh->vbo);
	glDeleteBuffers(1, &mesh->ibo);
	glDeleteVertexArrays(1, &mesh->vao);
	free(mesh);
}

GLuint compile_shader(const char *source, GLenum type) {
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

shader_t shader_create(const char *vertex_shader, const char *fragment_shader) {
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

void shader_set_matrix4(shader_t shader, const char *name, const float *matrix) {
	shader_use(shader);
	GLint location = glGetUniformLocation(shader, name);
	glUniformMatrix4fv(location, 1, GL_FALSE, matrix);
}

void shader_set_float(shader_t shader, const char *name, float value) {
	shader_use(shader);
	GLint location = glGetUniformLocation(shader, name);
	glUniform1f(location, value);
}

void shader_set_int(shader_t shader, const char *name, int value) {
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
	glActiveTexture(GL_TEXTURE0);// Set active texture unit to 0
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

void GlTextureLoader::load(spine::AtlasPage &page, const spine::String &path) {
	page.texture = (void *) (uintptr_t) texture_load(path.buffer());
}

void GlTextureLoader::unload(void *texture) {
	texture_dispose((texture_t) (uintptr_t) texture);
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
    )",
									R"(
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
	auto *renderer = (renderer_t *) malloc(sizeof(renderer_t));
	renderer->shader = shader;
	renderer->mesh = mesh;
	renderer->vertex_buffer_size = 0;
	renderer->vertex_buffer = nullptr;
	renderer->renderer = new SkeletonRenderer();
	return renderer;
}

void renderer_set_viewport_size(renderer_t *renderer, int width, int height) {
	float matrix[16];
	matrix_ortho_projection(matrix, (float) width, (float) height);
	shader_use(renderer->shader);
	shader_set_matrix4(renderer->shader, "uMatrix", matrix);
}

void renderer_draw(renderer_t *renderer, Skeleton *skeleton, bool premultipliedAlpha) {
	shader_use(renderer->shader);
	shader_set_int(renderer->shader, "uTexture", 0);
	glEnable(GL_BLEND);

	RenderCommand *command = renderer->renderer->render(*skeleton);
	while (command) {
		int num_command_vertices = command->numVertices;
		if (renderer->vertex_buffer_size < num_command_vertices) {
			renderer->vertex_buffer_size = num_command_vertices;
			free(renderer->vertex_buffer);
			renderer->vertex_buffer = (vertex_t *) malloc(sizeof(vertex_t) * renderer->vertex_buffer_size);
		}
		float *positions = command->positions;
		float *uvs = command->uvs;
		uint32_t *colors = command->colors;
		uint32_t *darkColors = command->darkColors;
		for (int i = 0, j = 0; i < num_command_vertices; i++, j += 2) {
			vertex_t *vertex = &renderer->vertex_buffer[i];
			vertex->x = positions[j];
			vertex->y = positions[j + 1];
			vertex->u = uvs[j];
			vertex->v = uvs[j + 1];
			uint32_t color = colors[i];
			vertex->color = (color & 0xFF00FF00) | ((color & 0x00FF0000) >> 16) | ((color & 0x000000FF) << 16);
			uint32_t darkColor = darkColors[i];
			vertex->darkColor = (darkColor & 0xFF00FF00) | ((darkColor & 0x00FF0000) >> 16) | ((darkColor & 0x000000FF) << 16);
		}
		int num_command_indices = command->numIndices;
		uint16_t *indices = command->indices;
		mesh_update(renderer->mesh, renderer->vertex_buffer, num_command_vertices, indices, num_command_indices);

		blend_mode_t blend_mode = blend_modes[command->blendMode];
		glBlendFuncSeparate(premultipliedAlpha ? (GLenum) blend_mode.source_color_pma : (GLenum) blend_mode.source_color, (GLenum) blend_mode.dest_color, (GLenum) blend_mode.source_alpha, (GLenum) blend_mode.dest_color);

		auto texture = (texture_t) (uintptr_t) command->texture;
		texture_use(texture);

		mesh_draw(renderer->mesh);
		command = command->next;
	}
}

void renderer_dispose(renderer_t *renderer) {
	shader_dispose(renderer->shader);
	mesh_dispose(renderer->mesh);
	free(renderer->vertex_buffer);
	delete renderer->renderer;
	free(renderer);
}