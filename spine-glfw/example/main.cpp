#include <glbinding/glbinding.h>
#include <glbinding/gl/gl.h>
#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
#include <iostream>
#include <spine-glfw.h>

int width = 800, height = 600;

GLFWwindow* init_glfw() {
    if (!glfwInit()) {
        std::cerr << "Failed to initialize GLFW" << std::endl;
        return nullptr;
    }
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
    GLFWwindow* window = glfwCreateWindow(width, height, "spine-glfw", NULL, NULL);
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
    spine_bone_set_is_y_down(true);

    // Load the atlas and the skeleton data
    atlas_t *atlas = atlas_load("data/spineboy-pma.atlas");
    spine_skeleton_data skeleton_data = skeleton_data_load("data/spineboy-pro.json", atlas);

    // Create a skeleton drawable from the data, set the skeleton's position to the bottom center of
    // the screen and scale it to make it smaller.
    spine_skeleton_drawable drawable = spine_skeleton_drawable_create(skeleton_data);
    spine_skeleton skeleton = spine_skeleton_drawable_get_skeleton(drawable);
    spine_skeleton_set_position(skeleton, width / 2, height - 100);
    spine_skeleton_set_scale(skeleton, 0.3, 0.3);

    // Set the "portal" animation on track 0 of the animation state, looping
    spine_animation_state animation_state = spine_skeleton_drawable_get_animation_state(drawable);
    spine_animation_state_set_animation_by_name(animation_state, 0, "portal", true);

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
        renderer_draw(renderer, drawable, atlas);

        // Present the rendering results and poll for events
        glfwSwapBuffers(window);
        glfwPollEvents();
    }

    // Dispose everything
    renderer_dispose(renderer);
    spine_skeleton_drawable_dispose(drawable);
    spine_skeleton_data_dispose(skeleton_data);
    atlas_dispose(atlas);

    // Kill the window and GLFW
    glfwTerminate();
    return 0;
}
