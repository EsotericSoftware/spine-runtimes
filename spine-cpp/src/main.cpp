#include <iostream>
#include <fstream>
#include <spine-sfml/spine.h>
#include <SFML/Graphics.hpp>

using namespace std;
using namespace spine;

int main () {
	ifstream file("../data/spineboy-skeleton.json");

	try {
		ifstream file2("../data/spineboy.atlas");
		Atlas *atlas = new Atlas(file2);
		SkeletonJson skeletonJson(atlas);
		SkeletonData *skeletonData = skeletonJson.readSkeletonData(file);
		Skeleton *skeleton = new Skeleton(skeletonData);
		skeleton->setToBindPose();
		skeleton->getRootBone()->x = 200;
		skeleton->getRootBone()->y = 420;
		skeleton->updateWorldTransform();

		sf::RenderWindow window(sf::VideoMode(640, 480), "Spine SFML");
		window.setFramerateLimit(60);
		sf::Event event;
		while (window.isOpen()) {
			while (window.pollEvent(event))
				if (event.type == sf::Event::Closed) window.close();
			window.clear();
			window.draw(*skeleton);
			window.display();
		}
	} catch (exception &ex) {
		cout << ex.what() << endl << flush;
	}
}
