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

#include "SpineSlot.h"

#include "SpineBone.h"
#include "SpineSkeleton.h"


void SpineSlot::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_to_setup_pos"), &SpineSlot::set_to_setup_pos);
	ClassDB::bind_method(D_METHOD("get_data"), &SpineSlot::get_data);
	ClassDB::bind_method(D_METHOD("get_bone"), &SpineSlot::get_bone);
	ClassDB::bind_method(D_METHOD("get_skeleton"), &SpineSlot::get_skeleton);
	ClassDB::bind_method(D_METHOD("get_color"), &SpineSlot::get_color);
	ClassDB::bind_method(D_METHOD("set_color"), &SpineSlot::set_color);
	ClassDB::bind_method(D_METHOD("get_dark_color"), &SpineSlot::get_dark_color);
	ClassDB::bind_method(D_METHOD("set_dark_color", "v"), &SpineSlot::set_dark_color);
	ClassDB::bind_method(D_METHOD("has_dark_color"), &SpineSlot::has_dark_color);
	ClassDB::bind_method(D_METHOD("get_attachment"), &SpineSlot::get_attachment);
	ClassDB::bind_method(D_METHOD("set_attachment", "v"), &SpineSlot::set_attachment);
	ClassDB::bind_method(D_METHOD("get_attachment_state"), &SpineSlot::get_attachment_state);
	ClassDB::bind_method(D_METHOD("set_attachment_state", "v"), &SpineSlot::set_attachment_state);
	ClassDB::bind_method(D_METHOD("get_deform"), &SpineSlot::get_deform);
	ClassDB::bind_method(D_METHOD("set_deform", "v"), &SpineSlot::set_deform);
}

SpineSlot::SpineSlot():slot(NULL) {}
SpineSlot::~SpineSlot() {}

void SpineSlot::set_to_setup_pos(){
	slot->setToSetupPose();
}

Ref<SpineSlotData> SpineSlot::get_data(){
	auto &sd = slot->getData();
	Ref<SpineSlotData> gd_sd(memnew(SpineSlotData));
	gd_sd->set_spine_object(&sd);
	return gd_sd;
}

Ref<SpineBone> SpineSlot::get_bone(){
	auto &b = slot->getBone();
	Ref<SpineBone> gd_b(memnew(SpineBone));
	gd_b->set_spine_object(&b);
	return gd_b;
}

Ref<SpineSkeleton> SpineSlot::get_skeleton(){
	auto &s = slot->getSkeleton();
	Ref<SpineSkeleton> gd_s(memnew(SpineSkeleton));
	gd_s->set_spine_object(&s);
	return gd_s;
}

Color SpineSlot::get_color(){
	auto &c = slot->getColor();
	return Color(c.r, c.g, c.b, c.a);
}
void SpineSlot::set_color(Color v){
	auto &c = slot->getColor();
	c.set(v.r, v.g, v.b, v.a);
}

Color SpineSlot::get_dark_color(){
	auto &c = slot->getDarkColor();
	return Color(c.r, c.g, c.b, c.a);
}
void SpineSlot::set_dark_color(Color v){
	auto &c = slot->getDarkColor();
	c.set(v.r, v.g, v.b, v.a);
}

bool SpineSlot::has_dark_color(){
	return slot->hasDarkColor();
}

Ref<SpineAttachment> SpineSlot::get_attachment(){
	auto a = slot->getAttachment();
	if(a == NULL) return NULL;
	Ref<SpineAttachment> gd_a(memnew(SpineAttachment));
	gd_a->set_spine_object(a);
	return gd_a;
}
void SpineSlot::set_attachment(Ref<SpineAttachment> v){
	if(v.is_valid()){
		slot->setAttachment(v->get_spine_object());
	}else{
		slot->setAttachment(NULL);
	}
}

int SpineSlot::get_attachment_state(){
	return slot->getAttachmentState();
}
void SpineSlot::set_attachment_state(int v){
	slot->setAttachmentState(v);
}

float SpineSlot::get_attachment_time(){
	return slot->getAttachmentTime();
}
void SpineSlot::set_attachment_time(float v){
	slot->setAttachmentTime(v);
}

Array SpineSlot::get_deform(){
	auto &ds = slot->getDeform();
	Array gd_ds;
	gd_ds.resize(ds.size());
	for(size_t i=0; i < ds.size(); ++i){
		gd_ds[i] = ds[i];
	}
	return gd_ds;
}
void SpineSlot::set_deform(Array gd_ds){
	auto &ds = slot->getDeform();
	ds.setSize(gd_ds.size(), 0);
	for(size_t i=0; i < gd_ds.size(); ++i){
		ds[i] = gd_ds[i];
	}
}