#include "sequence.h"
#include <spine/spine.h>

using namespace spine;

spine_sequence spine_sequence_create(int count, bool pathSuffix) {
	return (spine_sequence) new (__FILE__, __LINE__) Sequence(count, pathSuffix);
}

spine_sequence spine_sequence_create2(spine_sequence other) {
	return (spine_sequence) new (__FILE__, __LINE__) Sequence(*((const Sequence *) other));
}

void spine_sequence_dispose(spine_sequence self) {
	delete (Sequence *) self;
}

void spine_sequence_update_1(spine_sequence self, spine_region_attachment attachment) {
	Sequence *_self = (Sequence *) self;
	_self->update(*((RegionAttachment *) attachment));
}

void spine_sequence_update_2(spine_sequence self, spine_mesh_attachment attachment) {
	Sequence *_self = (Sequence *) self;
	_self->update(*((MeshAttachment *) attachment));
}

spine_array_texture_region spine_sequence_get_regions(spine_sequence self) {
	Sequence *_self = (Sequence *) self;
	return (spine_array_texture_region) &_self->getRegions();
}

int spine_sequence_resolve_index(spine_sequence self, spine_slot_pose pose) {
	Sequence *_self = (Sequence *) self;
	return _self->resolveIndex(*((SlotPose *) pose));
}

/*@null*/ spine_texture_region spine_sequence_get_region(spine_sequence self, int index) {
	Sequence *_self = (Sequence *) self;
	return (spine_texture_region) _self->getRegion(index);
}

spine_array_float spine_sequence_get_u_vs(spine_sequence self, int index) {
	Sequence *_self = (Sequence *) self;
	return (spine_array_float) &_self->getUVs(index);
}

spine_array_float spine_sequence_get_offsets(spine_sequence self, int index) {
	Sequence *_self = (Sequence *) self;
	return (spine_array_float) &_self->getOffsets(index);
}

int spine_sequence_get_start(spine_sequence self) {
	Sequence *_self = (Sequence *) self;
	return _self->getStart();
}

void spine_sequence_set_start(spine_sequence self, int start) {
	Sequence *_self = (Sequence *) self;
	_self->setStart(start);
}

int spine_sequence_get_digits(spine_sequence self) {
	Sequence *_self = (Sequence *) self;
	return _self->getDigits();
}

void spine_sequence_set_digits(spine_sequence self, int digits) {
	Sequence *_self = (Sequence *) self;
	_self->setDigits(digits);
}

int spine_sequence_get_setup_index(spine_sequence self) {
	Sequence *_self = (Sequence *) self;
	return _self->getSetupIndex();
}

void spine_sequence_set_setup_index(spine_sequence self, int setupIndex) {
	Sequence *_self = (Sequence *) self;
	_self->setSetupIndex(setupIndex);
}

bool spine_sequence_has_path_suffix(spine_sequence self) {
	Sequence *_self = (Sequence *) self;
	return _self->hasPathSuffix();
}

const char *spine_sequence_get_path(spine_sequence self, const char *basePath, int index) {
	Sequence *_self = (Sequence *) self;
	return (const char *) &_self->getPath(String(basePath), index);
}

int spine_sequence_get_id(spine_sequence self) {
	Sequence *_self = (Sequence *) self;
	return _self->getId();
}
