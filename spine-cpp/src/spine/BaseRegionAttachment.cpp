#include <math.h>
#include <spine/BaseRegionAttachment.h>

namespace spine {

BaseRegionAttachment::BaseRegionAttachment () :
				x(0),
				y(0),
				scaleX(1),
				scaleY(1),
				rotation(0),
				width(0),
				height(0) {
}

void BaseRegionAttachment::updateOffset () {
	float localX2 = width / 2;
	float localY2 = height / 2;
	float localX = -localX2;
	float localY = -localY2;
	localX *= scaleX;
	localY *= scaleY;
	localX2 *= scaleX;
	localY2 *= scaleY;
	float radians = rotation * M_PI / 180;
	float cos = cosf(radians);
	float sin = sinf(radians);
	float localXCos = localX * cos + x;
	float localXSin = localX * sin;
	float localYCos = localY * cos + y;
	float localYSin = localY * sin;
	float localX2Cos = localX2 * cos + x;
	float localX2Sin = localX2 * sin;
	float localY2Cos = localY2 * cos + y;
	float localY2Sin = localY2 * sin;
	offset[0] = localXCos - localYSin;
	offset[1] = localYCos + localXSin;
	offset[2] = localXCos - localY2Sin;
	offset[3] = localY2Cos + localXSin;
	offset[4] = localX2Cos - localY2Sin;
	offset[5] = localY2Cos + localX2Sin;
	offset[6] = localX2Cos - localYSin;
	offset[7] = localYCos + localX2Sin;
}

} /* namespace spine */
