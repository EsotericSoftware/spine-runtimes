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

sf::BlendMode normal = sf::BlendMode(sf::BlendMode::SrcAlpha, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode additive = sf::BlendMode(sf::BlendMode::SrcAlpha, sf::BlendMode::One);
sf::BlendMode multiply = sf::BlendMode(sf::BlendMode::DstColor, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode screen = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcColor);

sf::BlendMode normalPma = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode additivePma = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::One);
sf::BlendMode multiplyPma = sf::BlendMode(sf::BlendMode::DstColor, sf::BlendMode::OneMinusSrcAlpha);
sf::BlendMode screenPma = sf::BlendMode(sf::BlendMode::One, sf::BlendMode::OneMinusSrcColor);

namespace spine {

	SkeletonDrawable::SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *stateData) : timeScale(1),
																									vertexArray(new VertexArray(Triangles, skeletonData->getBones().size() * 4)),
																									worldVertices(), clipper() {
		Bone::setYDown(true);
		worldVertices.ensureCapacity(SPINE_MESH_VERTEX_COUNT_MAX);
		skeleton = new (__FILE__, __LINE__) Skeleton(skeletonData);
		tempUvs.ensureCapacity(16);
		tempColors.ensureCapacity(16);

		ownsAnimationStateData = stateData == 0;
		if (ownsAnimationStateData) stateData = new (__FILE__, __LINE__) AnimationStateData(skeletonData);

		state = new (__FILE__, __LINE__) AnimationState(stateData);

		quadIndices.add(0);
		quadIndices.add(1);
		quadIndices.add(2);
		quadIndices.add(2);
		quadIndices.add(3);
		quadIndices.add(0);
	}

	SkeletonDrawable::~SkeletonDrawable() {
		delete vertexArray;
		if (ownsAnimationStateData) delete state->getData();
		delete state;
		delete skeleton;
	}

	void SkeletonDrawable::update(float deltaTime) {
		state->update(deltaTime * timeScale);
		state->apply(*skeleton);
		skeleton->updateWorldTransform();
	}

	void SkeletonDrawable::draw(RenderTarget &target, RenderStates states) const {
		vertexArray->clear();
		states.texture = NULL;

		// Early out if skeleton is invisible
		if (skeleton->getColor().a == 0) return;

		sf::Vertex vertex;
		Texture *texture = NULL;
		for (unsigned i = 0; i < skeleton->getSlots().size(); ++i) {
			Slot &slot = *skeleton->getDrawOrder()[i];
			Attachment *attachment = slot.getAttachment();
			if (!attachment) {
				clipper.clipEnd(slot);
				continue;
			}

			// Early out if the slot color is 0 or the bone is not active
			if (slot.getColor().a == 0 || !slot.getBone().isActive()) {
				clipper.clipEnd(slot);
				continue;
			}

			Vector<float> *vertices = &worldVertices;
			Vector<float> *uvs = NULL;
			Vector<unsigned short> *indices = NULL;
			int indicesCount = 0;
			Color *attachmentColor;

			if (attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
				RegionAttachment *regionAttachment = (RegionAttachment *) attachment;
				attachmentColor = &regionAttachment->getColor();

				// Early out if the slot color is 0
				if (attachmentColor->a == 0) {
					clipper.clipEnd(slot);
					continue;
				}

				worldVertices.setSize(8, 0);
				regionAttachment->computeWorldVertices(slot, worldVertices, 0, 2);
				uvs = &regionAttachment->getUVs();
				indices = &quadIndices;
				indicesCount = 6;
				texture = (Texture *) ((AtlasRegion *) regionAttachment->getRegion())->page->texture;

			} else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
				MeshAttachment *mesh = (MeshAttachment *) attachment;
				attachmentColor = &mesh->getColor();

				// Early out if the slot color is 0
				if (attachmentColor->a == 0) {
					clipper.clipEnd(slot);
					continue;
				}

				worldVertices.setSize(mesh->getWorldVerticesLength(), 0);
				mesh->computeWorldVertices(slot, 0, mesh->getWorldVerticesLength(), worldVertices.buffer(), 0, 2);
				uvs = &mesh->getUVs();
				indices = &mesh->getTriangles();
				indicesCount = mesh->getTriangles().size();
				texture = (Texture *) ((AtlasRegion *) mesh->getRegion())->page->texture;

			} else if (attachment->getRTTI().isExactly(ClippingAttachment::rtti)) {
				ClippingAttachment *clip = (ClippingAttachment *) slot.getAttachment();
				clipper.clipStart(slot, clip);
				continue;
			} else
				continue;

			Uint8 r = static_cast<Uint8>(skeleton->getColor().r * slot.getColor().r * attachmentColor->r * 255);
			Uint8 g = static_cast<Uint8>(skeleton->getColor().g * slot.getColor().g * attachmentColor->g * 255);
			Uint8 b = static_cast<Uint8>(skeleton->getColor().b * slot.getColor().b * attachmentColor->b * 255);
			Uint8 a = static_cast<Uint8>(skeleton->getColor().a * slot.getColor().a * attachmentColor->a * 255);
			vertex.color.r = r;
			vertex.color.g = g;
			vertex.color.b = b;
			vertex.color.a = a;

			Color light;
			light.r = r / 255.0f;
			light.g = g / 255.0f;
			light.b = b / 255.0f;
			light.a = a / 255.0f;

			sf::BlendMode blend;
			if (!usePremultipliedAlpha) {
				switch (slot.getData().getBlendMode()) {
					case BlendMode_Normal:
						blend = normal;
						break;
					case BlendMode_Additive:
						blend = additive;
						break;
					case BlendMode_Multiply:
						blend = multiply;
						break;
					case BlendMode_Screen:
						blend = screen;
						break;
					default:
						blend = normal;
				}
			} else {
				switch (slot.getData().getBlendMode()) {
					case BlendMode_Normal:
						blend = normalPma;
						break;
					case BlendMode_Additive:
						blend = additivePma;
						break;
					case BlendMode_Multiply:
						blend = multiplyPma;
						break;
					case BlendMode_Screen:
						blend = screenPma;
						break;
					default:
						blend = normalPma;
				}
			}

			if (states.texture == 0) states.texture = texture;

			if (states.blendMode != blend || states.texture != texture) {
				target.draw(*vertexArray, states);
				vertexArray->clear();
				states.blendMode = blend;
				states.texture = texture;
			}

			if (clipper.isClipping()) {
				clipper.clipTriangles(worldVertices, *indices, *uvs, 2);
				vertices = &clipper.getClippedVertices();
				uvs = &clipper.getClippedUVs();
				indices = &clipper.getClippedTriangles();
				indicesCount = clipper.getClippedTriangles().size();
			}

			Vector2u size = texture->getSize();

			for (int ii = 0; ii < indicesCount; ++ii) {
				int index = (*indices)[ii] << 1;
				vertex.position.x = (*vertices)[index];
				vertex.position.y = (*vertices)[index + 1];
				vertex.texCoords.x = (*uvs)[index] * size.x;
				vertex.texCoords.y = (*uvs)[index + 1] * size.y;
				vertexArray->append(vertex);
			}
			clipper.clipEnd(slot);
		}
		target.draw(*vertexArray, states);
		clipper.clipEnd();
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

	SpineExtension *getDefaultExtension() {
		return new DefaultSpineExtension();
	}
}// namespace spine
