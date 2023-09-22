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

#include "SpineAttachment.h"
#include "SpineCommon.h"

void SpineAttachment::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_attachment_name"), &SpineAttachment::get_attachment_name);
	ClassDB::bind_method(D_METHOD("copy"), &SpineAttachment::copy);
}

SpineAttachment::~SpineAttachment() {
	if (get_spine_object()) get_spine_object()->dereference();
}

String SpineAttachment::get_attachment_name() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getName().buffer();
}

Ref<SpineAttachment> SpineAttachment::copy() {
	SPINE_CHECK(get_spine_object(), nullptr)
	auto copy = get_spine_object()->copy();
	if (!copy) return nullptr;
	Ref<SpineAttachment> attachment_ref(memnew(SpineAttachment));
	attachment_ref->set_spine_object(get_spine_owner(), copy);
	return attachment_ref;
}
