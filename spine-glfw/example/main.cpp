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

int main() {
	// Initialize GLFW and glbinding
	GLFWwindow *window = init_glfw();
	if (!window) return -1;

	// We use a y-down coordinate system, see renderer_set_viewport_size()
	Bone::setYDown(true);

	// Load the atlas and the skeleton data
	GlTextureLoader textureLoader;
	Atlas *atlas = new Atlas("data/spineboy-pma.atlas", &textureLoader);
	SkeletonJson json(atlas);
	SkeletonData *skeletonData = json.readSkeletonDataFile("data/spineboy-pro.json");

	// Create a skeleton from the data, set the skeleton's position to the bottom center of
	// the screen and scale it to make it smaller.
	Skeleton skeleton(skeletonData);
	skeleton.setPosition(width / 2, height - 100);
	skeleton.setScaleX(0.3);
	skeleton.setScaleY(0.3);

	// Create an AnimationState to drive animations on the skeleton. Set the "portal" animation
	// on track with index 0.
	AnimationStateData animationStateData(skeletonData);
	AnimationState animationState(&animationStateData);
	animationState.setAnimation(0, "portal", true);

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
		animationState.update(delta);
		animationState.apply(skeleton);

		// Update the skeleton time (used for physics)
		skeleton.update(delta);

		// Calculate the new pose
		skeleton.updateWorldTransform(spine::Physics_Update);

		// Clear the screen
		gl::glClear(gl::GL_COLOR_BUFFER_BIT);

		// Render the skeleton in its current pose
		renderer_draw(renderer, &skeleton, true);

		// Present the rendering results and poll for events
		glfwSwapBuffers(window);
		glfwPollEvents();
	}

	// Dispose everything
	renderer_dispose(renderer);
	delete skeletonData;
	delete atlas;

	// Kill the window and GLFW
	glfwTerminate();
	return 0;
}
