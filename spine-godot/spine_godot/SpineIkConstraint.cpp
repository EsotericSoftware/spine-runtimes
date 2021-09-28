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

#include "SpineIkConstraint.h"
#include "SpineBone.h"

void SpineIkConstraint::_bind_methods() {
	// ClassDB::bind_method(D_METHOD("apply"), &SpineIkConstraint::apply);
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

SpineIkConstraint::SpineIkConstraint():ik_constraint(NULL) {}
SpineIkConstraint::~SpineIkConstraint() {}

// void SpineIkConstraint::apply(){
// 	ik_constraint->apply();
// }

void SpineIkConstraint::update(){
	ik_constraint->update();
}

int SpineIkConstraint::get_order(){
	return ik_constraint->getOrder();
}

Ref<SpineIkConstraintData> SpineIkConstraint::get_data(){
	auto &ikc = ik_constraint->getData();
	Ref<SpineIkConstraintData> gd_ikc(memnew(SpineIkConstraintData));
	gd_ikc->set_spine_object(&ikc);
	return gd_ikc;
}

Array SpineIkConstraint::get_bones(){
	auto &bs = ik_constraint->getBones();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for(size_t i=0; i<bs.size(); ++i){
		auto b = bs[i];
		if(b == NULL) gd_bs[i] = Ref<SpineBone>(NULL);
		Ref<SpineBone> gd_b(memnew(SpineBone));
		gd_b->set_spine_object(b);
		gd_bs[i] = gd_b;
	}
	return gd_bs;
}

Ref<SpineBone> SpineIkConstraint::get_target(){
	auto b = ik_constraint->getTarget();
	if(b == NULL) return NULL;
	Ref<SpineBone> gd_b(memnew(SpineBone));
	gd_b->set_spine_object(b);
	return gd_b;
}
void SpineIkConstraint::set_target(Ref<SpineBone> v){
	if(v.is_valid()){
		ik_constraint->setTarget(v->get_spine_object());
	} else{
		ik_constraint->setTarget(NULL);
	}
}

int SpineIkConstraint::get_bend_direction(){
	return ik_constraint->getBendDirection();
}
void SpineIkConstraint::set_bend_direction(int v){
	ik_constraint->setBendDirection(v);
}

bool SpineIkConstraint::get_compress(){
	return ik_constraint->getCompress();
}
void SpineIkConstraint::set_compress(bool v){
	ik_constraint->setCompress(v);
}

bool SpineIkConstraint::get_stretch(){
	return ik_constraint->getStretch();
}
void SpineIkConstraint::set_stretch(bool v){
	ik_constraint->setStretch(v);
}

float SpineIkConstraint::get_mix(){
	return ik_constraint->getMix();
}
void SpineIkConstraint::set_mix(float v){
	ik_constraint->setMix(v);
}

float SpineIkConstraint::get_softness(){
	return ik_constraint->getSoftness();
}
void SpineIkConstraint::set_softness(float v){
	ik_constraint->setSoftness(v);
}

bool SpineIkConstraint::is_active(){
	return ik_constraint->isActive();
}
void SpineIkConstraint::set_active(bool v){
	ik_constraint->setActive(v);
}