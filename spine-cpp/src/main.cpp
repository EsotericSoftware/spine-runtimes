#include <iostream>
#include <fstream>
#include <spine/BaseSkeleton.h>
#include <spine/AtlasData.h>
#include <spine-sfml/spine.h>
#include <SFML/Graphics.hpp>

using namespace std;
using namespace spine;

int main () {
	ifstream file("../spineboy-skeleton.json");

	try {
		SkeletonJson skeletonJson;
		SkeletonData *skeletonData = skeletonJson.readSkeletonData(file);
		cout << skeletonData->bones.size() << " bone datas.\n";
		cout << skeletonData->slots.size() << " slot datas.\n";
		cout << skeletonData->skins.size() << " skins.\n";
		cout << (skeletonData->defaultSkin ? "Has" : "Doesn't have") << " default skin.\n";

		BaseSkeleton *skeleton = new BaseSkeleton(skeletonData);
		cout << skeleton->bones.size() << " bones.\n";
	} catch (exception &ex) {
		cout << ex.what() << endl;
	}
	cout << flush;

	ifstream file2("../uiskin.atlas");
	new AtlasData(file2);

	sf::RenderWindow window(sf::VideoMode(640, 480), "Spine SFML");
	window.setFramerateLimit(60);

	sf::CircleShape shape(100.f);
	shape.setFillColor(sf::Color::Green);

	sf::Event event;
	while (window.isOpen() && false) {
		while (window.pollEvent(event))
			if (event.type == sf::Event::Closed) window.close();
		window.clear();
		window.draw(shape);
		window.display();
	}

	return 0;
}
