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

#pragma once

#include "SpineCommon.h"
#include "SpineSkeleton.h"

class SpineSprite2D;
class SpineSkeletonDataResource;

Ref<SpineSkeletonDataResource> spine_sprite_get_skeleton_data_res(Object *owner);
bool spine_sprite_is_visible_in_tree(Object *owner);
void spine_sprite_set_modified_bones(Object *owner);
Transform2D spine_sprite_get_global_transform_2d(Object *owner);
Transform2D spine_sprite_get_global_transform_2d_affine_inverse(Object *owner);
Ref<SpineSkeleton> spine_sprite_get_skeleton_ref(Object *owner);

inline constexpr const char *SPINE_PREVIEW_NONE = "None";
inline constexpr const char *SPINE_PREVIEW_LEGACY_EMPTY = "-- Empty --";
inline constexpr const char *SPINE_PREVIEW_LEGACY_DEFAULT_SKIN = "Default";

String spine_resolve_preview_skin(const Ref<SpineSkeletonDataResource> &data_res, const String &skin);

inline String spine_normalize_preview_animation(const String &animation) {
	if (animation == SPINE_PREVIEW_LEGACY_EMPTY) {
		return SPINE_PREVIEW_NONE;
	}
	return animation;
}

inline bool spine_preview_animation_is_none(const String &animation) {
	const String normalized = spine_normalize_preview_animation(animation);
	return normalized.is_empty() || normalized == SPINE_PREVIEW_NONE;
}
