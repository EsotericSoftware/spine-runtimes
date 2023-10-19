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

#include <spine/AtlasAttachmentLoader.h>
#include <spine/extension.h>
#include <spine/Sequence.h>

static int /*bool*/ loadSequence(spAtlas *atlas, const char *basePath, spSequence *sequence) {
	spTextureRegionArray *regions = sequence->regions;
	char *path = CALLOC(char, strlen(basePath) + sequence->digits + 2);
	int i;
	for (i = 0; i < regions->size; i++) {
		spSequence_getPath(sequence, basePath, i, path);
		regions->items[i] = SUPER(spAtlas_findRegion(atlas, path));
		if (!regions->items[i]) {
			FREE(path);
			return 0;
		}
		regions->items[i]->rendererObject = regions->items[i];
	}
	FREE(path);
	return -1;
}

spAttachment *_spAtlasAttachmentLoader_createAttachment(spAttachmentLoader *loader, spSkin *skin, spAttachmentType type,
														const char *name, const char *path, spSequence *sequence) {
	spAtlasAttachmentLoader *self = SUB_CAST(spAtlasAttachmentLoader, loader);
	switch (type) {
		case SP_ATTACHMENT_REGION: {
			spRegionAttachment *attachment = spRegionAttachment_create(name);
			if (sequence) {
				if (!loadSequence(self->atlas, path, sequence)) {
					spAttachment_dispose(SUPER(attachment));
					_spAttachmentLoader_setError(loader, "Couldn't load sequence for region attachment: ", path);
					return 0;
				}
			} else {
				spAtlasRegion *region = spAtlas_findRegion(self->atlas, path);
				if (!region) {
					spAttachment_dispose(SUPER(attachment));
					_spAttachmentLoader_setError(loader, "Region not found: ", path);
					return 0;
				}
				attachment->rendererObject = region;
				attachment->region = SUPER(region);
			}
			return SUPER(attachment);
		}
		case SP_ATTACHMENT_MESH:
		case SP_ATTACHMENT_LINKED_MESH: {
			spMeshAttachment *attachment = spMeshAttachment_create(name);

			if (sequence) {
				if (!loadSequence(self->atlas, path, sequence)) {
					spAttachment_dispose(SUPER(SUPER(attachment)));
					_spAttachmentLoader_setError(loader, "Couldn't load sequence for mesh attachment: ", path);
					return 0;
				}
			} else {
				spAtlasRegion *region = spAtlas_findRegion(self->atlas, path);
				if (!region) {
					_spAttachmentLoader_setError(loader, "Region not found: ", path);
					return 0;
				}
				attachment->rendererObject = region;
				attachment->region = SUPER(region);
			}
			return SUPER(SUPER(attachment));
		}
		case SP_ATTACHMENT_BOUNDING_BOX:
			return SUPER(SUPER(spBoundingBoxAttachment_create(name)));
		case SP_ATTACHMENT_PATH:
			return SUPER(SUPER(spPathAttachment_create(name)));
		case SP_ATTACHMENT_POINT:
			return SUPER(spPointAttachment_create(name));
		case SP_ATTACHMENT_CLIPPING:
			return SUPER(SUPER(spClippingAttachment_create(name)));
		default:
			_spAttachmentLoader_setUnknownTypeError(loader, type);
			return 0;
	}

	UNUSED(skin);
}

spAtlasAttachmentLoader *spAtlasAttachmentLoader_create(spAtlas *atlas) {
	spAtlasAttachmentLoader *self = NEW(spAtlasAttachmentLoader);
	_spAttachmentLoader_init(SUPER(self), _spAttachmentLoader_deinit, _spAtlasAttachmentLoader_createAttachment, 0, 0);
	self->atlas = atlas;
	return self;
}
