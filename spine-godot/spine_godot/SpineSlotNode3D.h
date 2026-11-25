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
#if VERSION_MAJOR > 3
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/node3d.hpp>
#include <godot_cpp/classes/shader_material.hpp>
#include <godot_cpp/core/object_id.hpp>
#else
#include "scene/3d/node_3d.h"
#include "scene/resources/material.h"
#endif
#include <vector>

namespace spine {
	class Slot;
}
class SpineSprite3D;

class SpineSlotNode3D : public Node3D {
	GDCLASS(SpineSlotNode3D, Node3D)
	friend class SpineSprite3D;

	struct SortingState {
		ObjectID id;
		float offset;
		bool use_aabb_center;
	};
	String slot_name;
	int slot_index = -1;
	NodePath depth_camera;
	float physical_depth = 0;
	Ref<ShaderMaterial> normal_material;
	Ref<ShaderMaterial> additive_material;
	std::vector<SortingState> saved_sorting;
	std::vector<ObjectID> geometries;
	bool geometries_dirty = true;
	uint64_t geometry_generation = 0;
	bool active = true;

	static void _bind_methods();
	void _notification(int what);
	void _validate_property(PropertyInfo &property) const;
	void tree_changed();
	void update_geometry_list();
	void update_depth_connection();
	void update_depth();
	float get_depth_sign() const;
	void release_sorting();
	void sync_slot(spine::Slot *slot, float pixel_size, float depth);
	const std::vector<ObjectID> &get_geometries();

public:
	~SpineSlotNode3D() override;
	void set_slot_name(const String &name);
	String get_slot_name() const;
	int get_slot_index() const;
	void set_depth_camera(const NodePath &path);
	NodePath get_depth_camera() const;
	void set_normal_material(const Ref<ShaderMaterial> &material);
	Ref<ShaderMaterial> get_normal_material() const;
	void set_additive_material(const Ref<ShaderMaterial> &material);
	Ref<ShaderMaterial> get_additive_material() const;
	PackedStringArray get_sorting_warnings() const;
#ifdef SPINE_GODOT_EXTENSION
	PackedStringArray _get_configuration_warnings() const override;
#else
	PackedStringArray get_configuration_warnings() const override;
#endif
};
#endif
