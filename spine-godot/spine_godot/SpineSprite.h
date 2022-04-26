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

class SpineSlotNode;

class SpineSprite : public Node2D, public spine::AnimationStateListenerObject {
	GDCLASS(SpineSprite, Node2D)

public:
	enum UpdateMode {
		UpdateMode_Process,
		UpdateMode_Physics,
		UpdateMode_Manual
	};
	
protected:
	Ref<SpineSkeletonDataResource> skeleton_data_res;
	Ref<SpineSkeleton> skeleton;
	Ref<SpineAnimationState> animation_state;
	UpdateMode update_mode;

	spine::Vector<spine::Vector<SpineSlotNode*> > slot_nodes;
	Vector<MeshInstance2D *> mesh_instances;
	spine::SkeletonClipping *skeleton_clipper;
	static Ref<CanvasItemMaterial> default_materials[4];
	
	static void _bind_methods();
	void _notification(int what);

	void generate_meshes_for_slots(Ref<SpineSkeleton> skeleton_ref);
	void remove_meshes();
	void sort_slot_nodes();
	void update_meshes(Ref<SpineSkeleton> skeleton);

	void callback(spine::AnimationState *state, spine::EventType type, spine::TrackEntry *entry, spine::Event *event);

public:
	SpineSprite();
	~SpineSprite();

	void set_skeleton_data_res(const Ref<SpineSkeletonDataResource> &_spine_skeleton_data_resource);
	
	Ref<SpineSkeletonDataResource> get_skeleton_data_res();

	Ref<SpineSkeleton> get_skeleton();
	
	Ref<SpineAnimationState> get_animation_state();

	void on_skeleton_data_changed();

	void update_skeleton(float delta);

	Transform2D get_global_bone_transform(const String &bone_name);

	void set_global_bone_transform(const String &bone_name, Transform2D transform);
	
	UpdateMode get_update_mode();

	void set_update_mode(UpdateMode v);

	Ref<SpineSkin> new_skin(const String &name);

#ifdef TOOLS_ENABLED
	virtual Rect2 _edit_get_rect() const;
	virtual bool _edit_use_rect() const;
#endif
};

VARIANT_ENUM_CAST(SpineSprite::UpdateMode);
#endif//GODOT_SPINESPRITE_H
