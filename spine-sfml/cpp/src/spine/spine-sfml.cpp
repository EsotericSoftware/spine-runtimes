/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#define SPINE_SHORT_NAMES

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

namespace Spine {

SkeletonDrawable::SkeletonDrawable(SkeletonData *skeletonData, AnimationStateData *stateData) :
		timeScale(1),
		vertexArray(new VertexArray(Triangles, skeletonData->getBones().size() * 4)),
		worldVertices(), clipper(), vertexEffect(NULL) {
	Bone::setYDown(true);
	worldVertices.ensureCapacity(SPINE_MESH_VERTEX_COUNT_MAX);
	skeleton = new(__FILE__, __LINE__) Skeleton(skeletonData);
	tempUvs.ensureCapacity(16);
	tempColors.ensureCapacity(16);

	ownsAnimationStateData = stateData == 0;
	if (ownsAnimationStateData) stateData = new(__FILE__, __LINE__) AnimationStateData(skeletonData);

	state = new(__FILE__, __LINE__) AnimationState(stateData);

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
	skeleton->update(deltaTime);
	state->update(deltaTime * timeScale);
	state->apply(*skeleton);
	skeleton->updateWorldTransform();
}

void SkeletonDrawable::draw(RenderTarget &target, RenderStates states) const {
	vertexArray->clear();
	states.texture = NULL;

	if (vertexEffect != NULL) vertexEffect->begin(*skeleton);

	sf::Vertex vertex;
	Texture *texture = NULL;
	for (int i = 0; i < skeleton->getSlots().size(); ++i) {
		Slot &slot = *skeleton->getDrawOrder()[i];
		Attachment *attachment = slot.getAttachment();
		if (!attachment) continue;

		Vector<float> *vertices = &worldVertices;
		int verticesCount = 0;
		Vector<float> *uvs = NULL;
		Vector<unsigned short> *indices = NULL;
		int indicesCount = 0;
		Color *attachmentColor;

		if (attachment->getRTTI().isExactly(RegionAttachment::rtti)) {
			RegionAttachment *regionAttachment = (RegionAttachment *) attachment;
			worldVertices.setSize(8, 0);
			regionAttachment->computeWorldVertices(slot.getBone(), worldVertices, 0, 2);
			verticesCount = 4;
			uvs = &regionAttachment->getUVs();
			indices = &quadIndices;
			indicesCount = 6;
			texture = (Texture *) ((AtlasRegion *) regionAttachment->getRendererObject())->page->rendererObject;
			attachmentColor = &regionAttachment->getColor();

		} else if (attachment->getRTTI().isExactly(MeshAttachment::rtti)) {
			MeshAttachment *mesh = (MeshAttachment *) attachment;
			worldVertices.setSize(mesh->getWorldVerticesLength(), 0);
			texture = (Texture *) ((AtlasRegion *) mesh->getRendererObject())->page->rendererObject;
			mesh->computeWorldVertices(slot, 0, mesh->getWorldVerticesLength(), worldVertices, 0, 2);
			verticesCount = mesh->getWorldVerticesLength() >> 1;
			uvs = &mesh->getUVs();
			indices = &mesh->getTriangles();
			indicesCount = mesh->getTriangles().size();
			attachmentColor = &mesh->getColor();
		} else if (attachment->getRTTI().isExactly(ClippingAttachment::rtti)) {
			ClippingAttachment *clip = (ClippingAttachment *) slot.getAttachment();
			clipper.clipStart(slot, clip);
			continue;
		} else continue;

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
			clipper.clipTriangles(worldVertices, verticesCount << 1, *indices, indicesCount, *uvs);
			vertices = &clipper.getClippedVertices();
			verticesCount = clipper.getClippedVertices().size() >> 1;
			uvs = &clipper.getClippedUVs();
			indices = &clipper.getClippedTriangles();
			indicesCount = clipper.getClippedTriangles().size();
		}

		Vector2u size = texture->getSize();

		if (vertexEffect != 0) {
			tempUvs.clear();
			tempColors.clear();
			for (int i = 0; i < verticesCount; i++) {
				Color vertexColor = light;
				Color dark;
				dark.r = dark.g = dark.b = dark.a = 0;
				int index = i << 1;
				float x = (*vertices)[index];
				float y = (*vertices)[index + 1];
				float u = (*uvs)[index];
				float v = (*uvs)[index + 1];
				vertexEffect->transform(x, y, u, v, vertexColor, dark);
				(*vertices)[index] = x;
				(*vertices)[index + 1] = y;
				tempUvs.add(u);
				tempUvs.add(v);
				tempColors.add(vertexColor);
			}

			for (int i = 0; i < indicesCount; ++i) {
				int index = (*indices)[i] << 1;
				vertex.position.x = (*vertices)[index];
				vertex.position.y = (*vertices)[index + 1];
				vertex.texCoords.x = (*uvs)[index] * size.x;
				vertex.texCoords.y = (*uvs)[index + 1] * size.y;
				Color vertexColor = tempColors[index >> 1];
				vertex.color.r = static_cast<Uint8>(vertexColor.r * 255);
				vertex.color.g = static_cast<Uint8>(vertexColor.g * 255);
				vertex.color.b = static_cast<Uint8>(vertexColor.b * 255);
				vertex.color.a = static_cast<Uint8>(vertexColor.a * 255);
				vertexArray->append(vertex);
			}
		} else {
			for (int ii = 0; ii < indicesCount; ++ii) {
				int index = (*indices)[ii] << 1;
				vertex.position.x = (*vertices)[index];
				vertex.position.y = (*vertices)[index + 1];
				vertex.texCoords.x = (*uvs)[index] * size.x;
				vertex.texCoords.y = (*uvs)[index + 1] * size.y;
				vertexArray->append(vertex);
			}
		}
		clipper.clipEnd(slot);
	}
	target.draw(*vertexArray, states);
	clipper.clipEnd();

	if (vertexEffect != 0) vertexEffect->end();
}

void SFMLTextureLoader::load(AtlasPage &page, const String &path) {
	Texture *texture = new Texture();
	if (!texture->loadFromFile(path.buffer())) return;

	if (page.magFilter == TextureFilter_Linear) texture->setSmooth(true);
	if (page.uWrap == TextureWrap_Repeat && page.vWrap == TextureWrap_Repeat) texture->setRepeated(true);

	page.rendererObject = texture;
	Vector2u size = texture->getSize();
	page.width = size.x;
	page.height = size.y;
}

void SFMLTextureLoader::unload(void *texture) {
	delete (Texture *) texture;
}

String SFMLTextureLoader::toString() const {
	return String("SFMLTextureLoader");
}

}
