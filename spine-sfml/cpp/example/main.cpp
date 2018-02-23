/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <iostream>
#include <string.h>
#include <spine/spine-sfml.h>
#include <spine/Debug.h>
#include <SFML/Graphics.hpp>
#include <SFML/Window/Mouse.hpp>

using namespace std;
using namespace Spine;
#include <stdio.h>
#include <stdlib.h>

void callback (AnimationState* state, EventType type, TrackEntry* entry, Event* event) {
	const String& animationName = (entry && entry->getAnimation()) ? entry->getAnimation()->getName() : String("");

	switch (type) {
	case EventType_Start:
		printf("%d start: %s\n", entry->getTrackIndex(), animationName.buffer());
		break;
	case EventType_Interrupt:
		printf("%d interrupt: %s\n", entry->getTrackIndex(), animationName.buffer());
		break;
	case EventType_End:
		printf("%d end: %s\n", entry->getTrackIndex(), animationName.buffer());
		break;
	case EventType_Complete:
		printf("%d complete: %s\n", entry->getTrackIndex(), animationName.buffer());
		break;
	case EventType_Dispose:
		printf("%d dispose: %s\n", entry->getTrackIndex(), animationName.buffer());
		break;
	case EventType_Event:
		printf("%d event: %s, %s: %d, %f, %s\n", entry->getTrackIndex(), animationName.buffer(), event->getData().getName().buffer(), event->getIntValue(), event->getFloatValue(),
				event->getStringValue().buffer());
		break;
	}
	fflush(stdout);
}

SkeletonData* readSkeletonJsonData (const String& filename, Atlas* atlas, float scale) {
	SkeletonJson* json = new (__FILE__, __LINE__) SkeletonJson(atlas);
	json->setScale(scale);
	SkeletonData* skeletonData = json->readSkeletonDataFile(filename);
	if (!skeletonData) {
		printf("%s\n", json->getError().buffer());
		exit(0);
	}
	delete json;
	return skeletonData;
}

SkeletonData* readSkeletonBinaryData (const char* filename, Atlas* atlas, float scale) {
	SkeletonBinary* binary = new (__FILE__, __LINE__) SkeletonBinary(atlas);
	binary->setScale(scale);
	SkeletonData *skeletonData = binary->readSkeletonDataFile(filename);
	if (!skeletonData) {
		printf("%s\n", binary->getError().buffer());
		exit(0);
	}
	delete binary;
	return skeletonData;
}

void testcase (void func(SkeletonData* skeletonData, Atlas* atlas),
		const char* jsonName, const char* binaryName, const char* atlasName,
		float scale) {
	SFMLTextureLoader textureLoader;
	Atlas* atlas = new (__FILE__, __LINE__) Atlas(atlasName, &textureLoader);

	SkeletonData* skeletonData = readSkeletonJsonData(jsonName, atlas, scale);
	func(skeletonData, atlas);
	delete skeletonData;

	skeletonData = readSkeletonBinaryData(binaryName, atlas, scale);
	func(skeletonData, atlas);
	delete skeletonData;

	delete atlas;
}

void spineboy (SkeletonData* skeletonData, Atlas* atlas) {
	SkeletonBounds bounds;

	// Configure mixing.
	AnimationStateData stateData(skeletonData);
	stateData.setMix("walk", "jump", 0.2f);
	stateData.setMix("jump", "run", 0.2f);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData, &stateData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setFlipX(false);
	skeleton->setFlipY(false);
	skeleton->setToSetupPose();

	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	Slot* headSlot = skeleton->findSlot("head");

	drawable->state->setOnAnimationEventFunc(callback);
	drawable->state->addAnimation(0, "walk", true, 0);
	drawable->state->addAnimation(0, "jump", false, 3);
	drawable->state->addAnimation(0, "run", true, 0);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - spineboy");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		bounds.update(*skeleton, true);
		sf::Vector2i position = sf::Mouse::getPosition(window);
		if (bounds.containsPoint(position.x, position.y)) {
			headSlot->getColor()._g = 0;
			headSlot->getColor()._b = 0;
		} else {
			headSlot->getColor()._g = 1;
			headSlot->getColor()._b = 1;
		}

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}

	delete drawable;
}

