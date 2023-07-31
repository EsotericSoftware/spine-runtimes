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

#include "SpineIkConstraintData.h"
#include "SpineCommon.h"

void SpineIkConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_bones"), &SpineIkConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpineIkConstraintData::get_target);
	ClassDB::bind_method(D_METHOD("set_target", "v"), &SpineIkConstraintData::set_target);
	ClassDB::bind_method(D_METHOD("get_bend_direction"), &SpineIkConstraintData::get_bend_direction);
	ClassDB::bind_method(D_METHOD("set_bend_direction", "v"), &SpineIkConstraintData::set_bend_direction);
	ClassDB::bind_method(D_METHOD("get_compress"), &SpineIkConstraintData::get_compress);
	ClassDB::bind_method(D_METHOD("set_compress", "v"), &SpineIkConstraintData::set_compress);
	ClassDB::bind_method(D_METHOD("get_stretch"), &SpineIkConstraintData::get_stretch);
	ClassDB::bind_method(D_METHOD("set_stretch", "v"), &SpineIkConstraintData::set_stretch);
	ClassDB::bind_method(D_METHOD("get_uniform"), &SpineIkConstraintData::get_uniform);
	ClassDB::bind_method(D_METHOD("set_uniform", "v"), &SpineIkConstraintData::set_uniform);
	ClassDB::bind_method(D_METHOD("get_mix"), &SpineIkConstraintData::get_mix);
	ClassDB::bind_method(D_METHOD("set_mix", "v"), &SpineIkConstraintData::set_mix);
	ClassDB::bind_method(D_METHOD("get_softness"), &SpineIkConstraintData::get_softness);
	ClassDB::bind_method(D_METHOD("set_softness", "v"), &SpineIkConstraintData::set_softness);
}

Array SpineIkConstraintData::get_bones() {
	Array result;
	SPINE_CHECK(get_spine_object(), result)
	auto bones = get_spine_constraint_data()->getBones();
	result.resize((int) bones.size());
	for (int i = 0; i < bones.size(); ++i) {
		Ref<SpineBoneData> bone_ref(memnew(SpineBoneData));
		bone_ref->set_spine_object(get_spine_owner(), bones[i]);
		result[i] = bone_ref;
	}
	return result;
}

Ref<SpineBoneData> SpineIkConstraintData::get_target() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto target = get_spine_constraint_data()->getTarget();
	if (!target) return nullptr;
	Ref<SpineBoneData> target_ref(memnew(SpineBoneData));
	target_ref->set_spine_object(get_spine_owner(), target);
	return target_ref;
}

void SpineIkConstraintData::set_target(Ref<SpineBoneData> v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setTarget(v.is_valid() && v->get_spine_object() ? v->get_spine_object() : nullptr);
}

int SpineIkConstraintData::get_bend_direction() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_constraint_data()->getBendDirection();
}

void SpineIkConstraintData::set_bend_direction(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setBendDirection(v);
}

bool SpineIkConstraintData::get_compress() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_constraint_data()->getCompress();
}

void SpineIkConstraintData::set_compress(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setCompress(v);
}

bool SpineIkConstraintData::get_stretch() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_constraint_data()->getStretch();
}

void SpineIkConstraintData::set_stretch(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setStretch(v);
}

bool SpineIkConstraintData::get_uniform() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_constraint_data()->getUniform();
}

void SpineIkConstraintData::set_uniform(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setUniform(v);
}

float SpineIkConstraintData::get_mix() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_constraint_data()->getMix();
}

void SpineIkConstraintData::set_mix(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setMix(v);
}

float SpineIkConstraintData::get_softness() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_constraint_data()->getSoftness();
}

void SpineIkConstraintData::set_softness(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_constraint_data()->setSoftness(v);
}
