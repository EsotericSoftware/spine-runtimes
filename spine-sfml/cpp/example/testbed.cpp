/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <SFML/Graphics.hpp>
#include <spine/Log.h>
#include <spine/spine-sfml.h>

using namespace spine;

class NullTextureLoader : public TextureLoader {
public:
	virtual void load(AtlasPage &, const String &) {}

	virtual void unload(void *) {}
};

class NullAttachmentLoader : public AttachmentLoader {
	virtual RegionAttachment *newRegionAttachment(Skin &, const String &name, const String &, Sequence *) {
		return new (__FILE__, __LINE__) RegionAttachment(name);
	}

	virtual MeshAttachment *newMeshAttachment(Skin &, const String &name, const String &, Sequence *) {
		return new (__FILE__, __LINE__) MeshAttachment(name);
	}

	virtual BoundingBoxAttachment *newBoundingBoxAttachment(Skin &, const String &name) {
		return new (__FILE__, __LINE__) BoundingBoxAttachment(name);
	}

	virtual PathAttachment *newPathAttachment(Skin &, const String &name) {
		return new (__FILE__, __LINE__) PathAttachment(name);
	}

	virtual PointAttachment *newPointAttachment(Skin &, const String &name) {
		return new (__FILE__, __LINE__) PointAttachment(name);
	}

	virtual ClippingAttachment *newClippingAttachment(Skin &, const String &name) {
		return new (__FILE__, __LINE__) ClippingAttachment(name);
	}

	virtual void configureAttachment(Attachment *) {
	}
};

int main(void) {
	String atlasFile("data/sack-pma.atlas");
	String skeletonFile("data/sack-pro.json");
	String animation = "walk";
	String skin = "";

	float scale = 0.6f;
	SFMLTextureLoader textureLoader;
	NullAttachmentLoader nullLoader;
	Atlas *atlas = atlasFile.length() == 0 ? nullptr : new Atlas(atlasFile, &textureLoader);
	SkeletonData *skeletonData = nullptr;
	if (strnstr(skeletonFile.buffer(), ".skel", skeletonFile.length()) != nullptr) {
		SkeletonBinary *binary = nullptr;
		if (atlas) {
			binary = new SkeletonBinary(atlas);
		} else {
			binary = new SkeletonBinary(&nullLoader);
		}
		binary->setScale(scale);
		skeletonData = binary->readSkeletonDataFile(skeletonFile);
		delete binary;
	} else {
		SkeletonJson *json = nullptr;
		if (atlas) {
			json = new SkeletonJson(atlas);
		} else {
			json = new SkeletonJson(&nullLoader);
		}
		json->setScale(scale);
		skeletonData = json->readSkeletonDataFile(skeletonFile);
		delete json;
	}

	AnimationStateData stateData(skeletonData);
	SkeletonDrawable drawable(skeletonData, &stateData);
	drawable.skeleton->updateWorldTransform(Physics_Update);
	drawable.skeleton->setPosition(320, 960);
	if (animation.length() > 0) drawable.state->setAnimation(0, animation, true);
	if (skin.length() > 0) drawable.skeleton->setSkin(skin);

	sf::RenderWindow window(sf::VideoMode(1024, 1024), "Spine SFML - testbed");
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

	return 0;
}
