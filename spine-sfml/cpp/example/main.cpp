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
#include <spine/spine-sfml.h>
#include <spine/Debug.h>
#include <SFML/Graphics.hpp>

using namespace std;
using namespace spine;
#include <memory>

template<typename T, typename... Args>
unique_ptr<T> make_unique(Args&&... args) {
	return unique_ptr<T>(new T(forward<Args>(args)...));
}

void callback (AnimationState* state, EventType type, TrackEntry* entry, Event* event) {
	SP_UNUSED(state);
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
			printf("%d event: %s, %s: %d, %f, %s %f %f\n", entry->getTrackIndex(), animationName.buffer(), event->getData().getName().buffer(), event->getIntValue(), event->getFloatValue(),
				   event->getStringValue().buffer(), event->getVolume(), event->getBalance());
			break;
	}
	fflush(stdout);
}

shared_ptr<SkeletonData> readSkeletonJsonData (const String& filename, Atlas* atlas, float scale) {
	SkeletonJson json(atlas);
	json.setScale(scale);
	auto skeletonData = json.readSkeletonDataFile(filename);
	if (!skeletonData) {
		printf("%s\n", json.getError().buffer());
		exit(0);
	}
	return shared_ptr<SkeletonData>(skeletonData);
}

shared_ptr<SkeletonData> readSkeletonBinaryData (const char* filename, Atlas* atlas, float scale) {
	SkeletonBinary binary(atlas);
	binary.setScale(scale);
	auto skeletonData = binary.readSkeletonDataFile(filename);
	if (!skeletonData) {
		printf("%s\n", binary.getError().buffer());
		exit(0);
	}
	return shared_ptr<SkeletonData>(skeletonData);
}

void testcase (void func(SkeletonData* skeletonData, Atlas* atlas),
			   const char* jsonName, const char* binaryName, const char* atlasName,
			   float scale) {
	SFMLTextureLoader textureLoader;
	auto atlas = make_unique<Atlas>(atlasName, &textureLoader);

	auto skeletonData = readSkeletonJsonData(jsonName, atlas.get(), scale);
	func(skeletonData.get(), atlas.get());

	skeletonData = readSkeletonBinaryData(binaryName, atlas.get(), scale);
	func(skeletonData.get(), atlas.get());
}

