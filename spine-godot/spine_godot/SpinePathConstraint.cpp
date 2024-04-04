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

#include "SpinePathConstraint.h"
#include "SpineBone.h"
#include "SpineCommon.h"
#include "SpineSprite.h"

void SpinePathConstraint::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update"), &SpinePathConstraint::update);
	ClassDB::bind_method(D_METHOD("get_order"), &SpinePathConstraint::get_order);
	ClassDB::bind_method(D_METHOD("get_position"), &SpinePathConstraint::get_position);
	ClassDB::bind_method(D_METHOD("set_position", "v"), &SpinePathConstraint::set_position);
	ClassDB::bind_method(D_METHOD("get_spacing"), &SpinePathConstraint::get_spacing);
	ClassDB::bind_method(D_METHOD("set_spacing", "v"), &SpinePathConstraint::set_spacing);
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpinePathConstraint::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("set_mix_rotate", "v"), &SpinePathConstraint::set_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpinePathConstraint::get_mix_x);
	ClassDB::bind_method(D_METHOD("set_mix_x", "v"), &SpinePathConstraint::set_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpinePathConstraint::get_mix_y);
	ClassDB::bind_method(D_METHOD("set_mix_y", "v"), &SpinePathConstraint::set_mix_y);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpinePathConstraint::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpinePathConstraint::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "v"), &SpinePathConstraint::set_target);
	ClassDB::bind_method(D_METHOD("get_data"), &SpinePathConstraint::get_data);
	ClassDB::bind_method(D_METHOD("is_active"), &SpinePathConstraint::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpinePathConstraint::set_active);
}

void SpinePathConstraint::update() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update(spine::Physics_Update);
}

int SpinePathConstraint::get_order() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getOrder();
}

float SpinePathConstraint::get_position() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getPosition();
}

void SpinePathConstraint::set_position(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setPosition(v);
}

float SpinePathConstraint::get_spacing() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getSpacing();
}

void SpinePathConstraint::set_spacing(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSpacing(v);
}

float SpinePathConstraint::get_mix_rotate() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixRotate();
}

void SpinePathConstraint::set_mix_rotate(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixRotate(v);
}

float SpinePathConstraint::get_mix_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixX();
}

void SpinePathConstraint::set_mix_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixX(v);
}

float SpinePathConstraint::get_mix_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixY();
}

void SpinePathConstraint::set_mix_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixY(v);
}

Array SpinePathConstraint::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto &bones = get_spine_object()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		auto bone = bones[i];
		Ref<SpineBone> bone_ref(memnew(SpineBone));
		bone_ref->set_spine_object(get_spine_owner(), bone);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineSlot> SpinePathConstraint::get_target() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto target = get_spine_object()->getTarget();
	if (!target) return nullptr;
	Ref<SpineSlot> target_ref(memnew(SpineSlot));
	target_ref->set_spine_object(get_spine_owner(), target);
	return target_ref;
}

void SpinePathConstraint::set_target(Ref<SpineSlot> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTarget(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

Ref<SpinePathConstraintData> SpinePathConstraint::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &data = get_spine_object()->getData();
	Ref<SpinePathConstraintData> data_ref(memnew(SpinePathConstraintData));
	data_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), &data);
	return data_ref;
}

bool SpinePathConstraint::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}

void SpinePathConstraint::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}