void goblins (SkeletonData* skeletonData, Atlas* atlas) {
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setFlipX(false);
	skeleton->setFlipY(false);
	skeleton->setSkin("goblin");
	skeleton->setSlotsToSetupPose();

	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "walk", true);

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
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	// BOZO spSwirlVertexEffect* effect = spSwirlVertexEffect_create(400);
	// effect->centerY = -200;
	// drawable->vertexEffect = &effect->super;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "walk", true);
	drawable->state->addAnimation(1, "gun-grab", false, 2);

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
		float percent = MathUtil::fmod(swirlTime, 2);
		if (percent > 1) percent = 1 - (percent - 1);
		// BOZO effect->angle = _spMath_interpolate(_spMath_pow2_apply, -60, 60, percent);

		drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
	// BOZO spSwirlVertexEffect_dispose(effect);
}

void tank (SkeletonData* skeletonData, Atlas* atlas) {
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setPosition(500, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "drive", true);

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
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "grow", true);

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
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setFlipX(false);
	skeleton->setFlipY(false);

	skeleton->setPosition(100, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "sneak", true);

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

	delete drawable;
}

void coin (SkeletonData* skeletonData, Atlas* atlas) {
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "rotate", true);
	drawable->update(0.1);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - vine");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	float swirlTime = 0;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		// drawable->update(delta);

		window.clear();
		window.draw(*drawable);
		window.display();
	}
}

void owl (SkeletonData* skeletonData, Atlas* atlas) {
	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;

	Skeleton* skeleton = drawable->skeleton;
	skeleton->setPosition(320, 400);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "idle", true);
	drawable->state->setAnimation(1, "blink", true);
	TrackEntry* left = drawable->state->setAnimation(2, "left", true);
	TrackEntry* right = drawable->state->setAnimation(3, "right", true);
	TrackEntry* up = drawable->state->setAnimation(4, "up", true);
	TrackEntry* down = drawable->state->setAnimation(5, "down", true);

	left->setAlpha(0);
	// BOZO left->setMixBlend(SP_MIX_BLEND_ADD);
	right->setAlpha(0);
	// BOZO right->mixBlend = SP_MIX_BLEND_ADD;
	up->setAlpha(0);
	// BOZO up->mixBlend = SP_MIX_BLEND_ADD;
	down->setAlpha(0);
	// BOZO down->mixBlend = SP_MIX_BLEND_ADD;

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - owl");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event)) {
			if (event.type == sf::Event::Closed) window.close();
			if (event.type == sf::Event::MouseMoved) {
				float x = event.mouseMove.x / 640.0f;
				left->setAlpha((MathUtil::max(x, 0.5f) - 0.5f) * 2);
				right->setAlpha((0.5 - MathUtil::min(x, 0.5f)) * 2);

				float y = event.mouseMove.y / 640.0f;
				down->setAlpha((MathUtil::max(y, 0.5f) - 0.5f) * 2);
				up->setAlpha((0.5 - MathUtil::min(y, 0.5f)) * 2);
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
	Skeleton* skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);
	AnimationStateData* animData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
	AnimationState* animState = new (__FILE__, __LINE__) AnimationState(animData);
	animState->setAnimation(0, "drive", true);

	float d = 3;
	for (int i = 0; i < 1; i++) {
		animState->update(d);
		animState->apply(*skeleton);
		skeleton->updateWorldTransform();
		printf("%s\n", skeleton->toString().buffer());
		printf("========================================\n");
		d += 0.1f;
	}

	delete skeleton;
	delete animData;
	delete animState;
}

int main () {
	DebugExtension dbgExtension;
	// SpineExtension::setInstance(&dbgExtension);
	testcase(test, "data/tank-pro.json", "data/tank-pro.skel", "data/tank.atlas", 1.0f);
	testcase(coin, "data/coin-pro.json", "data/coin-pro.skel", "data/coin.atlas", 0.5f);
	testcase(spineboy, "data/spineboy-ess.json", "data/spineboy-ess.skel", "data/spineboy.atlas", 0.6f);
	testcase(owl, "data/owl-pro.json", "data/owl-pro.skel", "data/owl.atlas", 0.5f);
	testcase(coin, "data/coin-pro.json", "data/coin-pro.skel", "data/coin.atlas", 0.5f);
	testcase(vine, "data/vine-pro.json", "data/vine-pro.skel", "data/vine.atlas", 0.5f);
	testcase(tank, "data/tank-pro.json", "data/tank-pro.skel", "data/tank.atlas", 0.2f);
	testcase(raptor, "data/raptor-pro.json", "data/raptor-pro.skel", "data/raptor.atlas", 0.5f);
	testcase(goblins, "data/goblins-pro.json", "data/goblins-pro.skel", "data/goblins.atlas", 1.4f);
	testcase(stretchyman, "data/stretchyman-pro.json", "data/stretchyman-pro.skel", "data/stretchyman.atlas", 0.6f);
	dbgExtension.reportLeaks();
	return 0;
}
