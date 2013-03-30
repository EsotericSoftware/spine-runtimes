#ifndef SPINE_ATTACHMENTLOADER_H_
#define SPINE_ATTACHMENTLOADER_H_

#include <spine/Attachment.h>

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef struct AttachmentLoader AttachmentLoader;
struct AttachmentLoader {
	const char* error1;
	const char* error2;

	Attachment* (*_newAttachment) (AttachmentLoader* loader, AttachmentType type, const char* name);
	void (*_dispose) (AttachmentLoader* loader);
};

void AttachmentLoader_dispose (AttachmentLoader* loader);

Attachment* AttachmentLoader_newAttachment (AttachmentLoader* loader, AttachmentType type, const char* name);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_ATTACHMENTLOADER_H_ */
