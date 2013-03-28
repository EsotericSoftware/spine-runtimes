#ifndef SPINE_SFMLREGIONATTACHMENT_H_
#define SPINE_SFMLREGIONATTACHMENT_H_

#include <spine/RegionAttachment.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct {
	RegionAttachment super;
	int meow;
} SfmlRegionAttachment;

SfmlRegionAttachment* SfmlRegionAttachment_create (const char* name);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SFMLREGIONATTACHMENT_H_ */
