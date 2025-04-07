/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#include "SpineEvent.h"
#include "SpineCommon.h"
#include "SpineSprite.h"

void SpineEvent::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_data"), &SpineEvent::get_data);
	ClassDB::bind_method(D_METHOD("get_time"), &SpineEvent::get_time);
	ClassDB::bind_method(D_METHOD("get_int_value"), &SpineEvent::get_int_value);
	ClassDB::bind_method(D_METHOD("set_int_value", "v"), &SpineEvent::set_int_value);
	ClassDB::bind_method(D_METHOD("get_float_value"), &SpineEvent::get_float_value);
	ClassDB::bind_method(D_METHOD("set_float_value", "v"), &SpineEvent::set_float_value);
	ClassDB::bind_method(D_METHOD("get_string_value"), &SpineEvent::get_string_value);
	ClassDB::bind_method(D_METHOD("set_string_value", "v"), &SpineEvent::set_string_value);
	ClassDB::bind_method(D_METHOD("get_volume"), &SpineEvent::get_volume);
	ClassDB::bind_method(D_METHOD("set_volume", "v"), &SpineEvent::set_volume);
	ClassDB::bind_method(D_METHOD("get_balance"), &SpineEvent::get_balance);
	ClassDB::bind_method(D_METHOD("set_balance", "v"), &SpineEvent::set_balance);
}

Ref<SpineEventData> SpineEvent::get_data() {
	SPINE_CHECK(get_spine_object(), nullptr)
	Ref<SpineEventData> event_data(memnew(SpineEventData));
	event_data->set_spine_object(*get_spine_owner()->get_skeleton_data_res(), (spine::EventData *) &get_spine_object()->getData());
	return event_data;
}

float SpineEvent::get_time() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getTime();
}

int SpineEvent::get_int_value() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getIntValue();
}

void SpineEvent::set_int_value(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setIntValue(v);
}

float SpineEvent::get_float_value() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getFloatValue();
}

void SpineEvent::set_float_value(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setFloatValue(v);
}

String SpineEvent::get_string_value() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getStringValue().buffer();
}

void SpineEvent::set_string_value(const String &v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setStringValue(spine::String(v.utf8()));
}

float SpineEvent::get_volume() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getVolume();
}

void SpineEvent::set_volume(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setVolume(v);
}

float SpineEvent::get_balance() {
	SPINE_CHECK(get_spine_object(), 0)
	return get_spine_object()->getBalance();
}

void SpineEvent::set_balance(float v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setBalance(v);
}
