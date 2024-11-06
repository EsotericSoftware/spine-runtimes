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

#pragma once

#include "SpineCommon.h"
#include "SpineSprite.h"
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/node2d.hpp>
#else
#include "scene/2d/node_2d.h"
#endif

class SpineSlotNode : public Node2D {
	GDCLASS(SpineSlotNode, Node2D)

protected:
	String slot_name;
	int slot_index;
	Ref<Material> normal_material;
	Ref<Material> additive_material;
	Ref<Material> multiply_material;
	Ref<Material> screen_material;

	static void _bind_methods();
	void _notification(int what);
	void _get_property_list(List<PropertyInfo> *list) const;
	bool _get(const StringName &property, Variant &value) const;
	bool _set(const StringName &property, const Variant &value);
	void on_world_transforms_changed(const Variant &_sprite);
	void update_transform(SpineSprite *sprite);

public:
	SpineSlotNode();

	void set_slot_name(const String &_slot_name);

	String get_slot_name();

	int get_slot_index() { return slot_index; }

	Ref<Material> get_normal_material();

	void set_normal_material(Ref<Material> material);

	Ref<Material> get_additive_material();

	void set_additive_material(Ref<Material> material);

	Ref<Material> get_multiply_material();

	void set_multiply_material(Ref<Material> material);

	Ref<Material> get_screen_material();

	void set_screen_material(Ref<Material> material);
};
