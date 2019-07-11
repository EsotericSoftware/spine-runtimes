/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated May 1, 2019. Replaces all prior versions.
 *
 * Copyright (c) 2013-2019, Esoteric Software LLC
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
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
 * INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <iostream>
#include <string.h>
#define SPINE_SHORT_NAMES
#include <spine/spine-sfml.h>
#include <SFML/Graphics.hpp>
#include <SFML/Window/Mouse.hpp>

using namespace std;
using namespace spine;
#include <stdio.h>
#include <stdlib.h>

void callback (AnimationState* state, EventType type, TrackEntry* entry, Event* event) {
	UNUSED(state);
	const char* animationName = (entry && entry->animation) ? entry->animation->name : 0;

	switch (type) {
	case ANIMATION_START:
		printf("%d start: %s\n", entry->trackIndex, animationName);
		break;
	case ANIMATION_INTERRUPT:
		printf("%d interrupt: %s\n", entry->trackIndex, animationName);
		break;
	case ANIMATION_END:
		printf("%d end: %s\n", entry->trackIndex, animationName);
		break;
	case ANIMATION_COMPLETE:
		printf("%d complete: %s\n", entry->trackIndex, animationName);
		break;
	case ANIMATION_DISPOSE:
		printf("%d dispose: %s\n", entry->trackIndex, animationName);
		break;
	case ANIMATION_EVENT:
		printf("%d event: %s, %s: %d, %f, %s %f %f\n", entry->trackIndex, animationName, event->data->name, event->intValue, event->floatValue,
				event->stringValue, event->volume, event->balance);
		break;
	}
	fflush(stdout);
}

