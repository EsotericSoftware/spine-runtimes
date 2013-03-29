#ifndef SPINE_ATLASATTACHMENTLOADER_H_
#define SPINE_ATLASATTACHMENTLOADER_H_

#include <spine/AttachmentLoader.h>
#include <spine/Atlas.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct {
	AttachmentLoader super;
	Atlas* atlas;
} AtlasAttachmentLoader;

AtlasAttachmentLoader* AtlasAttachmentLoader_create (Atlas* atlas);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_ATLASATTACHMENTLOADER_H_ */
