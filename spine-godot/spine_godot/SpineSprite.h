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

#ifndef GODOT_SPINESPRITE_H
#define GODOT_SPINESPRITE_H

#include "SpineSkeleton.h"
#include "SpineAnimationState.h"
#include "scene/2d/node_2d.h"
#include "scene/2d/mesh_instance_2d.h"
#include "scene/resources/texture.h"

class SpineSprite : public Node2D, public spine::AnimationStateListenerObject {
	GDCLASS(SpineSprite, Node2D);

protected:
	static void _bind_methods();

	void _notification(int p_what);

	void _get_property_list(List<PropertyInfo> *p_list) const;
	bool _get(const StringName &p_property, Variant &r_value) const;
	bool _set(const StringName &p_property, const Variant &p_value);

	void _validate_and_play_current_animations();

public:
	enum ProcessMode {
		ProcessMode_Process,
		ProcessMode_Physics,
		ProcessMode_Manual
	};

private:
	Ref<SpineSkeletonDataResource> skeleton_data_res;

	Ref<SpineSkeleton> skeleton;
	Ref<SpineAnimationState> animation_state;

	String preview_animation;
	Array bind_slot_nodes;
	bool overlap;

	ProcessMode process_mode;

	Vector<MeshInstance2D *> mesh_instances;
	spine::SkeletonClipping *skeleton_clipper;
	static Ref<CanvasItemMaterial> materials[4];

public:
	SpineSprite();
	~SpineSprite();

	void set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &a);
	Ref<SpineSkeletonDataResource> get_skeleton_data_res();

	Ref<SpineSkeleton> get_skeleton();
	Ref<SpineAnimationState> get_animation_state();

	void generate_meshes_for_slots(Ref<SpineSkeleton> skeleton_ref);
	void remove_meshes();

	void update_meshes(Ref<SpineSkeleton> s);

	void update_bind_slot_nodes();
	void update_bind_slot_node_transform(Ref<SpineBone> bone, Node2D *node2d);
	void update_bind_slot_node_draw_order(const String &slot_name, Node2D *node2d);
	Node *find_child_node_by_node(Node *node);

	virtual void callback(spine::AnimationState *state, spine::EventType type, spine::TrackEntry *entry, spine::Event *event);

	void _on_skeleton_data_changed();

	void _update_all(float delta);

	Array get_bind_slot_nodes();
	void set_bind_slot_nodes(Array v);

	Transform2D bone_get_global_transform(const String &bone_name);
	void bone_set_global_transform(const String &bone_name, Transform2D transform);

	bool get_overlap();
	void set_overlap(bool v);

	ProcessMode get_process_mode();
	void set_process_mode(ProcessMode v);

#ifdef TOOLS_ENABLED
	virtual Rect2 _edit_get_rect() const;
	virtual bool _edit_use_rect() const;
#endif
};

VARIANT_ENUM_CAST(SpineSprite::ProcessMode);
#endif//GODOT_SPINESPRITE_H
