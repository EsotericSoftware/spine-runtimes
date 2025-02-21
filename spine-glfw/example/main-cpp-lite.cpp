#include <glbinding/glbinding.h>
#include <glbinding/gl/gl.h>
#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
#include <iostream>
#include <spine-glfw.h>

using namespace spine;

int width = 800, height = 600;

GLFWwindow *init_glfw() {
	if (!glfwInit()) {
		std::cerr << "Failed to initialize GLFW" << std::endl;
		return nullptr;
	}
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	GLFWwindow *window = glfwCreateWindow(width, height, "spine-glfw", NULL, NULL);
	if (!window) {
		std::cerr << "Failed to create GLFW window" << std::endl;
		glfwTerminate();
		return nullptr;
	}
	glfwMakeContextCurrent(window);
	glbinding::initialize(glfwGetProcAddress);
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
	// Initialize GLFW and glbinding
	GLFWwindow *window = init_glfw();
	if (!window) return -1;

	// We use a y-down coordinate system, see renderer_set_viewport_size()
	Bone::setYDown(true);

	// Load the atlas and the skeleton data
	int atlas_length = 0;
	uint8_t *atlas_bytes = read_file("data/spineboy-pma.atlas", &atlas_length);
	spine_atlas atlas = spine_atlas_load_callback((utf8 *) atlas_bytes, "data/", load_texture, unload_texture);
	int skeleton_length = 0;
	uint8_t *skeleton_bytes = read_file("data/spineboy-pro.skel", &skeleton_length);
	spine_skeleton_data_result result = spine_skeleton_data_load_binary(atlas, skeleton_bytes, skeleton_length);
	spine_skeleton_data skeleton_data = spine_skeleton_data_result_get_data(result);

	// Create a skeleton from the data, set the skeleton's position to the bottom center of
	// the screen and scale it to make it smaller.
	spine_skeleton_drawable drawable = spine_skeleton_drawable_create(skeleton_data);
	spine_skeleton skeleton = spine_skeleton_drawable_get_skeleton(drawable);
	spine_skeleton_set_position(skeleton, width / 2, height - 100);
	spine_skeleton_set_scale(skeleton, 0.3f, 0.3f);

	// Create an AnimationState to drive animations on the skeleton. Set the "portal" animation
	// on track with index 0.
	spine_animation_state animation_state = spine_skeleton_drawable_get_animation_state(drawable);
	spine_animation_state_data animation_state_data = spine_animation_state_get_data(animation_state);
	spine_animation_state_data_set_default_mix(animation_state_data, 0.2f);
	spine_animation_state_set_animation_by_name(animation_state, 0, "portal", true);
	spine_animation_state_add_animation_by_name(animation_state, 0, "run", true, 0);

	// Create the renderer and set the viewport size to match the window size. This sets up a
	// pixel perfect orthogonal projection for 2D rendering.
	renderer_t *renderer = renderer_create();
	renderer_set_viewport_size(renderer, width, height);

	// Rendering loop
	double lastTime = glfwGetTime();
	while (!glfwWindowShouldClose(window)) {
		// Calculate the delta time in seconds
		double currTime = glfwGetTime();
		float delta = currTime - lastTime;
		lastTime = currTime;

		// Update and apply the animation state to the skeleton
		spine_animation_state_update(animation_state, delta);
		spine_animation_state_apply(animation_state, skeleton);

		// Update the skeleton time (used for physics)
		spine_skeleton_update(skeleton, delta);

		// Calculate the new pose
		spine_skeleton_update_world_transform(skeleton, SPINE_PHYSICS_UPDATE);

		// Clear the screen
		gl::glClear(gl::GL_COLOR_BUFFER_BIT);

		// Render the skeleton in its current pose
		renderer_draw_lite(renderer, skeleton, true);

		// Present the rendering results and poll for events
		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	// Dispose everything
	renderer_dispose(renderer);
	// delete skeletonData;
	delete atlas;

	// Kill the window and GLFW
	glfwTerminate();
	return 0;
}
