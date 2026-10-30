#ifndef SPINE_SPINE_SKELETON_BINARY_H
#define SPINE_SPINE_SKELETON_BINARY_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skeleton_binary spine_skeleton_binary_create(spine_atlas atlas);
SPINE_C_API spine_skeleton_binary spine_skeleton_binary_create2(spine_attachment_loader attachmentLoader, bool ownsLoader);

SPINE_C_API void spine_skeleton_binary_dispose(spine_skeleton_binary self);

SPINE_C_API /*@null*/ spine_skeleton_data spine_skeleton_binary_read_skeleton_data(spine_skeleton_binary self, /*@null*/ const unsigned char *binary,
																				   int length);
SPINE_C_API /*@null*/ spine_skeleton_data spine_skeleton_binary_read_skeleton_data_file(spine_skeleton_binary self, const char *path);
SPINE_C_API void spine_skeleton_binary_set_scale(spine_skeleton_binary self, float scale);
SPINE_C_API const char *spine_skeleton_binary_get_error(spine_skeleton_binary self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKELETON_BINARY_H */
