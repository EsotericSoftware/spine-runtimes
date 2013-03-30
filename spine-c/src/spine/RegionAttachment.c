#include <spine/RegionAttachment.h>
#include <math.h>
#include <spine/util.h>
#include <spine/extension.h>

void _RegionAttachment_init (RegionAttachment* self, const char* name) {
	self->scaleX = 1;
	self->scaleY = 1;
	_Attachment_init(&self->super, name, ATTACHMENT_REGION);
}

void _RegionAttachment_deinit (RegionAttachment* self) {
	_Attachment_deinit(&self->super);
}

void RegionAttachment_updateOffset (RegionAttachment* self) {
	float localX2 = self->width / 2;
	float localY2 = self->height / 2;
	float localX = -localX2;
	float localY = -localY2;
	localX *= self->scaleX;
	localY *= self->scaleY;
	localX2 *= self->scaleX;
	localY2 *= self->scaleY;
	float radians = (float)(self->rotation * 3.1415926535897932385 / 180);
	float cosine = cosf(radians);
	float sine = sinf(radians);
	float localXCos = localX * cosine + self->x;
	float localXSin = localX * sine;
	float localYCos = localY * cosine + self->y;
	float localYSin = localY * sine;
	float localX2Cos = localX2 * cosine + self->x;
	float localX2Sin = localX2 * sine;
	float localY2Cos = localY2 * cosine + self->y;
	float localY2Sin = localY2 * sine;
	self->offset[0] = localXCos - localYSin;
	self->offset[1] = localYCos + localXSin;
	self->offset[2] = localXCos - localY2Sin;
	self->offset[3] = localY2Cos + localXSin;
	self->offset[4] = localX2Cos - localY2Sin;
	self->offset[5] = localY2Cos + localX2Sin;
	self->offset[6] = localX2Cos - localYSin;
	self->offset[7] = localYCos + localX2Sin;
}
