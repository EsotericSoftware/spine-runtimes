#include "skeleton_binary.h"
#include <spine/spine.h>

using namespace spine;

spine_skeleton_binary spine_skeleton_binary_create(spine_atlas atlas) {
	return (spine_skeleton_binary) new (__FILE__, __LINE__) SkeletonBinary(*((Atlas *) atlas));
}

spine_skeleton_binary spine_skeleton_binary_create2(spine_attachment_loader attachmentLoader, bool ownsLoader) {
	return (spine_skeleton_binary) new (__FILE__, __LINE__) SkeletonBinary(*((AttachmentLoader *) attachmentLoader), ownsLoader);
}

void spine_skeleton_binary_dispose(spine_skeleton_binary self) {
	delete (SkeletonBinary *) self;
}

/*@null*/ spine_skeleton_data spine_skeleton_binary_read_skeleton_data(spine_skeleton_binary self, /*@null*/ const unsigned char *binary,
																	   int length) {
	SkeletonBinary *_self = (SkeletonBinary *) self;
	return (spine_skeleton_data) _self->readSkeletonData(binary, length);
}

/*@null*/ spine_skeleton_data spine_skeleton_binary_read_skeleton_data_file(spine_skeleton_binary self, const char *path) {
	SkeletonBinary *_self = (SkeletonBinary *) self;
	return (spine_skeleton_data) _self->readSkeletonDataFile(String(path));
}

void spine_skeleton_binary_set_scale(spine_skeleton_binary self, float scale) {
	SkeletonBinary *_self = (SkeletonBinary *) self;
	_self->setScale(scale);
}

const char *spine_skeleton_binary_get_error(spine_skeleton_binary self) {
	SkeletonBinary *_self = (SkeletonBinary *) self;
	return _self->getError().buffer();
}
