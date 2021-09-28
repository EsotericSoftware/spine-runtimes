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

#include "SpineIkConstraintData.h"

void SpineIkConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_all_bone_data"), &SpineIkConstraintData::get_bones);
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

SpineIkConstraintData::SpineIkConstraintData() {}
SpineIkConstraintData::~SpineIkConstraintData() {}

Array SpineIkConstraintData::get_bones(){
	auto bs = get_spine_data()->getBones();
	Array gd_bs;
	gd_bs.resize(bs.size());
	for(size_t i=0; i < bs.size(); ++i){
		if(bs[i] == NULL) gd_bs[i] = Ref<SpineBoneData>(NULL);
		else {
			Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
			gd_b->set_spine_object(bs[i]);
			gd_bs[i] = gd_b;
		}
	}
	return gd_bs;
}

Ref<SpineBoneData> SpineIkConstraintData::get_target(){
	auto b = get_spine_data()->getTarget();
	if(b == NULL) return NULL;
	Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
	gd_b->set_spine_object(b);
	return gd_b;
}
void SpineIkConstraintData::set_target(Ref<SpineBoneData> v){
	if(v.is_valid()){
		get_spine_data()->setTarget(v->get_spine_object());
	}else{
		get_spine_data()->setTarget(NULL);
	}
}

int SpineIkConstraintData::get_bend_direction(){
	return get_spine_data()->getBendDirection();
}
void SpineIkConstraintData::set_bend_direction(int v){
	get_spine_data()->setBendDirection(v);
}

bool SpineIkConstraintData::get_compress(){
	return get_spine_data()->getCompress();
}
void SpineIkConstraintData::set_compress(bool v){
	get_spine_data()->setCompress(v);
}

bool SpineIkConstraintData::get_stretch(){
	return get_spine_data()->getStretch();
}
void SpineIkConstraintData::set_stretch(bool v){
	get_spine_data()->setStretch(v);
}

bool SpineIkConstraintData::get_uniform(){
	return get_spine_data()->getUniform();
}
void SpineIkConstraintData::set_uniform(bool v){
	get_spine_data()->setUniform(v);
}

float SpineIkConstraintData::get_mix(){
	return get_spine_data()->getMix();
}
void SpineIkConstraintData::set_mix(float v){
	get_spine_data()->setMix(v);
}

float SpineIkConstraintData::get_softness(){
	return get_spine_data()->getSoftness();
}
void SpineIkConstraintData::set_softness(float v){
	get_spine_data()->setSoftness(v);
}