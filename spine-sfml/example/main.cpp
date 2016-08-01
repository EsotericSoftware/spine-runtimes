/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <iostream>
#include <string.h>
#include <spine/spine-sfml.h>
#include <SFML/Graphics.hpp>
#include <SFML/Window/Mouse.hpp>

using namespace std;
using namespace spine;
#include <stdio.h>
#include <stdlib.h>

void callback (AnimationState* state, int trackIndex, EventType type, Event* event, int loopCount) {
	TrackEntry* entry = AnimationState_getCurrent(state, trackIndex);
	const char* animationName = (entry && entry->animation) ? entry->animation->name : 0;

	switch (type) {
	case ANIMATION_START:
		printf("%d start: %s\n", trackIndex, animationName);
		break;
	case ANIMATION_END:
		printf("%d end: %s\n", trackIndex, animationName);
		break;
	case ANIMATION_COMPLETE:
		printf("%d complete: %s, %d\n", trackIndex, animationName, loopCount);
		break;
	case ANIMATION_EVENT:
		printf("%d event: %s, %s: %d, %f, %s\n", trackIndex, animationName, event->data->name, event->intValue, event->floatValue,
				event->stringValue);
		break;
	}
	fflush(stdout);
}

void spineboy () {
	// Load atlas, skeleton, and animations.
	Atlas* atlas = Atlas_createFromFile("data/spineboy.atlas", 0);
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.6f;
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/spineboy.json");
	if (!skeletonData) {
		printf("%s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);
	SkeletonBounds* bounds = SkeletonBounds_create();

	// Configure mixing.
	AnimationStateData* stateData = AnimationStateData_create(skeletonData);
	AnimationStateData_setMixByName(stateData, "walk", "jump", 0.2f);
	AnimationStateData_setMixByName(stateData, "jump", "run", 0.2f);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData, stateData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->flipX = false;
	skeleton->flipY = false;
	Skeleton_setToSetupPose(skeleton);

	skeleton->x = 320;
	skeleton->y = 460;
	Skeleton_updateWorldTransform(skeleton);

	Slot* headSlot = Skeleton_findSlot(skeleton, "head");

	drawable->state->listener = callback;
	AnimationState_setAnimationByName(drawable->state, 0, "test", true);
	AnimationState_addAnimationByName(drawable->state, 0, "walk", true, 0);
	AnimationState_addAnimationByName(drawable->state, 0, "jump", false, 3);
	AnimationState_addAnimationByName(drawable->state, 0, "run", true, 0);

	sf::RenderWindow window(sf::VideoMode(640, 480), "Spine SFML - spineboy");
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
		if (SkeletonBounds_containsPoint(bounds, position.x, position.y)) {
			headSlot->g = 0;
			headSlot->b = 0;
		} else {
			headSlot->g = 1;
			headSlot->b = 1;
		}

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}

	SkeletonData_dispose(skeletonData);
	SkeletonBounds_dispose(bounds);
	Atlas_dispose(atlas);
}

void goblins () {
	// Load atlas, skeleton, and animations.
	Atlas* atlas = Atlas_createFromFile("data/goblins-mesh.atlas", 0);
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 1.4f;
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/goblins-mesh.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->flipX = false;
	skeleton->flipY = false;
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

	SkeletonData_dispose(skeletonData);
	Atlas_dispose(atlas);
}

void raptor () {
	// Load atlas, skeleton, and animations.
	Atlas* atlas = Atlas_createFromFile("data/raptor.atlas", 0);
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.5f;
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/raptor.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 320;
	skeleton->y = 590;
	Skeleton_updateWorldTransform(skeleton);

	AnimationState_setAnimationByName(drawable->state, 0, "walk", true);
	AnimationState_addAnimationByName(drawable->state, 1, "gungrab", false, 2);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - raptor");
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

	SkeletonData_dispose(skeletonData);
	Atlas_dispose(atlas);
}

void tank () {
	// Load atlas, skeleton, and animations.
	Atlas* atlas = Atlas_createFromFile("data/tank.atlas", 0);
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.2f;
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/tank.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

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

	SkeletonData_dispose(skeletonData);
	Atlas_dispose(atlas);
}

void vine () {
	// Load atlas, skeleton, and animations.
	Atlas* atlas = Atlas_createFromFile("data/vine.atlas", 0);
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 0.5f;
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/vine.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->x = 320;
	skeleton->y = 590;
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

	SkeletonData_dispose(skeletonData);
	Atlas_dispose(atlas);
}

/**
 * Used for debugging purposes during runtime development
 */
void test () {
	// Load atlas, skeleton, and animations.
	Atlas* atlas = Atlas_createFromFile("data/tank.atlas", 0);
	SkeletonJson* json = SkeletonJson_create(atlas);
	json->scale = 1;
	SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "data/tank.json");
	if (!skeletonData) {
		printf("Error: %s\n", json->error);
		exit(0);
	}
	SkeletonJson_dispose(json);

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

	SkeletonData_dispose(skeletonData);
	Skeleton_dispose(skeleton);
	Atlas_dispose(atlas);
}

int main () {
	test();
	vine();
	tank();
	raptor();
	spineboy();
	goblins();
}
