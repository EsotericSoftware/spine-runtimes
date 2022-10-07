/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

#include <SFML/Graphics.hpp>
#include <spine/Debug.h>
#include <spine/Log.h>
#include <spine/spine-sfml.h>

using namespace std;
using namespace spine;

DebugExtension dbgExtension(SpineExtension::getInstance());

void test() {
    SFMLTextureLoader textureLoader;
    Atlas atlas("data/bomb.atlas", &textureLoader);
    SkeletonBinary loader(&atlas);
    SkeletonData *skeletonData = loader.readSkeletonDataFile("data/bomb.skel");

    SkeletonDrawable drawable(skeletonData);
    drawable.setUsePremultipliedAlpha(true);
    drawable.skeleton->setPosition(320, 590);
    drawable.state->setAnimation(0, "expl", false);
    drawable.skeleton->setSkin("mdl");

    sf::RenderWindow window(sf::VideoMode(640, 640), "Spine SFML - Test");
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

    delete skeletonData;
}

int main() {
    SpineExtension::setInstance(&dbgExtension);
    test();
    dbgExtension.reportLeaks();
    return 0;
}