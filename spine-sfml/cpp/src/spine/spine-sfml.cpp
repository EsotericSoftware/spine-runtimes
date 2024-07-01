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

#include <spine/spine-sfml.h>

#ifndef SPINE_MESH_VERTEX_COUNT_MAX
#define SPINE_MESH_VERTEX_COUNT_MAX 1000
#endif

using namespace sf;
using namespace spine;

sf::BlendMode blendModes[] = {
        sf::BlendMode(sf::BlendMode::SrcAlpha, sf::BlendMode::OneMinusSrcAlpha),
        sf::BlendMode(sf::BlendMode::SrcAlpha, sf::BlendMode::One),
        sf::BlendMode(sf::BlendMode::DstColor, sf::BlendMode::OneMinusSrcAlpha),
        sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcColor)
};

sf::BlendMode blendModesPma[] = {
        sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcAlpha),
        sf::BlendMode(sf::BlendMode::One, sf::BlendMode::One),
        sf::BlendMode(sf::BlendMode::DstColor, sf::BlendMode::OneMinusSrcAlpha),
        sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcColor),
};

SkeletonRenderer *skeletonRenderer = nullptr;

SkeletonDrawable::SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *stateData) : timeScale(1),
                                                                                                usePremultipliedAlpha(false),
                                                                                                vertexArray(new VertexArray(Triangles, skeletonData->getBones().size() * 4)) {
    Bone::setYDown(true);
    skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);
    ownsAnimationStateData = stateData == 0;
    if (ownsAnimationStateData) stateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);
    state = new (__FILE__, __LINE__) AnimationState(stateData);
}

SkeletonDrawable::~SkeletonDrawable() {
    delete vertexArray;
    if (ownsAnimationStateData) delete state->getData();
    delete state;
    delete skeleton;
}

void SkeletonDrawable::update(float deltaTime, Physics physics) {
    state->update(deltaTime * timeScale);
    state->apply(*skeleton);
    skeleton->update(deltaTime * timeScale);
    skeleton->updateWorldTransform(physics);
}

inline void toSFMLColor(uint32_t color, sf::Color *sfmlColor) {
    sfmlColor->a = (color >> 24) & 0xFF;
    sfmlColor->r = (color >> 16) & 0xFF;
    sfmlColor->g = (color >> 8) & 0xFF;
    sfmlColor->b = color & 0xFF;
}

void SkeletonDrawable::draw(RenderTarget &target, RenderStates states) const {
    states.texture = NULL;
    vertexArray->clear();

    if (!skeletonRenderer) skeletonRenderer = new (__FILE__, __LINE__) SkeletonRenderer();
    RenderCommand *command = skeletonRenderer->render(*skeleton);
    while (command) {
        Texture *texture = (Texture *)command->texture;
        Vector2u size = texture->getSize();
        Vertex vertex;
        float *positions = command->positions;
        float *uvs = command->uvs;
        uint32_t *colors = command->colors;
        uint16_t *indices = command->indices;
        for (int i = 0, n = command->numIndices; i < n; ++i) {
            int ii = indices[i];
            int index = ii << 1;
            vertex.position.x = positions[index];
            vertex.position.y = positions[index + 1];
            vertex.texCoords.x = uvs[index] * size.x;
            vertex.texCoords.y = uvs[index + 1] * size.y;
            toSFMLColor(colors[ii], &vertex.color);
            vertexArray->append(vertex);
        }
        BlendMode blendMode = command->blendMode;
        states.blendMode = usePremultipliedAlpha ? blendModesPma[blendMode] : blendModes[blendMode];
        states.texture = texture;
        target.draw(*vertexArray, states);
        vertexArray->clear();

        command = command->next;
    }
}

void SFMLTextureLoader::load(AtlasPage &page, const String &path) {
    Texture *texture = new Texture();
    if (!texture->loadFromFile(path.buffer())) return;

    if (page.magFilter == TextureFilter_Linear) texture->setSmooth(true);
    if (page.uWrap == TextureWrap_Repeat && page.vWrap == TextureWrap_Repeat) texture->setRepeated(true);

    page.texture = texture;
    Vector2u size = texture->getSize();
    page.width = size.x;
    page.height = size.y;
}

void SFMLTextureLoader::unload(void *texture) {
    delete (Texture *) texture;
}

SpineExtension *spine::getDefaultExtension() {
    return new DefaultSpineExtension();
}
