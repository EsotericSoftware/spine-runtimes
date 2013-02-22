#include <iostream>
#include <fstream>
#include <spine-sfml/spine.h>
#include <SFML/Graphics.hpp>

using namespace std;
using namespace spine;

int main () {

	try {
		ifstream atlasFile("../data/spineboy.atlas");
		Atlas *atlas = new Atlas(atlasFile);

		SkeletonJson skeletonJson(atlas);

		ifstream skeletonFile("../data/spineboy-skeleton.json");
		SkeletonData *skeletonData = skeletonJson.readSkeletonData(skeletonFile);

		ifstream animationFile("../data/spineboy-walk.json");
		Animation *animation = skeletonJson.readAnimation(animationFile, skeletonData);

		Skeleton *skeleton = new Skeleton(skeletonData);
		skeleton->flipX = false;
		skeleton->flipY = false;
		skeleton->setToBindPose();
		skeleton->getRootBone()->x = 200;
		skeleton->getRootBone()->y = 420;
		skeleton->updateWorldTransform();

		sf::RenderWindow window(sf::VideoMode(640, 480), "Spine SFML");
		window.setFramerateLimit(60);
		sf::Event event;
		sf::Clock deltaClock;
		float animationTime = 0;
		while (window.isOpen()) {
			while (window.pollEvent(event))
				if (event.type == sf::Event::Closed) window.close();
			window.clear();
			window.draw(*skeleton);
			window.display();

			float delta = deltaClock.getElapsedTime().asSeconds();
			deltaClock.restart();
			animationTime += delta;

			animation->apply(skeleton, animationTime, true);
			skeleton->updateWorldTransform();
		}
	} catch (exception &ex) {
		cout << ex.what() << endl << flush;
	}
}
