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

#include "arrays.h"
#include <spine/spine.h>

using namespace spine;

spine_array_float spine_array_float_create(void) {
	return (spine_array_float) new (__FILE__, __LINE__) Array<float>();
}

spine_array_float spine_array_float_create_with_capacity(size_t initialCapacity) {
	return (spine_array_float) new (__FILE__, __LINE__) Array<float>(initialCapacity);
}
void spine_array_float_dispose(spine_array_float array) {
	delete (Array<float> *) array;
}
void spine_array_float_clear(spine_array_float array) {
	Array<float> *_array = (Array<float> *) array;
	_array->clear();
}

size_t spine_array_float_get_capacity(spine_array_float array) {
	Array<float> *_array = (Array<float> *) array;
	return _array->getCapacity();
}

size_t spine_array_float_size(spine_array_float array) {
	Array<float> *_array = (Array<float> *) array;
	return _array->size();
}

spine_array_float spine_array_float_set_size(spine_array_float array, size_t newSize, float defaultValue) {
	Array<float> *_array = (Array<float> *) array;
	return (spine_array_float) &_array->setSize(newSize, defaultValue);
}

void spine_array_float_ensure_capacity(spine_array_float array, size_t newCapacity) {
	Array<float> *_array = (Array<float> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_float_add(spine_array_float array, float inValue) {
	Array<float> *_array = (Array<float> *) array;
	_array->add(inValue);
}

void spine_array_float_add_all(spine_array_float array, spine_array_float inValue) {
	Array<float> *_array = (Array<float> *) array;
	_array->addAll(*((const Array<float> *) inValue));
}

void spine_array_float_clear_and_add_all(spine_array_float array, spine_array_float inValue) {
	Array<float> *_array = (Array<float> *) array;
	_array->clearAndAddAll(*((const Array<float> *) inValue));
}

void spine_array_float_remove_at(spine_array_float array, size_t inIndex) {
	Array<float> *_array = (Array<float> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_float_contains(spine_array_float array, float inValue) {
	Array<float> *_array = (Array<float> *) array;
	return _array->contains(inValue);
}

int spine_array_float_index_of(spine_array_float array, float inValue) {
	Array<float> *_array = (Array<float> *) array;
	return _array->indexOf(inValue);
}

/*@null*/ float *spine_array_float_buffer(spine_array_float array) {
	Array<float> *_array = (Array<float> *) array;
	return _array->buffer();
}

spine_array_int spine_array_int_create(void) {
	return (spine_array_int) new (__FILE__, __LINE__) Array<int>();
}

spine_array_int spine_array_int_create_with_capacity(size_t initialCapacity) {
	return (spine_array_int) new (__FILE__, __LINE__) Array<int>(initialCapacity);
}
void spine_array_int_dispose(spine_array_int array) {
	delete (Array<int> *) array;
}
void spine_array_int_clear(spine_array_int array) {
	Array<int> *_array = (Array<int> *) array;
	_array->clear();
}

size_t spine_array_int_get_capacity(spine_array_int array) {
	Array<int> *_array = (Array<int> *) array;
	return _array->getCapacity();
}

size_t spine_array_int_size(spine_array_int array) {
	Array<int> *_array = (Array<int> *) array;
	return _array->size();
}

spine_array_int spine_array_int_set_size(spine_array_int array, size_t newSize, int defaultValue) {
	Array<int> *_array = (Array<int> *) array;
	return (spine_array_int) &_array->setSize(newSize, defaultValue);
}

void spine_array_int_ensure_capacity(spine_array_int array, size_t newCapacity) {
	Array<int> *_array = (Array<int> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_int_add(spine_array_int array, int inValue) {
	Array<int> *_array = (Array<int> *) array;
	_array->add(inValue);
}

void spine_array_int_add_all(spine_array_int array, spine_array_int inValue) {
	Array<int> *_array = (Array<int> *) array;
	_array->addAll(*((const Array<int> *) inValue));
}

void spine_array_int_clear_and_add_all(spine_array_int array, spine_array_int inValue) {
	Array<int> *_array = (Array<int> *) array;
	_array->clearAndAddAll(*((const Array<int> *) inValue));
}

void spine_array_int_remove_at(spine_array_int array, size_t inIndex) {
	Array<int> *_array = (Array<int> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_int_contains(spine_array_int array, int inValue) {
	Array<int> *_array = (Array<int> *) array;
	return _array->contains(inValue);
}

int spine_array_int_index_of(spine_array_int array, int inValue) {
	Array<int> *_array = (Array<int> *) array;
	return _array->indexOf(inValue);
}

/*@null*/ int *spine_array_int_buffer(spine_array_int array) {
	Array<int> *_array = (Array<int> *) array;
	return _array->buffer();
}

spine_array_unsigned_short spine_array_unsigned_short_create(void) {
	return (spine_array_unsigned_short) new (__FILE__, __LINE__) Array<unsigned short>();
}

spine_array_unsigned_short spine_array_unsigned_short_create_with_capacity(size_t initialCapacity) {
	return (spine_array_unsigned_short) new (__FILE__, __LINE__) Array<unsigned short>(initialCapacity);
}
void spine_array_unsigned_short_dispose(spine_array_unsigned_short array) {
	delete (Array<unsigned short> *) array;
}
void spine_array_unsigned_short_clear(spine_array_unsigned_short array) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	_array->clear();
}

size_t spine_array_unsigned_short_get_capacity(spine_array_unsigned_short array) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	return _array->getCapacity();
}

size_t spine_array_unsigned_short_size(spine_array_unsigned_short array) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	return _array->size();
}

spine_array_unsigned_short spine_array_unsigned_short_set_size(spine_array_unsigned_short array, size_t newSize, unsigned short defaultValue) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	return (spine_array_unsigned_short) &_array->setSize(newSize, defaultValue);
}

void spine_array_unsigned_short_ensure_capacity(spine_array_unsigned_short array, size_t newCapacity) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_unsigned_short_add(spine_array_unsigned_short array, unsigned short inValue) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	_array->add(inValue);
}

void spine_array_unsigned_short_add_all(spine_array_unsigned_short array, spine_array_unsigned_short inValue) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	_array->addAll(*((const Array<unsigned short> *) inValue));
}

void spine_array_unsigned_short_clear_and_add_all(spine_array_unsigned_short array, spine_array_unsigned_short inValue) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	_array->clearAndAddAll(*((const Array<unsigned short> *) inValue));
}

void spine_array_unsigned_short_remove_at(spine_array_unsigned_short array, size_t inIndex) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_unsigned_short_contains(spine_array_unsigned_short array, unsigned short inValue) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	return _array->contains(inValue);
}

int spine_array_unsigned_short_index_of(spine_array_unsigned_short array, unsigned short inValue) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	return _array->indexOf(inValue);
}

/*@null*/ unsigned short *spine_array_unsigned_short_buffer(spine_array_unsigned_short array) {
	Array<unsigned short> *_array = (Array<unsigned short> *) array;
	return _array->buffer();
}

spine_array_property_id spine_array_property_id_create(void) {
	return (spine_array_property_id) new (__FILE__, __LINE__) Array<PropertyId>();
}

spine_array_property_id spine_array_property_id_create_with_capacity(size_t initialCapacity) {
	return (spine_array_property_id) new (__FILE__, __LINE__) Array<PropertyId>(initialCapacity);
}
void spine_array_property_id_dispose(spine_array_property_id array) {
	delete (Array<PropertyId> *) array;
}
void spine_array_property_id_clear(spine_array_property_id array) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	_array->clear();
}

size_t spine_array_property_id_get_capacity(spine_array_property_id array) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	return _array->getCapacity();
}

