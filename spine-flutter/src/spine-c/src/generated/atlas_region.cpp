#include "atlas_region.h"
#include <spine/spine.h>

using namespace spine;

spine_atlas_region spine_atlas_region_create(void) {
	return (spine_atlas_region) new (__FILE__, __LINE__) AtlasRegion();
}

void spine_atlas_region_dispose(spine_atlas_region self) {
	delete (AtlasRegion *) self;
}

spine_rtti spine_atlas_region_get_rtti(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return (spine_rtti) &_self->getRTTI();
}

/*@null*/ spine_atlas_page spine_atlas_region_get_page(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return (spine_atlas_page) _self->getPage();
}

const char *spine_atlas_region_get_name(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getName().buffer();
}

int spine_atlas_region_get_index(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getIndex();
}

int spine_atlas_region_get_x(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getX();
}

int spine_atlas_region_get_y(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getY();
}

float spine_atlas_region_get_offset_x(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getOffsetX();
}

float spine_atlas_region_get_offset_y(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getOffsetY();
}

int spine_atlas_region_get_packed_width(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getPackedWidth();
}

int spine_atlas_region_get_packed_height(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getPackedHeight();
}

int spine_atlas_region_get_original_width(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getOriginalWidth();
}

int spine_atlas_region_get_original_height(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getOriginalHeight();
}

bool spine_atlas_region_get_rotate(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getRotate();
}

int spine_atlas_region_get_degrees(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getDegrees();
}

spine_array_int spine_atlas_region_get_splits(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return (spine_array_int) &_self->getSplits();
}

spine_array_int spine_atlas_region_get_pads(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return (spine_array_int) &_self->getPads();
}

spine_array_float spine_atlas_region_get_values(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return (spine_array_float) &_self->getValues();
}

void spine_atlas_region_set_page(spine_atlas_region self, /*@null*/ spine_atlas_page value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setPage((AtlasPage *) value);
}

void spine_atlas_region_set_name(spine_atlas_region self, const char *value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setName(String(value));
}

void spine_atlas_region_set_index(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setIndex(value);
}

void spine_atlas_region_set_x(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setX(value);
}

void spine_atlas_region_set_y(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setY(value);
}

void spine_atlas_region_set_offset_x(spine_atlas_region self, float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setOffsetX(value);
}

void spine_atlas_region_set_offset_y(spine_atlas_region self, float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setOffsetY(value);
}

void spine_atlas_region_set_packed_width(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setPackedWidth(value);
}

void spine_atlas_region_set_packed_height(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setPackedHeight(value);
}

void spine_atlas_region_set_original_width(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setOriginalWidth(value);
}

void spine_atlas_region_set_original_height(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setOriginalHeight(value);
}

void spine_atlas_region_set_rotate(spine_atlas_region self, bool value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setRotate(value);
}

void spine_atlas_region_set_degrees(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setDegrees(value);
}

void spine_atlas_region_set_splits(spine_atlas_region self, spine_array_int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setSplits(*((const Array<int> *) value));
}

void spine_atlas_region_set_pads(spine_atlas_region self, spine_array_int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setPads(*((const Array<int> *) value));
}

void spine_atlas_region_set_values(spine_atlas_region self, spine_array_float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setValues(*((const Array<float> *) value));
}

float spine_atlas_region_get_u(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getU();
}

void spine_atlas_region_set_u(spine_atlas_region self, float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setU(value);
}

float spine_atlas_region_get_v(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getV();
}

void spine_atlas_region_set_v(spine_atlas_region self, float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setV(value);
}

float spine_atlas_region_get_u2(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getU2();
}

void spine_atlas_region_set_u2(spine_atlas_region self, float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setU2(value);
}

float spine_atlas_region_get_v2(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getV2();
}

void spine_atlas_region_set_v2(spine_atlas_region self, float value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setV2(value);
}

int spine_atlas_region_get_region_width(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getRegionWidth();
}

void spine_atlas_region_set_region_width(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setRegionWidth(value);
}

int spine_atlas_region_get_region_height(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getRegionHeight();
}

void spine_atlas_region_set_region_height(spine_atlas_region self, int value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setRegionHeight(value);
}

/*@null*/ void *spine_atlas_region_get_renderer_object(spine_atlas_region self) {
	AtlasRegion *_self = (AtlasRegion *) self;
	return _self->getRendererObject();
}

void spine_atlas_region_set_renderer_object(spine_atlas_region self, /*@null*/ void *value) {
	AtlasRegion *_self = (AtlasRegion *) self;
	_self->setRendererObject(value);
}

spine_rtti spine_atlas_region_rtti(void) {
	return (spine_rtti) &AtlasRegion::rtti;
}
