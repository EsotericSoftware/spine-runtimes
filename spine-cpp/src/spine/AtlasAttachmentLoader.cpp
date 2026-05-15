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

#include <spine/AtlasAttachmentLoader.h>
#include <spine/BoundingBoxAttachment.h>
#include <spine/ClippingAttachment.h>
#include <spine/MeshAttachment.h>
#include <spine/PathAttachment.h>
#include <spine/PointAttachment.h>
#include <spine/RegionAttachment.h>
#include <spine/Skin.h>

#include <spine/Atlas.h>

using namespace spine;

AtlasAttachmentLoader::AtlasAttachmentLoader(Atlas &atlas) : AttachmentLoader(), _atlas(&atlas) {
}

static void findRegions(Atlas *atlas, AtlasAttachmentLoader *loader, const String &name, const String &basePath, Sequence *sequence) {
	Array<TextureRegion *> &regions = sequence->getRegions();
	for (int i = 0, n = (int) regions.size(); i < n; i++) {
		String path = sequence->getPath(basePath, i);
		regions[i] = loader->findRegion(path);
	}
}

RegionAttachment *AtlasAttachmentLoader::newRegionAttachment(Skin &skin, const String &placeholder, const String &name, const String &path,
															 Sequence *sequence) {
	SP_UNUSED(skin);
	SP_UNUSED(placeholder);
	findRegions(_atlas, this, name, path, sequence);
	return new (__FILE__, __LINE__) RegionAttachment(name, sequence);
}

MeshAttachment *AtlasAttachmentLoader::newMeshAttachment(Skin &skin, const String &placeholder, const String &name, const String &path,
														 Sequence *sequence) {
	SP_UNUSED(skin);
	SP_UNUSED(placeholder);
	findRegions(_atlas, this, name, path, sequence);
	return new (__FILE__, __LINE__) MeshAttachment(name, sequence);
}

BoundingBoxAttachment *AtlasAttachmentLoader::newBoundingBoxAttachment(Skin &skin, const String &placeholder, const String &name) {
	SP_UNUSED(skin);
	SP_UNUSED(placeholder);
	return new (__FILE__, __LINE__) BoundingBoxAttachment(name);
}

PathAttachment *AtlasAttachmentLoader::newPathAttachment(Skin &skin, const String &placeholder, const String &name) {
	SP_UNUSED(skin);
	SP_UNUSED(placeholder);
	return new (__FILE__, __LINE__) PathAttachment(name);
}

PointAttachment *AtlasAttachmentLoader::newPointAttachment(Skin &skin, const String &placeholder, const String &name) {
	SP_UNUSED(skin);
	SP_UNUSED(placeholder);
	return new (__FILE__, __LINE__) PointAttachment(name);
}

ClippingAttachment *AtlasAttachmentLoader::newClippingAttachment(Skin &skin, const String &placeholder, const String &name) {
	SP_UNUSED(skin);
	SP_UNUSED(placeholder);
	return new (__FILE__, __LINE__) ClippingAttachment(name);
}

AtlasRegion *AtlasAttachmentLoader::findRegion(const String &name) {
	return _atlas->findRegion(name);
}
