#include <spine/RegionAttachment.h>
#include <math.h>
#include <spine/util.h>

void RegionAttachment_init (RegionAttachment* this, const char* name) {
	Attachment_init(&this->super, name);
	this->scaleX = 1;
	this->scaleY = 1;
}

void RegionAttachment_dispose (RegionAttachment* this) {
	Attachment_dispose(&this->super);
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
	float cos = cosf(radians);
	float sin = sinf(radians);
	float localXCos = localX * cos + this->x;
	float localXSin = localX * sin;
	float localYCos = localY * cos + this->y;
	float localYSin = localY * sin;
	float localX2Cos = localX2 * cos + this->x;
	float localX2Sin = localX2 * sin;
	float localY2Cos = localY2 * cos + this->y;
	float localY2Sin = localY2 * sin;
	this->offset[0] = localXCos - localYSin;
	this->offset[1] = localYCos + localXSin;
	this->offset[2] = localXCos - localY2Sin;
	this->offset[3] = localY2Cos + localXSin;
	this->offset[4] = localX2Cos - localY2Sin;
	this->offset[5] = localY2Cos + localX2Sin;
	this->offset[6] = localX2Cos - localYSin;
	this->offset[7] = localYCos + localX2Sin;
}
