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

#include "generated/types.h"
#include "spine/Physics.h"
#include "extensions.h"
#include <spine/spine.h>
#include <spine/Version.h>
#include <spine/Debug.h>
#include <spine/SkeletonRenderer.h>
#include <spine/AnimationState.h>
#include <cstring>

using namespace spine;

// Internal structures
struct _spine_atlas_result {
	spine_atlas atlas;
	const char *error;
};

struct _spine_skeleton_data_result {
	spine_skeleton_data skeletonData;
	const char *error;
};

struct _spine_bounds {
	float x, y, width, height;
};

struct _spine_vector {
	float x, y;
};

struct _spine_skeleton_drawable : public SpineObject {
	spine_skeleton skeleton;
	spine_animation_state animationState;
	spine_animation_state_data animationStateData;
	spine_animation_state_events animationStateEvents;
	SkeletonRenderer *renderer;
};

struct _spine_skin_entry {
	int32_t slotIndex;
	const char *name;
	spine_attachment attachment;
};

struct _spine_skin_entries {
	int32_t numEntries;
	_spine_skin_entry *entries;
};

// Animation state event tracking
struct AnimationStateEvent {
	EventType type;
	TrackEntry *entry;
	Event *event;
	AnimationStateEvent(EventType type, TrackEntry *entry, Event *event) : type(type), entry(entry), event(event) {};
};

class EventListener : public AnimationStateListenerObject, public SpineObject {
public:
	Array<AnimationStateEvent> events;

	void callback(AnimationState *state, EventType type, TrackEntry *entry, Event *event) override {
		events.add(AnimationStateEvent(type, entry, event));
		SP_UNUSED(state);
	}
};

// Static variables
static Color NULL_COLOR(0, 0, 0, 0);
static SpineExtension *defaultExtension = nullptr;
static DebugExtension *debugExtension = nullptr;

static void initExtensions() {
	if (defaultExtension == nullptr) {
		defaultExtension = new DefaultSpineExtension();
		debugExtension = new DebugExtension(defaultExtension);
	}
}

namespace spine {
	SpineExtension *getDefaultExtension() {
		initExtensions();
		return defaultExtension;
	}
}

// Version functions
int32_t spine_major_version() {
	return SPINE_MAJOR_VERSION;
}

int32_t spine_minor_version() {
	return SPINE_MINOR_VERSION;
}

void spine_enable_debug_extension(bool enable) {
	initExtensions();
	SpineExtension::setInstance(enable ? debugExtension : defaultExtension);
}

void spine_report_leaks() {
	initExtensions();
	debugExtension->reportLeaks();
	fflush(stdout);
}

// Atlas functions
class SpineCTextureLoader : public TextureLoader {
	void load(AtlasPage &page, const String &path) {
		page.texture = (void *) (intptr_t) page.index;
	}

	void unload(void *texture) {
	}
};
static SpineCTextureLoader textureLoader;

spine_atlas_result spine_atlas_load(const char *atlasData) {
	if (!atlasData) return nullptr;
	_spine_atlas_result *result = SpineExtension::calloc<_spine_atlas_result>(1, __FILE__, __LINE__);
	int32_t length = (int32_t) strlen(atlasData);
	auto atlas = new (__FILE__, __LINE__) Atlas(atlasData, length, "", &textureLoader, true);
	if (!atlas) {
		result->error = (const char *) strdup("Failed to load atlas");
		return (spine_atlas_result) result;
	}
	result->atlas = (spine_atlas) atlas;
	return (spine_atlas_result) result;
}

class CallbackTextureLoad : public TextureLoader {
	spine_texture_loader_load_func loadCb;
	spine_texture_loader_unload_func unloadCb;

public:
	CallbackTextureLoad() : loadCb(nullptr), unloadCb(nullptr) {
	}

	void setCallbacks(spine_texture_loader_load_func load, spine_texture_loader_unload_func unload) {
		loadCb = load;
		unloadCb = unload;
	}

	void load(AtlasPage &page, const String &path) {
		page.texture = this->loadCb(path.buffer());
	}

