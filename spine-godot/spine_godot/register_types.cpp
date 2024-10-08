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


#include "SpineCommon.h"
#ifdef SPINE_GODOT_EXTENSION
#else
#include "modules/register_module_types.h"
#endif
#include "register_types.h"
#include "SpineEditorPlugin.h"
#include "SpineAtlasResource.h"
#include "SpineSkeletonFileResource.h"
#include "SpineSkeletonDataResource.h"
#include "SpineSprite.h"
#include "SpineSkeleton.h"
#include "SpineAnimationState.h"
#include "SpineAnimationTrack.h"
#include "SpineEventData.h"
#include "SpineEvent.h"
#include "SpineTrackEntry.h"
#include "SpineBoneData.h"
#include "SpineSlotData.h"
#include "SpineAttachment.h"
#include "SpineConstraintData.h"
#include "SpineSkin.h"
#include "SpineIkConstraintData.h"
#include "SpineTransformConstraintData.h"
#include "SpinePathConstraintData.h"
#include "SpinePhysicsConstraintData.h"
#include "SpineTimeline.h"
#include "SpineConstant.h"
#include "SpineSlotNode.h"
#include "SpineBoneNode.h"
#include "spine/Bone.h"

static SpineAtlasResourceFormatLoader *atlas_loader;
static SpineAtlasResourceFormatSaver *atlas_saver;
static SpineSkeletonFileResourceFormatLoader *skeleton_file_loader;
static SpineSkeletonFileResourceFormatSaver *skeleton_file_saver;

#ifdef TOOLS_ENABLED
#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/classes/editor_plugin_registration.hpp>
#else
#include "editor/editor_node.h"
#include "SpineEditorPlugin.h"

static void editor_init_callback() {
	EditorNode::get_singleton()->add_editor_plugin(memnew(SpineEditorPlugin(EditorNode::get_singleton())));
}
#endif
#endif

