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

#include "SpineConstraintData.h"
#include "SpineCommon.h"
#include <spine/ConstraintData.h>

void SpineConstraintData::_bind_methods() {
	ClassDB::bind_method(D_METHOD("get_constraint_name"), &SpineConstraintData::get_constraint_name);
	ClassDB::bind_method(D_METHOD("get_order"), &SpineConstraintData::get_order);
	ClassDB::bind_method(D_METHOD("set_order", "v"), &SpineConstraintData::set_order);
	ClassDB::bind_method(D_METHOD("is_skin_required"), &SpineConstraintData::is_skin_required);
	ClassDB::bind_method(D_METHOD("set_skin_required", "v"), &SpineConstraintData::set_skin_required);
}

String SpineConstraintData::get_constraint_name() {
	SPINE_CHECK(get_spine_object(), "")
	return get_spine_object()->getName().buffer();
}

int SpineConstraintData::get_order() {
	SPINE_CHECK(get_spine_object(), 0)
	return (int) get_spine_object()->getOrder();
}

void SpineConstraintData::set_order(int v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setOrder(v);
}

bool SpineConstraintData::is_skin_required() {
	SPINE_CHECK(get_spine_object(), false)
	return get_spine_object()->isSkinRequired();
}

void SpineConstraintData::set_skin_required(bool v) {
	SPINE_CHECK(get_spine_object(), )
	get_spine_object()->setSkinRequired(v);
}
