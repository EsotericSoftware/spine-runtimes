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

#include <spine/PathConstraint.h>
#include <spine/Skeleton.h>
#include <spine/extension.h>

spPathConstraint* spPathConstraint_create (spPathConstraintData* data, const spSkeleton* skeleton) {
	int i;
	spPathConstraint *self = NEW(spPathConstraint);
	CONST_CAST(spPathConstraintData*, self->data) = data;
	self->bonesCount = data->bonesCount;
	CONST_CAST(spBone**, self->bones) = MALLOC(spBone*, self->bonesCount);
	for (i = 0; i < self->bonesCount; ++i)
		self->bones[i] = spSkeleton_findBone(skeleton, self->data->bones[i]->name);
	self->target = spSkeleton_findSlot(skeleton, self->data->target->name);
	self->position = data->position;
	self->spacing = data->spacing;
	self->rotateMix = data->rotateMix;
	self->translateMix = data->translateMix;
	self->spacesCount = 0;
	self->spaces = 0;
	self->positionsCount = 0;
	self->positions = 0;
	self->worldCount = 0;
	self->world = 0;
	self->curvesCount = 0;
	self->curves = 0;
	self->lengthsCount = 0;
	self->lengths = 0;
	return 0;
}

void spPathConstraint_dispose (spPathConstraint* self) {
	FREE(self->bones);
	FREE(self->spaces);
	if (self->positions) FREE(self->positions);
	if (self->world) FREE(self->world);
	if (self->curves) FREE(self->curves);
	if (self->lengths) FREE(self->lengths);
	FREE(self);
}

void spPathConstraint_apply (spPathConstraint* self) {
	int i, p, n, length, x, y, dx, dy, s;
	float* spaces, *lengths, *positions;
	float spacing;
	spSkeleton* skeleton;
	float skeletonX, skeletonY, boneX, boneY, offsetRotation;
	int/*bool*/tip;
	float rotateMix = self->rotateMix, translateMix = self->translateMix;
	int translate = translateMix > 0, rotate = rotateMix > 0;
	spPathAttachment* attachment = (spPathAttachment*)self->target->attachment;
	spPathConstraintData* data = self->data;
	spSpacingMode spacingMode = data->spacingMode;
	int lengthSpacing = spacingMode == SP_SPACING_MODE_LENGTH;
	spRotateMode rotateMode = data->rotateMode;
	int tangents = rotateMode == SP_ROTATE_MODE_TANGENT, scale = rotateMode == SP_ROTATE_MODE_CHAIN_SCALE;
	int boneCount = self->bonesCount, spacesCount = tangents ? boneCount : boneCount + 1;
	spBone** bones = self->bones;

	if (!translate && !rotate) return;
	if ((attachment == 0) || (attachment->super.super.type != SP_ATTACHMENT_PATH)) return;

	if (self->spacesCount != spacesCount) {
		if (self->spaces) FREE(self->spaces);
		self->spaces = MALLOC(float, spacesCount);
		self->spacesCount = spacesCount;
	}
	spaces = self->spaces;
	lengths = 0;
	spacing = self->spacing;
	if (scale || lengthSpacing) {
		if (scale) {
			if (self->lengthsCount != boneCount) {
				if (self->lengths) FREE(self->lengths);
				self->lengths = MALLOC(float, boneCount);
				self->lengthsCount = boneCount;
			}
			lengths = self->lengths;
		}
		for (i = 0, n = spacesCount - 1; i < n;) {
			spBone* bone = bones[i];
			length = bone->data->length, x = length * bone->a, y = length * bone->c;
			length = SQRT(x * x + y * y);
			if (scale) lengths[i] = length;
			spaces[++i] = lengthSpacing ? MAX(0, length + spacing) : spacing;
		}
	} else {
		for (i = 1; i < spacesCount; i++) {
			spaces[i] = spacing;
		}
	}

	positions = spPathConstraint_computeWorldPositions(self, attachment, spacesCount, tangents,
											 data->positionMode == SP_POSITION_MODE_PERCENT, spacingMode == SP_SPACING_MODE_PERCENT);
	skeleton = self->target->bone->skeleton;
	skeletonX = skeleton->x, skeletonY = skeleton->y;
	boneX = positions[0], boneY = positions[1], offsetRotation = self->data->offsetRotation;
	tip = rotateMode == SP_ROTATE_MODE_CHAIN_SCALE && offsetRotation == 0;
	for (i = 0, p = 3; i < boneCount; i++, p += 3) {
		spBone* bone = bones[i];
		CONST_CAST(float, bone->worldX) += (boneX - skeletonX - bone->worldX) * translateMix;
		CONST_CAST(float, bone->worldY) += (boneY - skeletonY - bone->worldY) * translateMix;
		x = positions[p], y = positions[p + 1], dx = x - boneX, dy = y - boneY;
		if (scale) {
			length = lengths[i];
			if (length != 0) {
				s = (SQRT(dx * dx + dy * dy) / length - 1) * rotateMix + 1;
				CONST_CAST(float, bone->a) *= s;
				CONST_CAST(float, bone->c) *= s;
			}
		}
		boneX = x;
		boneY = y;
		if (rotate) {
			float a = bone->a, b = bone->b, c = bone->c, d = bone->d, r, cosine, sine;
			if (tangents)
				r = positions[p - 1];
			else if (spaces[i + 1] == 0)
				r = positions[p + 2];
			else
				r = ATAN2(dy, dx);
			r -= ATAN2(c, a) - offsetRotation * DEG_RAD;
			if (tip) {
				cosine = COS(r);
				sine = SIN(r);
				length = bone->data->length;
				boneX += (length * (cosine * a - sine * c) - dx) * rotateMix;
				boneY += (length * (sine * a + cosine * c) - dy) * rotateMix;
			}
			if (r > PI)
				r -= PI2;
			else if (r < -PI)
				r += PI2;
			r *= rotateMix;
			cosine = COS(r);
			sine = SIN(r);
			CONST_CAST(float, bone->a) = cosine * a - sine * c;
			CONST_CAST(float, bone->b) = cosine * b - sine * d;
			CONST_CAST(float, bone->c) = sine * a + cosine * c;
			CONST_CAST(float, bone->d) = sine * b + cosine * d;
		}
	}
}

float* spPathConstraint_computeWorldPositions(spPathConstraint* self, spPathAttachment* path, int spacesCount, int/*bool*/ tangents, int/*bool*/percentPosition, int/**/percentSpacing) {
	return 0;
}
