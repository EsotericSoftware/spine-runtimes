#ifndef SPINE_SLOTDATA_H_
#define SPINE_SLOTDATA_H_

#include <spine/BoneData.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct {
	const char* const name;
	const BoneData* const boneData;
	const char* const attachmentName;
	float r, g, b, a;
} SlotData;

SlotData* SlotData_create (const char* name, BoneData* boneData);
void SlotData_dispose (SlotData* slotData);

/** @param attachmentName May be zero. */
void SlotData_setAttachmentName (SlotData* slotData, const char* attachmentName);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SLOTDATA_H_ */