SkeletonData* readSkeletonJsonData (const char* filename, Atlas* atlas, float scale) {
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = scale;
	SkeletonData* skeletonData = SkeletonJson_readSkeletonDataFile(json, filename);
	if (!skeletonData) {
		printf("%s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);
	return skeletonData;
}

SkeletonData* readSkeletonBinaryData (const char* filename, Atlas* atlas, float scale) {
	SkeletonBinary* binary = SkeletonBinary_create(atlas);
	binary->scale = scale;
	SkeletonData *skeletonData = SkeletonBinary_readSkeletonDataFile(binary, filename);
	if (!skeletonData) {
		printf("%s\n", binary->error);
		exit(0);
	}
	SkeletonBinary_dispose(binary);
	return skeletonData;
}

void testcase (void func(SkeletonData* skeletonData, Atlas* atlas),
		const char* jsonName, const char* binaryName, const char* atlasName,
		float scale) {
	Atlas* atlas = Atlas_createFromFile(atlasName, 0);

	SkeletonData* skeletonData = readSkeletonJsonData(jsonName, atlas, scale);
	func(skeletonData, atlas);
	SkeletonData_dispose(skeletonData);

	skeletonData = readSkeletonBinaryData(binaryName, atlas, scale);
	func(skeletonData, atlas);
	SkeletonData_dispose(skeletonData);

	Atlas_dispose(atlas);
}

void spineboy (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonBounds* bounds = SkeletonBounds_create();

	// Configure mixing.
	AnimationStateData* stateData = AnimationStateData_create(skeletonData);
	AnimationStateData_setMixByName(stateData, "walk", "jump", 0.2f);
	AnimationStateData_setMixByName(stateData, "jump", "run", 0.2f);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData, stateData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	Skeleton_setToSetupPose(skeleton);

	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	Slot* headSlot = Skeleton_findSlot(skeleton, "head");

	drawable->state->listener = callback;
	AnimationState_addAnimationByName(drawable->state, 0, "walk", true, 0);
	AnimationState_addAnimationByName(drawable->state, 0, "jump", false, 3);
	AnimationState_addAnimationByName(drawable->state, 0, "run", true, 0);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - spineboy");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		SkeletonBounds_update(bounds, skeleton, true);
		sf::Vector2i position = sf::Mouse::getPosition(window);
		if (SkeletonBounds_containsPoint(bounds, (float)position.x, (float)position.y)) {
			headSlot->color.g = 0;
			headSlot->color.b = 0;
		} else {
			headSlot->color.g = 1;
			headSlot->color.b = 1;
		}

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}

	SkeletonBounds_dispose(bounds);
}

void goblins (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	Skeleton_setSkinByName(skeleton, "goblin");
	Skeleton_setSlotsToSetupPose(skeleton);
	//Skeleton_setAttachment(skeleton, "left hand item", "dagger");

	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "walk", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - goblins");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

void raptor (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	spSwirlVertexEffect* effect = spSwirlVertexEffect_create(400);
	effect->centerY = -200;
	drawable->vertexEffect = &effect->super;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "walk", true);
	AnimationState_addAnimationByName(drawable->state, 1, "gun-grab", false, 2);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - raptor");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	float swirlTime = 0;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		swirlTime += delta;
		float percent = (float)fmod(swirlTime, 2);
		if (percent > 1) percent = 1 - (percent - 1);
		effect->angle = _spMath_interpolate(_spMath_pow2_apply, -60, 60, percent);

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
	spSwirlVertexEffect_dispose(effect);
}

void tank (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 500;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "drive", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - tank");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;

	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();
		drawable->update(delta);
		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

void vine (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "grow", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - vine");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

void stretchyman (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 100;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "sneak", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - Streatchyman");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

void coin (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 320;
	skeleton->y = 320;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "animation", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - vine");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;

	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

void owl (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 320;
	skeleton->y = 400;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "idle", true);
	AnimationState_setAnimationByName(drawable->state, 1, "blink", true);
	spTrackEntry* left = AnimationState_setAnimationByName(drawable->state, 2, "left", true);
	spTrackEntry* right = AnimationState_setAnimationByName(drawable->state, 3, "right", true);
	spTrackEntry* up = AnimationState_setAnimationByName(drawable->state, 4, "up", true);
	spTrackEntry* down = AnimationState_setAnimationByName(drawable->state, 5, "down", true);

	left->alpha = 0;
	right->alpha = 0;
	right->mixBlend = SP_MIX_BLEND_ADD;
	up->alpha = 0;
	up->mixBlend = SP_MIX_BLEND_ADD;
	down->alpha = 0;
	down->mixBlend = SP_MIX_BLEND_ADD;

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - owl");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event)) {
			if (event.type == sf::Event::Closed) window.close();
			if (event.type == sf::Event::MouseMoved) {
				float x = event.mouseMove.x / 640.0f;
				left->alpha = (MAX(x, 0.5f) - 0.5f) * 2;
				right->alpha = (0.5f - MIN(x, 0.5f)) * 2;

				float y = event.mouseMove.y / 640.0f;
				down->alpha = (MAX(y, 0.5f) - 0.5f) * 2;
				up->alpha = (0.5f - MIN(y, 0.5f)) * 2;
			}
		}

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

/**
 * Used for debugging purposes during runtime development
 */
void test (SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	spSkeleton* skeleton = Skeleton_create(skeletonData);
	spAnimationStateData* animData = spAnimationStateData_create(skeletonData);
	spAnimationState* animState = spAnimationState_create(animData);
	spAnimationState_setAnimationByName(animState, 0, "drive", true);


	float d = 3;
	for (int i = 0; i < 1; i++) {
		spSkeleton_update(skeleton, d);
		spAnimationState_update(animState, d);
		spAnimationState_apply(animState, skeleton);
		spSkeleton_updateWorldTransform(skeleton);
		for (int ii = 0; ii < skeleton->bonesCount; ii++) {
			spBone* bone = skeleton->bones[ii];
			printf("%s %f %f %f %f %f %f\n", bone->data->name, bone->a, bone->b, bone->c, bone->d, bone->worldX, bone->worldY);
		}
		printf("========================================\n");
		d += 0.1f;
	}

	Skeleton_dispose(skeleton);
}

void testSkinsApi(SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;

	spSkin* skin = spSkin_create("test-skin");
	spSkin_copySkin(skin, spSkeletonData_findSkin(skeletonData, "goblingirl"));
	// spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "goblingirl"));
	spSkeleton_setSkin(skeleton, skin);
	spSkeleton_setSlotsToSetupPose(skeleton);

	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "walk", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - skins api");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}

	spSkin_clear(skin);
	spSkin_dispose(skin);
}

void testMixAndMatch(SkeletonData* skeletonData, Atlas* atlas) {
	UNUSED(atlas);
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;

	// Create a new skin, by mixing and matching other skins
	// that fit together. Items making up the girl are individual
	// skins. Using the skin API, a new skin is created which is
	// a combination of all these individual item skins.
	spSkin* skin = spSkin_create("mix-and-match");
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "skin-base"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "nose/short"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "eyelids/girly"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "eyes/violet"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "hair/brown"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "clothes/hoodie-orange"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "legs/pants-jeans"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "accessories/bag"));
	spSkin_addSkin(skin, spSkeletonData_findSkin(skeletonData, "accessories/hat-red-yellow"));
	spSkeleton_setSkin(skeleton, skin);
	spSkeleton_setSlotsToSetupPose(skeleton);

	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "dance", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - mix and match");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}

	spSkin_clear(skin);
	spSkin_dispose(skin);
}

int main () {
	testcase(testMixAndMatch, "data/mix-and-match-pro.json", "data/mix-and-match-pro.skel", "data/mix-and-match-pma.atlas", 0.5f);
	testcase(goblins, "data/goblins-pro.json", "data/goblins-pro.skel", "data/goblins-pma.atlas", 1.4f);
	testcase(test, "data/tank-pro.json", "data/tank-pro.skel", "data/tank-pma.atlas", 1.0f);
	testcase(spineboy, "data/spineboy-pro.json", "data/spineboy-pro.skel", "data/spineboy-pma.atlas", 0.6f);
	testcase(stretchyman, "data/stretchyman-stretchy-ik-pro.json", "data/stretchyman-stretchy-ik-pro.skel", "data/stretchyman-pma.atlas", 0.6f);
	testcase(owl, "data/owl-pro.json", "data/owl-pro.skel", "data/owl-pma.atlas", 0.5f);
	testcase(coin, "data/coin-pro.json", "data/coin-pro.skel", "data/coin-pma.atlas", 0.5f);
	testcase(vine, "data/vine-pro.json", "data/vine-pro.skel", "data/vine-pma.atlas", 0.5f);
	testcase(tank, "data/tank-pro.json", "data/tank-pro.skel", "data/tank-pma.atlas", 0.2f);
	testcase(raptor, "data/raptor-pro.json", "data/raptor-pro.skel", "data/raptor-pma.atlas", 0.5f);
	testcase(stretchyman, "data/stretchyman-pro.json", "data/stretchyman-pro.skel", "data/stretchyman-pma.atlas", 0.6f);
	// testcase(testSkinsApi, "data/goblins-pro.json", "data/goblins-pro.skel", "data/goblins-pma.atlas", 1.4f);
	return 0;
}
