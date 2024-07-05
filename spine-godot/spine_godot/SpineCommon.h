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

#ifndef SPINE_COMMON_H
#define SPINE_COMMON_H

#ifdef SPINE_GODOT_EXTENSION
#include <godot_cpp/core/version.hpp>
#include <godot_cpp/classes/ref_counted.hpp>
#include <godot_cpp/variant/string_name.hpp>
using namespace godot;
#define REFCOUNTED RefCounted
#define EMPTY(x) ((x).is_empty())
#define EMPTY_PTR(x) ((x)->is_empty())
#define SSIZE(x) ((x).length())
#define INSTANTIATE(x) (x).instantiate()
#define NOTIFY_PROPERTY_LIST_CHANGED() notify_property_list_changed()
#define VARIANT_FLOAT Variant::FLOAT
#define PROPERTY_USAGE_NOEDITOR PROPERTY_USAGE_NO_EDITOR
#define RES Ref<Resource>
#define REF Ref<RefCounted>
#define GEOMETRY2D Geometry2D
#define VERSION_MAJOR GODOT_VERSION_MAJOR
#define VERSION_MINOR GODOT_VERSION_MINOR
// FIXME this doesn't do the same as the engine SNAME in terms of caching
#define SNAME(name) StringName(name)
#define RS RenderingServer
#else
#include "core/version.h"
#if VERSION_MAJOR > 3
#include "core/core_bind.h"
#include "core/error/error_macros.h"
#define REFCOUNTED RefCounted
#define EMPTY(x) ((x).is_empty())
#define EMPTY_PTR(x) ((x)->is_empty())
#define SSIZE(x) ((x).size())
#define INSTANTIATE(x) (x).instantiate()
#define NOTIFY_PROPERTY_LIST_CHANGED() notify_property_list_changed()
#define VARIANT_FLOAT Variant::FLOAT
#define PROPERTY_USAGE_NOEDITOR PROPERTY_USAGE_NO_EDITOR
#define RES Ref<Resource>
#define REF Ref<RefCounted>
#define GEOMETRY2D Geometry2D
#else
#include "core/object.h"
#include "core/reference.h"
#include "core/error_macros.h"
#define REFCOUNTED Reference
#define EMPTY(x) ((x).empty())
#define EMPTY_PTR(x) ((x)->empty())
#define SSIZE(x) ((x).size())
#define INSTANTIATE(x) (x).instance()
#define NOTIFY_PROPERTY_LIST_CHANGED() property_list_changed_notify()
#define VARIANT_FLOAT Variant::REAL
#define GDREGISTER_CLASS(x) ClassDB::register_class<x>()
#define GEOMETRY2D Geometry
#ifndef SNAME
#define SNAME(m_arg) ([]() -> const StringName & { static StringName sname = _scs_create(m_arg); return sname; })()
#endif
#endif
#endif

#define SPINE_CHECK(obj, ret)                      \
	if (!(obj)) {                                  \
		ERR_PRINT("Native Spine object not set."); \
		return ret;                                \
	}

#define SPINE_STRING(x) spine::String((x).utf8())
#define SPINE_STRING_TMP(x) spine::String((x).utf8(), true, false)

// Can't do template classes with Godot's object model :(
class SpineObjectWrapper : public REFCOUNTED {
	GDCLASS(SpineObjectWrapper, REFCOUNTED)

	Object *spine_owner;
	void *spine_object;

protected:
	static void _bind_methods() {
		ClassDB::bind_method(D_METHOD("_internal_spine_objects_invalidated"), &SpineObjectWrapper::spine_objects_invalidated);
	}

	void spine_objects_invalidated() {
		spine_object = nullptr;
#if VERSION_MAJOR > 3
		spine_owner->disconnect(SNAME("_internal_spine_objects_invalidated"), callable_mp(this, &SpineObjectWrapper::spine_objects_invalidated));
#else
		spine_owner->disconnect(SNAME("_internal_spine_objects_invalidated"), this, SNAME("_internal_spine_objects_invalidated"));
#endif
	}

	SpineObjectWrapper() : spine_owner(nullptr), spine_object(nullptr) {
	}

	template<typename OWNER, typename OBJECT>
	void _set_spine_object_internal(const OWNER *_owner, OBJECT *_object) {
		if (spine_owner) {
			ERR_PRINT("Owner already set.");
			return;
		}
		if (spine_object) {
			ERR_PRINT("Object already set.");
			return;
		}
		if (!_owner) {
			ERR_PRINT("Owner must not be null.");
			return;
		}

		spine_owner = (Object *) _owner;
		spine_object = _object;
#if VERSION_MAJOR > 3
		spine_owner->connect(SNAME("_internal_spine_objects_invalidated"), callable_mp(this, &SpineObjectWrapper::spine_objects_invalidated));
#else
		spine_owner->connect(SNAME("_internal_spine_objects_invalidated"), this, SNAME("_internal_spine_objects_invalidated"));
#endif
	}

	void *_get_spine_object_internal() { return spine_object; }
	void *_get_spine_owner_internal() { return spine_owner; }
};

class SpineSprite;

template<typename OBJECT>
class SpineSpriteOwnedObject : public SpineObjectWrapper {
public:
	void set_spine_object(const SpineSprite *_owner, OBJECT *_object) {
		_set_spine_object_internal(_owner, _object);
	}

	OBJECT *get_spine_object() {
		return (OBJECT *) _get_spine_object_internal();
	}

	SpineSprite *get_spine_owner() {
		return (SpineSprite *) _get_spine_owner_internal();
	}
};

class SpineSkeletonDataResource;

template<typename OBJECT>
class SpineSkeletonDataResourceOwnedObject : public SpineObjectWrapper {
public:
	virtual void set_spine_object(const SpineSkeletonDataResource *_owner, OBJECT *_object) {
		_set_spine_object_internal(_owner, _object);
	}

	OBJECT *get_spine_object() {
		return (OBJECT *) _get_spine_object_internal();
	}

	SpineSkeletonDataResource *get_spine_owner() {
		return (SpineSkeletonDataResource *) _get_spine_owner_internal();
	}
};

#endif