size_t spine_array_property_id_size(spine_array_property_id array) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	return _array->size();
}

spine_array_property_id spine_array_property_id_set_size(spine_array_property_id array, size_t newSize, int64_t defaultValue) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	return (spine_array_property_id) &_array->setSize(newSize, *((const PropertyId *) defaultValue));
}

void spine_array_property_id_ensure_capacity(spine_array_property_id array, size_t newCapacity) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_property_id_add(spine_array_property_id array, int64_t inValue) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	_array->add(*((const PropertyId *) inValue));
}

void spine_array_property_id_add_all(spine_array_property_id array, spine_array_property_id inValue) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	_array->addAll(*((const Array<PropertyId> *) inValue));
}

void spine_array_property_id_clear_and_add_all(spine_array_property_id array, spine_array_property_id inValue) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	_array->clearAndAddAll(*((const Array<PropertyId> *) inValue));
}

void spine_array_property_id_remove_at(spine_array_property_id array, size_t inIndex) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_property_id_contains(spine_array_property_id array, int64_t inValue) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	return _array->contains(*((const PropertyId *) inValue));
}

int spine_array_property_id_index_of(spine_array_property_id array, int64_t inValue) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	return _array->indexOf(*((const PropertyId *) inValue));
}

/*@null*/ int64_t *spine_array_property_id_buffer(spine_array_property_id array) {
	Array<PropertyId> *_array = (Array<PropertyId> *) array;
	return (int64_t *) _array->buffer();
}

spine_array_animation spine_array_animation_create(void) {
	return (spine_array_animation) new (__FILE__, __LINE__) Array<Animation *>();
}

spine_array_animation spine_array_animation_create_with_capacity(size_t initialCapacity) {
	return (spine_array_animation) new (__FILE__, __LINE__) Array<Animation *>(initialCapacity);
}
void spine_array_animation_dispose(spine_array_animation array) {
	delete (Array<Animation *> *) array;
}
void spine_array_animation_clear(spine_array_animation array) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	_array->clear();
}

size_t spine_array_animation_get_capacity(spine_array_animation array) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	return _array->getCapacity();
}

size_t spine_array_animation_size(spine_array_animation array) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	return _array->size();
}

spine_array_animation spine_array_animation_set_size(spine_array_animation array, size_t newSize, spine_animation defaultValue) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	return (spine_array_animation) &_array->setSize(newSize, (Animation *) defaultValue);
}

void spine_array_animation_ensure_capacity(spine_array_animation array, size_t newCapacity) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_animation_add(spine_array_animation array, spine_animation inValue) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	_array->add((Animation *) inValue);
}

void spine_array_animation_add_all(spine_array_animation array, spine_array_animation inValue) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	_array->addAll(*((const Array<Animation *> *) inValue));
}

void spine_array_animation_clear_and_add_all(spine_array_animation array, spine_array_animation inValue) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	_array->clearAndAddAll(*((const Array<Animation *> *) inValue));
}

void spine_array_animation_remove_at(spine_array_animation array, size_t inIndex) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_animation_contains(spine_array_animation array, spine_animation inValue) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	return _array->contains((Animation *) inValue);
}

int spine_array_animation_index_of(spine_array_animation array, spine_animation inValue) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	return _array->indexOf((Animation *) inValue);
}

/*@null*/ spine_animation *spine_array_animation_buffer(spine_array_animation array) {
	Array<Animation *> *_array = (Array<Animation *> *) array;
	return (spine_animation *) _array->buffer();
}

spine_array_atlas_page spine_array_atlas_page_create(void) {
	return (spine_array_atlas_page) new (__FILE__, __LINE__) Array<AtlasPage *>();
}

spine_array_atlas_page spine_array_atlas_page_create_with_capacity(size_t initialCapacity) {
	return (spine_array_atlas_page) new (__FILE__, __LINE__) Array<AtlasPage *>(initialCapacity);
}
void spine_array_atlas_page_dispose(spine_array_atlas_page array) {
	delete (Array<AtlasPage *> *) array;
}
void spine_array_atlas_page_clear(spine_array_atlas_page array) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	_array->clear();
}

size_t spine_array_atlas_page_get_capacity(spine_array_atlas_page array) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	return _array->getCapacity();
}

size_t spine_array_atlas_page_size(spine_array_atlas_page array) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	return _array->size();
}

spine_array_atlas_page spine_array_atlas_page_set_size(spine_array_atlas_page array, size_t newSize, spine_atlas_page defaultValue) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	return (spine_array_atlas_page) &_array->setSize(newSize, (AtlasPage *) defaultValue);
}

void spine_array_atlas_page_ensure_capacity(spine_array_atlas_page array, size_t newCapacity) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_atlas_page_add(spine_array_atlas_page array, spine_atlas_page inValue) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	_array->add((AtlasPage *) inValue);
}

void spine_array_atlas_page_add_all(spine_array_atlas_page array, spine_array_atlas_page inValue) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	_array->addAll(*((const Array<AtlasPage *> *) inValue));
}

void spine_array_atlas_page_clear_and_add_all(spine_array_atlas_page array, spine_array_atlas_page inValue) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	_array->clearAndAddAll(*((const Array<AtlasPage *> *) inValue));
}

void spine_array_atlas_page_remove_at(spine_array_atlas_page array, size_t inIndex) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_atlas_page_contains(spine_array_atlas_page array, spine_atlas_page inValue) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	return _array->contains((AtlasPage *) inValue);
}

int spine_array_atlas_page_index_of(spine_array_atlas_page array, spine_atlas_page inValue) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	return _array->indexOf((AtlasPage *) inValue);
}

/*@null*/ spine_atlas_page *spine_array_atlas_page_buffer(spine_array_atlas_page array) {
	Array<AtlasPage *> *_array = (Array<AtlasPage *> *) array;
	return (spine_atlas_page *) _array->buffer();
}

spine_array_atlas_region spine_array_atlas_region_create(void) {
	return (spine_array_atlas_region) new (__FILE__, __LINE__) Array<AtlasRegion *>();
}

spine_array_atlas_region spine_array_atlas_region_create_with_capacity(size_t initialCapacity) {
	return (spine_array_atlas_region) new (__FILE__, __LINE__) Array<AtlasRegion *>(initialCapacity);
}
void spine_array_atlas_region_dispose(spine_array_atlas_region array) {
	delete (Array<AtlasRegion *> *) array;
}
void spine_array_atlas_region_clear(spine_array_atlas_region array) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	_array->clear();
}

size_t spine_array_atlas_region_get_capacity(spine_array_atlas_region array) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	return _array->getCapacity();
}

size_t spine_array_atlas_region_size(spine_array_atlas_region array) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	return _array->size();
}

spine_array_atlas_region spine_array_atlas_region_set_size(spine_array_atlas_region array, size_t newSize, spine_atlas_region defaultValue) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	return (spine_array_atlas_region) &_array->setSize(newSize, (AtlasRegion *) defaultValue);
}

void spine_array_atlas_region_ensure_capacity(spine_array_atlas_region array, size_t newCapacity) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_atlas_region_add(spine_array_atlas_region array, spine_atlas_region inValue) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	_array->add((AtlasRegion *) inValue);
}

void spine_array_atlas_region_add_all(spine_array_atlas_region array, spine_array_atlas_region inValue) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	_array->addAll(*((const Array<AtlasRegion *> *) inValue));
}

void spine_array_atlas_region_clear_and_add_all(spine_array_atlas_region array, spine_array_atlas_region inValue) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	_array->clearAndAddAll(*((const Array<AtlasRegion *> *) inValue));
}

