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

#ifndef SPINE_PHYSICSCONSTRAINT_H_
#define SPINE_PHYSICSCONSTRAINT_H_

#include <spine/dll.h>
#include <spine/PhysicsConstraintData.h>
#include <spine/Bone.h>
#include <spine/Physics.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct spPhysicsConstraint {
    spPhysicsConstraintData *data;
    spBone* bone;

    float inertia;
    float strength;
    float damping;
    float massInverse;
    float wind;
    float gravity;
    float mix;

    int/*bool*/ reset;
    float ux;
    float uy;
    float cx;
    float cy;
    float tx;
    float ty;
    float xOffset;
    float xVelocity;
    float yOffset;
    float yVelocity;
    float rotateOffset;
    float rotateVelocity;
    float scaleOffset;
    float scaleVelocity;

    int/*bool*/ active;

    struct spSkeleton *skeleton;
    float remaining;
    float lastTime;

} spPhysicsConstraint;

SP_API spPhysicsConstraint *
spPhysicsConstraint_create(spPhysicsConstraintData *data, struct spSkeleton *skeleton);

SP_API void spPhysicsConstraint_dispose(spPhysicsConstraint *self);

SP_API void spPhysicsConstraint_reset(spPhysicsConstraint *self);

SP_API void spPhysicsConstraint_setToSetupPose(spPhysicsConstraint *self);

SP_API void spPhysicsConstraint_update(spPhysicsConstraint *self, spPhysics physics);

SP_API void spPhysicsConstraint_rotate(spPhysicsConstraint *self, float x, float y, float degrees);

SP_API void spPhysicsConstraint_translate(spPhysicsConstraint *self, float x, float y);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_PHYSICSCONSTRAINT_H_ */
