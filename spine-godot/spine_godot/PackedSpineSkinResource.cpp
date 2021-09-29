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

#include "PackedSpineSkinResource.h"

void PackedSpineSkinResource::_bind_methods() {
	ClassDB::bind_method(D_METHOD("set_skin_name", "v"), &PackedSpineSkinResource::set_skin_name);
	ClassDB::bind_method(D_METHOD("get_skin_name"), &PackedSpineSkinResource::get_skin_name);
	ClassDB::bind_method(D_METHOD("set_sub_skin_names", "v"), &PackedSpineSkinResource::set_sub_skin_names);
	ClassDB::bind_method(D_METHOD("get_sub_skin_names"), &PackedSpineSkinResource::get_sub_skin_names);

	ADD_SIGNAL(MethodInfo("property_changed"));

	ADD_PROPERTY(PropertyInfo(Variant::STRING, "skin_name"), "set_skin_name", "get_skin_name");
	ADD_PROPERTY(PropertyInfo(Variant::ARRAY, "sub_skin_names"), "set_sub_skin_names", "get_sub_skin_names");
}

PackedSpineSkinResource::PackedSpineSkinResource() : skin_name("custom_skin_name") {}
PackedSpineSkinResource::~PackedSpineSkinResource() {}

void PackedSpineSkinResource::set_skin_name(const String &v) {
	skin_name = v;
	emit_signal("property_changed");
}
String PackedSpineSkinResource::get_skin_name() {
	return skin_name;
}

void PackedSpineSkinResource::set_sub_skin_names(Array v) {
	sub_skin_names = v;
	emit_signal("property_changed");
}
Array PackedSpineSkinResource::get_sub_skin_names() {
	return sub_skin_names;
}
