#include "atlas_attachment_loader.h"
#include <spine/spine.h>

using namespace spine;

spine_atlas_attachment_loader spine_atlas_attachment_loader_create(spine_atlas atlas) {
	return (spine_atlas_attachment_loader) new (__FILE__, __LINE__) AtlasAttachmentLoader(*((Atlas *) atlas));
}

void spine_atlas_attachment_loader_dispose(spine_atlas_attachment_loader self) {
	delete (AtlasAttachmentLoader *) self;
}

/*@null*/ spine_region_attachment spine_atlas_attachment_loader_new_region_attachment(spine_atlas_attachment_loader self, spine_skin skin,
																					  const char *placeholder, const char *name, const char *path,
																					  /*@null*/ spine_sequence sequence) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_region_attachment) _self->newRegionAttachment(*((Skin *) skin), String(placeholder), String(name), String(path),
																(Sequence *) sequence);
}

/*@null*/ spine_mesh_attachment spine_atlas_attachment_loader_new_mesh_attachment(spine_atlas_attachment_loader self, spine_skin skin,
																				  const char *placeholder, const char *name, const char *path,
																				  /*@null*/ spine_sequence sequence) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_mesh_attachment) _self->newMeshAttachment(*((Skin *) skin), String(placeholder), String(name), String(path), (Sequence *) sequence);
}

/*@null*/ spine_bounding_box_attachment spine_atlas_attachment_loader_new_bounding_box_attachment(spine_atlas_attachment_loader self, spine_skin skin,
																								  const char *placeholder, const char *name) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_bounding_box_attachment) _self->newBoundingBoxAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_path_attachment spine_atlas_attachment_loader_new_path_attachment(spine_atlas_attachment_loader self, spine_skin skin,
																				  const char *placeholder, const char *name) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_path_attachment) _self->newPathAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_point_attachment spine_atlas_attachment_loader_new_point_attachment(spine_atlas_attachment_loader self, spine_skin skin,
																					const char *placeholder, const char *name) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_point_attachment) _self->newPointAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_clipping_attachment spine_atlas_attachment_loader_new_clipping_attachment(spine_atlas_attachment_loader self, spine_skin skin,
																						  const char *placeholder, const char *name) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_clipping_attachment) _self->newClippingAttachment(*((Skin *) skin), String(placeholder), String(name));
}

/*@null*/ spine_atlas_region spine_atlas_attachment_loader_find_region(spine_atlas_attachment_loader self, const char *name) {
	AtlasAttachmentLoader *_self = (AtlasAttachmentLoader *) self;
	return (spine_atlas_region) _self->findRegion(String(name));
}
