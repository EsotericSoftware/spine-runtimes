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

#pragma once

#include "SpineCommon.h"
#include "SpinePhysicsConstraintData.h"
#include "SpineBone.h"
#include <spine/PhysicsConstraint.h>

class SpinePhysicsConstraintPose;

class SpinePhysicsConstraint : public SpineControllerOwnedObject<spine::PhysicsConstraint> {
	GDCLASS(SpinePhysicsConstraint, SpineObjectWrapper)

protected:
	static void _bind_methods();

public:
	void update(Ref<SpineSkeleton> skeleton, SpineConstant::Physics physics);

	Ref<SpinePhysicsConstraintData> get_data();

	Ref<SpineBonePose> get_bone();
	void set_bone(Ref<SpineBonePose> v);

	Ref<SpinePhysicsConstraintPose> get_pose();
	Ref<SpinePhysicsConstraintPose> get_applied_pose();

	void reset(Ref<SpineSkeleton> skeleton);

	void translate(float x, float y);

	void rotate(float x, float y, float degrees);
};
