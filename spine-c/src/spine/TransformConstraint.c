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
	self->x = data->x;
	self->y = data->y;
	self->bone = spSkeleton_findBone(skeleton, self->data->bone->name);
	self->target = spSkeleton_findBone(skeleton, self->data->target->name);
	return self;
}

void spTransformConstraint_dispose (spTransformConstraint* self) {
	FREE(self);
}

void spTransformConstraint_apply (spTransformConstraint* self) {
	if (self->translateMix > 0) {
		float tx, ty;
		spBone_localToWorld(self->target, self->x, self->y, &tx, &ty);
		CONST_CAST(float, self->bone->worldX) = self->bone->worldX + (tx - self->bone->worldX) * self->translateMix;
		CONST_CAST(float, self->bone->worldY) = self->bone->worldY + (ty - self->bone->worldY) * self->translateMix;
	}
}
