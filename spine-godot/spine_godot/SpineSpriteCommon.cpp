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

#include "SpineSpriteCommon.h"
#include "SpineSprite2D.h"
#include "SpineSkeleton.h"
#include "SpineSkeletonDataResource.h"

String spine_resolve_preview_skin(const Ref<SpineSkeletonDataResource> &data_res, const String &skin) {
	String resolved = skin;
	if (resolved == SPINE_PREVIEW_LEGACY_DEFAULT_SKIN || resolved == SPINE_PREVIEW_NONE) {
		resolved = "";
	}
	if (!resolved.is_empty()) {
		return resolved;
	}
	if (!data_res.is_valid() || !data_res->is_skeleton_data_loaded()) {
		return "";
	}
	spine::SkeletonData *skeleton_data = data_res->get_skeleton_data();
	if (!skeleton_data) {
		return "";
	}
	spine::Skin *default_skin = skeleton_data->getDefaultSkin();
	if (!default_skin) {
		return "";
	}
#if (VERSION_MAJOR >= 4 && VERSION_MINOR >= 5)
	return String::utf8(default_skin->getName().buffer());
#else
	String name;
	name.parse_utf8(default_skin->getName().buffer());
	return name;
#endif
}

Ref<SpineSkeletonDataResource> spine_sprite_get_skeleton_data_res(Object *owner) {
	if (!owner) return Ref<SpineSkeletonDataResource>();
	if (SpineSprite2D *sprite = Object::cast_to<SpineSprite2D>(owner)) return sprite->get_skeleton_data_res();
	return Ref<SpineSkeletonDataResource>();
}

bool spine_sprite_is_visible_in_tree(Object *owner) {
	if (!owner) return false;
	if (SpineSprite2D *sprite = Object::cast_to<SpineSprite2D>(owner)) return sprite->is_visible_in_tree();
	return false;
}

void spine_sprite_set_modified_bones(Object *owner) {
	if (!owner) return;
	if (SpineSprite2D *sprite = Object::cast_to<SpineSprite2D>(owner)) sprite->set_modified_bones();
}

Transform2D spine_sprite_get_global_transform_2d(Object *owner) {
	if (!owner) return Transform2D();
	if (SpineSprite2D *sprite = Object::cast_to<SpineSprite2D>(owner)) return sprite->get_global_transform();
	return Transform2D();
}

Transform2D spine_sprite_get_global_transform_2d_affine_inverse(Object *owner) {
	return spine_sprite_get_global_transform_2d(owner).affine_inverse();
}

Ref<SpineSkeleton> spine_sprite_get_skeleton_ref(Object *owner) {
	if (!owner) return Ref<SpineSkeleton>();
	if (SpineSprite2D *sprite = Object::cast_to<SpineSprite2D>(owner)) return sprite->get_skeleton();
	return Ref<SpineSkeleton>();
}