#ifdef SPINE_GODOT_EXTENSION
void initialize_spine_godot_module(ModuleInitializationLevel level) {
	if (level == MODULE_INITIALIZATION_LEVEL_EDITOR) {
		GDREGISTER_CLASS(SpineAtlasResourceImportPlugin);
		GDREGISTER_CLASS(SpineJsonResourceImportPlugin);
		GDREGISTER_CLASS(SpineBinaryResourceImportPlugin);
		GDREGISTER_CLASS(SpineSkeletonDataResourceInspectorPlugin);
		GDREGISTER_CLASS(SpineEditorPlugin);
		EditorPlugins::add_plugin_class(StringName("SpineEditorPlugin"));
	}
	if (level != MODULE_INITIALIZATION_LEVEL_SCENE) return;
#else
#if VERSION_MAJOR > 3
void initialize_spine_godot_module(ModuleInitializationLevel level) {
	if (level == MODULE_INITIALIZATION_LEVEL_EDITOR) {
#ifdef TOOLS_ENABLED
		EditorNode::add_init_callback(editor_init_callback);
		GDREGISTER_CLASS(SpineEditorPropertyAnimationMixes);
		return;
#endif
	}
	if (level != MODULE_INITIALIZATION_LEVEL_CORE) return;
#else
void register_spine_godot_types() {
#ifdef TOOLS_ENABLED
	EditorNode::add_init_callback(editor_init_callback);
	GDREGISTER_CLASS(SpineEditorPropertyAnimationMixes);
#endif
#endif
#endif
	spine::Bone::setYDown(true);

	GDREGISTER_CLASS(SpineAtlasResourceFormatLoader);
	GDREGISTER_CLASS(SpineAtlasResourceFormatSaver);
	GDREGISTER_CLASS(SpineSkeletonFileResourceFormatLoader);
	GDREGISTER_CLASS(SpineSkeletonFileResourceFormatSaver);

	GDREGISTER_CLASS(SpineObjectWrapper);
	GDREGISTER_CLASS(SpineAtlasResource);
	GDREGISTER_CLASS(SpineSkeletonFileResource);
	GDREGISTER_CLASS(SpineSkeletonDataResource);
	GDREGISTER_CLASS(SpineAnimationMix);
	GDREGISTER_CLASS(SpineSprite);
	GDREGISTER_CLASS(SpineMesh2D);
	GDREGISTER_CLASS(SpineSkeleton);
	GDREGISTER_CLASS(SpineAnimationState);
	GDREGISTER_CLASS(SpineAnimation);
	GDREGISTER_CLASS(SpineEventData);
	GDREGISTER_CLASS(SpineTrackEntry);
	GDREGISTER_CLASS(SpineEvent);
	GDREGISTER_CLASS(SpineBoneData);
	GDREGISTER_CLASS(SpineSlotData);
	GDREGISTER_CLASS(SpineAttachment);
	GDREGISTER_CLASS(SpineSkinEntry);
	GDREGISTER_CLASS(SpineConstraintData);
	GDREGISTER_CLASS(SpineSkin);
	GDREGISTER_CLASS(SpineIkConstraintData);
	GDREGISTER_CLASS(SpineTransformConstraintData);
	GDREGISTER_CLASS(SpinePathConstraintData);
	GDREGISTER_CLASS(SpinePhysicsConstraintData);
	GDREGISTER_CLASS(SpineBone);
	GDREGISTER_CLASS(SpineSlot);
	GDREGISTER_CLASS(SpineIkConstraint);
	GDREGISTER_CLASS(SpinePathConstraint);
	GDREGISTER_CLASS(SpineTransformConstraint);
	GDREGISTER_CLASS(SpinePhysicsConstraint);
	GDREGISTER_CLASS(SpineTimeline);
	GDREGISTER_CLASS(SpineConstant);

	GDREGISTER_CLASS(SpineSlotNode);
	GDREGISTER_CLASS(SpineBoneNode);
#ifndef SPINE_GODOT_EXTENSION
	GDREGISTER_CLASS(SpineAnimationTrack);
#endif

#ifdef SPINE_GODOT_EXTENSION
	atlas_loader = memnew(SpineAtlasResourceFormatLoader);
	ResourceLoader::get_singleton()->add_resource_format_loader(atlas_loader);

	atlas_saver = memnew(SpineAtlasResourceFormatSaver);
	ResourceSaver::get_singleton()->add_resource_format_saver(atlas_saver);

	skeleton_file_loader = memnew(SpineSkeletonFileResourceFormatLoader);
	ResourceLoader::get_singleton()->add_resource_format_loader(skeleton_file_loader);

	skeleton_file_saver = memnew(SpineSkeletonFileResourceFormatSaver);
	ResourceSaver::get_singleton()->add_resource_format_saver(skeleton_file_saver);
#else
#if VERSION_MAJOR > 3
	atlas_loader = memnew(SpineAtlasResourceFormatLoader);
	ResourceLoader::add_resource_format_loader(atlas_loader);

	atlas_saver = memnew(SpineAtlasResourceFormatSaver);
	ResourceSaver::add_resource_format_saver(atlas_saver);

	skeleton_file_loader = memnew(SpineSkeletonFileResourceFormatLoader);
	ResourceLoader::add_resource_format_loader(skeleton_file_loader);

	skeleton_file_saver = memnew(SpineSkeletonFileResourceFormatSaver);
	ResourceSaver::add_resource_format_saver(skeleton_file_saver);
#else
	atlas_loader = memnew(SpineAtlasResourceFormatLoader);
	ResourceLoader::add_resource_format_loader(atlas_loader);

	atlas_saver = memnew(SpineAtlasResourceFormatSaver);
	ResourceSaver::add_resource_format_saver(atlas_saver);

	skeleton_file_loader = memnew(SpineSkeletonFileResourceFormatLoader);
	ResourceLoader::add_resource_format_loader(skeleton_file_loader);

	skeleton_file_saver = memnew(SpineSkeletonFileResourceFormatSaver);
	ResourceSaver::add_resource_format_saver(skeleton_file_saver);
#endif
#endif
	printf(">>>>>>>>>>>>>>>>>>>> fuck\n");
}

#if VERSION_MAJOR > 3
void uninitialize_spine_godot_module(ModuleInitializationLevel level) {
	return;
	if (level != MODULE_INITIALIZATION_LEVEL_CORE) return;
#else
void unregister_spine_godot_types() {
#endif
#ifdef SPINE_GODOT_EXTENSION
	ResourceLoader::get_singleton()->remove_resource_format_loader(atlas_loader);
	ResourceSaver::get_singleton()->remove_resource_format_saver(atlas_saver);
	ResourceLoader::get_singleton()->remove_resource_format_loader(skeleton_file_loader);
	ResourceSaver::get_singleton()->remove_resource_format_saver(skeleton_file_saver);
#else
	ResourceLoader::remove_resource_format_loader(atlas_loader);
	ResourceSaver::remove_resource_format_saver(atlas_saver);
	ResourceLoader::remove_resource_format_loader(skeleton_file_loader);
	ResourceSaver::remove_resource_format_saver(skeleton_file_saver);
#endif
}


#ifdef SPINE_GODOT_EXTENSION
extern "C" GDExtensionBool GDE_EXPORT spine_godot_library_init(GDExtensionInterfaceGetProcAddress p_get_proc_address, GDExtensionClassLibraryPtr p_library, GDExtensionInitialization *r_initialization) {
	GDExtensionBinding::InitObject init_obj(p_get_proc_address, p_library, r_initialization);
	init_obj.register_initializer(initialize_spine_godot_module);
	init_obj.register_terminator(uninitialize_spine_godot_module);
	init_obj.set_minimum_library_initialization_level(MODULE_INITIALIZATION_LEVEL_CORE);
	return init_obj.init();
}
#endif