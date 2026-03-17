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
	GLFWwindow *window = glfwCreateWindow(width, height, "spine-glfw dragon", NULL, NULL);
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
	GLFWwindow *window = init_glfw();
	if (!window) return -1;

	spine_enable_debug_extension(true);

	Bone::setYDown(true);

	{

		GlTextureLoader textureLoader;
		Atlas *atlas = new Atlas("data/dragon-pma.atlas", &textureLoader);

		SkeletonBinary binary(*atlas);
		SkeletonData *skeletonData = binary.readSkeletonDataFile("data/dragon-ess.skel");
		if (!skeletonData) {
			std::cerr << "Failed to load dragon: " << binary.getError().buffer() << std::endl;
			return -1;
		}

		Skeleton skeleton(*skeletonData);
		skeleton.setPosition(width / 2, height / 2);
		skeleton.setScaleX(0.5);
		skeleton.setScaleY(0.5);
		skeleton.setupPose();

		AnimationStateData animationStateData(*skeletonData);
		AnimationState animationState(animationStateData);
		animationState.setAnimation(0, "flying", true);

		renderer_t *renderer = renderer_create();
		renderer_set_viewport_size(renderer, width, height);

		double lastTime = glfwGetTime();
		while (!glfwWindowShouldClose(window)) {
			double currTime = glfwGetTime();
			float delta = currTime - lastTime;
			lastTime = currTime;

			animationState.update(delta);
			animationState.apply(skeleton);
			skeleton.update(delta);
			skeleton.updateWorldTransform(spine::Physics_Update);

			gl::glClear(gl::GL_COLOR_BUFFER_BIT);
			renderer_draw(renderer, &skeleton, true);

			glfwSwapBuffers(window);
			glfwPollEvents();
		}

		renderer_dispose(renderer);
		delete skeletonData;
		delete atlas;
	}

	spine_report_leaks();
	glfwTerminate();
	return 0;
}
