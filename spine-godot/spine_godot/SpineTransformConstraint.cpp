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

#include "SpineTransformConstraint.h"
#include "SpineCommon.h"
#include "SpineSprite.h"

void SpineTransformConstraint::_bind_methods() {
	ClassDB::bind_method(D_METHOD("update"), &SpineTransformConstraint::update);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineTransformConstraint::get_data);
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineTransformConstraint::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpineTransformConstraint::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "v"), &SpineTransformConstraint::set_target);
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpineTransformConstraint::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("set_mix_rotate", "v"), &SpineTransformConstraint::set_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpineTransformConstraint::get_mix_x);
	ClassDB::bind_method(D_METHOD("set_mix_x", "v"), &SpineTransformConstraint::set_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpineTransformConstraint::get_mix_y);
	ClassDB::bind_method(D_METHOD("set_mix_y", "v"), &SpineTransformConstraint::set_mix_y);
	ClassDB::bind_method(D_METHOD("get_mix_scale_x"), &SpineTransformConstraint::get_mix_scale_x);
	ClassDB::bind_method(D_METHOD("set_mix_scale_x", "v"), &SpineTransformConstraint::set_mix_scale_x);
	ClassDB::bind_method(D_METHOD("get_mix_scale_y"), &SpineTransformConstraint::get_mix_scale_y);
	ClassDB::bind_method(D_METHOD("set_mix_scale_y", "v"), &SpineTransformConstraint::set_mix_scale_y);
	ClassDB::bind_method(D_METHOD("get_mix_shear_y"), &SpineTransformConstraint::get_mix_shear_y);
	ClassDB::bind_method(D_METHOD("set_mix_shear_y", "v"), &SpineTransformConstraint::set_mix_shear_y);
	ClassDB::bind_method(D_METHOD("is_active"), &SpineTransformConstraint::is_active);
	ClassDB::bind_method(D_METHOD("set_active", "v"), &SpineTransformConstraint::set_active);
}

void SpineTransformConstraint::update() {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->update(spine::Physics_Update);
}

int SpineTransformConstraint::get_order() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getOrder();
}

Ref<SpineTransformConstraintData> SpineTransformConstraint::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto &data = get_spine_object()->getData();
	Ref<SpineTransformConstraintData> data_ref(memnew(SpineTransformConstraintData));
	data_ref->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), &data);
	return data_ref;
}

Array SpineTransformConstraint::get_bones() {
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

Ref<SpineBone> SpineTransformConstraint::get_target() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto target = get_spine_object()->getTarget();
	if (!target) return nullptr;
	Ref<SpineBone> target_ref(memnew(SpineBone));
	target_ref->set_spine_object(get_spine_owner(), target);
	return target_ref;
}

void SpineTransformConstraint::set_target(Ref<SpineBone> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setTarget(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

float SpineTransformConstraint::get_mix_rotate() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixRotate();
}

void SpineTransformConstraint::set_mix_rotate(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixRotate(v);
}

float SpineTransformConstraint::get_mix_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixX();
}

void SpineTransformConstraint::set_mix_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixX(v);
}

float SpineTransformConstraint::get_mix_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixY();
}

void SpineTransformConstraint::set_mix_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixY(v);
}

float SpineTransformConstraint::get_mix_scale_x() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixScaleX();
}

void SpineTransformConstraint::set_mix_scale_x(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixScaleX(v);
}

float SpineTransformConstraint::get_mix_scale_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixScaleY();
}

void SpineTransformConstraint::set_mix_scale_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixScaleY(v);
}

float SpineTransformConstraint::get_mix_shear_y() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getMixShearY();
}

void SpineTransformConstraint::set_mix_shear_y(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setMixShearY(v);
}

bool SpineTransformConstraint::is_active() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isActive();
}

void SpineTransformConstraint::set_active(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setActive(v);
}
