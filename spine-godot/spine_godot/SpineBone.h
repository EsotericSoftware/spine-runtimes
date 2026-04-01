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
#include "SpineBoneData.h"
#include "SpineBonePose.h"
#include "SpineConstant.h"
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/node2d.hpp>
#else
#include "scene/2d/node_2d.h"
#endif
#include <spine/Bone.h>

class SpineSkeleton;
class SpineSprite;

class SpineBone : public SpineSpriteOwnedObject<spine::Bone> {
	GDCLASS(SpineBone, SpineObjectWrapper)

protected:
	static void _bind_methods();

public:
	Vector2 world_to_local(Vector2 world_position);

	Vector2 world_to_parent(Vector2 world_position);

	Vector2 local_to_world(Vector2 local_position);

	Vector2 parent_to_world(Vector2 local_position);

	float world_to_local_rotation(float world_rotation);

	float local_to_world_rotation(float local_rotation);

	void rotate_world(float degrees);

	Ref<SpineBoneData> get_data();

	Ref<SpineBone> get_parent();

	Array get_children();

	Ref<SpineBonePose> get_pose();

	Ref<SpineBonePose> get_applied_pose();

	bool is_active();

	void set_active(bool v);

	void update(Ref<SpineSkeleton> skeleton, SpineConstant::Physics physics);

	// External feature functions
	void apply_world_transform_2d(const Variant &o);

	Transform2D get_transform();

	void set_transform(Transform2D transform);

	Transform2D get_global_transform();

	void set_global_transform(Transform2D trans);
};
