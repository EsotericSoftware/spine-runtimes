#include "attachment_loader.h"
#include <spine/spine.h>

using namespace spine;

void spine_attachment_loader_dispose(spine_attachment_loader self) {
	delete (AttachmentLoader *) self;
}

/*@null*/ spine_region_attachment spine_attachment_loader_new_region_attachment(spine_attachment_loader self, spine_skin skin,
																				const char *placeholder, const char *name, const char *path,
																				/*@null*/ spine_sequence sequence) {
	AttachmentLoader *_self = (AttachmentLoader *) self;
	return (spine_region_attachment) _self->newRegionAttachment(*((Skin *) skin), String(placeholder), String(name), String(path),
																(Sequence *) sequence);
}

/*@null*/ spine_mesh_attachment spine_attachment_loader_new_mesh_attachment(spine_attachment_loader self, spine_skin skin, const char *placeholder,
																			const char *name, const char *path, /*@null*/ spine_sequence sequence) {
	AttachmentLoader *_self = (AttachmentLoader *) self;
	return (spine_mesh_attachment) _self->newMeshAttachment(*((Skin *) skin), String(placeholder), String(name), String(path), (Sequence *) sequence);
}

/*@null*/ spine_bounding_box_attachment spine_attachment_loader_new_bounding_box_attachment(spine_attachment_loader self, spine_skin skin,
																							const char *placeholder, const char *name) {
	AttachmentLoader *_self = (AttachmentLoader *) self;
	return (spine_bounding_box_attachment) _self->newBoundingBoxAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_path_attachment spine_attachment_loader_new_path_attachment(spine_attachment_loader self, spine_skin skin, const char *placeholder,
																			const char *name) {
	AttachmentLoader *_self = (AttachmentLoader *) self;
	return (spine_path_attachment) _self->newPathAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_point_attachment spine_attachment_loader_new_point_attachment(spine_attachment_loader self, spine_skin skin, const char *placeholder,
																			  const char *name) {
	AttachmentLoader *_self = (AttachmentLoader *) self;
	return (spine_point_attachment) _self->newPointAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_clipping_attachment spine_attachment_loader_new_clipping_attachment(spine_attachment_loader self, spine_skin skin,
																					const char *placeholder, const char *name) {
	AttachmentLoader *_self = (AttachmentLoader *) self;
	return (spine_clipping_attachment) _self->newClippingAttachment(*((Skin *) skin), String(placeholder), String(name));
}
