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

#pragma once

#include "SpineCommon.h"
#include "SpineSkeleton.h"
#include "SpineSprite.h"
#include "scene/2d/node_2d.h"

class SpineBoneNode : public Node2D {
	GDCLASS(SpineBoneNode, Node2D)

protected:
	String bone_name;
	SpineConstant::BoneMode bone_mode;
	bool enabled;
	Color debug_color;
	float debug_thickness;

	static void _bind_methods();
	void _notification(int what);
	void _get_property_list(List<PropertyInfo> *list) const;
	bool _get(const StringName &property, Variant &value) const;
	bool _set(const StringName &property, const Variant &value);
	void on_world_transforms_changed(const Variant &_sprite);
	void update_transform(SpineSprite *sprite);
	void init_transform(SpineSprite *sprite);
	void draw();

public:
	SpineBoneNode() : bone_mode(SpineConstant::BoneMode_Follow), enabled(true), debug_color(Color::hex(0xff000077)), debug_thickness(5) {}

	SpineConstant::BoneMode get_bone_mode();

	void set_bone_mode(SpineConstant::BoneMode bone_mode);

	void set_enabled(bool _enabled);

	bool get_enabled();

	void set_debug_thickness(float _thickness);

	float get_debug_thickness();

	void set_debug_color(Color _color);

	Color get_debug_color();

	SpineSprite *find_parent_sprite() const;

	Ref<SpineBone> find_bone() const;
};
