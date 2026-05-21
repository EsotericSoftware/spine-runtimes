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

#include "SpineSpriteRenderSeparator.h"

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/core/memory.hpp>
#else
#include "core/os/memory.h"
#endif

void SpineSpriteRenderSeparator::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_source_sprite", "node_path"), &SpineSpriteRenderSeparator::set_source_sprite);
	ClassDB::bind_method(D_METHOD("get_source_sprite"), &SpineSpriteRenderSeparator::get_source_sprite);
	ClassDB::bind_method(D_METHOD("set_separator_slot_names", "slot_names"), &SpineSpriteRenderSeparator::set_separator_slot_names);
	ClassDB::bind_method(D_METHOD("get_separator_slot_names"), &SpineSpriteRenderSeparator::get_separator_slot_names);

	ADD_PROPERTY(PropertyInfo(Variant::NODE_PATH, "source_sprite", PROPERTY_HINT_NODE_PATH_VALID_TYPES, "SpineSprite"), "set_source_sprite",
				 "get_source_sprite");
	ADD_PROPERTY(PropertyInfo(Variant::PACKED_STRING_ARRAY, "separator_slot_names"), "set_separator_slot_names", "get_separator_slot_names");
}

SpineSpriteRenderSeparator::SpineSpriteRenderSeparator() : proxies_dirty(true) {
}

void SpineSpriteRenderSeparator::_notification(int what) {
	switch (what) {
		case NOTIFICATION_ENTER_TREE: {
			set_process_internal(true);
			break;
		}
		case NOTIFICATION_INTERNAL_PROCESS: {
			if (proxies_dirty && rebuild_proxies()) proxies_dirty = false;
			break;
		}
		default:
			break;
	}
}

SpineSprite *SpineSpriteRenderSeparator::resolve_source_sprite() const {
	if (source_sprite_path.is_empty()) return nullptr;
	if (!is_inside_tree()) return nullptr;
	Node *node = get_node_or_null(source_sprite_path);
	return Object::cast_to<SpineSprite>(node);
}

void SpineSpriteRenderSeparator::clear_proxies() {
	for (int i = 0; i < managed_proxies.size(); i++) {
		SpineSlotRangeProxy *proxy = managed_proxies[i];
		if (proxy) {
			remove_child(proxy);
			memdelete(proxy);
		}
	}
	managed_proxies.clear();
}

bool SpineSpriteRenderSeparator::rebuild_proxies() {
	SpineSprite *sprite = resolve_source_sprite();
	if (!sprite) return false;
	Ref<SpineSkeletonDataResource> data_res = sprite->get_skeleton_data_res();
	if (!data_res.is_valid() || !data_res->is_skeleton_data_loaded()) return false;

#ifdef SPINE_GODOT_EXTENSION
	PackedStringArray slot_names;
#else
	Vector<String> slot_names;
#endif
	data_res->get_slot_names(slot_names);
	int slot_count = (int) slot_names.size();
	if (slot_count <= 0) return false;

	clear_proxies();

	// Resolve separator slot names to indices: sorted, de-duplicated, and keeping
	// only interior boundaries (a boundary at slot 0 or past the last slot would
	// yield an empty range).
	Vector<int> boundaries;
	for (int i = 0; i < (int) separator_slot_names.size(); i++) {
		String separator_name = separator_slot_names[i];
		int index = -1;
		for (int s = 0; s < slot_count; s++) {
			if (slot_names[s] == separator_name) {
				index = s;
				break;
			}
		}
		if (index <= 0 || index >= slot_count) continue;
		if (boundaries.find(index) == -1) boundaries.push_back(index);
	}
	boundaries.sort();
	int boundary_count = (int) boundaries.size();

	// Build N+1 contiguous slot ranges and one SpineSlotRangeProxy child per range.
	int range_start = 0;
	for (int b = 0; b <= boundary_count; b++) {
		int range_end = (b < boundary_count) ? boundaries[b] - 1 : slot_count - 1;

		SpineSlotRangeProxy *proxy = memnew(SpineSlotRangeProxy);
		add_child(proxy);
		proxy->set_source_sprite(proxy->get_path_to(sprite));
		// An empty start/end name means "first slot" / "last slot", which keeps
		// the boundary proxies correct even if the slot count changes.
		proxy->set_start_slot_name(range_start == 0 ? String() : slot_names[range_start]);
		proxy->set_end_slot_name(range_end >= slot_count - 1 ? String() : slot_names[range_end]);
		managed_proxies.push_back(proxy);

		if (b < boundary_count) range_start = boundaries[b];
	}
	return true;
}

void SpineSpriteRenderSeparator::set_source_sprite(const NodePath &path) {
	source_sprite_path = path;
	clear_proxies();
	proxies_dirty = true;
}

NodePath SpineSpriteRenderSeparator::get_source_sprite() const {
	return source_sprite_path;
}

void SpineSpriteRenderSeparator::set_separator_slot_names(const PackedStringArray &names) {
	separator_slot_names = names;
	clear_proxies();
	proxies_dirty = true;
}

PackedStringArray SpineSpriteRenderSeparator::get_separator_slot_names() const {
	return separator_slot_names;
}