	void unload(void *texture) {
		this->unloadCb(texture);
	}
};
static CallbackTextureLoad callbackLoader;

spine_atlas_result spine_atlas_load_callback(const char *atlasData, const char *atlasDir, spine_texture_loader_load_func load,
											 spine_texture_loader_unload_func unload) {
	if (!atlasData) return nullptr;
	_spine_atlas_result *result = SpineExtension::calloc<_spine_atlas_result>(1, __FILE__, __LINE__);
	int32_t length = (int32_t) strlen(atlasData);
	callbackLoader.setCallbacks(load, unload);
	auto atlas = new (__FILE__, __LINE__) Atlas(atlasData, length, (const char *) atlasDir, &callbackLoader, true);
	if (!atlas) {
		result->error = (const char *) strdup("Failed to load atlas");
		return (spine_atlas_result) result;
	}
	result->atlas = (spine_atlas) atlas;
	return (spine_atlas_result) result;
}

const char *spine_atlas_result_get_error(spine_atlas_result result) {
	if (!result) return nullptr;
	return ((_spine_atlas_result *) result)->error;
}

spine_atlas spine_atlas_result_get_atlas(spine_atlas_result result) {
	if (!result) return nullptr;
	return ((_spine_atlas_result *) result)->atlas;
}

void spine_atlas_result_dispose(spine_atlas_result result) {
	if (!result) return;
	_spine_atlas_result *_result = (_spine_atlas_result *) result;
	if (_result->error) {
		SpineExtension::free(_result->error, __FILE__, __LINE__);
	}
	SpineExtension::free(_result, __FILE__, __LINE__);
}

// Skeleton data loading
spine_skeleton_data_result spine_skeleton_data_load_json(spine_atlas atlas, const char *skeletonData, const char *path) {
	if (!atlas || !skeletonData) return nullptr;
	_spine_skeleton_data_result *result = SpineExtension::calloc<_spine_skeleton_data_result>(1, __FILE__, __LINE__);
	SkeletonJson json(*(Atlas *) atlas);
	json.setScale(1);

	SkeletonData *data = json.readSkeletonData(skeletonData);
	if (!data) {
		result->error = SpineExtension::strdup("Failed to load skeleton data", __FILE__, __LINE__);
		return (spine_skeleton_data_result) result;
	}

	// Set name from path if provided
	if (path != nullptr && data != nullptr) {
		String pathStr(path);

		// Extract filename without extension from path
		int lastSlash = pathStr.lastIndexOf('/');
		int lastBackslash = pathStr.lastIndexOf('\\');
		int start = 0;

		if (lastSlash != -1) start = lastSlash + 1;
		if (lastBackslash != -1 && lastBackslash > start) start = lastBackslash + 1;

		int lastDot = pathStr.lastIndexOf('.');
		if (lastDot != -1 && lastDot > start) {
			data->setName(pathStr.substring(start, lastDot - start));
		} else {
			data->setName(pathStr.substring(start));
		}
	}

	result->skeletonData = (spine_skeleton_data) data;
	return (spine_skeleton_data_result) result;
}

spine_skeleton_data_result spine_skeleton_data_load_binary(spine_atlas atlas, const uint8_t *skeletonData, int32_t length, const char *path) {
	if (!atlas || !skeletonData) return nullptr;
	_spine_skeleton_data_result *result = SpineExtension::calloc<_spine_skeleton_data_result>(1, __FILE__, __LINE__);
	SkeletonBinary binary(*(Atlas *) atlas);
	binary.setScale(1);

	SkeletonData *data = binary.readSkeletonData((const unsigned char *) skeletonData, length);
	if (!data) {
		result->error = SpineExtension::strdup("Failed to load skeleton data", __FILE__, __LINE__);
		return (spine_skeleton_data_result) result;
	}

	// Set name from path if provided
	if (path != nullptr && data != nullptr) {
		String pathStr(path);

		// Extract filename without extension from path
		int lastSlash = pathStr.lastIndexOf('/');
		int lastBackslash = pathStr.lastIndexOf('\\');
		int start = 0;

		if (lastSlash != -1) start = lastSlash + 1;
		if (lastBackslash != -1 && lastBackslash > start) start = lastBackslash + 1;

		int lastDot = pathStr.lastIndexOf('.');
		if (lastDot != -1 && lastDot > start) {
			data->setName(pathStr.substring(start, lastDot - start));
		} else {
			data->setName(pathStr.substring(start));
		}
	}

	result->skeletonData = (spine_skeleton_data) data;
	return (spine_skeleton_data_result) result;
}

