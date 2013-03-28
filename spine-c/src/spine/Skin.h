#ifndef SPINE_SKIN_H_
#define SPINE_SKIN_H_

#include <spine/Attachment.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct SkinEntry SkinEntry;
struct SkinEntry {
	int slotIndex;
	const char* name;
	Attachment* attachment;
	const SkinEntry* next;
};

typedef struct {
	const char* const name;
	const SkinEntry* const entries;
} Skin;

Skin* Skin_create (const char* name);
void Skin_dispose (Skin* skin);

/** The Skin owns the attachment. */
void Skin_addAttachment (Skin* skin, int slotIndex, const char* name, Attachment* attachment);
/** May return null. */
Attachment* Skin_getAttachment (const Skin* skin, int slotIndex, const char* name);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_SKIN_H_ */