void spine_array_atlas_region_remove_at(spine_array_atlas_region array, size_t inIndex) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_atlas_region_contains(spine_array_atlas_region array, spine_atlas_region inValue) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	return _array->contains((AtlasRegion *) inValue);
}

int spine_array_atlas_region_index_of(spine_array_atlas_region array, spine_atlas_region inValue) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	return _array->indexOf((AtlasRegion *) inValue);
}

/*@null*/ spine_atlas_region *spine_array_atlas_region_buffer(spine_array_atlas_region array) {
	Array<AtlasRegion *> *_array = (Array<AtlasRegion *> *) array;
	return (spine_atlas_region *) _array->buffer();
}

spine_array_attachment spine_array_attachment_create(void) {
	return (spine_array_attachment) new (__FILE__, __LINE__) Array<Attachment *>();
}

spine_array_attachment spine_array_attachment_create_with_capacity(size_t initialCapacity) {
	return (spine_array_attachment) new (__FILE__, __LINE__) Array<Attachment *>(initialCapacity);
}
void spine_array_attachment_dispose(spine_array_attachment array) {
	delete (Array<Attachment *> *) array;
}
void spine_array_attachment_clear(spine_array_attachment array) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	_array->clear();
}

size_t spine_array_attachment_get_capacity(spine_array_attachment array) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	return _array->getCapacity();
}

size_t spine_array_attachment_size(spine_array_attachment array) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	return _array->size();
}

spine_array_attachment spine_array_attachment_set_size(spine_array_attachment array, size_t newSize, spine_attachment defaultValue) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	return (spine_array_attachment) &_array->setSize(newSize, (Attachment *) defaultValue);
}

void spine_array_attachment_ensure_capacity(spine_array_attachment array, size_t newCapacity) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_attachment_add(spine_array_attachment array, spine_attachment inValue) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	_array->add((Attachment *) inValue);
}

void spine_array_attachment_add_all(spine_array_attachment array, spine_array_attachment inValue) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	_array->addAll(*((const Array<Attachment *> *) inValue));
}

void spine_array_attachment_clear_and_add_all(spine_array_attachment array, spine_array_attachment inValue) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	_array->clearAndAddAll(*((const Array<Attachment *> *) inValue));
}

void spine_array_attachment_remove_at(spine_array_attachment array, size_t inIndex) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_attachment_contains(spine_array_attachment array, spine_attachment inValue) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	return _array->contains((Attachment *) inValue);
}

int spine_array_attachment_index_of(spine_array_attachment array, spine_attachment inValue) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	return _array->indexOf((Attachment *) inValue);
}

/*@null*/ spine_attachment *spine_array_attachment_buffer(spine_array_attachment array) {
	Array<Attachment *> *_array = (Array<Attachment *> *) array;
	return (spine_attachment *) _array->buffer();
}

spine_array_bone spine_array_bone_create(void) {
	return (spine_array_bone) new (__FILE__, __LINE__) Array<Bone *>();
}

spine_array_bone spine_array_bone_create_with_capacity(size_t initialCapacity) {
	return (spine_array_bone) new (__FILE__, __LINE__) Array<Bone *>(initialCapacity);
}
void spine_array_bone_dispose(spine_array_bone array) {
	delete (Array<Bone *> *) array;
}
void spine_array_bone_clear(spine_array_bone array) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	_array->clear();
}

size_t spine_array_bone_get_capacity(spine_array_bone array) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	return _array->getCapacity();
}

size_t spine_array_bone_size(spine_array_bone array) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	return _array->size();
}

spine_array_bone spine_array_bone_set_size(spine_array_bone array, size_t newSize, spine_bone defaultValue) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	return (spine_array_bone) &_array->setSize(newSize, (Bone *) defaultValue);
}

void spine_array_bone_ensure_capacity(spine_array_bone array, size_t newCapacity) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_bone_add(spine_array_bone array, spine_bone inValue) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	_array->add((Bone *) inValue);
}

void spine_array_bone_add_all(spine_array_bone array, spine_array_bone inValue) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	_array->addAll(*((const Array<Bone *> *) inValue));
}

void spine_array_bone_clear_and_add_all(spine_array_bone array, spine_array_bone inValue) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	_array->clearAndAddAll(*((const Array<Bone *> *) inValue));
}

void spine_array_bone_remove_at(spine_array_bone array, size_t inIndex) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_bone_contains(spine_array_bone array, spine_bone inValue) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	return _array->contains((Bone *) inValue);
}

int spine_array_bone_index_of(spine_array_bone array, spine_bone inValue) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	return _array->indexOf((Bone *) inValue);
}

/*@null*/ spine_bone *spine_array_bone_buffer(spine_array_bone array) {
	Array<Bone *> *_array = (Array<Bone *> *) array;
	return (spine_bone *) _array->buffer();
}

spine_array_bone_data spine_array_bone_data_create(void) {
	return (spine_array_bone_data) new (__FILE__, __LINE__) Array<BoneData *>();
}

spine_array_bone_data spine_array_bone_data_create_with_capacity(size_t initialCapacity) {
	return (spine_array_bone_data) new (__FILE__, __LINE__) Array<BoneData *>(initialCapacity);
}
void spine_array_bone_data_dispose(spine_array_bone_data array) {
	delete (Array<BoneData *> *) array;
}
void spine_array_bone_data_clear(spine_array_bone_data array) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	_array->clear();
}

size_t spine_array_bone_data_get_capacity(spine_array_bone_data array) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	return _array->getCapacity();
}

size_t spine_array_bone_data_size(spine_array_bone_data array) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	return _array->size();
}

spine_array_bone_data spine_array_bone_data_set_size(spine_array_bone_data array, size_t newSize, spine_bone_data defaultValue) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	return (spine_array_bone_data) &_array->setSize(newSize, (BoneData *) defaultValue);
}

void spine_array_bone_data_ensure_capacity(spine_array_bone_data array, size_t newCapacity) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_bone_data_add(spine_array_bone_data array, spine_bone_data inValue) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	_array->add((BoneData *) inValue);
}

void spine_array_bone_data_add_all(spine_array_bone_data array, spine_array_bone_data inValue) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	_array->addAll(*((const Array<BoneData *> *) inValue));
}

void spine_array_bone_data_clear_and_add_all(spine_array_bone_data array, spine_array_bone_data inValue) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	_array->clearAndAddAll(*((const Array<BoneData *> *) inValue));
}

void spine_array_bone_data_remove_at(spine_array_bone_data array, size_t inIndex) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_bone_data_contains(spine_array_bone_data array, spine_bone_data inValue) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	return _array->contains((BoneData *) inValue);
}

int spine_array_bone_data_index_of(spine_array_bone_data array, spine_bone_data inValue) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	return _array->indexOf((BoneData *) inValue);
}

/*@null*/ spine_bone_data *spine_array_bone_data_buffer(spine_array_bone_data array) {
	Array<BoneData *> *_array = (Array<BoneData *> *) array;
	return (spine_bone_data *) _array->buffer();
}

spine_array_bone_pose spine_array_bone_pose_create(void) {
	return (spine_array_bone_pose) new (__FILE__, __LINE__) Array<BonePose *>();
}

spine_array_bone_pose spine_array_bone_pose_create_with_capacity(size_t initialCapacity) {
	return (spine_array_bone_pose) new (__FILE__, __LINE__) Array<BonePose *>(initialCapacity);
}
void spine_array_bone_pose_dispose(spine_array_bone_pose array) {
	delete (Array<BonePose *> *) array;
}
void spine_array_bone_pose_clear(spine_array_bone_pose array) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	_array->clear();
}

size_t spine_array_bone_pose_get_capacity(spine_array_bone_pose array) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	return _array->getCapacity();
}

