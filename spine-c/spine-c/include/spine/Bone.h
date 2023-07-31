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

#ifndef SPINE_BONE_H_
#define SPINE_BONE_H_

#include <spine/dll.h>
#include <spine/BoneData.h>

#ifdef __cplusplus
extern "C" {
#endif

struct spSkeleton;

typedef struct spBone spBone;
struct spBone {
	spBoneData *const data;
	struct spSkeleton *const skeleton;
	spBone *const parent;
	int childrenCount;
	spBone **const children;
	float x, y, rotation, scaleX, scaleY, shearX, shearY;
	float ax, ay, arotation, ascaleX, ascaleY, ashearX, ashearY;

	float const a, b, worldX;
	float const c, d, worldY;

	int/*bool*/ sorted;
	int/*bool*/ active;
};

SP_API void spBone_setYDown(int/*bool*/yDown);

SP_API int/*bool*/spBone_isYDown();

/* @param parent May be 0. */
SP_API spBone *spBone_create(spBoneData *data, struct spSkeleton *skeleton, spBone *parent);

SP_API void spBone_dispose(spBone *self);

SP_API void spBone_setToSetupPose(spBone *self);

SP_API void spBone_update(spBone *self);

SP_API void spBone_updateWorldTransform(spBone *self);

SP_API void spBone_updateWorldTransformWith(spBone *self, float x, float y, float rotation, float scaleX, float scaleY,
											float shearX, float shearY);

SP_API float spBone_getWorldRotationX(spBone *self);

SP_API float spBone_getWorldRotationY(spBone *self);

SP_API float spBone_getWorldScaleX(spBone *self);

SP_API float spBone_getWorldScaleY(spBone *self);

SP_API void spBone_updateAppliedTransform(spBone *self);

SP_API void spBone_worldToLocal(spBone *self, float worldX, float worldY, float *localX, float *localY);

SP_API void spBone_localToWorld(spBone *self, float localX, float localY, float *worldX, float *worldY);

SP_API float spBone_worldToLocalRotation(spBone *self, float worldRotation);

SP_API float spBone_localToWorldRotation(spBone *self, float localRotation);

SP_API void spBone_rotateWorld(spBone *self, float degrees);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_BONE_H_ */
