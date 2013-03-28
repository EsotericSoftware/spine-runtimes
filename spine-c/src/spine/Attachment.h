#ifndef SPINE_ATTACHMENT_H_
#define SPINE_ATTACHMENT_H_

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

typedef enum {
	ATTACHMENT_REGION, ATTACHMENT_REGION_SEQUENCE
} AttachmentType;

typedef struct Attachment Attachment;
struct Attachment {
	const char* const name;
	void (*_dispose) (Attachment* attachment);
};

typedef Attachment* (*AttachmentLoader) (AttachmentType type, const char* name);

void Attachment_setAttachmentLoader (AttachmentLoader loader);
AttachmentLoader Attachment_getAttachmentLoader ();

void Attachment_init (Attachment* attachment, const char* name);
void Attachment_dispose (Attachment* attachment);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_ATTACHMENT_H_ */
