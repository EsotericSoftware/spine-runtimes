/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include "SpineCollisionShapeProxy.h"

#include "SpineSprite.h"

void SpineCollisionShapeProxy::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_spine_sprite_path"), &SpineCollisionShapeProxy::get_spine_sprite_path);
	ClassDB::bind_method(D_METHOD("set_spine_sprite_path", "v"), &SpineCollisionShapeProxy::set_spine_sprite_path);

	ClassDB::bind_method(D_METHOD("get_slot"), &SpineCollisionShapeProxy::get_slot);
	ClassDB::bind_method(D_METHOD("set_slot", "v"), &SpineCollisionShapeProxy::set_slot);

	ClassDB::bind_method(D_METHOD("get_sync_transform"), &SpineCollisionShapeProxy::get_sync_transform);
	ClassDB::bind_method(D_METHOD("set_sync_transform", "v"), &SpineCollisionShapeProxy::set_sync_transform);

	ADD_PROPERTY(PropertyInfo(Variant::NODE_PATH, "spine_sprite_path"), "set_spine_sprite_path", "get_spine_sprite_path");
	ADD_PROPERTY(PropertyInfo(Variant::BOOL, "sync_transform"), "set_sync_transform", "get_sync_transform");
}

SpineCollisionShapeProxy::SpineCollisionShapeProxy() : sync_transform(true) {
}

SpineCollisionShapeProxy::~SpineCollisionShapeProxy() {
}

void SpineCollisionShapeProxy::_notification(int p_what) {
	switch (p_what) {
		case NOTIFICATION_READY: {
			set_process_internal(true);
		} break;
		case NOTIFICATION_INTERNAL_PROCESS: {
			if (!disabled) {
				if (sync_transform) _sync_transform(get_spine_sprite());
				_update_polygon_from_spine_sprite(get_spine_sprite());
				if (is_visible()) update();
			}
		} break;
	}
}

SpineSprite *SpineCollisionShapeProxy::get_spine_sprite() const {
	return (SpineSprite *) get_node_or_null(spine_sprite_path);
}

NodePath SpineCollisionShapeProxy::get_spine_sprite_path() {
	return spine_sprite_path;
}

void SpineCollisionShapeProxy::set_spine_sprite_path(NodePath v) {
	spine_sprite_path = v;

	_update_polygon_from_spine_sprite(get_spine_sprite());
}

String SpineCollisionShapeProxy::get_slot() const {
	return slot;
}

void SpineCollisionShapeProxy::set_slot(const String &v) {
	slot = v;
	_update_polygon_from_spine_sprite(get_spine_sprite());
}

void SpineCollisionShapeProxy::_update_polygon_from_spine_sprite(SpineSprite *sprite) {
	_clear_polygon();
	if (sprite == nullptr || slot.empty()) {
		return;
	}

	if (!sprite->get_skeleton().is_valid()) {
		return;
	}

	auto sk = sprite->get_skeleton()->get_spine_object();

	spine::Vector<float> vertices;

	spine::Slot *s = sk->findSlot(spine::String(slot.utf8()));
	if (!s) {
		return;
	}
	spine::Attachment *attachment = s->getAttachment();
	if (!attachment) {
		return;
	}

	if (attachment->getRTTI().isExactly(spine::BoundingBoxAttachment::rtti)) {
		auto *box = (spine::BoundingBoxAttachment *) attachment;

		vertices.setSize(box->getWorldVerticesLength(), 0);
		box->computeWorldVertices(*s, vertices);
	} else {
		return;
	}

	polygon.resize(vertices.size() / 2);
	for (size_t j = 0; j < vertices.size(); j += 2) {
		polygon.set(j / 2, Vector2(vertices[j], -vertices[j + 1]));
	}

	set_polygon(polygon);
}

void SpineCollisionShapeProxy::_clear_polygon() {
	polygon.clear();
	set_polygon(polygon);
}

void SpineCollisionShapeProxy::_sync_transform(SpineSprite *sprite) {
	if (sprite == nullptr) return;
	set_global_transform(sprite->get_global_transform());
}

bool SpineCollisionShapeProxy::get_sync_transform() {
	return sync_transform;
}

void SpineCollisionShapeProxy::set_sync_transform(bool v) {
	sync_transform = v;
}

void SpineCollisionShapeProxy::_get_property_list(List<PropertyInfo> *p_list) const {
	PropertyInfo p;
	Vector<String> res;

	p.name = "slot";
	p.type = Variant::STRING;
	_get_slot_list(res);
	if (res.empty()) res.push_back("No Slot");
	p.hint_string = String(",").join(res);
	p.hint = PROPERTY_HINT_ENUM;
	p_list->push_back(p);
}

bool SpineCollisionShapeProxy::_get(const StringName &p_property, Variant &r_value) const {
	if (p_property == "slot") {
		r_value = get_slot();
		return true;
	}
	return false;
}

bool SpineCollisionShapeProxy::_set(const StringName &p_property, const Variant &p_value) {
	if (p_property == "slot") {
		set_slot(p_value);
		return true;
	}
	return false;
}

void SpineCollisionShapeProxy::_get_slot_list(Vector<String> &res) const {
	if (get_spine_sprite() == nullptr) {
		return;
	}

	auto sprite = get_spine_sprite();
	if (!sprite->get_skeleton().is_valid()) {
		return;
	}

	auto slots = sprite->get_skeleton()->get_slots();
	res.resize(slots.size());
	for (size_t i = 0; i < res.size(); ++i) {
		auto slot = (Ref<SpineSlot>) slots[i];
		if (slot.is_valid())
			res.set(i, slot->get_data()->get_slot_name());
	}
}
