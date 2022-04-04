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

#include "SpineTransformConstraint.h"

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

SpineTransformConstraint::SpineTransformConstraint() : transform_constraint(NULL) {}
SpineTransformConstraint::~SpineTransformConstraint() {}

void SpineTransformConstraint::update() {
	transform_constraint->update();
}

int SpineTransformConstraint::get_order() {
	return transform_constraint->getOrder();
}

Ref<SpineTransformConstraintData> SpineTransformConstraint::get_data() {
	auto &d = transform_constraint->getData();
	Ref<SpineTransformConstraintData> gd_d(memnew(SpineTransformConstraintData));
	gd_d->set_spine_object(&d);
	return gd_d;
}

Array SpineTransformConstraint::get_bones() {
	auto &bs = transform_constraint->getBones();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for (size_t i = 0; i < bs.size(); ++i) {
		auto b = bs[i];
		if (b == NULL) gd_bs[i] = Ref<SpineBone>(NULL);
		Ref<SpineBone> gd_b(memnew(SpineBone));
		gd_b->set_spine_object(b);
		gd_bs[i] = gd_b;
	}
	return gd_bs;
}

Ref<SpineBone> SpineTransformConstraint::get_target() {
	auto b = transform_constraint->getTarget();
	if (b == NULL) return NULL;
	Ref<SpineBone> gd_b(memnew(SpineBone));
	gd_b->set_spine_object(b);
	return gd_b;
}
void SpineTransformConstraint::set_target(Ref<SpineBone> v) {
	if (v.is_valid()) {
		transform_constraint->setTarget(v->get_spine_object());
	} else {
		transform_constraint->setTarget(NULL);
	}
}

float SpineTransformConstraint::get_mix_rotate() {
	return transform_constraint->getMixRotate();
}
void SpineTransformConstraint::set_mix_rotate(float v) {
	transform_constraint->setMixRotate(v);
}

float SpineTransformConstraint::get_mix_x() {
	return transform_constraint->getMixX();
}
void SpineTransformConstraint::set_mix_x(float v) {
	transform_constraint->setMixX(v);
}

float SpineTransformConstraint::get_mix_y() {
	return transform_constraint->getMixY();
}
void SpineTransformConstraint::set_mix_y(float v) {
	transform_constraint->setMixY(v);
}

float SpineTransformConstraint::get_mix_scale_x() {
	return transform_constraint->getMixScaleX();
}
void SpineTransformConstraint::set_mix_scale_x(float v) {
	transform_constraint->setMixScaleX(v);
}

float SpineTransformConstraint::get_mix_scale_y() {
	return transform_constraint->getMixScaleY();
}
void SpineTransformConstraint::set_mix_scale_y(float v) {
	transform_constraint->setMixScaleY(v);
}

float SpineTransformConstraint::get_mix_shear_y() {
	return transform_constraint->getMixShearY();
}
void SpineTransformConstraint::set_mix_shear_y(float v) {
	transform_constraint->setMixShearY(v);
}

bool SpineTransformConstraint::is_active() {
	return transform_constraint->isActive();
}
void SpineTransformConstraint::set_active(bool v) {
	transform_constraint->setActive(v);
}