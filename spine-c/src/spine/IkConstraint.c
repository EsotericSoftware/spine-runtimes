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

#include <spine/IkConstraint.h>
#include <spine/Skeleton.h>
#include <spine/extension.h>

spIkConstraint* spIkConstraint_create (spIkConstraintData* data, const spSkeleton* skeleton) {
	int i;

	spIkConstraint* self = NEW(spIkConstraint);
	CONST_CAST(spIkConstraintData*, self->data) = data;
	self->bendDirection = data->bendDirection;
	self->mix = data->mix;

	self->bonesCount = self->data->bonesCount;
	self->bones = MALLOC(spBone*, self->bonesCount);
	for (i = 0; i < self->bonesCount; ++i)
		self->bones[i] = spSkeleton_findBone(skeleton, self->data->bones[i]->name);
	self->target = spSkeleton_findBone(skeleton, self->data->target->name);

	return self;
}

void spIkConstraint_dispose (spIkConstraint* self) {
	FREE(self->bones);
	FREE(self);
}

void spIkConstraint_apply (spIkConstraint* self) {
	switch (self->bonesCount) {
	case 1:
		spIkConstraint_apply1(self->bones[0], self->target->worldX, self->target->worldY, self->mix);
		break;
	case 2:
		spIkConstraint_apply2(self->bones[0], self->bones[1], self->target->worldX, self->target->worldY, self->bendDirection,
				self->mix);
		break;
	}
}

void spIkConstraint_apply1 (spBone* bone, float targetX, float targetY, float alpha) {
	float parentRotation = !bone->parent ? 0 : spBone_getWorldRotationX(bone->parent);
	float rotation = bone->rotation;
	float rotationIK = ATAN2(targetY - bone->worldY, targetX - bone->worldX) * RAD_DEG - parentRotation;
	if (bone->worldSignX != bone->worldSignY) rotationIK = 360 - rotationIK;
	if (rotationIK > 180) rotationIK -= 360;
	else if (rotationIK < -180) rotationIK += 360;
	spBone_updateWorldTransformWith(bone, bone->x, bone->y, rotation + (rotationIK - rotation) * alpha, bone->scaleX, bone->scaleY);
}

void spIkConstraint_apply2 (spBone* parent, spBone* child, float targetX, float targetY, int bendDir, float alpha) {
	float px = parent->x, py = parent->y, psx = parent->scaleX, psy = parent->scaleY, csx = child->scaleX, cy = child->y;
	int offset1, offset2, sign2;
	spBone* pp = parent->parent;
	float tx, ty, dx, dy, l1, l2, a1, a2, psd, r;
	if (alpha == 0) return;
	if (psx < 0) {
		psx = -psx;
		offset1 = 180;
		sign2 = -1;
	} else {
		offset1 = 0;
		sign2 = 1;
	}
	if (psy < 0) {
		psy = -psy;
		sign2 = -sign2;
	}
	if (csx < 0) {
		csx = -csx;
		offset2 = 180;
	} else
		offset2 = 0;
	if (!pp) {
		tx = targetX - px;
		ty = targetY - py;
		dx = child->worldX - px;
		dy = child->worldY - py;
	} else {
		float a = pp->a, b = pp->b, c = pp->c, d = pp->d, invDet = 1 / (a * d - b * c);
		float wx = pp->worldX, wy = pp->worldY, x = targetX - wx, y = targetY - wy;
		tx = (x * d - y * b) * invDet - px;
		ty = (y * a - x * c) * invDet - py;
		x = child->worldX - wx;
		y = child->worldY - wy;
		dx = (x * d - y * b) * invDet - px;
		dy = (y * a - x * c) * invDet - py;
	}
	l1 = SQRT(dx * dx + dy * dy);
	l2 = child->data->length * csx;
	psd = psx - psy;
	if (psd < 0 ? -psd : psd <= 0.0001f) {
		float cos, a, o;
		l2 *= psx;
		cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2);
		if (cos < -1) cos = -1;
		else if (cos > 1) cos = 1;
		a2 = ACOS(cos) * bendDir;
		a = l1 + l2 * cos;
		o = l2 * SIN(a2);
		a1 = ATAN2(ty * a - tx * o, tx * a + ty * o);
	} else {
		float a = psx * l2, b = psy * l2, ta = ATAN2(ty, tx);
		float aa = a * a, bb = b * b, ll = l1 * l1, dd = tx * tx + ty * ty;
		float c0 = bb * ll + aa * dd - aa * bb, c1 = -2 * bb * l1, c2 = bb - aa;
		float d = c1 * c1 - 4 * c2 * c0;
		float minAngle = 0, minDist = 999999999.0f, minX = 0, minY = 0;
		float maxAngle = 0, maxDist = 0, maxX = 0, maxY = 0;
		float x = l1 + a, dist = x * x, angle, y;
		cy = 0;
		if (d >= 0) {
			float q = SQRT(d), r0, r1, ar0, ar1;;
			if (c1 < 0) q = -q;
			q = -(c1 + q) / 2;
			r0 = q / c2;
			r1 = c0 / q;
			ar0 = r0 < 0 ? -r0 : r0;
			ar1 = r1 < 0 ? -r1 : r1;
			r = ar0 < ar1 ? r0 : r1;
			if (r * r <= dd) {
				float y1 = SQRT(dd - r * r) * bendDir;
				a1 = ta - ATAN2(y1, r);
				a2 = ATAN2(y1 / psy, (r - l1) / psx);
				goto outer;
			}
		}
		if (dist > maxDist) {
			maxAngle = 0;
			maxDist = dist;
			maxX = x;
		}
		x = l1 - a;
		dist = x * x;
		if (dist < minDist) {
			minAngle = PI;
			minDist = dist;
			minX = x;
		}
		angle = ACOS(-a * l1 / (aa - bb));
		x = a * COS(angle) + l1;
		y = b * SIN(angle);
		dist = x * x + y * y;
		if (dist < minDist) {
			minAngle = angle;
			minDist = dist;
			minX = x;
			minY = y;
		}
		if (dist > maxDist) {
			maxAngle = angle;
			maxDist = dist;
			maxX = x;
			maxY = y;
		}
		if (dd <= (minDist + maxDist) / 2) {
			a1 = ta - ATAN2(minY * bendDir, minX);
			a2 = minAngle * bendDir;
		} else {
			a1 = ta - ATAN2(maxY * bendDir, maxX);
			a2 = maxAngle * bendDir;
		}
	}
	outer: {
		float offset = ATAN2(cy, child->x) * sign2;
		a1 = (a1 - offset) * RAD_DEG + offset1;
		a2 = (a2 + offset) * RAD_DEG * sign2 + offset2;
		if (a1 > 180) a1 -= 360;
		else if (a1 < -180) a1 += 360;
		if (a2 > 180) a2 -= 360;
		else if (a2 < -180) a2 += 360;
		r = parent->rotation;
		spBone_updateWorldTransformWith(parent, parent->x, parent->y, r + (a1 - r) * alpha, parent->scaleX, parent->scaleY);
		r = child->rotation;
		spBone_updateWorldTransformWith(child, child->x, cy, r + (a2 - r) * alpha, child->scaleX, child->scaleY);
	}
}
