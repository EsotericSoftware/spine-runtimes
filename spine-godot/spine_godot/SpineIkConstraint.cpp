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

#include "SpineIkConstraint.h"
#include "SpineBone.h"
#include "SpineCommon.h"
#include "SpineSprite.h"

void SpineIkConstraint::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update"), &SpineIkConstraint::update);
	ClassDB::bind_method(D_METHOD("get_order"), &SpineIkConstraint::get_order);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineIkConstraint::get_data);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineIkConstraint::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpineIkConstraint::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "v"), &SpineIkConstraint::set_target);
	ClassDB::bind_method(D_METHOD("get_bend_direction"), &SpineIkConstraint::get_bend_direction);
	ClassDB::bind_method(D_METHOD("set_bend_direction", "v"), &SpineIkConstraint::set_bend_direction);
	ClassDB::bind_method(D_METHOD("get_compress"), &SpineIkConstraint::get_compress);
	ClassDB::bind_method(D_METHOD("set_compress", "v"), &SpineIkConstraint::set_compress);
	ClassDB::bind_method(D_METHOD("get_stretch"), &SpineIkConstraint::get_stretch);
	ClassDB::bind_method(D_METHOD("set_stretch", "v"), &SpineIkConstraint::set_stretch);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpineIkConstraint::get_mix);
	ClassDB::bind_method(D_METHOD("set_mix", "v"), &SpineIkConstraint::set_mix);
	ClassDB::bind_method(D_METHOD("get_softness"), &SpineIkConstraint::get_softness);
	ClassDB::bind_method(D_METHOD("set_softness", "v"), &SpineIkConstraint::set_softness);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineIkConstraint::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineIkConstraint::set_active);
}

void SpineIkConstraint::update() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update(spine::Physics_Update);
}

int SpineIkConstraint::get_order() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getOrder();
}

Ref<SpineIkConstraintData> SpineIkConstraint::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &ik_constraint_data = get_spine_object()->getData();
	Ref<SpineIkConstraintData> ik_constraint_data_ref(memnew(SpineIkConstraintData));
	ik_constraint_data_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), &ik_constraint_data);
	return ik_constraint_data_ref;
}

Array SpineIkConstraint::get_bones() {
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

Ref<SpineBone> SpineIkConstraint::get_target() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto target = get_spine_object()->getTarget();
	if (!target) return nullptr;
	Ref<SpineBone> target_ref(memnew(SpineBone));
	target_ref->set_spine_object(get_spine_owner(), target);
	return target_ref;
}

void SpineIkConstraint::set_target(Ref<SpineBone> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTarget(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

int SpineIkConstraint::get_bend_direction() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getBendDirection();
}

void SpineIkConstraint::set_bend_direction(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setBendDirection(v);
}

bool SpineIkConstraint::get_compress() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getCompress();
}

void SpineIkConstraint::set_compress(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setCompress(v);
}

bool SpineIkConstraint::get_stretch() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->getStretch();
}

void SpineIkConstraint::set_stretch(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setStretch(v);
}

float SpineIkConstraint::get_mix() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMix();
}
void SpineIkConstraint::set_mix(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMix(v);
}

float SpineIkConstraint::get_softness() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getSoftness();
}

void SpineIkConstraint::set_softness(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSoftness(v);
}

bool SpineIkConstraint::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}

void SpineIkConstraint::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}