const char *spine_skeleton_data_result_get_error(spine_skeleton_data_result result) {
	if (!result) return nullptr;
	return ((_spine_skeleton_data_result *) result)->error;
}

spine_skeleton_data spine_skeleton_data_result_get_data(spine_skeleton_data_result result) {
	if (!result) return nullptr;
	return ((_spine_skeleton_data_result *) result)->skeletonData;
}

void spine_skeleton_data_result_dispose(spine_skeleton_data_result result) {
	if (!result) return;
	_spine_skeleton_data_result *_result = (_spine_skeleton_data_result *) result;
	if (_result->error) {
		SpineExtension::free(_result->error, __FILE__, __LINE__);
	}
	SpineExtension::free(_result, __FILE__, __LINE__);
}

// Skeleton drawable
spine_skeleton_drawable spine_skeleton_drawable_create(spine_skeleton_data skeletonData) {
	if (!skeletonData) return nullptr;
	_spine_skeleton_drawable *drawable = new (__FILE__, __LINE__) _spine_skeleton_drawable();

	Skeleton *skeleton = new (__FILE__, __LINE__) Skeleton(*((SkeletonData *) skeletonData));
	AnimationStateData *stateData = new (__FILE__, __LINE__) AnimationStateData(*(SkeletonData *) skeletonData);
	AnimationState *state = new (__FILE__, __LINE__) AnimationState(*stateData);
	EventListener *listener = new (__FILE__, __LINE__) EventListener();
	state->setListener(listener);

	drawable->skeleton = (spine_skeleton) skeleton;
	drawable->animationStateData = (spine_animation_state_data) stateData;
	drawable->animationState = (spine_animation_state) state;
	drawable->animationStateEvents = (spine_animation_state_events) listener;
	drawable->renderer = new (__FILE__, __LINE__) SkeletonRenderer();

	return (spine_skeleton_drawable) drawable;
}

void spine_skeleton_drawable_update(spine_skeleton_drawable drawable, float delta) {
	if (!drawable) return;
	_spine_skeleton_drawable *_drawable = (_spine_skeleton_drawable *) drawable;
	Skeleton *skeleton = (Skeleton *) _drawable->skeleton;
	AnimationState *state = (AnimationState *) _drawable->animationState;

	state->update(delta);
	state->apply(*skeleton);
	skeleton->update(delta);
	skeleton->updateWorldTransform(spine::Physics_Update);
}

spine_render_command spine_skeleton_drawable_render(spine_skeleton_drawable drawable) {
	if (!drawable) return nullptr;
	_spine_skeleton_drawable *_drawable = (_spine_skeleton_drawable *) drawable;
	Skeleton *skeleton = (Skeleton *) _drawable->skeleton;
	SkeletonRenderer *renderer = _drawable->renderer;

	RenderCommand *commands = renderer->render(*skeleton);
	return (spine_render_command) commands;
}

void spine_skeleton_drawable_dispose(spine_skeleton_drawable drawable) {
	if (!drawable) return;
	_spine_skeleton_drawable *_drawable = (_spine_skeleton_drawable *) drawable;

	if (_drawable->renderer) {
		delete _drawable->renderer;
	}
	if (_drawable->animationState) {
		delete (AnimationState *) _drawable->animationState;
	}
	if (_drawable->animationStateData) {
		delete (AnimationStateData *) _drawable->animationStateData;
	}
	if (_drawable->skeleton) {
		delete (Skeleton *) _drawable->skeleton;
	}
	if (_drawable->animationStateEvents) {
		delete (EventListener *) _drawable->animationStateEvents;
	}

	delete _drawable;
}

