/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <iostream>
#include <spine/spine-sfml.h>
#include <SFML/Graphics.hpp>

using namespace std;
using namespace spine;

int main () {

	try {
		Atlas* atlas = Atlas_readAtlasFile("../data/spineboy.atlas");
		SkeletonJson* json = SkeletonJson_create(atlas);
		SkeletonData *skeletonData = SkeletonJson_readSkeletonDataFile(json, "../data/spineboy-skeleton.json");
		Animation* animation = SkeletonJson_readAnimationFile(json, "../data/spineboy-walk.json", skeletonData);
		SkeletonJson_dispose(json);

		Skeleton* skeleton = Skeleton_create(skeletonData);
		skeleton->flipX = false;
		skeleton->flipY = false;
		Skeleton_setToBindPose(skeleton);
		Skeleton_getRootBone(skeleton)->x = 320;
		Skeleton_getRootBone(skeleton)->y = 420;
		Skeleton_updateWorldTransform(skeleton);

		sf::RenderWindow window(sf::VideoMode(640, 480), "Spine SFML");
		window.setFramerateLimit(60);
		sf::Event event;
		sf::Clock deltaClock;
		float animationTime = 0;
		while (window.isOpen()) {
			while (window.pollEvent(event))
				if (event.type == sf::Event::Closed) window.close();
			window.clear();
			window.draw(Skeleton_getDrawable(skeleton));
			window.display();

			float delta = deltaClock.getElapsedTime().asSeconds();
			deltaClock.restart();
			animationTime += delta;

			Animation_apply(animation, skeleton, animationTime, true);
			Skeleton_updateWorldTransform(skeleton);
		}

		Skeleton_dispose(skeleton);
		SkeletonData_dispose(skeletonData);
		Atlas_dispose(atlas);
	} catch (exception &ex) {
		cout << ex.what() << endl << flush;
	}
}
