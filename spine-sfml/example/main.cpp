/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#include "spine/AnimationState.h"
#include <spine-sfml.h>
#include <stdio.h>

using namespace spine;

int main() {
    // Create SFML window
    sf::RenderWindow window(sf::VideoMode(800, 600), "Spine SFML C++ Example");
    window.setFramerateLimit(60);

    // Load atlas and skeleton
    SFMLTextureLoader textureLoader;
    Atlas atlas("data/spineboy-pma.atlas", &textureLoader);

    SkeletonBinary binary(atlas);
    SkeletonData* skeletonData = binary.readSkeletonDataFile("data/spineboy-pro.skel");
    if (!skeletonData) {
        printf("Failed to load skeleton data\n");
        return 1;
    }

    // Create skeleton and animation state
    Skeleton skeleton(*skeletonData);
    AnimationStateData animationStateData(*skeletonData);
    animationStateData.setDefaultMix(0.2f);
    AnimationState animationState(animationStateData);

    // Setup skeleton
    skeleton.setPosition(400, 500);
    skeleton.setScaleX(0.5f);
    skeleton.setScaleY(0.5f);
    skeleton.setupPose();

    // Setup animation sequence with listener callback on the "run" animation
    animationState.setAnimation(0, "portal", false);
    animationState.addAnimation(0, "run", true, 0).setListener([](AnimationState* state, EventType type, TrackEntry* entry, Event* event, void* userData) {
       switch(type) {
        case spine::EventType_Start:
            printf("Animation started: %s\n", entry->getAnimation().getName().buffer());
            break;
        case spine::EventType_Interrupt:
            printf("Animation interrupted: %s\n", entry->getAnimation().getName().buffer());
            break;
        case spine::EventType_End:
            printf("Animation ended: %s\n", entry->getAnimation().getName().buffer());
            break;
        case spine::EventType_Complete:
            printf("Animation completed: %s\n", entry->getAnimation().getName().buffer());
            break;
        case spine::EventType_Dispose:
            printf("Animation disposed: %s\n", entry->getAnimation().getName().buffer());
            break;
        case spine::EventType_Event:
            printf("Event fired: %s\n", event->getData().getName().buffer());
            break;
       }
    });

    // Main loop
    sf::Clock clock;
    while (window.isOpen()) {
        // Handle SDL events
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) {
                window.close();
            }
        }

        // Update animation
        float deltaTime = clock.restart().asSeconds();
        animationState.update(deltaTime);
        animationState.apply(skeleton);
        skeleton.update(deltaTime);
        skeleton.updateWorldTransform(Physics_Update);

        // Clear and draw
        window.clear(sf::Color::Black);
        SFML_draw(skeleton, window, true);
        window.display();
    }

    // Cleanup (only skeletonData, everything else is stack allocated)
    delete skeletonData;

    return 0;
}