spine_skeleton spine_skeleton_drawable_get_skeleton(spine_skeleton_drawable drawable) {
	if (!drawable) return nullptr;
	return ((_spine_skeleton_drawable *) drawable)->skeleton;
}

spine_animation_state spine_skeleton_drawable_get_animation_state(spine_skeleton_drawable drawable) {
	if (!drawable) return nullptr;
	return ((_spine_skeleton_drawable *) drawable)->animationState;
}

spine_animation_state_data spine_skeleton_drawable_get_animation_state_data(spine_skeleton_drawable drawable) {
	if (!drawable) return nullptr;
	return ((_spine_skeleton_drawable *) drawable)->animationStateData;
}

spine_animation_state_events spine_skeleton_drawable_get_animation_state_events(spine_skeleton_drawable drawable) {
	if (!drawable) return nullptr;
	return ((_spine_skeleton_drawable *) drawable)->animationStateEvents;
}


// Animation state events functions
int32_t spine_animation_state_events_get_num_events(spine_animation_state_events events) {
	if (!events) return 0;
	EventListener *listener = (EventListener *) events;
	return (int32_t) listener->events.size();
}

int32_t spine_animation_state_events_get_event_type(spine_animation_state_events events, int32_t index) {
	if (!events) return 0;
	EventListener *listener = (EventListener *) events;
	if (index < 0 || index >= (int32_t) listener->events.size()) return 0;
	return (int32_t) listener->events[index].type;
}

spine_track_entry spine_animation_state_events_get_track_entry(spine_animation_state_events events, int32_t index) {
	if (!events) return nullptr;
	EventListener *listener = (EventListener *) events;
	if (index < 0 || index >= (int32_t) listener->events.size()) return nullptr;
	return (spine_track_entry) listener->events[index].entry;
}

spine_event spine_animation_state_events_get_event(spine_animation_state_events events, int32_t index) {
	if (!events) return nullptr;
	EventListener *listener = (EventListener *) events;
	if (index < 0 || index >= (int32_t) listener->events.size()) return nullptr;
	return (spine_event) listener->events[index].event;
}

void spine_animation_state_events_reset(spine_animation_state_events events) {
	if (!events) return;
	EventListener *listener = (EventListener *) events;
	listener->events.clear();
}

// Skin entries functions

void spine_skin_entries_dispose(spine_skin_entries entries) {
	if (!entries) return;
	_spine_skin_entries *_entries = (_spine_skin_entries *) entries;
	if (_entries->entries) {
		for (int i = 0; i < _entries->numEntries; i++) {
			if (_entries->entries[i].name) {
				SpineExtension::free(_entries->entries[i].name, __FILE__, __LINE__);
			}
		}
		SpineExtension::free(_entries->entries, __FILE__, __LINE__);
	}
	SpineExtension::free(_entries, __FILE__, __LINE__);
}

int32_t spine_skin_entries_get_num_entries(spine_skin_entries entries) {
	if (!entries) return 0;
	return ((_spine_skin_entries *) entries)->numEntries;
}

spine_skin_entry spine_skin_entries_get_entry(spine_skin_entries entries, int32_t index) {
	if (!entries) return nullptr;
	_spine_skin_entries *_entries = (_spine_skin_entries *) entries;
	if (index < 0 || index >= _entries->numEntries) return nullptr;
	return (spine_skin_entry) &_entries->entries[index];
}

int32_t spine_skin_entry_get_slot_index(spine_skin_entry entry) {
	if (!entry) return 0;
	return ((_spine_skin_entry *) entry)->slotIndex;
}

const char *spine_skin_entry_get_name(spine_skin_entry entry) {
	if (!entry) return nullptr;
	return ((_spine_skin_entry *) entry)->name;
}

spine_attachment spine_skin_entry_get_attachment(spine_skin_entry entry) {
	if (!entry) return nullptr;
	return ((_spine_skin_entry *) entry)->attachment;
}

