#ifndef SPINE_SPINE_SKELETON_JSON_H
#define SPINE_SPINE_SKELETON_JSON_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_skeleton_json spine_skeleton_json_create(spine_atlas atlas);
SPINE_C_API spine_skeleton_json spine_skeleton_json_create2(spine_attachment_loader attachmentLoader, bool ownsLoader);

SPINE_C_API void spine_skeleton_json_dispose(spine_skeleton_json self);

SPINE_C_API /*@null*/ spine_skeleton_data spine_skeleton_json_read_skeleton_data_file(spine_skeleton_json self, const char *path);
SPINE_C_API /*@null*/ spine_skeleton_data spine_skeleton_json_read_skeleton_data(spine_skeleton_json self, /*@null*/ const char *json);
SPINE_C_API void spine_skeleton_json_set_scale(spine_skeleton_json self, float scale);
SPINE_C_API const char *spine_skeleton_json_get_error(spine_skeleton_json self);

#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_SKELETON_JSON_H */