size_t spine_array_bone_pose_size(spine_array_bone_pose array) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	return _array->size();
}

spine_array_bone_pose spine_array_bone_pose_set_size(spine_array_bone_pose array, size_t newSize, spine_bone_pose defaultValue) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	return (spine_array_bone_pose) &_array->setSize(newSize, (BonePose *) defaultValue);
}

void spine_array_bone_pose_ensure_capacity(spine_array_bone_pose array, size_t newCapacity) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_bone_pose_add(spine_array_bone_pose array, spine_bone_pose inValue) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	_array->add((BonePose *) inValue);
}

void spine_array_bone_pose_add_all(spine_array_bone_pose array, spine_array_bone_pose inValue) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	_array->addAll(*((const Array<BonePose *> *) inValue));
}

void spine_array_bone_pose_clear_and_add_all(spine_array_bone_pose array, spine_array_bone_pose inValue) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	_array->clearAndAddAll(*((const Array<BonePose *> *) inValue));
}

void spine_array_bone_pose_remove_at(spine_array_bone_pose array, size_t inIndex) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_bone_pose_contains(spine_array_bone_pose array, spine_bone_pose inValue) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	return _array->contains((BonePose *) inValue);
}

int spine_array_bone_pose_index_of(spine_array_bone_pose array, spine_bone_pose inValue) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	return _array->indexOf((BonePose *) inValue);
}

/*@null*/ spine_bone_pose *spine_array_bone_pose_buffer(spine_array_bone_pose array) {
	Array<BonePose *> *_array = (Array<BonePose *> *) array;
	return (spine_bone_pose *) _array->buffer();
}

spine_array_bounding_box_attachment spine_array_bounding_box_attachment_create(void) {
	return (spine_array_bounding_box_attachment) new (__FILE__, __LINE__) Array<BoundingBoxAttachment *>();
}

spine_array_bounding_box_attachment spine_array_bounding_box_attachment_create_with_capacity(size_t initialCapacity) {
	return (spine_array_bounding_box_attachment) new (__FILE__, __LINE__) Array<BoundingBoxAttachment *>(initialCapacity);
}
void spine_array_bounding_box_attachment_dispose(spine_array_bounding_box_attachment array) {
	delete (Array<BoundingBoxAttachment *> *) array;
}
void spine_array_bounding_box_attachment_clear(spine_array_bounding_box_attachment array) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	_array->clear();
}

size_t spine_array_bounding_box_attachment_get_capacity(spine_array_bounding_box_attachment array) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	return _array->getCapacity();
}

size_t spine_array_bounding_box_attachment_size(spine_array_bounding_box_attachment array) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	return _array->size();
}

spine_array_bounding_box_attachment spine_array_bounding_box_attachment_set_size(spine_array_bounding_box_attachment array, size_t newSize,
																				 spine_bounding_box_attachment defaultValue) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	return (spine_array_bounding_box_attachment) &_array->setSize(newSize, (BoundingBoxAttachment *) defaultValue);
}

void spine_array_bounding_box_attachment_ensure_capacity(spine_array_bounding_box_attachment array, size_t newCapacity) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_bounding_box_attachment_add(spine_array_bounding_box_attachment array, spine_bounding_box_attachment inValue) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	_array->add((BoundingBoxAttachment *) inValue);
}

void spine_array_bounding_box_attachment_add_all(spine_array_bounding_box_attachment array, spine_array_bounding_box_attachment inValue) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	_array->addAll(*((const Array<BoundingBoxAttachment *> *) inValue));
}

void spine_array_bounding_box_attachment_clear_and_add_all(spine_array_bounding_box_attachment array, spine_array_bounding_box_attachment inValue) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	_array->clearAndAddAll(*((const Array<BoundingBoxAttachment *> *) inValue));
}

void spine_array_bounding_box_attachment_remove_at(spine_array_bounding_box_attachment array, size_t inIndex) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_bounding_box_attachment_contains(spine_array_bounding_box_attachment array, spine_bounding_box_attachment inValue) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	return _array->contains((BoundingBoxAttachment *) inValue);
}

int spine_array_bounding_box_attachment_index_of(spine_array_bounding_box_attachment array, spine_bounding_box_attachment inValue) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	return _array->indexOf((BoundingBoxAttachment *) inValue);
}

/*@null*/ spine_bounding_box_attachment *spine_array_bounding_box_attachment_buffer(spine_array_bounding_box_attachment array) {
	Array<BoundingBoxAttachment *> *_array = (Array<BoundingBoxAttachment *> *) array;
	return (spine_bounding_box_attachment *) _array->buffer();
}

spine_array_constraint spine_array_constraint_create(void) {
	return (spine_array_constraint) new (__FILE__, __LINE__) Array<Constraint *>();
}

spine_array_constraint spine_array_constraint_create_with_capacity(size_t initialCapacity) {
	return (spine_array_constraint) new (__FILE__, __LINE__) Array<Constraint *>(initialCapacity);
}
void spine_array_constraint_dispose(spine_array_constraint array) {
	delete (Array<Constraint *> *) array;
}
void spine_array_constraint_clear(spine_array_constraint array) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	_array->clear();
}

size_t spine_array_constraint_get_capacity(spine_array_constraint array) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	return _array->getCapacity();
}

size_t spine_array_constraint_size(spine_array_constraint array) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	return _array->size();
}

spine_array_constraint spine_array_constraint_set_size(spine_array_constraint array, size_t newSize, spine_constraint defaultValue) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	return (spine_array_constraint) &_array->setSize(newSize, (Constraint *) defaultValue);
}

void spine_array_constraint_ensure_capacity(spine_array_constraint array, size_t newCapacity) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_constraint_add(spine_array_constraint array, spine_constraint inValue) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	_array->add((Constraint *) inValue);
}

void spine_array_constraint_add_all(spine_array_constraint array, spine_array_constraint inValue) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	_array->addAll(*((const Array<Constraint *> *) inValue));
}

void spine_array_constraint_clear_and_add_all(spine_array_constraint array, spine_array_constraint inValue) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	_array->clearAndAddAll(*((const Array<Constraint *> *) inValue));
}

void spine_array_constraint_remove_at(spine_array_constraint array, size_t inIndex) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_constraint_contains(spine_array_constraint array, spine_constraint inValue) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	return _array->contains((Constraint *) inValue);
}

int spine_array_constraint_index_of(spine_array_constraint array, spine_constraint inValue) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	return _array->indexOf((Constraint *) inValue);
}

/*@null*/ spine_constraint *spine_array_constraint_buffer(spine_array_constraint array) {
	Array<Constraint *> *_array = (Array<Constraint *> *) array;
	return (spine_constraint *) _array->buffer();
}

spine_array_constraint_data spine_array_constraint_data_create(void) {
	return (spine_array_constraint_data) new (__FILE__, __LINE__) Array<ConstraintData *>();
}

spine_array_constraint_data spine_array_constraint_data_create_with_capacity(size_t initialCapacity) {
	return (spine_array_constraint_data) new (__FILE__, __LINE__) Array<ConstraintData *>(initialCapacity);
}
void spine_array_constraint_data_dispose(spine_array_constraint_data array) {
	delete (Array<ConstraintData *> *) array;
}
void spine_array_constraint_data_clear(spine_array_constraint_data array) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	_array->clear();
}

size_t spine_array_constraint_data_get_capacity(spine_array_constraint_data array) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	return _array->getCapacity();
}

size_t spine_array_constraint_data_size(spine_array_constraint_data array) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	return _array->size();
}

spine_array_constraint_data spine_array_constraint_data_set_size(spine_array_constraint_data array, size_t newSize,
																 spine_constraint_data defaultValue) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	return (spine_array_constraint_data) &_array->setSize(newSize, (ConstraintData *) defaultValue);
}

