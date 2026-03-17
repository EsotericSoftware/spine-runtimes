/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *****************************************************************************/

#include <glbinding/gl/gl.h>
#include <glbinding/glbinding.h>
#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
#include <iostream>
#include <spine-glfw.h>

using namespace spine;

int width = 800, height = 600;
bool isDragging = false;
double lastMouseX = 0, lastMouseY = 0;
spine_skeleton dragSkeleton = nullptr;

void mouse_button_callback(GLFWwindow *window, int button, int action, int mods) {
	SP_UNUSED(mods);
	if (button == GLFW_MOUSE_BUTTON_LEFT) {
		if (action == GLFW_PRESS) {
			isDragging = true;
			glfwGetCursorPos(window, &lastMouseX, &lastMouseY);
		} else if (action == GLFW_RELEASE) {
			isDragging = false;
		}
	}
}

void cursor_position_callback(GLFWwindow *window, double xpos, double ypos) {
	SP_UNUSED(window);
	if (isDragging && dragSkeleton != nullptr) {
		double deltaX = xpos - lastMouseX;
		double deltaY = ypos - lastMouseY;
		spine_skeleton_set_x(dragSkeleton, spine_skeleton_get_x(dragSkeleton) + (float) deltaX);
		spine_skeleton_set_y(dragSkeleton, spine_skeleton_get_y(dragSkeleton) + (float) deltaY);
		lastMouseX = xpos;
		lastMouseY = ypos;
	}
}

GLFWwindow *init_glfw() {
	if (!glfwInit()) {
		std::cerr << "Failed to initialize GLFW" << std::endl;
		return nullptr;
	}
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	GLFWwindow *window = glfwCreateWindow(width, height, "spine-glfw physics c - Drag anywhere", NULL, NULL);
	if (!window) {
		std::cerr << "Failed to create GLFW window" << std::endl;
		glfwTerminate();
		return nullptr;
	}
	glfwMakeContextCurrent(window);
	glbinding::initialize(glfwGetProcAddress);
	glfwSetMouseButtonCallback(window, mouse_button_callback);
	glfwSetCursorPosCallback(window, cursor_position_callback);
	return window;
}

uint8_t *read_file(const char *path, int *length) {
	FILE *file = fopen(path, "rb");
	if (!file) return 0;
	fseek(file, 0, SEEK_END);
	*length = (int) ftell(file);
	fseek(file, 0, SEEK_SET);
	uint8_t *data = (uint8_t *) malloc(*length);
	fread(data, 1, *length, file);
	fclose(file);
	return data;
}

void *load_texture(const char *path) {
	return (void *) (uintptr_t) texture_load(path);
}

void unload_texture(void *texture) {
	texture_dispose((texture_t) (uintptr_t) texture);
}

int main() {
	GLFWwindow *window = init_glfw();
	if (!window) return -1;

	spine_enable_debug_extension(true);

	Bone::setYDown(true);

	{

		int atlas_length = 0;
		uint8_t *atlas_bytes = read_file("data/celestial-circus-pma.atlas", &atlas_length);
		spine_atlas_result atlas_result = spine_atlas_load_callback((const char *) atlas_bytes, "data/", load_texture, unload_texture);
		spine_atlas atlas = spine_atlas_result_get_atlas(atlas_result);
		spine_atlas_result_dispose(atlas_result);

		int skeleton_length = 0;
		uint8_t *skeleton_bytes = read_file("data/celestial-circus-pro.skel", &skeleton_length);
		spine_skeleton_data_result skeleton_result = spine_skeleton_data_load_binary(atlas, skeleton_bytes, skeleton_length,
																					 "data/celestial-circus-pro.skel");
		spine_skeleton_data skeleton_data = spine_skeleton_data_result_get_data(skeleton_result);
		spine_skeleton_data_result_dispose(skeleton_result);

		spine_skeleton_drawable drawable = spine_skeleton_drawable_create(skeleton_data);
		spine_skeleton skeleton = spine_skeleton_drawable_get_skeleton(drawable);
		spine_skeleton_set_scale(skeleton, 0.2f, 0.2f);
		spine_skeleton_setup_pose(skeleton);
		spine_skeleton_update(skeleton, 0);
		spine_skeleton_update_world_transform(skeleton, SPINE_PHYSICS_UPDATE);
		spine_skeleton_set_position(skeleton, width / 2.0f, height / 2.0f + 150.0f);
		dragSkeleton = skeleton;

		spine_animation_state animation_state = spine_skeleton_drawable_get_animation_state(drawable);
		spine_animation_state_set_animation_1(animation_state, 0, "eyeblink-long", true);
		spine_animation_state_set_animation_1(animation_state, 1, "wind-idle", true);

		renderer_t *renderer = renderer_create();
		renderer_set_viewport_size(renderer, width, height);

		double lastTime = glfwGetTime();
		while (!glfwWindowShouldClose(window)) {
			double currTime = glfwGetTime();
			float delta = (float) (currTime - lastTime);
			lastTime = currTime;
			spine_animation_state_update(animation_state, delta);
			spine_animation_state_apply(animation_state, skeleton);
			spine_skeleton_update(skeleton, delta);
			spine_skeleton_update_world_transform(skeleton, SPINE_PHYSICS_UPDATE);
			gl::glClear(gl::GL_COLOR_BUFFER_BIT);
			renderer_draw_c(renderer, skeleton, true);
			glfwSwapBuffers(window);
			glfwPollEvents();
		}

		dragSkeleton = nullptr;
		renderer_dispose(renderer);
		spine_skeleton_drawable_dispose(drawable);
		spine_skeleton_data_dispose(skeleton_data);
		spine_atlas_dispose(atlas);
		free(atlas_bytes);
		free(skeleton_bytes);
	}

	spine_report_leaks();
	glfwTerminate();
	return 0;
}
