#include <spine/RegionAttachment.h>
#include <math.h>
#include <spine/util.h>
#include <spine/extension.h>

void _RegionAttachment_init (RegionAttachment* this, const char* name) {
	this->scaleX = 1;
	this->scaleY = 1;
	_Attachment_init(&this->super, name, ATTACHMENT_REGION);
}

void _RegionAttachment_deinit (RegionAttachment* this) {
	_Attachment_deinit(&this->super);
}

void RegionAttachment_updateOffset (RegionAttachment* this) {
	float localX2 = this->width / 2;
	float localY2 = this->height / 2;
	float localX = -localX2;
	float localY = -localY2;
	localX *= this->scaleX;
	localY *= this->scaleY;
	localX2 *= this->scaleX;
	localY2 *= this->scaleY;
	float radians = (float)(this->rotation * 3.1415926535897932385 / 180);
	float cosine = cos(radians);
	float sine = sin(radians);
	float localXCos = localX * cosine + this->x;
	float localXSin = localX * sine;
	float localYCos = localY * cosine + this->y;
	float localYSin = localY * sine;
	float localX2Cos = localX2 * cosine + this->x;
	float localX2Sin = localX2 * sine;
	float localY2Cos = localY2 * cosine + this->y;
	float localY2Sin = localY2 * sine;
	this->offset[0] = localXCos - localYSin;
	this->offset[1] = localYCos + localXSin;
	this->offset[2] = localXCos - localY2Sin;
	this->offset[3] = localY2Cos + localXSin;
	this->offset[4] = localX2Cos - localY2Sin;
	this->offset[5] = localY2Cos + localX2Sin;
	this->offset[6] = localX2Cos - localYSin;
	this->offset[7] = localYCos + localX2Sin;
}