void spine_array_constraint_data_ensure_capacity(spine_array_constraint_data array, size_t newCapacity) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_constraint_data_add(spine_array_constraint_data array, spine_constraint_data inValue) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	_array->add((ConstraintData *) inValue);
}

void spine_array_constraint_data_add_all(spine_array_constraint_data array, spine_array_constraint_data inValue) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	_array->addAll(*((const Array<ConstraintData *> *) inValue));
}

void spine_array_constraint_data_clear_and_add_all(spine_array_constraint_data array, spine_array_constraint_data inValue) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	_array->clearAndAddAll(*((const Array<ConstraintData *> *) inValue));
}

void spine_array_constraint_data_remove_at(spine_array_constraint_data array, size_t inIndex) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_constraint_data_contains(spine_array_constraint_data array, spine_constraint_data inValue) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	return _array->contains((ConstraintData *) inValue);
}

int spine_array_constraint_data_index_of(spine_array_constraint_data array, spine_constraint_data inValue) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	return _array->indexOf((ConstraintData *) inValue);
}

/*@null*/ spine_constraint_data *spine_array_constraint_data_buffer(spine_array_constraint_data array) {
	Array<ConstraintData *> *_array = (Array<ConstraintData *> *) array;
	return (spine_constraint_data *) _array->buffer();
}

spine_array_event spine_array_event_create(void) {
	return (spine_array_event) new (__FILE__, __LINE__) Array<Event *>();
}

spine_array_event spine_array_event_create_with_capacity(size_t initialCapacity) {
	return (spine_array_event) new (__FILE__, __LINE__) Array<Event *>(initialCapacity);
}
void spine_array_event_dispose(spine_array_event array) {
	delete (Array<Event *> *) array;
}
void spine_array_event_clear(spine_array_event array) {
	Array<Event *> *_array = (Array<Event *> *) array;
	_array->clear();
}

size_t spine_array_event_get_capacity(spine_array_event array) {
	Array<Event *> *_array = (Array<Event *> *) array;
	return _array->getCapacity();
}

size_t spine_array_event_size(spine_array_event array) {
	Array<Event *> *_array = (Array<Event *> *) array;
	return _array->size();
}

spine_array_event spine_array_event_set_size(spine_array_event array, size_t newSize, spine_event defaultValue) {
	Array<Event *> *_array = (Array<Event *> *) array;
	return (spine_array_event) &_array->setSize(newSize, (Event *) defaultValue);
}

void spine_array_event_ensure_capacity(spine_array_event array, size_t newCapacity) {
	Array<Event *> *_array = (Array<Event *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_event_add(spine_array_event array, spine_event inValue) {
	Array<Event *> *_array = (Array<Event *> *) array;
	_array->add((Event *) inValue);
}

void spine_array_event_add_all(spine_array_event array, spine_array_event inValue) {
	Array<Event *> *_array = (Array<Event *> *) array;
	_array->addAll(*((const Array<Event *> *) inValue));
}

void spine_array_event_clear_and_add_all(spine_array_event array, spine_array_event inValue) {
	Array<Event *> *_array = (Array<Event *> *) array;
	_array->clearAndAddAll(*((const Array<Event *> *) inValue));
}

void spine_array_event_remove_at(spine_array_event array, size_t inIndex) {
	Array<Event *> *_array = (Array<Event *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_event_contains(spine_array_event array, spine_event inValue) {
	Array<Event *> *_array = (Array<Event *> *) array;
	return _array->contains((Event *) inValue);
}

int spine_array_event_index_of(spine_array_event array, spine_event inValue) {
	Array<Event *> *_array = (Array<Event *> *) array;
	return _array->indexOf((Event *) inValue);
}

/*@null*/ spine_event *spine_array_event_buffer(spine_array_event array) {
	Array<Event *> *_array = (Array<Event *> *) array;
	return (spine_event *) _array->buffer();
}

spine_array_event_data spine_array_event_data_create(void) {
	return (spine_array_event_data) new (__FILE__, __LINE__) Array<EventData *>();
}

spine_array_event_data spine_array_event_data_create_with_capacity(size_t initialCapacity) {
	return (spine_array_event_data) new (__FILE__, __LINE__) Array<EventData *>(initialCapacity);
}
void spine_array_event_data_dispose(spine_array_event_data array) {
	delete (Array<EventData *> *) array;
}
void spine_array_event_data_clear(spine_array_event_data array) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	_array->clear();
}

size_t spine_array_event_data_get_capacity(spine_array_event_data array) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	return _array->getCapacity();
}

size_t spine_array_event_data_size(spine_array_event_data array) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	return _array->size();
}

spine_array_event_data spine_array_event_data_set_size(spine_array_event_data array, size_t newSize, spine_event_data defaultValue) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	return (spine_array_event_data) &_array->setSize(newSize, (EventData *) defaultValue);
}

void spine_array_event_data_ensure_capacity(spine_array_event_data array, size_t newCapacity) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_event_data_add(spine_array_event_data array, spine_event_data inValue) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	_array->add((EventData *) inValue);
}

void spine_array_event_data_add_all(spine_array_event_data array, spine_array_event_data inValue) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	_array->addAll(*((const Array<EventData *> *) inValue));
}

void spine_array_event_data_clear_and_add_all(spine_array_event_data array, spine_array_event_data inValue) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	_array->clearAndAddAll(*((const Array<EventData *> *) inValue));
}

void spine_array_event_data_remove_at(spine_array_event_data array, size_t inIndex) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_event_data_contains(spine_array_event_data array, spine_event_data inValue) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	return _array->contains((EventData *) inValue);
}

int spine_array_event_data_index_of(spine_array_event_data array, spine_event_data inValue) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	return _array->indexOf((EventData *) inValue);
}

/*@null*/ spine_event_data *spine_array_event_data_buffer(spine_array_event_data array) {
	Array<EventData *> *_array = (Array<EventData *> *) array;
	return (spine_event_data *) _array->buffer();
}

spine_array_from_property spine_array_from_property_create(void) {
	return (spine_array_from_property) new (__FILE__, __LINE__) Array<FromProperty *>();
}

spine_array_from_property spine_array_from_property_create_with_capacity(size_t initialCapacity) {
	return (spine_array_from_property) new (__FILE__, __LINE__) Array<FromProperty *>(initialCapacity);
}
void spine_array_from_property_dispose(spine_array_from_property array) {
	delete (Array<FromProperty *> *) array;
}
void spine_array_from_property_clear(spine_array_from_property array) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	_array->clear();
}

size_t spine_array_from_property_get_capacity(spine_array_from_property array) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	return _array->getCapacity();
}

size_t spine_array_from_property_size(spine_array_from_property array) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	return _array->size();
}

spine_array_from_property spine_array_from_property_set_size(spine_array_from_property array, size_t newSize, spine_from_property defaultValue) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	return (spine_array_from_property) &_array->setSize(newSize, (FromProperty *) defaultValue);
}

void spine_array_from_property_ensure_capacity(spine_array_from_property array, size_t newCapacity) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_from_property_add(spine_array_from_property array, spine_from_property inValue) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	_array->add((FromProperty *) inValue);
}

void spine_array_from_property_add_all(spine_array_from_property array, spine_array_from_property inValue) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	_array->addAll(*((const Array<FromProperty *> *) inValue));
}

void spine_array_from_property_clear_and_add_all(spine_array_from_property array, spine_array_from_property inValue) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	_array->clearAndAddAll(*((const Array<FromProperty *> *) inValue));
}

