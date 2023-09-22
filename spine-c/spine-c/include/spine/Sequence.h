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

#ifndef SPINE_SEQUENCE_H
#define SPINE_SEQUENCE_H

#include <spine/dll.h>
#include <spine/TextureRegion.h>
#include <spine/Atlas.h>
#include "Attachment.h"
#include "Slot.h"

#ifdef __cplusplus
extern "C" {
#endif

_SP_ARRAY_DECLARE_TYPE(spTextureRegionArray, spTextureRegion*)

typedef struct spSequence {
	int id;
	int start;
	int digits;
	int setupIndex;
	spTextureRegionArray *regions;
} spSequence;

SP_API spSequence *spSequence_create(int numRegions);

SP_API void spSequence_dispose(spSequence *self);

SP_API spSequence *spSequence_copy(spSequence *self);

SP_API void spSequence_apply(spSequence *self, spSlot *slot, spAttachment *attachment);

SP_API void spSequence_getPath(spSequence *self, const char *basePath, int index, char *path);

#define SP_SEQUENCE_MODE_HOLD 0
#define SP_SEQUENCE_MODE_ONCE 1
#define SP_SEQUENCE_MODE_LOOP 2
#define SP_SEQUENCE_MODE_PINGPONG 3
#define SP_SEQUENCE_MODE_ONCEREVERSE 4
#define SP_SEQUENCE_MODE_LOOPREVERSE 5
#define SP_SEQUENCE_MODE_PINGPONGREVERSE 6

#ifdef __cplusplus
}
#endif

#endif
