#include "texture_region.h"
#include <spine/spine.h>

using namespace spine;

spine_texture_region spine_texture_region_create(void) {
	return (spine_texture_region) new (__FILE__, __LINE__) TextureRegion();
}

void spine_texture_region_dispose(spine_texture_region self) {
	delete (TextureRegion *) self;
}

spine_rtti spine_texture_region_get_rtti(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return (spine_rtti) &_self->getRTTI();
}

float spine_texture_region_get_u(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getU();
}

void spine_texture_region_set_u(spine_texture_region self, float value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setU(value);
}

float spine_texture_region_get_v(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getV();
}

void spine_texture_region_set_v(spine_texture_region self, float value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setV(value);
}

float spine_texture_region_get_u2(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getU2();
}

void spine_texture_region_set_u2(spine_texture_region self, float value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setU2(value);
}

float spine_texture_region_get_v2(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getV2();
}

void spine_texture_region_set_v2(spine_texture_region self, float value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setV2(value);
}

int spine_texture_region_get_region_width(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getRegionWidth();
}

void spine_texture_region_set_region_width(spine_texture_region self, int value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setRegionWidth(value);
}

int spine_texture_region_get_region_height(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getRegionHeight();
}

void spine_texture_region_set_region_height(spine_texture_region self, int value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setRegionHeight(value);
}

/*@null*/ void *spine_texture_region_get_renderer_object(spine_texture_region self) {
	TextureRegion *_self = (TextureRegion *) self;
	return _self->getRendererObject();
}

void spine_texture_region_set_renderer_object(spine_texture_region self, /*@null*/ void *value) {
	TextureRegion *_self = (TextureRegion *) self;
	_self->setRendererObject(value);
}

spine_rtti spine_texture_region_rtti(void) {
	return (spine_rtti) &TextureRegion::rtti;
}