void spine_array_from_property_remove_at(spine_array_from_property array, size_t inIndex) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_from_property_contains(spine_array_from_property array, spine_from_property inValue) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	return _array->contains((FromProperty *) inValue);
}

int spine_array_from_property_index_of(spine_array_from_property array, spine_from_property inValue) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	return _array->indexOf((FromProperty *) inValue);
}

/*@null*/ spine_from_property *spine_array_from_property_buffer(spine_array_from_property array) {
	Array<FromProperty *> *_array = (Array<FromProperty *> *) array;
	return (spine_from_property *) _array->buffer();
}

spine_array_physics_constraint spine_array_physics_constraint_create(void) {
	return (spine_array_physics_constraint) new (__FILE__, __LINE__) Array<PhysicsConstraint *>();
}

spine_array_physics_constraint spine_array_physics_constraint_create_with_capacity(size_t initialCapacity) {
	return (spine_array_physics_constraint) new (__FILE__, __LINE__) Array<PhysicsConstraint *>(initialCapacity);
}
void spine_array_physics_constraint_dispose(spine_array_physics_constraint array) {
	delete (Array<PhysicsConstraint *> *) array;
}
void spine_array_physics_constraint_clear(spine_array_physics_constraint array) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	_array->clear();
}

size_t spine_array_physics_constraint_get_capacity(spine_array_physics_constraint array) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	return _array->getCapacity();
}

size_t spine_array_physics_constraint_size(spine_array_physics_constraint array) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	return _array->size();
}

spine_array_physics_constraint spine_array_physics_constraint_set_size(spine_array_physics_constraint array, size_t newSize,
																	   spine_physics_constraint defaultValue) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	return (spine_array_physics_constraint) &_array->setSize(newSize, (PhysicsConstraint *) defaultValue);
}

void spine_array_physics_constraint_ensure_capacity(spine_array_physics_constraint array, size_t newCapacity) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_physics_constraint_add(spine_array_physics_constraint array, spine_physics_constraint inValue) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	_array->add((PhysicsConstraint *) inValue);
}

void spine_array_physics_constraint_add_all(spine_array_physics_constraint array, spine_array_physics_constraint inValue) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	_array->addAll(*((const Array<PhysicsConstraint *> *) inValue));
}

void spine_array_physics_constraint_clear_and_add_all(spine_array_physics_constraint array, spine_array_physics_constraint inValue) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	_array->clearAndAddAll(*((const Array<PhysicsConstraint *> *) inValue));
}

void spine_array_physics_constraint_remove_at(spine_array_physics_constraint array, size_t inIndex) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_physics_constraint_contains(spine_array_physics_constraint array, spine_physics_constraint inValue) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	return _array->contains((PhysicsConstraint *) inValue);
}

int spine_array_physics_constraint_index_of(spine_array_physics_constraint array, spine_physics_constraint inValue) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	return _array->indexOf((PhysicsConstraint *) inValue);
}

/*@null*/ spine_physics_constraint *spine_array_physics_constraint_buffer(spine_array_physics_constraint array) {
	Array<PhysicsConstraint *> *_array = (Array<PhysicsConstraint *> *) array;
	return (spine_physics_constraint *) _array->buffer();
}

spine_array_polygon spine_array_polygon_create(void) {
	return (spine_array_polygon) new (__FILE__, __LINE__) Array<Polygon *>();
}

spine_array_polygon spine_array_polygon_create_with_capacity(size_t initialCapacity) {
	return (spine_array_polygon) new (__FILE__, __LINE__) Array<Polygon *>(initialCapacity);
}
void spine_array_polygon_dispose(spine_array_polygon array) {
	delete (Array<Polygon *> *) array;
}
void spine_array_polygon_clear(spine_array_polygon array) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	_array->clear();
}

size_t spine_array_polygon_get_capacity(spine_array_polygon array) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	return _array->getCapacity();
}

size_t spine_array_polygon_size(spine_array_polygon array) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	return _array->size();
}

spine_array_polygon spine_array_polygon_set_size(spine_array_polygon array, size_t newSize, spine_polygon defaultValue) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	return (spine_array_polygon) &_array->setSize(newSize, (Polygon *) defaultValue);
}

void spine_array_polygon_ensure_capacity(spine_array_polygon array, size_t newCapacity) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_polygon_add(spine_array_polygon array, spine_polygon inValue) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	_array->add((Polygon *) inValue);
}

void spine_array_polygon_add_all(spine_array_polygon array, spine_array_polygon inValue) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	_array->addAll(*((const Array<Polygon *> *) inValue));
}

void spine_array_polygon_clear_and_add_all(spine_array_polygon array, spine_array_polygon inValue) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	_array->clearAndAddAll(*((const Array<Polygon *> *) inValue));
}

void spine_array_polygon_remove_at(spine_array_polygon array, size_t inIndex) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_polygon_contains(spine_array_polygon array, spine_polygon inValue) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	return _array->contains((Polygon *) inValue);
}

int spine_array_polygon_index_of(spine_array_polygon array, spine_polygon inValue) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	return _array->indexOf((Polygon *) inValue);
}

/*@null*/ spine_polygon *spine_array_polygon_buffer(spine_array_polygon array) {
	Array<Polygon *> *_array = (Array<Polygon *> *) array;
	return (spine_polygon *) _array->buffer();
}

spine_array_skin spine_array_skin_create(void) {
	return (spine_array_skin) new (__FILE__, __LINE__) Array<Skin *>();
}

spine_array_skin spine_array_skin_create_with_capacity(size_t initialCapacity) {
	return (spine_array_skin) new (__FILE__, __LINE__) Array<Skin *>(initialCapacity);
}
void spine_array_skin_dispose(spine_array_skin array) {
	delete (Array<Skin *> *) array;
}
void spine_array_skin_clear(spine_array_skin array) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	_array->clear();
}

size_t spine_array_skin_get_capacity(spine_array_skin array) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	return _array->getCapacity();
}

size_t spine_array_skin_size(spine_array_skin array) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	return _array->size();
}

spine_array_skin spine_array_skin_set_size(spine_array_skin array, size_t newSize, spine_skin defaultValue) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	return (spine_array_skin) &_array->setSize(newSize, (Skin *) defaultValue);
}

void spine_array_skin_ensure_capacity(spine_array_skin array, size_t newCapacity) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_skin_add(spine_array_skin array, spine_skin inValue) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	_array->add((Skin *) inValue);
}

void spine_array_skin_add_all(spine_array_skin array, spine_array_skin inValue) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	_array->addAll(*((const Array<Skin *> *) inValue));
}

void spine_array_skin_clear_and_add_all(spine_array_skin array, spine_array_skin inValue) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	_array->clearAndAddAll(*((const Array<Skin *> *) inValue));
}

void spine_array_skin_remove_at(spine_array_skin array, size_t inIndex) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_skin_contains(spine_array_skin array, spine_skin inValue) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	return _array->contains((Skin *) inValue);
}

int spine_array_skin_index_of(spine_array_skin array, spine_skin inValue) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	return _array->indexOf((Skin *) inValue);
}

/*@null*/ spine_skin *spine_array_skin_buffer(spine_array_skin array) {
	Array<Skin *> *_array = (Array<Skin *> *) array;
	return (spine_skin *) _array->buffer();
}

spine_array_slot spine_array_slot_create(void) {
	return (spine_array_slot) new (__FILE__, __LINE__) Array<Slot *>();
}

spine_array_slot spine_array_slot_create_with_capacity(size_t initialCapacity) {
	return (spine_array_slot) new (__FILE__, __LINE__) Array<Slot *>(initialCapacity);
}
void spine_array_slot_dispose(spine_array_slot array) {
	delete (Array<Slot *> *) array;
}
void spine_array_slot_clear(spine_array_slot array) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	_array->clear();
}

