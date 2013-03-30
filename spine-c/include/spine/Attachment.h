#ifndef SPINE_ATTACHMENT_H_
#define SPINE_ATTACHMENT_H_

#ifdef __cplusplus
namespace spine {
extern "C" {
#endif

struct Slot;

typedef enum {
	ATTACHMENT_REGION, ATTACHMENT_REGION_SEQUENCE
} AttachmentType;

typedef struct Attachment Attachment;
struct Attachment {
	const char* const name;
	int type;

	void (*_draw) (Attachment* attachment, struct Slot* slot);
	void (*_dispose) (Attachment* attachment);
};

void Attachment_dispose (Attachment* attachment);

void Attachment_draw (Attachment* attachment, struct Slot* slot);

#ifdef __cplusplus
}
}
#endif

#endif /* SPINE_ATTACHMENT_H_ */
