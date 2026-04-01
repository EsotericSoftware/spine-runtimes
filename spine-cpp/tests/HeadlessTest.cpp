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

#include <spine/spine.h>
#include "SkeletonSerializer.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <locale.h>

using namespace spine;

// Provide the default extension implementation
namespace spine {
	SpineExtension *getDefaultExtension() {
		return new DefaultSpineExtension();
	}
}// namespace spine

// Mock texture that doesn't require OpenGL
class MockTexture {
public:
	int width = 1024;
	int height = 1024;
};

// Custom texture loader that doesn't load actual textures
class HeadlessTextureLoader : public TextureLoader {
public:
	virtual void load(AtlasPage &page, const String &path) override {
		// Don't load actual texture, just set dimensions
		page.texture = new MockTexture();
		page.width = 1024;
		page.height = 1024;
	}

	virtual void unload(void *texture) override {
		delete static_cast<MockTexture *>(texture);
	}
};


int main(int argc, char *argv[]) {
	// Set locale to ensure consistent number formatting
	setlocale(LC_ALL, "C");

	if (argc < 3) {
		fprintf(stderr, "Usage: HeadlessTest <skeleton-path> <atlas-path> [animation-name] [animation-name-2]\n");
		return 1;
	}

	Bone::setYDown(false);

	const char *skeletonPath = argv[1];
	const char *atlasPath = argv[2];
	const char *animationName = argc >= 4 ? argv[3] : nullptr;
	const char *animationName2 = argc >= 5 ? argv[4] : nullptr;

	// Load atlas with headless texture loader
	HeadlessTextureLoader textureLoader;
	Atlas *atlas = new Atlas(atlasPath, &textureLoader);

	// Load skeleton data
	SkeletonData *skeletonData = nullptr;

	if (strstr(skeletonPath, ".json") != nullptr) {
		SkeletonJson json(*atlas);
		skeletonData = json.readSkeletonDataFile(skeletonPath);
	} else {
		SkeletonBinary binary(*atlas);
		skeletonData = binary.readSkeletonDataFile(skeletonPath);
	}

	if (!skeletonData) {
		fprintf(stderr, "Failed to load skeleton data\n");
		delete atlas;
		return 1;
	}

	// Create skeleton instance
	Skeleton skeleton(*skeletonData);

	// Set animation if provided
	AnimationState *state = nullptr;
	AnimationStateData *stateData = nullptr;
	if (animationName != nullptr) {
		// Create animation state only when needed
		stateData = new AnimationStateData(*skeletonData);
		state = new AnimationState(*stateData);

		// Find and set animation
		Animation *animation = skeletonData->findAnimation(animationName);
		if (!animation) {
			fprintf(stderr, "Animation not found: %s\n", animationName);
			delete skeletonData;
			delete atlas;
			return 1;
		}
		state->setAnimation(0, *animation, true);
		// Update and apply
		state->update(0.016f);
		state->apply(skeleton);
	}

	skeleton.updateWorldTransform(Physics_Update);

	// Use SkeletonSerializer for JSON output
	SkeletonSerializer serializer;

	// Print skeleton data
	printf("=== SKELETON DATA ===\n");
	printf("%s", serializer.serializeSkeletonData(skeletonData).buffer());

	// Print skeleton state
	printf("\n=== SKELETON STATE ===\n");
	printf("%s", serializer.serializeSkeleton(&skeleton).buffer());

	// Print animation state only if animation was loaded
	if (state != nullptr) {
		printf("\n=== ANIMATION STATE ===\n");
		printf("%s", serializer.serializeAnimationState(state).buffer());
	}

	// Transition test: if a second animation is provided, play A for 10 frames, transition to B,
	// then sample skeleton state at frames 5, 10, 15, 20 during the mix.
	if (state != nullptr && animationName2 != nullptr) {
		Animation *animation2 = skeletonData->findAnimation(animationName2);
		if (!animation2) {
			fprintf(stderr, "Animation not found: %s\n", animationName2);
			delete state;
			delete stateData;
			delete skeletonData;
			delete atlas;
			return 1;
		}

		// Reset skeleton and state
		skeleton.setupPose();
		state->clearTracks();
		state->setAnimation(0, *skeletonData->findAnimation(animationName), true);

		// Run 10 frames of animation A
		for (int i = 0; i < 10; i++) {
			state->update(1 / 60.0f);
			state->apply(skeleton);
			skeleton.updateWorldTransform(Physics_Update);
		}

		// Transition to animation B
		state->setAnimation(0, *animation2, true);

		// Run 20 frames through the mix, serializing at frames 5, 10, 15, 20
		for (int i = 1; i <= 20; i++) {
			state->update(1 / 60.0f);
			state->apply(skeleton);
			skeleton.updateWorldTransform(Physics_Update);
			if (i == 5 || i == 10 || i == 15 || i == 20) {
				SkeletonSerializer transSerializer;
				printf("\n=== TRANSITION FRAME %d ===\n", i);
				printf("%s", transSerializer.serializeSkeleton(&skeleton).buffer());
			}
		}
	}

	// Cleanup
	if (state != nullptr) {
		delete state;
	}
	if (stateData != nullptr) {
		delete stateData;
	}
	delete skeletonData;
	delete atlas;

	return 0;
}