// Skin functions
spine_skin_entries spine_skin_get_entries(spine_skin skin) {
	if (!skin) return nullptr;

	Skin *_skin = (Skin *) skin;
	_spine_skin_entries *result = SpineExtension::calloc<_spine_skin_entries>(1, __FILE__, __LINE__);

	// First pass: count the entries
	{
		Skin::AttachmentMap::Entries entries = _skin->getAttachments();
		int count = 0;
		while (entries.hasNext()) {
			entries.next();
			count++;
		}
		result->numEntries = count;
	}

	// Second pass: populate the entries
	if (result->numEntries > 0) {
		result->entries = SpineExtension::calloc<_spine_skin_entry>(result->numEntries, __FILE__, __LINE__);

		Skin::AttachmentMap::Entries entries = _skin->getAttachments();
		int index = 0;
		while (entries.hasNext()) {
			Skin::AttachmentMap::Entry &entry = entries.next();
			result->entries[index].slotIndex = (int32_t) entry._slotIndex;
			result->entries[index].name = SpineExtension::strdup(entry._placeholder.buffer(), __FILE__, __LINE__);
			result->entries[index].attachment = (spine_attachment) entry._attachment;
			index++;
		}
	}

	return (spine_skin_entries) result;
}

// Skeleton bounds function
void spine_skeleton_get_bounds(spine_skeleton self, spine_array_float output) {
	spine_array_float_clear(output);
	if (!self) return;

	Skeleton *_skeleton = (Skeleton *) self;
	float x, y, width, height;
	_skeleton->getBounds(x, y, width, height);
	spine_array_float_add(output, x);
	spine_array_float_add(output, y);
	spine_array_float_add(output, width);
	spine_array_float_add(output, height);
}

void spine_skeleton_get_position_v(spine_skeleton self, spine_array_float output) {
	spine_array_float_clear(output);
	if (!self) return;

	Skeleton *_skeleton = (Skeleton *) self;
	float x, y;
	_skeleton->getPosition(x, y);
	spine_array_float_add(output, x);
	spine_array_float_add(output, y);
}

void spine_bone_pose_world_to_local_v(spine_bone_pose self, float world_x, float world_y, spine_array_float output) {
	spine_array_float_clear(output);
	if (!self) return;

	BonePose *_self = (BonePose *) self;
	float localX, localY;
	_self->worldToLocal(world_x, world_y, localX, localY);
	spine_array_float_add(output, localX);
	spine_array_float_add(output, localY);
}

void spine_bone_pose_local_to_world_v(spine_bone_pose self, float local_x, float local_y, spine_array_float output) {
	spine_array_float_clear(output);
	if (!self) return;

	BonePose *_self = (BonePose *) self;
	float worldX, worldY;
	_self->localToWorld(local_x, local_y, worldX, worldY);
	spine_array_float_add(output, worldX);
	spine_array_float_add(output, worldY);
}

void spine_bone_pose_world_to_parent_v(spine_bone_pose self, float world_x, float world_y, spine_array_float output) {
	spine_array_float_clear(output);
	if (!self) return;

	BonePose *_self = (BonePose *) self;
	float parentX, parentY;
	_self->worldToParent(world_x, world_y, parentX, parentY);
	spine_array_float_add(output, parentX);
	spine_array_float_add(output, parentY);
}

void spine_bone_pose_parent_to_world_v(spine_bone_pose self, float parent_x, float parent_y, spine_array_float output) {
	spine_array_float_clear(output);
	if (!self) return;

	BonePose *_self = (BonePose *) self;
	float worldX, worldY;
	_self->parentToWorld(parent_x, parent_y, worldX, worldY);
	spine_array_float_add(output, worldX);
	spine_array_float_add(output, worldY);
}

// Event listener functions
void spine_animation_state_set_listener(spine_animation_state state, spine_animation_state_listener listener, void *user_data) {
	if (!state) return;
	AnimationState *_state = (AnimationState *) state;
	_state->setListener((AnimationStateListener) listener, user_data);
}

void spine_track_entry_set_listener(spine_track_entry entry, spine_animation_state_listener listener, void *user_data) {
	if (!entry) return;
	TrackEntry *_entry = (TrackEntry *) entry;
	_entry->setListener((AnimationStateListener) listener, user_data);
}