void spineboy (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonBounds bounds;

	// Configure mixing.
	AnimationStateData stateData(skeletonData);
	stateData.setMix("walk", "jump", 0.2f);
	stateData.setMix("jump", "run", 0.2f);

	SkeletonDrawable drawable(skeletonData, &stateData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;
	skeleton->setToSetupPose();

	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	Slot* headSlot = skeleton->findSlot("head");

	drawable.state->setListener(callback);
	drawable.state->addAnimation(0, "walk", true, 0);
	drawable.state->addAnimation(0, "jump", false, 3);
	drawable.state->addAnimation(0, "run", true, 0);

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
		if (bounds.containsPoint((float)position.x, (float)position.y)) {
			headSlot->getColor().g = 0;
			headSlot->getColor().b = 0;
		} else {
			headSlot->getColor().g = 1;
			headSlot->getColor().b = 1;
		}

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void goblins (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;

	Skin* skin = skeleton->getData()->findSkin("goblingirl");
	Skin copy("test");

	copy.copySkin(skin);

	skeleton->setSkin(&copy);
	skeleton->setSlotsToSetupPose();

	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "walk", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - goblins");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void raptor (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	PowInterpolation pow2(2);
	PowOutInterpolation powOut2(2);
	SwirlVertexEffect effect(400, powOut2);
	effect.setCenterY(-200);
	drawable.vertexEffect = &effect;

	Skeleton* skeleton = drawable.skeleton;
	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "walk", true);
	drawable.state->addAnimation(1, "gun-grab", false, 2);

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
		effect.setAngle(pow2.interpolate(-60.0f, 60.0f, percent));

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void tank (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;
	skeleton->setPosition(500, 590);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "drive", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - tank");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;

	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();
		drawable.update(delta);
		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void vine (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;
	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "grow", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - vine");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void stretchyman (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;

	skeleton->setPosition(100, 590);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "sneak", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - Streatchyman");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void stretchymanStrechyIk (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable* drawable = new SkeletonDrawable(skeletonData);
	drawable->timeScale = 1;
	drawable->setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable->skeleton;

	skeleton->setPosition(100, 590);
	skeleton->updateWorldTransform();

	drawable->state->setAnimation(0, "sneak", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - Streatchyman Stretchy IK");
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
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;
	skeleton->setPosition(320, 320);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "animation", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - coin");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;

	while (window.isOpen()) {
		while (window.pollEvent(event)) {
			if (event.type == sf::Event::Closed) window.close();
		}

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void owl (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;
	skeleton->setPosition(320, 400);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "idle", true);

	TrackEntry* left = drawable.state->setAnimation(1, "left", true);
	TrackEntry* right = drawable.state->setAnimation(2, "right", true);
	TrackEntry* up = drawable.state->setAnimation(3, "up", true);
	TrackEntry* down = drawable.state->setAnimation(4, "down", true);

	left->setAlpha(0);
	left->setMixBlend(MixBlend_Add);
	right->setAlpha(0);
	right->setMixBlend(MixBlend_Add);
	up->setAlpha(0);
	up->setMixBlend(MixBlend_Add);
	down->setAlpha(0);
	down->setMixBlend(MixBlend_Add);

	// drawable.state->setAnimation(5, "blink", true);

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
				right->setAlpha((0.5f - MathUtil::min(x, 0.5f)) * 2);

				float y = event.mouseMove.y / 640.0f;
				down->setAlpha((MathUtil::max(y, 0.5f) - 0.5f) * 2);
				up->setAlpha((0.5f - MathUtil::min(y, 0.5f)) * 2);
			}
		}

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

void mixAndMatch (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	SkeletonDrawable drawable(skeletonData);
	drawable.timeScale = 1;
	drawable.setUsePremultipliedAlpha(true);

	Skeleton* skeleton = drawable.skeleton;

	Skin skin("mix-and-match");
	skin.addSkin(skeletonData->findSkin("skin-base"));
	skin.addSkin(skeletonData->findSkin("nose/short"));
	skin.addSkin(skeletonData->findSkin("eyelids/girly"));
	skin.addSkin(skeletonData->findSkin("eyes/violet"));
	skin.addSkin(skeletonData->findSkin("hair/brown"));
	skin.addSkin(skeletonData->findSkin("clothes/hoodie-orange"));
	skin.addSkin(skeletonData->findSkin("legs/pants-jeans"));
	skin.addSkin(skeletonData->findSkin("accessories/bag"));
	skin.addSkin(skeletonData->findSkin("accessories/hat-red-yellow"));

	skeleton->setSkin(&skin);
	skeleton->setSlotsToSetupPose();

	skeleton->setPosition(320, 590);
	skeleton->updateWorldTransform();

	drawable.state->setAnimation(0, "dance", true);

	sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - goblins");
	window.setFramerateLimit(60);
	sf::Event event;
	sf::Clock deltaClock;
	while (window.isOpen()) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();

		float delta = deltaClock.getElapsedTime().asSeconds();
		deltaClock.restart();

		drawable.update(delta);

		window.clear();
		window.draw(drawable);
		window.display();
	}
}

/**
 * Used for debugging purposes during runtime development
 */
void test (SkeletonData* skeletonData, Atlas* atlas) {
	SP_UNUSED(atlas);

	Skeleton skeleton(skeletonData);
	AnimationStateData animationStateData(skeletonData);
	AnimationState animationState(&animationStateData);
	animationState.setAnimation(0, "idle", true);

	float d = 3;
	for (int i = 0; i < 1; i++) {
		animationState.update(d);
		animationState.apply(skeleton);
		skeleton.updateWorldTransform();
		d += 0.1f;
	}
}

int main () {
	DebugExtension dbgExtension(SpineExtension::getInstance());
	SpineExtension::setInstance(&dbgExtension);

	testcase(mixAndMatch, "data/mix-and-match-pro.json", "data/mix-and-match-pro.skel", "data/mix-and-match-pma.atlas", 0.5f);
	testcase(goblins, "data/goblins-pro.json", "data/goblins-pro.skel", "data/goblins-pma.atlas", 1.4f);
	testcase(owl, "data/owl-pro.json", "data/owl-pro.skel", "data/owl-pma.atlas", 0.5f);
	testcase(spineboy, "data/spineboy-pro.json", "data/spineboy-pro.skel", "data/spineboy-pma.atlas", 0.6f);
	testcase(raptor, "data/raptor-pro.json", "data/raptor-pro.skel", "data/raptor-pma.atlas", 0.5f);
	testcase(coin, "data/coin-pro.json", "data/coin-pro.skel", "data/coin-pma.atlas", 0.5f);
	testcase(vine, "data/vine-pro.json", "data/vine-pro.skel", "data/vine-pma.atlas", 0.5f);
	testcase(tank, "data/tank-pro.json", "data/tank-pro.skel", "data/tank-pma.atlas", 0.2f);
	testcase(raptor, "data/raptor-pro.json", "data/raptor-pro.skel", "data/raptor-pma.atlas", 0.5f);
	testcase(stretchyman, "data/stretchyman-pro.json", "data/stretchyman-pro.skel", "data/stretchyman-pma.atlas", 0.6f);

	dbgExtension.reportLeaks();
	return 0;
}