size_t spine_array_slot_get_capacity(spine_array_slot array) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	return _array->getCapacity();
}

size_t spine_array_slot_size(spine_array_slot array) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	return _array->size();
}

spine_array_slot spine_array_slot_set_size(spine_array_slot array, size_t newSize, spine_slot defaultValue) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	return (spine_array_slot) &_array->setSize(newSize, (Slot *) defaultValue);
}

void spine_array_slot_ensure_capacity(spine_array_slot array, size_t newCapacity) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_slot_add(spine_array_slot array, spine_slot inValue) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	_array->add((Slot *) inValue);
}

void spine_array_slot_add_all(spine_array_slot array, spine_array_slot inValue) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	_array->addAll(*((const Array<Slot *> *) inValue));
}

void spine_array_slot_clear_and_add_all(spine_array_slot array, spine_array_slot inValue) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	_array->clearAndAddAll(*((const Array<Slot *> *) inValue));
}

void spine_array_slot_remove_at(spine_array_slot array, size_t inIndex) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_slot_contains(spine_array_slot array, spine_slot inValue) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	return _array->contains((Slot *) inValue);
}

int spine_array_slot_index_of(spine_array_slot array, spine_slot inValue) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	return _array->indexOf((Slot *) inValue);
}

/*@null*/ spine_slot *spine_array_slot_buffer(spine_array_slot array) {
	Array<Slot *> *_array = (Array<Slot *> *) array;
	return (spine_slot *) _array->buffer();
}

spine_array_slot_data spine_array_slot_data_create(void) {
	return (spine_array_slot_data) new (__FILE__, __LINE__) Array<SlotData *>();
}

spine_array_slot_data spine_array_slot_data_create_with_capacity(size_t initialCapacity) {
	return (spine_array_slot_data) new (__FILE__, __LINE__) Array<SlotData *>(initialCapacity);
}
void spine_array_slot_data_dispose(spine_array_slot_data array) {
	delete (Array<SlotData *> *) array;
}
void spine_array_slot_data_clear(spine_array_slot_data array) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	_array->clear();
}

size_t spine_array_slot_data_get_capacity(spine_array_slot_data array) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	return _array->getCapacity();
}

size_t spine_array_slot_data_size(spine_array_slot_data array) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	return _array->size();
}

spine_array_slot_data spine_array_slot_data_set_size(spine_array_slot_data array, size_t newSize, spine_slot_data defaultValue) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	return (spine_array_slot_data) &_array->setSize(newSize, (SlotData *) defaultValue);
}

void spine_array_slot_data_ensure_capacity(spine_array_slot_data array, size_t newCapacity) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_slot_data_add(spine_array_slot_data array, spine_slot_data inValue) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	_array->add((SlotData *) inValue);
}

void spine_array_slot_data_add_all(spine_array_slot_data array, spine_array_slot_data inValue) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	_array->addAll(*((const Array<SlotData *> *) inValue));
}

void spine_array_slot_data_clear_and_add_all(spine_array_slot_data array, spine_array_slot_data inValue) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	_array->clearAndAddAll(*((const Array<SlotData *> *) inValue));
}

void spine_array_slot_data_remove_at(spine_array_slot_data array, size_t inIndex) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_slot_data_contains(spine_array_slot_data array, spine_slot_data inValue) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	return _array->contains((SlotData *) inValue);
}

int spine_array_slot_data_index_of(spine_array_slot_data array, spine_slot_data inValue) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	return _array->indexOf((SlotData *) inValue);
}

/*@null*/ spine_slot_data *spine_array_slot_data_buffer(spine_array_slot_data array) {
	Array<SlotData *> *_array = (Array<SlotData *> *) array;
	return (spine_slot_data *) _array->buffer();
}

spine_array_texture_region spine_array_texture_region_create(void) {
	return (spine_array_texture_region) new (__FILE__, __LINE__) Array<TextureRegion *>();
}

spine_array_texture_region spine_array_texture_region_create_with_capacity(size_t initialCapacity) {
	return (spine_array_texture_region) new (__FILE__, __LINE__) Array<TextureRegion *>(initialCapacity);
}
void spine_array_texture_region_dispose(spine_array_texture_region array) {
	delete (Array<TextureRegion *> *) array;
}
void spine_array_texture_region_clear(spine_array_texture_region array) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	_array->clear();
}

size_t spine_array_texture_region_get_capacity(spine_array_texture_region array) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	return _array->getCapacity();
}

size_t spine_array_texture_region_size(spine_array_texture_region array) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	return _array->size();
}

spine_array_texture_region spine_array_texture_region_set_size(spine_array_texture_region array, size_t newSize, spine_texture_region defaultValue) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	return (spine_array_texture_region) &_array->setSize(newSize, (TextureRegion *) defaultValue);
}

void spine_array_texture_region_ensure_capacity(spine_array_texture_region array, size_t newCapacity) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_texture_region_add(spine_array_texture_region array, spine_texture_region inValue) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	_array->add((TextureRegion *) inValue);
}

void spine_array_texture_region_add_all(spine_array_texture_region array, spine_array_texture_region inValue) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	_array->addAll(*((const Array<TextureRegion *> *) inValue));
}

void spine_array_texture_region_clear_and_add_all(spine_array_texture_region array, spine_array_texture_region inValue) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	_array->clearAndAddAll(*((const Array<TextureRegion *> *) inValue));
}

void spine_array_texture_region_remove_at(spine_array_texture_region array, size_t inIndex) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_texture_region_contains(spine_array_texture_region array, spine_texture_region inValue) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	return _array->contains((TextureRegion *) inValue);
}

int spine_array_texture_region_index_of(spine_array_texture_region array, spine_texture_region inValue) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	return _array->indexOf((TextureRegion *) inValue);
}

/*@null*/ spine_texture_region *spine_array_texture_region_buffer(spine_array_texture_region array) {
	Array<TextureRegion *> *_array = (Array<TextureRegion *> *) array;
	return (spine_texture_region *) _array->buffer();
}

spine_array_timeline spine_array_timeline_create(void) {
	return (spine_array_timeline) new (__FILE__, __LINE__) Array<Timeline *>();
}

spine_array_timeline spine_array_timeline_create_with_capacity(size_t initialCapacity) {
	return (spine_array_timeline) new (__FILE__, __LINE__) Array<Timeline *>(initialCapacity);
}
void spine_array_timeline_dispose(spine_array_timeline array) {
	delete (Array<Timeline *> *) array;
}
void spine_array_timeline_clear(spine_array_timeline array) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	_array->clear();
}

size_t spine_array_timeline_get_capacity(spine_array_timeline array) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	return _array->getCapacity();
}

size_t spine_array_timeline_size(spine_array_timeline array) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	return _array->size();
}

spine_array_timeline spine_array_timeline_set_size(spine_array_timeline array, size_t newSize, spine_timeline defaultValue) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	return (spine_array_timeline) &_array->setSize(newSize, (Timeline *) defaultValue);
}

void spine_array_timeline_ensure_capacity(spine_array_timeline array, size_t newCapacity) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_timeline_add(spine_array_timeline array, spine_timeline inValue) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	_array->add((Timeline *) inValue);
}

void spine_array_timeline_add_all(spine_array_timeline array, spine_array_timeline inValue) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	_array->addAll(*((const Array<Timeline *> *) inValue));
}

void spine_array_timeline_clear_and_add_all(spine_array_timeline array, spine_array_timeline inValue) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	_array->clearAndAddAll(*((const Array<Timeline *> *) inValue));
}

