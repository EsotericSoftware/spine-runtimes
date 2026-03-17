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

static GLFWwindow *init_glfw() {
	if (!glfwInit()) {
		std::cerr << "Failed to initialize GLFW" << std::endl;
		return nullptr;
	}
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
	glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
	GLFWwindow *window = glfwCreateWindow(64, 64, "spine-glfw mix-and-match", NULL, NULL);
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

	Bone::setYDown(true);

	const char *atlasPath = "../spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match.atlas";
	const char *skeletonPath = "../spine-ue/Content/GettingStarted/Assets/mix-and-match/mix-and-match-pro.skel";

	{
		GlTextureLoader textureLoader;
		Atlas *atlas = new Atlas(atlasPath, &textureLoader);
		SkeletonBinary binary(*atlas);
		SkeletonData *skeletonData = binary.readSkeletonDataFile(skeletonPath);
		if (!skeletonData) {
			std::cerr << "Failed to load mix-and-match: " << binary.getError().buffer() << std::endl;
			delete atlas;
			glfwDestroyWindow(window);
			glfwTerminate();
			return -1;
		}

		Skeleton skeleton(*skeletonData);
		skeleton.setupPose();
		skeleton.updateWorldTransform(spine::Physics_None);

		std::cout << "Loaded mix-and-match successfully. Bones=" << skeletonData->getBones().size() << ", slots=" << skeletonData->getSlots().size()
				  << ", skins=" << skeletonData->getSkins().size() << std::endl;

		delete skeletonData;
		delete atlas;
	}

	glfwDestroyWindow(window);
	glfwTerminate();
	return 0;
}
