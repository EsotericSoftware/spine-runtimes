/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/TransformConstraint.h>
#include <spine/Skeleton.h>
#include <spine/extension.h>

spTransformConstraint* spTransformConstraint_create (spTransformConstraintData* data, const spSkeleton* skeleton) {
	spTransformConstraint* self = NEW(spTransformConstraint);
	CONST_CAST(spTransformConstraintData*, self->data) = data;
	self->translateMix = data->translateMix;
	self->rotateMix = data->rotateMix;
	self->scaleMix = data->scaleMix;
	self->shearMix = data->shearMix;
	self->offsetX = data->offsetX;
	self->offsetY = data->offsetY;
	self->bone = spSkeleton_findBone(skeleton, self->data->bone->name);
	self->target = spSkeleton_findBone(skeleton, self->data->target->name);
	return self;
}

void spTransformConstraint_dispose (spTransformConstraint* self) {
	FREE(self);
}

void spTransformConstraint_apply (spTransformConstraint* self) {
	spBone* bone = self->bone;
	spBone* target = self->target;

	if (self->rotateMix > 0) {
		float cosine, sine;
		float a = bone->a, b = bone->b, c = bone->c, d = bone->d;
		float r = atan2(target->c, target->a) - atan2(c, a) + self->offsetRotation * DEG_RAD;
		if (r > PI)
			r -= PI2;
		else if (r < -PI) r += PI2;
		r *= self->rotateMix;
		cosine = COS(r); sine = SIN(r);
		CONST_CAST(float, bone->a) = cosine * a - sine * c;
		CONST_CAST(float, bone->b) = cosine * b - sine * d;
		CONST_CAST(float, bone->c) = sine * a + cosine * c;
		CONST_CAST(float, bone->d) = sine * b + cosine * d;
	}

	if (self->scaleMix > 0) {
		float bs = (float)SQRT(bone->a * bone->a + bone->c * bone->c);
		float ts = (float)SQRT(target->a * target->a + target->c * target->c);
		float s = bs > 0.00001f ? (bs + (ts - bs + self->offsetScaleX) * self->scaleMix) / bs : 0;
		CONST_CAST(float, bone->a) *= s;
		CONST_CAST(float, bone->c) *= s;
		bs = (float)SQRT(bone->b * bone->b + bone->d * bone->d);
		ts = (float)SQRT(target->b * target->b + target->d * target->d);
		s = bs > 0.00001f ? (bs + (ts - bs + self->offsetScaleY) * self->scaleMix) / bs : 0;
		CONST_CAST(float, bone->b) *= s;
		CONST_CAST(float, bone->d) *= s;
	}

	if (self->shearMix > 0) {
		float b = bone->b, d = bone->d;
		float by = atan2(d, b);
		float r = atan2(target->d, target->b) - atan2(target->c, target->a) - (by - atan2(bone->c, bone->a));
		float s;
		if (r > PI)
			r -= PI2;
		else if (r < -PI) r += PI2;
		r = by + (r + self->offsetShearY * DEG_RAD) * self->shearMix;
		s = (float)SQRT(b * b + d * d);
		CONST_CAST(float, bone->b) = COS(r) * s;
		CONST_CAST(float, bone->d) = SIN(r) * s;
	}

	if (self->translateMix > 0) {
		float tx, ty;
		spBone_localToWorld(self->target, self->offsetX, self->offsetY, &tx, &ty);
		CONST_CAST(float, self->bone->worldX) += (tx - self->bone->worldX) * self->translateMix;
		CONST_CAST(float, self->bone->worldY) += (ty - self->bone->worldY) * self->translateMix;
	}
}