void spine_array_timeline_remove_at(spine_array_timeline array, size_t inIndex) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_timeline_contains(spine_array_timeline array, spine_timeline inValue) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	return _array->contains((Timeline *) inValue);
}

int spine_array_timeline_index_of(spine_array_timeline array, spine_timeline inValue) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	return _array->indexOf((Timeline *) inValue);
}

/*@null*/ spine_timeline *spine_array_timeline_buffer(spine_array_timeline array) {
	Array<Timeline *> *_array = (Array<Timeline *> *) array;
	return (spine_timeline *) _array->buffer();
}

spine_array_to_property spine_array_to_property_create(void) {
	return (spine_array_to_property) new (__FILE__, __LINE__) Array<ToProperty *>();
}

spine_array_to_property spine_array_to_property_create_with_capacity(size_t initialCapacity) {
	return (spine_array_to_property) new (__FILE__, __LINE__) Array<ToProperty *>(initialCapacity);
}
void spine_array_to_property_dispose(spine_array_to_property array) {
	delete (Array<ToProperty *> *) array;
}
void spine_array_to_property_clear(spine_array_to_property array) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	_array->clear();
}

size_t spine_array_to_property_get_capacity(spine_array_to_property array) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	return _array->getCapacity();
}

size_t spine_array_to_property_size(spine_array_to_property array) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	return _array->size();
}

spine_array_to_property spine_array_to_property_set_size(spine_array_to_property array, size_t newSize, spine_to_property defaultValue) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	return (spine_array_to_property) &_array->setSize(newSize, (ToProperty *) defaultValue);
}

void spine_array_to_property_ensure_capacity(spine_array_to_property array, size_t newCapacity) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_to_property_add(spine_array_to_property array, spine_to_property inValue) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	_array->add((ToProperty *) inValue);
}

void spine_array_to_property_add_all(spine_array_to_property array, spine_array_to_property inValue) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	_array->addAll(*((const Array<ToProperty *> *) inValue));
}

void spine_array_to_property_clear_and_add_all(spine_array_to_property array, spine_array_to_property inValue) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	_array->clearAndAddAll(*((const Array<ToProperty *> *) inValue));
}

void spine_array_to_property_remove_at(spine_array_to_property array, size_t inIndex) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_to_property_contains(spine_array_to_property array, spine_to_property inValue) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	return _array->contains((ToProperty *) inValue);
}

int spine_array_to_property_index_of(spine_array_to_property array, spine_to_property inValue) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	return _array->indexOf((ToProperty *) inValue);
}

/*@null*/ spine_to_property *spine_array_to_property_buffer(spine_array_to_property array) {
	Array<ToProperty *> *_array = (Array<ToProperty *> *) array;
	return (spine_to_property *) _array->buffer();
}

spine_array_track_entry spine_array_track_entry_create(void) {
	return (spine_array_track_entry) new (__FILE__, __LINE__) Array<TrackEntry *>();
}

spine_array_track_entry spine_array_track_entry_create_with_capacity(size_t initialCapacity) {
	return (spine_array_track_entry) new (__FILE__, __LINE__) Array<TrackEntry *>(initialCapacity);
}
void spine_array_track_entry_dispose(spine_array_track_entry array) {
	delete (Array<TrackEntry *> *) array;
}
void spine_array_track_entry_clear(spine_array_track_entry array) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	_array->clear();
}

size_t spine_array_track_entry_get_capacity(spine_array_track_entry array) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	return _array->getCapacity();
}

size_t spine_array_track_entry_size(spine_array_track_entry array) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	return _array->size();
}

spine_array_track_entry spine_array_track_entry_set_size(spine_array_track_entry array, size_t newSize, spine_track_entry defaultValue) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	return (spine_array_track_entry) &_array->setSize(newSize, (TrackEntry *) defaultValue);
}

void spine_array_track_entry_ensure_capacity(spine_array_track_entry array, size_t newCapacity) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_track_entry_add(spine_array_track_entry array, spine_track_entry inValue) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	_array->add((TrackEntry *) inValue);
}

void spine_array_track_entry_add_all(spine_array_track_entry array, spine_array_track_entry inValue) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	_array->addAll(*((const Array<TrackEntry *> *) inValue));
}

void spine_array_track_entry_clear_and_add_all(spine_array_track_entry array, spine_array_track_entry inValue) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	_array->clearAndAddAll(*((const Array<TrackEntry *> *) inValue));
}

void spine_array_track_entry_remove_at(spine_array_track_entry array, size_t inIndex) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_track_entry_contains(spine_array_track_entry array, spine_track_entry inValue) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	return _array->contains((TrackEntry *) inValue);
}

int spine_array_track_entry_index_of(spine_array_track_entry array, spine_track_entry inValue) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	return _array->indexOf((TrackEntry *) inValue);
}

/*@null*/ spine_track_entry *spine_array_track_entry_buffer(spine_array_track_entry array) {
	Array<TrackEntry *> *_array = (Array<TrackEntry *> *) array;
	return (spine_track_entry *) _array->buffer();
}

spine_array_update spine_array_update_create(void) {
	return (spine_array_update) new (__FILE__, __LINE__) Array<Update *>();
}

spine_array_update spine_array_update_create_with_capacity(size_t initialCapacity) {
	return (spine_array_update) new (__FILE__, __LINE__) Array<Update *>(initialCapacity);
}
void spine_array_update_dispose(spine_array_update array) {
	delete (Array<Update *> *) array;
}
void spine_array_update_clear(spine_array_update array) {
	Array<Update *> *_array = (Array<Update *> *) array;
	_array->clear();
}

size_t spine_array_update_get_capacity(spine_array_update array) {
	Array<Update *> *_array = (Array<Update *> *) array;
	return _array->getCapacity();
}

size_t spine_array_update_size(spine_array_update array) {
	Array<Update *> *_array = (Array<Update *> *) array;
	return _array->size();
}

spine_array_update spine_array_update_set_size(spine_array_update array, size_t newSize, spine_update defaultValue) {
	Array<Update *> *_array = (Array<Update *> *) array;
	return (spine_array_update) &_array->setSize(newSize, (Update *) defaultValue);
}

void spine_array_update_ensure_capacity(spine_array_update array, size_t newCapacity) {
	Array<Update *> *_array = (Array<Update *> *) array;
	_array->ensureCapacity(newCapacity);
}

void spine_array_update_add(spine_array_update array, spine_update inValue) {
	Array<Update *> *_array = (Array<Update *> *) array;
	_array->add((Update *) inValue);
}

void spine_array_update_add_all(spine_array_update array, spine_array_update inValue) {
	Array<Update *> *_array = (Array<Update *> *) array;
	_array->addAll(*((const Array<Update *> *) inValue));
}

void spine_array_update_clear_and_add_all(spine_array_update array, spine_array_update inValue) {
	Array<Update *> *_array = (Array<Update *> *) array;
	_array->clearAndAddAll(*((const Array<Update *> *) inValue));
}

void spine_array_update_remove_at(spine_array_update array, size_t inIndex) {
	Array<Update *> *_array = (Array<Update *> *) array;
	_array->removeAt(inIndex);
}

bool spine_array_update_contains(spine_array_update array, spine_update inValue) {
	Array<Update *> *_array = (Array<Update *> *) array;
	return _array->contains((Update *) inValue);
}

int spine_array_update_index_of(spine_array_update array, spine_update inValue) {
	Array<Update *> *_array = (Array<Update *> *) array;
	return _array->indexOf((Update *) inValue);
}

/*@null*/ spine_update *spine_array_update_buffer(spine_array_update array) {
	Array<Update *> *_array = (Array<Update *> *) array;
	return (spine_update *) _array->buffer();
}
