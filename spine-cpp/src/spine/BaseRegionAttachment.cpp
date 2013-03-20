/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <math.h>
#include <spine/BaseRegionAttachment.h>

#ifndef M_PI
#define M_PI 3.1415926535897932385
#endif

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
	float radians = (float)(rotation * M_PI / 180);
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
