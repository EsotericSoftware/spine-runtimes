#ifndef SPINE_REGIONATTACHMENT_H_
#define SPINE_REGIONATTACHMENT_H_

#include <spine/Attachment.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct RegionAttachment RegionAttachment;
struct RegionAttachment {
	Attachment super;
	float x, y, scaleX, scaleY, rotation, width, height;
	float offset[8];
};

void RegionAttachment_updateOffset (RegionAttachment* attachment);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_REGIONATTACHMENT_H_ */
