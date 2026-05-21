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
#include "SpineSprite.h"
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/node2d.hpp>
#else
#include "scene/2d/node_2d.h"
#endif

// Renders a contiguous range of a SpineSprite's slots from a different position
// in the scene tree, so that unrelated nodes can be drawn "between" parts of a
// single skeleton (the Godot equivalent of spine-unity's SkeletonRenderSeparator).
//
// The proxy does not reparent or rebuild the source's geometry. It owns a small
// pool of SpineMesh2D children and, every frame, mirrors the geometry and blend
// material that the source SpineSprite already computed for the claimed slots.
// The source hides its own copy of those slots (see SpineSprite::is_slot_externally_rendered).
class SpineSlotRangeProxy : public Node2D {
	GDCLASS(SpineSlotRangeProxy, Node2D)

protected:
	NodePath source_sprite_path;
	String start_slot_name;
	String end_slot_name;
	bool follow_source_transform;

	// The source this proxy is currently registered with. Never dereferenced
	// without a validity check; cleared via the source's tree_exiting signal.
	SpineSprite *registered_source;
	int registered_start_index;
	int registered_end_index;
	Vector<SpineMesh2D *> proxy_meshes;

	static void _bind_methods();
	void _notification(int what);
	void _get_property_list(List<PropertyInfo> *list) const;
	bool _get(const StringName &property, Variant &value) const;
	bool _set(const StringName &property, const Variant &value);

	SpineSprite *resolve_source_sprite() const;
	bool resolve_slot_range(SpineSprite *sprite, int &start_index, int &end_index) const;
	void register_with_source(SpineSprite *sprite, int start_index, int end_index);
	void unregister_from_source();
	void rebuild_proxy_meshes(int count);
	void clear_proxy_meshes();
	void hide_proxy_meshes();
	void update_proxy();
	void _on_source_tree_exiting();

public:
	SpineSlotRangeProxy();

	void set_source_sprite(const NodePath &path);
	NodePath get_source_sprite() const;

	void set_follow_source_transform(bool enabled);
	bool get_follow_source_transform() const;

	void set_start_slot_name(const String &name);
	String get_start_slot_name() const;

	void set_end_slot_name(const String &name);
	String get_end_slot_name() const;
};
