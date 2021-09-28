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

#include "SpineTransformConstraintData.h"

void SpineTransformConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_all_bone_data"), &SpineTransformConstraintData::get_bones);
	ClassDB::bind_method(D_METHOD("get_target"), &SpineTransformConstraintData::get_target);
	ClassDB::bind_method(D_METHOD("get_mix_rotate"), &SpineTransformConstraintData::get_mix_rotate);
	ClassDB::bind_method(D_METHOD("get_mix_x"), &SpineTransformConstraintData::get_mix_x);
	ClassDB::bind_method(D_METHOD("get_mix_y"), &SpineTransformConstraintData::get_mix_y);
	ClassDB::bind_method(D_METHOD("get_mix_scale_x"), &SpineTransformConstraintData::get_mix_scale_x);
	ClassDB::bind_method(D_METHOD("get_mix_scale_y"), &SpineTransformConstraintData::get_mix_scale_y);
	ClassDB::bind_method(D_METHOD("get_mix_shear_y"), &SpineTransformConstraintData::get_mix_shear_y);
	ClassDB::bind_method(D_METHOD("get_offset_rotation"), &SpineTransformConstraintData::get_offset_rotation);
	ClassDB::bind_method(D_METHOD("get_offset_x"), &SpineTransformConstraintData::get_offset_x);
	ClassDB::bind_method(D_METHOD("get_offset_y"), &SpineTransformConstraintData::get_offset_y);
	ClassDB::bind_method(D_METHOD("get_offset_scale_x"), &SpineTransformConstraintData::get_offset_scale_x);
	ClassDB::bind_method(D_METHOD("get_offset_scale_y"), &SpineTransformConstraintData::get_offset_scale_y);
	ClassDB::bind_method(D_METHOD("get_offset_shear_y"), &SpineTransformConstraintData::get_offset_shear_y);
	ClassDB::bind_method(D_METHOD("is_relative"), &SpineTransformConstraintData::is_relative);
	ClassDB::bind_method(D_METHOD("is_local"), &SpineTransformConstraintData::is_local);
}

SpineTransformConstraintData::SpineTransformConstraintData() {}
SpineTransformConstraintData::~SpineTransformConstraintData() {}

Array SpineTransformConstraintData::get_bones(){
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
Ref<SpineBoneData> SpineTransformConstraintData::get_target(){
	auto b = get_spine_data()->getTarget();
	if(b == NULL) return NULL;
	Ref<SpineBoneData> gd_b(memnew(SpineBoneData));
	gd_b->set_spine_object(b);
	return gd_b;
}
float SpineTransformConstraintData::get_mix_rotate(){
	return get_spine_data()->getMixRotate();
}
float SpineTransformConstraintData::get_mix_x(){
	return get_spine_data()->getMixX();
}
float SpineTransformConstraintData::get_mix_y(){
	return get_spine_data()->getMixY();
}
float SpineTransformConstraintData::get_mix_scale_x(){
	return get_spine_data()->getMixScaleX();
}
float SpineTransformConstraintData::get_mix_scale_y(){
	return get_spine_data()->getMixScaleY();
}
float SpineTransformConstraintData::get_mix_shear_y(){
	return get_spine_data()->getMixShearY();
}

float SpineTransformConstraintData::get_offset_rotation(){
	return get_spine_data()->getOffsetRotation();
}
float SpineTransformConstraintData::get_offset_x(){
	return get_spine_data()->getOffsetX();
}
float SpineTransformConstraintData::get_offset_y(){
	return get_spine_data()->getOffsetY();
}
float SpineTransformConstraintData::get_offset_scale_x(){
	return get_spine_data()->getOffsetScaleX();
}
float SpineTransformConstraintData::get_offset_scale_y(){
	return get_spine_data()->getOffsetScaleY();
}
float SpineTransformConstraintData::get_offset_shear_y(){
	return get_spine_data()->getOffsetShearY();
}

bool SpineTransformConstraintData::is_relative(){
	return get_spine_data()->isRelative();
}
bool SpineTransformConstraintData::is_local(){
	return get_spine_data()->isLocal();
}