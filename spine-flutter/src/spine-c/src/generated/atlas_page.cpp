#include "atlas_page.h"
#include <spine/spine.h>

using namespace spine;

spine_atlas_page spine_atlas_page_create(const char *inName) {
	return (spine_atlas_page) new (__FILE__, __LINE__) AtlasPage(String(inName));
}

void spine_atlas_page_dispose(spine_atlas_page self) {
	delete (AtlasPage *) self;
}

const char *spine_atlas_page_get_name(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->name.buffer();
}

void spine_atlas_page_set_name(spine_atlas_page self, const char *value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->name = String(value);
}

const char *spine_atlas_page_get_texture_path(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->texturePath.buffer();
}

void spine_atlas_page_set_texture_path(spine_atlas_page self, const char *value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->texturePath = String(value);
}

spine_format spine_atlas_page_get_format(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return (spine_format) _self->format;
}

void spine_atlas_page_set_format(spine_atlas_page self, spine_format value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->format = (Format) value;
}

spine_texture_filter spine_atlas_page_get_min_filter(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return (spine_texture_filter) _self->minFilter;
}

void spine_atlas_page_set_min_filter(spine_atlas_page self, spine_texture_filter value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->minFilter = (TextureFilter) value;
}

spine_texture_filter spine_atlas_page_get_mag_filter(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return (spine_texture_filter) _self->magFilter;
}

void spine_atlas_page_set_mag_filter(spine_atlas_page self, spine_texture_filter value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->magFilter = (TextureFilter) value;
}

spine_texture_wrap spine_atlas_page_get_u_wrap(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return (spine_texture_wrap) _self->uWrap;
}

void spine_atlas_page_set_u_wrap(spine_atlas_page self, spine_texture_wrap value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->uWrap = (TextureWrap) value;
}

spine_texture_wrap spine_atlas_page_get_v_wrap(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return (spine_texture_wrap) _self->vWrap;
}

void spine_atlas_page_set_v_wrap(spine_atlas_page self, spine_texture_wrap value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->vWrap = (TextureWrap) value;
}

int spine_atlas_page_get_width(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->width;
}

void spine_atlas_page_set_width(spine_atlas_page self, int value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->width = value;
}

int spine_atlas_page_get_height(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->height;
}

void spine_atlas_page_set_height(spine_atlas_page self, int value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->height = value;
}

bool spine_atlas_page_get_pma(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->pma;
}

void spine_atlas_page_set_pma(spine_atlas_page self, bool value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->pma = value;
}

int spine_atlas_page_get_index(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->index;
}

void spine_atlas_page_set_index(spine_atlas_page self, int value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->index = value;
}

/*@null*/ void *spine_atlas_page_get_texture(spine_atlas_page self) {
	AtlasPage *_self = (AtlasPage *) self;
	return _self->texture;
}

void spine_atlas_page_set_texture(spine_atlas_page self, /*@null*/ void *value) {
	AtlasPage *_self = (AtlasPage *) self;
	_self->texture = (void *) value;
}
