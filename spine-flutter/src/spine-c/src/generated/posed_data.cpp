#include "posed_data.h"
#include <spine/spine.h>

using namespace spine;

spine_posed_data spine_posed_data_create(const char *name) {
	return (spine_posed_data) new (__FILE__, __LINE__) PosedData(String(name));
}

void spine_posed_data_dispose(spine_posed_data self) {
	delete (PosedData *) self;
}

const char *spine_posed_data_get_name(spine_posed_data self) {
	PosedData *_self = (PosedData *) self;
	return _self->getName().buffer();
}

bool spine_posed_data_get_skin_required(spine_posed_data self) {
	PosedData *_self = (PosedData *) self;
	return _self->getSkinRequired();
}

void spine_posed_data_set_skin_required(spine_posed_data self, bool skinRequired) {
	PosedData *_self = (PosedData *) self;
	_self->setSkinRequired(skinRequired);
}
