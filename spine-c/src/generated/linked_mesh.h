#ifndef SPINE_SPINE_LINKED_MESH_H
#define SPINE_SPINE_LINKED_MESH_H

#include "../base.h"
#include "types.h"
#include "arrays.h"

#ifdef __cplusplus
extern "C" {
#endif

SPINE_C_API spine_linked_mesh spine_linked_mesh_create(spine_mesh_attachment mesh, const int skinIndex, size_t slotIndex, size_t sourceIndex,
													   const char *source, bool inheritTimelines);
SPINE_C_API spine_linked_mesh spine_linked_mesh_create2(spine_mesh_attachment mesh, const char *skin, size_t slotIndex, size_t sourceIndex,
														const char *source, bool inheritTimelines);

SPINE_C_API void spine_linked_mesh_dispose(spine_linked_mesh self);


#ifdef __cplusplus
}
#endif

#endif /* SPINE_SPINE_LINKED_MESH_H */
